using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NightWorld.Collections.Concurrent;

namespace Lexplosion.Logic.Network.SMP
{
    /// <summary>
    /// Надстройка над SmpClient для возможности соединения сразу с несколькими хостами.
    /// </summary>
    class SmpServer : IServerTransmitter
    {
        public bool IsWork { get; private set; } = true;

        private readonly ConcurrentDictionary<ClientDesc, SmpClient> _clients = new();
        private readonly ConcurrentDictionary<ClientDesc, Thread> _sendThreads = new();
        private readonly ConcurrentQueue<ClientDesc> _receivingQueue = new();

        private readonly AutoResetEvent _receiveWait = new AutoResetEvent(false);
        private readonly object _closeLocker = new object();

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
                    _receivingQueue.Enqueue(clientData);
                    _receiveWait.Set();
                });

                if (connected)
                {
                    client.MessageReceived -= messageReceived;
                    client.MessageReceived += delegate ()
                    {
                        _receivingQueue.Enqueue(clientData);
                        _receiveWait.Set();
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
                _clients[clientData] = client;
                _availableSend.Add(clientData);
                connected = true;
                connectedEvent.Set();
                _sendAvailableWaiting.Set();

                return true;
            }

            return false;
        }

        public ClientDesc Receive(out byte[] data)
        {
            if (_receivingQueue.Count > 0)
            {
                _receivingQueue.TryDequeue(out ClientDesc clientDesc);

                SmpClient client;
                if (_clients.ContainsKey(clientDesc))
                {
                    client = _clients[clientDesc];
                }
                else
                {
                    data = Array.Empty<byte>();
                    return clientDesc;
                }

                client.Receive(out data);

                return clientDesc;
            }
            else //буфер пуст
            {
                while (IsWork)
                {
                    _receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (_receivingQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        _receivingQueue.TryDequeue(out ClientDesc clientDesc);

                        if (_clients.ContainsKey(clientDesc))
                        {
                            SmpClient client = _clients[clientDesc];
                            client.Receive(out data);

                            return clientDesc;
                        }
                    }
                }
            }

            Runtime.DebugConsoleWrite("SMP SERVER STOP WORK");
            data = Array.Empty<byte>();
            return ClientDesc.Empty;
        }

        private ConcurrentHashSet<ClientDesc> _availableSend = new();
        private AutoResetEvent _sendAvailableWaiting = new(false);

        public void Send(byte[] inputData, ClientDesc clientDesc)
        {
            SmpClient client = _clients[clientDesc];
            bool sended = client.TrySend(inputData);

            if (!sended)
            {
                _availableSend.TryRemove(clientDesc, out _);
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    client.Send(inputData);
                    lock (_closeLocker)
                    {
                        if (!client.IsClosed)
                        {
                            _availableSend.Add(clientDesc);
                            _sendAvailableWaiting.Set();
                        }
                    }
                });
            }
        }

        public IReadOnlyCollection<ClientDesc> WaitSendAvailable()
        {
            if (_availableSend.Count > 0) return _availableSend;
            _sendAvailableWaiting.WaitOne();
            return _availableSend;
        }

        public void StopWork()
        {
        }

        public bool Close(ClientDesc point)
        {
            lock (_closeLocker)
            {
                if (_clients.ContainsKey(point))
                {
                    Runtime.DebugConsoleWrite("SmpServer client close " + point + " " + new System.Diagnostics.StackTrace());
                    _clients.TryRemove(point, out SmpClient client);
                    _availableSend.Remove(point);
                    client.Close();
                }
            }

            return true;
        }

        public event ClientPointHandle ClientClosing;
    }
}
