using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    class SmpClient
    {
        protected class PackageInfo
        {
            public byte[] Data;
            public ushort ID;
        }

        protected UdpClient socket = null;
        protected bool isConnected = false;
        public long ping = -1;

        public IPEndPoint point = null;

        protected Thread serviceSend = null;
        protected Thread serviceReceive = null;
        protected Thread connectionControl = null;

        protected ConcurrentDictionary<ushort, byte[]> packagesBuffer = new ConcurrentDictionary<ushort, byte[]>(); //буфер принятых пакетов
        protected ConcurrentQueue<byte[]> sendingBuffer = new ConcurrentQueue<byte[]>(); //буфер пакетов на отправку
        public ConcurrentQueue<byte[]> packagesQueue = new ConcurrentQueue<byte[]>(); //очередь обработанных пакетов
        public List<ushort> repeatDelivery = null; //список айдишников пакетов, отправку которых нужно повторить

        public ushort pointer = 0; //указатель на id следующего пакета, который нужно получить
        public ushort sendingPointer = 0; //указатель на id следующего пакета который будет отправляться
        protected long lastTime = 0; //время отправки последнего пакета

        protected AutoResetEvent threadReset = new AutoResetEvent(false);
        protected AutoResetEvent suspensionSend = new AutoResetEvent(false);

        public object confirmationLocker = new object();
        public object repeatDeliveryLocker = new object();

        protected Semaphore semaphore = new Semaphore(1, 1);
        protected AutoResetEvent sendingWait = new AutoResetEvent(false);
        protected ManualResetEvent waitOldDatagrams = new ManualResetEvent(true); // эта херь убдет стопать отправляющий поток, если было получени повторное подвердение доставки второго пакета
        protected AutoResetEvent sendingCycleDetector = new AutoResetEvent(false);

        protected List<int[]> packagesInfo = new List<int[]>(); //количество пакетов и байт, что стоит на отправку. 0 - количество пакетов 1 - количество байт
        protected ushort lastPackage = 0; //id пакета, о доставке которого сейчас ожидается подтверждение

        protected bool workPing = false;
        protected long[] times = new long[20]; //харнит время отправки пакетов с пингом

        protected bool successfulDelivery = false;
        protected bool oldDatagramArrived = false;

        protected ushort maxPackageSize = 540; //максимальный размер отправляемых пакетов
        protected ushort maxPackagesCount = 4; //количество пакетов которое можно отправить за 1 раз. В процессе работы это значение может меняться. Чем стабльнее сеть, тем оно выше
        protected const ushort maxDatagramsCount = 10; //максимальное количество пакетов, что можно отправить за один раз
        protected ushort successfulDeliveryCount = 1; //количество раз, когда пакеты былаи доставлены с первого раза
        protected int needConfirmation = -1;

        protected const int pingConst = 10; //эта константа приьавлеятся к пингу при отправке сообщений
        protected int[] delayMultipliers = new int[15]
        { 1, 2, 2, 2, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1 }; //этот массив хранит множители пинга протправке сообщений

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        public SmpClient(int port)
        {
            socket = new UdpClient(port);
        }

        public SmpClient(int port, string addr)
        {
            socket = new UdpClient(new IPEndPoint(IPAddress.Parse(addr), port));
        }
        public SmpClient(UdpClient s)
        {
            socket = s;

            var sioUdpConnectionReset = -1744830452;
            var inValue = new byte[] { 0 };
            var outValue = new byte[] { 0 };
            socket.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
            socket.Ttl = 128;
        }

        public SmpClient(IPEndPoint addr)
        {
            socket = new UdpClient(addr);
        }

        public delegate void Closing(IPEndPoint ip);

        public event Closing ClientClosing;

        public bool Connect(int port, string addr)
        {
            IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse(addr), port);
            int i = 20;

            var thread = new Thread(delegate ()
            {
                byte[] data = null;

                while (!isConnected && (i > 0))
                {
                    try
                    {
                        data = socket.Receive(ref remoteIp);
                        if (data.Length > 0)
                        {
                            if (data[0] == 0x00 || data[0] == 0x01 || data[0] == 0x03) //если пришел пакет на установления соединения, пинг или пакет данных, то isConnected делаем true
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
                    catch { }
                }

            });
            thread.Start();

            //измеряем пинг
            Ping ping_ = new Ping();
            PingReply pingReply = ping_.Send(remoteIp.Address);
            ping = pingReply.RoundtripTime + 1;
            ping = 80;

            while (!isConnected && (i > 0))
            {
                socket.Send(new byte[1] { 0x00 }, 1, remoteIp);

                Thread.Sleep((int)ping);
                i--;
            }

            if (isConnected)
            {
                point = remoteIp;
                socket.Connect(point);

                serviceSend = new Thread(delegate () { ServiceSend(); });
                serviceReceive = new Thread(delegate () { ServiceReceive(); });
                connectionControl = new Thread(delegate () { ConnectionControl(); });

                serviceSend.Start();
                serviceReceive.Start();
                connectionControl.Start();

                ping = Ping(); //более точно измеряем пинг
                if (ping == -1)
                {
                    Console.WriteLine("avcz");
                    isConnected = false;
                    return false;
                }

                lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000;

                return true;
            }
            else
            {
                return false;
            }
        }

        public int CalculateMTU()
        {
            socket.Client.DontFragment = true;
            socket.Send(new byte[2048], 2048);
            return 0;
        }

        public void Close()
        {
            if (isConnected) //проверяем является ли соединение активным
            {
                try //может произойти ситуация что в другом потоке соединение уже будет закрыто, поэтому сделал костыль
                {
                    Console.WriteLine("close-data-123");
                    for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                    {
                        socket.Send(new byte[1] { 0x05 }, 1);
                    }
                }
                catch { }

                isConnected = false;
                socket.Close();
                threadReset.Set();

                serviceSend.Abort();
                serviceReceive.Abort();
                connectionControl.Abort();
            }
        }

        public long Ping()
        {
            workPing = true;
            byte i = 0;

            while (workPing && i < 20)
            {
                times[i] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                socket.Send(new byte[2] { 0x01, i }, 2);
                i++;

                Thread.Sleep((int)ping);
            }

            if (!workPing)
            {
                return ping;
            }
            else
            {
                return -1;
            }
        }

        protected void ConnectionControl() //метод работающий всегда. контролирует доставку пакетов
        {
            while (isConnected)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTime >= 10000) //проверяем что последний пакет был отправлен более 2 секунд назад
                {
                    if (Ping() == -1) //проверяем ответил ли хост
                    {
                        Console.WriteLine("ConnectionControl");
                        Close();
                        ClientClosing?.Invoke(point);
                    }
                }

                Thread.Sleep(10000);
            }
        }

        protected void ServiceSend() //метод работающий всегда. отправляет пакеты данных
        {
            while (isConnected || packagesInfo.Count > 0)
            {
                // TODO: попытаться фиксануть этот костыль
                while (packagesInfo.Count == 0) //костыль блять
                {
                    sendingWait.WaitOne();
                }

                semaphore.WaitOne();
                sendingCycleDetector.Reset();

                List<PackageInfo> sendData = new List<PackageInfo>();

                ushort idInt;
                lock (confirmationLocker)
                {
                    idInt = sendingPointer;
                }

                int i;
                byte[] lastId = BitConverter.GetBytes(idInt + (packagesInfo.Count - 1));
                //Console.WriteLine("LASTSTSTS ID " + idInt + (client.packagesInfo.Count - 1));
                for (i = 0; i < packagesInfo.Count; i++)
                {
                    byte[] id = BitConverter.GetBytes(idInt);

                    sendData.Add(new PackageInfo
                    {
                        Data = new byte[packagesInfo[i][1] + 2],
                        ID = idInt
                    });

                    sendData[i].Data[0] = 0x03;  //код пакета
                    sendData[i].Data[1] = id[0]; //первая часть его айдишника
                    sendData[i].Data[2] = id[1]; //вторая часть

                    int offset = 3;
                    for (int j = 0; j < packagesInfo[i][0]; j++)
                    {
                        sendingBuffer.TryDequeue(out byte[] temp);

                        byte[] packageSize = BitConverter.GetBytes((ushort)temp.Length);

                        sendData[i].Data[offset] = 0x00; //это флаг
                        sendData[i].Data[offset + 1] = packageSize[0]; //размер сегмента данных (первая часть)
                        sendData[i].Data[offset + 2] = packageSize[1]; //размер сегмента данных (вторая часть)

                        offset += 3;

                        Array.Copy(temp, 0, sendData[i].Data, offset, temp.Length);
                        offset += temp.Length;
                    }

                    sendData[i].Data[sendData[i].Data.Length - 1] = lastId[0];
                    sendData[i].Data[sendData[i].Data.Length - 2] = lastId[1];

                    idInt++;
                }

                sendData[i - 1].Data[3] = 0x01; //флагу последнего пакета присваевам значение что необходимо подтверждение доставки

                lock (confirmationLocker)
                {
                    sendingPointer = idInt;
                    idInt--;
                    lastPackage = idInt;
                }

                packagesInfo.Clear();
                semaphore.Release(); //возобновляем работу метода send

                lock (confirmationLocker)
                {
                    successfulDelivery = false;
                }

                int b = 0;
                long delay = (ping + pingConst) * sendData.Count;

                while (isConnected && !successfulDelivery && b < 15) //отправляем этот пакет максимум 20 раз, пока соединение активно. Когда придет подтверждение доставки sendingPointer увеличится на еденицу
                {
                    if (b > 1)
                    {
                        // скорее всего сеть перегружена. поэтому отправляем только один пакет
                        if (sendData.Count > 1)
                        {
                            socket.Send(sendData[1].Data, sendData[1].Data.Length);
                        }
                        else
                        {
                            socket.Send(sendData[0].Data, sendData[0].Data.Length);
                        }
                    }
                    else
                    {
                        i = 0;
                        while (i < sendData.Count && !successfulDelivery)
                        {
                            socket.Send(sendData[i].Data, sendData[i].Data.Length);
                            i++;
                        }
                    }

                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (!successfulDelivery)
                    {
                    Begin: suspensionSend.WaitOne((int)delay); //после отправки пакета стопаем цикл пока не придет подвтерждение или запрос на повторную отправку. Но максимальное время остановки == delay.

                        if (!successfulDelivery)
                        {
                            List<ushort> repeatDelivery_;
                            lock (repeatDeliveryLocker)
                            {
                                repeatDelivery_ = repeatDelivery;
                                repeatDelivery = null;
                            }

                            // проверяем есть ли пакеты, которые необходимо пееротправить
                            if (repeatDelivery_ != null)
                            {
                                PackageInfo[] sendData_ = sendData.ToArray();
                                bool isValid = true;
                                foreach (ushort Id in repeatDelivery_) // TODO: тут намутить бинарный поиск или нет
                                {
                                    bool isValid_ = false;
                                    foreach (PackageInfo info in sendData_)
                                    {
                                        if (info.ID == Id)
                                        {
                                            isValid_ = true;
                                            break;
                                        }
                                    }

                                    if (!isValid_)
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }

                                if (isValid)
                                {
                                    foreach (PackageInfo info in sendData_)
                                    {
                                        if (!repeatDelivery_.Contains(info.ID))
                                        {
                                            sendData.Remove(info);
                                        }
                                    }

                                    b = -1; //присваиваем -1 что бы к концу этой интерации значение было 0
                                    delay = (ping + pingConst) * sendData.Count; // возвращаем задержку к изначальному состоянию
                                }
                                else
                                {
                                    goto Begin;
                                }
                            }
                            else
                            {
                                delay *= delayMultipliers[b];
                            }
                        }
                    }

                    b++;

                    if (b == 15 && Ping() != -1)
                    {
                        b = 14;
                    }
                }

                //вычисляем максимально количество отправляемых за раз пакетов
                /*if (b < 3 && client.successfulDeliveryCount < maxDatagramsCount) //если эти пакеты доставлены с первого раза, то увелчиваем successfulDeliveryCount на 1
                {
                    client.successfulDeliveryCount++;
                }
                else if (b >= 3) //иначе обновляем значение maxPackagesCount и обнуляем successfulDeliveryCount
                {
                    client.maxPackagesCount = client.successfulDeliveryCount;
                    Console.WriteLine(client.maxPackagesCount + " B");
                    client.successfulDeliveryCount = 1;
                }
                else if (client.successfulDeliveryCount == maxDatagramsCount)
                {
                    client.maxPackagesCount = client.successfulDeliveryCount;
                    Console.WriteLine(client.maxPackagesCount + " A");
                }*/

                if (b >= 14 && !successfulDelivery)
                {
                    Console.WriteLine("Client Ping -1 " + idInt + " " + lastPackage);
                    // TODO: тут че-то сделать надо
                    break;
                }

                sendingCycleDetector.Set();
            }
        }

        protected void ServiceReceive() //метод работающий всегда. Читает UDP сокет и контролирует доставку пакетов
        {
            byte[] data;
            byte[] testarr = new byte[0];
            while (isConnected)
            {
                try
                {
                    data = socket.Receive(ref point); // TODO: может получиться как с сервером: если отправить данные на закрытый сокет, то наш сокет возможно начнет бросать исключения. То есть если сервер откинет коньки, то этот сокет будет бросать исключения
                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (data[0] == 3)
                    {
                        ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                        Console.WriteLine("ПРИШЕЛ " + id);
                    }

                    if (data.Length > 0)
                    {
                        switch (data[0])
                        {
                            case 0: //запрос на подключение (в штатном режиме приходит только если данный хост уже получил такой же запрос и isConnected установил true)
                                if (data.Length == 1)
                                {
                                    socket.Send(new byte[2] { 0x00, 0x01 }, 2);
                                }
                                break;

                            case 1: //пришел пакет с пингом
                                if (data.Length == 2)
                                {
                                    socket.Send(new byte[2] { 0x02, data[1] }, 2);
                                }
                                break;

                            case 2: //пришел ответ на пинг
                                if (data.Length == 2 && data[1] < 21)
                                {
                                    ping = DateTimeOffset.Now.ToUnixTimeMilliseconds() - times[data[1]]; //вчитаем из данного времени время отправки пакета
                                    ping += 1; // Прибавляем еденицу потому что может получиться 0, если соединение локальное

                                    workPing = false;
                                }
                                break;

                            case 3: //пришел пакет данных
                                if (data.Length > 2)
                                {
                                    ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);

                                    if (id >= pointer && id - pointer <= 120) //проверяем новый ли это пакет
                                    {
                                        if (needConfirmation == -1)
                                        {
                                            needConfirmation = BitConverter.ToUInt16(new byte[2] { data[data.Length - 1], data[data.Length - 2] }, 0);
                                        }

                                        if (id == pointer)
                                        {
                                            if (data[3] == 0x01) //пакет требует подтверждение доставки
                                            {
                                                //отправляем подтверждение
                                                byte[] neEbyKakNazvat = BitConverter.GetBytes(id);
                                                socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);
                                                needConfirmation = -1;
                                            }

                                            //разбиваем пакет на блоки данных и кладём в очередь
                                            int offset = 4;
                                            int size; //размер первого блока данных
                                            while (offset < data.Length - 2)
                                            {
                                                size = BitConverter.ToUInt16(new byte[2] { data[offset], data[offset + 1] }, 0);
                                                byte[] dataBlock = new byte[size];

                                                Array.Copy(data, offset + 2, dataBlock, 0, size);
                                                packagesQueue.Enqueue(dataBlock); //помещаем пакет в очередь

                                                offset += size + 3;
                                            }

                                            pointer++;

                                            //ищем в буфере следующие пакеты и помещаем их в очередь
                                            ushort nextId = pointer;
                                            while (packagesBuffer.ContainsKey(nextId))
                                            {
                                                byte[] tempData;
                                                packagesBuffer.TryRemove(nextId, out tempData); //удаляем элемент из буфера

                                                if (tempData[0] == 0x01) //пакет требует подтверждение доставки
                                                {
                                                    //отправляем подтверждение
                                                    byte[] neEbyKakNazvat = BitConverter.GetBytes(nextId);
                                                    socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);
                                                    needConfirmation = -1;
                                                }

                                                offset = 1;
                                                while (offset < tempData.Length - 2)
                                                {
                                                    size = BitConverter.ToUInt16(new byte[2] { tempData[offset], tempData[offset + 1] }, 0);
                                                    byte[] dataBlock = new byte[size];

                                                    Array.Copy(tempData, offset + 2, dataBlock, 0, size);
                                                    packagesQueue.Enqueue(dataBlock); //помещаем пакет в очередь

                                                    offset += size + 3;
                                                }

                                                pointer++;
                                                nextId++;
                                            }

                                            threadReset.Set(); //возобнавляем ожидающий поток
                                        }
                                        else if (!packagesBuffer.ContainsKey(id)) //проверяем есть ли уже этот элемент в буфере
                                        {
                                            packagesBuffer[id] = new byte[data.Length - 3];
                                            Array.Copy(data, 3, packagesBuffer[id], 0, data.Length - 3); //убираем служебные данные и помещяем элемент в буфер

                                            List<ushort> repeatDelivery = new List<ushort>();
                                            ushort i = pointer;

                                            while (i <= needConfirmation)
                                            {
                                                if (!packagesBuffer.ContainsKey(i))
                                                {
                                                    repeatDelivery.Add(i);
                                                }
                                                i++;
                                            }

                                            if (repeatDelivery.Count > 0)
                                            {
                                                byte[] package = new byte[repeatDelivery.Count * 2 + 1];
                                                package[0] = 0x06;

                                                int offset = 1;
                                                foreach (ushort repeatId in repeatDelivery)
                                                {
                                                    byte[] repeatIdBytes = BitConverter.GetBytes(repeatId);
                                                    package[offset] = repeatIdBytes[0];
                                                    package[offset + 1] = repeatIdBytes[1];
                                                    offset += 2;
                                                }

                                                if (!Enumerable.SequenceEqual(testarr, package))
                                                {
                                                    socket.Send(package, package.Length);
                                                    testarr = package;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (data[3] == 0x01)
                                        {
                                            socket.Send(new byte[3] { 0x04, data[1], data[2] }, 3); //отправляем пакет подтверждающий доставку
                                        }

                                        if (needConfirmation != -1 && BitConverter.ToUInt16(new byte[2] { data[data.Length - 1], data[data.Length - 2] }, 0) == needConfirmation)
                                        {
                                            packagesBuffer[id] = new byte[data.Length - 3];
                                            Array.Copy(data, 3, packagesBuffer[id], 0, data.Length - 3); //убираем служебные данные и помещяем элемент в буфер

                                            List<ushort> repeatDelivery = new List<ushort>();
                                            ushort i = pointer;
                                            while (i <= needConfirmation)
                                            {
                                                if (!packagesBuffer.ContainsKey(i))
                                                {
                                                    repeatDelivery.Add(i);
                                                }
                                                i++;
                                            }

                                            if (repeatDelivery.Count > 0)
                                            {
                                                byte[] package = new byte[repeatDelivery.Count * 2 + 1];
                                                package[0] = 0x06;

                                                int offset = 1;
                                                foreach (ushort repeatId in repeatDelivery)
                                                {
                                                    byte[] repeatIdBytes = BitConverter.GetBytes(repeatId);
                                                    package[offset] = repeatIdBytes[0];
                                                    package[offset + 1] = repeatIdBytes[1];
                                                    offset += 2;
                                                }

                                                if (!Enumerable.SequenceEqual(testarr, package))
                                                {
                                                    socket.Send(package, package.Length);
                                                    testarr = package;
                                                }
                                            }
                                        }
                                    }
                                }

                                break;

                            case 4: //пришло подтверждение доставки пакета
                                {
                                    if (data.Length == 3)
                                    {
                                        lock (confirmationLocker)
                                        {
                                            ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                            if (id == lastPackage && !successfulDelivery)
                                            {
                                                successfulDelivery = true;
                                                suspensionSend.Set(); //возобновляем отправляющий цикл, ведь подтверждение пришло
                                            }
                                        }

                                    }
                                }
                                break;

                            case 5: //пришел пакет на обрыв соединения
                                Console.WriteLine("case 5 client");
                                isConnected = false;
                                socket.Close();
                                threadReset.Set();
                                ClientClosing?.Invoke(point);

                                break;

                            case 6: // пришел пакет со списком пакетов, которые нужно переотправить
                                    //проверяем валидность этого пакета. пакет должен содержать список айдишников. каждый id занимает 2 байта. первый байт - код.
                                if ((data.Length - 1) % 2 == 0)
                                {
                                    List<ushort> ids = new List<ushort>();
                                    int i = 1;
                                    while (i < data.Length)
                                    {
                                        ushort id = BitConverter.ToUInt16(new byte[2] { data[i], data[i + 1] }, 0);
                                        ids.Add(id);
                                        i += 2;
                                    }

                                    lock (repeatDeliveryLocker)
                                    {
                                        repeatDelivery = ids;
                                    }
                                    //Console.WriteLine("SET " + string.Join(" ,", ids));

                                    suspensionSend.Set();
                                }

                                break;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Исключение");
                    // TODO: тут наверное завершать работу
                }
            }
        }

        public void Send(byte[] inputData)
        {
            //больше пакетов отправлять нельзя. Ждём когда уже отправленные пакеты дойдут чтобы поместить в буфер этот
            if (packagesInfo.Count >= maxPackagesCount)
            {
                sendingCycleDetector.WaitOne();
            }

            semaphore.WaitOne(); //ждём когда поток отпарвки сделает свою работу

            int temp;
            //если в буфер не пуст
            if (packagesInfo.Count > 0)
            {
                temp = packagesInfo[packagesInfo.Count - 1][1] + inputData.Length + 3; // расчитываем размер, который получиться у итогового пакета который будем отправлять, если этот пакет засунуть в буфер

                //если этот размер меньше максимального
                if (temp <= maxPackageSize)
                {
                    sendingBuffer.Enqueue(inputData);

                    packagesInfo[packagesInfo.Count - 1][0]++;
                    packagesInfo[packagesInfo.Count - 1][1] = temp;
                }
                else
                {
                    //если размер этого пакета меньше максимального - суем его в буфер, но уже в следующий отправляемый пакет, ведь в этот он не вмещается
                    if (inputData.Length + 6 <= maxPackageSize)
                    {
                        packagesInfo.Add(new int[2] { 1, 6 });
                        sendingBuffer.Enqueue(inputData);
                        packagesInfo[packagesInfo.Count - 1][1] += inputData.Length;
                    }
                    else
                    {
                        //разбиваем пакет на части и добавляем в буфер
                    }
                }
            }
            else
            {
                packagesInfo.Add(new int[2] { 1, 6 });
                if (inputData.Length + 6 <= maxPackageSize)
                {
                    sendingBuffer.Enqueue(inputData);
                    packagesInfo[0][1] += inputData.Length;
                }
                else
                {
                    //разбиваем пакет на части и добавляем в буфер
                }
            }

            sendingWait.Set();
            semaphore.Release();
        }

        public bool Receive(out byte[] data)
        {
            if (packagesQueue.Count > 0)
            {
                packagesQueue.TryDequeue(out data);

                return true;
            }
            else //буфер пуст
            {
                while (isConnected)
                {
                    threadReset.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (packagesQueue.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        packagesQueue.TryDequeue(out data);

                        return true;
                    }
                }
            }

            data = new byte[0];
            return false; //если достигнута эта часть кода, значит serverWorkd стало равно false, что означает остановку сервера
        }
    }
}
