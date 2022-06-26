using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceMenuViewModel : SubmenuViewModel, ISubmenu
    {
        private int _tabControlSelectedValue;
        private List<Tab> _showCaseTabMenu;

        public event ISubmenu.NavigationToMenuCallBack NavigationToMainMenu;

        #region commands

        public ICommand NavigationMainMenuCommand
        {
            get => new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => { NavigationToMainMenu?.Invoke(); return MainViewModel.MainMenuVM; });
        }

        #endregion

        #region props

        public int TabControlSelectedIndex
        {
            get => _tabControlSelectedValue;
            set
            {
                _tabControlSelectedValue = value;
                OnPropertyChanged(nameof(TabControlSelectedIndex));
                if (value == Tabs.Count - 1)
                {
                    NavigationMainMenuCommand.Execute(null);
                }
            }
        }

        #endregion

        public InstanceMenuViewModel(InstanceClient instanceClient, MainViewModel mainViewModel = null )
        {
            _showCaseTabMenu = new List<Tab>()
            {
                new Tab()
                {
                    Header = "Overview",
                    Content = new OverviewViewModel(instanceClient, this)
                },
                new Tab()
                {
                    Header = "Changelog",
                    Content = null
                }
            };

            var _settingsTabs = new List<Tab>()
            {
                new Tab
                {
                    Id = 0,
                    Header = "Параметры",
                    Content = new InstanceSettingsViewModel(instanceClient)
                },
                new Tab
                {
                    Id = 3,
                    Header = "Дополнения",
                    Content = new FactoryDLCVM(mainViewModel, instanceClient)
                },
                new Tab 
                {
                    Id = 4,
                    Header = "Журнал изменений",
                    Content = null
                },
            };

            if (instanceClient.Type == InstanceSource.Local) 
            { 
                _settingsTabs.Add(new Tab { Id = 1, Header = "О Сборке", Content = new InstanceCreationViewModel(instanceClient) });
            }

            _settingsTabs.Sort();

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Обзор",
                    Content =  new TabMenuViewModel(_showCaseTabMenu, instanceClient.Name, instanceClient)
                },
                new Tab
                {
                    Header = "Конфигурация",
                    Content = new TabMenuViewModel(_settingsTabs, "Настройки сборки"),
                },
                new Tab
                {
                    Header = "Назад",
                    Content = null
                },
            };
            SelectedTab = Tabs[0];
        }
    }
}
