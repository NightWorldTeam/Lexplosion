using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.FileSystem
{
    public class FileReceiver
    {
        #region staticData
        private class RecieverInfo
        {
            public string Login;
            public string UserUUID;
            public string FileId;
            public string Parameters;
        }

        private static object _dataClientInitLocker = new object();

        public static List<FileReceiver> GetDistributors()
        {
            var data = new List<FileReceiver>();
            string anwer = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "getFileDistributions", new Dictionary<string, string>
            {
                ["UUID"] = GlobalData.User.UUID,
                ["sessionToken"] = GlobalData.User.SessionToken
            });

            if (anwer == null)
            {
                return data;
            }

            try
            {
                var recieversInfo = JsonConvert.DeserializeObject<List<RecieverInfo>>(anwer);

                if (recieversInfo != null)
                {
                    foreach (RecieverInfo reciverInfo in recieversInfo)
                    {
                        var parameters = JsonConvert.DeserializeObject<DistributionData>(reciverInfo.Parameters);
                        data.Add(new FileReceiver(reciverInfo.Login, reciverInfo.UserUUID, reciverInfo.FileId, parameters));
                    }
                }
            }
            catch { }

            return data;
        }

        #endregion

        private string _ownerLogin;
        private string _ownerUUID;
        private string _fileId;

        private DistributionState _state;
        private DataClient _dataClient = null;
        private DistributionData _info;

        public event Action<double> ProcentUpdate;
        public event Action<double> SpeedUpdate;
        public event Action<DistributionState> StateChanged;

        private object _locker = new object();

        public string OwnerLogin
        {
            get => _ownerLogin;
        }

        public DistributionState GetState
        {
            get => _state;
        }

        public string Name
        {
            get => _info?.Name;
        }

        public string Id
        {
            get => _fileId;
        }

        private FileReceiver(string ownerLogin, string ownerUUID, string fileId, DistributionData info)
        {
            _ownerLogin = ownerLogin;
            _ownerUUID = ownerUUID;
            _fileId = fileId;
            _info = info;

            _state = DistributionState.InQueue;
        }

        public void CancelDownload()
        {
            lock (_locker)
            {
                _dataClient?.Close();
                _dataClient = null;
            }
        }

        public FileRecvResult StartDownload(string fileName)
        {
            lock (_locker)
            {
                var publicKey = Cryptography.DecodeRsaParams(_info.PublicRsaKey);
                var serverData = new ControlServerData(LaunсherSettings.ServerIp);

                _dataClient?.Close();
                _dataClient = new DataClient(publicKey, _info.ConfirmWord, serverData, fileName, _fileId);
                _dataClient.SpeedUpdate += SpeedUpdate;
                _dataClient.ProcentUpdate += ProcentUpdate;

                lock (_dataClientInitLocker)
                {
                    bool result = _dataClient.Initialization(GlobalData.User.UUID, GlobalData.User.SessionToken, _ownerUUID);
                    if (!result)
                    {
                        _dataClient.Close();
                        return FileRecvResult.ConnectionClose;
                    }
                }

                _state = DistributionState.InProcess;
                StateChanged?.Invoke(_state);
            }

            return _dataClient?.WorkWait() ?? FileRecvResult.Canceled;
        }
    }
}
