using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Core;
using System;

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
                _login= value;
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

        public NightWorldAuthorizationModel(DoNotificationCallback doNotification) : base(doNotification)
        {
            var test = LoadSavedAccount(AccountType.NightWorld);
        }

        #endregion Constructors


        #region Public & Protected Methods


        public void LogIn()
        {
            AuthCode authCode = Authentication.Instance.Auth(
                AccountType.NightWorld, (Login == "") ? null : Login, Password?.Length == 0 ? null : Password, IsRememberMe
                );
        }


        #endregion Public & Protected Methods


        #region Private Methods





        #endregion Private Methods
    }
}
