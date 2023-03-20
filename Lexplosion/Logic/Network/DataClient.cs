using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network
{
    class DataClient : NetworkClient
    {
        const string clientType = "data-client"; // эта строка нужна при подключении к управляющему серверу
        private readonly string _fileId;
        private readonly FileStream _fstream;

        private long _fileSize = 0;
        private long _dataCount = 0;
        private byte[] _fileSha256;

        private bool _isWorking = true;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent _workWait = new ManualResetEvent(false);

        private Thread _calculateThread;

        private byte[] _confirmWord;
        private RSAParameters _publicRsaKey;
        private byte[] _aesKey;
        private byte[] _aesIV;

        public DataClient(RSAParameters publicRsaKey, string confirmWord, string controlServer, string filename, string fileId) : base(clientType, controlServer)
        {
            _fstream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            _fileId = fileId;

            _confirmWord = Encoding.UTF8.GetBytes(confirmWord);
            _publicRsaKey = publicRsaKey;

            var rnd = new Random();
            _aesKey = rnd.GenerateBytes(32);
            _aesIV = rnd.GenerateBytes(16);
        }

        protected override void Sending()
        {
            // отправляем сообщение о подключении
            Bridge.Send(new byte[] { 0x00 });

            // отправляем сообщение с aes ключами
            byte[] payload = new byte[48];
            Array.Copy(_aesKey, 0, payload, 0, 32);
            Array.Copy(_aesIV, 0, payload, 32, 16);
            Bridge.Send(Сryptography.RsaEncode(payload, _publicRsaKey));
        }

        protected override void Reading()
        {
            try
            {
                // получаем кодовое слово
                Bridge.Receive(out byte[] data);

                if (!_confirmWord.SequenceEqual(Сryptography.AesDecode(data, _aesKey, _aesIV)))
                {
                    // TODO: тут выходить
                }

                //отправляем id файла
                Bridge.Send(Сryptography.AesEncode(Encoding.UTF8.GetBytes(_fileId), _aesKey, _aesIV));

                // получаем размер файла
                Bridge.Receive(out data);
                data = Сryptography.AesDecode(data, _aesKey, _aesIV);
                if (data.Length != 8) // размер файла должен быть типа long, то есть 8 байт. если нет - выходим
                {
                    return; // TODO: а точно достаточно return сделать?
                }

                try
                {
                    _fileSize = BitConverter.ToInt64(data, 0);
                }
                catch
                {
                    return;
                }

                // получаем хэш файла
                Bridge.Receive(out data);
                _fileSha256 = Сryptography.AesDecode(data, _aesKey, _aesIV);

                _calculateThread = new Thread(SpeedClaculate);
                _calculateThread.Start();

                long offset = 0;
                _isWorking = Bridge.Receive(out data);
                while (_isWorking && data.Length > 0 && offset < _fileSize)
                {
                    data = Сryptography.AesDecode(data, _aesKey, _aesIV);

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
