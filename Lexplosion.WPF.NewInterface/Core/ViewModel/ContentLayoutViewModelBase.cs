using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Core
{
    public abstract class ContentLayoutViewModelBase : ViewModelBase
    {
        protected ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }

        private string _headerKey;
        public string HeaderKey
        {
            get => _headerKey;
            set => RaiseAndSetIfChanged(ref _headerKey, value);
        }

        public TabItemModel? SelectedItem { get; set; }

        public ContentLayoutViewModelBase()
        {
            _tabs.CollectionChanged += _tabs_CollectionChanged; 
        }

        private void _tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tabItem = sender as Collection<TabItemModel>;
            
            if (tabItem[0] == null)
                return;

            switch (e.Action) 
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        tabItem[0].SelectedChanged += OnSelectedItemChanged;
                        if (tabItem[0].IsSelected)
                            OnSelectedItemChanged(tabItem[0]);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        tabItem[0].SelectedChanged -= OnSelectedItemChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace: 
                    {
                        (e.OldItems[0] as TabItemModel).SelectedChanged -= OnSelectedItemChanged;
                        var newItem = (e.NewItems[0] as TabItemModel);
                        newItem.SelectedChanged += OnSelectedItemChanged;
                        if (newItem.IsSelected)
                            OnSelectedItemChanged(newItem);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    { }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        foreach (var oldItem in e.OldItems) 
                        {
                            (oldItem as TabItemModel).SelectedChanged -= OnSelectedItemChanged;
                        }
                    }
                    break;
            }
        }

        protected void OnSelectedItemChanged(TabItemModel tabItemModel) 
        {
            SelectedItem = tabItemModel;
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
