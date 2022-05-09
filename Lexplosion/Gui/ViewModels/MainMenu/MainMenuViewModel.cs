using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class MainMenuViewModel : SubmenuViewModel
    {
        public ICommand NavigationFactoryCommand { get; }
        public ICommand NavigationInstanceCommand { get; private set; }
        public ICommand NavigationShowCaseCommand { get; private set; }

        private RelayCommand _logoClickCommand;

        public RelayCommand LogoClickCommand
        {
            get => _logoClickCommand ?? (new RelayCommand(obj =>
            {
                var values = (object[])obj;

                var result = (bool)values[0];
                var outsideId = (string)values[1];
                var localId = (string)values[2];
                var source = (InstanceSource)values[3];
                var name = (string)values[4];
                var isInstalled = (bool)values[5];
                if (isInstalled) {
                    NavigationShowCaseCommand = new NavigateCommand<InstanceMenuViewModel>(
                        MainViewModel.NavigationStore, () => new InstanceMenuViewModel(localId, outsideId, name, source));
                } else 
                {
                    NavigationShowCaseCommand = new NavigateCommand<ShowCaseViewModel>(
                        MainViewModel.NavigationStore, () => new ShowCaseViewModel(localId, outsideId, name, source));
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

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Каталог",
                    Content = new CatalogViewModel()
                },
                new Tab
                {
                    Header = "Библиотека",
                    Content = new LibraryViewModel()
                },
                new Tab
                {
                    Header = "Сетевая игра",
                    Content = null
                },
                new Tab
                {
                    Header = "Настройки",
                    Content = new TabMenuViewModel(GeneralSettingsTabs, "Настройки")
                }
            };
            SelectedTab = Tabs[0];
        }
    }
}
