using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class Accounts : ObservableObject
    {
        private ObservableCollection<Account> _list;


        #region Properties


        public AccountType Type { get; }
        public string IconSource { get; }
        public CollectionViewSource List { get; } = new();
        public int Count { get => _list.Count; }
        public bool HasAccounts { get => _list.Count > 0; }
        public bool IsNightWorldAccount { get; }


        #endregion Properties


        #region Commands


        private RelayCommand _activateAccountCommand;
        public ICommand ActivateAccountCommand 
        {
            get => RelayCommand.GetCommand<Account>(ref _activateAccountCommand, (acc) => 
            {
                if (!acc.IsAuthed) {
                    acc.Auth();
                }

                acc.IsActive = true;
                Account.SaveAll();
            });
        }

        private RelayCommand _doAccountLauncher;
        public ICommand DoAccountLauncher 
        {
            get => RelayCommand.GetCommand<Account>(ref _doAccountLauncher, (acc) => 
            {
                if (!acc.IsAuthed) {
                    acc.Auth();
                }

                acc.IsLaunch = true;
                Account.SaveAll();
            });
        }


        #endregion Commands


        #region Commands


        public Accounts(AccountType type, IList<Account> list)
        {
            Type = type;
            IsNightWorldAccount = type == AccountType.NightWorld;
            _list = new ObservableCollection<Account>(list);
            List.Source = _list;
            IconSource = $"pack://application:,,,/assets/images/icons/{Type.ToString().ToLower()}.png";
        }


        #endregion Constructors


        #region Public Methods


        public void AddAccount(Account account) 
        {
            _list.Add(account);
            OnPropertyChanged(nameof(HasAccounts));
        }

        public void FilterAccountsByLogin(string value) 
        {
            if (List.View == null)
                return;

            value ??= value;
            List.View.Filter = (i => (i as Account).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }


        #endregion Public Methods
    }

    public sealed class AccountsSettingsModel : ViewModelBase
    {
        #region Properties


        public IList<Accounts> AccountsByType { get; } = new ObservableCollection<Accounts>();

        public int ActiveAccountIndex { get; private set; }
        public int LaunchAccountIndex { get; private set; }

        private string _searchBoxText;
        public string SearchBoxText
        {
            get => _searchBoxText; set
            {
                _searchBoxText = value;
                foreach (var accounts in AccountsByType) 
                { 
                    accounts.FilterAccountsByLogin(value);
                }
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Constructors


        public AccountsSettingsModel()
        {
            var enumValues = Enum.GetValues(typeof(AccountType));
            var enumValueCount = enumValues.Length;

            foreach (AccountType i in enumValues) 
            {
                AccountsByType.Add(new Accounts(i, new List<Account>()));
            }

            foreach (var account in Account.List) 
            {
                if (Account.ActiveAccount == account)
                    ActiveAccountIndex = AccountsByType[(int)account.AccountType].Count;

                if (Account.LaunchedAccount == account)
                    LaunchAccountIndex = AccountsByType[(int)account.AccountType].Count;

                AccountsByType[(int)account.AccountType].AddAccount(account);
            }
        }


        #endregion Constructors


        public void AddAccount(Account account) 
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                AccountsByType[(int)account.AccountType].AddAccount(account);
            });
        }
    }

    public class AccountsSettingsViewModel : ViewModelBase
    {
        private ModalNavigationStore _modalNavigationStore;

        public AccountsSettingsModel Model { get; }


        #region Commands


        private RelayCommand _activateAccountCommand;
        public ICommand ActivateAccountCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _activateAccountCommand, (acc) =>
            {
                acc.IsActive = true;
                Account.SaveAll();
            });
        }

        private RelayCommand _doAccountLauncherCommand;
        public ICommand DoAccountLauncherCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _doAccountLauncherCommand, (acc) =>
            {
                Runtime.DebugWrite($"{acc.AccountType} {acc.Login} executed.");
                acc.IsLaunch = true;
                Account.SaveAll();
            });
        }


        private RelayCommand _singOutCommand;
        public ICommand SingOutCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _singOutCommand, (acc) =>
            {
                Runtime.DebugWrite($"{acc.AccountType} {acc.Login} executed.");
                Account.SaveAll();
            });
        }


        private RelayCommand _addAccountCommand;
        public ICommand OpenAccountFactoryCommand {
            get => RelayCommand.GetCommand(ref _addAccountCommand, () => 
            {
                _modalNavigationStore.Open(new AccountFactoryViewModel(Model.AddAccount));
            });
        }




        #endregion Commands


        public AccountsSettingsViewModel(ModalNavigationStore modalNavigationStore)
        {
            _modalNavigationStore = modalNavigationStore;
            Model = new AccountsSettingsModel();
        }
    }
}
