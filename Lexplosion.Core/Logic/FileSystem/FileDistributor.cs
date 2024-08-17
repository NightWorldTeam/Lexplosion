using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Network;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using System.Collections.Concurrent;

namespace Lexplosion.Logic.FileSystem
{
    public class FileDistributor
    {
        private static DataServer _dataServer = null;
        private static string _publicRsaKey;
        private static string _confirmWord;
        private static int _distributionsCount = 0;
        private static object _createLock = new object();
        private static ConcurrentDictionary<string, FileDistributor> _distributors = new();
        private static bool _isWork = true;
        private static List<string> _files = new();

        private Thread _informingThread;
        private AutoResetEvent _waitingInforming;
        private ConcurrentDictionary<string, Player> _connectedUsers = new();
        private string _fileId;

        public event Action OnClosed;

        public event Action<Player> UserConnected;
        public event Action<Player> UserDisconnected;

        public static string SharesDir
        {
            get
            {
                return WithDirectory.DirectoryPath + "/shares/files/";
            }
        }

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

        public static FileDistributor CreateDistribution(string filePath, string name, string userUUID, string userSessionToken)
        {
            if (!_isWork) return null;

            _files.Add(filePath);

            lock (_createLock)
            {
                if (_dataServer == null)
                {
                    Cryptography.CreateRsaKeys(out RSAParameters privateKey, out _publicRsaKey);
                    _confirmWord = new Random().GenerateString(32);

                    var serverData = new ControlServerData(LaunсherSettings.ServerIp);
                    _dataServer = new DataServer(privateKey, _confirmWord, userUUID, userSessionToken, serverData);

                    _dataServer.ConnectingUser += (string uuid) =>
                    {
                        var player = new Player(uuid,
                            () =>
                            {
                                _dataServer.KickClient(uuid);
                            },
                            () =>
                            {
                                _dataServer.UnkickClient(uuid);
                            }
                        );

                        string fileId = _dataServer.GetDownloadedFileId(userUUID);
                        _distributors.TryGetValue(fileId, out FileDistributor value);
                        if (value == null) return;

                        value._connectedUsers[uuid] = player;
                        value.UserConnected?.Invoke(player);
                    };

                    _dataServer.DisconnectedUser += (string uuid) =>
                    {
                        string fileId = _dataServer.GetDownloadedFileId(userUUID);
                        _distributors.TryGetValue(fileId, out FileDistributor value);
                        if (value == null) return;

                        value._connectedUsers.TryRemove(uuid, out Player player);
                        value.UserDisconnected?.Invoke(player);
                    };
                }

                //Получаем хэш файла
                string hash;
                using (FileStream fstream = File.OpenRead(filePath))
                {
                    hash = Cryptography.Sha256(fstream);
                }

                if (!_dataServer.AddFile(filePath, hash))
                {
                    return null;
                }

                string answer = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "setFileDistribution", new Dictionary<string, string>
                {
                    ["UUID"] = userUUID,
                    ["sessionToken"] = userSessionToken,
                    ["FileId"] = hash,
                    ["Parameters"] = JsonConvert.SerializeObject(new DistributionData
                    {
                        Name = name,
                        PublicRsaKey = _publicRsaKey,
                        ConfirmWord = _confirmWord
                    })
                });

                Runtime.DebugWrite(answer);

                _distributionsCount++;

                var dstr = new FileDistributor(hash, userUUID, userSessionToken);
                _distributors[dstr._fileId] = dstr;

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
                    _dataServer?.StopWork();
                    _dataServer = null;
                }

                if (_isWork)
                {
                    _distributors.TryRemove(this._fileId, out _);

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

            // TODO: тут пихнуть какой-нибудь лукер, ибо в _distributors может null присвоится во время работы CreateDistribution
            if (_distributors != null)
            {
                foreach (var distributor in _distributors)
                {
                    distributor.Value.Stop();
                }

                _distributors = null;
            }

            if (_files != null)
            {
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
}
