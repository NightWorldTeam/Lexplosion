using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using LumiSoft.Net.STUN.Client;

namespace Lexplosion.Logic.Network
{
    using SMP;
    using TURN;

    abstract class NetworkClient // TODO: вложенные потоки нужно сделать нефоновыми. ну чтобы они давали программе закрыться
    {
        protected IClientTransmitter Bridge;
        protected string ClientType;
        protected string ControlServer;
        protected bool SmpConnection;

        protected Thread readingThread;
        protected Thread sendingThread;

        public NetworkClient(string clientType, string controlServer)
        {
            ClientType = clientType;
            ControlServer = controlServer;
        }

        public virtual bool Initialization(string UUID, string sessionToken, string serverUUID)
        {
            bool directConnectPossible = true; //описывает возможно ли прямое подключение через smp. Если оно не возможно и SmpConnection true, то трафик будет гнаться через Smp ретранслятор
            string myExternalPort = null;
            string myExternalIp = null;

            try
            {
                while (true)
                {
                    //подключаемся к управляющему серверу
                    TcpClient client = new TcpClient();
                    Runtime.DebugWrite("CONNECT Initialization");
                    client.Connect(ControlServer, 4565);

                    NetworkStream stream = client.GetStream();
                    string st = "{\"UUID-server\" : \"" + serverUUID + "\", \"type\": \"" + ClientType + "\", \"UUID\": \"" + UUID + "\", \"sessionToken\": \"" + sessionToken + "\"}";
                    byte[] sendData = Encoding.UTF8.GetBytes(st);
                    stream.Write(sendData, 0, sendData.Length); //авторизируемся на управляющем сервере
                    Runtime.DebugWrite("ASZSAFDSDFAFSADSAFDFSDSD " + serverUUID);

                    {
                        byte[] buf = new byte[2];
                        int bytes = stream.Read(buf, 0, buf.Length);

                        if (buf[0] == ControlSrverCodes.B) // сервер согласился, а управляющий сервер запрашивает порт
                        {
                            byte[] dataToSend;
                            if (buf[1] == 1) //Определяем по какому методу работает сервер. 1 - прямое подключение. 0 - через TURN
                            {
                                var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                udpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

                                if (directConnectPossible)
                                {
                                    STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, udpSocket);
                                    if (result == null)
                                    {
                                        // TODO: че-то делать
                                    }

                                    myExternalPort = result.PublicEndPoint.Port.ToString();
                                    myExternalIp = result.PublicEndPoint.Address.ToString();
                                    if (result.NetType == STUN_NetType.UdpBlocked || result.NetType == STUN_NetType.Symmetric || result.NetType == STUN_NetType.SymmetricUdpFirewall)
                                    {
                                        directConnectPossible = false;
                                        dataToSend = Encoding.UTF8.GetBytes(myExternalPort + ",proxy");
                                    }
                                    else
                                    {
                                        Runtime.DebugWrite("My EndPoint " + result.PublicEndPoint.ToString());
                                        Runtime.DebugWrite("Nat type " + result.NetType);

                                        dataToSend = Encoding.UTF8.GetBytes(myExternalPort);
                                    }                   
                                }
                                else
                                {
                                    string pt = myExternalPort ?? ((IPEndPoint)udpSocket.LocalEndPoint).Port.ToString();
                                    dataToSend = Encoding.UTF8.GetBytes(pt + ",proxy");
                                }

                                var point = (IPEndPoint)udpSocket.LocalEndPoint;
                                udpSocket.Close();
                                Bridge = new SmpClient(point);

                                SmpConnection = true;
                            }
                            else
                            {
                                Bridge = new TurnBridgeClient();
                                dataToSend = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                                SmpConnection = false;
                            }

                            stream.Write(dataToSend, 0, dataToSend.Length); //отправяем управляющему серверу наш порт

                            Bridge.ClientClosing += Close;
                        }
                        else
                        {
                            Runtime.DebugWrite("Bytes count: " + bytes + ", buf[0]=" + buf[0] + ", buf[1]=" + buf[1]);
                            return false;
                        }
                    }

                    byte[] data = new byte[50];
                    int data_lenght = stream.Read(data, 0, data.Length);

                    bool isConected;
                    if (SmpConnection)
                    {
                        try
                        {
                            string hostPointData = Encoding.UTF8.GetString(data, 0, data_lenght);

                            byte[] connectionCode;
                            string hostPort;
                            using (SHA1 sha = new SHA1Managed())
                            {
                                if (hostPointData.EndsWith(",proxy"))
                                {
                                    Runtime.DebugWrite("The server requires udp proxy");
                                    directConnectPossible = false;
                                    hostPointData = hostPointData.Replace(",proxy", "");
                                }

                                hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                                var strCode = serverUUID + "," + UUID + "," + hostPort + "," + myExternalPort;
                                connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(strCode));
                                Runtime.DebugWrite("Connection code: " + strCode);
                            }

                            IPEndPoint hostPoint;
                            if (directConnectPossible)
                            {
                                Runtime.DebugWrite("Udp direct connection");
                                string hostIp = hostPointData.Replace(":" + hostPort, "");
                                Runtime.DebugWrite("Host EndPoint " + new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)));

                                hostPoint = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                            }
                            else
                            {
                                Runtime.DebugWrite("UDP connect through proxy");
                                hostPoint = new IPEndPoint(IPAddress.Parse(ControlServer), 4719);
                            }

                            isConected = ((SmpClient)Bridge).Connect(hostPoint, connectionCode);
                        }
                        catch
                        {
                            isConected = false;
                        }
                    }
                    else
                    {
                        Runtime.DebugWrite("Tcp proxy");
                        isConected = ((TurnBridgeClient)Bridge).Connect(UUID, serverUUID, ControlServer);
                    }

                    stream.Close();
                    client.Close();

                    if (isConected)
                    {
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
                        // если наш метод подключения не Smp, или прямое подключение невозможно,
                        // то выходим из этого цикла и сигнализируем о неудаче. Иначе мы перейдем на вторую итерацию
                        if (!(SmpConnection && directConnectPossible))
                        {
                            Runtime.DebugWrite("пиздец");
                            return false;
                        }
                        else
                        {
                            // SmpConnection равен true. ставим флаг что прямое соединение невозможно и переходим на следующую итерацию
                            directConnectPossible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("NetworkClient Init exception " + ex);
                return false;
            }
        }

        protected abstract void Close(IPEndPoint point);

        protected abstract void Sending();

        protected abstract void Reading();

    }
}
