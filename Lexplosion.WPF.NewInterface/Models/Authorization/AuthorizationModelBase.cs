using Lexplosion.WPF.NewInterface.Core;
using System;

namespace Lexplosion.WPF.NewInterface.Models.Authorization
{
    public abstract class AuthorizationModelBase : VMBase, IAuthorizationModel
    {
        protected readonly Action<string, bool, bool> _successfulAuthorization;
        protected readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };


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


        public AuthorizationModelBase(Action<string, bool, bool> successfulAuthorization)
        {
            _successfulAuthorization = successfulAuthorization;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LogIn()
        {
            
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void AuthCodeHandler(AuthCode code)
        {
            
        }


        #endregion Private Methods
    }
}
