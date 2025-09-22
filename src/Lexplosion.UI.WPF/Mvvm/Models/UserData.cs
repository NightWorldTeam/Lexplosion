namespace Lexplosion.UI.WPF.Mvvm.Models
{
    public sealed class UserData : VMBase
    {
        public static UserData Instance { get; } = new UserData();


        private bool _isAuthrized;
        public bool IsAuthrized
        {
            get => _isAuthrized; set
            {
                _isAuthrized = value;
                OnPropertyChanged();
            }
        }

        private AccountType _currentAccountType;
        public AccountType CurrentAccountType
        {
            get => _currentAccountType; set
            {
                _currentAccountType = value;
                OnPropertyChanged();
            }
        }


        private string _nickname;
        public string Nickname
        {
            get => _nickname; set
            {
                _nickname = value;
                OnPropertyChanged();
            }
        }



        #region Constructors


        private UserData()
        {

        }


        #endregion Constructors
    }
}
