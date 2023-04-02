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
        protected Semaphore SendingBlock = new Semaphore(1, 1); //блокировка во время работы метода Sending
        private AutoResetEvent _controlConnectionBlock = new AutoResetEvent(false); // чтобы методы MaintainingConnection и Accepting одновременно не обраащлись к управляющему серверу

        protected IServerTransmitter Server;

        protected bool IsWork = false;

        protected string UUID;
        protected string _sessionToken;
        protected bool DirectConnection;
        protected string ControlServer;

        private Socket _controlConnection;

        public event Action<string> ConnectingUser;
        public event Action<string> DisconnectedUser;

        // тут хранится список клиентов. В одном соответсвие uuid и ip, в другом наоборот
        private ConcurrentDictionary<string, IPEndPoint> _uuidPointPair = new ConcurrentDictionary<string, IPEndPoint>();
        private ConcurrentDictionary<IPEndPoint, string> _pointUuidPair = new ConcurrentDictionary<IPEndPoint, string>();

        protected HashSet<string> KickedClients = new HashSet<string>(); //тут хранятся выкинутые клиенты

        public NetworkServer(string uuid, string sessionToken, string serverType, bool directConnection, string controlServer)
        {
            UUID = uuid;
            _sessionToken = sessionToken;
            IsWork = true;
            ControlServer = controlServer;
            DirectConnection = directConnection;

            _controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Server = DirectConnection ? new SmpServer() : new TurnBridgeServer();
            Server.ClientClosing += ClientAbort;

            ReadingThread = new Thread(Reading); //этот поток читает сообщения от клиента
            SendingThread = new Thread(Sending); //этот поток отправляет сообщения клиенту

            AcceptingThread = new Thread(delegate () //поток принимающий новые подключения
            {
                Accepting(serverType);
            });

            MaintainingThread = new Thread(MaintainingConnection); //поток отправляющий управляющему серверу пустые пакеты для поддержиния соединения
        }

        protected void StartThreads()
        {
            AcceptingThread.Start();
            SendingThread.Start();
            ReadingThread.Start();
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

            Runtime.DebugWrite("Repeat connection to control server");
            DirectConnection = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetConnection(string serverType)
        {
            for (int i = 0; i < 5; i++)
            {
                Runtime.DebugWrite("Сonnection attempt " + i);

                //подключаемся к управляющему серверу
                try
                {
                    _controlConnection.Connect(new IPEndPoint(IPAddress.Parse(ControlServer), 4565));
                }
                catch (Exception ex)
                {
                    //при ошибке ждем 30 секунд и пытаемся повторить
                    Runtime.DebugWrite("Сonnection to control server error: " + ex);
                    Thread.Sleep(10000);
                    Runtime.DebugWrite("Repeat connection");

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
                    " \"method\": \"" + (DirectConnection ? "STUN" : "TURN") + "\"," +
                    " \"sessionToken\" : \"" + _sessionToken + "\"}";

                    byte[] sendData = Encoding.UTF8.GetBytes(st);
                    _controlConnection.Send(sendData); //авторизируемся на упрявляющем сервере
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Сonnection to control server error: " + ex);
                    Thread.Sleep(3000);
                    PrepeareRepeat();
                    continue;
                }

                MaintainingThread.Start();
                Runtime.DebugWrite("ASZSAFDSDFAFSADSAFDFSDSD");

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
                if (DirectConnection)
                {
                    Runtime.DebugWrite("Udp connection");

                    using (SHA1 sha = new SHA1Managed())
                    {
                        byte[] connectionCode;
                        string hostPort;
                        if (!directConnectPossible || hostPointData.EndsWith(",proxy"))
                        {
                            Runtime.DebugWrite("Udp proxy (" + directConnectPossible + ", " + hostPointData.EndsWith(",proxy") + ")");
                            point = new IPEndPoint(IPAddress.Parse(ControlServer), 4719);
                            hostPointData = hostPointData.Replace(",proxy", "");
                            hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                        }
                        else
                        {
                            Runtime.DebugWrite("Udp direct connection");
                            hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                            string hostIp = hostPointData.Replace(":" + hostPort, "");
                            point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                        }

                        var strCode = UUID + "," + clientUUID + "," + myPort + "," + hostPort;
                        Runtime.DebugWrite("Connection code: " + strCode);
                        connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(strCode));

                        var localPoint = (IPEndPoint)udpSocket.LocalEndPoint;
                        udpSocket.Close();
                        isConected = ((SmpServer)Server).Connect(localPoint, point, connectionCode);
                    }
                }
                else
                {
                    Runtime.DebugWrite("Tcp Proxy");
                    isConected = ((TurnBridgeServer)Server).Connect(UUID, clientUUID, out point);
                }
            }
            catch (Exception ex)
            {
                isConected = false;
                Runtime.DebugWrite("Connect exception " + ex);
            }

            AcceptingBlock.WaitOne();

            if (isConected)
            {
                Runtime.DebugWrite("КОННЕКТ!!!");
                if (AfterConnect(point))
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
                    Runtime.DebugWrite("Пиздец");
                    AcceptingBlock.Release();
                }
            }
            else
            {
                Runtime.DebugWrite("Пиздец1");
                AcceptingBlock.Release();
            }

            ConnectionWait.Set();
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

                        Runtime.DebugWrite("ControlServerRecv");
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
                            Runtime.DebugWrite("ControlServerEndRecv");
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
                                    udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                                    STUN_Result result = null;
                                    try
                                    {
                                        // TODO: сделать получения списка stun серверов с нашего сервера
                                        result = STUN_Client.Query("stun.l.google.com", 19305, udpSocket); //получем наш внешний адрес
                                        Runtime.DebugWrite("NatType " + result.NetType.ToString());
                                    }
                                    catch { }

                                    //парсим порт
                                    if (result?.PublicEndPoint != null)
                                    {
                                        myPort = result.PublicEndPoint.Port.ToString();
                                        portData = Encoding.UTF8.GetBytes(myPort.ToString());

                                        Runtime.DebugWrite("My EndPoint " + result.PublicEndPoint.ToString());
                                    }
                                    else // какая-то хуйня. будем устанавливать соединение через ретранслятор
                                    {
                                        Runtime.DebugWrite("My EndPoint error");
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
                                Runtime.DebugWrite("Repet 1");
                                needRepeat = true;
                                break;
                            }
                        }
                        else
                        {
                            Runtime.DebugWrite("Repet 2");
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

        protected virtual void ClientAbort(IPEndPoint point) // мeтод который вызывается при обрыве соединения
        {
            try
            {
                _pointUuidPair.TryRemove(point, out string clientUuid);
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
        protected virtual bool AfterConnect(IPEndPoint point)
        {
            AcceptingBlock.Release();
            return true;
        }

        protected abstract void Sending(); // тут получаем данные от клиентов

        protected abstract void Reading(); // тут получаем данные из сети

    }
}
