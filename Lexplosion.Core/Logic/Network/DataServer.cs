using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NightWorld.Collections.Concurrent;

namespace Lexplosion.Logic.Network
{
    class DataServer : NetworkServer
    {
        protected struct FileData
        {
            public FileStream Stream;
            public long FileSize;
        }

        protected class ClientData
        {
            public string FileId; // отправляемый файл
            public long ReadingOffset; //офсет в отправляемом файле
            public byte[] EncodeKey; //ключ ширования
            public byte[] IV; //вектор инициализации
        }

        protected Dictionary<string, FileData> SFilesList = new(); // список всех FileStream и размеров файлов

        protected ConcurrentDictionary<ClientDesc, ClientData> ClientsData = new();

        protected ConcurrentHashSet<ClientDesc> AuthorizedClients = new();
        protected List<ClientDesc> AvailableConnections = new();

        protected AutoResetEvent WaitClient = new AutoResetEvent(false);
        protected Semaphore SendingBlock = new Semaphore(1, 1); //блокировка во время работы метода Sending

        const int HEAP = 1400; //количество байт отправляемых за один раз
        const string SERVER_TYPE = "data-server"; // эта строка нужна при подключении к управляющему серверу

        private byte[] _сonfirmWord;
        private RSAParameters _privateRsaKey;

        private object _abortLoocker = new object();
        private object _authorizeLocker = new object();

        public DataServer(RSAParameters privateRsaKey, string confirmWord, string uuid, string sessionToken, ControlServerData server) : base(uuid, sessionToken, SERVER_TYPE, true, server)
        {
            _сonfirmWord = Encoding.UTF8.GetBytes(confirmWord);
            _privateRsaKey = privateRsaKey;

            StartThreads();
        }

        protected override bool AfterConnect(ClientDesc clientDesc)
        {
            AvailableConnections.Add(clientDesc);
            base.AfterConnect(clientDesc);

            return true;
        }

        protected override void Sending()
        {
            var toDisconect = new List<ClientDesc>();
            WaitClient.WaitOne(); //ждём первого авторизированного клиента

            while (IsWork)
            {
                if (AuthorizedClients.Count == 0)
                {
                    WaitClient.WaitOne(); //ждём первого авторизированного клиента
                }


                IEnumerable<ClientDesc> clients = Server.WaitSendAvailable();

                SendingBlock.WaitOne();

                foreach (ClientDesc clientPoint in clients)
                {
                    try
                    {
                        if (!AuthorizedClients.Contains(clientPoint)) continue;

                        ClientData clientData = ClientsData[clientPoint];

                        byte[] buffer = new byte[HEAP];

                        var fileData = SFilesList[clientData.FileId];
                        FileStream file = fileData.Stream; //получаем FileStream для этого клиента
                        long fileSize = fileData.FileSize;

                        file.Seek(clientData.ReadingOffset, SeekOrigin.Begin); //перемещаем указатель чтения файла
                        int bytesCount = file.Read(buffer, 0, HEAP); //читаем файл
                        clientData.ReadingOffset += HEAP; //увеличиваем оффсет

                        byte[] buffer_;
                        if (bytesCount != HEAP)
                        {
                            buffer_ = new byte[bytesCount];
                            Array.Copy(buffer, 0, buffer_, 0, bytesCount); //обрезаем буффер до нужных размеров
                        }
                        else
                        {
                            buffer_ = buffer;
                        }

                        // TODO: тут трай надо        -- 09.07.2023 А нахуя тут трай?
                        byte[] payload = Cryptography.AesEncode(buffer_, clientData.EncodeKey, clientData.IV);
                        Server.Send(payload, clientPoint); //отправляем

                        //файл передан, удаляем клиента
                        if (clientData.ReadingOffset >= fileSize)
                        {
                            Runtime.DebugConsoleWrite("END SEND. Bytes count " + clientData.ReadingOffset);
                            toDisconect.Add(clientPoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Runtime.DebugConsoleWrite("Sending exception " + ex);
                        toDisconect.Add(clientPoint);
                        continue;
                        // TODO: че-то мутить
                    }
                }

                SendingBlock.Release();

                if (toDisconect.Count > 0)
                {
                    foreach (ClientDesc point in toDisconect)
                    {
                        //удаляем клиента, но не вызываем закрытия соединения что бы последние пакеты не потерялись.
                        //Позже клиент сам оборвет сединение. Или же оно закроется само через некоторое время
                        ClientAbort(point);
                    }

                    toDisconect = new List<ClientDesc>();
                }
            }
        }

        protected override void Reading()
        {
            //первое значение - стадия подключения, второе - aes ключ, третье - aes вектор инициализации
            var connectionStages = new Dictionary<ClientDesc, ReferenceTuple<int, byte[], byte[]>>();

            while (IsWork)
            {
                try
                {
                    ClientDesc point = Server.Receive(out byte[] data);

                    if (point.IsEmpty)
                    {
                        // возможно метод AfterConnect еще не начал работать. если метод подключения в процессе работы, то мы тут остановимся
                        ConnectionWait.WaitOne();
                    }

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
                                }
                                else
                                {
                                    Server.Close(point);
                                }

                                AcceptingBlock.Release();
                                continue;
                            }
                            else // в соотвествии со стадией подключения выполняем действия
                            {
                                if (connectionStages[point].Value1 == 0)
                                {
                                    byte[] aesData = Cryptography.RsaDecode(data, _privateRsaKey); // получем aes ключ для шифрования
                                    if (aesData.Length == 48)
                                    {
                                        byte[] aesKey = new byte[32];
                                        byte[] aesIV = new byte[16];

                                        Array.Copy(aesData, 0, aesKey, 0, 32);
                                        Array.Copy(aesData, 32, aesIV, 0, 16);

                                        // отправляем кодовое слово клиенту в зашифрованном виде
                                        Server.Send(Cryptography.AesEncode(_сonfirmWord, aesKey, aesIV), point);

                                        connectionStages[point].Value2 = aesKey;
                                        connectionStages[point].Value3 = aesIV;
                                        connectionStages[point].Value1++;
                                    }
                                    else
                                    {
                                        connectionStages.Remove(point);
                                        Server.Close(point);
                                        AcceptingBlock.Release();
                                        continue;
                                    }
                                }
                                else
                                {
                                    byte[] aesKey = connectionStages[point].Value2;
                                    byte[] aesIV = connectionStages[point].Value3;

                                    string profileId = Encoding.UTF8.GetString(Cryptography.AesDecode(data, aesKey, aesIV));

                                    if (SFilesList.ContainsKey(profileId))
                                    {
                                        ClientsData[point] = new ClientData
                                        {
                                            FileId = profileId,
                                            ReadingOffset = 0,
                                            EncodeKey = aesKey,
                                            IV = aesIV
                                        };

                                        connectionStages.Remove(point);
                                        AuthorizedClients.Add(point);

                                        byte[] fileSize = BitConverter.GetBytes(SFilesList[profileId].FileSize);
                                        Server.Send(Cryptography.AesEncode(fileSize, aesKey, aesIV), point); // отправляем размер файла

                                        WaitClient.Set();
                                        Runtime.DebugConsoleWrite("Авторизировал");
                                    }
                                    else
                                    {
                                        Runtime.DebugConsoleWrite("PARASHA");

                                        connectionStages.Remove(point);
                                        Server.Close(point);
                                        AcceptingBlock.Release();
                                        continue;
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

        protected override void ClientAbort(ClientDesc clientData)
        {
            lock (_abortLoocker)
            {
                if (ClientsData.ContainsKey(clientData))
                {
                    AcceptingBlock.WaitOne();
                    SendingBlock.WaitOne();
                    Runtime.DebugConsoleWrite("ClientAbort. StackTrace: " + new System.Diagnostics.StackTrace());

                    AvailableConnections.Remove(clientData);
                    AuthorizedClients.Remove(clientData);
                    ClientsData.TryRemove(clientData, out _);

                    base.ClientAbort(clientData);

                    AcceptingBlock.Release();
                    SendingBlock.Release();
                }
            }
        }

        public bool AddFile(string fileName, string id)
        {
            FileStream stream;
            long fileSize;

            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                fileSize = stream.Length;
            }
            catch
            {
                return false;
            }

            SFilesList[id] = new FileData
            {
                Stream = stream,
                FileSize = fileSize
            };

            return true;
        }

        public override void StopWork()
        {
            base.StopWork();
            foreach (var file in SFilesList.Values)
            {
                file.Stream.Close();
            }
        }
    }
}