using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class ShowCaseViewModel : SubmenuViewModel, ISubmenu
    {
        public event ISubmenu.NavigationToMenuCallBack NavigationToMainMenu;

        public ICommand NavigationMainMenuCommand 
        { 
            get => 
                new NavigateCommand<MainMenuViewModel>(
                    MainViewModel.NavigationStore, () => 
                    { 
                        NavigationToMainMenu?.Invoke();
                        return MainViewModel.MainMenuVM; 
                    }
                ); 
        }

        private int _tabControlSelectedValue;
        private List<Tab> _showCaseTabMenu;

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

        public ShowCaseViewModel(InstanceClient instanceClient)
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
                    Header = "Mods",
                    Content = new DevСurtainViewModel()
                },
                new Tab()
                {
                    Header = "Changelog",
                    Content = new DevСurtainViewModel() //new ChangelogViewModel()
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
