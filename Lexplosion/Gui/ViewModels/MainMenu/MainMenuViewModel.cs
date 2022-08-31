using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu.Multiplayer;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class MainMenuViewModel : SubmenuViewModel
    {
        private readonly MainViewModel _mainViewModel;

        /* multiplayer fields */
        private readonly List<Tab> _multiplayerTabs;
        private GeneralMultiplayerViewModel _generalMultiplayerViewModel = new GeneralMultiplayerViewModel();
        private FriendsTabViewModel _friendsTabViewModel = new FriendsTabViewModel();
        private ChannelTabViewModel _channelTabViewModel = new ChannelTabViewModel();
        /* multiplayer fields */

        /* settings fields*/
        private readonly List<Tab> GeneralSettingsTabs;
        /* settings fields*/

        /* mainmenu fields */
        private readonly CatalogViewModel _catalogVM;
        private readonly LibraryViewModel _libraryVM;
        private readonly TabMenuViewModel _multiplayerTabMenu;
        private readonly TabMenuViewModel _settingsTabMenu;
        /* mainmenu fields */

        #region commands

        public ICommand NavigationFactoryCommand { get; }
        public ICommand NavigationInstanceCommand { get; private set; }
        public ICommand NavigationShowCaseCommand { get; private set; }

        private RelayCommand _logoClickCommand;

        public RelayCommand LogoClickCommand
        {
            get => _logoClickCommand ?? (new RelayCommand(obj =>
            {
                var instanceFormViewModel = (InstanceFormViewModel)obj;

                Console.WriteLine(instanceFormViewModel.Client.Name + " " + instanceFormViewModel.Client.InLibrary);

                    NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                        MainViewModel.NavigationStore, () => new InstanceMenuViewModel(instanceFormViewModel, _mainViewModel));
                NavigationShowCaseCommand?.Execute(null);
            }));
        }


        #endregion commands


        public MainMenuViewModel(MainViewModel mainViewModel)
        {

            _mainViewModel = mainViewModel;

            _catalogVM = new CatalogViewModel(mainViewModel);
            _libraryVM = new LibraryViewModel(mainViewModel);


            #region multiplayer init


            _multiplayerTabs = new List<Tab>()
            {
                //new Tab 
                //{
                //    Header = "Cервера партнёров",
                //    Content = null
                //},
                new Tab
                {
                    Header = ResourceGetter.GetString("general"),
                    Content = _generalMultiplayerViewModel
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("friends"),
                    Content = _friendsTabViewModel
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("channels"),
                    Content = _channelTabViewModel
                }
            };

            _multiplayerTabMenu = new TabMenuViewModel(_multiplayerTabs, "Сетевая игра");


            #endregion multiplayer init


            #region settings init

            GeneralSettingsTabs = new List<Tab>()
            {
                new Tab
                {
                    Header = ResourceGetter.GetString("general"),
                    Content = new GeneralSettingsViewModel()
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("account"),
                    Content = new DevСurtainViewModel()
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("about"),
                    Content = new DevСurtainViewModel()
                }
            };
            _settingsTabMenu = new TabMenuViewModel(GeneralSettingsTabs, "Настройки");


            #endregion multiplayer init


            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = ResourceGetter.GetString("catalog"),
                    Content = _catalogVM
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("library"),
                    Content = _libraryVM
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("multiplayer"),
                    Content = _multiplayerTabMenu
                },
                new Tab
                {
                    Header = ResourceGetter.GetString("settings"),
                    Content = _settingsTabMenu
                },
            };
            SelectedTab = Tabs[0];
        }
    }
}
