using Lexplosion.Logic.Management.Authentication;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using System;

namespace Lexplosion.WPF.NewInterface.Models.Authorization.OAuth2
{
    public class MicrosoftAuthorizationModel : AuthModelBase, IOAuth2Model
    {
        private readonly string OAuth2Url = "https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED";


        #region Constuctors


        public MicrosoftAuthorizationModel(DoNotificationCallback doNotification) : base(doNotification)
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
            var authCode = Authentication.Instance.Auth(AccountType.Microsoft, "", data, true);
            PerformAuthCode(AccountType.Microsoft, authCode);
            // WARINING: WINDOWS ONLY METHOD
            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void CommandReceiver_MicrosoftAuthPassed(string microsoftData, MicrosoftAuthRes result)
        {
            var authCode = Authentication.Instance.Auth(AccountType.Microsoft, "", microsoftData, true);
            PerformAuthCode(AccountType.Microsoft, authCode);
            // WARINING: WINDOWS ONLY METHOD
            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
        }


        #endregion Private Methods
    }
}
