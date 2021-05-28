using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    class DataServer: NetworkServer
    {
        Dictionary<string, string> FilesList = new Dictionary<string, string>(); // список всех файлов
        Dictionary<string, FileStream> SFilesList = new Dictionary<string, FileStream>(); // списко всех FileStream

        Dictionary<IPEndPoint, string> TransmittedFiles = new Dictionary<IPEndPoint, string>(); // соотвествие клиента и отправляемого ему файла
        Dictionary<IPEndPoint, int> OffsetsList = new Dictionary<IPEndPoint, int>(); // соответсвие клиента и оффсета в отправляемом ему файле

        public DataServer(int port) : base(port)
        {

        }

        protected override void Sending()
        {
            threadReset.WaitOne(); //ждём первого подключения

            while (true)
            {
                AcceptingBlock.WaitOne();

                foreach (IPEndPoint clientPoint in AvailableConnections)
                {
                    byte[] buffer = new byte[128];
                    FileStream file = SFilesList[TransmittedFiles[clientPoint]];

                    int bytesCount = file.Read(buffer, OffsetsList[clientPoint], 128);

                    byte[] buffer_ = new byte[bytesCount];
                    Array.Copy(buffer, 0, buffer_, 0, bytesCount);

                    file.Seek(OffsetsList[clientPoint] + 128, SeekOrigin.Begin);
                    OffsetsList[clientPoint] += 128;

                    Server.Send(buffer_, clientPoint);

                }

                AcceptingBlock.Release();

            }      

        }

        public void AddFile(string fileName, string id)
        {

        }
    }
}
