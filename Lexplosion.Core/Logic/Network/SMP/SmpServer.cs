using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    /// <summary>
    /// Надстройка над SmpClient для возможности соединения сразу с несколькими хостами.
    /// </summary>
    class SmpServer : IServerTransmitter
    {
        public bool IsWork { get; private set; } = true;

        private readonly ConcurrentDictionary<IPEndPoint, SmpClient> clients = new ConcurrentDictionary<IPEndPoint, SmpClient>();
        private readonly ConcurrentQueue<IPEndPoint> receivingQueue = new ConcurrentQueue<IPEndPoint>();

        private readonly AutoResetEvent receiveWait = new AutoResetEvent(false);
        private readonly Semaphore cloaseBlock = new Semaphore(1, 1);

        public bool Connect(IPEndPoint localPoint, IPEndPoint remotePoint, byte[] connectionCode)
        {
            var connectedEvent = new ManualResetEvent(false);
            var client = new SmpClient(localPoint);

            bool connected = false;

            Action messageReceived = null;
            messageReceived = delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    connectedEvent.WaitOne();
                    receivingQueue.Enqueue(remotePoint);
                    receiveWait.Set();
                });

                if (connected)
                {
                    client.MessageReceived -= messageReceived;
                    client.MessageReceived += delegate ()
                    {
                        receivingQueue.Enqueue(remotePoint);
                        receiveWait.Set();
                    };
                }

                //if (connected)
                //{
                //    receivingQueue.Enqueue(remotePoint);
                //    receiveWait.Set();
                //}
                //else
                //{
                //    ThreadPool.QueueUserWorkItem(delegate (object state)
                //    {
                //        connectedEvent.WaitOne();
                //        receivingQueue.Enqueue(remotePoint);
                //        receiveWait.Set();
                //    });
                //}
            };

            client.MessageReceived += messageReceived;

            client.ClientClosing += delegate (IPEndPoint ip)
            {
                ClientClosing?.Invoke(ip);
            };

            if (client.Connect(remotePoint, connectionCode))
            {
                Thread.Sleep(300);
                clients[remotePoint] = client;
                connected = true;
                connectedEvent.Set();

                return true;
            }

            return false;
        }

        public IPEndPoint Receive(out byte[] data)
        {
            if (receivingQueue.Count > 0)
            {
                receivingQueue.TryDequeue(out IPEndPoint point);

                SmpClient client;
                if (clients.ContainsKey(point))
                {
                    client = clients[point];
                }
                else
                {
                    data = new byte[0];
                    return null;
                }

                if (client.Receive(out data))
                {
                    return point;
                }
                else
                {
                    return null;
                }
            }
            else //буфер пуст
            {
                while (IsWork)
                {
                    receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (receivingQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        receivingQueue.TryDequeue(out IPEndPoint point);

                        if (clients.ContainsKey(point))
                        {
                            SmpClient client = clients[point];
                            if (client.Receive(out data))
                            {
                                return point;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            Runtime.DebugWrite("SMP SERVER STOP WORK");
            data = new byte[0];
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
            if (point != null && clients.ContainsKey(point))
            {
                clients.TryRemove(point, out SmpClient client);
                client.Close();
            }
            cloaseBlock.Release();

            return true;
        }

        public event PointHandle ClientClosing;
    }
}
