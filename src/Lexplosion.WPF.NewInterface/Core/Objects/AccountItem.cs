using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class AccountSource 
    {
        public AccountType Type { get; }
        public string IconSource { get; }

        public AccountSource(AccountType type)
        {
            Type = type;
            IconSource = $"pack://application:,,,/assets/images/icons/{type.ToString().ToLower()}.png"; ;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is AccountSource))
                return false;

            return (obj as AccountSource).Type == Type;
        }
    }

    public class AccountItem : ObservableObject
    {
        public Account Account { get; set; }
        public AccountSource Source { get; set;}
        public bool IsNightWorldAccount { get; }

        public AccountItem(Account account)
        {
            Account = account;
            Source = new AccountSource(account.AccountType);
            IsNightWorldAccount = Source.Type == AccountType.NightWorld;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is AccountItem))
                return false;

            return (obj as AccountItem).Account == Account;
        }
    }
}
