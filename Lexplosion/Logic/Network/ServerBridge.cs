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

        public ServerBridge(string uuid, int port) : base(uuid, serverType)
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
            SendingBlock.WaitOne();

            //удаляем клиента везде
            Sockets.Remove(Connections[point]);
            ClientsPoints.TryRemove(Connections[point], out _);
            Connections.TryRemove(point, out Socket sock);
            sock.Close(); //зыкрываем соединение

            AcceptingBlock.Release();
            SendingBlock.Release();
        }

        protected override void BeforeConnect(IPEndPoint point)
        {
            Socket bridge = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bridge.Connect("127.0.0.1", Port);
            Connections[point] = bridge;
            ClientsPoints[bridge] = point;
            AcceptingBlock.Release();

            //добавляем клиента
            ConnectSemaphore.WaitOne();
            Sockets.Add(bridge);
            ConnectSemaphore.Release();
        }

        protected override void Sending() //отправляем данные с майнкрафт клиентов в сеть
        {
            SendingWait.WaitOne(); //ждём первого подключения

            List<Socket> isDisconected = new List<Socket>();

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
                catch (ArgumentNullException e)
                {
                    SendingWait.WaitOne(); //ждём первого подключения
                    SendingBlock.Release();

                    continue;
                }
                catch (SocketException e)
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
                        byte[] data = new byte[500];
                        int bytes = sock.Receive(data);

                        if (bytes == 0)
                        {
                            Console.WriteLine("Bytes is 0");
                            // TODO: закрывать соединение
                        }

                        byte[] data_ = new byte[bytes]; // TODO: тут хуевый перенос массива
                        for (int i = 0; i < bytes; i++)
                        {
                            data_[i] = data[i];
                        }

                        Server.Send(data_, ClientsPoints[sock]);
                        Console.WriteLine("ОТПРАВИЛ БЛЯТЬ");
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("sending1 ");
                        isDisconected.Add(sock); //добавляем клиента в список чтобы потом отключить
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("sending2 " + e);
                        // TODO: подумать че делать
                    }
                }

                SendingBlock.Release();

                // отключаем клиентов которые попали в isDisconected
                foreach (Socket sock in isDisconected)
                {
                    IPEndPoint point = ClientsPoints[sock];
                    Server.Close(point); // при отключении клиента еще будет вызван метод ClientAbort
                }

            }
        }

        protected override void Reading() //данные из сети отправляем майнкрафту
        {
            ReadingWait.WaitOne(); //ждём первого подключения
            ReadingWait.Set();

            while (IsWork)
            {
                try
                {
                    IPEndPoint point = Server.Receive(out byte[] data);
                    AcceptingBlock.WaitOne();
                    Connections[point].Send(data, data.Length, SocketFlags.None);
                    Console.WriteLine("ПРИНЯЛ БЛЯТЬ");
                    AcceptingBlock.Release();
                }
                catch (Exception e)
                {
                    Console.WriteLine("reading " + e);
                    break;
                    // TODO: тут че-то сделать
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
