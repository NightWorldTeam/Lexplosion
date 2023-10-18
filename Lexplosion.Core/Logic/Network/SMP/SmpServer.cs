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

        private readonly ConcurrentDictionary<ClientDesc, SmpClient> clients = new();
        private readonly ConcurrentQueue<ClientDesc> receivingQueue = new();

        private readonly AutoResetEvent receiveWait = new AutoResetEvent(false);
        private readonly Semaphore cloaseBlock = new Semaphore(1, 1);

        public bool Connect(IPEndPoint localPoint, ClientDesc clientData, byte[] connectionCode)
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
                    receivingQueue.Enqueue(clientData);
                    receiveWait.Set();
                });

                if (connected)
                {
                    client.MessageReceived -= messageReceived;
                    client.MessageReceived += delegate ()
                    {
                        receivingQueue.Enqueue(clientData);
                        receiveWait.Set();
                    };
                }
            };

            client.MessageReceived += messageReceived;

            client.ClientClosing += delegate (IPEndPoint point)
            {
                ClientClosing?.Invoke(clientData);
            };

            if (client.Connect(clientData.Point, connectionCode))
            {
                clients[clientData] = client;
                connected = true;
                connectedEvent.Set();

                return true;
            }

            return false;
        }

        public ClientDesc Receive(out byte[] data)
        {
            if (receivingQueue.Count > 0)
            {
                receivingQueue.TryDequeue(out ClientDesc clientDesc);

                SmpClient client;
                if (clients.ContainsKey(clientDesc))
                {
                    client = clients[clientDesc];
                }
                else
                {
                    data = new byte[0];
                    return clientDesc;
                }

                client.Receive(out data);

                return clientDesc;
            }
            else //буфер пуст
            {
                while (IsWork)
                {
                    receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (receivingQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        receivingQueue.TryDequeue(out ClientDesc clientDesc);

                        if (clients.ContainsKey(clientDesc))
                        {
                            SmpClient client = clients[clientDesc];
                            client.Receive(out data);

                            return clientDesc;
                        }
                    }
                }
            }

            Runtime.DebugConsoleWrite("SMP SERVER STOP WORK");
            data = new byte[0];
            return ClientDesc.Empty;
        }

        public void Send(byte[] inputData, ClientDesc clientDesc)
        {
            SmpClient client = clients[clientDesc];
            client.Send(inputData);
        }

        public void StopWork()
        {
        }

        public bool Close(ClientDesc point)
        {
            cloaseBlock.WaitOne();
            if (clients.ContainsKey(point))
            {
                Runtime.DebugConsoleWrite("SmpServer client close " + point + " " + new System.Diagnostics.StackTrace());
                clients.TryRemove(point, out SmpClient client);
                client.Close();
            }
            cloaseBlock.Release();

            return true;
        }

        public event ClientPointHandle ClientClosing;
    }
}
