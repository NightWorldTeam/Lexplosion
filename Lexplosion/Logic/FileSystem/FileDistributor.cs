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

        private string _fileId;
        private Thread _informingThread;
        private AutoResetEvent _waitingInforming;

        private FileDistributor(string fileId, string UUID, string sessionToken)
        {
            _fileId = fileId;
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

        public static FileDistributor CreateDistribution(string filename)
        {
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
                using (FileStream fstream = File.OpenRead(filename))
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
                        Name = "XYI",
                        PublicRsaKey = _publicRsaKey,
                        ConfirmWord = _confirmWord
                    })
                });

                Runtime.DebugWrite(answer);

                _dataServer.AddFile(filename, hash);
                _distributionsCount++;

                return new FileDistributor(hash, GlobalData.User.UUID, GlobalData.User.SessionToken);
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
            }
        }
    }
}
