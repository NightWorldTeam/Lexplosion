using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class ServerBridge : NetworkServer
    {
        protected ConcurrentDictionary<IPEndPoint, Socket> Connections = new ConcurrentDictionary<IPEndPoint, Socket>(); //это нужно для читающего потока
        protected ConcurrentDictionary<Socket, IPEndPoint> ClientsPoints = new ConcurrentDictionary<Socket, IPEndPoint>(); //этот список нужен для отправляющего потока
        protected List<Socket> Sockets = new List<Socket>(); //этот список нужен для отправляющего потока
        protected Semaphore ConnectSemaphore = new Semaphore(1, 1); //блокировка для метода BeforeConnect

        protected AutoResetEvent SendingWait = new AutoResetEvent(false);
        protected AutoResetEvent ReadingWait = new AutoResetEvent(false);

        const string serverType = "game-server"; // эта строка нужна при подключении к управляющему серверу
        readonly int Port;

        private bool _isWork;
        private object _stopLosk = new object();
        private object _abortLoocker = new object();

        public ServerBridge(string uuid, string sessionToken, int localGamePort, bool directConnection, string server) : base(uuid, sessionToken, serverType, directConnection, server)
        {
            Port = localGamePort;
            _isWork = true;

            StartThreads();
        }

        protected override void ClientAbort(IPEndPoint point)
        {
            lock (_abortLoocker)
            {
                if (point != null && Connections.ContainsKey(point)) // может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
                {
                    Runtime.DebugWrite("clientAbort. StackTrace: " + new System.Diagnostics.StackTrace());    

                    AcceptingBlock.WaitOne();
                    Connections.TryRemove(point, out Socket sock);
                    sock.Close(); //зыкрываем соединение с майнкрафтом.
                    SendingBlock.WaitOne();

                    //удаляем клиента везде
                    Sockets.Remove(sock);
                    ClientsPoints.TryRemove(sock, out _);
                    base.ClientAbort(point);

                    AcceptingBlock.Release();
                    SendingBlock.Release();
                    Runtime.DebugWrite("clientAbort end");
                }
            }
        }

        protected override bool AfterConnect(IPEndPoint point)
        {
            Runtime.DebugWrite("Before connect method");

            bool value = true;
            Socket bridge = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                bridge.Connect("127.0.0.1", Port);
                Connections[point] = bridge;
                ClientsPoints[bridge] = point;
            }
            catch
            {
                value = false;

                if (Connections.ContainsKey(point))
                    Connections.TryRemove(point, out _);
                if (ClientsPoints.ContainsKey(bridge))
                    ClientsPoints.TryRemove(bridge, out _);
            }

            Runtime.DebugWrite("Before connect method 1");
            base.AfterConnect(point);
            Runtime.DebugWrite("Before connect method 2");

            if (value)
            {
                //добавляем клиента
                ConnectSemaphore.WaitOne();
                Sockets.Add(bridge);
                ConnectSemaphore.Release();
            }

            SendingWait.Set(); // если это первый клиент, то сейчас читающий поток будет запущен
            ReadingWait.Set();

            Runtime.DebugWrite("Before connect method end");

            return value;
        }

        protected override void Sending() //отправляем данные с майнкрафт клиентов в сеть
        {
            SendingWait.WaitOne(); //ждём первого подключения

            List<IPEndPoint> isDisconected = new List<IPEndPoint>();

            while (IsWork)
            {
                SendingBlock.WaitOne();

                ConnectSemaphore.WaitOne();
                List<Socket> listeningSokets = new List<Socket>(Sockets);
                ConnectSemaphore.Release();

                try
                {
                    Socket.Select(listeningSokets, null, null, -1); //слушаем все сокеты
                }
                catch (ArgumentNullException)
                {
                    Runtime.DebugWrite("SendingWait Start");

                    SendingWait.WaitOne(); //ждём первого подключения
                    SendingBlock.Release();

                    Runtime.DebugWrite("SendingWait End");

                    continue;
                }
                catch (SocketException e)
                {
                    // TODO: тут что-то придумать
                    Runtime.DebugWrite("Sending exeption " + e);
                    SendingBlock.Release();
                    continue;
                }
                catch (Exception e)
                {
                    Runtime.DebugWrite("Sending exeption " + e);
                    // TODO: какое-то странное исключение, выходим
                    SendingBlock.Release();
                    continue;
                }

                foreach (Socket sock in listeningSokets)
                {
                    try
                    {
                        //получем данные с локального сокета и отправляем клиенту через сеть с помощью SMP
                        byte[] data = new byte[1200]; // TODO: думаю тут можно заюзать sock.Available вместо 1200
                        int bytes = sock.Receive(data);

                        if (bytes == 0)
                        {
                            Runtime.DebugWrite("BYTES 0");
                            isDisconected.Add(ClientsPoints[sock]); //добавляем клиента в список чтобы потом отключить
                            continue;
                        }

                        byte[] data_ = new byte[bytes]; // TODO: тут хуевый перенос массива
                        for (int i = 0; i < bytes; i++)
                        {
                            data_[i] = data[i];
                        }

                        var point = ClientsPoints[sock];

                        Server.Send(data_, point);
                    }
                    catch (Exception e)
                    {
                        Runtime.DebugWrite("sending1 " + e);
                        isDisconected.Add(ClientsPoints[sock]); //добавляем клиента в список чтобы потом отключить
                    }
                }
                SendingBlock.Release();

                if (isDisconected.Count > 0)
                {
                    // отключаем клиентов которые попали в isDisconected
                    foreach (IPEndPoint point in isDisconected)
                    {
                        Runtime.DebugWrite("DISCONECTED");
                        Server.Close(point);
                        ClientAbort(point);
                    }

                    isDisconected = new List<IPEndPoint>();
                }
            }
        }

        protected override void Reading() //данные из сети отправляем майнкрафту
        {
            ReadingWait.WaitOne(); //ждём первого подключения
            ReadingWait.Set();

            while (IsWork)
            {
                IPEndPoint point = Server.Receive(out byte[] data);

                try
                {
                    if (data.Length != 0)
                    {
                        AcceptingBlock.WaitOne();
                        try
                        {
                            Connections[point].Send(data, data.Length, SocketFlags.None);
                            AcceptingBlock.Release();
                        }
                        catch // Обрываем соединение с этми клиентом нахуй
                        {
                            Runtime.DebugWrite("SERVER CLOSE 1 ");
                            AcceptingBlock.Release();
                            Server.Close(point);
                            ClientAbort(point);
                        }
                    }
                    else // Количество байт 0 - значит соединение было оборвано
                    {
                        Runtime.DebugWrite("SERVER CLOSE 2");
                        Server.Close(point);
                        ClientAbort(point);
                    }
                }
                catch // Обрываем соединение с этми клиентом нахуй
                {
                    Runtime.DebugWrite("SERVER CLOSE 3 ");
                    Server.Close(point);
                    ClientAbort(point);
                }
            }

            Runtime.DebugWrite("READING METHOD END");
        }

        public override void StopWork()
        {
            lock (_stopLosk)
            {
                if (_isWork)
                {
                    Runtime.DebugWrite("SERVER STOP WORK METHOD");
                    base.StopWork();

                    foreach (Socket sock in Sockets)
                    {
                        sock.Close();
                    }

                    _isWork = false;
                }
            }
        }
    }
}
