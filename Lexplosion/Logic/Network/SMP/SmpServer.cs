using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    class SmpServer : IServerTransmitter
    {
        private class Message
        {
            public IPEndPoint Point;
            public bool IsFull;
        }

        public bool IsWork { get; private set; } = true;
        public bool WaitFullPackage
        {
            get
            {
                return waitFullPackage;
            }

            set
            {
                waitFullPackage = value;
                foreach (SmpClient client in clients.Values)
                {
                    client.WaitFullPackage = value;
                }
            }
        }

        private bool waitFullPackage = true;

        private readonly IPEndPoint localPoint;
        private readonly ConcurrentDictionary<IPEndPoint, SmpClient> clients = new ConcurrentDictionary<IPEndPoint, SmpClient>();
        private readonly ConcurrentQueue<Message> receivingQueue = new ConcurrentQueue<Message>();

        private readonly AutoResetEvent receiveWait = new AutoResetEvent(false);
        private readonly Semaphore cloaseBlock = new Semaphore(1, 1);

        public SmpServer(IPEndPoint point_)
        {
            localPoint = point_;
        }

        public bool Connect(IPEndPoint remoteIp)
        {
            SmpClient client = new SmpClient(localPoint, true);

            client.MessageReceived += delegate (bool isFull)
            {
                receivingQueue.Enqueue(new Message
                {
                    Point = remoteIp,
                    IsFull = isFull
                });

                receiveWait.Set();
            };

            client.ClientClosing += delegate (IPEndPoint ip)
            {
                ClientClosing?.Invoke(ip);
            };

            if (client.Connect(remoteIp))
            {
                clients[remoteIp] = client;
                return true;
            }

            return false;
        }

        public IPEndPoint Receive(out byte[] data)
        {
            if (receivingQueue.Count > 0)
            {
                receivingQueue.TryDequeue(out Message message);

                if ((WaitFullPackage && message.IsFull) || !WaitFullPackage)
                {
                    SmpClient client = clients[message.Point];
                    if (client.Receive(out data))
                    {
                        return message.Point;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else //буфер пуст
            {
                while (IsWork)
                {
                    receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (receivingQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        receivingQueue.TryDequeue(out Message message);

                        if ((WaitFullPackage && message.IsFull) || !WaitFullPackage)
                        {
                            SmpClient client = clients[message.Point];
                            if (client.Receive(out data))
                            {
                                return message.Point;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            data = null;
            return null;
        }

        public void Send(byte[] inputData, IPEndPoint ip)
        {
            SmpClient client = clients[ip];
            client.Send(inputData);
        }

        public void StopWork()
        {
        }

        public bool Close(IPEndPoint point)
        {
            cloaseBlock.WaitOne();
            if (clients.ContainsKey(point))
            {
                SmpClient client = clients[point];
                client.Close();
            }
            cloaseBlock.Release();

            return true;
        }

        public event PointHandle ClientClosing;
    }
}
