using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization
{
    public class NightWorldAuthorizationModel : AuthModelBase, IBasicAuthModel
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

        private string _password = null;
        public string Password
        {
            get => _password; set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private bool _isRememberMe = false;
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
            AuthCode authCode = Authentication.Instance.Auth(
                AccountType.NightWorld,
                Login?.Length == 0 ? null : Login,
                Password?.Length == 0 ? null : Password,
                IsRememberMe
                );
        }


        #endregion Public & Protected Methods


        #region Private Methods



        #endregion Private Methods
    }
}
