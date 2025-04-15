using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
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
            var account = new Account(AccountType.NoAuth, Nickname);
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
