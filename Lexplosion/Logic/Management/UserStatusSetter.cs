using Lexplosion.Global;
using Lexplosion.Logic.Network;
using System.Threading;

namespace Lexplosion.Logic.Management
{
    static class UserStatusSetter
    {
        private static Statuses status;
        private static Statuses baseStatus = Statuses.Online;
        private static string clientName = null;

        public static void SetBaseStatus(Statuses status_)
        {
            baseStatus = status_;
            status = status_;

            if (status_ != Statuses.Offline)
            {
                Lexplosion.Run.TaskRun(delegate () 
                {
                    while (true)
                    {
                        int _status;
                        if (status == Statuses.OnlyOnline)
                        {
                            _status = (int)Statuses.Online;
                        }
                        else
                        {
                            _status = (int)status;
                        }

                        ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setUserActivity.php?status=" + _status + "&UUID=" + UserData.UUID + "&accessToken=" + UserData.AccessToken + "&gameClientName=" + clientName);
                        Thread.Sleep(54000); // Ждём 9 минут
                    }
                });
            }
        }

        public static void GameStart(string clientName_)
        {
            if (baseStatus == Statuses.Online)
            {
                clientName = clientName_;
                status = Statuses.InGame;
                ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setUserActivity.php?status=2&UUID=" + UserData.UUID + "&accessToken=" + UserData.AccessToken + "&gameClientName=" + clientName);
            }
        }

        public static void GameStop()
        {
            if (baseStatus == Statuses.Online)
            {
                status = baseStatus;
                ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setUserActivity.php?status=1&UUID=" + UserData.UUID + "&accessToken=" + UserData.AccessToken);
            }
        }

        public static void Exit()
        {
            ToServer.HttpGet(LaunсherSettings.URL.LogicScripts + "setUserActivity.php?status=0&UUID=" + UserData.UUID + "&accessToken=" + UserData.AccessToken);
        }

        public enum Statuses
        {
            Offline,
            Online,
            InGame,
            NotDisturb,
            OnlyOnline // это значит что нужно отображать стаус онлайн, но показывать что в игре не надо
        }
    }
}
