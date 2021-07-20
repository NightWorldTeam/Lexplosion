using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Network
{
    class Gateway
    {
        public bool isServer = false;
        public bool isClient = false;
        public AutoResetEvent waiting = new AutoResetEvent(false);

        private Thread WaitingOpenThread;
        private Thread WorkThread;

        public void Initialization(int pid)
        {
            WaitingOpenThread = new Thread(delegate ()
            {
                WaitingOpen(pid);
            });

            WaitingOpenThread.Start();

            WorkThread = new Thread(delegate ()
            {
                UdpClient client = new UdpClient();
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                client.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
                client.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

                bool successful = ListenGameSrvers(client, out string name, out int port, pid);

                if (!successful) // TODO: из всего алгоритма выходить не надо, надо только перевести всё в ручной режим
                {
                    return;
                }

                isServer = true;

                List<List<string>> input = new List<List<string>>();

                input.Add(new List<string>() { "UUID", "344a7f427fb765610ef96eb7bce95257" });
                input.Add(new List<string>() { "password", Сryptography.Sha256("1") });

                string ans = ToServer.HttpPost("https://night-world.org/libraries/scripts/setGameServer.php", input);

                ServerBridge server = new ServerBridge(port);

                client.Client.ReceiveTimeout = 3000;
                while (true)
                {
                    bool result = ListenGameSrvers(client, out string name_, out int port_, pid);
                    if (!result || name_ != name || port != port_) // если функция вернула false или изменилось имя или изменился порт - значит серер был закрыт
                    {
                        isServer = false;
                        // TODO: тут деинициализировать сервер
                        break;
                    }

                    Thread.Sleep(3000);
                }
            });

            WorkThread.Start();
        }

        public bool ListenGameSrvers(UdpClient client, out string name, out int port, int pid)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);

            name = "";
            port = -1;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                test:
                    byte[] data = client.Receive(ref ip);
                    if (isClient) //если работает клиент то ждем когда перестанет
                    {
                        waiting.WaitOne();
                        goto test; // TODO: думаю, можно сделать это без goto
                    }

                    List<ushort> processPorts = Utils.GetProcessUdpPorts(pid);

                    if (processPorts.Contains((ushort)ip.Port)) // проверяем принадлежит ли порт, с которого мы получили данные нужному нам процессу 
                    {
                        string strData = Encoding.ASCII.GetString(data);

                        if (strData.Substring(0, 6) == "[MOTD]" && strData.Substring(strData.Length - 5, 5) == "[/AD]")
                        {
                            string name_ = strData.Substring(6, strData.IndexOf("[/MOTD]") - 6);
                            int port_ = Int32.Parse(strData.Substring(strData.IndexOf("[AD]") + 4, strData.Length - strData.IndexOf("[/AD]") - 1));

                            name = name_;
                            port = port_;

                            return true;

                        }
                    }
                }
                catch { }
            }

            return false;
        }

        public void WaitingOpen(int pid)
        {
            ClientBridge bridge = new ClientBridge();

            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // TODO: MulticastTimeToLive
            client.Client.Ttl = 0; //это чтобы другие компьютеры в локальной сети не видели этого сервера
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4445));
            client.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"), IPAddress.Parse("127.0.0.1"));

            while (true)
            {
                List<ushort> processPorts = Utils.GetProcessUdpPorts(pid);
                if (processPorts.Contains(4445) && !isServer)
                {
                    List<List<string>> input = new List<List<string>>();

                    input.Add(new List<string>() { "UUID", "bbab3c32222e4f08a8b291d1e9b9267c" });
                    input.Add(new List<string>() { "password", Сryptography.Sha256("tipidor") });

                    string data = ToServer.HttpPost("https://night-world.org/libraries/scripts/getGameServers.php", input);
                    List<string> servers = null;
                    try
                    {
                        servers = JsonConvert.DeserializeObject<List<string>>(data);
                    }
                    catch { }

                    if (servers != null && servers.Count > 0)
                    {
                        isClient = true;

                        List<int> ports = bridge.SetServers(servers);

                        if (!bridge.IsInitialized)
                        {
                            bridge.StartEmulation("test");
                        }

                        //Отправляем пакеты сервера для отображения в локальных мирах
                        foreach (int port in ports)
                        {
                            byte[] _data = Encoding.UTF8.GetBytes("[MOTD]§3Епёта, твой друг Editor играет §l§n§o§m§k§kkizyak[/MOTD][AD]" + port + "[/AD]");
                            client.Send(_data, _data.Length, new IPEndPoint(IPAddress.Parse("224.0.2.60"), 4445));
                        }
                    }
                }

                Thread.Sleep(2000);
            }
        }

        public void Stop()
        {
            WaitingOpenThread.Abort();
            WorkThread.Abort();
        }

    }
}
