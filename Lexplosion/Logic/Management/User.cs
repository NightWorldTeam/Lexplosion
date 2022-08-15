using System.Collections.Generic;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic
{
    class User : VMBase
    {
        public string Login { get; private set; } = "";
        public string UUID { get; private set; } = "00000000-0000-0000-0000-000000000000";
        public string AccessToken { get; private set; } = "null";
        public string SessionToken { get; private set; } = "";
        public AccountType AccountType { get; private set; }

        private ActivityStatus _status;
        public ActivityStatus Status 
        {
            get => _status; 
            private set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _gameClientName = "";

        public AuthCode Auth(string login, string password, bool saveUser, AccountType accountType)
        {
            accountType = AccountType.Mojang;
            AuthResult response = null;

            if (accountType == AccountType.NightWorld)
            {
                response = ToServer.Authorization(login, password);
            }
            else if (accountType == AccountType.Mojang)
            {
                response = MojangApi.Authorization(login, password);
            }

            if (response != null)
            {
                if (response.Status == AuthCode.Successfully)
                {
                    Login = response.Login;
                    UUID = response.UUID;
                    AccessToken = response.AccesToken;
                    SessionToken = response.SessionToken;

                    if (saveUser)
                    {
                        DataFilesManager.SaveAccount(login, password, accountType);
                    }

                    AccountType = accountType;
                    Status = ActivityStatus.Online;

                    if (accountType == AccountType.NightWorld)
                    {
                        // запускаем поток который постоянно будет уведомлять сервер о том что мы в сети
                        Lexplosion.Run.TaskRun(delegate ()
                        {
                            while (true)
                            {
                                ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setActivity?status=" + (int)Status + "&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
                                Thread.Sleep(54000); // Ждём 9 минут
                            }
                        });
                    }

                    return AuthCode.Successfully;
                }
                else
                {
                    return response.Status;
                }
            }
            else
            {
                return AuthCode.NoConnect;
            }
         }

        public void GameStart(string clientName_)
        {
            if (Status == ActivityStatus.Online)
            {
                _gameClientName = clientName_;
                Status = ActivityStatus.InGame;
                ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setActivity?status=2&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + clientName_);
            }
        }

        public void GameStop()
        {
            if (Status == ActivityStatus.InGame)
            {
                Status = ActivityStatus.Online;
                ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setActivity?status=1&UUID=" + UUID + "&sessionToken=" + SessionToken);
            }
        }

        public void Exit()
        {
            ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setActivity?status=0&UUID=" + UUID + "&sessionToken=" + SessionToken);
        }

        public void ChangeBaseStatus(ActivityStatus status)
        {
            Status = status;
        }
    }
}
