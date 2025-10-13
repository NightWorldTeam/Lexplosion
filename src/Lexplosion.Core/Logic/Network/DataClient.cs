using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Lexplosion.Logic.Network
{
    class DataClient : NetworkClient
    {
        const string CLIENT_TYPE = "data-client"; // эта строка нужна при подключении к управляющему серверу
        private readonly string _fileId; // он же хэш файла
        private readonly FileStream _fstream;

        private long _fileSize = 0;
        private long _dataCount = 0;

        private bool _isWorking = true;
        private bool _successfulTransfer = false;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent _workWait = new ManualResetEvent(false);

        private Thread _calculateThread;

        private byte[] _confirmWord;
        private RSAParameters _publicRsaKey;
        private byte[] _aesKey;
        private byte[] _aesIV;

        private bool _isManualClosed = false;

        private bool _isClosed = false;
        private object _closeLocker = new object();

        private string _fileName;

        /// <param name="publicRsaKey">Публичный rsa ключ хоста, с которого будет идити получение файла</param>
        /// <param name="confirmWord">Кодовое слово хоста. Используется для верификации передающего хоста</param>
        /// <param name="controlServer">IP кправляющего сервера</param>
        /// <param name="filename">Имя с которым файл будет сохранен на диске</param>
        /// <param name="fileId">ID файла получения, он же его хэш</param>
        public DataClient(RSAParameters publicRsaKey, string confirmWord, ControlServerData controlServer, string filename, string fileId) : base(CLIENT_TYPE, controlServer)
        {
            _fileName = filename;
            _fstream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
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
            Bridge.Send(Cryptography.RsaEncode(payload, _publicRsaKey));
        }

        protected override void Reading()
        {
            try
            {
                Runtime.DebugWrite("Start init");

                // получаем кодовое слово
                Bridge.Receive(out byte[] data);

                if (!_confirmWord.SequenceEqual(Cryptography.AesDecode(data, _aesKey, _aesIV)))
                {
                    Runtime.DebugConsoleWrite("Confirm word error");
                    _isWorking = false;
                    goto EndPoint;
                }

                //отправляем id файла
                Bridge.Send(Cryptography.AesEncode(Encoding.UTF8.GetBytes(_fileId), _aesKey, _aesIV));

                // получаем размер файла
                Bridge.Receive(out data);
                data = Cryptography.AesDecode(data, _aesKey, _aesIV);
                if (data.Length != 8) // размер файла должен быть типа long, то есть 8 байт. если нет - выходим
                {
                    Runtime.DebugConsoleWrite("data.Length != 8");
                    _isWorking = false;
                    goto EndPoint;
                }

                try
                {
                    _fileSize = BitConverter.ToInt64(data, 0);
                }
                catch
                {
                    _isWorking = false;
                    goto EndPoint;
                }

                _calculateThread = new Thread(SpeedClaculate);
                _calculateThread.Start();

                Runtime.DebugWrite("Start file download");
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = _aesKey;
                    aesAlg.IV = _aesIV;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    long offset = 0;
                    while (offset < _fileSize && (_isWorking = Bridge.Receive(out data)) && data.Length > 0)
                    {
                        data = Cryptography.CryptoDecode(decryptor, data);

                        offset += data.Length;
                        _dataCount += data.Length;
                        try // чисто перестраховка на случай если в _fileSize как-то 0 попадёт
                        {
                            ProcentUpdate?.Invoke((offset / (double)_fileSize) * 100);
                        }
                        catch { }

                        _fstream.Write(data, 0, data.Length);
                    }
                }

                Runtime.DebugConsoleWrite("End reading cycle");
            }
            catch (Exception ex)
            {
                Runtime.DebugConsoleWrite("Exception " + ex);
                _isWorking = false;
            }

        EndPoint:
            _resetEvent.Set();

            Bridge.Close();
            Close(null);

            _calculateThread?.Abort();
            Runtime.DebugConsoleWrite("EndPoint");
        }

        private void SpeedClaculate()
        {
            const int calculationDelay = 1000; //секунда
            while (_isWorking)
            {
                _resetEvent.WaitOne(calculationDelay); // ждём указанное время

                long dataCount = _dataCount;
                _dataCount = 0;

                double byteToMillSec = dataCount / calculationDelay;
                double bitToSec = (byteToMillSec * 8) * 1000;

                SpeedUpdate?.Invoke((bitToSec / (1024 * 1014)));
                Runtime.DebugWrite($"Speed Update {(bitToSec / (1024 * 1014))}");
            }
        }

        /// <summary>
        /// Дожидается оконачания получения файла
        /// </summary>
        /// <returns>результат скачивания.</returns>
        public FileRecvResult WorkWait()
        {
            _workWait.WaitOne();
            _workWait.Reset();

            if (_isManualClosed) return FileRecvResult.Canceled;
            return _successfulTransfer ? FileRecvResult.Successful : FileRecvResult.UnknownError;
        }

        protected override void Close(IPEndPoint point)
        {
            lock (_closeLocker)
            {
                if (!_isClosed)
                {
                    Runtime.DebugConsoleWrite("Close start. StackTrace: " + new System.Diagnostics.StackTrace());
                    _isClosed = true;

                    try
                    {
                        _fstream.Close();
                        using (FileStream fstream = File.OpenRead(_fileName))
                        {
                            string fileSha256 = Cryptography.Sha256(fstream);
                            _successfulTransfer = (fstream.Length == _fileSize) && (_fileId == fileSha256);
                            Runtime.DebugConsoleWrite("fstream.Length " + fstream.Length + ", _fileSize " + _fileSize + ", _fileId " + _fileId + ", fileSha256 " + fileSha256);
                        }
                    }
                    catch (Exception ex)
                    {
                        Runtime.DebugConsoleWrite("Exception " + ex);
                        _successfulTransfer = false;
                    }

                    _fstream.Close();
                    _workWait.Set();

                    Runtime.DebugConsoleWrite("Close end");
                }
            }
        }

        public void Close()
        {
            try
            {
                _isManualClosed = true;
                Close(null);
                Bridge?.Close();
            }
            catch { }
        }

        public event Action<double> ProcentUpdate;
        /// <summary>
        /// Обновление скорости передачи в Мбит/c. Обновляется каждые 5 секунд
        /// </summary>
        public event Action<double> SpeedUpdate;
    }
}