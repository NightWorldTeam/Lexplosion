using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.GeneralSettings
{
    public sealed class AccountsSettingsModel : ViewModelBase
    {
        private readonly AppCore _appCore;


        #region Properties


        private ObservableCollection<AccountItem> _accounts { get; } = [];
        public FiltableObservableCollection Accounts { get; } = [];


        public int ActiveAccountIndex { get; private set; }
        public int LaunchAccountIndex { get; private set; }

        private string _searchBoxText;
        public string SearchBoxText
        {
            get => _searchBoxText; set
            {
                _searchBoxText = value;
                Accounts.Filter = (obj) => (obj as AccountItem).Account.Login.IndexOf(_searchBoxText, StringComparison.InvariantCultureIgnoreCase) > -1;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Constructors


        public AccountsSettingsModel(AppCore appCore)
        {
            _appCore = appCore;
            Accounts.Source = _accounts;

            foreach (var account in Account.List)
            {
                _accounts.Add(new(account));
            }

            Account.AccountAdded += Account_AccountAdded;
        }

        private void Account_AccountAdded(Account obj)
        {
            AddAccount(obj);
        }


        #endregion Constructors


        public void AddAccount(Account account)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _accounts.Add(new AccountItem(account));
            });
        }

        public void RemoveAccount(Account account)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _accounts.Remove(new AccountItem(account));
                account.RemoveFromList();
            });
        }

        public void ReauthAccount(Account acc)
        {
            Runtime.TaskRun(() =>
            {
                var authResult = acc.Auth();

                if (authResult == AuthCode.Successfully)
                    Account.SaveAll();

                else if (acc.AccountType == AccountType.Microsoft && (authResult == AuthCode.TokenError || authResult == AuthCode.SessionExpired))
                {
                    AuthMicrosoftAccount(acc);
                }
                else
                {
                    // TODO: Notification
                    // TODO: Error Handler
                }
            });
        }

        public void AuthMicrosoftAccount(Account account)
        {
            void successAuth(string token, MicrosoftAuthRes res)
            {
                if (res == MicrosoftAuthRes.Successful)
                {
                    Runtime.TaskRun(() => {
                        var code = account.Auth(token);
                        if (code == AuthCode.Successfully)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Account.SaveAll();
                                CommandReceiver.MicrosoftAuthPassed -= successAuth;
                                //IsAuthorizationInProcess = false;
                            });
                        }
                    });
                }
            }

            CommandReceiver.MicrosoftAuthPassed += successAuth;

            System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
        }

        public void ActivateAccount(Account acc)
        {
            if (!acc.IsAuthed)
            {
                Runtime.TaskRun(() =>
                {
                    var authResult = acc.Auth();
                    if (authResult == AuthCode.Successfully)
                    {
                        acc.IsActive = true;
                        Account.SaveAll();
                    }

                    Runtime.DebugWrite(acc.IsLaunch);
                });

                return;
            }

            acc.IsActive = true;
            Account.SaveAll();
        }

        public void DoAccountLauncherCommand(Account acc) 
        {
            if (!acc.IsAuthed)
            {
                Runtime.TaskRun(() =>
                {
                    var authResult = acc.Auth();
                    if (authResult == AuthCode.Successfully)
                    {
                        acc.IsLaunch = true;
                        Account.SaveAll();
                    }
                    Runtime.DebugWrite(acc.IsLaunch);
                });

                return;
            }

            acc.IsLaunch = true;
            Account.SaveAll();
        }

        public void SignOut(Account acc)
        {
            Runtime.DebugWrite($"{acc.AccountType} {acc.Login} executed.");

            _appCore.ModalNavigationStore.Open(new ConfirmActionViewModel("Удаление аккаунта", "ха-ха-ха",
                (obj) =>
                {
                    RemoveAccount(acc);
                    Account.SaveAll();
                }));
        }
    }
}
