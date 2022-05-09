using LumiSoft.Net.STUN.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    using SMP;
    using TURN;

    abstract class NetworkServer // TODO: на стороне сервера проверять есть один у дркгого в списке друзей
    {
        protected Thread AcceptingThread;
        protected Thread ReadingThread;
        protected Thread SendingThread;

        protected Semaphore AcceptingBlock; //блокировка во время приёма подключения
        protected Semaphore SendingBlock; //блокировка во время работы метода Sending

        protected AutoResetEvent SendingWait;
        protected AutoResetEvent ReadingWait;

        protected IServerTransmitter Server;
        protected UdpClient udpClient = null;

        protected bool IsWork = false;

        protected string UUID;
        protected bool DirectConnection;
        protected string ControlServer;

        private Socket controlConnection;

        public NetworkServer(string uuid, string serverType, bool directConnection, string controlServer)
        {
            UUID = uuid;
            IsWork = true;
            ControlServer = controlServer;
            DirectConnection = directConnection;
            AcceptingBlock = new Semaphore(1, 1);
            SendingBlock = new Semaphore(1, 1);

            SendingWait = new AutoResetEvent(false);
            ReadingWait = new AutoResetEvent(false);

            if (DirectConnection)
            {
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Server = new SmpServer((IPEndPoint)udpClient.Client.LocalEndPoint);
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

            AcceptingThread.Start();
            SendingThread.Start();
            ReadingThread.Start();

        }

        protected void Accepting(string serverType) // TODO: нужно избегать повторного подключения
        {
            //подключаемся к управляющему серверу
            controlConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            controlConnection.Connect(new IPEndPoint(IPAddress.Parse(ControlServer), 4565));

            string st =
                "{\"UUID\" : \"" + UUID + "\", \"type\": \"" + serverType + "\", \"method\": \"" + (DirectConnection ? "STUN" : "TURN") + "\"}";
            byte[] sendData = Encoding.UTF8.GetBytes(st);
            controlConnection.Send(sendData); //авторизируемся на упрявляющем сервере
            Console.WriteLine("ASZSAFDSDFAFSADSAFDFSDSD");

            while (IsWork)
            {
                Console.WriteLine("BVC1");
                string clientUUID;

                {
                    byte[] data = new byte[33];

                    Console.WriteLine("ControlServerRecv");
                    int bytes = controlConnection.Receive(data);
                    Console.WriteLine("ControlServerEndRecv");

                    if (bytes > 1 && data[0] == 97) // data[0] == 97 значит поступил запрос на поделючение
                    {
                        clientUUID = Encoding.UTF8.GetString(data, 1, 32); // получаем UUID клиента
                        controlConnection.Send(new byte[1] { 97 }); //отправляем серверу соглашение

                        bytes = controlConnection.Receive(data);
                        if (bytes == 1 && data[0] == 98) //сервер запрашивает мой порт
                        {
                            byte[] portData;
                            if (DirectConnection)
                            {
                                // TODO: сделать получения списка stun серверов с нашего сервера
                                STUN_Result result = STUN_Client.Query("stun.l.google.com", 19305, udpClient.Client); //получем наш внешний адрес

                                //парсим порт
                                string externalPort = result.PublicEndPoint.ToString(); // TODO: был нулл поинтер
                                externalPort = externalPort.Substring(externalPort.IndexOf(":") + 1, externalPort.Length - externalPort.IndexOf(":") - 1).Trim();
                                portData = Encoding.UTF8.GetBytes(externalPort.ToString());

                                Console.WriteLine("My EndPoint " + result.PublicEndPoint.ToString() + " " + result.NetType);
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

                        /*hostPort = "9655";
                        hostIp = "127.0.0.1";*/

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
        }

        public virtual void StopWork()
        {
            IsWork = false;

            AcceptingThread.Abort();
            controlConnection.Send(new byte[1] { 122 }); // отправляем управляющиму серверу сообщение что мы отключаемся
            controlConnection.Close(); //закрываем соединение с управляющим сервером

            SendingThread.Abort();
            ReadingThread.Abort();

            Server.StopWork();

            if (udpClient != null)
            {
                udpClient.Close();
            }
        }

        protected abstract void ClientAbort(IPEndPoint point); // мeтод который вызывается при обрыве соединения

        protected abstract bool BeforeConnect(IPEndPoint point); // это метод который запускается после установления соединения

        protected abstract void Sending(); // тут получаем данные от клиентов

        protected abstract void Reading(); // тут получаем данные из сети

    }
}
