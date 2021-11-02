using LumiSoft.Net.STUN.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    using SMP;

    abstract class NetworkClient // TODO: вложенные потоки нужно сделать нефоновыми. ну чтобы они давали программе закрыться
    {
        protected SmpClient Bridge;
        protected string ClientType;

        public NetworkClient(string clientType)
        {
            ClientType = clientType;
        }

        public virtual void Initialization(string UUID, string serverUUID)
        {
            UdpClient bridgeUdp = new UdpClient(9655);
            Bridge = new SmpClient(bridgeUdp);
            Bridge.ClientClosing += Close;

            //подключаемся к управляющему серверу
            TcpClient client = new TcpClient();
            client.Connect("194.61.2.176", 4565);

            NetworkStream stream = client.GetStream();
            string st = "{\"UUID-server\" : \"" + serverUUID + "\", \"type\": \"" + ClientType + "\", \"UUID\": \"" + UUID + "\"}";
            byte[] sendData = Encoding.UTF8.GetBytes(st);
            stream.Write(sendData, 0, sendData.Length); //авторизируемся на управляющем сервере
            Console.WriteLine("ASZSAFDSDFAFSADSAFDFSDSD " + serverUUID);

            {
                byte[] buf = new byte[1];
                int bytes = stream.Read(buf, 0, buf.Length);

                if (buf[0] == 98) // сервер согласился, а управляющий сервер запрашивает порт
                {
                    STUN_Result result = STUN_Client.Query("64.233.163.127", 19305, bridgeUdp.Client); //получем наш внешний адрес
                    Console.WriteLine(result.NetType);

                    //парсим и получаем порт
                    string externalPort = result.PublicEndPoint.ToString();
                    externalPort = externalPort.Substring(externalPort.IndexOf(":") + 1, externalPort.Length - externalPort.IndexOf(":") - 1).Trim();
                    byte[] portData = Encoding.UTF8.GetBytes(externalPort);

                    stream.Write(portData, 0, portData.Length); //отправяем управляющему серверу наш порт
                }
                else
                {
                    // TODO: либо управляющий сервер отъехал, либо сервер отказал
                }
            }

            byte[] data = new byte[21];
            byte[] resp;

            {
                int bytes = stream.Read(data, 0, data.Length);
                resp = new byte[bytes];

                for (int i = 0; i < bytes; i++) //переносим массив
                {
                    resp[i] = data[i];
                }
            }

            string str = Encoding.UTF8.GetString(resp, 0, resp.Length);
            string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
            string hostIp = str.Replace(":" + hostPort, "");

            if (Bridge.Connect(Int32.Parse(hostPort), hostIp)) // TODO: было исключени о неверном формате строки
            {
                Console.WriteLine("Ping " + Bridge.ping);
                stream.Close();
                client.Close();

                Thread readingThread = new Thread(delegate ()
                {
                    Reading();
                });

                Thread sendingThread = new Thread(delegate ()
                {
                    Sending();
                });

                sendingThread.Start();
                readingThread.Start();
            }
            else
            {
                Console.WriteLine("пиздец");
            }
        }

        public virtual void Close(IPEndPoint point) { }

        protected virtual void Sending() { }

        protected virtual void Reading() { }

    }
}
