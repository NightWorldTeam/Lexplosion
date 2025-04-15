using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Core.Objects
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


        #region Constructors


        public Accounts(AccountType type, IList<Account> list)
        {
            Type = type;
            IsNightWorldAccount = type == AccountType.NightWorld;
            _list = new ObservableCollection<Account>(list);
            List.Source = _list;
            IconSource = $"pack://application:,,,/assets/images/icons/{Type.ToString().ToLower()}.png";

            Account.AccountAdded += (account) =>
            {
                _list.Add(account);
            };
        }


        #endregion Constructors


        #region Public Methods


        public void AddAccount(Account account)
        {
            if (Account.ActiveAccount == null && account.AccountType == AccountType.NightWorld)
            {
                account.IsActive = true;
                if (Account.LaunchAccount == null)
                {
                    account.IsLaunch = true;
                }

                Account.SaveAll();
            }

            _list.Add(account);
            OnPropertyChanged(nameof(HasAccounts));
        }

        // TODO: Если аккаунт остался один выводит модальное окно с подтверждение действия,
        // И запуск окна авторизации в случае согласия.
        public void RemoveAccount(Account account)
        {
            _list.Remove(account);
            account.RemoveFromList();

            if (IsNightWorldAccount && _list.Count == 1)
            {
                _list[0].IsActive = true;

                if (Account.ListCount == 1 || Account.LaunchAccount == null)
                {
                    _list[0].IsLaunch = true;
                }
                Account.SaveAll();
            }
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

}
