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
    using TURN;

    abstract class NetworkServer
    {
        protected Thread AcceptingThread;
        protected Thread ReadingThread;
        protected Thread SendingThread;
        private Thread MaintainingThread;

        protected Semaphore AcceptingBlock; //блокировка во время приёма подключения
        protected Semaphore SendingBlock; //блокировка во время работы метода Sending
        protected AutoResetEvent ControlConnectionBlock; // чтобы методы MaintainingConnection и Accepting одновременно не обраащлись к управляющему серверу

        protected AutoResetEvent SendingWait;
        protected AutoResetEvent ReadingWait;

        protected IServerTransmitter Server;

        protected bool IsWork = false;

        protected string UUID;
        protected string _sessionToken;
        protected bool DirectConnection;
        protected string ControlServer;

        private readonly IPEndPoint localPoint = new IPEndPoint(IPAddress.Any, 9654);

        private Socket controlConnection;

        public event Action<string> ConnectingUser;
        public event Action<string> DisconnectedUser;

        protected ConcurrentDictionary<string, IPEndPoint> UuidPointPair;
        protected ConcurrentDictionary<IPEndPoint, string> PointUuidPair;

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
            UuidPointPair = new ConcurrentDictionary<string, IPEndPoint>();
            PointUuidPair = new ConcurrentDictionary<IPEndPoint, string>();

            controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (DirectConnection)
            {
                Server = new SmpServer(localPoint);
            }
            else
            {
                Server = new TurnBridgeServer();
            }

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

        protected void Accepting(string serverType) // TODO: нужно избегать повторного подключения
        {
            while (true)
            {
                bool needRepeat = false;

                //подключаемся к управляющему серверу
                controlConnection.Connect(new IPEndPoint(IPAddress.Parse(ControlServer), 4565));

                string st =
                    "{\"UUID\" : \"" + UUID + "\", \"type\": \"" + serverType + "\", \"method\": \"" + (DirectConnection ? "STUN" : "TURN") + "\", \"sessionToken\" : \"" + _sessionToken + "\"}";
                byte[] sendData = Encoding.UTF8.GetBytes(st);
                controlConnection.Send(sendData); //авторизируемся на упрявляющем сервере
                MaintainingThread.Start();
                Console.WriteLine("ASZSAFDSDFAFSADSAFDFSDSD");

                while (IsWork)
                {
                    try
                    {
                        Console.WriteLine("BVC1");
                        string clientUUID;
                        {
                            byte[] data = new byte[33];

                            Console.WriteLine("ControlServerRecv");
                            ControlConnectionBlock.Set(); // освобождаем семафор переда как начать слушать сокет. Ждать мы на Receive можем долго
                            controlConnection.ReceiveTimeout = -1; // делаем бесконечное ожидание
                            int bytes = controlConnection.Receive(data); // TODO: в трай запихать
                            ControlConnectionBlock.WaitOne(); // блочим семофор
                            controlConnection.ReceiveTimeout = 10000; //огрниччиваем ожидание до 10 секунд
                            Console.WriteLine("ControlServerEndRecv");

                            if (bytes > 1 && data[0] == ControlSrverCodes.A) // data[0] == 97 значит поступил запрос на поделючение
                            {
                                clientUUID = Encoding.UTF8.GetString(data, 1, 32); // получаем UUID клиента

                                // этот клиент был кикнут. послыем его нахуй
                                lock (KickedClients)
                                {
                                    if (KickedClients.Contains(clientUUID))
                                    {
                                        controlConnection.Send(new byte[1] { ControlSrverCodes.E }); //отправляем серверу отказ
                                        continue;
                                    }
                                }

                                controlConnection.Send(new byte[1] { ControlSrverCodes.A }); //отправляем серверу соглашение

                                bytes = controlConnection.Receive(data);
                                if (bytes == 1 && data[0] == ControlSrverCodes.B) //сервер запрашивает мой порт
                                {
                                    byte[] portData;
                                    if (DirectConnection)
                                    {
                                        UdpClient sock = new UdpClient();
                                        sock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                        sock.Client.Bind(localPoint);

                                        // TODO: сделать получения списка stun серверов с нашего сервера
                                        STUN_Result result = null;
                                        try
                                        {
                                            result = STUN_Client.Query("stun.l.google.com", 19305, sock.Client); //получем наш внешний адрес
                                            Console.WriteLine("NatType " + result.NetType.ToString());
                                        }
                                        catch { }
                                        sock.Close();

                                        // какая-то хуйня. Преходим на соединение через ретранслятор
                                        if (result == null)
                                        {
                                            needRepeat = true;
                                            break;
                                        }

                                        //парсим порт
                                        string externalPort;
                                        if (result.PublicEndPoint != null)
                                        {
                                            externalPort = result.PublicEndPoint.ToString();
                                            externalPort = externalPort.Substring(externalPort.IndexOf(":") + 1, externalPort.Length - externalPort.IndexOf(":") - 1).Trim();
                                            portData = Encoding.UTF8.GetBytes(externalPort.ToString());
                                        }
                                        else // опять какая-то хуйня. Преходим на соединение через ретранслятор
                                        {
                                            needRepeat = true;
                                            break;
                                        }

                                        Console.WriteLine("My EndPoint " + result.PublicEndPoint.ToString());
                                    }
                                    else
                                    {
                                        portData = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                                    }

                                    controlConnection.Send(portData); //отправляем серверу наш порт
                                }
                                else
                                {
                                    // TODO: опять же произошло что-то с сервером
                                    continue;
                                }
                            }
                            else
                            {
                                // TODO: что-то странное произошло. сервер должен 1 байт вернуть и отправить запрос на подключени
                                continue;
                            }
                        }

                        Console.WriteLine("BVC2");

                        {
                            byte[] data = new byte[21];
                            int bytes = controlConnection.Receive(data); //получем ip клиента

                            byte[] resp = new byte[bytes];
                            for (int i = 0; i < bytes; i++) // TODO: сделать этот перенос нормально, но не через resize
                            {
                                resp[i] = data[i];
                            }

                            bool isConected;
                            IPEndPoint point;
                            if (DirectConnection)
                            {
                                string str = Encoding.UTF8.GetString(resp, 0, resp.Length);
                                string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
                                string hostIp = str.Replace(":" + hostPort, "");

                                //hostPort = "9655";
                                //hostIp = "127.0.0.1";

                                point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                                Console.WriteLine("Host EndPoint " + point);
                                isConected = ((SmpServer)Server).Connect(point);
                            }
                            else
                            {
                                Console.WriteLine("BVC3");
                                isConected = ((TurnBridgeServer)Server).Connect(UUID, clientUUID, out point);
                                Console.WriteLine("BVC4");
                            }

                            Console.WriteLine("BVC5");
                            AcceptingBlock.WaitOne();

                            if (isConected)
                            {
                                Console.WriteLine("КОННЕКТ!!!");
                                if (BeforeConnect(point))
                                {
                                    UuidPointPair[clientUUID] = point;
                                    PointUuidPair[point] = clientUUID;

                                    try
                                    {
                                        ConnectingUser?.Invoke(clientUUID);
                                    }
                                    catch { }
                                    Console.WriteLine("КОННЕКТ2!!!");
                                    SendingWait.Set(); // если это первый клиент, то сейчас читающий поток будет запущен
                                    ReadingWait.Set();
                                }
                                else
                                {
                                    Console.WriteLine("Пиздец");
                                    AcceptingBlock.Release();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Пиздец");
                                AcceptingBlock.Release();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                if (needRepeat)
                {
                    AcceptingBlock.Release();

                    ControlConnectionBlock.WaitOne();
                    controlConnection.Send(new byte[1] { ControlSrverCodes.Z });
                    controlConnection.Close();
                    controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    Console.WriteLine("Repeat connection to control server");
                    DirectConnection = false;
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
                controlConnection.Send(new byte[1] { ControlSrverCodes.Z }); // отправляем управляющиму серверу сообщение что мы отключаемся
            }
            catch { }
            controlConnection.Close(); //закрываем соединение с управляющим сервером

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
                    IPEndPoint point = UuidPointPair[uuid];
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
                    controlConnection.Send(new byte[1] { ControlSrverCodes.Y });
                    ControlConnectionBlock.Set();
                    Thread.Sleep(240000); // ждём 4 минуты
                }
            }
            catch { }
        }

        protected virtual void ClientAbort(IPEndPoint point) // мeтод который вызывается при обрыве соединения
        {
            PointUuidPair.TryRemove(point, out string clientUuid);
            UuidPointPair.TryRemove(clientUuid, out _);

            try
            {
                DisconnectedUser?.Invoke(clientUuid);
            }
            catch { }
        }

        protected abstract bool BeforeConnect(IPEndPoint point); // это метод который запускается после установления соединения

        protected abstract void Sending(); // тут получаем данные от клиентов

        protected abstract void Reading(); // тут получаем данные из сети

    }
}
