using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class ClientBridge: NetworkClient // TODO: возможно Initialization заменить на конструктор
    {
        protected Socket ServerSimulator;
        protected Socket listener;

        protected List<Socket> Sockets = new List<Socket>();
        protected Dictionary<Socket, string> AvailableServers = new Dictionary<Socket, string>();

        protected AutoResetEvent WaitAccepting = new AutoResetEvent(false);
        protected Semaphore AcceptingBlock = new Semaphore(1,1); //блокировка на время работы метода AcceptHandler

        public bool IsInitialized { get; private set; } = false;
        private bool IsConnected = false; // когда будет подключен майкнрафт клиент эта переменная будет true

        public List<int> SetServers(List<string> servers)
        {
            List<int> ports = new List<int>();

            //добавляем новые сервера
            foreach (string server_uuid in servers)
            {
                if (!AvailableServers.ContainsValue(server_uuid))
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
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
        public void AcceptHandler(IAsyncResult data)
        {
            AcceptingBlock.WaitOne();

            if (IsInitialized)
            {
                Socket listener_ = (Socket)data.AsyncState;
                Socket serverSimulator_ = listener_.EndAccept(data);

                // если майкнрафт клиент уже подключен то отвергаем это подключение и выходим нахер, ибо это какое-то левое подключение
                if (IsConnected)
                {
                    AcceptingBlock.Release();
                    serverSimulator_.Close();
                    return;
                }

                // TODO: тут проверить тот ли клиент подключился
                IsConnected = true;

                listener = listener_;
                ServerSimulator = serverSimulator_;

                WaitAccepting.Set();
            }

            AcceptingBlock.Release();

        }

        public void StartEmulation(string UUID)
        {
            IsInitialized = true;

            new Thread(delegate () //поток принимающий новые подключения
            {
                WaitAccepting.WaitOne();

                string serverUUID;
                serverUUID = AvailableServers[listener];

                //закрываем другие сокеты
                foreach (Socket sock in AvailableServers.Keys)
                {
                    if (AvailableServers[listener] != serverUUID)
                    {
                        sock.Close();
                    }
                }

                base.Initialization(UUID, ((IPEndPoint)listener.LocalEndPoint).Port, serverUUID);

            }).Start();     

        }

        public override void Close(IPEndPoint point)
        {
            IsInitialized = false;
            IsConnected = false;
            ServerSimulator.Close(); //закрываем соединение с клиентом
        }

        override protected void Sending() //отправляет данные с майнкрафт клиента в сеть
        {
            try
            {
                while (Bridge.IsConnected)
                {
                    byte[] buffer = new byte[256];
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
            catch
            {
                Bridge.Close();
                Close(null);
            }

        }

        override protected void Reading() //получаем данные из сети и отправляем на майкрафт клиент
        {
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
            catch { }

            Console.WriteLine("ВЫШЕЛ " + isWorking + " " + Bridge.IsConnected);

            Bridge.Close();
            Close(null);

        }

    }
}
