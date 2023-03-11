using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LumiSoft.Net.STUN.Client;

namespace Lexplosion.Logic.Network
{
    using SMP;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using TURN;

    abstract class NetworkServer
    {
        protected Thread AcceptingThread;
        protected Thread ReadingThread;
        protected Thread SendingThread;
        private Thread MaintainingThread;

        protected Semaphore AcceptingBlock; //блокировка во время приёма подключения
        protected Semaphore SendingBlock; //блокировка во время работы метода Sending
        private AutoResetEvent ControlConnectionBlock; // чтобы методы MaintainingConnection и Accepting одновременно не обраащлись к управляющему серверу

        protected AutoResetEvent SendingWait;
        protected AutoResetEvent ReadingWait;

        protected IServerTransmitter Server;

        protected bool IsWork = false;

        protected string UUID;
        protected string _sessionToken;
        protected bool DirectConnection;
        protected string ControlServer;

        private Socket _controlConnection;

        public event Action<string> ConnectingUser;
        public event Action<string> DisconnectedUser;

        private ConcurrentDictionary<string, IPEndPoint> _uuidPointPair;
        private ConcurrentDictionary<IPEndPoint, string> _pointUuidPair;

        protected HashSet<string> KickedClients; //тут хранятся выкинутые клиенты

        public NetworkServer(string uuid, string sessionToken, string serverType, bool directConnection, string controlServer)
        {
            UUID = uuid;
            _sessionToken = sessionToken;
            IsWork = true;
            ControlServer = controlServer;
            DirectConnection = directConnection;
            AcceptingBlock = new Semaphore(1, 1);
            SendingBlock = new Semaphore(1, 1);
            ControlConnectionBlock = new AutoResetEvent(false);

            SendingWait = new AutoResetEvent(false);
            ReadingWait = new AutoResetEvent(false);

            KickedClients = new HashSet<string>();

            // тут хранится список клиентов. В одном соответсвие uuid и ip, в другом наоборот
            _uuidPointPair = new ConcurrentDictionary<string, IPEndPoint>();
            _pointUuidPair = new ConcurrentDictionary<IPEndPoint, string>();

            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Server = DirectConnection ? new SmpServer() : new TurnBridgeServer();
            Server.ClientClosing += ClientAbort;

            ReadingThread = new Thread(delegate () //этот поток читает сообщения от клиента
            {
                Reading();
            });

            SendingThread = new Thread(delegate () //этот поток отправляет сообщения клиенту
            {
                Sending();
            });

            AcceptingThread = new Thread(delegate () //поток принимающий новые подключения
            {
                Accepting(serverType);
            });

            MaintainingThread = new Thread(delegate () //поток отправляющий управляющему серверу пустые пакеты для поддержиния соединения
            {
                MaintainingConnection();
            });

            AcceptingThread.Start();
            SendingThread.Start();
            ReadingThread.Start();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepeareRepeat()
        {
            ControlConnectionBlock.WaitOne();

            try
            {
                _controlConnection.Send(new byte[1] { ControlSrverCodes.Z });
            }
            finally
            {
                _controlConnection.Close();
            }

            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Runtime.DebugWrite("Repeat connection to control server");
            DirectConnection = false;
        }

        protected void Accepting(string serverType)
        {
            // если стоит парметр установки прямого соединения, то проверяем, возможно ли его вообще установить. если нет - переходим на TURN
            if (DirectConnection)
            {
                STUN_Result result = null;
                try
                {
                    // TODO: сделать получения списка stun серверов с нашего сервера
                    result = STUN_Client.Query("stun.l.google.com", 19305, new IPEndPoint(IPAddress.Any, 0)); //получем наш внешний адрес
                    Runtime.DebugWrite("NatType " + result.NetType.ToString());
                }
                catch { }

                if (result == null || result.NetType == STUN_NetType.UdpBlocked || result.NetType == STUN_NetType.Symmetric || result.NetType == STUN_NetType.SymmetricUdpFirewall)
                {
                    DirectConnection = false;
                }
            }

            while (true)
            {
                bool needRepeat = false;

                //подключаемся к управляющему серверу
                try
                {
                    _controlConnection.Connect(new IPEndPoint(IPAddress.Parse(ControlServer), 4565));
                }
                catch
                {
                    //при ошибке ждем 30 секунд и пытаемся повторить
                    Thread.Sleep(30000);
                    continue;
                }

                try
                {
                    string st =
                    "{\"UUID\" : \"" + UUID + "\"," +
                    " \"type\": \"" + serverType + "\"," +
                    " \"method\": \"" + (DirectConnection ? "STUN" : "TURN") + "\"," +
                    " \"sessionToken\" : \"" + _sessionToken + "\"}";

                    byte[] sendData = Encoding.UTF8.GetBytes(st);
                    _controlConnection.Send(sendData); //авторизируемся на упрявляющем сервере
                }
                catch
                {
                    PrepeareRepeat();
                    continue;
                }

                MaintainingThread.Start();
                Runtime.DebugWrite("ASZSAFDSDFAFSADSAFDFSDSD");

                while (IsWork)
                {
                    try
                    {
                        string clientUUID;
                        string myPoint = null;
                        IPEndPoint localPoint = null;

                        {
                            byte[] data = new byte[33];

                            Runtime.DebugWrite("ControlServerRecv");
                            ControlConnectionBlock.Set(); // освобождаем семафор переда как начать слушать сокет. Ждать мы на Receive можем долго
                            _controlConnection.ReceiveTimeout = -1; // делаем бесконечное ожидание

                            int bytes;
                            try
                            {
                                bytes = _controlConnection.Receive(data);
                            }
                            catch
                            {
                                needRepeat = true;
                                break;
                            }
                            finally
                            {
                                ControlConnectionBlock.WaitOne(); // блочим семофор
                                _controlConnection.ReceiveTimeout = 10000; //огрниччиваем ожидание до 10 секунд
                                Runtime.DebugWrite("ControlServerEndRecv");
                            }

                            if (bytes > 1 && data[0] == ControlSrverCodes.A) // data[0] == 97 значит поступил запрос на подключение
                            {
                                clientUUID = Encoding.UTF8.GetString(data, 1, 32); // получаем UUID клиента

                                // этот клиент был кикнут. послыем его нахуй
                                lock (KickedClients)
                                {
                                    if (KickedClients.Contains(clientUUID))
                                    {
                                        _controlConnection.Send(new byte[1] { ControlSrverCodes.E }); //отправляем серверу отказ
                                        continue;
                                    }
                                }

                                // такой клиент уже подключен. Значит обрываем прошлое соединение
                                if (_uuidPointPair.ContainsKey(clientUUID))
                                {
                                    _uuidPointPair.TryGetValue(clientUUID, out IPEndPoint point);
                                    if (point != null)
                                    {
                                        ClientAbort(point);
                                    }
                                }

                                _controlConnection.Send(new byte[1] { ControlSrverCodes.A }); //отправляем серверу соглашение

                                bytes = _controlConnection.Receive(data);
                                if (bytes == 1 && data[0] == ControlSrverCodes.B) //сервер запрашивает мой порт
                                {
                                    byte[] portData;
                                    if (DirectConnection)
                                    {
                                        var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                        
                                        STUN_Result result = null;
                                        try
                                        {
                                            // TODO: сделать получения списка stun серверов с нашего сервера
                                            result = STUN_Client.Query("stun.l.google.com", 19305, udpSocket); //получем наш внешний адрес
                                            Runtime.DebugWrite("NatType " + result.NetType.ToString());
                                        }
                                        catch { }

                                        localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
                                        udpSocket.Close();

                                        // какая-то хуйня. Пробуем переподключиться к управляющему серверу
                                        if (result == null)
                                        {
                                            needRepeat = true;
                                            break;
                                        }

                                        //парсим порт
                                        string externalPort;
                                        if (result.PublicEndPoint != null)
                                        {
                                            myPoint = result.PublicEndPoint.ToString();
                                            externalPort = myPoint.Substring(myPoint.IndexOf(":") + 1, myPoint.Length - myPoint.IndexOf(":") - 1).Trim();
                                            portData = Encoding.UTF8.GetBytes(externalPort.ToString());
                                        }
                                        else // опять какая-то хуйня. Пробуем переподключиться к управляющему серверу
                                        {
                                            needRepeat = true;
                                            break;
                                        }

                                        Runtime.DebugWrite("My EndPoint " + result.PublicEndPoint.ToString());
                                    }
                                    else
                                    {
                                        portData = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                                    }

                                    _controlConnection.Send(portData); //отправляем серверу наш порт
                                }
                                else
                                {
                                    needRepeat = true;
                                    break;
                                }
                            }
                            else
                            {
                                needRepeat = true;
                                break;
                            }
                        }
                        {
                            byte[] data = new byte[50];
                            int data_lenght = _controlConnection.Receive(data); //получем ip клиента

                            bool isConected;
                            IPEndPoint point;
                            if (DirectConnection)
                            {
                                string str = Encoding.UTF8.GetString(data, 0, data_lenght);

                                using (SHA1 sha = new SHA1Managed())
                                {
                                    byte[] connectionCode;
                                    if (str.EndsWith(",proxy"))
                                    {
                                        point = new IPEndPoint(IPAddress.Parse("194.61.2.176"), 4719);
                                        Runtime.DebugWrite("Connection code: " + myPoint + ", " + str.Replace(",proxy", ""));
                                        connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(myPoint + ", " + str.Replace(",proxy", "")));
                                    }
                                    else
                                    {
                                        string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
                                        string hostIp = str.Replace(":" + hostPort, "");

                                        point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                                        Runtime.DebugWrite("Host EndPoint " + point);

                                        Runtime.DebugWrite("Connection code: " + myPoint + ", " + str);
                                        connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(myPoint + ", " + str));
                                    }

                                    isConected = ((SmpServer)Server).Connect(localPoint, point, connectionCode);
                                }
                            }
                            else
                            {
                                isConected = ((TurnBridgeServer)Server).Connect(UUID, clientUUID, out point);
                            }

                            AcceptingBlock.WaitOne();

                            if (isConected)
                            {
                                Runtime.DebugWrite("КОННЕКТ!!!");
                                if (BeforeConnect(point))
                                {
                                    _uuidPointPair[clientUUID] = point;
                                    _pointUuidPair[point] = clientUUID;

                                    try
                                    {
                                        ConnectingUser?.Invoke(clientUUID);
                                    }
                                    catch { }

                                    SendingWait.Set(); // если это первый клиент, то сейчас читающий поток будет запущен
                                    ReadingWait.Set();
                                }
                                else
                                {
                                    Runtime.DebugWrite("Пиздец");
                                    AcceptingBlock.Release();
                                }
                            }
                            else
                            {
                                Runtime.DebugWrite("Пиздец");
                                AcceptingBlock.Release();
                            }
                        }
                    }
                    catch { }
                }

                if (needRepeat)
                {
                    PrepeareRepeat();
                }
                else
                {
                    break;
                }
            }
        }

        public virtual void StopWork()
        {
            IsWork = false;

            AcceptingThread.Abort();
            MaintainingThread.Abort();
            try
            {
                _controlConnection.Send(new byte[1] { ControlSrverCodes.Z }); // отправляем управляющиму серверу сообщение что мы отключаемся
            }
            catch { }
            _controlConnection.Close(); //закрываем соединение с управляющим сервером

            SendingThread.Abort();
            ReadingThread.Abort();

            Server.StopWork();
        }

        public void KickClient(string uuid)
        {
            try
            {
                if (uuid != "bbab3c32222e4f08a8b291d1e9b9267c" && uuid != "0920b1809fb09e14c2e0526a94fb7c93")
                {
                    lock (KickedClients)
                    {
                        KickedClients.Add(uuid);
                    }

                    IPEndPoint point = _uuidPointPair[uuid];
                    ClientAbort(point);
                }
            }
            catch { }
        }

        public void UnkickClient(string uuid)
        {
            lock (KickedClients)
            {
                if (KickedClients.Contains(uuid))
                {
                    KickedClients.Remove(uuid);
                }
            }
        }

        private void MaintainingConnection()
        {
            try
            {
                Thread.Sleep(240000); // ждём 4 минуты

                while (IsWork)
                {
                    ControlConnectionBlock.WaitOne();
                    _controlConnection.Send(new byte[1] { ControlSrverCodes.Y });
                    ControlConnectionBlock.Set();
                    Thread.Sleep(240000); // ждём 4 минуты
                }
            }
            catch { }
        }

        protected virtual void ClientAbort(IPEndPoint point) // мeтод который вызывается при обрыве соединения
        {
            _pointUuidPair.TryRemove(point, out string clientUuid);
            _uuidPointPair.TryRemove(clientUuid, out _);

            try
            {
                if (clientUuid != null)
                {
                    ThreadPool.QueueUserWorkItem((object obj) =>
                    {
                        DisconnectedUser?.Invoke(clientUuid);
                    });
                }
            }
            catch { }
        }

        /// <summary>
        /// это метод который запускается после установления соединения
        /// </summary>
        protected virtual bool BeforeConnect(IPEndPoint point)
        {
            AcceptingBlock.Release();
            return true;
        }

        protected abstract void Sending(); // тут получаем данные от клиентов

        protected abstract void Reading(); // тут получаем данные из сети

    }
}
