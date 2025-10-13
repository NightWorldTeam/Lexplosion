using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization.OAuth2
{
    public class MicrosoftAuthorizationModel : AuthModelBase, IOAuth2Model
    {
        private const string OAuth2Url = "https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED";


        private bool _isCanceled;


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
            _isCanceled = true;
        }

        public void LogIn()
        {
            FollowTo();
        }

        public void ManualInput(string data)
        {
            LogInInternal(data);
        }

        protected virtual void PerformMicrosoftAuthCode(MicrosoftAuthRes authCode, string microsoftData = "")
        {
            switch (authCode)
            {
                case MicrosoftAuthRes.Successful:
                    LogInInternal(microsoftData);
                    break;
                case MicrosoftAuthRes.UnknownError:
                    _appCore.MessageService.Error("AuthErrorMicrosoftUnknownError", true);
                    break;
                case MicrosoftAuthRes.UserDenied:
                    _appCore.MessageService.Error("AuthErrorMicrosoftUserDenied", true);
                    break;
                case MicrosoftAuthRes.Minor:
                    _appCore.MessageService.Error("AuthErrorMicrosoftMinor", true);
                    break;
                case MicrosoftAuthRes.NoXbox:
                    _appCore.MessageService.Error("AuthErrorNoXbox", true);
                    break;
                default:
                    break;
            }

            _appCore.ModalNavigationStore.Close();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void CommandReceiver_MicrosoftAuthPassed(string microsoftData, MicrosoftAuthRes result)
        {
            if (_isCanceled)
            {
                _isCanceled = false;
                return;
            }

            PerformMicrosoftAuthCode(result, microsoftData);
        }

        private void LogInInternal(string microsoftData)
        {
            var services = Runtime.ServicesContainer;
            var account = new Account(AccountType.Microsoft, services, services.DataFilesService);

            if (Account.LaunchAccount == null)
            {
                account.IsLaunch = true;
            }

            Runtime.TaskRun(() =>
            {
                var authCode = account.Auth(microsoftData);
                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformNightWorldAuthCode(account, authCode);
                    // TODO: WARINING: WINDOWS ONLY METHOD
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                });
            });
        }

        private void FollowTo()
        {
            _isCanceled = false;
            System.Diagnostics.Process.Start(OAuth2Url);
        }

        #endregion Private Methods
    }
}
