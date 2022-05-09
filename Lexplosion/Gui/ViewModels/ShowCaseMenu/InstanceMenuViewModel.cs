using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
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
                    NavigationMainMenuCommand.Execute(null);
            }
        }

        #endregion

        public InstanceMenuViewModel(string localId, string outsideId, string name, InstanceSource source)
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => new MainMenuViewModel());

            _showCaseTabMenu = new List<Tab>()
            {
                new Tab()
                {
                    Header = "Overview",
                    Content = new OverviewViewModel(outsideId, localId, source)
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

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Обзор",
                    Content =  new TabMenuViewModel(_showCaseTabMenu, name)
                },
                new Tab 
                {
                    Header = "Настройки",
                    Content = new InstanceSettingsViewModel(localId),
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
