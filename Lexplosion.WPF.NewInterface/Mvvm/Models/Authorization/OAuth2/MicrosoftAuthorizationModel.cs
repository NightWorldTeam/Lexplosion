using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.OAuth2
{
    public class MicrosoftAuthorizationModel : AuthModelBase, IOAuth2Model
    {
        private readonly string OAuth2Url = "https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED";


        #region Constuctors


        public MicrosoftAuthorizationModel(AppCore appCore) : base(appCore)
        {
            // Подписываемся на обработку данных с браузера
            CommandReceiver.MicrosoftAuthPassed += CommandReceiver_MicrosoftAuthPassed;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void FollowTo()
        {
            System.Diagnostics.Process.Start(OAuth2Url);
        }

        public void LogIn()
        {
            FollowTo();
        }

        public void ManualInput(string data)
        {
            LogIn(data);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void CommandReceiver_MicrosoftAuthPassed(string microsoftData, MicrosoftAuthRes result)
        {

        }


        private void LogIn(string microsoftData) 
        {
            var account = new Account(AccountType.Microsoft);

            Runtime.TaskRun(() =>
            {
                var authCode = account.Auth(microsoftData);
                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformAuthCode(account, authCode);
                    // TODO: WARINING: WINDOWS ONLY METHOD
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                });
            });
        }


        #endregion Private Methods
    }
}
