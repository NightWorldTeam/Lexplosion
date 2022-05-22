using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceMenuViewModel : SubmenuViewModel
    {
        private int _tabControlSelectedValue;
        private List<Tab> _showCaseTabMenu;

        #region commands
        public ICommand NavigationMainMenuCommand { get; }
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
                    GC.Collect();
                    NavigationMainMenuCommand.Execute(null);
                }
            }
        }

        #endregion

        public InstanceMenuViewModel(InstanceClient instanceClient)
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);

            _showCaseTabMenu = new List<Tab>()
            {
                new Tab()
                {
                    Header = "Overview",
                    Content = new OverviewViewModel(instanceClient)
                },
                new Tab()
                {
                    Header = "Mods",
                    Content = null
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
                    Header = "Игровые настройки",
                    Content = new InstanceSettingsViewModel(instanceClient)
                },
                new Tab
                {
                    Header = "О Сборке",
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
                    Header = "Настройки",
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
