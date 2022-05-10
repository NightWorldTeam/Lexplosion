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
        protected ConcurrentDictionary<IPEndPoint, Socket> Connections; //это нужно для читающего потока
        protected ConcurrentDictionary<Socket, IPEndPoint> ClientsPoints; //этот список нужен для отправляющего потока
        protected List<Socket> Sockets; //этот список нужен для отправляющего потока
        protected Semaphore ConnectSemaphore; //блокировка для метода BeforeConnect

        const string serverType = "game-server"; // эта строка нужна при подключении к управляющему серверу
        int Port;

        public ServerBridge(string uuid, int port, bool directConnection, string server) : base(uuid, serverType, directConnection, server)
        {
            ConnectSemaphore = new Semaphore(1, 1);
            Connections = new ConcurrentDictionary<IPEndPoint, Socket>();
            ClientsPoints = new ConcurrentDictionary<Socket, IPEndPoint>();
            Sockets = new List<Socket>();
            Port = port;
        }

        protected override void ClientAbort(IPEndPoint point)
        {
            Console.WriteLine("clientAbort");
            AcceptingBlock.WaitOne();
            Console.WriteLine("clientAbort1");
            SendingBlock.WaitOne();
            Console.WriteLine("ABORT IN SENDING");

            //удаляем клиента везде
            if (Connections.ContainsKey(point)) // может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
            {
                Console.WriteLine("Sockets " + Sockets.Count);
                Sockets.Remove(Connections[point]);
                ClientsPoints.TryRemove(Connections[point], out _);
                Connections.TryRemove(point, out Socket sock);
                sock.Close(); //зыкрываем соединение
            }

            AcceptingBlock.Release();
            Console.WriteLine("ABORT NOT IN SENDING");
            SendingBlock.Release();
        }

        protected override bool BeforeConnect(IPEndPoint point)
        {
            bool value = true;
            Socket bridge = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                bridge.Connect("127.0.0.1", Port);
                Console.WriteLine("КОННЕКТ К ЛОКАЛКЕ!!!!");
                Console.WriteLine("Connections " + Connections.Count + " " + string.Join(", ", Connections.Keys));
                Connections[point] = bridge;
                Console.WriteLine("Connections1 " + Connections.Count + " " + string.Join(", ", Connections.Keys));
                Console.WriteLine("ClientsPoints " + ClientsPoints.Count + " " + string.Join(", ", ClientsPoints.Keys));
                ClientsPoints[bridge] = point;
                Console.WriteLine("ClientsPoints1 " + ClientsPoints.Count + " " + string.Join(", ", ClientsPoints.Keys));
            }
            catch
            {
                value = false;
            }
            AcceptingBlock.Release();

            if (value)
            {
                //добавляем клиента
                ConnectSemaphore.WaitOne();
                Sockets.Add(bridge);
                ConnectSemaphore.Release();
            }

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
                //Console.WriteLine("Sokets count " + Sockets.Count);
                List<Socket> listeningSokets = new List<Socket>(Sockets);
                //Console.WriteLine("listeningSokets count " + listeningSokets.Count);
                ConnectSemaphore.Release();

                try
                {
                    Socket.Select(listeningSokets, null, null, -1); //слушаем все сокеты
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("WAIT GDFGFDGFD");
                    SendingWait.WaitOne(); //ждём первого подключения
                    SendingBlock.Release();
                    Console.WriteLine("WAIT GDFGFDGFD1");

                    continue;
                }
                catch (SocketException)
                {
                    Console.WriteLine("SendingSocketException");
                    // TODO: тут что-то придумать
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine("SendingException " + e);
                    // TODO: какое-то странное исключение, выходим
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
                            Console.WriteLine("BYTES 0");
                            isDisconected.Add(ClientsPoints[sock]); //добавляем клиента в список чтобы потом отключить
                            continue;
                        }

                        byte[] data_ = new byte[bytes]; // TODO: тут хуевый перенос массива
                        for (int i = 0; i < bytes; i++)
                        {
                            data_[i] = data[i];
                        }

                        Server.Send(data_, ClientsPoints[sock]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("sending1 " + e);
                        isDisconected.Add(ClientsPoints[sock]); //добавляем клиента в список чтобы потом отключить
                    }
                }

                SendingBlock.Release();

                // отключаем клиентов которые попали в isDisconected
                foreach (IPEndPoint point in isDisconected)
                {
                    Console.WriteLine("DISCONECTED");
                    Server.Close(point); // при отключении клиента еще будет вызван метод ClientAbort
                }

                if (isDisconected.Count > 0)
                {
                    isDisconected.Clear();
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
                        Connections[point].Send(data, data.Length, SocketFlags.None);
                        AcceptingBlock.Release();
                    }
                    else // Количество байт 0 - значит соединение было обрвано
                    {
                        Console.WriteLine("SERVER CLOSE 1");
                        Server.Close(point);
                    }

                }
                catch (Exception e) // Обрываем соединение с этми клиентом нахуй
                {
                    Console.WriteLine("SERVER CLOSE 2 " + e);
                    Server.Close(point);
                }
            }
        }

        public override void StopWork()
        {
            base.StopWork();

            foreach (Socket sock in Sockets)
            {
                sock.Close();
            }
        }
    }
}
