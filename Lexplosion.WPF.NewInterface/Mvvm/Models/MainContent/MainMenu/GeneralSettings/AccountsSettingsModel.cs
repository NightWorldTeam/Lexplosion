using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.GeneralSettings
{
    public sealed class AccountsSettingsModel : ViewModelBase
    {
        #region Properties


        public IList<Accounts> AccountsByType { get; } = new ObservableCollection<Accounts>();

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


        public AccountsSettingsModel()
        {
            Accounts.Source = _accounts;

            foreach (var account in Account.List)
            {
                _accounts.Add(new(account));
            }
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
            });
        }
    }
}
