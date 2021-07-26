using LumiSoft.Net.STUN.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    using SMP;
    using System.Collections.Generic;

    abstract class NetworkServer // TODO: на стороне сервера проверять есть один у дркгого в списке друзей
    {
        protected Thread AcceptingThread;
        protected Thread ReadingThread;
        protected Thread SendingThread;

        protected Semaphore AcceptingBlock; //блокировка во время приёма подключения
        protected Semaphore SendingBlock; //блокировка во время работы метода Sending

        protected AutoResetEvent threadReset;
        protected AutoResetEvent threadResetReading;

        protected SmpServer Server;
        protected UdpClient ServerUdp;

        protected List<IPEndPoint> AvailableConnections;
        protected bool IsWork = false;

        public NetworkServer(string serverPrefix)
        {
            IsWork = true;
            AcceptingBlock = new Semaphore(1, 1);
            SendingBlock = new Semaphore(1, 1);

            threadReset = new AutoResetEvent(false);
            threadResetReading = new AutoResetEvent(false);

            ServerUdp = new UdpClient(9654);
            Server = new SmpServer(ServerUdp);

            AvailableConnections = new List<IPEndPoint>();

            Server.ClientClosing += ClientAbort;

            ReadingThread = new Thread(delegate () //этот поток читает сообщения от клиента
            {
                Reading();
            });

            SendingThread = new Thread(delegate () //этот поток отправляет сообщения клиенту
            {
                Sending();
            });

            AcceptingThread = new Thread(delegate () //поток принимающий новые подключения
            {
                Accepting(serverPrefix);
            });

            AcceptingThread.Start();
            SendingThread.Start();
            ReadingThread.Start();

        }

        protected virtual void Accepting(string serverType) // TODO: нужно избегать повторного подключения
        {
            //подключаемся к управляющему серверу
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 4565); //194.61.2.176

            NetworkStream stream = client.GetStream();

            string st = "{\"UUID\" : \"344a7f427fb765610ef96eb7bce95257\", \"type\": \""+ serverType + "\"}";
            byte[] sendData = Encoding.UTF8.GetBytes(st);
            stream.Write(sendData, 0, sendData.Length); //авторизируемся на упрявляющем сервере

            while (IsWork)
            {
                {
                    byte[] data = new byte[1];

                    int bytes = stream.Read(data, 0, data.Length);

                    if (bytes == 1 && data[0] == 97) // data[0] == 97 значит поступил запрос на поделючение
                    {
                        stream.Write(new byte[1] { 97 }, 0, 1); //отправляем серверу соглашение

                        bytes = stream.Read(data, 0, data.Length);
                        if (bytes == 1 && data[0] == 98) //сервер запрашивает мой порт
                        {
                            Server.ReciveStop.WaitOne(); // это нужно чтобы не было коллизий с работающем методом Recive
                            STUN_Result result = STUN_Client.Query("stun.l.google.com", 19302, ServerUdp.Client); //получем наш внешний адрес
                            Server.ReciveStop.Release();

                            //парсим порт
                            string externalPort = result.PublicEndPoint.ToString(); // TODO: был нулл поинтер
                            externalPort = externalPort.Substring(externalPort.IndexOf(":") + 1, externalPort.Length - externalPort.IndexOf(":") - 1).Trim();
                            byte[] portData = Encoding.UTF8.GetBytes(externalPort.ToString());
                            //byte[] portData = Encoding.UTF8.GetBytes("fsd");

                            stream.Write(portData, 0, portData.Length); //отправляем серверу наш порт

                        }
                        else
                        {
                            // TODO: опять же произошло что-то с сервером
                            continue;
                        }

                    }
                    else
                    {
                        // TODO: что-то странное произошло. сервер должен 1 байт вернуть и отправить запрос на подключени
                        continue;
                    }
                }

                {
                    byte[] data = new byte[21];
                    int bytes = stream.Read(data, 0, data.Length); //получем ip клиента

                    byte[] resp = new byte[bytes];
                    for (int i = 0; i < bytes; i++) // TODO: сделать этот перенос нормально, но не через resize
                    {
                        resp[i] = data[i];
                    }

                    string str = Encoding.UTF8.GetString(resp, 0, resp.Length);
                    string hostPort = str.Substring(str.IndexOf(":") + 1, str.Length - str.IndexOf(":") - 1).Trim();
                    string hostIp = str.Replace(":" + hostPort, "");

                    IPEndPoint point = new IPEndPoint(IPAddress.Parse(hostIp), Int32.Parse(hostPort));

                    AcceptingBlock.WaitOne();

                    if (Server.Connect(point))
                    {
                        AvailableConnections.Add(point);
                        BeforeConnect(point);

                        threadReset.Set(); // если это первый клиент, то сейчас читающий поток будет запущен
                        threadResetReading.Set();
                    }
                    else
                    {
                        AcceptingBlock.Release();
                    }
                }
            }
        }

        public virtual void StopWork()
        {
            IsWork = false;

            AcceptingThread.Abort();
            SendingThread.Abort();
            ReadingThread.Abort();

            Server.StopWork();
        }

        protected virtual void ClientAbort(IPEndPoint point) { } // мeтод который вызывается при обрыве соединения

        protected virtual void BeforeConnect(IPEndPoint point) { } // это метод который запускается после установления соединения

        protected virtual void Sending() { } // тут получаем данные от клиентов

        protected virtual void Reading() { } // тут получаем данные из сети

    }
}
