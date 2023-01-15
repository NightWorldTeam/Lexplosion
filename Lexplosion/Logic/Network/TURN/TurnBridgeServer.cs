using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Markup;

namespace Lexplosion.Logic.Network.TURN
{
    class TurnBridgeServer : IServerTransmitter
    {
        private ConcurrentDictionary<IPEndPoint, Socket> _pointsSockets = new ConcurrentDictionary<IPEndPoint, Socket>();
        private List<Socket> _sockets = new List<Socket>();

        private object _waitDeletingLoocker = new object();
        private ManualResetEvent _waitConnections = new ManualResetEvent(false); // блокировка метода Receive, если нет клиентов

        private bool IsWork = true;

        public bool Connect(string selfUUID, string hostUUID, out IPEndPoint point)
        {
            try
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
                sock.Connect(new IPEndPoint(IPAddress.Parse("194.61.2.176"), 9765));
                sock.Send(data);

                lock (_waitDeletingLoocker)
                {
                    _pointsSockets[(IPEndPoint)sock.LocalEndPoint] = sock;
                    _sockets.Add(sock);
                }

                _waitConnections.Set();

                point = (IPEndPoint)sock.LocalEndPoint;

                Runtime.DebugWrite("CONNECTED FGDSGFSD");
            }
            catch
            {
                point = null;
                return false;
            }

            return true;
        }

        public IPEndPoint Receive(out byte[] data)
        {
            while (IsWork)
            {
                _waitConnections.WaitOne(); // тут метод остановится, если нет ни одного клиента

                List<Socket> sockets_;
                lock (_waitDeletingLoocker)
                    sockets_ = new List<Socket>(_sockets);

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
            _pointsSockets[ip].Send(inputData);
        }

        public void StopWork()
        {
            IsWork = false;
            lock (_waitDeletingLoocker)
            {
                foreach (var socket in _sockets)
                {
                    socket.Close();
                }
            }
        }

        public bool Close(IPEndPoint point)
        {
            Runtime.DebugWrite("TURN CLOSE ");
            lock (_waitDeletingLoocker)
            {
                // может произойти хуйня, что этот метод будет вызван 2 раза для одного хоста, поэтому проверим не удалили ли мы его уже
                if (IsWork && _pointsSockets.ContainsKey(point))
                {
                    Runtime.DebugWrite("TRUN CLOSE GSFSDGF");
                    _pointsSockets.TryRemove(point, out Socket sock);
                    _sockets.Remove(sock);
                    if (_sockets.Count == 0) // если не осталось клиентов, то стопаем метод Receive
                    {
                        _waitConnections.Reset();
                    }
                    sock.Close();
                }
            }
            Runtime.DebugWrite("TURN END CLOSE ");

            return true;
        }

        public event PointHandle ClientClosing;
    }
}
