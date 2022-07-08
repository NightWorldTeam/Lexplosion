using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.MainMenu.Multiplayer;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class MainMenuViewModel : SubmenuViewModel
    {
        private readonly List<Tab> GeneralSettingsTabs = new List<Tab>()
        {
            new Tab
            {
                Header = "Основное",
                Content = new GeneralSettingsViewModel()
            },
            new Tab
            {
                Header = "Учетная запись",
                Content = null
            },
            new Tab
            {
                Header = "О лаунчере",
                Content = null
            }
        };

        private readonly List<Tab> MultiplayerTabs = new List<Tab>()
        {
            new Tab
            {
                Header = "Общее",
                Content = new GeneralMultiplayerViewModel()
            },
            new Tab
            {
                Header = "Друзья",
                Content = null
            },
            new Tab
            {
                Header = "Комнаты",
                Content = null
            }
        };

        private readonly MainViewModel _mainViewModel;

        private readonly CatalogViewModel _catalogVM;
        private readonly LibraryViewModel _libraryVM = new LibraryViewModel();
        private readonly TabMenuViewModel _tabMenuViewModel;
        private readonly TabMenuViewModel _tabMenuViewModel1;

        #region Commands

        public ICommand NavigationFactoryCommand { get; }
        public ICommand NavigationInstanceCommand { get; private set; }
        public ICommand NavigationShowCaseCommand { get; private set; }

        private RelayCommand _logoClickCommand;

        public RelayCommand LogoClickCommand
        {
            get => _logoClickCommand ?? (new RelayCommand(obj =>
            {
                var instanceClient = (InstanceClient)obj;

                if (instanceClient.InLibrary)
                {
                    NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                        MainViewModel.NavigationStore, () => new InstanceMenuViewModel(instanceClient, _mainViewModel));
                }
                else
                {
                    NavigationShowCaseCommand = new NavigateCommand<ShowCaseViewModel>(
                        MainViewModel.NavigationStore, () => new ShowCaseViewModel(instanceClient));
                }
                NavigationShowCaseCommand?.Execute(null);
            }));
        }

        #endregion CommandsEnd

        public MainMenuViewModel(MainViewModel mainViewModel)
        {
            NavigationFactoryCommand = new NavigateCommand<InstanceFactoryViewModel>(
                 MainViewModel.NavigationStore, () => new InstanceFactoryViewModel());

            _tabMenuViewModel = new TabMenuViewModel(MultiplayerTabs, "Сетевая игра");
            _tabMenuViewModel1 = new TabMenuViewModel(GeneralSettingsTabs, "Настройки");

            _mainViewModel = mainViewModel;

            _catalogVM = new CatalogViewModel(mainViewModel);

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Каталог",
                    Content = _catalogVM
                },
                new Tab
                {
                    Header = "Библиотека",
                    Content = _libraryVM
                },
                new Tab
                {
                    Header = "Сетевая игра",
                    Content = _tabMenuViewModel
                },
                new Tab
                {
                    Header = "Настройки",
                    Content = _tabMenuViewModel1
                }
            };
            SelectedTab = Tabs[0];
        }
    }
}
