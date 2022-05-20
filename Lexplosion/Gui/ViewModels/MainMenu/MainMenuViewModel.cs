using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class MainMenuViewModel : SubmenuViewModel
    {
        private readonly CatalogViewModel _catalogVM;
        private readonly LibraryViewModel _libraryVM;
        //private readonly CatalogViewModel _catalogVM;
        private readonly TabMenuViewModel _tabMenuViewModel;

        public ICommand NavigationFactoryCommand { get; }
        public ICommand NavigationInstanceCommand { get; private set; }
        public ICommand NavigationShowCaseCommand { get; private set; }

        private RelayCommand _logoClickCommand;

        public RelayCommand LogoClickCommand
        {
            get => _logoClickCommand ?? (new RelayCommand(obj =>
            {
                var instanceClient = (InstanceClient)obj;

                if (!instanceClient.IsNonInstalled) {
                    NavigationShowCaseCommand = new NavigateCommand<ShowCaseViewModel>(
                        MainViewModel.NavigationStore, () => new ShowCaseViewModel(instanceClient));

                } else 
                {
                    NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                        MainViewModel.NavigationStore, () => new InstanceMenuViewModel(instanceClient));
                }
                NavigationShowCaseCommand?.Execute(null);
            }));
        }

        private List<Tab> GeneralSettingsTabs = new List<Tab>()
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

        public MainMenuViewModel()
        {
            NavigationFactoryCommand = new NavigateCommand<InstanceFactoryViewModel>(
                 MainViewModel.NavigationStore, () => new InstanceFactoryViewModel());

            _catalogVM = new CatalogViewModel();
            _libraryVM = new LibraryViewModel();
            //_catalogVM = new CatalogViewModel();
            _tabMenuViewModel = new TabMenuViewModel(GeneralSettingsTabs, "Настройки");

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
                    Content = null
                },
                new Tab
                {
                    Header = "Настройки",
                    Content = _tabMenuViewModel
                }
            };
            SelectedTab = Tabs[0];
        }
    }
}
