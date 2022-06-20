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

        public InstanceMenuViewModel(InstanceClient instanceClient)
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
                    Header = "Параметры",
                    Content = new InstanceSettingsViewModel(instanceClient)
                },
                new Tab
                {
                    Header = "О Сборке",
                    Content = new InstanceCreationViewModel()
                },
                new Tab
                {
                    Header = "Дополнения",
                    Content = null
                },
                new Tab 
                {
                    Header = "Журнал изменений",
                    Content = null
                },
            };

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Обзор",
                    Content =  new TabMenuViewModel(_showCaseTabMenu, instanceClient.Name)
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
