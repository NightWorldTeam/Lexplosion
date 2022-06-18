using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Network
{
    class Gateway
    {
        private Thread ServerSimulatorThread;
        private Thread ClientSimulatorThread;
        private Thread InformingThread;

        ServerBridge Server = null;
        string ControlServer = "";

        string UUID;
        string accessToken;

        public Gateway(string uuid, string accessToken_, string controlServer)
        {
            UUID = uuid;
            accessToken = accessToken_;
            ControlServer = controlServer;
        }

        public void Initialization(int pid)
        {
            ServerSimulatorThread = new Thread(delegate ()
            {
                ServerSimulator(pid);
            });

            ServerSimulatorThread.Start();

            ClientSimulatorThread = new Thread(delegate ()
            {
                ClientSimulator(pid);
            });

            ClientSimulatorThread.Start();
        }

        public bool ListenGameSrvers(UdpClient client, out string name, out int port, int pid)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);

            name = "";
            port = -1;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    byte[] data;
                    data = client.Receive(ref ip);

                    // TODO: ещё ник проверять
                    if (Utils.ContainsUdpPort(pid, ip.Port)) // проверяем принадлежит ли порт, с которого мы получили данные нужному нам процессу 
                    {
                        string strData = Encoding.ASCII.GetString(data);

                        if (strData.Substring(0, 6) == "[MOTD]" && strData.Substring(strData.Length - 5, 5) == "[/AD]")
                        {
                            string name_ = strData.Substring(6, strData.IndexOf("[/MOTD]") - 6);
                            int port_ = Int32.Parse(strData.Replace("[MOTD]" + name_ + "[/MOTD]", "").Replace("[/AD]", "").Replace("[AD]", ""));

                            name = name_;
                            port = port_;

                            return true;
                        }
                    }
                    else // пришел пакет от другого клиента, кторый с нами никак не связан
                    {
                        i--;
                        continue;
                    }
                }
                catch { }
            }

            return false;
        }

        // Симуляция майнкрафт клиента. То есть используется если наш макрафт является сервером
        public void ClientSimulator(int pid)
        {
            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            client.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
            client.JoinMulticastGroup(IPAddress.Parse("224.0.2.60")); // TODO: try пихнуть
            client.Client.ReceiveTimeout = -1; // убираем таймоут, чтобы этот метод мог ждать бесконечно

            while (true)
            {
                AutoResetEvent waitingInforming = new AutoResetEvent(false);

                bool successful = ListenGameSrvers(client, out string name, out int port, pid);

                if (!successful) // TODO: из всего алгоритма выходить не надо, надо только перевести всё в ручной режим
                {
                    return;
                }

                InformingThread = new Thread(delegate ()
                {
                    List<List<string>> input = new List<List<string>>
                    {
                        new List<string>() { "UUID", UUID },
                        new List<string>() { "accessToken", accessToken }
                    };

                    try
                    {
                        // раз в 2 минуты отправляем пакеты основному серверу информирующие о доступности нашего игровго сервера
                        do
                        {
                            string ans = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "setGameServer", input);
                            Console.WriteLine(ans);
                        }
                        while (!waitingInforming.WaitOne(120000));
                    }
                    finally
                    {
                        Task.Run(delegate ()
                        {
                            ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "dropGameServer", input);
                        });
                    }
                });

                InformingThread.Start();
                Server = new ServerBridge(UUID, accessToken, port, false, ControlServer);

                while (true)
                {
                    Console.WriteLine("GAME SERVER IS WORK");
                    // проверяем имеется ли этот порт. Если имеется - значит сервер запущен
                    if (!Utils.ContainsTcpPort(pid, port))
                    {
                        break;
                    }

                    Thread.Sleep(3000);
                }

                waitingInforming.Set(); // высвобождаем поток InformingThread чтобы он не ждал лишнее время
                Server.StopWork();
            }
        }

        struct OnlineUserInfo
        {
            public string login;
            public string gameClientName;
        }

        // Симуляция майнкрафт сервера. То есть используется если наш макрафт является клиентом
        public void ServerSimulator(int pid)
        {
            ClientBridge bridge = new ClientBridge(UUID, accessToken, ControlServer);

            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 0);
            client.Client.Ttl = 0; //это чтобы другие компьютеры в локальной сети не видели этого сервера
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            client.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"), IPAddress.Parse("127.0.0.1"));

            while (true)
            {
                if (Utils.ContainsUdpPort(pid, 4445))
                {
                    List<List<string>> input = new List<List<string>>
                    {
                        new List<string>() { "UUID", UUID },
                        new List<string>() { "accessToken", accessToken }
                    };

                    string data = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "getGameServers", input);
                    Dictionary<string, OnlineUserInfo> servers = null;
                    try
                    {
                        servers = JsonConvert.DeserializeObject<Dictionary<string, OnlineUserInfo>>(data);
                    }
                    catch { }

                    if (servers != null && servers.Count > 0)
                    {
                        Dictionary<string, int> ports = bridge.SetServers(new List<string>(servers.Keys));

                        //Отправляем пакеты сервера для отображения в локальных мирах
                        foreach (string uuid in ports.Keys)
                        {
                            string text = servers[uuid].login + " играет";
                            if (servers[uuid].gameClientName != null)
                            {
                                text += " в " + servers[uuid].gameClientName;
                            }
                            byte[] _data = Encoding.UTF8.GetBytes("[MOTD]§3" + text + "[/MOTD][AD]" + ports[uuid] + "[/AD]");
                            client.Send(_data, _data.Length, new IPEndPoint(IPAddress.Parse("224.0.2.60"), 4445));
                        }
                    }
                }

                Thread.Sleep(2000);
            }
        }

        public void StopWork()
        {
            try { ServerSimulatorThread.Abort(); } catch { }
            try { ClientSimulatorThread.Abort(); } catch { }
            try { if (InformingThread != null) InformingThread.Abort(); } catch { }

            if (Server != null)
            {
                Server.StopWork();
            }
        }
    }
}
