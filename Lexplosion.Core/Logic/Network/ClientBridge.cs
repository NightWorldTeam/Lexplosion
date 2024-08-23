using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class ClientBridge : NetworkClient
    {
        protected Socket ServerSimulator;

        protected Dictionary<Socket, string> AvailableServers = new Dictionary<Socket, string>();
        private readonly Semaphore _availableServersBlock = new Semaphore(1, 1);

        protected Semaphore AcceptingBlock = new Semaphore(1, 1); //блокировка на время работы метода AcceptHandler
        protected AutoResetEvent SendingWait = new AutoResetEvent(false);
        protected AutoResetEvent ReadingWait = new AutoResetEvent(false);

        private bool IsConnected = false; // когда будет подключен майкнрафт клиент эта переменная будет true
        private readonly string UUID = "";
        private readonly string _sessionToken = "";

        const string CLIENT_TYPE = "game-client"; // эта строка нужна при подключении к управляющему серверу

        private readonly object _closeLock = new object();

        public ClientBridge(string uuid, string sessionToken, ControlServerData server) : base(CLIENT_TYPE, server)
        {
            UUID = uuid;
            _sessionToken = sessionToken;
        }

        public Dictionary<string, int> SetServers(List<string> servers)
        {
            Dictionary<string, int> ports = new Dictionary<string, int>();

            //убираем сервера, которых нет в списке

            _availableServersBlock.WaitOne();
            Socket[] values = new Socket[AvailableServers.Count];
            AvailableServers.Keys.CopyTo(values, 0);

            foreach (Socket serverSocket in values)
            {
                string uuid = AvailableServers[serverSocket];
                if (!servers.Contains(uuid))
                {
                    AvailableServers.Remove(serverSocket);
                    serverSocket.Close();
                }
                else
                {
                    ports[uuid] = ((IPEndPoint)serverSocket.LocalEndPoint).Port; //добавояем порт сокета в список
                }
            }

            //добавляем новые сервера
            foreach (string server_uuid in servers)
            {
                if (!ports.ContainsKey(server_uuid))
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Bind(new IPEndPoint(IPAddress.Any, 0));
                    sock.Listen(1);

                    AvailableServers[sock] = server_uuid;

                    ports[server_uuid] = ((IPEndPoint)sock.LocalEndPoint).Port; //добавояем порт сокета в список
                    sock.BeginAccept(null, 0, AcceptHandler, sock); // запусакаем асинхронный асепт
                }
            }

            _availableServersBlock.Release();

            return ports;
        }

        // этот метод срабатывает при подключении клиента
        private void AcceptHandler(IAsyncResult data)
        {
            Runtime.DebugConsoleWrite("AcceptHandler1");
            AcceptingBlock.WaitOne();

            Socket listener = (Socket)data.AsyncState;

            // проверяем доступность основного сокета
            bool socketIsClose;
            if (ServerSimulator != null)
            {
                try
                {
                    socketIsClose = ServerSimulator.Poll(50, SelectMode.SelectRead) && ServerSimulator.Available == 0;
                }
                catch
                {
                    socketIsClose = true;
                }
            }
            else
            {
                socketIsClose = false;
            }

            if (socketIsClose)
            {
                //основной сокет не доступен. закрываем основное соединение чтобы принять это
                Runtime.DebugConsoleWrite("ServerSimulator closed");
                Close(null);
                Runtime.DebugConsoleWrite("End close");
            }

            //проверяем доступность новго сокета
            bool listenerIsClosed;
            try
            {
                listenerIsClosed = listener.Poll(50, SelectMode.SelectRead) && listener.Available == 0;
            }
            catch
            {
                listenerIsClosed = true;
            }

            // если майкнрафт клиент уже подключен то отвергаем это подключение и выходим нахер, ибо это какое-то левое подключение
            if (IsConnected || listenerIsClosed || !AvailableServers.ContainsKey(listener))
            {
                if (!listenerIsClosed)
                {
                    Runtime.DebugConsoleWrite("Set new BeginAccept");
                    Socket sock = listener.EndAccept(data);
                    sock.Close();
                    listener.BeginAccept(null, 0, AcceptHandler, listener); // возвращаем асинхронный асепт
                }

                AcceptingBlock.Release();
                Runtime.DebugConsoleWrite("AcceptHandler1.1");

                return;
            }

            // TODO: тут проверить тот ли клиент подключился
            Runtime.DebugConsoleWrite("wait _availableServersBlock");
            _availableServersBlock.WaitOne();
            string serverUUID = AvailableServers[listener];
            Runtime.DebugConsoleWrite("AcceptHandler2");
            if (base.Initialization(UUID, _sessionToken, serverUUID))
            {
                Runtime.DebugConsoleWrite("AcceptHandler3");
                Socket serverSimulator_ = listener.EndAccept(data);
                ServerSimulator = serverSimulator_;
                IsConnected = true;

                ReadingWait.Set();
                SendingWait.Set();

                //закрываем другие сокеты
                Socket[] values = new Socket[AvailableServers.Count];
                AvailableServers.Keys.CopyTo(values, 0);
                foreach (Socket sock in values)
                {
                    if (AvailableServers[sock] != serverUUID)
                    {
                        Runtime.DebugConsoleWrite("Remove socket ");
                        AvailableServers.Remove(sock);
                        sock.Close();
                    }
                }

                _availableServersBlock.Release();
                AcceptingBlock.Release();

                Runtime.DebugConsoleWrite("AcceptHandler4");
            }
            else
            {
                _availableServersBlock.Release();
                AcceptingBlock.Release();
                Socket sock = listener.EndAccept(data);
                sock.Close();
                Runtime.DebugConsoleWrite("AcceptHandler1.2");
            }

            listener.BeginAccept(null, 0, new AsyncCallback(AcceptHandler), listener); // возвращаем асинхронный асепт
        }

        protected override void Close(IPEndPoint point)
        {
            lock (_closeLock)
            {
                if (IsConnected)
                {
                    Runtime.DebugConsoleWrite("Close. StackTrace: " + new System.Diagnostics.StackTrace());
                    IsConnected = false;
                    Bridge.Close();
                    ServerSimulator.Close(); //закрываем соединение с клиентом   
                    Runtime.DebugConsoleWrite("End Close");
                }
            }
        }

        protected override void Sending() //отправляет данные с майнкрафт клиента в сеть
        {
            SendingWait.WaitOne();
            Runtime.DebugConsoleWrite("sending begin");

            try
            {
                while (Bridge.IsConnected)
                {
                    byte[] buffer = new byte[1200];
                    int bytes = ServerSimulator.Receive(buffer);

                    if (bytes == 0)
                    {
                        Runtime.DebugConsoleWrite("CLOSE. BYTES IS 0");
                        Close(null);
                        readingThread.Abort();
                        break;
                    }

                    byte[] buffer_ = new byte[bytes]; // TODO: что-то придумать с копированием
                    for (int i = 0; i < bytes; i++)
                    {
                        buffer_[i] = buffer[i];
                    }

                    Bridge.Send(buffer_);
                }
            }
            catch (Exception e)
            {
                Runtime.DebugConsoleWrite("Sending " + e);
                Close(null);
                readingThread.Abort();
            }

            Runtime.DebugConsoleWrite("Sending end " + Bridge.IsConnected);
        }

        protected override void Reading() //получаем данные из сети и отправляем на майкрафт клиент
        {
            ReadingWait.WaitOne();
            Runtime.DebugConsoleWrite("reading begin");

            bool isWorking = Bridge.Receive(out byte[] buffer);

            try
            {
                while (isWorking && Bridge.IsConnected && buffer.Length > 0)
                {
                    ServerSimulator.Send(buffer, buffer.Length, SocketFlags.None);
                    isWorking = Bridge.Receive(out buffer);
                }

            }
            catch (Exception ex)
            {
                Runtime.DebugConsoleWrite("Reading exception " + ex);
            }

            Runtime.DebugConsoleWrite("Reading " + Bridge.IsConnected + " " + isWorking + ", buffer.Length " + buffer?.Length);

            Close(null);
            sendingThread.Abort();
        }

    }
}
