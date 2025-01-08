using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Security.Principal;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization
{
    public sealed class NightWorldAuthorizationModel : AuthModelBase, IBasicAuthModel
    {
        #region Properties


        private string _login = string.Empty;
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password; set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private bool _isRememberMe;
        public bool IsRememberMe
        {
            get => _isRememberMe; set
            {
                _isRememberMe = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constuctors


        public NightWorldAuthorizationModel(DoNotificationCallback doNotification, string loadedLogin = "") : base(doNotification)
        {
            Login = loadedLogin;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LogIn()
        {
            var account = new Account(AccountType.NightWorld, Login);

            Runtime.TaskRun(() =>
            {
                var authCode = account.Auth(Password);
                App.Current.Dispatcher.Invoke(() =>
                {
                    AuthorizationCodeHandler(account, authCode);
                });
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods



        private void AuthorizationCodeHandler(Account account, AuthCode code)
        {
            switch (code)
            {
                case AuthCode.Successfully:
                    {
                        account.IsActive = true;
                        account.Save();
                    }
                    break;
                case AuthCode.DataError:
                    break;
                case AuthCode.NoConnect:
                    break;
                case AuthCode.TokenError:
                    break;
                case AuthCode.SessionExpired:
                    break;
                case AuthCode.NeedMicrosoftAuth:
                    break;
                default:
                    break;
            }
        }



        #endregion Private Methods
    }
}
