using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public class TabItemModel : VMBase
    {
        public string TextKey { get; set; }
        public VMBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected 
        {
            get => _isSelected; set 
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public class GeneralSettingsLayoutViewModel : VMBase
    {
        private ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }

        public GeneralSettingsLayoutViewModel()
        {
            _tabs.Add(new TabItemModel { TextKey = "general", Content = new GeneralSettingsViewModel(), IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "appearance", Content = null });
            _tabs.Add(new TabItemModel { TextKey = "language", Content = null });
            _tabs.Add(new TabItemModel { TextKey = "about", Content = null });
        }
    }
}
