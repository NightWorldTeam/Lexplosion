using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Lexplosion.Logic.Network
{
    class ServerBridge: NetworkServer
    {
        protected ConcurrentDictionary<IPEndPoint, Socket> Connections; //это нужно для читающего потока
        protected ConcurrentDictionary<Socket, IPEndPoint> ClientsPoints; //этот список нужен для отправляющего потока
        protected List<Socket> Sockets; //этот список нужен для отправляющего потока

        public ServerBridge(int port): base(port)
        {
            Connections = new ConcurrentDictionary<IPEndPoint, Socket>();
            ClientsPoints = new ConcurrentDictionary<Socket, IPEndPoint>();
            Sockets = new List<Socket>();
        }

        protected override void ClientAbort(IPEndPoint point)
        {
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

        protected override void BeforeConnect(IPEndPoint point, int port)
        {
            Socket bridge = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bridge.Connect("127.0.0.1", port);
            Connections[point] = bridge;
            ClientsPoints[bridge] = point;
            AcceptingBlock.Release();

            //добавляем клиента
            ListSocketsBlock.WaitOne();
            Sockets.Add(bridge);
            ListSocketsBlock.Release();
        }

        protected override void Sending() //отправляем данные с майнкрафт клиентов в сеть
        {
            threadReset.WaitOne(); //ждём первого подключения

            List<Socket> isDisconected = new List<Socket>();

            while (IsWork)
            {
                SendingBlock.WaitOne();

                ListSocketsBlock.WaitOne();
                List<Socket> listeningSokets = new List<Socket>(Sockets);
                ListSocketsBlock.Release();

                try
                {
                    Socket.Select(listeningSokets, null, null, -1); //слушаем все сокеты
                }
                catch (ArgumentNullException e)
                {
                    threadReset.WaitOne(); //ждём первого подключения
                    SendingBlock.Release();

                    continue;
                }
                catch (SocketException e)
                {
                    // TODO: тут что-то придумать
                    continue;
                }
                catch
                {
                    // TODO: какое-то странное исключение, выходим
                }

                foreach (Socket sock in listeningSokets)
                {
                    try
                    {
                        //получем данные с локального сокета и отправляем клиенту через сеть с помощью SMP
                        byte[] data = new byte[256];
                        int bytes = sock.Receive(data); 

                        byte[] data_ = new byte[bytes]; // TODO: тут хуевый перенос массива
                        for (int i = 0; i < bytes; i++)
                        {
                            data_[i] = data[i];
                        }

                        Server.Send(data_, ClientsPoints[sock]);
                    }
                    catch (SocketException e)
                    {
                        isDisconected.Add(sock); //добавляем клиента в список чтобы потом отключить
                    }
                    catch
                    {
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

        protected override void Reading() //данные из сети отправляем майнкрафт клиентам
        {
            threadResetReading.WaitOne(); //ждём первого подключения
            threadResetReading.Set();

            while (IsWork)
            {
                try
                {
                    IPEndPoint point = Server.Receive(out byte[] data);
                    AcceptingBlock.WaitOne();
                    Connections[point].Send(data, data.Length, SocketFlags.None);
                    AcceptingBlock.Release();
                }
                catch 
                {
                    break;
                    // TODO: тут че-то сделать
                }
            }
        }

        public override void StopWork()
        {
            base.StopWork();

            foreach(Socket sock in Sockets)
            {
                sock.Close();
            }
        }
    }
}
