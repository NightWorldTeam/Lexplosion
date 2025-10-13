using LumiSoft.Net.STUN.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        protected ControlServerData ControlServer;
        protected bool SmpConnection;

        protected Thread readingThread;
        protected Thread sendingThread;

        protected (string, int) SelectedStunServer;
        protected (string, int)[] StunServers;

        public NetworkClient(string clientType, ControlServerData controlServer)
        {
            ClientType = clientType;
            ControlServer = controlServer;

            StunServers = controlServer.StunServers;
            SelectedStunServer = controlServer.StunServers[0];
        }

        public virtual bool Initialization(string UUID, string sessionToken, string serverUUID)
        {
            bool directConnectPossible = true; //описывает возможно ли прямое подключение через smp. Если оно не возможно и SmpConnection true, то трафик будет гнаться через Smp ретранслятор
            string myExternalPort = null;

            try
            {
                while (true)
                {
                    //подключаемся к управляющему серверу
                    TcpClient client = new TcpClient();
                    Runtime.DebugConsoleWrite("CONNECT Initialization");
                    client.Connect(ControlServer.HandshakeServerPoint);

                    NetworkStream stream = client.GetStream();
                    var st = "{\"UUID-server\" : \"" + serverUUID + "\", \"type\": \"" + ClientType + "\", \"UUID\": \"" + UUID + "\", \"sessionToken\": \"" + sessionToken + "\"}";
                    byte[] sendData = Encoding.UTF8.GetBytes(st);
                    stream.Write(sendData, 0, sendData.Length); //авторизируемся на управляющем сервере
                    Runtime.DebugConsoleWrite("Server uuid: " + serverUUID);

                    {
                        byte[] buf = new byte[2];
                        int bytes = stream.Read(buf, 0, buf.Length);
                        Runtime.DebugConsoleWrite("Data recieved");

                        if (buf[0] == ControlSrverCodes.B) // сервер согласился, а управляющий сервер запрашивает порт
                        {
                            Runtime.DebugConsoleWrite("ControlSrverCodes.B");
                            byte[] dataToSend;
                            if (buf[1] == 1) //Определяем по какому методу работает сервер. 1 - прямое подключение. 0 - через TURN
                            {
                                var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                udpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

                                if (directConnectPossible)
                                {
                                    STUN_Result result = StunQuery(udpSocket);
                                    if (result?.PublicEndPoint != null)
                                    {
                                        myExternalPort = result.PublicEndPoint.Port.ToString();
                                        if (result.NetType == STUN_NetType.UdpBlocked || result.NetType == STUN_NetType.Symmetric || result.NetType == STUN_NetType.SymmetricUdpFirewall)
                                        {
                                            directConnectPossible = false;
                                            dataToSend = Encoding.UTF8.GetBytes(myExternalPort + ",proxy");
                                            Runtime.DebugConsoleWrite("Nat type " + result.NetType);
                                        }
                                        else
                                        {
                                            Runtime.DebugConsoleWrite("My EndPoint " + result.PublicEndPoint.ToString() + " Nat type " + result.NetType);
                                            dataToSend = Encoding.UTF8.GetBytes(myExternalPort);
                                        }
                                    }
                                    else
                                    {
                                        directConnectPossible = false;
                                        myExternalPort = ((IPEndPoint)udpSocket.LocalEndPoint).Port.ToString(); // в этом случае он нихуя не external
                                        dataToSend = Encoding.UTF8.GetBytes(myExternalPort + ",proxy");
                                        Runtime.DebugConsoleWrite("STUN_Result is null");
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
                                Bridge = new TurnBridgeClient(UUID, ClientType[0], ControlServer.TurnPoint);
                                dataToSend = Encoding.UTF8.GetBytes(" "); // если мы работает с TURN, то нам поебать на порт. Отправляем простой пробел
                                SmpConnection = false;
                            }

                            Runtime.DebugConsoleWrite("Send port to server");

                            stream.Write(dataToSend, 0, dataToSend.Length); //отправяем управляющему серверу наш порт

                            Bridge.ClientClosing += Close;
                        }
                        else
                        {
                            Runtime.DebugConsoleWrite("Bytes count: " + bytes + ", buf[0]=" + buf[0] + ", buf[1]=" + buf[1]);
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
                                    Runtime.DebugConsoleWrite("The server requires udp proxy");
                                    directConnectPossible = false;
                                    hostPointData = hostPointData.Replace(",proxy", "");
                                }

                                hostPort = hostPointData.Substring(hostPointData.IndexOf(":") + 1, hostPointData.Length - hostPointData.IndexOf(":") - 1).Trim();
                                var strCode = serverUUID + "," + UUID + "," + hostPort + "," + myExternalPort;
                                connectionCode = sha.ComputeHash(Encoding.UTF8.GetBytes(strCode));
                                Runtime.DebugConsoleWrite("Connection code: " + strCode);
                            }

                            IPEndPoint hostPoint;
                            if (directConnectPossible)
                            {
                                Runtime.DebugConsoleWrite("Udp direct connection");
                                string hostIp = hostPointData.Replace(":" + hostPort, "");
                                Runtime.DebugConsoleWrite("Host EndPoint " + new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)));

                                hostPoint = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));
                            }
                            else
                            {
                                Runtime.DebugConsoleWrite("UDP connect through proxy");
                                hostPoint = ControlServer.SmpProxyPoint;
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
                        Runtime.DebugConsoleWrite("Tcp proxy");
                        isConected = ((TurnBridgeClient)Bridge).Connect(serverUUID);
                    }

                    stream.Close();
                    client.Close();

                    if (isConected)
                    {
                        readingThread = new Thread(Reading);
                        sendingThread = new Thread(Sending);

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
                            Runtime.DebugConsoleWrite("пиздец");
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
                Runtime.DebugConsoleWrite("NetworkClient Init exception " + ex);
                return false;
            }
        }

        private STUN_Result StunQuery(Socket udpSocket)
        {
            STUN_Result result = null;
            foreach (var stunServ in StunServers)
            {
                Runtime.DebugConsoleWrite("Check stun server: " + stunServ);
                try
                {
                    result = STUN_Client.Query(stunServ.Item1, stunServ.Item2, udpSocket); //получем наш внешний адрес
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

            return result;
        }

        protected abstract void Close(IPEndPoint point);

        protected abstract void Sending();

        protected abstract void Reading();

    }
}
