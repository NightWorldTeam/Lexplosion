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
        private readonly Semaphore AvailableServersBlock = new Semaphore(1, 1);

        protected Semaphore AcceptingBlock = new Semaphore(1, 1); //блокировка на время работы метода AcceptHandler
        protected AutoResetEvent SendingWait = new AutoResetEvent(false);
        protected AutoResetEvent ReadingWait = new AutoResetEvent(false);

        private bool IsConnected = false; // когда будет подключен майкнрафт клиент эта переменная будет true
        private readonly string UUID = "";
        private readonly string _sessionToken = "";

        const string clientType = "game-client"; // эта строка нужна при подключении к управляющему серверу

        private readonly object _closeLock = new object();

        public ClientBridge(string uuid, string sessionToken, string server) : base(clientType, server)
        {
            UUID = uuid;
            _sessionToken = sessionToken;
        }

        public Dictionary<string, int> SetServers(List<string> servers)
        {
            Dictionary<string, int> ports = new Dictionary<string, int>();

            //убираем сервера, которых нет в списке

            AvailableServersBlock.WaitOne();
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
                    sock.BeginAccept(null, 0, new AsyncCallback(AcceptHandler), sock); // запусакаем асинхронный асепт
                }
            }

            AvailableServersBlock.Release();

            return ports;
        }

        // этот метод срабатывает при подключении клиента
        private void AcceptHandler(IAsyncResult data)
        {
            Runtime.DebugWrite("AcceptHandler1");
            AcceptingBlock.WaitOne();

            Socket listener = (Socket)data.AsyncState;

            // если майкнрафт клиент уже подключен то отвергаем это подключение и выходим нахер, ибо это какое-то левое подключение
            if (IsConnected || !AvailableServers.ContainsKey(listener))
            {
                AcceptingBlock.Release();
                Socket sock = listener.EndAccept(data);
                sock.Close();
                Runtime.DebugWrite("AcceptHandler1.1");
                listener.BeginAccept(null, 0, new AsyncCallback(AcceptHandler), listener); // возвращаем асинхронный асепт

                return;
            }

            // TODO: тут проверить тот ли клиент подключился
            AvailableServersBlock.WaitOne();
            string serverUUID = AvailableServers[listener];
            Runtime.DebugWrite("AcceptHandler2");
            if (base.Initialization(UUID, _sessionToken, serverUUID))
            {
                Runtime.DebugWrite("AcceptHandler3");
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
                        AvailableServers.Remove(sock);
                        sock.Close();
                    }
                }

                AvailableServersBlock.Release();
                AcceptingBlock.Release();

                Runtime.DebugWrite("AcceptHandler4");
            }
            else
            {
                AvailableServersBlock.Release();
                AcceptingBlock.Release();
                Socket sock = listener.EndAccept(data);
                sock.Close();
                Runtime.DebugWrite("AcceptHandler1.2");
            }

            listener.BeginAccept(null, 0, new AsyncCallback(AcceptHandler), listener); // возвращаем асинхронный асепт
        }

        protected override void Close(IPEndPoint point)
        {
            lock (_closeLock)
            {
                if (IsConnected)
                {
                    Runtime.DebugWrite("Close");
                    IsConnected = false;
                    Bridge.Close();
                    ServerSimulator.Close(); //закрываем соединение с клиентом   
                }
            }
        }

        override protected void Sending() //отправляет данные с майнкрафт клиента в сеть
        {
            SendingWait.WaitOne();
            Runtime.DebugWrite("sending begin");

            try
            {
                while (Bridge.IsConnected)
                {
                    byte[] buffer = new byte[1200];
                    int bytes = ServerSimulator.Receive(buffer);

                    if (bytes == 0)
                    {
                        Runtime.DebugWrite("CLOSE. BYTES IS 0");
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
                Runtime.DebugWrite("Sending " + e);
                Close(null);
                readingThread.Abort();
            }

        }

        override protected void Reading() //получаем данные из сети и отправляем на майкрафт клиент
        {
            ReadingWait.WaitOne();
            Runtime.DebugWrite("reading begin");

            bool isWorking = Bridge.Receive(out byte[] buffer);

            try
            {
                while (isWorking && Bridge.IsConnected && buffer.Length > 0)
                {
                    ServerSimulator.Send(buffer, buffer.Length, SocketFlags.None);
                    isWorking = Bridge.Receive(out buffer);
                }

            }
            catch
            {
                Runtime.DebugWrite("Reading exception ");
            }

            Runtime.DebugWrite("Reading " + Bridge.IsConnected);

            Close(null);
            sendingThread.Abort();
        }

    }
}
