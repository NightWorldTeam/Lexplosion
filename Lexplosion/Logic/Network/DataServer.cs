using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    class DataServer: NetworkServer
    {
        protected Dictionary<string, FileStream> SFilesList; // список всех FileStream

        protected Dictionary<IPEndPoint, string> TransmittedFiles; // соотвествие клиента и отправляемого ему файла
        protected Dictionary<IPEndPoint, int> OffsetsList; // соответсвие клиента и оффсета в отправляемом ему файле

        protected List<IPEndPoint> NotAuthorized;

        protected Semaphore FilesListSemaphore; //блокировка для метода AddFile

        const int Heap = 128; //количество байт отправляемых за один раз
        const string serverType = "data-server"; // эта строка нужна при подключении к управляющему серверу

        public DataServer() : base(serverType)
        {
            SFilesList = new Dictionary<string, FileStream>();

            TransmittedFiles = new Dictionary<IPEndPoint, string>();
            OffsetsList = new Dictionary<IPEndPoint, int>();

            NotAuthorized = new List<IPEndPoint>();
            FilesListSemaphore = new Semaphore(1, 1);
        }

        protected override void BeforeConnect(IPEndPoint point)
        {
            NotAuthorized.Add(point);
        }

        protected override void Sending()
        {
            threadReset.WaitOne(); //ждём первого подключения

            while (IsWork)
            {
                AcceptingBlock.WaitOne();

                foreach (IPEndPoint clientPoint in AvailableConnections)
                {
                    byte[] buffer = new byte[Heap];
                    FilesListSemaphore.WaitOne();
                    FileStream file = SFilesList[TransmittedFiles[clientPoint]]; //получаем FileStream для этого клиента
                    FilesListSemaphore.Release();

                    file.Seek(OffsetsList[clientPoint], SeekOrigin.Begin);//перемещаем указатель чтения файла
                    int bytesCount = file.Read(buffer, OffsetsList[clientPoint], Heap); //читаем файл
                    OffsetsList[clientPoint] += Heap; //увеличиваем оффсет

                    byte[] buffer_ = new byte[bytesCount];
                    Array.Copy(buffer, 0, buffer_, 0, bytesCount); //обрезаем буффер до нужных размеров

                    Server.Send(buffer_, clientPoint); //отправляем

                }

                AcceptingBlock.Release();
            }      
        }

        protected override void Reading() 
        {
            threadResetReading.WaitOne(); //ждём первого подключения
            threadResetReading.Set();

            while (IsWork)
            {
                try
                {
                    IPEndPoint point = Server.Receive(out byte[] data);

                    AcceptingBlock.WaitOne();
                    if (AvailableConnections.Contains(point) && NotAuthorized.Contains(point))
                    {
                        try
                        {
                            string profileId = Encoding.UTF8.GetString(data);
                            FilesListSemaphore.WaitOne();

                            if (SFilesList.ContainsKey(profileId))
                            {
                                FilesListSemaphore.Release();
                                TransmittedFiles[point] = profileId;
                                OffsetsList[point] = 0;
                                NotAuthorized.Remove(point);
                            }
                            else
                            {
                                FilesListSemaphore.Release();
                                // TODO: че-то делать
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

        public bool AddFile(string fileName, string id)
        {
            FileStream stream;
            try
            {
                stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                return false;
            }

            FilesListSemaphore.WaitOne();
            SFilesList[id] = stream;
            FilesListSemaphore.Release();

            return true;
        }
    }
}
