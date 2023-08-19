using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories.Modrinth;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.ViewModels.Modal;
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
        internal INavigationStore NavigationStore { get; } = new NavigationStore();
        
        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;
        public IModalViewModel CurrentModalViewModel => ModalNavigationStore.Instance.CurrentViewModel;

        public bool IsModalOpen { get => ModalNavigationStore.Instance.CurrentViewModel != null; }

        public MainViewModel()
        {
            var s = new ModrinthModsViewModel();
            ModalNavigationStore.Instance.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            ModalNavigationStore.Instance.Open(new LeftMenuControl(
                new ModalLeftMenuTabItem[3]
                {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = true
                    }
                }
                ));

            ModalNavigationStore.Instance.Close();

            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;
            NavigationStore.CurrentViewModel = new MainMenuLayoutViewModel();
            //NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }

        private void Instance_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsModalOpen));
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
