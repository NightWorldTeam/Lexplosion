using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.Authorization;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels
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
                _nickname= value;
                OnPropertyChanged();
            }
        }



        #region Constructors


        private UserData()
        {
            
        }


        #endregion Constructors
    }

    public sealed class MainViewModel : VMBase
    {


        internal INavigationStore<VMBase> NavigationStore { get; } = new NavigationStore();
        public VMBase CurrentViewModel => NavigationStore.Content;


        public MainViewModel()
        {
            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;
            NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }

        private void NavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public static void ChangeColor(Color color)
        {
        }
    }
}
