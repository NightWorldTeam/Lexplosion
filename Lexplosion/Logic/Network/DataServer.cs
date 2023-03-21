using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class DataServer : NetworkServer
    {
        protected struct FileData
        {
            public FileStream Stream;
            public long FileSize;
            public byte[] Sha256;
        }

        protected Dictionary<string, FileData> SFilesList; // список всех FileStream и размеров файлов

        // первое значение - отправляемый файл, второе - офсет в отправляемом файле, третье - ключ ширования, четвертое - вектор инициализации
        protected Dictionary<IPEndPoint, ReferenceTuple<string, int, byte[], byte[]>> ClientsData;

        protected List<IPEndPoint> AuthorizedClients;
        protected List<IPEndPoint> AvailableConnections;

        protected Semaphore FilesListSemaphore; //блокировка для метода AddFile
        protected AutoResetEvent WaitClient = new AutoResetEvent(false);

        const int Heap = 1400; //количество байт отправляемых за один раз
        const string serverType = "data-server"; // эта строка нужна при подключении к управляющему серверу

        private byte[] _сonfirmWord;
        private RSAParameters _privateRsaKey;

        private object _abortLoocker = new object();
        private object _authorizeLocker = new object();

        public DataServer(RSAParameters privateRsaKey, string confirmWord, string uuid, string sessionToken, string server) : base(uuid, sessionToken, serverType, true, server)
        {
            SFilesList = new Dictionary<string, FileData>();

            ClientsData = new();

            AuthorizedClients = new List<IPEndPoint>();
            FilesListSemaphore = new Semaphore(1, 1);
            AvailableConnections = new List<IPEndPoint>();

            _сonfirmWord = Encoding.UTF8.GetBytes(confirmWord);
            _privateRsaKey = privateRsaKey;
        }

        protected override bool AfterConnect(IPEndPoint point)
        {
            AvailableConnections.Add(point);
            base.AfterConnect(point);

            return true;
        }

        protected override void Sending()
        {
            List<IPEndPoint> toDisconect = new List<IPEndPoint>();

            while (IsWork)
            {
                if (AuthorizedClients == null || AuthorizedClients.Count == 0)
                {
                    WaitClient.WaitOne(); //ждём первого авторизированного клиента
                }

                IPEndPoint[] authorizedClients;
                lock (_authorizeLocker)
                {
                    authorizedClients = AuthorizedClients.ToArray();
                }

                SendingBlock.WaitOne();
                foreach (IPEndPoint clientPoint in authorizedClients)
                {
                    byte[] buffer = new byte[Heap];

                    FilesListSemaphore.WaitOne();
                    ReferenceTuple<string, int, byte[], byte[]> clientData = ClientsData[clientPoint];
                    var fileData = SFilesList[clientData.Value1];
                    FileStream file = fileData.Stream; //получаем FileStream для этого клиента
                    long fileSize = fileData.FileSize;
                    FilesListSemaphore.Release();

                    file.Seek(clientData.Value2, SeekOrigin.Begin); //перемещаем указатель чтения файла
                    int bytesCount = file.Read(buffer, 0, Heap); //читаем файл
                    clientData.Value2 += Heap; //увеличиваем оффсет

                    byte[] buffer_;
                    if (bytesCount != Heap)
                    {
                        buffer_ = new byte[bytesCount];
                        Array.Copy(buffer, 0, buffer_, 0, bytesCount); //обрезаем буффер до нужных размеров
                    }
                    else
                    {
                        buffer_ = buffer;
                    }

                    // TODO: тут трай надо
                    byte[] payload = Сryptography.AesEncode(buffer_, clientData.Value3, clientData.Value4);
                    Server.Send(payload, clientPoint); //отправляем

                    //файл передан, закрываем соединение
                    if (clientData.Value2 >= fileSize)
                    {
                        Runtime.DebugWrite("END SEND");
                        toDisconect.Add(clientPoint);
                    }
                }

                SendingBlock.Release();

                if (toDisconect.Count > 0)
                {
                    foreach (IPEndPoint point in toDisconect)
                    {
                        Server.Close(point);
                        ClientAbort(point);
                    }

                    toDisconect = new List<IPEndPoint>();
                }
            }
        }

        protected override void Reading()
        {
            //первое значение - стадия подключения, второе - aes ключ, третье - aes вектор инициализации
            var connectionStages = new Dictionary<IPEndPoint, ReferenceTuple<int, byte[], byte[]>>();

            while (IsWork)
            {
                try
                {
                    IPEndPoint point = Server.Receive(out byte[] data);

                    AcceptingBlock.WaitOne();
                    if (AvailableConnections.Contains(point))
                    {
                        try
                        {
                            if (!connectionStages.ContainsKey(point)) // клиент только инициализировал подключение
                            {
                                if (data.Length == 1 && data[0] == 0x00)
                                {
                                    connectionStages[point] = new ReferenceTuple<int, byte[], byte[]>
                                    {
                                        Value1 = 0
                                    };

                                    AcceptingBlock.Release();
                                    continue;
                                }
                                else
                                {
                                    // TODO: тут отключать наверное
                                }
                            }
                            else // в соотвествии со стадией подключения выполняем действия
                            {
                                if (connectionStages[point].Value1 == 0)
                                {
                                    byte[] aesData = Сryptography.RsaDecode(data, _privateRsaKey); // получем aes ключ для шифрования
                                    if (aesData.Length == 48)
                                    {
                                        byte[] aesKey = new byte[32];
                                        byte[] aesIV = new byte[16];

                                        Array.Copy(aesData, 0, aesKey, 0, 32);
                                        Array.Copy(aesData, 32, aesIV, 0, 16);

                                        // отправляем кодовое слово клиенту в зашифрованном виде
                                        Server.Send(Сryptography.AesEncode(_сonfirmWord, aesKey, aesIV), point);

                                        connectionStages[point].Value2 = aesKey;
                                        connectionStages[point].Value3 = aesIV;
                                        connectionStages[point].Value1++;
                                    }
                                    else
                                    {
                                        // TODO: наверное отключать
                                    }
                                }
                                else
                                {
                                    byte[] aesKey = connectionStages[point].Value2;
                                    byte[] aesIV = connectionStages[point].Value3;

                                    string profileId = Encoding.UTF8.GetString(Сryptography.AesDecode(data, aesKey, aesIV));
                                    FilesListSemaphore.WaitOne();

                                    if (SFilesList.ContainsKey(profileId))
                                    {
                                        FilesListSemaphore.Release();

                                        ClientsData[point] = new ReferenceTuple<string, int, byte[], byte[]>
                                        {
                                            Value1 = profileId,
                                            Value2 = 0,
                                            Value3 = aesKey,
                                            Value4 = aesIV
                                        };

                                        connectionStages.Remove(point);
                                        lock (_authorizeLocker)
                                        {
                                            AuthorizedClients.Add(point);
                                        }

                                        byte[] fileSize = BitConverter.GetBytes(SFilesList[profileId].FileSize);
                                        Server.Send(Сryptography.AesEncode(fileSize, aesKey, aesIV), point); // отправляем размер файла
                                        Server.Send(Сryptography.AesEncode(SFilesList[profileId].Sha256, aesKey, aesIV), point); // отправляем хэш

                                        WaitClient.Set();
                                        Runtime.DebugWrite("Авторизировал");
                                    }
                                    else
                                    {
                                        Runtime.DebugWrite("PARASHA");
                                        FilesListSemaphore.Release();
                                        // TODO: че-то делать
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // TODO: че-то делать
                        }

                    }
                    AcceptingBlock.Release();
                }
                catch
                {
                    break;
                    // TODO: тут че-то сделать
                }
            }
        }

        protected override void ClientAbort(IPEndPoint point)
        {
            lock (_abortLoocker)
            {
                if (point != null && ClientsData.ContainsKey(point))
                {
                    AcceptingBlock.WaitOne();
                    SendingBlock.WaitOne();

                    // TODO: тут наверное проверять на наличие. И вообще проверить логику отлючения
                    AvailableConnections.Remove(point);
                    AuthorizedClients.Remove(point);
                    ClientsData.Remove(point);

                    base.ClientAbort(point);

                    AcceptingBlock.Release();
                    SendingBlock.Release();
                }
            }
        }

        public bool AddFile(string fileName, string id)
        {
            FileStream stream;
            long fileSize;
            byte[] sha256;

            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (SHA256 SHA256 = SHA256Managed.Create())
                {
                    sha256 = SHA256.ComputeHash(stream);
                    fileSize = stream.Length;
                }
            }
            catch
            {
                return false;
            }

            FilesListSemaphore.WaitOne();
            SFilesList[id] = new FileData
            {
                Stream = stream,
                FileSize = fileSize,
                Sha256 = sha256,
            };
            FilesListSemaphore.Release();

            return true;
        }
    }
}
