using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Lexplosion.Logic.Network
{
    class DataClient : NetworkClient
    {
        const string clientType = "data-client"; // эта строка нужна при подключении к управляющему серверу
        private readonly string fileId;
        private readonly FileStream fstream;

        public DataClient(string server, string filename, string fileId_) : base(clientType, server)
        {
            fstream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            fileId = fileId_;
        }

        protected override void Sending()
        {
            Bridge.Send(Encoding.UTF8.GetBytes(fileId));
        }

        protected override void Reading()
        {
            int i = 0;
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                long offset = 0;
                bool isWorking = Bridge.Receive(out byte[] data);
                stopwatch.Start();
                int count = 0;
                i++;

                while (isWorking)
                {
                    offset += data.Length;

                    fstream.Write(data, 0, data.Length);
                    fstream.Seek(offset, SeekOrigin.Begin);

                    isWorking = Bridge.Receive(out data);
                    i++;

                    count += data.Length;
                    if (i == 200)
                    {
                        stopwatch.Stop();
                        long timeDelta = stopwatch.ElapsedMilliseconds;
                        double byteToMillSec = (double)count / (double)timeDelta;
                        double bitToSec = (byteToMillSec * 8) * 1000;
                        Console.WriteLine((bitToSec / (1024 * 1014)).ToString("0.######") + " Mbit/sec");
                        count = 0;
                        i = 0;
                        stopwatch = new Stopwatch();
                        stopwatch.Start();
                    }
                }
            }
            catch { }

            Bridge.Close();
            Close(null);

        }

        public override void Close(IPEndPoint point)
        {
            //fstream.Close();
        }
    }
}
