using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization
{
    public sealed class NoAccountAuthorizationModel : ViewModelBase, IAuthModel
    {
        private string _nickname;
        public string Nickname 
        {
            get => _nickname; set 
            {
                _nickname = value;

                IsValid = IsNicknameValid(value);

                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid { get; private set; }


        public NoAccountAuthorizationModel()
        {
            
        }


        public void LogIn() 
        {
			var services = Runtime.ServicesContainer;
            var account = new Account(AccountType.NoAuth, services, services.DataFilesService, Nickname);
            account.IsLaunch = true;
            account.Save();
        }

        private bool IsNicknameValid(string nickname) 
        {
            if (string.IsNullOrWhiteSpace(nickname)) 
            {
                return false;
            }

            if (nickname.Length < 3 && nickname.Length > 16) 
            {
                return false;
            }

            return true;
        }
    }
}
