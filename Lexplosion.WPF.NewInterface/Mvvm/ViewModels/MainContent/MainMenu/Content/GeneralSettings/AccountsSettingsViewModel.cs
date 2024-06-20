using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public struct Accounts : INotifyPropertyChanged
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
                acc.IsActive = true;
                Account.SaveAll();
            });
        }

        private RelayCommand _doAccountLauncher;
        public ICommand DoAccountLauncher 
        {
            get => RelayCommand.GetCommand<Account>(ref _doAccountLauncher, (acc) => 
            {
                Runtime.DebugWrite($"{acc.AccountType} {acc.Login} executed.");
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }

    public class AccountsSettingsViewModel : ViewModelBase
    {
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


        #endregion Commands


        public AccountsSettingsViewModel()
        {
            Model = new AccountsSettingsModel();
        }
    }
}
