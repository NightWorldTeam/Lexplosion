using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using Lexplosion.Global;
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
        private AutoResetEvent waitingInforming = new AutoResetEvent(false);

        private Thread WaitingOpenThread;
        private Thread WorkThread;
        private Thread InformingThread;

        ServerBridge Server = null;

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

                while (true)
                {
                    client.Client.ReceiveTimeout = -1;
                    bool successful = ListenGameSrvers(client, out string name, out int port, pid);

                    if (!successful) // TODO: из всего алгоритма выходить не надо, надо только перевести всё в ручной режим
                    {
                        return;
                    }

                    isServer = true;

                    InformingThread = new Thread(delegate ()
                    {
                        List<List<string>> input = new List<List<string>>();

                        input.Add(new List<string>() { "UUID", UserData.UUID });
                        input.Add(new List<string>() { "password", UserData.PaswordSHA });

                        //раз в 2 минуты отправляем пакеты основному серверу информирующие о доступности нашего игровго сервера
                        while (isServer)
                        {
                            ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "setGameServer.php", input);
                            waitingInforming.WaitOne(120000);
                        }

                        ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "dropGameServer.php", input);
                    });

                    InformingThread.Start();
                    Server = new ServerBridge(UserData.UUID, port);

                    client.Client.ReceiveTimeout = 3000;
                    while (true)
                    {
                        bool result = ListenGameSrvers(client, out string name_, out int port_, pid);
                        if (!result || name_ != name || port != port_) // если функция вернула false или изменилось имя или изменился порт - значит серер был закрыт
                        {
                            isServer = false;
                            waitingInforming.Set(); // вызвобождаем поток InformingThread чтобы он не ждал лишнее время
                            Server.StopWork();
                            break;
                        }

                        Thread.Sleep(3000);
                    }
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
                    byte[] data;
                    while (true)
                    {
                        data = client.Receive(ref ip);
                        if (isClient) //если работает клиент то ждем когда перестанет
                        {
                            waiting.WaitOne();
                        }
                        else
                        {
                            break;
                        }
                    }

                    List<ushort> processPorts = Utils.GetProcessUdpPorts(pid);

                    if (processPorts.Contains((ushort)ip.Port)) // проверяем принадлежит ли порт, с которого мы получили данные нужному нам процессу 
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
                }
                catch { }
            }

            return false;
        }

        public void WaitingOpen(int pid)
        {
            ClientBridge bridge = new ClientBridge(UserData.UUID);

            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            // TODO: MulticastTimeToLive
            client.Client.Ttl = 0; //это чтобы другие компьютеры в локальной сети не видели этого сервера
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
            client.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"), IPAddress.Parse("127.0.0.1"));

            while (true)
            {
                List<ushort> processPorts = Utils.GetProcessUdpPorts(pid);
                if (processPorts.Contains(4445) && !isServer)
                {
                    List<List<string>> input = new List<List<string>>();
                    input.Add(new List<string>() { "UUID", UserData.UUID });
                    input.Add(new List<string>() { "password", UserData.PaswordSHA });

                    string data = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "getGameServers.php", input);
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

        public void StopWork()
        {
            isServer = false;

            WaitingOpenThread.Abort();
            WorkThread.Abort();

            if (Server != null)
            {
                waitingInforming.Set();
                Server.StopWork();
            }
        }

    }
}
