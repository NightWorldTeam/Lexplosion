using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class ClientBridge : NetworkClient // TODO: возможно Initialization заменить на конструктор
    {
        protected Socket ServerSimulator;

        protected List<Socket> Sockets = new List<Socket>();
        protected Dictionary<Socket, string> AvailableServers = new Dictionary<Socket, string>();

        protected Semaphore AcceptingBlock = new Semaphore(1, 1); //блокировка на время работы метода AcceptHandler
        protected AutoResetEvent SendingWait = new AutoResetEvent(false);
        protected AutoResetEvent ReadingWait = new AutoResetEvent(false);

        public bool IsInitialized { get; private set; } = true;
        private bool IsConnected = false; // когда будет подключен майкнрафт клиент эта переменная будет true
        string UUID = "";

        const string clientType = "game-client"; // эта строка нужна при подключении к управляющему серверу

        public ClientBridge(string uuid) : base(clientType)
        {
            UUID = uuid;
        }

        public List<int> SetServers(List<string> servers)
        {
            List<int> ports = new List<int>();

            //добавляем новые сервера
            foreach (string server_uuid in servers)
            {
                if (!AvailableServers.ContainsValue(server_uuid))
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Bind(new IPEndPoint(IPAddress.Any, 0));
                    sock.Listen(1);

                    AvailableServers[sock] = server_uuid;
                    Sockets.Add(sock);
                }
            }

            //убираем сервера, которых нет в списке

            Socket[] values = new Socket[AvailableServers.Count];
            AvailableServers.Keys.CopyTo(values, 0);

            foreach (Socket serverSocket in values)
            {
                if (!servers.Contains(AvailableServers[serverSocket]))
                {
                    Sockets.Remove(serverSocket);
                    AvailableServers.Remove(serverSocket);
                    serverSocket.Close();
                }
                else
                {
                    ports.Add(((IPEndPoint)serverSocket.LocalEndPoint).Port); //добавояем порт сокета в список
                    serverSocket.BeginAccept(null, 0, new AsyncCallback(AcceptHandler), serverSocket); // запусакаем асинхронный асепт
                }
            }

            return ports;
        }

        // этот метод срабатывает при подключении клиента
        private void AcceptHandler(IAsyncResult data)
        {
            AcceptingBlock.WaitOne();

            if (IsInitialized)
            {
                Socket listener = (Socket)data.AsyncState;

                // если майкнрафт клиент уже подключен то отвергаем это подключение и выходим нахер, ибо это какое-то левое подключение
                if (IsConnected)
                {
                    AcceptingBlock.Release();
                    Socket sock = listener.EndAccept(data);
                    sock.Close();

                    return;
                }

                // TODO: тут проверить тот ли клиент подключился
                string serverUUID = AvailableServers[listener];
                base.Initialization(UUID, serverUUID);

                Socket serverSimulator_ = listener.EndAccept(data);
                ServerSimulator = serverSimulator_;

                ReadingWait.Set();
                SendingWait.Set();

                //закрываем другие сокеты
                foreach (Socket sock in AvailableServers.Keys)
                {
                    if (AvailableServers[listener] != serverUUID)
                    {
                        sock.Close();
                    }
                }

                IsConnected = true;
            }

            AcceptingBlock.Release();
        }

        public override void Close(IPEndPoint point)
        {
            Console.WriteLine("Close");
            IsInitialized = false;
            IsConnected = false;
            ServerSimulator.Close(); //закрываем соединение с клиентом
        }

        override protected void Sending() //отправляет данные с майнкрафт клиента в сеть
        {
            SendingWait.WaitOne();
            Console.WriteLine("sending begin");

            try
            {
                while (Bridge.IsConnected)
                {
                    byte[] buffer = new byte[500];
                    int bytes = ServerSimulator.Receive(buffer);
                    //Array.Resize(ref data, bytes);

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
                Console.WriteLine("Sending " + e);
                Bridge.Close();
                Close(null);
            }

        }

        override protected void Reading() //получаем данные из сети и отправляем на майкрафт клиент
        {
            ReadingWait.WaitOne();
            Console.WriteLine("reading begin");

            bool isWorking = Bridge.Receive(out byte[] buffer);

            try
            {
                while (isWorking && Bridge.IsConnected)
                {
                    /*try
                    {        
                        Client.Send(buffer, buffer.Length, SocketFlags.None);
                        isWorking = Bridge.Receive(out buffer);
                    }
                    catch
                    {
                        break;
                    }*/
                    ServerSimulator.Send(buffer, buffer.Length, SocketFlags.None);
                    isWorking = Bridge.Receive(out buffer);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Reading exception " + e);
            }

            Console.WriteLine("Reading " + Bridge.IsConnected);

            Bridge.Close();
            Close(null);

        }

    }
}
