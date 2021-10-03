using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    class SmpServer
    {
        protected class Client
        {
            public bool isConnected = false; //состаяние подключения
            public long ping = -1;

            public IPEndPoint point = null;

            public ConcurrentDictionary<ushort, byte[]> packagesBuffer = new ConcurrentDictionary<ushort, byte[]>(); //буфер принятых пакетов
            public ConcurrentQueue<byte[]> packagesQueue = new ConcurrentQueue<byte[]>(); //очередь обработанных пакетов
            public ConcurrentQueue<byte[]> sendingBuffer = new ConcurrentQueue<byte[]>(); //буфер пакетов на отправку

            public ushort pointer = 0; //указатель на id следующего пакета, который нужно получить
            public ushort sendingPointer = 0; //указатель на id следующего пакета который будет отправляться
            public long lastTime = 0; //время отправки последнего пакета

            public bool workPing = false;
            public long[] times = new long[20]; //харнит время отправки пакетов с пингом

            public AutoResetEvent suspensionSend = new AutoResetEvent(false);
            public AutoResetEvent threadReset = new AutoResetEvent(false);

            public Semaphore WaitDeletingConnection = new Semaphore(1, 1); // это для блокировки метода DeletingConnection, если он был вызван повторно
            public AutoResetEvent sendingCycleDetector = new AutoResetEvent(false);

            public object locker = new object();

            public Thread serviceSend = null;

            public List<int[]> packagesInfo = new List<int[]>(); //количество пакетов и байт, что стоит на отправку. 0 - количество пакетов 1 - количество байт

            public Semaphore semaphore = new Semaphore(1, 1);
            public AutoResetEvent sendingWait = new AutoResetEvent(false);

            public ushort lastPackage = 0; //id пакета, о доставке которого сейчас ожидается подтверждение
            public ushort maxPackageSize = 3172;
            public ushort maxPackagesCount = 3;

            public bool successfulDelivery = false;

        }

        protected const int pingConst = 10; //эта константа приьавлеятся к пингу при отправке сообщений
        protected int[] delayMultipliers = new int[15]
        { 1, 2, 2, 2, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1 }; //этот массив хранит множители пинга протправке сообщений

        protected ConcurrentDictionary<IPEndPoint, Client> clients = new ConcurrentDictionary<IPEndPoint, Client>();
        protected ConcurrentQueue<IPEndPoint> clientQueue = new ConcurrentQueue<IPEndPoint>(); //эта очередь нужна для метода Receive. В неё хранится ip клиентов от которых были получены пакеты

        protected UdpClient socket = null;
        protected Thread connectionSupport = null;

        protected AutoResetEvent threadReset = new AutoResetEvent(false);
        protected Semaphore ReceiveSignal = new Semaphore(1, 1); // это для блокировки во время работы метода Receive
        protected Semaphore ReadingSignal = new Semaphore(1, 1); // это для блокировки во время работы читающего потока
        public Semaphore ReciveStop = new Semaphore(1, 1); // это нужно чтобы читающий поток можно было приостановить извне

        protected bool ServerWork = true;

        protected IPEndPoint workPing = null; //при работе Ping() сюда помещается ip. когда Ping() переменной присваевается null

        protected bool isConnected = false; //перемнная используемая во время подклчения нового клиента. после удачно подключения снова false
        protected IPEndPoint remoteIp = null;

        public Thread serviceRead = null;

        public SmpServer(int port)
        {
            socket = new UdpClient(port);

            threadReset.Reset();

            connectionSupport = new Thread(delegate () { ConnectionSupport(); });
            connectionSupport.Start();

            serviceRead = new Thread(delegate () { ServiceRead(); });
            serviceRead.Start();

        }
        public SmpServer(UdpClient soc)
        {
            socket = soc;
            var sioUdpConnectionReset = -1744830452;
            var inValue = new byte[] { 0 };
            var outValue = new byte[] { 0 };
            socket.Client.IOControl(sioUdpConnectionReset, inValue, outValue); // это нужно чтобы если мы отправляем данные на уже закрытый клиентский сокет наш сокет не закрывался

            threadReset.Reset();

            connectionSupport = new Thread(delegate () { ConnectionSupport(); });
            connectionSupport.Start();

            serviceRead = new Thread(delegate () { ServiceRead(); });
            serviceRead.Start();
        }

        public bool Connect(IPEndPoint point)
        {
            remoteIp = point;
            int i = 20;

            //измеряем пинг
            Ping ping_ = new Ping();
            PingReply pingReply = ping_.Send(remoteIp.Address);
            long tempPing = pingReply.RoundtripTime + 1;
            tempPing = 80;

            while (!isConnected && (i > 0))
            {
                socket.Send(new byte[1] { 0x00 }, 1, remoteIp);
                i--;

                Thread.Sleep((int)tempPing);
            }

            if (isConnected)
            {
                var client = new Client
                {
                    ping = tempPing,
                    point = remoteIp,
                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000,
                    isConnected = true
                };

                clients[remoteIp] = client;
                client.serviceSend = new Thread(delegate () { ServiceSend(ref client); });
                client.serviceSend.Start();

                client.ping = Ping(remoteIp);
                if (client.ping == -1)
                {
                    DeletingConnection(ref client);
                    isConnected = false;
                    remoteIp = null;

                    return false;
                }

                isConnected = false;
                remoteIp = null;

                return true;
            }
            else
            {
                isConnected = false;
                remoteIp = null;

                return false;
            }
        }

        public int CalculateMTU()
        {
            socket.Client.DontFragment = true;
            socket.Send(new byte[2048], 2048, new IPEndPoint(IPAddress.Parse("8.8.8.8"), 80));

            return 0;
        }

        public IPEndPoint[] GetClients()
        {
            IPEndPoint[] ipEndPoints = new IPEndPoint[clients.Count];
            clients.Keys.CopyTo(ipEndPoints, 0);

            return ipEndPoints;
        }

        public delegate void Closing(IPEndPoint ip);

        public event Closing ClientClosing;

        public long Ping(IPEndPoint ip)
        {
            workPing = ip;
            byte i = 0;

            while (workPing != null && i < 20)
            {
                clients[ip].times[i] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                socket.Send(new byte[2] { 0x01, i }, 2, ip);
                i++;

                Thread.Sleep((int)clients[ip].ping);
            }

            if (workPing == null)
            {
                return clients[ip].ping;
            }
            else
            {
                workPing = null;
                return -1;
            }
        }

        public bool Close(IPEndPoint point) // TODO: доработать этот метод. Ему нельзя работать паралельно с уже работающим DeletingConnection
        {
            if (clients.ContainsKey(point))
            {
                for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                {
                    socket.Send(new byte[1] { 0x05 }, 1, point);
                }

                var client = clients[point];
                DeletingConnection(ref client);

                return true;
            }
            else
            {
                return false;
            }
        }

        //метод останавливает работу сервера
        public void StopWork()
        {
            ServerWork = false;

            serviceRead.Abort();
            connectionSupport.Abort();

            foreach (Client client in clients.Values)
            {
                for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                {
                    socket.Send(new byte[1] { 0x05 }, 1, client.point);
                }

                // TODO: сделать какой-нибудь id для сессии, чтобы запросы на разрыв соединения от предидущего соединения на влияли на текущее

                client.serviceSend.Abort();
            }

            socket.Close();
            socket.Dispose();
        }

        protected void ConnectionSupport() //метод отправляющий пакеты пинга при долгой неактивности для удержания соединения
        {
            while (ServerWork)
            {
                foreach (Client client in clients.Values)
                {
                    if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - client.lastTime >= 10000) //проверяем что последний пакет был отправлен более 2 секунд назад
                    {
                        if (client.isConnected) // TODO: не знаю. нужно ли оставлять так или сделать по-другому
                        {
                            if (Ping(client.point) == -1) //проверяем ответил ли хост
                            {
                                Console.WriteLine("ConnectionSupport");
                                var data = client;
                                DeletingConnection(ref data);
                            }
                        }
                    }
                }

                Thread.Sleep(10000);
            }
        }

        protected void ServiceSend(ref Client client) //метод отправляющий пакеты данных
        {
            while (client.isConnected && ServerWork)
            {
                // TODO: попытаться фиксануть этот костыль
                while (client.packagesInfo.Count == 0) //костыль блять
                {
                    client.sendingWait.WaitOne();
                }

                client.semaphore.WaitOne();
                client.sendingCycleDetector.Reset();

                List<byte[]> sendData = new List<byte[]>();

                ushort idInt;
                lock (client.locker)
                {
                    idInt = client.sendingPointer;
                }

                int i;
                for (i = 0; i < client.packagesInfo.Count; i++)
                {
                    byte[] id = BitConverter.GetBytes(idInt);

                    sendData.Add(new byte[client.packagesInfo[i][1]]);
                    byte[] temp;

                    sendData[i][0] = 0x03;  //код пакета
                    sendData[i][1] = id[0]; //первая часть его айдишника
                    sendData[i][2] = id[1]; //вторая часть

                    int offset = 3;
                    for (int j = 0; j < client.packagesInfo[i][0]; j++)
                    {
                        client.sendingBuffer.TryDequeue(out temp);

                        byte[] packageSize = BitConverter.GetBytes((ushort)temp.Length);

                        sendData[i][offset] = 0x00; //это флаг
                        sendData[i][offset + 1] = packageSize[0]; //размер сегмента данных (первая часть)
                        sendData[i][offset + 2] = packageSize[1]; //размер сегмента данных (вторая часть)

                        offset += 3;

                        Array.Copy(temp, 0, sendData[i], offset, temp.Length);
                        offset += temp.Length;
                    }

                    idInt++;
                }

                sendData[i - 1][3] = 0x01; //флагу последнего пакета присваевам значение что необходимо подтверждение доставки

                lock (client.locker)
                {
                    client.sendingPointer = idInt;
                    idInt--;
                    client.lastPackage = idInt;
                }

                client.packagesInfo.Clear();
                client.semaphore.Release(); //возобновляем работу метода send

                lock (client.locker)
                {
                    client.successfulDelivery = false;
                }

                int b = 0;
                bool successfulDelivery = false;
                long delay = client.ping + pingConst;

                while (ServerWork && client.isConnected && (!client.successfulDelivery || !successfulDelivery) && b < 15) //отправляем этот пакет максимум 20 раз, пока соединение активно. Когда придет подтверждение доставки sendingPointer увеличится на еденицу
                {
                    i = 0;
                    while (i < sendData.Count && !client.successfulDelivery)
                    {
                        socket.Send(sendData[i], sendData[i].Length, client.point);
                        i++;
                    }
                    client.lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (!client.successfulDelivery)
                    {
                        successfulDelivery = client.suspensionSend.WaitOne((int)delay); //после отправки пакета стопаем цикл пока не придет подвтерждение. Но максимальное время остановки == пингу. Если этот метод будет остановлен другим потоком при успешной доставке, то он вернет true
                        delay *= delayMultipliers[b];
                    }

                    b++;
                }

                if (b >= 14 && (!client.successfulDelivery && !successfulDelivery))
                {
                    Console.WriteLine("Ping -1 " + idInt + " " + client.lastPackage + " " + successfulDelivery.ToString());
                    DeletingConnection(ref client);
                    break;
                }

                client.sendingCycleDetector.Set();
            }
        }

        //метод удаляющий клиента
        protected void DeletingConnection(ref Client client)  // TODO: учесть что этот метод может быть вызван двумя разнымии потоками
        {
            // TODO: доработать отключение отправляющего потока

            client.WaitDeletingConnection.WaitOne();

            if (!client.isConnected)
            {
                return; // клиент уже был удален, а это повторный вызов
            }

            ReadingSignal.WaitOne(); //ждём когда читающий поток отработает
            client.isConnected = false; // читающий поток должен переставать принимать пакеты этого клиента, а другие процессы должны остановиться

            ReceiveSignal.WaitOne(); //ждем когда метод Receive закончит работу

            // убираем из clientQueue пакеты этого клиента
            while (clientQueue.Count > 0)
            {
                clientQueue.TryDequeue(out IPEndPoint ipPoint);

                if (ipPoint.ToString() != client.point.ToString())
                {
                    clientQueue.Enqueue(ipPoint);
                    break;
                }
            }

            //удаляем клиента из списка
            IPEndPoint iPoint = client.point;
            clients[iPoint].serviceSend.Abort();
            clients.TryRemove(iPoint, out _); // TODO: нет синхронизации. Некоторые потоки в этот момент могут работать с этим списком

            ReadingSignal.Release();
            ReceiveSignal.Release();

            ClientClosing?.Invoke(iPoint); //Вызываем событие закрытия

            client.WaitDeletingConnection.Release();
        }

        protected void ServiceRead() //метод работающий всегда. Читает UDP сокет
        {
            byte[] data;
            while (ServerWork)
            {
                Client client;
                bool closing = false;
                var controlList = new List<Socket> { socket.Client };

                try
                {
                    IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
                    Socket.Select(controlList, null, null, -1);

                    ReciveStop.WaitOne();
                    data = socket.Receive(ref point);
                    ReciveStop.Release();

                    ReadingSignal.WaitOne(); // блокируем на время работы

                    if (data.Length > 0 && clients.ContainsKey(point) && clients[point].isConnected) //Console.WriteLine("TEST1 " + data[0] + " " + data.Length + " " + client.isConnected);
                    {
                        client = clients[point];
                        client.lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        switch (data[0])
                        {
                            case 0: //запрос на подключение
                                if (data.Length == 1)
                                {
                                    socket.Send(new byte[2] { 0x00, 0x01 }, 2, client.point);
                                }
                                break;

                            case 1: //пришел пакет с пингом
                                if (data.Length == 2)
                                {
                                    socket.Send(new byte[2] { 0x02, data[1] }, 2, client.point);
                                }
                                break;

                            case 2: //пришел ответ на пинг
                                if (workPing != null && data.Length == 2 && data[1] < 21 && workPing.ToString() == client.point.ToString())
                                {
                                    client.ping = DateTimeOffset.Now.ToUnixTimeMilliseconds() - client.times[data[1]]; //вчитаем из данного времени время отправки пакета, делим на 2 и получем пинг
                                    client.ping += 1; // Прибавляем еденицу потому что может получиться 0, если соединение локальное

                                    workPing = null;
                                }
                                break;

                            case 3: //пришел пакет данных
                                if (data.Length > 2)
                                {
                                    ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                    if (id >= client.pointer && id - client.pointer <= 20) //проверяем новый ли это пакет
                                    {
                                        if (id == client.pointer)
                                        {
                                            if (data[3] == 0x01) //пакет требует подтверждение доставки
                                            {
                                                //отправляем подтверждение
                                                byte[] neEbyKakNazvat = BitConverter.GetBytes(id);
                                                socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3, client.point);
                                            }

                                            //разбиваем пакет на блоки данных и кладём в очередь
                                            int offset = 4;
                                            int size; //размер первого блока данных
                                            while (offset < data.Length)
                                            {
                                                size = BitConverter.ToUInt16(new byte[2] { data[offset], data[offset + 1] }, 0);
                                                byte[] dataBlock = new byte[size];

                                                Array.Copy(data, offset + 2, dataBlock, 0, size);
                                                client.packagesQueue.Enqueue(dataBlock); //помещаем пакет в очередь
                                                clientQueue.Enqueue(client.point); //помещаем ip в очередь клиентов

                                                offset += size + 3;
                                            }

                                            client.pointer++;

                                            //ищем в буфере следующие пакеты и помещаем их в очередь
                                            ushort nextId = client.pointer;
                                            while (client.packagesBuffer.ContainsKey(nextId))
                                            {
                                                client.packagesBuffer.TryRemove(nextId, out byte[] tempData); //удаляем элемент из буфера

                                                if (tempData[0] == 0x01) //пакет требует подтверждение доставки
                                                {
                                                    //отправляем подтверждение
                                                    byte[] neEbyKakNazvat = BitConverter.GetBytes(nextId);
                                                    socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3, client.point);
                                                }

                                                offset = 1;
                                                while (offset < tempData.Length)
                                                {
                                                    size = BitConverter.ToUInt16(new byte[2] { tempData[offset], tempData[offset + 1] }, 0);
                                                    byte[] dataBlock = new byte[size];

                                                    Array.Copy(tempData, offset + 2, dataBlock, 0, size);
                                                    client.packagesQueue.Enqueue(dataBlock); //помещаем пакет в очередь
                                                    clientQueue.Enqueue(client.point); //помещаем ip в очередь клиентов

                                                    offset += size + 3;
                                                }

                                                client.pointer++;
                                                nextId++;
                                            }

                                            threadReset.Set(); //возобнавляем ожидающий поток

                                        }
                                        else if (!client.packagesBuffer.ContainsKey(id)) //проверяем есть ли уже этот элемент в буфере
                                        {
                                            client.packagesBuffer[id] = new byte[data.Length - 3];
                                            Array.Copy(data, 3, client.packagesBuffer[id], 0, data.Length - 3); //убираем служебные данные и помещяем элемент в буфер
                                        }

                                    }
                                    else if (data[3] == 0x01)
                                    {
                                        socket.Send(new byte[3] { 0x04, data[1], data[2] }, 3, client.point); //отправляем пакет подтверждающий доставку
                                    }

                                }
                                break;

                            case 4: //пришло подтверждение доставки пакета
                                if (data.Length == 3)
                                {
                                    lock (client.locker)
                                    {
                                        ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                        if (id == client.lastPackage && !client.successfulDelivery)
                                        {
                                            client.successfulDelivery = true;
                                            client.suspensionSend.Set(); //возобновляем отправляющий цикл, ведь подтверждение пришло
                                        }
                                    }

                                }
                                break;

                            case 5: //пришел пакет на обрыв соединения
                                Console.WriteLine("case 5");
                                ReadingSignal.Release(); //разблочиваем
                                DeletingConnection(ref client);
                                closing = true;

                                break;
                        }
                    }
                    else
                    {
                        if (!isConnected)
                        {
                            bool isPackage = (data[0] == 0x00 || data[0] == 0x01 || data[0] == 0x03);

                            if (data.Length > 0 && isPackage && remoteIp != null && remoteIp.ToString() == point.ToString())
                            {
                                isConnected = true;

                                // если это пакет пинга то отвечаем на него
                                if (data[0] == 0x01)
                                {
                                    socket.Send(new byte[2] { 0x02, data[1] }, 2, remoteIp);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // TODO: тут куда-то кидать инфу об исключении
                    break;
                }

                if (!closing) //если он уже не был разлочен при case 5
                {
                    ReadingSignal.Release(); //разблочиваем
                }
            }
        }

        public void Send(byte[] inputData, IPEndPoint ip)
        {
            var client = clients[ip];

            //больше пакетов отправлять нельзя. Ждём когда уже отправленные пакеты дойдут чтобы отправить этот
            if (client.packagesInfo.Count >= client.maxPackagesCount)
            {
                client.sendingCycleDetector.WaitOne();
            }

            client.semaphore.WaitOne(); //ждём когда поток отпарвки сделает свою работу

            int temp;
            //если в буфер не пуст
            if (client.packagesInfo.Count > 0)
            {
                temp = client.packagesInfo[client.packagesInfo.Count - 1][1] + inputData.Length + 3; // расчитываем размер, который получиться у итогового пакета который будем отправлять, если этот пакет засунуть в буфер

                //если этот размер меньше максимального
                if (temp <= client.maxPackageSize)
                {
                    client.sendingBuffer.Enqueue(inputData);

                    client.packagesInfo[client.packagesInfo.Count - 1][0]++;
                    client.packagesInfo[client.packagesInfo.Count - 1][1] = temp;
                }
                else
                {
                    //если размер этого пакета меньше максимального - суем его в буфер, но уже в следующий отправляемый пакет, ведь в этот он не вмещается
                    if (inputData.Length + 6 <= clients[ip].maxPackageSize)
                    {
                        client.packagesInfo.Add(new int[2] { 1, 6 });
                        client.sendingBuffer.Enqueue(inputData);
                        client.packagesInfo[client.packagesInfo.Count - 1][1] += inputData.Length;
                    }
                    else
                    {
                        //разбиваем пакет на части и добавляем в буфер
                    }
                }
            }
            else
            {
                client.packagesInfo.Add(new int[2] { 1, 6 });
                if (inputData.Length + 6 <= clients[ip].maxPackageSize)
                {
                    client.sendingBuffer.Enqueue(inputData);
                    client.packagesInfo[0][1] += inputData.Length;
                }
                else
                {
                    //разбиваем пакет на части и добавляем в буфер
                }
            }

            client.sendingWait.Set();
            client.semaphore.Release();
        }

        public IPEndPoint Receive(out byte[] data)
        {
            ReceiveSignal.WaitOne();

            if (clientQueue.Count > 0)
            {
                IPEndPoint ipPoint;
                clientQueue.TryDequeue(out ipPoint); //получаем ip клииента от которого пришло последняя датаграмма

                clients[ipPoint].packagesQueue.TryDequeue(out data);
                ReceiveSignal.Release();

                return ipPoint;

            }
            else //буфер пуст
            {
                while (ServerWork)
                {
                    ReceiveSignal.Release();
                    threadReset.WaitOne(); //этот поток возобновится когда появятся новые пакеты
                    ReceiveSignal.WaitOne();

                    if (clientQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        IPEndPoint ipPoint;
                        clientQueue.TryDequeue(out ipPoint); //получаем ip клиента от которого пришло последняя датаграмма

                        clients[ipPoint].packagesQueue.TryDequeue(out data);
                        ReceiveSignal.Release();

                        return ipPoint;

                    }
                }
            }

            data = new byte[0];

            ReceiveSignal.Release();
            return null; //если достигнута эта часть кода, значит serverWorkd стало равно false, что означает остановку сервера
        }
    }
}
