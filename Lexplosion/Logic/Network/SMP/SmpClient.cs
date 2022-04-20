using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    class SmpClient : IClientTransmitter
    {
        /* Я и из будующего, предлагаю тебе пойти нахуй, я не собираюсь комментировать код, мне лень */
        private struct DataFlags
        {
            public const byte None = 0;
            public const byte NotFull = 1;
        }

        private struct Flags
        {
            public const byte None = 0;
            public const byte NeedConfirm = 1;
        }

        private struct HeaderPositions
        {
            public const byte Code = 0; // Код пакета
            public const byte Id_1 = 1; // Первая часть айдишника
            public const byte Id_2 = 2; // Вторая часть
            public const byte Flag = 3; // Флаг пакета
            public const byte LastId_1 = 4; // id последнего пакета в данном окне. Первая часть
            public const byte LastId_2 = 5; // Вторая часть
            public const byte AttemptsCounts = 6; // Количество попыток отправки данного пакета

            public const byte FirstDataByte = AttemptsCounts + 1; // Позиция первого байта вне заголовка и совестно размер хэадера
        }

        private class Package
        {
            public List<byte[]> Segments = new List<byte[]>();
            public int Size;
            public bool lastSegmentIsFull = true;
        }

        public class Message
        {
            public byte[] data;
            public bool IsFull;
        }

        private readonly ConcurrentQueue<List<Package>> sendingBuffer = new ConcurrentQueue<List<Package>>(); // Буфер пакетов на отправку
        private readonly ConcurrentQueue<Message> receivingBuffer = new ConcurrentQueue<Message>();
        private readonly ConcurrentDictionary<ushort, byte[]> packagesBuffer = new ConcurrentDictionary<ushort, byte[]>(); //буфер принятых пакетов

        private readonly Semaphore sendBlock = new Semaphore(1, 1);
        private readonly AutoResetEvent waitSendData = new AutoResetEvent(false);
        private readonly AutoResetEvent deliveryWait = new AutoResetEvent(false);
        private readonly Semaphore repeatDeliveryBlock = new Semaphore(1, 1);
        private readonly Semaphore closeBlock = new Semaphore(1, 1);
        private readonly AutoResetEvent receiveWait = new AutoResetEvent(false);
        private readonly ManualResetEvent sendingCycleDetector = new ManualResetEvent(false);

        private ushort sendingPointer = 0;
        private ushort receivingPointer = 0;
        private int lastPackage = -1;
        private List<ushort> repeatDeliveryList = null;

        private int maxPackagesCount = 100;
        private long rtt = -1; // пинг в обе стороны (время ожидание ответа)
        private int mtu = 68; //68
        private int hostMtu = -1; // mtu удалённого хоста

        private IPEndPoint point = null;
        private readonly UdpClient socket = null;

        private Thread serviceSend;
        private Thread serviceReceive;
        private Thread connectionControl;

        private bool workPing = false; // когда работает метод, вычислящий rtt эта переменная становится true
        private readonly long[] times = new long[20]; // этот массив тоже нужен для метода вычисления пинга
        private long pingPackagesDelay = 0;
        private readonly AutoResetEvent pingWait = new AutoResetEvent(false);

        private readonly AutoResetEvent mtuWait = new AutoResetEvent(false); // ожидание ответа при вычислении mtu
        private int mtuPackageId = -1; // айди mtu пакета

        private readonly AutoResetEvent mtuInfoWait = new AutoResetEvent(false); // ожидание ответа при отправке своего mtu
        private readonly Semaphore calculateMtuBlock = new Semaphore(1, 1);

        public delegate void ReceiveHandle(bool isFull);
        public event ReceiveHandle MessageReceived;

        private bool inStopping = false; // это флаг чтобы в процессе закрытия соединения нельзя было вызвать метод send
        private long lastTime = 0; //время отправки последнего пакета
        private readonly int[] delayMultipliers = new int[15]
        { 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1 }; //этот массив хранит множители пинга протправке сообщений

        public bool IsConnected { get; private set; } = false;

        public bool WaitFullPackage { get; set; } = true;

        public long Ping
        {
            get
            {
                return rtt / 2;
            }
        }

        public SmpClient(IPEndPoint point, bool a = false)
        {
            socket = new UdpClient();
            if (a) socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Client.Bind(point);

            var sioUdpConnectionReset = -1744830452;
            var inValue = new byte[] { 0 };
            var outValue = new byte[] { 0 };
            socket.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
        }

        public SmpClient(UdpClient sock)
        {
            socket = sock;

            var sioUdpConnectionReset = -1744830452;
            var inValue = new byte[] { 0 };
            var outValue = new byte[] { 0 };
            socket.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
        }

        public bool Connect(IPEndPoint remoteIp)
        {
            socket.Connect(remoteIp);

            var thread = new Thread(delegate ()
            {
                byte[] data = null;

                while (!IsConnected)
                {
                    //try
                    {
                        data = socket.Receive(ref remoteIp);
                        if (data.Length > 0)
                        {
                            if (data[0] == 0x00 || data[0] == 0x01 || data[0] == 0x03 || data[0] == 0x02) //если пришел пинг, ответ на пинг, пакет данных или пакет с кодом вычисления mtu то isConnected делаем true
                            {
                                if (data[0] == 0x00 && data.Length > 2) // если это пакет с вычислением mtu - отвечаем на него
                                {
                                    socket.Send(new byte[2] { 0x07, data[1] }, 2);
                                }
                                else if (data[0] == 0x01) // если это пакет пинга то отвечаем на него
                                {
                                    socket.Send(new byte[2] { 0x02, data[1] }, 2);
                                }
                                else if (data[0] == 0x02) // если это ответ на пинг, то отвечаем на него
                                {
                                    PingProcessing(data);
                                }
                            }
                        }
                    }
                    //catch { }
                }

            });
            thread.Start();

            rtt = CalculateRTT(); //измеряем rtt
            Console.WriteLine("RTT " + rtt);

            if (rtt != -1) // если -1, значит ответные пакеты не дошли. Соединение установить не удалось
            {
                IsConnected = true;
                point = remoteIp;

                thread.Abort();
                serviceSend = new Thread(ServiceSend);
                serviceReceive = new Thread(ServiceReceive);
                connectionControl = new Thread(ConnectionControl);

                serviceReceive.Start();

                mtu = CalculateMTU(); // измеряем mtu
                SendMTUInfo(); // отправляем наш mtu хосту
                calculateMtuBlock.WaitOne();
                if (hostMtu != -1) // пакет с инфой об mtu хоста уже был получен
                {
                    // если mtu хоста меньше, то обновляем наш mtu
                    if (hostMtu < mtu)
                    {
                        mtu = hostMtu;
                    }
                }
                else
                {
                    hostMtu = -2; // устанавливаем -2 чтобы при получении пакета с инфой наш mtu был обновлён
                }
                calculateMtuBlock.Release();

                socket.Client.ReceiveBufferSize = maxPackagesCount * mtu;

                serviceSend.Start();
                connectionControl.Start();

                lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000;

                Console.WriteLine("MTU " + mtu);

                return true;
            }
            else
            {
                return false;
            }
        }

        private int CalculateMTU()
        {
            socket.Client.DontFragment = true;

            int thisData = 10;
            int lostData = 1500;

            byte packageId = 0;
            int difference = 1490;
            while (difference > 1)
            {
                difference = lostData - thisData;

                mtuPackageId = packageId;
                byte[] data = new byte[thisData];
                data[1] = packageId;

                int j;
                for (j = 0; j < 5; j++) // пробуем отправить 5 раз
                {
                    try
                    {
                        socket.Send(data, thisData);

                        if (mtuWait.WaitOne((int)rtt * 2) && mtuPackageId == packageId)
                        {
                            break;
                        }
                    }
                    catch { }
                }

                if (j == 5) // пакет не дошёл 
                {
                    // TODO: если первый пакет не дойдёт, то наверное закрывать соединение
                    int thisData_ = thisData;
                    thisData -= (difference / 2) + (difference % 2);
                    lostData = thisData_;
                }
                else // покет дошёл
                {
                    thisData += difference / 2;
                }

                packageId++;
            }

            mtuPackageId = -1;

            socket.Client.DontFragment = false;
            return thisData;
        }

        private void SendMTUInfo()
        {
            byte[] payload = BitConverter.GetBytes((ushort)mtu);
            byte[] data = new byte[3];

            Array.Copy(payload, 0, data, 1, 2);
            data[0] = 0x08;

            for (int j = 0; j < 5; j++) // пробуем отправить 5 раз
            {
                try
                {
                    socket.Send(data, data.Length);

                    if (mtuInfoWait.WaitOne((int)rtt))
                    {
                        break;
                    }
                }
                catch { }
            }
        }

        private long CalculateRTT()
        {
            long rttSum = 0;

            byte i = 0;
            for (int j = 0; j < 5; j++) // сий процесс повторяем 5 раз
            {
                workPing = true;

                bool successful = false;
                while (!successful && i < 20)
                {
                    times[i] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    Console.WriteLine("SEND");
                    socket.Send(new byte[2] { 0x01, i }, 2);
                    i++;

                    successful = pingWait.WaitOne(200);
                }

                if (!successful)
                {
                    return -1;
                }

                rttSum += pingPackagesDelay;
            }

            Console.WriteLine("RTT " + ((rttSum / 5) + 1));

            // вычиляем среднее значение и возвращаем его
            return (rttSum / 5) + 1;
        }


        private void PingProcessing(byte[] data)
        {
            if (data.Length == 2 && data[1] < 21 && workPing)
            {
                pingPackagesDelay = DateTimeOffset.Now.ToUnixTimeMilliseconds() - times[data[1]]; //вчитаем из данного времени время отправки пакета
                workPing = false;
                pingWait.Set();
            }
        }

        private void ConnectionControl() //метод работающий всегда. контролирует доставку пакетов
        { // TODO: потом на сервере этот метод как-то занести в один поток для всех клиентов
            while (IsConnected)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTime >= 10000) //проверяем что последний пакет был отправлен более 2 секунд назад
                {
                    if (CalculateRTT() == -1) //проверяем ответил ли хост
                    {
                        Console.WriteLine("ConnectionControl");
                        StopWork();
                        ClientClosing?.Invoke(point);
                    }
                }

                Thread.Sleep(10000);
            }
        }

        private void ServiceSend()
        {
            while (IsConnected)
            {
                sendingCycleDetector.Reset();
                // ждём появления сообщений в буфере
                while (sendingBuffer.Count == 0)
                {
                    waitSendData.WaitOne();
                }

                sendBlock.WaitOne();

                var packages = new SortedDictionary<ushort, byte[]>();
                sendingBuffer.TryPeek(out List<Package> packagesHeap); // получаем кучу пакетов

                int lastPackageId_ = packagesHeap.Count + sendingPointer - 1;
                ushort lastPackageId = (ushort)(lastPackageId_ > 65535 ? 65535 : lastPackageId_); // id последнего пакета в этом сегменте отправки
                int i = 0;
                // проходимся по всем пакетам
                foreach (Package packageInfo in packagesHeap)
                {
                    byte[] package = new byte[packageInfo.Size];

                    byte[] id = BitConverter.GetBytes(sendingPointer);
                    package[HeaderPositions.Code] = 0x03; //код пакета
                    package[HeaderPositions.Id_1] = id[0]; //первая часть его айдишника
                    package[HeaderPositions.Id_2] = id[1]; //вторая
                    id = BitConverter.GetBytes(lastPackageId);
                    package[HeaderPositions.LastId_1] = id[0]; // первая часть id последнего пакета
                    package[HeaderPositions.LastId_2] = id[1]; // вторая часть
                    package[HeaderPositions.AttemptsCounts] = 0; // этот байт отвечает за номер попытки отправки

                    int offset = HeaderPositions.FirstDataByte;
                    int lastFlagIndex = 0;
                    // проходимся по каждому сегменту
                    foreach (byte[] payload in packageInfo.Segments)
                    {
                        package[offset] = DataFlags.None; // это флаг данного сегмента данных. 0 - значит нихуя не делать
                        lastFlagIndex = offset;

                        int payloadSize = payload.Length;
                        byte[] size = BitConverter.GetBytes(payloadSize);
                        package[offset + 1] = size[0]; // первая часть размера сегмента данных
                        package[offset + 2] = size[1]; // вторая часть
                        offset += 3;

                        Array.Copy(payload, 0, package, offset, payloadSize);
                        offset += payloadSize;
                    }

                    if (!packageInfo.lastSegmentIsFull) // последний сегмент данных не полный и надо поставить флаг что его необходимо склеить
                    {
                        package[lastFlagIndex] = DataFlags.NotFull;
                    }

                    packages[sendingPointer] = package;
                    sendingPointer++;
                    i++;

                    if (sendingPointer == 0)
                    {
                        break;
                    }
                }

                if (packagesHeap.Count == i) // если все пакеты из кучи были поставлены на отправку, то убираем эту кучу из буфера
                {
                    sendingBuffer.TryDequeue(out _);
                }
                else // если нет, то тогда убираем из кучи поставленные на отправку пакеты. Оставшиеся пакеты отправим на следующей итерации
                {
                    for (int j = 0; j < i; j++)
                    {
                        packagesHeap.RemoveAt(0);
                    }
                }

                sendBlock.Release();

                repeatDeliveryBlock.WaitOne();
                lastPackage = lastPackageId;
                deliveryWait.Reset();
                repeatDeliveryBlock.Release();

                byte attemptCounts = 0;
                int delay = (int)(rtt * packages.Count);
                // цикл отправки
                while (IsConnected && attemptCounts < 15)
                {
                    if (attemptCounts > 0)
                    {
                        Console.WriteLine("AXAXAXAXAXAX " + attemptCounts + " " + lastPackageId);
                    }
                    foreach (ushort id in packages.Keys)
                    {
                        packages[id][HeaderPositions.AttemptsCounts] = attemptCounts; // увставляем номер попытки
                        socket.Send(packages[id], packages[id].Length);
                    }

                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    Begin:
                    if (!deliveryWait.WaitOne(delay)) // истекло время ожидания
                    {
                        delay *= delayMultipliers[attemptCounts];
                        attemptCounts++;
                    }
                    else // либо пришло подтверждение доставки, либо пришел запрос на повторную доставку
                    {
                        repeatDeliveryBlock.WaitOne();
                        if (repeatDeliveryList == null) // пакеты удачно доставлены
                        {
                            //Console.WriteLine("YRAAAAA " + lastPackageId);
                            repeatDeliveryBlock.Release();
                            break;
                        }
                        else // хост просит повторить отправку некоторых пакетов
                        {
                            // оставляем в списке только те айдишники, которые надо повторить
                            SortedDictionary<ushort, byte[]> packages_ = new SortedDictionary<ushort, byte[]>();
                            bool isValid = true;
                            ushort maxId = 0;
                            foreach (ushort repeatId in repeatDeliveryList)
                            {
                                if (packages.ContainsKey(repeatId))
                                {
                                    maxId = repeatId;
                                    packages_[repeatId] = packages[repeatId];
                                }
                                else
                                {
                                    isValid = false;
                                    break;
                                }
                            }

                            if (isValid)
                            {
                                packages_[maxId][HeaderPositions.Flag] = Flags.NeedConfirm;

                                packages = packages_;
                                repeatDeliveryList = null;
                                repeatDeliveryBlock.Release();
                            }
                            else
                            {
                                repeatDeliveryBlock.Release();
                                goto Begin;
                            }
                        }
                    }
                }

                if (attemptCounts == 15)
                {
                    Console.WriteLine("PIZDETS!!!!");
                    new Thread(delegate ()
                    {
                        Close();
                    }).Start();
                }

                lastPackage = -1;

                sendingCycleDetector.Set();
            }
        }

        private void ServiceReceive()
        {
            void addPackage(byte[] package)
            {
                int offset = HeaderPositions.FirstDataByte;
                while (offset < package.Length - 3)
                {
                    byte flag = package[offset];
                    ushort size = BitConverter.ToUInt16(new byte[2] { package[offset + 1], package[offset + 2] }, 0);
                    offset += 3;

                    byte[] payload = new byte[size];
                    Array.Copy(package, offset, payload, 0, size);
                    offset += size;

                    bool isFull = (flag == DataFlags.None);


                    receivingBuffer.Enqueue(new Message
                    {
                        data = payload,
                        IsFull = isFull
                    });

                    MessageReceived?.Invoke(isFull);
                }
            }

            byte[] data;
            int waitingLastPackage = -1;
            int attemptSendCounts = -1;
            ushort lastMissedPacket = 0;

            while (IsConnected)
            {
                try
                {
                    data = socket.Receive(ref point);
                }
                catch
                {
                    break;
                }

                if (data.Length > 0)
                {
                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    switch (data[0])
                    {
                        case 0: // пришел пакет с вычислением mtu
                            if (data.Length > 2)
                            {
                                socket.Send(new byte[2] { 0x07, data[1] }, 2);
                            }
                            break;
                        case 1: //пришел пакет с пингом
                            if (data.Length == 2)
                            {
                                socket.Send(new byte[2] { 0x02, data[1] }, 2);
                            }
                            break;
                        case 2: //пришел ответ на пинг
                            PingProcessing(data);
                            break;
                        case 3: //пришел пакет данных
                            if (data.Length > 5)
                            {
                                ushort id = BitConverter.ToUInt16(new byte[2] {
                                    data[HeaderPositions.Id_1],
                                    data[HeaderPositions.Id_2]
                                }, 0);

                                ushort lastId = BitConverter.ToUInt16(new byte[2] {
                                    data[HeaderPositions.LastId_1],
                                    data[HeaderPositions.LastId_2]
                                }, 0);

                                //Console.WriteLine("RECV " + receivingPointer + " " + id + " " + lastId);

                                if (id >= receivingPointer && id - receivingPointer < maxPackagesCount)
                                {
                                    waitingLastPackage = lastId;

                                    if (id == receivingPointer)
                                    {
                                        addPackage(data);
                                        packagesBuffer.TryRemove(receivingPointer, out _);
                                        receiveWait.Set();

                                        receivingPointer++;
                                    }
                                    else
                                    {
                                        packagesBuffer[id] = data;
                                    }

                                    // проходимся по буферу в поисках пакетов, которые мы уже получили. Если пакет найден - пихаем  в буфер
                                    while (packagesBuffer.ContainsKey(receivingPointer))
                                    {
                                        addPackage(packagesBuffer[receivingPointer]);
                                        packagesBuffer.TryRemove(receivingPointer, out _);
                                        receiveWait.Set();

                                        receivingPointer++;
                                    }

                                    // проверяем все ли пакеты были получены
                                    if (receivingPointer == (ushort)(lastId + 1))
                                    {
                                        // отправляем подтверждение
                                        byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);
                                        socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);
                                        //Console.WriteLine("SUCS " + id + " " + receivingPointer);
                                        attemptSendCounts = -1;
                                        waitingLastPackage = -1;
                                    }
                                    else
                                    {
                                        bool needRepeat = false;
                                        for (int i = receivingPointer; i <= lastId; i++)
                                        {
                                            if (packagesBuffer.ContainsKey((ushort)i))
                                            {
                                                needRepeat = true;
                                                break;
                                            }
                                        }

                                        if (needRepeat)
                                        {
                                            var package = new List<byte>
                                            {
                                                0x06,
                                                data[HeaderPositions.LastId_1],
                                                data[HeaderPositions.LastId_2]
                                            };

                                            bool flag = true;
                                            for (int i = receivingPointer; i <= lastId; i++)
                                            {
                                                if (!packagesBuffer.ContainsKey((ushort)i))
                                                {
                                                    if (i == lastMissedPacket && data[HeaderPositions.AttemptsCounts] <= attemptSendCounts)
                                                    {
                                                        needRepeat = false;
                                                        break;
                                                    }

                                                    if (flag)
                                                    {
                                                        lastMissedPacket = (ushort)i;
                                                        flag = false;
                                                    }

                                                    byte[] packageId = BitConverter.GetBytes((ushort)i);
                                                    package.Add(packageId[0]);
                                                    package.Add(packageId[1]);
                                                }
                                            }

                                            if (needRepeat)
                                            {
                                                byte[] array = package.ToArray();
                                                socket.Send(array, array.Length);
                                                string str = "";
                                                for (int h = 3; h < array.Length - 1; h += 2)
                                                {
                                                    var idg = BitConverter.ToUInt16(new byte[2] { array[h], array[h + 1] }, 0);
                                                    str += idg + ", ";
                                                }
                                                //Console.WriteLine("RETAT 1 " + str);
                                            }

                                            attemptSendCounts = data[HeaderPositions.AttemptsCounts];
                                        }
                                    }
                                }
                                else
                                {
                                    if (waitingLastPackage == lastId && data[HeaderPositions.AttemptsCounts] > attemptSendCounts)
                                    {
                                        var package = new List<byte> {
                                            0x06,
                                            data[HeaderPositions.LastId_1],
                                            data[HeaderPositions.LastId_2]
                                        };

                                        for (int i = receivingPointer; i <= lastId; i++)
                                        {
                                            if (!packagesBuffer.ContainsKey((ushort)i))
                                            {
                                                byte[] packageId = BitConverter.GetBytes((ushort)i);
                                                package.Add(packageId[0]);
                                                package.Add(packageId[1]);
                                            }
                                        }

                                        if (package.Count > 3)
                                        {
                                            byte[] array = package.ToArray();
                                            socket.Send(array, array.Length);
                                        }
                                        else
                                        {
                                            byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);
                                            socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);
                                        }

                                        attemptSendCounts = data[HeaderPositions.AttemptsCounts];
                                    }
                                    else if (id == lastId || data[HeaderPositions.Flag] == Flags.NeedConfirm)
                                    {
                                        byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);
                                        socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);
                                        //Console.WriteLine("SUCS0 " + id +  " " + lastId + " " + receivingPointer);
                                    }
                                }

                            }
                            break;
                        case 4: // пришло подтверждение доставки пакета
                            if (data.Length == 3)
                            {
                                ushort id = BitConverter.ToUInt16(new byte[2] {
                                    data[1],
                                    data[2]
                                }, 0);

                                repeatDeliveryBlock.WaitOne();
                                if (id == lastPackage)
                                {
                                    //Console.WriteLine("PODV " + id);
                                    repeatDeliveryList = null;
                                    deliveryWait.Set();
                                }
                                repeatDeliveryBlock.Release();
                            }

                            break;
                        case 5: // обрыв соединения
                            Console.WriteLine("StopWork!!!!");
                            new Thread(delegate ()
                            {
                                StopWork();
                                ClientClosing?.Invoke(point);
                            }).Start();
                            break;
                        case 6: // пришел пакет со списком пакетов, которые нужно переотправить
                                //проверяем валидность этого пакета. пакет должен содержать список айдишников. каждый id занимает 2 байта. первый байт - код.
                            if (data.Length > 3 && ((data.Length - 1) % 2 == 0))
                            {
                                repeatDeliveryBlock.WaitOne();
                                ushort packageId = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                if (packageId == lastPackage) // проверяем не старый ли это запрос на повторную отправку
                                {
                                    List<ushort> ids = new List<ushort>();
                                    int i = 3;
                                    while (i < data.Length)
                                    {
                                        ushort id = BitConverter.ToUInt16(new byte[2] { data[i], data[i + 1] }, 0);
                                        ids.Add(id);
                                        i += 2;
                                    }

                                    repeatDeliveryList = ids;
                                    deliveryWait.Set();
                                }
                                repeatDeliveryBlock.Release();
                            }
                            break;
                        case 7: // пришел ответ на вычисление mtu
                            if (data.Length == 2)
                            {
                                mtuPackageId = data[1];
                                mtuWait.Set();
                            }
                            break;
                        case 8: // пришёл пакет с инфой об mtu
                            if (data.Length == 3)
                            {
                                ushort hostMtu_ = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                socket.Send(new byte[1] { 0x09 }, 1);

                                calculateMtuBlock.WaitOne();
                                if (hostMtu == -1) // метод Connect ещё не отработал
                                {
                                    hostMtu = hostMtu_;
                                }
                                else // connect уже отработал, можно обновлять mtu
                                {
                                    // если mtu, отправленный хостом меньше, который вычислили мы, то обновляем его
                                    if (hostMtu_ < mtu)
                                    {
                                        mtu = hostMtu_;
                                    }
                                }
                                calculateMtuBlock.Release();
                            }
                            break;
                        case 9: // пришёл ответ на пакет с инфой об mtu
                            mtuInfoWait.Set();
                            break;

                    }
                }
            }
        }

        public void Send(byte[] inputData)
        {
            if (inStopping) // TODO: думаю конкретно в лаунчере это можно убрать, а вообще в протоколе оставить надо
            {
                return;
            }

            Begin: sendBlock.WaitOne();

            int _mtu = mtu;
            int _maxPackagesCount = maxPackagesCount;

            List<Package> packages;
            if (sendingBuffer.Count > 0)
            {
                if (sendingBuffer.Count > 1)
                {
                    sendBlock.Release();
                    sendingCycleDetector.WaitOne();
                    goto Begin;
                }

                sendingBuffer.TryPeek(out packages);
            }
            else
            {
                packages = new List<Package>();
                sendingBuffer.Enqueue(packages);
            }

            void createPackage(byte[] inputData_)
            {
                if (inputData_.Length + 10 <= _mtu)
                {
                    Package package = new Package
                    {
                        Size = inputData_.Length + 10
                    };
                    package.Segments.Add(inputData_);
                    packages.Add(package);
                }
                else
                {
                    int offset = 0;
                    while (offset < inputData_.Length)
                    {
                        int lenght = (inputData_.Length - offset) > (_mtu - 10) ? _mtu - 10 : inputData_.Length - offset;
                        byte[] part = new byte[lenght];
                        Array.Copy(inputData_, offset, part, 0, lenght);

                        Package package = new Package
                        {
                            Size = lenght + 10,
                            lastSegmentIsFull = false
                        };
                        package.Segments.Add(part);

                        if (packages.Count >= _maxPackagesCount)
                        {
                            packages = new List<Package>();
                            sendingBuffer.Enqueue(packages);
                        }

                        packages.Add(package);

                        offset += lenght;
                    }

                    packages[packages.Count - 1].lastSegmentIsFull = true;
                }
            }

            if (packages.Count <= _maxPackagesCount)
            {
                if (packages.Count > 0)
                {
                    Package lastElement = packages[packages.Count - 1];
                    if (lastElement.Size + inputData.Length + 3 <= _mtu)
                    {
                        lastElement.Segments.Add(inputData);
                        lastElement.Size += inputData.Length + 3;
                    }
                    else
                    {
                        if (packages.Count < _maxPackagesCount)
                        {
                            int freeSpace = _mtu - lastElement.Size - 3;
                            if (freeSpace > 0)
                            {
                                byte[] part = new byte[freeSpace];
                                Array.Copy(inputData, 0, part, 0, freeSpace);

                                lastElement.Segments.Add(part);
                                lastElement.Size += freeSpace + 3;
                                lastElement.lastSegmentIsFull = false;

                                int partSize = inputData.Length - freeSpace;
                                part = new byte[partSize];
                                Array.Copy(inputData, freeSpace, part, 0, partSize);

                                createPackage(part);
                            }
                            else
                            {
                                createPackage(inputData);
                            }
                        }
                        else
                        {
                            sendBlock.Release();
                            sendingCycleDetector.WaitOne();
                            goto Begin;
                        }
                    }
                }
                else
                {
                    createPackage(inputData);
                }
            }
            else
            {
                sendBlock.Release();
                sendingCycleDetector.WaitOne();
                goto Begin;
            }

            waitSendData.Set();
            sendBlock.Release();
        }

        public bool Receive(out byte[] data)
        {
            void FormingData(out byte[] _data)
            {
                List<byte[]> buffer = new List<byte[]>();
                int messageSize = 0;

                receivingBuffer.TryDequeue(out Message segment);
                buffer.Add(segment.data);
                messageSize += segment.data.Length;

                while (!segment.IsFull && WaitFullPackage && (IsConnected || receivingBuffer.Count > 0))
                {
                    if (receivingBuffer.Count > 0)
                    {
                        receivingBuffer.TryDequeue(out segment);
                        buffer.Add(segment.data);
                        messageSize += segment.data.Length;
                    }
                    else
                    {
                        receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты
                    }
                }

                _data = new byte[messageSize];
                int offset = 0;
                foreach (byte[] segmentBytes in buffer)
                {
                    int len = segmentBytes.Length;
                    Array.Copy(segmentBytes, 0, _data, offset, len);
                    offset += len;
                }
            }

            if (receivingBuffer.Count > 0)
            {
                FormingData(out data);
                return true;
            }
            else //буфер пуст
            {
                while (IsConnected)
                {
                    receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (receivingBuffer.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        FormingData(out data);
                        return true;
                    }
                }
            }

            data = null;
            return false;
        }

        private void StopWork()
        {
            IsConnected = false;
            connectionControl.Abort();
            //serviceReceive.Abort();
            serviceSend.Abort();
            socket.Close();
            receiveWait.Set();
        }

        public void Close()
        {
            closeBlock.WaitOne();
            try
            {
                if (IsConnected)
                {
                    sendBlock.WaitOne();
                    inStopping = true; // ставим флаг чтобы send нельзя было вызвать ещё раз
                    sendBlock.Release();

                    while (sendingBuffer.Count != 0) // ждём когда все пакеты из буфера будут доставлены
                    {
                        sendingCycleDetector.WaitOne();
                    }

                    for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                    {
                        socket.Send(new byte[1] { 0x05 }, 1); // TODO: было исключение доступ к ликвидированному объекту запрещен
                    }

                    StopWork();
                }
            }
            catch { }
            closeBlock.Release();
        }

        public event PointHandle ClientClosing;
    }
}