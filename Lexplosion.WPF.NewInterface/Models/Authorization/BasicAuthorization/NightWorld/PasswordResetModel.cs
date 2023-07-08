using System;

namespace Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization.NightWorld
{
    public class PasswordResetModel : VMBase
    {
        private string _login;
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public void GetCode() 
        {
            if (string.IsNullOrEmpty(_login))
            {

            }
            else 
            {
                new Exception("Code field is empty");
            }
        }
    }
}
