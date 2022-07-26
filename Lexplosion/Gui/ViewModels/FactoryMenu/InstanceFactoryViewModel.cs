using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class InstanceFactoryViewModel : SubmenuViewModel
    {
        public ICommand NavigationMainMenuCommand { get; }

        private int _tabControlSelectedValue;

        public int TabControlSelectedIndex 
        {
            get => _tabControlSelectedValue;
            set 
            {
                _tabControlSelectedValue = value;
                OnPropertyChanged(nameof(TabControlSelectedIndex));
                if (value == Tabs.Count - 1)
                    NavigationMainMenuCommand.Execute(null);
            }
        }

        private FactoryGeneralViewModel _factoryGeneralVM;

        public InstanceFactoryViewModel()
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);

            _factoryGeneralVM = new FactoryGeneralViewModel();

            var FactoryTabs = new List<Tab>()
            {
                new Tab
                {
                    Header = "Основное",
                    Content = _factoryGeneralVM
                }
            }; 

            var _tabMenuViewModel = new TabMenuViewModel(FactoryTabs, "Создание сборки");

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Создание Сборки",
                    Content = _tabMenuViewModel
                },
                new Tab
                {
                    Header = "Назад",
                    Content = null
                }
            };
            SelectedTab = Tabs[0];
            TabControlSelectedIndex = 0;
        }
    }
}
