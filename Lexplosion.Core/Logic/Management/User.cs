using System.Threading;
using Lexplosion.Global;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic
{
    public class User : VMBase
    {
        public string Login { get; private set; }
        public string UUID { get; private set; }
        public string AccessToken { get; private set; }
        public string SessionToken { get; private set; }
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

        public User(string login, string uuid, string accessToken, string sessionToken, AccountType accountType, ActivityStatus status)
        {
            Login = login;
            UUID = uuid;
            AccessToken = accessToken;
            SessionToken = sessionToken;
            AccountType = accountType;
            Status = status;

            if (accountType == AccountType.NightWorld)
            {
                // запускаем поток который постоянно будет уведомлять сервер о том что мы в сети
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    while (true)
                    {
                        ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=" + (int)Status + "&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
                        Thread.Sleep(54000); // Ждём 9 минут
                    }
                });
            }
        }

        private string _gameClientName = "";

        public void GameStart(string clientName_)
        {
            if (Status == ActivityStatus.Online)
            {
                _gameClientName = clientName_;
                Status = ActivityStatus.InGame;
                ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=2&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + clientName_);
            }
        }

        public void GameStop(string str)
        {
            if (Status == ActivityStatus.InGame)
            {
                Status = ActivityStatus.Online;
                ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=1&UUID=" + UUID + "&sessionToken=" + SessionToken);
            }
        }

        public void Exit()
        {
            ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=0&UUID=" + UUID + "&sessionToken=" + SessionToken);
        }

        public void ChangeBaseStatus(ActivityStatus status)
        {
            int statusInt = 0;
            if (status == ActivityStatus.Offline)
            {
                statusInt = 1;
            }
            else if (status == ActivityStatus.NotDisturb)
            {
                statusInt = 2;
            }

            ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setBaseStatus?activityStatus=" + statusInt + "&UUID=" + UUID + "&sessionToken=" + SessionToken);
            Status = status;
        }
    }
}
