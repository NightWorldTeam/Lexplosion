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

    abstract class NetworkClient // TODO: вложенные потоки нужно сделать нефоновыми. ну чтобы они давали программе закрыться
    {
        protected IClientTransmitter Bridge;
        protected string ClientType;
        protected string ControlServer;
        protected bool DirectConnection;

        protected Thread readingThread;
        protected Thread sendingThread;

        private IPEndPoint localPoint = new IPEndPoint(IPAddress.Any, 15323);

        public NetworkClient(string clientType, string controlServer)
        {
            ClientType = clientType;
            ControlServer = controlServer;
        }

        public virtual bool Initialization(string UUID, string serverUUID)
        {
            //подключаемся к управляющему серверу
            TcpClient client = new TcpClient();
            client.Connect(ControlServer, 4565);

            NetworkStream stream = client.GetStream();
            string st = "{\"UUID-server\" : \"" + serverUUID + "\", \"type\": \"" + ClientType + "\", \"UUID\": \"" + UUID + "\"}";
            byte[] sendData = Encoding.UTF8.GetBytes(st);
            stream.Write(sendData, 0, sendData.Length); //авторизируемся на управляющем сервере
            Console.WriteLine("ASZSAFDSDFAFSADSAFDFSDSD " + serverUUID);

            {
                byte[] buf = new byte[2];
                int bytes = stream.Read(buf, 0, buf.Length);
                Console.WriteLine("BUF-0 " + buf[0]);

                if (buf[0] == 98) // сервер согласился, а управляющий сервер запрашивает порт
                {
                    Console.WriteLine("BUF " + buf[1]);
                    byte[] portData;
                    if (buf[1] == 1) //Определяем по какому методу работает сервер. 1 - прямое подключение. 0 - через TURN
                    {
                        UdpClient sock = new UdpClient();
                        sock.Client.Bind(localPoint);
                        Bridge = new SmpClient(sock);

                        STUN_Result result = STUN_Client.Query("stun.zoiper.com", 3478, sock.Client);
                        Console.WriteLine("My EndPoint " + result.PublicEndPoint.ToString());

                        //парсим и получаем порт
                        string externalPort = result.PublicEndPoint.ToString();
                        externalPort = externalPort.Substring(externalPort.IndexOf(":") + 1, externalPort.Length - externalPort.IndexOf(":") - 1).Trim();
                        portData = Encoding.UTF8.GetBytes(externalPort);


                        DirectConnection = true;
                    }
                    else
                    {
                        Bridge = new TurnBridgeClient();
                        portData = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                        DirectConnection = false;
                    }

                    stream.Write(portData, 0, portData.Length); //отправяем управляющему серверу наш порт

                    Bridge.ClientClosing += Close;
                }
                else
                {
                    // TODO: либо управляющий сервер отъехал, либо сервер отказал
                    return false;
                }
            }

            byte[] data = new byte[21];
            byte[] resp;

            { // TODO: данные могут прийти не полностью, tcp их может разбить
                int bytes = stream.Read(data, 0, data.Length);
                resp = new byte[bytes];

                for (int i = 0; i < bytes; i++) //переносим массив
                {
                    resp[i] = data[i];
                }
            }

            bool isConected;

            if (DirectConnection)
            {
                string str = Encoding.UTF8.GetString(resp, 0, resp.Length);
                string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
                string hostIp = str.Replace(":" + hostPort, "");
                Console.WriteLine("Host EndPoint " + new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)));
                isConected = ((SmpClient)Bridge).Connect(new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)));
            }
            else
            {
                Console.WriteLine("FFHNHBGHJCMGCHM,VHJ,HJ,HJ");
                isConected = ((TurnBridgeClient)Bridge).Connect(UUID, serverUUID);
            }

            if (isConected) // TODO: было исключени о неверном формате строки
            {
                //Console.WriteLine("Ping " + Bridge.ping);
                stream.Close();
                client.Close();

                readingThread = new Thread(delegate ()
                {
                    Reading();
                });

                sendingThread = new Thread(delegate ()
                {
                    Sending();
                });

                sendingThread.Start();
                readingThread.Start();

                return true;
            }
            else
            {
                Console.WriteLine("пиздец");
                return false;
            }
        }

        public abstract void Close(IPEndPoint point);

        protected abstract void Sending();

        protected abstract void Reading();

    }
}
