using System.Collections.ObjectModel;

namespace Lexplosion.Gui
{
    public abstract class SubmenuViewModel : VMBase
    {
        public ObservableCollection<Tab<VMBase>> Tabs { get; protected set; } = new ObservableCollection<Tab<VMBase>>();
        private Tab<VMBase> _selectedTab = new Tab<VMBase>();

        public Tab<VMBase> SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public void SetSelectedTabIndex(int index)
        {
            SelectedTab = Tabs[index];
        }
    }
}
