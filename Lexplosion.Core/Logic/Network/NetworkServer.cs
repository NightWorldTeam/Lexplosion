using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using LumiSoft.Net.STUN.Client;

namespace Lexplosion.Logic.Network
{
    using SMP;
    using TURN;

    abstract class NetworkServer
    {
        protected Thread AcceptingThread;
        protected Thread ReadingThread;
        protected Thread SendingThread;
        private Thread MaintainingThread;

        protected Semaphore AcceptingBlock = new Semaphore(1, 1); //блокировка во время приёма подключения
        protected ManualResetEvent ConnectionWait = new(false); // блокируется на время работы метода PerformConnect

        private AutoResetEvent _controlConnectionBlock = new AutoResetEvent(false); // чтобы методы MaintainingConnection и Accepting одновременно не обраащлись к управляющему серверу
        private ManualResetEvent _threadsStartWait = new(false);

        private Socket _controlConnection;
        protected IServerTransmitter Server;
        protected bool IsWork = false;

        protected string UUID;
        protected string _sessionToken;
        protected bool SmpConnection;
        protected ControlServerData ControlServer;

        public event Action<string> ConnectingUser;
        public event Action<string> DisconnectedUser;

        // тут хранится список клиентов. В одном соответсвие uuid и ip, в другом наоборот
        private ConcurrentDictionary<string, IPEndPoint> _uuidPointPair = new ConcurrentDictionary<string, IPEndPoint>();
        private ConcurrentDictionary<IPEndPoint, string> _pointUuidPair = new ConcurrentDictionary<IPEndPoint, string>();

        protected HashSet<string> KickedClients = new HashSet<string>(); //тут хранятся выкинутые клиенты

        public NetworkServer(string uuid, string sessionToken, string serverType, bool directConnection, ControlServerData controlServer)
        {
            UUID = uuid;
            _sessionToken = sessionToken;
            IsWork = true;
            ControlServer = controlServer;

            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            AcceptingThread = new Thread(delegate () //поток принимающий новые подключения
            {
                TransmitterPrepear(directConnection, serverType);
                _threadsStartWait.Set();
                Accepting(serverType);
            });

            //этот поток читает сообщения от клиента
            ReadingThread = new Thread(delegate ()
            {
                _threadsStartWait.WaitOne();
                Reading();
            });

            //этот поток отправляет сообщения клиенту
            SendingThread = new Thread(delegate ()
            {
                _threadsStartWait.WaitOne();
                Sending();
            });

            MaintainingThread = new Thread(MaintainingConnection); //поток отправляющий управляющему серверу пустые пакеты для поддержиния соединения
        }

        protected void StartThreads()
        {
            AcceptingThread.Start();
            SendingThread.Start();
            ReadingThread.Start();
        }

        protected static readonly (string, int)[] StunServers = new (string, int)[]
        {
            new ("stun.l.google.com", 19305),
            new ("79.174.92.100", 3478),
            new ("stun.webcalldirect.com", 3478)
        };

        protected (string, int) SelectedStunServer = StunServers[0];

        private void TransmitterPrepear(bool directConnectionIsPriority, string serverType)
        {
            // если стоит парметр установки прямого соединения, то проверяем, возможно ли его вообще установить. если нет - переходим на TURN
            if (directConnectionIsPriority)
            {
                STUN_Result result = null;
                foreach (var stunServ in StunServers)
                {
                    Runtime.DebugConsoleWrite("Check stun server: " + stunServ);
                    try
                    {
                        result = STUN_Client.Query(stunServ.Item1, stunServ.Item2, new IPEndPoint(IPAddress.Any, 0)); //получем наш внешний адрес
                        Runtime.DebugConsoleWrite("NatType " + result?.NetType.ToString());

                        if (result != null && result.NetType != STUN_NetType.UdpBlocked)
                        {
                            Runtime.DebugConsoleWrite("Selected stun server: " + stunServ);
                            SelectedStunServer = stunServ;
                            break;
                        }
                    }
                    catch { }
                }

                if (result != null && result.NetType != STUN_NetType.UdpBlocked && result.NetType != STUN_NetType.Symmetric && result.NetType != STUN_NetType.SymmetricUdpFirewall)
                {
                    SmpConnection = true;
                    Server = new SmpServer();
                }
                else
                {
                    SmpConnection = false;
                    Server = new TurnBridgeServer(UUID, serverType[0], ControlServer.TurnPoint);
                }
            }
            else
            {
                SmpConnection = false;
                Server = new TurnBridgeServer(UUID, serverType[0], ControlServer.TurnPoint);
            }

            Server.ClientClosing += ClientAbort;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrepeareRepeat()
        {
            try
            {
                MaintainingThread.Abort();
            }
            catch { }

            MaintainingThread = new Thread(MaintainingConnection);

            try
            {
                _controlConnection.Send(new byte[1] { ControlSrverCodes.Z });
            }
            catch { }
            finally
            {
                try { _controlConnection?.Close(); } catch { }
            }

            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Runtime.DebugConsoleWrite("Repeat connection to control server");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetConnection(string serverType)
        {
            for (int i = 0; i < 5; i++)
            {
                Runtime.DebugConsoleWrite("Сonnection attempt " + i);

                //подключаемся к управляющему серверу
                try
                {
                    _controlConnection.Connect(ControlServer.HandshakeServerPoint);
                }
                catch (Exception ex)
                {
                    //при ошибке ждем 30 секунд и пытаемся повторить
                    Runtime.DebugConsoleWrite("Сonnection to control server error: " + ex);
                    Thread.Sleep(10000);
                    Runtime.DebugConsoleWrite("Repeat connection");

                    try
                    {
                        _controlConnection.Close();
                    }
                    catch { }

                    _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    continue;
                }

                try
                {
                    var st =
                    "{\"UUID\" : \"" + UUID + "\"," +
                    " \"type\": \"" + serverType + "\"," +
                    " \"method\": \"" + (SmpConnection ? "STUN" : "TURN") + "\"," +
                    " \"sessionToken\" : \"" + _sessionToken + "\"}";

                    byte[] sendData = Encoding.UTF8.GetBytes(st);
                    _controlConnection.Send(sendData); //авторизируемся на упрявляющем сервере
                }
                catch (Exception ex)
                {
                    Runtime.DebugConsoleWrite("Сonnection to control server error: " + ex);
                    Thread.Sleep(3000);
                    PrepeareRepeat();
                    continue;
                }

                MaintainingThread.Start();
                Runtime.DebugConsoleWrite("ASZSAFDSDFAFSADSAFDFSDSD");

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PerformConnect(string clientUUID, string myPort, Socket udpSocket, string hostPointData, bool directConnectPossible)
        {
            bool isConected;
            IPEndPoint point = null;
            ConnectionWait.Reset();

            try
            {
                if (Server is SmpServer)
                {
                    Runtime.DebugConsoleWrite("Udp connection");

                    using (SHA1 sha = new SHA1Managed())
                    {
                        byte[] connectionCode;
                        string hostPort;
                        if (!directConnectPossible || hostPointData.EndsWith(",proxy"))
                        {
                            Runtime.DebugConsoleWrite("Udp proxy (" + directConnectPossible + ", " + hostPointData.EndsWith(",proxy") + ")");
                            point = ControlServer.SmpProxyPoint;
                            hostPointData = hostPointData.Replace(",proxy", "");
                            hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                        }
                        else
                        {
                            Runtime.DebugConsoleWrite("Udp direct connection");
                            hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                            string hostIp = hostPointData.Replace(":" + hostPort, "");
                            point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                        }

                        var strCode = UUID + "," + clientUUID + "," + myPort + "," + hostPort;
                        Runtime.DebugConsoleWrite("Connection code: " + strCode);
                        connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(strCode));

                        var localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
                        udpSocket.Close();
                        isConected = ((SmpServer)Server).Connect(localPoint, new ClientDesc(clientUUID, point), connectionCode);
                    }
                }
                else
                {
                    Runtime.DebugConsoleWrite("Tcp Proxy");
                    isConected = ((TurnBridgeServer)Server).Connect(clientUUID, out ClientDesc clientDesc);
                    point = clientDesc.Point;
                }
            }
            catch (Exception ex)
            {
                isConected = false;
                Runtime.DebugConsoleWrite("Connect exception " + ex);
            }

            AcceptingBlock.WaitOne();

            if (isConected)
            {
                Runtime.DebugConsoleWrite("КОННЕКТ!!!");
                if (AfterConnect(new ClientDesc(clientUUID, point)))
                {
                    _uuidPointPair[clientUUID] = point;
                    _pointUuidPair[point] = clientUUID;

                    try
                    {
                        ConnectingUser?.Invoke(clientUUID);
                    }
                    catch { }
                }
                else
                {
                    Runtime.DebugConsoleWrite("Пиздец");
                    AcceptingBlock.Release();
                }
            }
            else
            {
                Runtime.DebugConsoleWrite("Пиздец1");
                AcceptingBlock.Release();
            }

            ConnectionWait.Set();
        }

        protected void Accepting(string serverType)
        {
            // TODO: если управляющий есрерв откажет в подключении, то эта поябень будет его постоянно долбить запросами, пытаясь подключиться
            bool contolServerExists = true;
            while (IsWork && contolServerExists)
            {
                contolServerExists = SetConnection(serverType);
                bool needRepeat = false;

                while (IsWork && contolServerExists)
                {
                    try
                    {
                        string clientUUID;

                        byte[] data = new byte[33];

                        Runtime.DebugConsoleWrite("ControlServerRecv");
                        _controlConnectionBlock.Set(); // освобождаем семафор переда как начать слушать сокет. Ждать мы на Receive можем долго
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
                            _controlConnectionBlock.WaitOne(); // блочим семофор
                            _controlConnection.ReceiveTimeout = 10000; //огрниччиваем ожидание до 10 секунд
                            Runtime.DebugConsoleWrite("ControlServerEndRecv");
                        }

                        bool directConnectPossible = true;
                        string myPort;
                        Socket udpSocket = null;
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
                                    ClientAbort(new ClientDesc(clientUUID, point));
                                }
                            }

                            _controlConnection.Send(new byte[1] { ControlSrverCodes.A }); //отправляем серверу соглашение

                            bytes = _controlConnection.Receive(data);
                            if (bytes == 1 && data[0] == ControlSrverCodes.B) //сервер запрашивает мой порт
                            {
                                byte[] portData;
                                if (SmpConnection)
                                {
                                    udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                                    STUN_Result result = null;
                                    try
                                    {
                                        // TODO: сделать получения списка stun серверов с нашего сервера
                                        result = STUN_Client.Query(SelectedStunServer.Item1, SelectedStunServer.Item2, udpSocket); //получем наш внешний адрес
                                        Runtime.DebugConsoleWrite("NatType " + result.NetType.ToString());
                                    }
                                    catch { }

                                    //result = null;

                                    //парсим порт
                                    if (result?.PublicEndPoint != null)
                                    {
                                        myPort = result.PublicEndPoint.Port.ToString();
                                        portData = Encoding.UTF8.GetBytes(myPort.ToString());

                                        Runtime.DebugConsoleWrite("My EndPoint " + result.PublicEndPoint.ToString());
                                    }
                                    else // какая-то хуйня. будем устанавливать соединение через ретранслятор
                                    {
                                        Runtime.DebugConsoleWrite("My EndPoint error");
                                        var localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
                                        portData = Encoding.UTF8.GetBytes(localPoint.Port + ",proxy");
                                        myPort = localPoint.Port.ToString();
                                        directConnectPossible = false;
                                    }
                                }
                                else
                                {
                                    myPort = "";
                                    portData = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                                }

                                _controlConnection.Send(portData); //отправляем серверу наш порт
                            }
                            else
                            {
                                Runtime.DebugConsoleWrite("Repeat 1");
                                needRepeat = true;
                                break;
                            }
                        }
                        else
                        {
                            Runtime.DebugConsoleWrite("Repeat 2");
                            needRepeat = true;
                            break;
                        }

                        byte[] pointData = new byte[50];
                        int pointDataLen = _controlConnection.Receive(pointData); //получем ip клиента
                        string pointDataStr = Encoding.UTF8.GetString(pointData, 0, pointDataLen);
                        PerformConnect(clientUUID, myPort, udpSocket, pointDataStr, directConnectPossible);
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
                    ClientAbort(new ClientDesc(uuid, point));
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
                Thread.Sleep(120000); // ждём 2 минуты

                while (IsWork)
                {
                    _controlConnectionBlock.WaitOne();
                    _controlConnection.Send(new byte[1] { ControlSrverCodes.Y });
                    _controlConnectionBlock.Set();
                    Thread.Sleep(120000); // ждём 2 минуты
                }
            }
            catch { }
        }

        protected virtual void ClientAbort(ClientDesc clientData) // мeтод который вызывается при обрыве соединения
        {
            try
            {
                _pointUuidPair.TryRemove(clientData.Point, out string clientUuid);
                if (clientUuid != null) _uuidPointPair.TryRemove(clientUuid, out _);

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
        protected virtual bool AfterConnect(ClientDesc clientData)
        {
            AcceptingBlock.Release();
            return true;
        }

        protected abstract void Sending(); // тут получаем данные от клиентов

        protected abstract void Reading(); // тут получаем данные из сети

    }
}
