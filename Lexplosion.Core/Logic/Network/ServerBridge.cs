using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class ServerBridge : NetworkServer
    {
        protected ConcurrentDictionary<ClientDesc, Socket> Connections = new(); //это нужно для читающего потока
        protected ConcurrentDictionary<Socket, ClientDesc> ClientsPoints = new(); //этот список нужен для отправляющего потока
        protected List<Socket> Sockets = new(); //этот список нужен для отправляющего потока
        protected Semaphore ConnectSemaphore = new(1, 1); //блокировка для метода BeforeConnect
        protected Semaphore SendingBlock = new(1, 1); //блокировка во время работы метода Sending

        protected AutoResetEvent SendingWait = new(false);
        protected AutoResetEvent ReadingWait = new(false);

        const string SERVER_TYPE = "game-server"; // эта строка нужна при подключении к управляющему серверу
        readonly int Port;

        private bool _isWork;
        private object _stopLosk = new object();
        private object _abortLoocker = new object();

        public ServerBridge(string uuid, string sessionToken, int localGamePort, bool directConnection, ControlServerData server) : base(uuid, sessionToken, SERVER_TYPE, directConnection, server)
        {
            Port = localGamePort;
            _isWork = true;

            StartThreads();
        }

        protected override void ClientAbort(ClientDesc clientData)
        {
            lock (_abortLoocker)
            {
                Runtime.DebugWrite("ClientAbort start. clientData: " + clientData);
                Runtime.DebugWrite(String.Join(", ", Connections) + " " + Connections.ContainsKey(clientData));
                if (Connections.ContainsKey(clientData)) // может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
                {
                    Runtime.DebugWrite("clientAbort. StackTrace: " + new System.Diagnostics.StackTrace());

                    AcceptingBlock.WaitOne();
                    Connections.TryRemove(clientData, out Socket sock);
                    sock.Close(); //зыкрываем соединение с майнкрафтом.
                    SendingBlock.WaitOne();

                    //удаляем клиента везде
                    Sockets.Remove(sock);
                    ClientsPoints.TryRemove(sock, out _);
                    base.ClientAbort(clientData);

                    AcceptingBlock.Release();
                    SendingBlock.Release();
                    Runtime.DebugWrite("clientAbort end, sock: " + sock.GetHashCode() + " " + clientData);
                }
            }
        }

        protected override bool AfterConnect(ClientDesc clientData)
        {
            Runtime.DebugWrite("Before connect method");

            bool value = true;
            Socket bridge = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                bridge.Connect("127.0.0.1", Port);
                Connections[clientData] = bridge;
                ClientsPoints[bridge] = clientData;
            }
            catch
            {
                value = false;

                if (Connections.ContainsKey(clientData))
                    Connections.TryRemove(clientData, out _);
                if (ClientsPoints.ContainsKey(bridge))
                    ClientsPoints.TryRemove(bridge, out _);
            }

            Runtime.DebugWrite("Before connect method 1");
            base.AfterConnect(clientData);
            Runtime.DebugWrite("Before connect method 2");

            if (value)
            {
                //добавляем клиента
                ConnectSemaphore.WaitOne();
                Sockets.Add(bridge);
                ConnectSemaphore.Release();
                Runtime.DebugWrite("sock " + bridge.GetHashCode() + ", point " + clientData);
            }

            SendingWait.Set(); // если это первый клиент, то сейчас читающий поток будет запущен
            ReadingWait.Set();

            Runtime.DebugWrite("Before connect method end");

            return value;
        }

        protected override void Sending() //отправляем данные с майнкрафт клиентов в сеть
        {
            SendingWait.WaitOne(); //ждём первого подключения

            List<ClientDesc> isDisconected = new List<ClientDesc>();

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
                            Runtime.DebugWrite("BYTES 0. listeningSokets Count " + listeningSokets.Count + ", Sockets count " + Sockets.Count);
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
                        Runtime.DebugWrite("sending1. sock " + sock.GetHashCode() + " listeningSokets Count " + listeningSokets.Count + ", Sockets count " + Sockets.Count + ", Exception: " + e);
                        isDisconected.Add(ClientsPoints[sock]); //добавляем клиента в список чтобы потом отключить
                    }
                }
                SendingBlock.Release();

                if (isDisconected.Count > 0)
                {
                    // отключаем клиентов которые попали в isDisconected
                    foreach (ClientDesc client in isDisconected)
                    {
                        Runtime.DebugWrite("DISCONECTED");
                        Server.Close(client);
                        ClientAbort(client);
                    }

                    isDisconected = new List<ClientDesc>();
                }
            }
        }

        protected override void Reading() //данные из сети отправляем майнкрафту
        {
            ReadingWait.WaitOne(); //ждём первого подключения
            ReadingWait.Set();

            try
            {
                while (IsWork)
                {
                    ClientDesc client = Server.Receive(out byte[] data);

                    try
                    {
                        if (data.Length != 0)
                        {
                            AcceptingBlock.WaitOne();
                            try
                            {
                                Connections[client].Send(data, data.Length, SocketFlags.None);
                            }
                            catch (KeyNotFoundException) // point отсуствует, пробуем повторить дождавшись окончания работы метода подключения
                            {
                                Runtime.DebugWrite("KeyNotFoundException");
                                AcceptingBlock.Release();
                                ConnectionWait.WaitOne(); // если метод подключения в процессе работы, то мы тут остановимся

                                AcceptingBlock.WaitOne();
                                Connections[client].Send(data, data.Length, SocketFlags.None);
                            }
                            finally // освобождаем семофор. если будет исключение, нижний catch поймает его и выполнит обрыв соединения
                            {
                                AcceptingBlock.Release();
                            }
                        }
                        else // Количество байт 0 - значит соединение было оборвано
                        {
                            Runtime.DebugWrite("SERVER CLOSE 2");
                            Server.Close(client);
                            ClientAbort(client);
                        }
                    }
                    catch (Exception e) // Обрываем соединение с этми клиентом нахуй
                    {
                        Runtime.DebugWrite("SERVER CLOSE 3 " + e);
                        Server.Close(client);
                        ClientAbort(client);
                    }
                }
            }
            catch { }

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
