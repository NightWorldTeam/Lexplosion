using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class ShowCaseViewModel : SubmenuViewModel
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

        private List<Tab> _showCaseTabMenu;

        public ShowCaseViewModel(InstanceClient instanceClient)
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

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Обзор",
                    Content =  new TabMenuViewModel(_showCaseTabMenu, instanceClient.Name)
                },
                new Tab
                {
                    Header = "Назад",
                    Content = null
                },
            };
            SelectedTab = Tabs[0];
            TabControlSelectedIndex = 0;
        }
    }
}
