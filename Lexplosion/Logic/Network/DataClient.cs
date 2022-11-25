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
        private readonly string _fileId;
        private readonly FileStream _fstream;

        private long _fileSize = 0;
        private long _dataCount = 0;

        private bool _isWorking = true;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);  
        private readonly ManualResetEvent _workWait = new ManualResetEvent(false);

        private Thread _calculateThread;

        public DataClient(string controlServer, string filename, string fileId) : base(clientType, controlServer)
        {
            _fstream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            _fileId = fileId;
        }

        protected override void Sending()
        {
            Bridge.Send(Encoding.UTF8.GetBytes(_fileId));
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
                    _fileSize = BitConverter.ToInt64(data, 0);
                }
                catch
                {
                    return;
                }

                _calculateThread = new Thread(delegate ()
                {
                    SpeedClaculate();
                });

                _calculateThread.Start();

                long offset = 0;
                _isWorking = Bridge.Receive(out data);

                while (_isWorking && data.Length > 0)
                {
                    offset += data.Length;
                    _dataCount += data.Length;
                    try // чисто перестраховка на случай если в оффсет как-то 0 попадёт
                    {
                        ProcentUpdate?.Invoke((offset / (double)_fileSize) * 100);
                    }
                    catch { }

                    _fstream.Write(data, 0, data.Length);
                    _fstream.Seek(offset, SeekOrigin.Begin);

                    _isWorking = Bridge.Receive(out data);
                }
            }
            catch 
            {
                _isWorking = false;
            }

            _resetEvent.Set();

            Bridge.Close();
            Close(null);

            _calculateThread?.Abort();
        }

        private void SpeedClaculate()
        {
            while (_isWorking)
            {
                _resetEvent.WaitOne(5000); // ждём 5 секунд

                long dataCount = _dataCount;
                _dataCount = 0;

                double byteToMillSec = dataCount / 5000.0;
                double bitToSec = (byteToMillSec * 8) * 1000;

                SpeedUpdate?.Invoke((bitToSec / (1024 * 1014)));
            }
        }

        public void WorkWait()
        {
            _workWait.WaitOne();
            _workWait.Reset();
        }

        protected override void Close(IPEndPoint point)
        {
            _fstream.Close();
            _workWait.Set();
        }

        public event Action<double> ProcentUpdate;
        /// <summary>
        /// Обновление скорости передачи в Мбит/c. Обновляется каждые 5 секунд
        /// </summary>
        public event Action<double> SpeedUpdate;
    }
}
