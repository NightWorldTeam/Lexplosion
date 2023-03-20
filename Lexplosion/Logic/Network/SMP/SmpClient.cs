using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{

    /// <summary>
    /// Клиент Stream Messages Protocol. Работает на udp. 
    /// Устанавливает соединение, контролириует доставку пакетов. Заточен для работы с Udp hole pushing.
    /// Может соединяться только с одним хостом.
    /// </summary>
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

        /// <summary>
        /// Позиции в заголовке пакета данных.
        /// </summary>
        private struct HeaderPositions
        {
            public const byte Code = 0; // Код пакета
            public const byte Id_1 = 1; // Первая часть айдишника
            public const byte Id_2 = 2; // Вторая часть
            public const byte Flag = 3; // Флаг пакета
            public const byte LastId_1 = 4; // id последнего пакета в данном окне. Первая часть
            public const byte LastId_2 = 5; // Вторая часть
            public const byte AttemptsCounts = 6; // Количество попыток отправки данного пакета

            public const byte FirstDataByte = AttemptsCounts + 1; // Позиция первого байта вне заголовка и соотвественно размер ебучего хэадера
        }

        private struct PackgeCodes
        {
            public const byte MtuRequest = 0x00;
            public const byte PingRequest = 0x01;
            public const byte PingResponse = 0x02;
            public const byte Data = 0x03;
            public const byte ConfirmDataDelivery = 0x04;
            public const byte ConnectionClose = 0x05;
            public const byte FailedList = 0x06;
            public const byte MtuResponse = 0x07;
            public const byte MtuInfo = 0x08;
            public const byte MtuInfoConfirm = 0x09;
            public const byte ConnectRequest = 0x0a;
            public const byte ConnectAnswer = 0x0b;
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

        private struct RttCalculator
        {
            private const int DeltesCount = 25;
            private long[] deltes;

            public RttCalculator(long firstRtt)
            {
                deltes = new long[DeltesCount];

                for (int i = 0; i < DeltesCount; i++)
                {
                    deltes[i] = firstRtt;
                }

                _rtt = firstRtt;
            }

            public void AddDelta(long delta)
            {
                long maxDelta = 0;
                long maxDelta2 = 0;
                for (int i = 0; i < DeltesCount - 1; i++)
                {
                    long nextDelta = deltes[i + 1];
                    deltes[i] = nextDelta;

                    if (nextDelta > maxDelta)
                    {
                        maxDelta2 = maxDelta;
                        maxDelta = nextDelta;
                    }
                }

                deltes[DeltesCount - 1] = delta;
                if (delta > maxDelta) maxDelta = delta;

                if (maxDelta < _rtt)
                {
                    long div = _rtt - maxDelta;
                    double multiplier = maxDelta2 / maxDelta;
                    _rtt -= Convert.ToInt64(div * (1 - multiplier));
                }
                else
                {
                    _rtt = maxDelta;
                }
            }

            private long _rtt;
            public long GetRtt
            {
                get => _rtt;
            }
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

        private int _maxPackagesCount = 100;
        private long _rtt = -1; // пинг в обе стороны (время ожидание ответа)
        private int _mtu = 68; // максимальный размер пакета
        private int _hostMtu = -1; // mtu удалённого хоста
        private byte _selfSessionId = 0; // наш id сессии. Его мы задаем при подключении и будем отправлять при отключении
        private byte _hostSessionId = 0; //id сесии хоста. Его мы будем проверять, если хост отправит запрос на отключение

        private RttCalculator _rttCalculator;

        private IPEndPoint point = null;
        private readonly UdpClient socket = null;

        private Thread serviceSend;
        private Thread serviceReceive;
        private Thread connectionControl;

        private bool _workPing = false; // когда работает метод, вычислящий rtt эта переменная становится true
        private readonly long[] _times = new long[20]; // этот массив тоже нужен для метода вычисления пинга
        private long _pingPackagesDelay = 0;
        private readonly AutoResetEvent _pingWait = new AutoResetEvent(false);

        private readonly AutoResetEvent _mtuWait = new AutoResetEvent(false); // ожидание ответа при вычислении mtu
        private int _mtuPackageId = -1; // айди mtu пакета

        private readonly AutoResetEvent _mtuInfoWait = new AutoResetEvent(false); // ожидание ответа при отправке своего mtu
        private readonly Semaphore _calculateMtuBlock = new Semaphore(1, 1);

        public event Action MessageReceived;

        private bool _inStopping = false; // это флаг чтобы в процессе закрытия соединения нельзя было вызвать метод send
        private long _lastTime = 0; //время отправки последнего пакета
        private readonly int[] _delayMultipliers = new int[15] //этот массив хранит множители rtt при отправке сообщений.
        { 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1 };      //Ключ - номер неуспешной попытки отправки. Будет сипользовано на следующий итерации.
                                                              //То есть если на нулевой итеарции не получилось доставить, то rtt будет домножен на нулвой множитель и полученное число будет использоваться как задержка на первой итерации.

        private byte[] _connectAnswerPackage;

        public bool IsConnected { get; private set; } = false;

        public long Ping
        {
            get
            {
                return _rtt / 2;
            }
        }

        public SmpClient(IPEndPoint point)
        {
            socket = new UdpClient();
            socket.Client.Bind(point);

            var sioUdpConnectionReset = -1744830452;
            var inValue = new byte[] { 0 };
            var outValue = new byte[] { 0 };
            socket.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
        }

        public bool Connect(IPEndPoint remoteIp, byte[] connectCode)
        {
            var connectionWait = new AutoResetEvent(false);

            _selfSessionId = (byte)(new Random()).Next(0, 0xff);

            byte[] _connectAnswerPackage = new byte[connectCode.Length + 3];
            _connectAnswerPackage[0] = PackgeCodes.ConnectAnswer;
            _connectAnswerPackage[1] = (byte)connectCode.Length; //вставлям размер кода подключения. Он не может быть больше 256 байт
            _connectAnswerPackage[_connectAnswerPackage.Length - 1] = _selfSessionId; //вставляем наш id сессии.
            Array.Copy(connectCode, 0, _connectAnswerPackage, 2, (byte)connectCode.Length); //копируем код подключения в пакет подключения

            var thread = new Thread(delegate ()
            {
                byte[] data = null;
                bool pointDefined = false;

                while (!IsConnected)
                {
                    try
                    {
                        // TODO: не просто принимать все поинты, а проверять ip игнорируя порт
                        IPEndPoint senderPoint = null;
                        data = socket.Receive(ref senderPoint);
                        if (data.Length > 0)
                        {
                            if (data[0] == PackgeCodes.MtuRequest && data.Length > 2) // если это пакет с вычислением mtu - отвечаем на него
                            {
                                if (pointDefined || senderPoint?.Equals(remoteIp) == true)
                                    socket.Send(new byte[2] { PackgeCodes.MtuResponse, data[1] }, 2);
                            }
                            else if (data[0] == PackgeCodes.PingRequest) // если это пакет пинга то отвечаем на него
                            {
                                if (pointDefined || senderPoint?.Equals(remoteIp) == true)
                                    socket.Send(new byte[2] { PackgeCodes.PingResponse, data[1] }, 2);
                            }
                            else if (data[0] == PackgeCodes.PingResponse) // если это ответ на пинг, то обрабатываем его
                            {
                                if (pointDefined)
                                    PingProcessing(data);
                            }
                            else if ((data[0] == PackgeCodes.ConnectRequest || data[0] == PackgeCodes.ConnectAnswer) && data.Length > 3)
                            {
                                byte codeSize = data[1];
                                if (codeSize + 3 == data.Length)
                                {
                                    byte[] recivedCode = new byte[codeSize];
                                    Array.Copy(data, 2, recivedCode, 0, codeSize);
                                    if (connectCode.SequenceEqual(recivedCode))
                                    {
                                        if (!pointDefined)
                                        {
                                            remoteIp = senderPoint;
                                            pointDefined = true;
                                            _hostSessionId = data[data.Length - 1]; // устанавливаем id сессии хоста
                                            Runtime.DebugWrite("_hostSessionId " + _hostSessionId);
                                            connectionWait.Set();
                                        }

                                        if (data[0] == PackgeCodes.ConnectRequest)
                                            socket.Send(_connectAnswerPackage, _connectAnswerPackage.Length, senderPoint);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            });
            thread.Start();

            //формируем пакет запроса подключения
            byte[] connectPackage = new byte[connectCode.Length + 3];
            connectPackage[0] = PackgeCodes.ConnectRequest;
            connectPackage[1] = (byte)connectCode.Length; //вставлям размер кода подключения. Он не может быть больше 256 байт
            connectPackage[connectPackage.Length - 1] = _selfSessionId; //херачим id для сессии.
            Array.Copy(connectCode, 0, connectPackage, 2, (byte)connectCode.Length); //копируем код подключения в пакет подключения

            int i = 0;
            bool successfulConnect = false;
            while (!successfulConnect && i < 20)
            {
                socket.Send(connectPackage, connectPackage.Length, remoteIp);
                i++;
                successfulConnect = connectionWait.WaitOne(200);
            }

            if (!successfulConnect)
            {
                Runtime.DebugWrite("Point error");
                return false;
            }
            Runtime.DebugWrite("Point is defined");

            socket.Connect(remoteIp);

            _rtt = CalculateRTT(); //измеряем rtt
            _rttCalculator = new RttCalculator(_rtt);
            Runtime.DebugWrite("RTT " + _rtt);

            if (_rtt != -1) // если -1, значит ответные пакеты не дошли. Соединение установить не удалось
            {
                IsConnected = true;
                point = remoteIp;

                thread.Abort();
                serviceSend = new Thread(ServiceSend);
                serviceReceive = new Thread(ServiceReceive);
                connectionControl = new Thread(ConnectionControl);

                _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10000;

                serviceReceive.Start();
                serviceSend.Start();
                connectionControl.Start();

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    _mtu = CalculateMTU(); // измеряем mtu
                    SendMTUInfo(); // отправляем наш mtu хосту
                    _calculateMtuBlock.WaitOne();
                    if (_hostMtu != -1) // пакет с инфой об mtu хоста уже был получен
                    {
                        // если mtu хоста меньше, то обновляем наш mtu
                        if (_hostMtu < _mtu)
                        {
                            _mtu = _hostMtu;
                        }
                    }
                    else
                    {
                        _hostMtu = -2; // устанавливаем -2 чтобы при получении пакета с инфой наш mtu был обновлён
                    }
                    _calculateMtuBlock.Release();

                    socket.Client.ReceiveBufferSize = _maxPackagesCount * _mtu;

                    Runtime.DebugWrite("MTU " + _mtu);
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        private int CalculateMTU()
        {
            int thisData = 10;
            int lostData = 1500;

            byte packageId = 0;
            int difference = 1490;
            while (difference > 1 && thisData > 0)
            {
                difference = lostData - thisData;

                _mtuPackageId = packageId;
                byte[] data = new byte[thisData];
                data[0] = PackgeCodes.MtuRequest;
                data[1] = packageId;

                int j;
                for (j = 0; j < 5; j++) // пробуем отправить 5 раз
                {
                    try
                    {
                        socket.Client.DontFragment = true;
                        socket.Send(data, thisData);
                        socket.Client.DontFragment = false;

                        if (_mtuWait.WaitOne((int)_rtt * 2) && _mtuPackageId == packageId)
                        {
                            break;
                        }
                    }
                    catch { }
                }

                if (j == 5 || thisData < 1) // пакет не дошёл 
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

            _mtuPackageId = -1;

            return thisData;
        }

        private void SendMTUInfo()
        {
            byte[] payload = BitConverter.GetBytes((ushort)_mtu);
            byte[] data = new byte[3];

            Array.Copy(payload, 0, data, 1, 2);
            data[0] = PackgeCodes.MtuInfo;

            for (int j = 0; j < 5; j++) // пробуем отправить 5 раз
            {
                try
                {
                    socket.Send(data, data.Length);

                    if (_mtuInfoWait.WaitOne((int)_rtt))
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

            try
            {
                byte i = 0;
                for (int j = 0; j < 5; j++) // сий процесс повторяем 5 раз
                {
                    _workPing = true;

                    bool successful = false;
                    while (!successful && i < 20)
                    {
                        _times[i] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        Runtime.DebugWrite("SEND");
                        socket.Send(new byte[2] { PackgeCodes.PingRequest, i }, 2);
                        i++;

                        successful = _pingWait.WaitOne(200);
                    }

                    if (!successful)
                    {
                        return -1;
                    }

                    rttSum += _pingPackagesDelay;
                }
            }
            catch
            {
                return -1;
            }

            Runtime.DebugWrite("RTT " + ((rttSum / 5) + 1));

            // вычиляем среднее значение и возвращаем его
            return (rttSum / 5) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PingProcessing(byte[] data)
        {
            if (data.Length == 2 && data[1] < 21 && _workPing)
            {
                _pingPackagesDelay = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _times[data[1]]; //вчитаем из данного времени время отправки пакета
                _workPing = false;
                _pingWait.Set();
            }
        }

        private void ConnectionControl() //метод работающий всегда. контролирует доставку пакетов
        { // TODO: потом на сервере этот метод как-то занести в один поток для всех клиентов
            while (IsConnected)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastTime >= 10000) //проверяем что последний пакет был отправлен более 2 секунд назад
                {
                    if (CalculateRTT() == -1) //проверяем ответил ли хост
                    {
                        Runtime.DebugWrite("ConnectionControl");
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

                try
                {
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
                        package[HeaderPositions.Code] = PackgeCodes.Data; //код пакета
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

                    byte attemptCount = 0;
                    int delay = (int)(_rtt + _rtt / 10);
                    long lastTime = 0;
                    bool repeated = false;

                    // цикл отправки
                    while (IsConnected && attemptCount < 15)
                    {
#if DEBUG
                        if (attemptCount > 0)
                        {
                            Runtime.DebugWrite("AXAXAXAXAXAX " + attemptCount + " " + lastPackageId + ", RTT " + _rtt);
                        }
#endif
                        foreach (ushort id in packages.Keys)
                        {
                            packages[id][HeaderPositions.AttemptsCounts] = attemptCount; // увставляем номер попытки
                            socket.Send(packages[id], packages[id].Length);
                        }

                        _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        if (attemptCount == 0 || repeated)
                        {
                            lastTime = _lastTime;
                            repeated = false;
                        }

                    Begin:
                        if (!deliveryWait.WaitOne(delay)) // истекло время ожидания
                        {
                            delay *= _delayMultipliers[attemptCount];
                            attemptCount++;
                        }
                        else // либо пришло подтверждение доставки, либо пришел запрос на повторную доставку
                        {
                            repeatDeliveryBlock.WaitOne();
                            if (repeatDeliveryList == null) // пакеты удачно доставлены
                            {
                                //рассчитываем задержку
                                long deltaTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTime;
                                _rttCalculator.AddDelta(deltaTime);
                                _rtt = _rttCalculator.GetRtt;

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

                                    repeated = true;
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

                    if (attemptCount == 15)
                    {
                        Runtime.DebugWrite("PIZDETS!!!!");
                        ThreadPool.QueueUserWorkItem(delegate (object state)
                        {
                            Close();
                            ClientClosing?.Invoke(point);
                        });
                    }

                    lastPackage = -1;
                }
                finally
                {
                    sendingCycleDetector.Set();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToReceivingBuffer(byte[] package)
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


                if (isFull)
                {
                    MessageReceived?.Invoke();
                }     
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DataPackcageProcessing(byte[] data, ref int waitingLastPackage, ref int attemptSendCounts, ref ushort lastMissedPacket)
        {
            if (data.Length > 5)
            {
                ushort id = BitConverter.ToUInt16(new byte[2]
                {
                    data[HeaderPositions.Id_1],
                    data[HeaderPositions.Id_2]
                }, 0);

                ushort lastId = BitConverter.ToUInt16(new byte[2]
                {
                    data[HeaderPositions.LastId_1],
                    data[HeaderPositions.LastId_2]
                }, 0);

                if (id >= receivingPointer && id - receivingPointer < _maxPackagesCount)
                {
                    waitingLastPackage = lastId;

                    if (id == receivingPointer)
                    {
                        AddToReceivingBuffer(data);
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
                        AddToReceivingBuffer(packagesBuffer[receivingPointer]);
                        packagesBuffer.TryRemove(receivingPointer, out _);
                        receiveWait.Set();

                        receivingPointer++;
                    }

                    // проверяем все ли пакеты были получены
                    if (receivingPointer == (ushort)(lastId + 1))
                    {
                        // отправляем подтверждение
                        byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);
                        socket.Send(new byte[3]
                        {
                            PackgeCodes.ConfirmDataDelivery,
                            neEbyKakNazvat[0],
                            neEbyKakNazvat[1]
                        }, 3);
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
                                PackgeCodes.FailedList,
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
                                for (int h = 3; h < array.Length - 1; h += 2)
                                {
                                    var idg = BitConverter.ToUInt16(new byte[2] {
                                        array[h],
                                        array[h + 1]
                                    },
                                    0);
                                }
                            }

                            attemptSendCounts = data[HeaderPositions.AttemptsCounts];
                        }
                    }
                }
                else
                {
                    if (waitingLastPackage == lastId && data[HeaderPositions.AttemptsCounts] > attemptSendCounts)
                    {
                        var package = new List<byte>
                        {
                            PackgeCodes.FailedList,
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
                            socket.Send(new byte[3]
                            {
                                PackgeCodes.ConfirmDataDelivery,
                                neEbyKakNazvat[0],
                                neEbyKakNazvat[1]
                            }, 3);
                        }

                        attemptSendCounts = data[HeaderPositions.AttemptsCounts];
                    }
                    else if (id == lastId || data[HeaderPositions.Flag] == Flags.NeedConfirm)
                    {
                        byte[] neEbyKakNazvat = BitConverter.GetBytes(lastId);
                        socket.Send(new byte[3]
                        {
                            PackgeCodes.ConfirmDataDelivery,
                            neEbyKakNazvat[0],
                            neEbyKakNazvat[1]
                        }, 3);
                    }
                }
            }
        }

        private void ServiceReceive()
        {
            int waitingLastPackage = -1;
            int attemptSendCounts = -1;
            ushort lastMissedPacket = 0;

            try
            {
                while (IsConnected)
                {
                    byte[] data = socket.Receive(ref point);

                    if (data.Length < 1)
                    {
                        continue;
                    }

                    _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    switch (data[0])
                    {
                        case PackgeCodes.ConnectRequest:
                            {
                                //здесь уже не проверяем код подключения, ведь он был уже проверен в методе connect и ip отправителя зафиксирован
                                if (data[0] == PackgeCodes.ConnectRequest && data.Length > 2)
                                {
                                    socket.Send(_connectAnswerPackage, _connectAnswerPackage.Length);
                                }
                            }
                            break;
                        case PackgeCodes.MtuRequest: // пришел пакет с вычислением mtu
                            if (data.Length > 2)
                            {
                                socket.Send(new byte[2] { PackgeCodes.MtuResponse, data[1] }, 2);
                            }
                            break;
                        case PackgeCodes.PingRequest: //пришел пакет с пингом
                            if (data.Length == 2)
                            {
                                socket.Send(new byte[2] { PackgeCodes.PingResponse, data[1] }, 2);
                            }
                            break;
                        case PackgeCodes.PingResponse: //пришел ответ на пинг
                            PingProcessing(data);
                            break;
                        case PackgeCodes.Data: //пришел пакет данных
                            DataPackcageProcessing(data, ref waitingLastPackage, ref attemptSendCounts, ref lastMissedPacket);
                            break;
                        case PackgeCodes.ConfirmDataDelivery: // пришло подтверждение доставки пакета
                            if (data.Length == 3)
                            {
                                ushort id = BitConverter.ToUInt16(new byte[2]
                                {
                                    data[1],
                                    data[2]
                                }, 0);

                                repeatDeliveryBlock.WaitOne();
                                if (id == lastPackage)
                                {
                                    repeatDeliveryList = null;
                                    deliveryWait.Set();
                                }

                                repeatDeliveryBlock.Release();
                            }
                            break;
                        case PackgeCodes.ConnectionClose: // обрыв соединения
                            Runtime.DebugWrite("StopWork!!!!");
                            if (data.Length == 2 && data[1] == _hostSessionId)
                            {
                                Runtime.DebugWrite("StopWork, _hostSessionId: " + _hostSessionId);
                                ThreadPool.QueueUserWorkItem(delegate (object state)
                                {
                                    StopWork();
                                    ClientClosing?.Invoke(point);
                                });
                            }
                            break;
                        case PackgeCodes.FailedList: // пришел пакет со списком пакетов, которые нужно переотправить
                            //проверяем валидность этого пакета. пакет должен содержать список айдишников. каждый id занимает 2 байта. первый байт - код. то есть размер должен быть нечетным
                            if (data.Length > 4 && ((data.Length & 1) == 1))
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
                        case PackgeCodes.MtuResponse: // пришел ответ на вычисление mtu
                            if (data.Length == 2)
                            {
                                _mtuPackageId = data[1];
                                _mtuWait.Set();
                            }
                            break;
                        case PackgeCodes.MtuInfo: // пришёл пакет с инфой об mtu
                            if (data.Length == 3)
                            {
                                ushort hostMtu_ = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                socket.Send(new byte[1] { PackgeCodes.MtuInfoConfirm }, 1);

                                _calculateMtuBlock.WaitOne();
                                if (_hostMtu == -1) // метод Connect ещё не отработал
                                {
                                    _hostMtu = hostMtu_;
                                }
                                else // connect уже отработал, можно обновлять mtu
                                {
                                    // если mtu, отправленный хостом меньше, который вычислили мы, то обновляем его
                                    if (hostMtu_ < _mtu)
                                    {
                                        _mtu = hostMtu_;
                                    }
                                }
                                _calculateMtuBlock.Release();
                            }
                            break;
                        case PackgeCodes.MtuInfoConfirm: // пришёл ответ на пакет с инфой об mtu
                            _mtuInfoWait.Set();
                            break;

                    }
                }
            }
            catch { }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreatePackagesHeap(byte[] inputData, ref List<Package> packagesHeap, ref int mtu, ref int maxPackagesCount)
        {
            if (inputData.Length + 10 <= mtu)
            {
                Package package = new Package
                {
                    Size = inputData.Length + 10
                };
                package.Segments.Add(inputData);
                packagesHeap.Add(package);
            }
            else
            {
                int offset = 0;
                while (offset < inputData.Length)
                {
                    int lenght = (inputData.Length - offset) > (mtu - 10) ? mtu - 10 : inputData.Length - offset;
                    byte[] part = new byte[lenght];
                    Array.Copy(inputData, offset, part, 0, lenght);

                    Package package = new Package
                    {
                        Size = lenght + 10,
                        lastSegmentIsFull = false
                    };
                    package.Segments.Add(part);

                    if (packagesHeap.Count >= maxPackagesCount)
                    {
                        packagesHeap = new List<Package>();
                        sendingBuffer.Enqueue(packagesHeap);
                    }

                    packagesHeap.Add(package);

                    offset += lenght;
                }

                packagesHeap[packagesHeap.Count - 1].lastSegmentIsFull = true;
            }
        }

        public void Send(byte[] inputData)
        {
            if (_inStopping || !IsConnected) return;
            Begin: sendBlock.WaitOne();

            int mtu = _mtu;
            int maxPackagesCount = _maxPackagesCount;

            List<Package> packagesHeap;
            if (sendingBuffer.Count > 0)
            {
                if (sendingBuffer.Count > 1)
                {
                    sendBlock.Release();
                    sendingCycleDetector.WaitOne();

                    if (_inStopping || !IsConnected) return;
                    goto Begin;
                }

                sendingBuffer.TryPeek(out packagesHeap);
            }
            else
            {
                packagesHeap = new List<Package>();
                sendingBuffer.Enqueue(packagesHeap);
            }

            if (packagesHeap.Count <= maxPackagesCount)
            {
                if (packagesHeap.Count > 0)
                {
                    Package lastElement = packagesHeap[packagesHeap.Count - 1];
                    if (lastElement.Size + inputData.Length + 3 <= mtu)
                    {
                        lastElement.Segments.Add(inputData);
                        lastElement.Size += inputData.Length + 3;
                    }
                    else
                    {
                        if (packagesHeap.Count < maxPackagesCount)
                        {
                            int freeSpace = mtu - lastElement.Size - 3;
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

                                CreatePackagesHeap(part, ref packagesHeap, ref mtu, ref maxPackagesCount);
                            }
                            else
                            {
                                CreatePackagesHeap(inputData, ref packagesHeap, ref mtu, ref maxPackagesCount);
                            }
                        }
                        else
                        {
                            sendBlock.Release();
                            sendingCycleDetector.WaitOne();

                            if (_inStopping || !IsConnected) return;
                            goto Begin;
                        }
                    }
                }
                else
                {
                    CreatePackagesHeap(inputData, ref packagesHeap, ref mtu, ref maxPackagesCount);
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

        private void FormingMessage(out byte[] data)
        {
            List<byte[]> buffer = new List<byte[]>();
            int messageSize = 0;

            receivingBuffer.TryDequeue(out Message segment);
            buffer.Add(segment.data);
            messageSize += segment.data.Length;

            while (!segment.IsFull && (IsConnected || receivingBuffer.Count > 0))
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

            data = new byte[messageSize];
            int offset = 0;
            foreach (byte[] segmentBytes in buffer)
            {
                int len = segmentBytes.Length;
                Array.Copy(segmentBytes, 0, data, offset, len);
                offset += len;
            }
        }

        public bool Receive(out byte[] data)
        {
            if (receivingBuffer.Count > 0)
            {
                FormingMessage(out data);
                return true;
            }
            else //буфер пуст
            {
                while (IsConnected)
                {
                    receiveWait.WaitOne(); //этот поток возобновится когда появятся новые пакеты

                    if (receivingBuffer.Count > 0) //если clientQueue.Count == 0 значит что прошлый пакет был принят блоком кода выше. Поэтому threadReset сохранило свое состояние, а пакет был извелчен
                    {
                        FormingMessage(out data);
                        return true;
                    }
                }
            }

            Runtime.DebugWrite("SMP CLIENT STOP WORK");
            data = new byte[0];
            return false;
        }

        private void StopWork()
        {
            Runtime.DebugWrite("StopWork() SMP CLIENT");
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
                    _inStopping = true; // ставим флаг чтобы send нельзя было вызвать ещё раз
                    sendBlock.Release();

                    while (sendingBuffer.Count != 0) // ждём когда все пакеты из буфера будут доставлены
                    {
                        sendingCycleDetector.WaitOne();
                    }

                    for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                    {
                        socket.Send(new byte[2] { PackgeCodes.ConnectionClose, _selfSessionId }, 2); // TODO: было исключение доступ к ликвидированному объекту запрещен
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