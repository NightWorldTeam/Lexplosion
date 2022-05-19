using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network.TURN
{
    class TurnBridgeServer : IServerTransmitter
    {
        protected ConcurrentDictionary<IPEndPoint, Socket> pointsSockets = new ConcurrentDictionary<IPEndPoint, Socket>();
        protected List<Socket> sockets = new List<Socket>();

        protected Semaphore WaitDeletingConnection = new Semaphore(1, 1);
        protected ManualResetEvent WaitConnections = new ManualResetEvent(false); // блокировка метода Receive, если нет клиентов

        protected bool IsWork = true;

        public bool Connect(string selfUUID, string hostUUID, out IPEndPoint point)
        {
            byte[] data = new byte[64];
            byte[] bselfUUID = Encoding.UTF8.GetBytes(selfUUID);
            byte[] bhostUUID = Encoding.UTF8.GetBytes(hostUUID);

            for (int i = 0; i < bselfUUID.Length; i++)
            {
                data[i] = bselfUUID[i];
            }

            for (int i = 0; i < bhostUUID.Length; i++)
            {
                data[i + 32] = bhostUUID[i];
            }

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(new IPEndPoint(IPAddress.Parse("194.61.2.176"), 8765)); // TODO: обернуть в трай
            sock.Send(data);

            WaitDeletingConnection.WaitOne();
            pointsSockets[(IPEndPoint)sock.LocalEndPoint] = sock;
            sockets.Add(sock);
            WaitDeletingConnection.Release();
            WaitConnections.Set();

            point = (IPEndPoint)sock.LocalEndPoint;

            Console.WriteLine("CONNECTED FGDSGFSD");

            return true;
        }

        public IPEndPoint Receive(out byte[] data)
        {
            while (IsWork)
            {
                WaitConnections.WaitOne(); // тут метод остановится, если нет ни одного клиента

                WaitDeletingConnection.WaitOne();
                List<Socket> sockets_ = new List<Socket>(sockets);
                WaitDeletingConnection.Release();

                Socket.Select(sockets_, null, null, -1);
                Socket sock = sockets_[0];

                IPEndPoint point;
                // Полученный из select сокет может быть отключенны и тогда RemoteEndPoint выкинет исключение. В этом случае мы продалжаем цикл и снова пытаемся считать данные
                try
                {
                    point = (IPEndPoint)sock.LocalEndPoint;
                }
                catch
                {
                    continue;
                }

                try
                {
                    data = new byte[sock.Available];
                    sock.Receive(data);
                }
                catch
                {
                    data = new byte[0];
                }

                return point;
            }

            data = new byte[0];
            return null;
        }

        public void Send(byte[] inputData, IPEndPoint ip)
        {
            pointsSockets[ip].Send(inputData);
        }

        public void StopWork()
        {
            IsWork = false;
            WaitDeletingConnection.WaitOne();
            foreach (var socket in sockets)
            {
                socket.Close();
            }
            WaitDeletingConnection.Release();
        }

        public bool Close(IPEndPoint point)
        {
            Console.WriteLine("TURN CLOSE ");
            WaitDeletingConnection.WaitOne();
            // может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
            if (IsWork && pointsSockets.ContainsKey(point))
            {
                Console.WriteLine("TRUN CLOSE GSFSDGF");
                pointsSockets.TryRemove(point, out Socket sock);
                sockets.Remove(sock);
                if (sockets.Count == 0) // если не осталось клиентов, то стопаем метод Receive
                {
                    WaitConnections.Reset();
                }
                sock.Close();
            }
            ClientClosing?.Invoke(point); //Вызываем событие закрытия
            WaitDeletingConnection.Release();

            return true;
        }

        public event PointHandle ClientClosing;
    }
}
