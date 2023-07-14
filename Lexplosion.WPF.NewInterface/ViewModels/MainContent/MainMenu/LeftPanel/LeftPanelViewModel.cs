using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class LeftPanelMenuItem : VMBase, IComparable<LeftPanelMenuItem>
    {
        public event Action<LeftPanelMenuItem> SelectedEvent;

        public uint Id { get; set; }
        public string TextKey { get; set; }
        public string Icon { get; set; }

        public double IconWidth { get; set; }
        public double IconHeight { get; set; }

        public VMBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected; set 
            {
                _isSelected = value;

                if (_isSelected) 
                { 
                    SelectedEvent?.Invoke(this);
                }
                OnPropertyChanged();
            }
        }

        public int CompareTo(LeftPanelMenuItem other)
        {
            return Id.CompareTo(other.Id);
        }
    }

    public sealed class LeftPanelViewModel : VMBase
    {
        public event Action<VMBase> SelectedItemChanged;

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


        #region Constructors


        public LeftPanelViewModel() 
        {
            
        }


        #endregion Constructors


        #region Public Methods


        public void AddTabItem(string name, string icon, VMBase content, int id = -1, double iconWidth = 20, double iconHeight = 20)
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

        public void OnSelectedTabItemChanged(LeftPanelMenuItem instance) 
        {
            SelectedItem = instance;
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
    }
}
