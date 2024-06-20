using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Modal
{
    public class AccountFactoryModel : ViewModelBase
    {
        private Action<Account> _addAccount;

        #region Properties


        private AccountType _accountType = AccountType.NightWorld;
        public AccountType AccountType
        {
            get => _accountType; set
            {
                _accountType = value;
                OnPropertyChanged();
            }
        }

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

        #endregion Properties

        // NoAuth       |$>> Login
        // NightWorld   |$>> Login + Password
        // Microsoft    |$>> Login + redirect to Microsoft

        public AccountFactoryModel(Action<Account> addAccount)
        {
            _addAccount = addAccount;
        }

        public void Auth()
        {
            switch (AccountType)
            {
                case AccountType.NoAuth: 
                    { 
                        NoAuth(); 
                        break; 
                    }
                case AccountType.NightWorld:
                    {
                        NightWorldAuth();
                        break;
                    }

                case AccountType.Microsoft:
                    {
                        MicrosoftAuth();
                        break;
                    }
                default:
                    break;
            };
        }

        private void NoAuth() 
        {
            var account = new Account(AccountType.NoAuth, Login);
            account.Save();
            _addAccount(account);
        }

        private void NightWorldAuth()
        {
            var account = new Account(AccountType.NightWorld, Login);
            var code1 = account.Auth(Password);
            if (code1 == AuthCode.Successfully) 
            { 
                account.Save();
                _addAccount(account);
            }
        }

        private void MicrosoftAuth()
        {
            var account = new Account(AccountType.Microsoft);

            void successAuth(string token, MicrosoftAuthRes res)
            {
                if (res == MicrosoftAuthRes.Successful)
                {
                    account.Auth(token);
                    account.Save();
                    _addAccount(account);
                    CommandReceiver.MicrosoftAuthPassed -= successAuth;
                }
            }

            CommandReceiver.MicrosoftAuthPassed += successAuth;
            // TODO: Засунуть это в констану.
            System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
        }
    }
}
