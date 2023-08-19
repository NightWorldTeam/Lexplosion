using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class LeftPanelViewModel : ViewModelBase
    {
        public event Action<ViewModelBase> SelectedItemChanged;


        #region Properties


        private ObservableCollection<LeftPanelMenuItem> _items = new ObservableCollection<LeftPanelMenuItem>();
        public IEnumerable<LeftPanelMenuItem> Items { get => _items; }


        private LeftPanelMenuItem _selectedItem;
        public LeftPanelMenuItem SelectedItem 
        {
            get => _selectedItem; set 
            {
                _selectedItem = value;
                SelectedItemChanged?.Invoke(value.Content);
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public LeftPanelViewModel() 
        {
            
        }


        #endregion Constructors


        #region Public Methods


        public void AddTabItem(string name, string icon, ViewModelBase content, int id = -1, double iconWidth = 20, double iconHeight = 20)
        {
            if (id == -1 || id < 0) 
            {
                id = _items.Count + 1;
            }

            var newTabItem = new LeftPanelMenuItem
            {
                Id = (uint)id,
                TextKey = name,
                Icon = icon,
                Content = content,
                IconWidth = iconWidth,
                IconHeight = iconHeight
            };

            newTabItem.SelectedEvent += OnSelectedTabItemChanged;

            _items.Add(newTabItem);
        }

        public void AddTabItem(LeftPanelMenuItem tabItem) 
        {
            tabItem.SelectedEvent += OnSelectedTabItemChanged;
            _items.Add(tabItem);
        }

        public void SelectFirst() 
        {
            _items[0].IsSelected = true;
        }

        public void SelectLast() 
        {
            _items[_items.Count - 1].IsSelected = false;
        }


        #endregion Public Methods


        #region Private Methods


        private void OnSelectedTabItemChanged(LeftPanelMenuItem instance)
        {
            SelectedItem = instance;
        }


        #endregion Private Methods
    }
}
