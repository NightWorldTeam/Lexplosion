using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        protected bool DirectConnection;

        protected Thread readingThread;
        protected Thread sendingThread;

        public NetworkClient(string clientType, string controlServer)
        {
            ClientType = clientType;
            ControlServer = controlServer;
        }

        public virtual bool Initialization(string UUID, string sessionToken, string serverUUID)
        {
            try
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
                        byte[] portData;
                        if (buf[1] == 1) //Определяем по какому методу работает сервер. 1 - прямое подключение. 0 - через TURN
                        {
                            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            udpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

                            STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, udpSocket);
                            Runtime.DebugWrite("My EndPoint " + result.PublicEndPoint.ToString());

                            var point = (IPEndPoint)udpSocket.LocalEndPoint;
                            udpSocket.Close();
                            Bridge = new SmpClient(point);

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
                    try
                    {
                        string str = Encoding.UTF8.GetString(resp, 0, resp.Length);
                        string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
                        string hostIp = str.Replace(":" + hostPort, "");

                        Runtime.DebugWrite("Host EndPoint " + new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)));
                        isConected = ((SmpClient)Bridge).Connect(new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort)), new byte[] {1,2,3,4});
                    }
                    catch
                    {
                        isConected = false;
                    }
                }
                else
                {
                    Runtime.DebugWrite("FFHNHBGHJCMGCHM,VHJ,HJ,HJ");
                    isConected = ((TurnBridgeClient)Bridge).Connect(UUID, serverUUID, ControlServer);
                }

                if (isConected)
                {
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
                    Runtime.DebugWrite("пиздец");
                    return false;
                }
            }
            catch
            {
                Runtime.DebugWrite("NetworkClient Init exception ");
                return false;
            }
        }

        protected abstract void Close(IPEndPoint point);

        protected abstract void Sending();

        protected abstract void Reading();

    }
}
