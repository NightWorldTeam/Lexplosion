using Lexplosion.WPF.NewInterface.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core
{
    public abstract class ContentLayoutViewModelBase : ViewModelBase
    {
        #region Properties


        protected ObservableCollection<TabItemModel> _tabs = new ObservableCollection<TabItemModel>();
        public IEnumerable<TabItemModel> Tabs { get => _tabs; }
        public int Count { get => _tabs.Count; }


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


        #region Constructors


        public ContentLayoutViewModelBase()
        {
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void AddTabItem(TabItemModel tabItemModel)
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


        #endregion Public & Protected Methods
    }
}
