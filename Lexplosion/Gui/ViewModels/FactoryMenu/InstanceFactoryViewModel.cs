using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
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
         
        public InstanceFactoryViewModel()
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => new MainMenuViewModel());

            Tabs = new ObservableCollection<Tab>
            {
                new Tab
                {
                    Header = "Создание Сборки",
                    Content = new FactoryGeneralViewModel()
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
