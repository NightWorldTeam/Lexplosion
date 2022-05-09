using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Network
{
    class Gateway
    {
        public bool isServer = false;
        public bool isClient = false;
        public ManualResetEvent clientModeWaiting = new ManualResetEvent(false);
        private AutoResetEvent waitingInforming = new AutoResetEvent(false);

        private Thread ServerSimulatorThread;
        private Thread ClientSimulatorThread;
        private Thread InformingThread;

        ServerBridge _serverBridge = null;
        ClientBridge _clientBridge = null;
        string ControlServer = "";

        string UUID;
        string accessToken;

        private UdpClient _serverSimulatorUdp;
        private UdpClient _clientSimulatorUdp;

        private bool _isWork = true;

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

        public bool ListenGameSrvers(UdpClient client, out string name, out int port, int pid, bool endlesswaiting)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);

            name = "";
            port = -1;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    byte[] data = null;
                    while (_isWork)
                    {
                        data = client.Receive(ref ip);
                        if (isClient) //если работает клиент то ждем когда перестанет
                        {
                            clientModeWaiting.WaitOne();
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
                    else // пришел пакет от другого клиента, кторый с нами никак не связан
                    {
                        // если это бесконечное ожидание, то мы просто оступаем на шаг назад, чтобы цикл не закночился
                        if (endlesswaiting)
                        {
                            i--;
                            continue;
                        }
                        else
                        {
                            Thread.Sleep(3000); // если оно не бесконечное, засыпаем на время таймаута
                        }

                    }
                }
                catch { }
            }

            return false;
        }

        // Симуляция майнкрафт клиента. То есть используется если наш макрафт является сервером
        public void ClientSimulator(int pid)
        {
            _clientSimulatorUdp = new UdpClient();
            _clientSimulatorUdp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _clientSimulatorUdp.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
            _clientSimulatorUdp.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

            while (_isWork)
            {
                _clientSimulatorUdp.Client.ReceiveTimeout = -1; // убираем таймоут, чтобы этот метод мог ждать бесконечно
                bool successful = ListenGameSrvers(_clientSimulatorUdp, out string name, out int port, pid, true);

                if (!successful) // TODO: из всего алгоритма выходить не надо, надо только перевести всё в ручной режим
                {
                    return;
                }

                isServer = true;

                InformingThread = new Thread(delegate ()
                {
                    List<List<string>> input = new List<List<string>>();

                    input.Add(new List<string>() { "UUID", UUID });
                    input.Add(new List<string>() { "accessToken", accessToken });

                    // раз в 2 минуты отправляем пакеты основному серверу информирующие о доступности нашего игровго сервера
                    while (isServer)
                    {
                        string ans = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "setGameServer.php", input);
                        Console.WriteLine(ans);
                        waitingInforming.WaitOne(120000);
                    }

                    ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "dropGameServer.php", input);
                });

                InformingThread.Start();
                _serverBridge = new ServerBridge(UUID, port, false, ControlServer);

                _clientSimulatorUdp.Client.ReceiveTimeout = 3000; // ставим таймаут, чтобы если пакетов небыло, ListenGameSrvers вернул false
                while (_isWork)
                {
                    bool result = ListenGameSrvers(_clientSimulatorUdp, out string name_, out int port_, pid, false);
                    if (!result || name_ != name || port != port_) // если функция вернула false или изменилось имя или изменился порт - значит серер был закрыт
                    {
                        isServer = false;
                        waitingInforming.Set(); // высвобождаем поток InformingThread чтобы он не ждал лишнее время
                        _serverBridge.StopWork();
                        break;
                    }

                    Thread.Sleep(3000);
                }
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
            _clientBridge = new ClientBridge(UUID, ControlServer);

            _serverSimulatorUdp = new UdpClient();
            _serverSimulatorUdp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _serverSimulatorUdp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 0);
            _serverSimulatorUdp.Client.Ttl = 0; //это чтобы другие компьютеры в локальной сети не видели этого сервера
            _serverSimulatorUdp.Client.Bind(new IPEndPoint(IPAddress.Any, 4445));
            _serverSimulatorUdp.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"), IPAddress.Parse("127.0.0.1"));

            while (true)
            {
                List<ushort> processPorts = Utils.GetProcessUdpPorts(pid);
                bool portContains = processPorts.Contains(4445);
                if (portContains && !isServer)
                {
                    List<List<string>> input = new List<List<string>>
                    {
                        new List<string>() { "UUID", UUID },
                        new List<string>() { "accessToken", accessToken }
                    };

                    string data = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "getGameServers.php", input);
                    Dictionary<string, OnlineUserInfo> servers = null;
                    try
                    {
                        servers = JsonConvert.DeserializeObject<Dictionary<string, OnlineUserInfo>>(data);
                    }
                    catch { }

                    if (servers != null && servers.Count > 0)
                    {
                        isClient = true;
                        clientModeWaiting.Reset();
                        Dictionary<string, int> ports = _clientBridge.SetServers(new List<string>(servers.Keys));

                        //Отправляем пакеты сервера для отображения в локальных мирах
                        foreach (string uuid in ports.Keys)
                        {
                            string text = servers[uuid].login + " играет";
                            if (servers[uuid].gameClientName != null)
                            {
                                text += " в " + servers[uuid].gameClientName;
                            }
                            byte[] _data = Encoding.UTF8.GetBytes("[MOTD]§3" + text + "[/MOTD][AD]" + ports[uuid] + "[/AD]");
                            _serverSimulatorUdp.Send(_data, _data.Length, new IPEndPoint(IPAddress.Parse("224.0.2.60"), 4445));
                        }
                    }
                }
                else if (!portContains)
                {
                    isClient = false;
                    clientModeWaiting.Set();
                }

                Thread.Sleep(2000);
            }
        }

        public void StopWork()
        {
            isServer = false;
            _isWork = false;

            try { _serverSimulatorUdp.Close(); } catch { }
            try { _clientSimulatorUdp.Close(); } catch { }

            try { ServerSimulatorThread.Abort(); } catch { }
            try { ClientSimulatorThread.Abort(); } catch { }

            if (_serverBridge != null)
            {
                waitingInforming.Set();
                _serverBridge.StopWork();
            }

            if (_clientBridge != null)
            {
                _clientBridge.Close(null);
            }
        }

    }
}
