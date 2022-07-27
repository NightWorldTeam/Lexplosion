using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic
{
    class User : VMBase
    {
        public string Login { get; private set; } = "";
        public string UUID { get; private set; } = "00000000-0000-0000-0000-000000000000";
        public string AccessToken { get; private set; } = "null";
        public string SessionToken { get; private set; } = "";
        public AccountType AccountType { get; private set; }
        public ActivityStatus Status { get; private set; }

        private string _gameClientName = "";

        public AuthCode Auth(string login, string password, bool saveUser)
        {
            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    Login = response["login"];
                    UUID = response["UUID"];
                    AccessToken = response["accesToken"];
                    SessionToken = response["sessionToken"];

                    if (saveUser)
                    {
                        DataFilesManager.SaveAccount(login, password);
                    }

                    AccountType = AccountType.NightWorld;
                    Status = ActivityStatus.Online;

                    // запускаем поток который постоянно будет уведомлять сервер о том что мы в сети
                    Lexplosion.Run.TaskRun(delegate ()
                    {
                        while (true)
                        {
                            ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setActivity?status=" + (int)Status + "&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
                            Thread.Sleep(54000); // Ждём 9 минут
                        }
                    });

                    return AuthCode.Successfully;
                }
                else
                {
                    return AuthCode.DataError;
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

        }
    }
}
