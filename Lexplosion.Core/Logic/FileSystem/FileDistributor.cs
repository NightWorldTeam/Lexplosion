using System.Collections.Generic;
using Lexplosion.Logic.Network;
using Lexplosion.Global;
using System.IO;
using System.Security.Cryptography;
using System;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System.Threading;

namespace Lexplosion.Logic.FileSystem
{
    public class FileDistributor
    {
        private static DataServer _dataServer = null;
        private static string _publicRsaKey;
        private static string _confirmWord;
        private static int _distributionsCount = 0;
        private static object _createLock = new object();
        private static HashSet<FileDistributor> _distributors = new();
        private static bool _isWork = true;
        private static List<string> _files = new();

        private Thread _informingThread;
        private AutoResetEvent _waitingInforming;

        public event Action OnClosed;

        public static string SharesDir
        {
            get
            {
                return WithDirectory.DirectoryPath + "/shares/files/";
            }
        }

        private FileDistributor(string fileId, string UUID, string sessionToken)
        {
            _waitingInforming = new AutoResetEvent(false);

            _informingThread = new Thread(() =>
            {
                var input = new Dictionary<string, string>
                {
                    ["UUID"] = UUID,
                    ["sessionToken"] = sessionToken,
                    ["FileId"] = fileId
                };

                try
                {
                    // раз в 2 минуты отправляем пакеты основному серверу информирующие о доступности раздачи
                    _waitingInforming.WaitOne(120000);
                    do
                    {
                        ToServer.HttpPost(LaunсherSettings.URL.UserApi + "setFileDistribution", input);
                    }
                    while (!_waitingInforming.WaitOne(120000));
                }
                finally
                {
                    ToServer.HttpPost(LaunсherSettings.URL.UserApi + "dropFileDistribution", input);
                }
            });

            _informingThread.Start();
        }

        public static FileDistributor CreateDistribution(string filePath, string name)
        {
            if (!_isWork) return null;

            _files.Add(filePath);

            lock (_createLock)
            {
                if (_dataServer == null)
                {
                    Сryptography.CreateRsaKeys(out RSAParameters privateKey, out _publicRsaKey);
                    _confirmWord = new Random().GenerateString(32);

                    _dataServer = new DataServer(privateKey, _confirmWord, GlobalData.User.UUID, GlobalData.User.SessionToken, LaunсherSettings.ServerIp);
                }

                //Получаем хэш файла
                string hash;
                using (FileStream fstream = File.OpenRead(filePath))
                {
                    hash = Сryptography.Sha256(fstream);
                }

                string answer = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "setFileDistribution", new Dictionary<string, string>
                {
                    ["UUID"] = GlobalData.User.UUID,
                    ["sessionToken"] = GlobalData.User.SessionToken,
                    ["FileId"] = hash,
                    ["Parameters"] = JsonConvert.SerializeObject(new DistributionData
                    {
                        Name = name,
                        PublicRsaKey = _publicRsaKey,
                        ConfirmWord = _confirmWord
                    })
                });

                Runtime.DebugWrite(answer);

                if (!_dataServer.AddFile(filePath, hash))
                {
                    return null;
                }

                _distributionsCount++;

                var dstr = new FileDistributor(hash, GlobalData.User.UUID, GlobalData.User.SessionToken);
                _distributors.Add(dstr);

                return dstr;
            }
        }

        public void Stop()
        {
            lock (_createLock)
            {
                _waitingInforming.Set();
                try { _informingThread.Abort(); } catch { }

                _distributionsCount--;
                if (_distributionsCount < 1)
                {
                    _dataServer.StopWork();
                    _dataServer = null;
                }

                if (_isWork)
                {
                    _distributors.Remove(this);

                    ThreadPool.QueueUserWorkItem(delegate (object state)
                    {
                        OnClosed?.Invoke();
                    });
                }
            }
        }

        public static void StopWork()
        {
            _isWork = false;

            foreach (var distributor in _distributors)
            {
                distributor.Stop();
            }

            _distributors = null;

            foreach (var file in _files)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            _files = null;
        }
    }
}
