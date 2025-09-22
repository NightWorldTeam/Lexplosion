using Lexplosion.UI.WPF.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.UI.WPF.Core
{
    public abstract class ContentLayoutViewModelBase : ViewModelBase
    {
        #region Properties


        protected ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }

        protected int Count { get => _tabs.Count; }

        private string _headerKey;
        public string HeaderKey
        {
            get => _headerKey;
            set => RaiseAndSetIfChanged(ref _headerKey, value);
        }

        private TabItemModel _selectedItem;
        public TabItemModel SelectedItem
        {
            get => _selectedItem; set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Public & Protected Methods


        protected void AddTabItem(TabItemModel tabItemModel)
        {
            _tabs.Add(tabItemModel);
            tabItemModel.SelectedChanged += (value, state) =>
            {
                if (state)
                {
                    SelectedItem = value;
                }
            };
        }

        public TabItemModel GetByTypeOfContent(Type type) 
        {
            return _tabs.FirstOrDefault(t => t.Content.GetType() == type);
        }

        public ContentLayoutViewModelBase()
        {
            _tabs.CollectionChanged += _tabs_CollectionChanged; 
        }

        private void _tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tabItem = sender as Collection<TabItemModel>;
            
            if (tabItem.Count == 0 || tabItem[0] == null)
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

        protected void OnSelectedItemChanged(TabItemModel tabItemModel, bool state = true) 
        {
            SelectedItem = tabItemModel;
            OnPropertyChanged(nameof(SelectedItem));
        }


        #endregion 
    }
}
