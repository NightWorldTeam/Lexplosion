using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Lexplosion.Logic.Network.SMP
{
    class SmpClient
    {
        protected UdpClient socket = null;
        protected bool isConnected = false;
        public long ping = -1;

        public IPEndPoint point = null;
        protected Thread serviceWork = null;

        protected ConcurrentDictionary<ushort, byte[]> packagesBuffer = new ConcurrentDictionary<ushort, byte[]>(); //буфер принятых пакетов
        protected ConcurrentQueue<byte[]> sendingBuffer = new ConcurrentQueue<byte[]>(); //буфер пакетов на отправку
        public ConcurrentQueue<byte[]> packagesQueue = new ConcurrentQueue<byte[]>(); //очередь обработанных пакетов

        public ushort pointer = 0; //указатель на id следующего пакета, который нужно получить
        public ushort sendingPointer = 0; //указатель на id следующего пакета который будет отправляться
        protected long lastTime = 0; //время отправки последнего пакета

        protected AutoResetEvent threadReset = new AutoResetEvent(false);
        protected AutoResetEvent suspensionSend = new AutoResetEvent(false);
        protected object locker = new object();

        protected Semaphore semaphore = new Semaphore(1, 1);
        protected AutoResetEvent[] sendingWait = new AutoResetEvent[2] { new AutoResetEvent(false), new AutoResetEvent(false) };

        protected ushort maxPackageSize = 1024;
        protected ushort maxPackagesCount = 10;

        protected List<int[]> packagesInfo = new List<int[]>(); //количество пакетов и байт, что стоит на отправку. 0 - количество пакетов 1 - количество байт
        protected ushort lastPackage = 0; //id пакета, о доставке которого сейчас ожидается подтверждение

        protected bool workPing = false;
        protected long[] times = new long[20]; //харнит время отправки пакетов с пингом

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
            ping = 40; // TODO: я тут 40 зачем-то поставил

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

                serviceWork = new Thread(delegate () { ServiceWork(); });
                serviceWork.Start();

                ping = Ping(); //более точно измеряем пинг
                if (ping == -1)
                {
                    isConnected = false;
                    return false;
                }

                lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            if (isConnected) //проверяем является ли соединение активным
            {
                try //может произойти ситуация что в другом потоке соединение уже будет закрыто, роэтому сделал костыль
                {
                    for (int i = 0; i < 20; i++) //отправляем 20 запросов на разрыв соединения
                    {
                        socket.Send(new byte[1] { 0x05 }, 1);
                    }

                    isConnected = false;
                    socket.Close();
                    threadReset.Set();
                }
                catch { }
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

                Thread.Sleep(100);

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

        protected void ServiceWork() //метод работающий всегда. Читает UDP сокет и контролирует доставку пакетов
        {
            new Thread(delegate () //поток отправляющий пакеты пинга при долгой неактивности для удержания соединения
            {
                while (isConnected)
                {
                    if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTime >= 2000) //проверяем что последний пакет был отправлен более 2 секунд назад
                    {
                        if (Ping() == -1) //проверяем ответил ли хост
                        {
                            Close();
                            ClientClosing?.Invoke(point);
                        }

                    }

                    Thread.Sleep(2000);
                }

            }).Start();

            new Thread(delegate () //поток отправляющий пакеты данных
            {
                while (isConnected)
                {
                    while (packagesInfo.Count == 0) //костыль блять
                    {
                        sendingWait[0].WaitOne();
                    }

                    semaphore.WaitOne();
                    sendingWait[1].Set();

                    List<byte[]> sendData = new List<byte[]>();

                    ushort idInt = sendingPointer;

                    int i;
                    for (i = 0; i < packagesInfo.Count; i++)
                    {
                        byte[] id = BitConverter.GetBytes(idInt);

                        sendData.Add(new byte[packagesInfo[i][1]]);
                        byte[] temp;

                        sendData[i][0] = 0x03;  //код пакета
                        sendData[i][1] = id[0]; //первая часть его айдишника
                        sendData[i][2] = id[1]; //вторая часть

                        int offset = 3;
                        for (int j = 0; j < packagesInfo[i][0]; j++)
                        {
                            sendingBuffer.TryDequeue(out temp);

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

                    idInt--;

                    sendData[i - 1][3] = 0x01; //флагу последнего пакета присваевам значение что необходимо подтверждение доставки
                    lock (locker)
                    {
                        lastPackage = idInt;
                    }

                    packagesInfo.Clear();

                    semaphore.Release(); //возобновляем работу метода send

                    int b = 0;
                    while (isConnected && (idInt + 1) != sendingPointer && b < 20) //отправляем этот пакет максимум 20 раз, пока соединение активно. Когда придет подтверждение доставки sendingPointer увеличится на еденицу
                    {
                        for (i = 0; i < sendData.Count; i++)
                        {
                            socket.Send(sendData[i], sendData[i].Length);
                        }
                        lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        b++;
                        suspensionSend.WaitOne((int)ping); //после отправки пакета стопаем цикл пока не придет подвтерждение. Но максимальное время остановки == пингу
                    }

                    if (b >= 19 && (idInt + 1) != sendingPointer)
                    {
                        isConnected = false;
                        socket.Close();
                        threadReset.Set();
                        ClientClosing?.Invoke(point);
                    }


                }

            }).Start();

            byte[] data;
            while (isConnected)
            {
                try
                {
                    data = socket.Receive(ref point); // TODO: может получиться как с сервером: если отправить данные на закрытый сокет, то наш сокет возможно начнет бросать исключения. То есть если сервер откинет коньки, то этот сокет будет бросать исключения
                    lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (data.Length > 0)
                    {
                        switch (data[0])
                        {
                            case 0: //запрос на подключение (в штатном режиме приходит только если данный хост уже получил такой же запрос и isConnected установил true)
                                socket.Send(new byte[1] { 0x00 }, 1);
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

                                    if (id >= pointer && id - pointer <= 20) //проверяем новый ли это пакет
                                    {
                                        if (id == pointer)
                                        {
                                            if (data[3] == 0x01) //пакет требует подтверждение доставки
                                            {
                                                //отправляем подтверждение
                                                byte[] neEbyKakNazvat = BitConverter.GetBytes(id);
                                                socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);

                                            }

                                            //разбиваем пакет на блоки данных и кладём в очередь
                                            int offset = 4;
                                            int size; //размер первого блока данных
                                            while (offset < data.Length)
                                            {
                                                size = BitConverter.ToUInt16(new byte[2] { data[offset], data[offset + 1] }, 0);
                                                byte[] dataBlock = new byte[size];

                                                Array.Copy(data, offset + 2, dataBlock, 0, size);
                                                packagesQueue.Enqueue(dataBlock); //помещаем пакет в очередь

                                                offset += size + 3;
                                            }

                                            pointer++;

                                            //ищем в буфере следующие пакеты и помещаем их в очередь
                                            ushort nextId = (ushort)(pointer + 1);
                                            while (packagesBuffer.ContainsKey(nextId))
                                            {
                                                byte[] tempData;
                                                packagesBuffer.TryRemove(nextId, out tempData); //удаляем элемент из буфера

                                                if (tempData[0] == 0x01) //пакет требует подтверждение доставки
                                                {
                                                    //отправляем подтверждение
                                                    byte[] neEbyKakNazvat = BitConverter.GetBytes(nextId);
                                                    socket.Send(new byte[3] { 0x04, neEbyKakNazvat[0], neEbyKakNazvat[1] }, 3);

                                                }

                                                offset = 1;
                                                while (offset < tempData.Length)
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
                                            Array.Copy(data, 2, packagesBuffer[id], 0, data.Length - 3); //убираем служебные данные и помещяем элемент в буфер

                                        }

                                    }
                                    else if (data[3] == 0x01)
                                    {
                                        socket.Send(new byte[3] { 0x04, data[1], data[2] }, 3); //отправляем пакет подтверждающий доставку
                                    }

                                }
                                break;

                            case 4: //пришло подтверждение доставки пакета
                                {
                                    ushort id = BitConverter.ToUInt16(new byte[2] { data[1], data[2] }, 0);
                                    if (data.Length == 3)
                                    {
                                        lock (locker)
                                        {
                                            if (id == lastPackage)
                                            {
                                                lastPackage++;
                                                sendingPointer = lastPackage;
                                                suspensionSend.Set(); //возобновляем отправляющий цикл, ведь подтверждение пришло
                                            }
                                        }

                                    }
                                }
                                break;

                            case 5: //пришел пакет на обрыв соединения
                                isConnected = false;
                                socket.Close();
                                threadReset.Set();
                                ClientClosing?.Invoke(point);

                                break;

                        }
                    }
                }
                catch { }

            }
        }

        public void Send(byte[] inputData)
        {
            if (packagesInfo.Count >= maxPackageSize)
            {
                sendingWait[1].WaitOne();
            }

            semaphore.WaitOne(); //ждём когда поток отпарвки сделает свою работу

            int temp;
            if (packagesInfo.Count > 0)
            {

                temp = packagesInfo[packagesInfo.Count - 1][1] + inputData.Length + 3;

                if (temp <= maxPackageSize)
                {
                    sendingBuffer.Enqueue(inputData);

                    packagesInfo[packagesInfo.Count - 1][0]++;
                    packagesInfo[packagesInfo.Count - 1][1] = temp;

                }
                else
                {
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

            sendingWait[0].Set();
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
