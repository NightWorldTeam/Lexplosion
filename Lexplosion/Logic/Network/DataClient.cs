using Lexplosion.Logic.Network.SMP;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class DataClient : NetworkClient
    {
        const string clientType = "data-client"; // эта строка нужна при подключении к управляющему серверу
        private readonly string fileId;
        private readonly FileStream fstream;

        private long fileSize = 0;
        private long dataCount = 0;

        private bool isWorking;

        private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

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
            try
            {
                // получаем размер файла
                Bridge.Receive(out byte[] data);
                if (data.Length != 8) // размер файла должен быть типа long, то есть 8 байт. если нет - выходим
                {
                    return;
                }

                try
                {
                    fileSize = BitConverter.ToInt64(data, 0);
                }
                catch
                {
                    return;
                }

                new Thread(delegate ()
                {
                    SpeedClaculate();
                }).Start();

                long offset = 0;
                isWorking = Bridge.Receive(out data);

                while (isWorking)
                {
                    offset += data.Length;
                    dataCount += data.Length;
                    try // чисто перестраховка на случай если в оффсет как-то 0 попадёт
                    {
                        ProcentUpdate?.Invoke((offset / (double)fileSize) * 100);
                    }
                    catch { }

                    fstream.Write(data, 0, data.Length);
                    fstream.Seek(offset, SeekOrigin.Begin);

                    isWorking = Bridge.Receive(out data);
                }
            }
            catch { }
            resetEvent.Set();

            Bridge.Close();
            Close(null);
        }

        private void SpeedClaculate()
        {
            while (isWorking)
            {
                resetEvent.WaitOne(5000); // ждём 5 секунд

                long dataCount_ = dataCount;
                dataCount = 0;

                double byteToMillSec = dataCount_ / 5000.0;
                double bitToSec = (byteToMillSec * 8) * 1000;

                SpeedUpdate?.Invoke((bitToSec / (1024 * 1014)));
            }
        }

        protected override void Close(IPEndPoint point)
        {
            fstream.Close();
        }

        public delegate void ParametrUpdate(double value);

        public event ParametrUpdate ProcentUpdate;
        public event ParametrUpdate SpeedUpdate;
    }
}
