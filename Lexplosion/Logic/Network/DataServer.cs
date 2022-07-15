using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        }

        protected Dictionary<string, FileData> SFilesList; // список всех FileStream и размеров файлов

        protected Dictionary<IPEndPoint, string> TransmittedFiles; // соотвествие клиента и отправляемого ему файла
        protected Dictionary<IPEndPoint, int> OffsetsList; // соответсвие клиента и оффсета в отправляемом ему файле

        protected List<IPEndPoint> AuthorizedClients;
        protected List<IPEndPoint> AvailableConnections;

        protected Semaphore FilesListSemaphore; //блокировка для метода AddFile
        protected AutoResetEvent WaitClient = new AutoResetEvent(false);

        const int Heap = 1400; //количество байт отправляемых за один раз
        const string serverType = "data-server"; // эта строка нужна при подключении к управляющему серверу

        public DataServer(string uuid, bool directConnection, string server) : base(uuid, "", serverType, directConnection, server)
        {
            SFilesList = new Dictionary<string, FileData>();

            TransmittedFiles = new Dictionary<IPEndPoint, string>();
            OffsetsList = new Dictionary<IPEndPoint, int>();

            AuthorizedClients = new List<IPEndPoint>();
            FilesListSemaphore = new Semaphore(1, 1);
            AvailableConnections = new List<IPEndPoint>();
        }

        protected override bool BeforeConnect(IPEndPoint point)
        {
            AvailableConnections.Add(point);
            AcceptingBlock.Release();
            return true;
        }

        protected override void Sending()
        {
            WaitClient.WaitOne(); //ждём первого авторизированного клиента (того, кто удачно передал id файла, который ему нужно скачать)
            while (IsWork)
            {
                AcceptingBlock.WaitOne();

                IPEndPoint[] authorizedClients = AuthorizedClients.ToArray();

                foreach (IPEndPoint clientPoint in authorizedClients)
                {
                    byte[] buffer = new byte[Heap];
                    FilesListSemaphore.WaitOne();
                    FileStream file = SFilesList[TransmittedFiles[clientPoint]].Stream; //получаем FileStream для этого клиента
                    FilesListSemaphore.Release();

                    file.Seek(OffsetsList[clientPoint], SeekOrigin.Begin);//перемещаем указатель чтения файла
                    int bytesCount = file.Read(buffer, 0, Heap); //читаем файл
                    OffsetsList[clientPoint] += Heap; //увеличиваем оффсет

                    byte[] buffer_ = new byte[bytesCount];
                    Array.Copy(buffer, 0, buffer_, 0, bytesCount); //обрезаем буффер до нужных размеров

                    Server.Send(buffer_, clientPoint); //отправляем

                    //файл передан, закрываем соединение
                    if (OffsetsList[clientPoint] >= SFilesList[TransmittedFiles[clientPoint]].FileSize)
                    {
                        Server.Close(clientPoint);
                        Console.WriteLine("END SEND");
                        AvailableConnections.Remove(clientPoint);
                        AuthorizedClients.Remove(clientPoint);
                        OffsetsList.Remove(clientPoint);

                        if (AvailableConnections.Count == 0) // клиенты закончились
                        {
                            AcceptingBlock.Release(); // разлочиваем AcceptingBlock
                            SendingWait.WaitOne(); //ждём нового подключения, после чего переходим на метку AfterAcceptingBlock чтобы повторно не вызвать AcceptingBlock.Release()s
                            goto AfterAcceptingBlock; // Да блять, это goto и мне похуй. Сейчас оно идеально вписывается в ситуацию. Не нужно городить лишних условий, флагов, циклов и прочей поябени
                        }
                    }
                }

                AcceptingBlock.Release();
                AfterAcceptingBlock:;
            }
        }

        protected override void Reading()
        {
            ReadingWait.WaitOne(); //ждём первого подключения
            ReadingWait.Set();

            while (IsWork)
            {
                try
                {
                    IPEndPoint point = Server.Receive(out byte[] data);

                    AcceptingBlock.WaitOne();
                    if (AvailableConnections.Contains(point))
                    {
                        //try
                        {
                            string profileId = Encoding.UTF8.GetString(data);
                            FilesListSemaphore.WaitOne();

                            if (SFilesList.ContainsKey(profileId))
                            {
                                FilesListSemaphore.Release();
                                TransmittedFiles[point] = profileId;
                                OffsetsList[point] = 0;
                                AuthorizedClients.Add(point);
                                Server.Send(BitConverter.GetBytes(SFilesList[profileId].FileSize), point);
                                WaitClient.Set();
                                Console.WriteLine("Авторизировал");
                            }
                            else
                            {
                                Console.WriteLine("PARASHA");
                                FilesListSemaphore.Release();
                                // TODO: че-то делать
                            }
                        }
                        /*catch
                        {
                            // TODO: че-то делать
                        }*/

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
            AcceptingBlock.WaitOne();
            SendingBlock.WaitOne();

            AvailableConnections.Remove(point);
            AuthorizedClients.Remove(point);
            OffsetsList.Remove(point);

            base.ClientAbort(point);

            AcceptingBlock.Release();
            SendingBlock.Release();
        }

        public bool AddFile(string fileName, string id)
        {
            FileStream stream;
            long fileSize;

            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                fileSize = (new FileInfo(fileName)).Length;
            }
            catch
            {
                return false;
            }

            FilesListSemaphore.WaitOne();
            SFilesList[id] = new FileData
            {
                Stream = stream,
                FileSize = fileSize
            };
            FilesListSemaphore.Release();

            return true;
        }
    }
}
