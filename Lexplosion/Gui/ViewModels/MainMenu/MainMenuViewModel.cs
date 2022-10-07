using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu.Multiplayer;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class MainMenuViewModel : SubmenuViewModel
    {
        private readonly MainViewModel _mainViewModel;

        /* multiplayer fields */
        private readonly List<Tab<VMBase>> _multiplayerTabs;
        private GeneralMultiplayerViewModel _generalMultiplayerViewModel = new GeneralMultiplayerViewModel();
        private FriendsTabViewModel _friendsTabViewModel = new FriendsTabViewModel();
        private ChannelTabViewModel _channelTabViewModel = new ChannelTabViewModel();
        /* multiplayer fields */

        /* settings fields*/
        private readonly List<Tab<VMBase>> GeneralSettingsTabs;
        /* settings fields*/

        /* mainmenu fields */
        private readonly CatalogViewModel _catalogVM;
        private readonly LibraryViewModel _libraryVM;
        private readonly TabMenuViewModel _multiplayerTabMenu;
        private readonly TabMenuViewModel _settingsTabMenu;
        /* mainmenu fields */


        #region Commands


        public ICommand NavigationFactoryCommand { get; }
        public ICommand NavigationInstanceCommand { get; private set; }
        public ICommand NavigationShowCaseCommand { get; private set; }

        private RelayCommand _logoClickCommand;
        /// <summary>
        /// Открывает страницу модпака при клина на Logo в InstanceForm
        /// </summary>
        public RelayCommand LogoClickCommand
        {
            get => _logoClickCommand ?? (_logoClickCommand = new RelayCommand(obj =>
            {
                OpenModpackPage((InstanceFormViewModel)obj);
            }));
        }


        #endregion Commands


        #region Constructors


        public MainMenuViewModel(MainViewModel mainViewModel)
        {

            _mainViewModel = mainViewModel;

            _catalogVM = new CatalogViewModel(mainViewModel);
            _libraryVM = new LibraryViewModel(mainViewModel);


            #region Initialize Multiplayer


            _multiplayerTabs = InitializeMultiplayerTabs();
            _multiplayerTabMenu = new TabMenuViewModel(_multiplayerTabs, ResourceGetter.GetString("multiplayer"));


            #endregion Initialize Multiplayer


            #region Initialize Settings 


            GeneralSettingsTabs = InitializeSettingsTabs();
            _settingsTabMenu = new TabMenuViewModel(GeneralSettingsTabs, ResourceGetter.GetString("settings"));


            #endregion Initialize Settings


            #region Initialize MainMenu


            Tabs = InitializeMainMenuTabs();
            SelectedTab = Tabs[0];


            #endregion Initialize MainMenu
        }


        #endregion Constructors


        #region Public & Protected Methods


        /// <summary>
        /// Открывает главную страницу модпака.
        /// </summary>
        /// <param name="viewModel">InstanceViewModel нужной сборки.</param>
        public void OpenModpackPage(InstanceFormViewModel viewModel)
        {
            NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                MainViewModel.NavigationStore, () => new InstanceMenuViewModel(viewModel, _mainViewModel));
            NavigationShowCaseCommand?.Execute(null);
        }

        /// <summary>
        /// Открывает страницу модпака сборки (дополнения).
        /// </summary>
        /// <param name="viewModel">InstanceViewModel нужной сборки.</param>
        /// <param name="isToDLC">Перейти на страницу с дополнениями?</param>
        public void OpenModpackPage(InstanceFormViewModel viewModel, bool isToDLC = false)
        {
            var index = 0;
            var subIndex = 0;

            if (isToDLC)
            {
                index = 1;
                subIndex = viewModel.Client.Type == InstanceSource.Local ? 3 : 1;
            }

            NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                MainViewModel.NavigationStore, () => new InstanceMenuViewModel(viewModel, _mainViewModel, index, subIndex));
            NavigationShowCaseCommand?.Execute(null);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private List<Tab<VMBase>> InitializeMultiplayerTabs()
        {
            VMBase curtains = new DevСurtainViewModel() { Message = "Функции нашей сетевой игры доступны только при использовании аккаунта NightWorld" };

            return new List<Tab<VMBase>>()
            {
                //new Tab 
                //{
                //    Header = "Cервера партнёров",
                //    Content = null
                //},
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("general"),
                    Content =  UserData.User.AccountType == AccountType.NightWorld ? _generalMultiplayerViewModel : curtains
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("friends"),
                    Content = UserData.User.AccountType == AccountType.NightWorld ? new DevСurtainViewModel() : curtains
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("channels"),
                    Content = UserData.User.AccountType == AccountType.NightWorld ? new DevСurtainViewModel() : curtains
                }
            };
        }

        private List<Tab<VMBase>> InitializeSettingsTabs()
        {
            return new List<Tab<VMBase>>()
            {
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("general"),
                    Content = new GeneralSettingsViewModel(_mainViewModel)
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("account"),
                    Content = new DevСurtainViewModel()
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("about"),
                    Content = new DevСurtainViewModel()
                }
            };
        }

        private ObservableCollection<Tab<VMBase>> InitializeMainMenuTabs()
        {
            return new ObservableCollection<Tab<VMBase>>
            {
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("catalog"),
                    Content = _catalogVM
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("library"),
                    Content = _libraryVM
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("multiplayer"),
                    Content = _multiplayerTabMenu
                },
                new Tab<VMBase>
                {
                    Header = ResourceGetter.GetString("settings"),
                    Content = _settingsTabMenu
                },
            };
        }


        #endregion Private Methods
    }
}
