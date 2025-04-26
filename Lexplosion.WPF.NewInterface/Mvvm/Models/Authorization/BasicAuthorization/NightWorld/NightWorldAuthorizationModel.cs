using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization
{
    public sealed class NightWorldAuthorizationModel : AuthModelBase, IBasicAuthModel
    {
        private readonly AppCore _appCore;


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


        public NightWorldAuthorizationModel(AppCore appCore) : base(appCore)
        {
            _appCore = appCore;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LogIn()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password)) 
            {
                _appCore.MessageService.Info("LoginOrPasswordNotFilled", true);
                return;
            }

			var services = Runtime.ServicesContainer;

			var account = new Account(AccountType.NightWorld, services, services.DataFilesService, Login);

            Runtime.TaskRun(() =>
            {
                var authCode = account.Auth(Password);
                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformNightWorldAuthCode(account, authCode);
                });
            });
        }


        #endregion Public & Protected Methods
    }
}
