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
            public string login;
            public string userUUID;
            public string fileId;
            public DistributionData parameters;
        }

        private static List<FileReceiver> _toProcessing = new List<FileReceiver>();

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
                        data.Add(new FileReceiver(reciverInfo.login, reciverInfo.userUUID, reciverInfo.fileId, reciverInfo.parameters));
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

        public event Action<double> ProcentUpdate;
        public event Action<double> SpeedUpdate;
        public event Action<DistributionState> StateChanged;

        public DistributionState GetState
        {
            get => _state;
        }

        private FileReceiver(string ownerLogin, string ownerUUID, string fileId, DistributionData info)
        {
            _ownerLogin = ownerLogin;
            _ownerUUID = ownerUUID;
            _fileId = fileId;

            _state = DistributionState.InQueue;
        }


        public void StartDownload(string fileName)
        {
            _state = DistributionState.InProcess;
            StateChanged?.Invoke(_state);

            _dataClient = new DataClient(LaunсherSettings.ServerIp, fileName, _fileId);
            _dataClient.SpeedUpdate += SpeedUpdate;
            _dataClient.ProcentUpdate += ProcentUpdate;

            _dataClient.Initialization(GlobalData.User.UUID, GlobalData.User.SessionToken, _ownerUUID);
            _dataClient.WorkWait();
        }
    }
}
