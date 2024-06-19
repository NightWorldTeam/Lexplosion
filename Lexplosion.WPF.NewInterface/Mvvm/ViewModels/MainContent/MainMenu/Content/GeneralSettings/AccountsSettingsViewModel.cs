using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public struct Accounts : INotifyPropertyChanged
    {
        public AccountType Type { get; }
        public string IconSource { get; }
        public IList<Account> List { get; }
        public bool HasAccounts { get => List.Count > 0; }
        public bool IsNightWorldAccount { get; }


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
                acc.IsLaunch = true;
                Account.SaveAll();
            });
        }

        public Accounts(AccountType type, IList<Account> list)
        {
            Type = type;
            IsNightWorldAccount = type == AccountType.NightWorld;
            List = new ObservableCollection<Account>(list);
            IconSource = $"pack://application:,,,/assets/images/icons/{Type.ToString().ToLower()}.png";
        }

        public void AddAccount(Account account) 
        {
            List.Add(account);
            OnPropertyChanged(nameof(HasAccounts));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class AccountsSettingsModel : ViewModelBase
    {
        public IList<Accounts> AccountsByType { get; } = new ObservableCollection<Accounts>();

        public int ActiveAccountIndex { get; private set; }
        public int LaunchAccountIndex { get; private set; }

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
                    ActiveAccountIndex = AccountsByType[(int)account.AccountType].List.Count;

                if (Account.LaunchedAccount == account)
                    LaunchAccountIndex = AccountsByType[(int)account.AccountType].List.Count;

                AccountsByType[(int)account.AccountType].AddAccount(account);
            }
        }


        public void ActiveAccount(Account account) 
        {
            account.IsActive = true;
            AccountsByType[(int)account.AccountType].List.IndexOf(account);

        }
    }

    public class AccountsSettingsViewModel : ViewModelBase
    {
        public AccountsSettingsModel Model { get; }

        public AccountsSettingsViewModel()
        {
            Model = new AccountsSettingsModel();
        }
    }
}
