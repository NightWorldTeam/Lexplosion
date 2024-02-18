﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using System.Net.NetworkInformation;

namespace Lexplosion.Logic.Network
{
    class OnlineGameGateway
    {
        private Thread ServerSimulatorThread;
        private Thread ClientSimulatorThread;
        private Thread InformingThread;

        private ServerBridge Server = null;

        private string UUID;
        private string sessionToken;

        public event Action<string> ConnectingUser;
        public event Action<string> DisconnectedUser;
        public event Action<OnlineGameStatus, string> StatusChanged;
        public event Action<SystemState> StateChanged;

        private bool _isInit = false;
        /// <summary>
        /// Использовать ли прямо подключение
        /// </summary>
        private bool _directConnection = false;

        private ControlServerData _controlServer;

        /// <summary>
        /// Отвечает за тевевую игру.
        /// </summary>
        /// <param name="uuid">Айдишник игрока.</param>
        /// <param name="sessionToken_">Его токен</param>
        /// <param name="controlServer">Айпи сервера сетевой игры</param>
        /// <param name="directConnection">Использовать ли прямо подключение в приоритете</param>
        public OnlineGameGateway(string uuid, string sessionToken_, ControlServerData controlServer, bool directConnection)
        {
            UUID = uuid;
            sessionToken = sessionToken_;
            _controlServer = controlServer;
            _directConnection = directConnection;
            Runtime.DebugWrite("Create Gateway");
        }

        public void Initialization(int pid)
        {
            if (!_isInit)
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

                _isInit = true;
            }
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

            try
            {
                //присоединяемся к мультикасту для Loopback адаптера
                var optionValue = new MulticastOption(IPAddress.Parse("224.0.2.60"));
                client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("multicast error: " + ex);
            }

            client.Client.ReceiveTimeout = -1; // убираем таймоут, чтобы этот метод мог ждать бесконечно

            string _name = null;
            int _port = 0;

            while (true)
            {
                AutoResetEvent waitingInforming = new AutoResetEvent(false);

                bool successful = ListenGameSrvers(client, out string name, out int port, pid);

                if (!successful) // TODO: из всего алгоритма выходить не надо, надо только перевести всё в ручной режим
                {
                    return;
                }

                // пока сервер в процессе закрытия мы можем получить данные будто бы он открыт. поэтому проверяем 
                if (!Utils.ContainsTcpPort(pid, port))
                {
                    Runtime.DebugWrite("Port " + port + " not contains");
                    Thread.Sleep(3000);
                    continue;
                }

                _name = name; _port = port;

                InformingThread = new Thread(delegate ()
                {
                    Dictionary<string, string> input = new Dictionary<string, string>
                    {
                        ["UUID"] = UUID,
                        ["sessionToken"] = sessionToken
                    };

                    try
                    {
                        // раз в 2 минуты отправляем пакеты основному серверу информирующие о доступности нашего игровго сервера
                        do
                        {
                            string ans = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "setGameServer", input);
                            Runtime.DebugWrite(ans);
                        }
                        while (!waitingInforming.WaitOne(120000));
                    }
                    finally
                    {
                        Task.Run(delegate ()
                        {
                            ToServer.HttpPost(LaunсherSettings.URL.UserApi + "dropGameServer", input);
                        });
                    }
                });

                Server = new ServerBridge(UUID, sessionToken, port, _directConnection, _controlServer);

                Server.ConnectingUser += ConnectingUser;
                Server.DisconnectedUser += DisconnectedUser;
                StatusChanged?.Invoke(OnlineGameStatus.OpenWorld, "");

                InformingThread.Start();

                while (true)
                {
                    // проверяем имеется ли этот порт. Если имеется - значит сервер запущен
                    if (!Utils.ContainsTcpPort(pid, port))
                    {
                        break;
                    }

                    Thread.Sleep(3000);
                }

                StatusChanged?.Invoke(OnlineGameStatus.None, "");
                waitingInforming.Set(); // высвобождаем поток InformingThread чтобы он не ждал лишнее время
                try { InformingThread.Abort(); } catch { }
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
            ClientBridge bridge = new ClientBridge(UUID, sessionToken, _controlServer);

            UdpClient client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            try
            {
                //присоединяемся к мультикасту для Loopback адаптера
                var optionValue = new MulticastOption(IPAddress.Parse("224.0.2.60"), NetworkInterface.LoopbackInterfaceIndex);
                client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("multicast error: " + ex);
            }

            while (true)
            {
                if (Utils.ContainsUdpPort(pid, 4445))
                {
                    Dictionary<string, string> input = new Dictionary<string, string>
                    {
                        ["UUID"] = UUID,
                        ["sessionToken"] = sessionToken
                    };

                    string data = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "getGameServers", input);
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

        public void KickClient(string uuid)
        {
            Server?.KickClient(uuid);
        }

        public void UnkickClient(string uuid)
        {
            Server?.UnkickClient(uuid);
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
