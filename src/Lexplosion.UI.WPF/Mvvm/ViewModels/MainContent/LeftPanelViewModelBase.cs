using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent
{
    public abstract class LeftPanelViewModelBase : ViewModelBase
    {
        public AutoResetEvent WaitHandler = new AutoResetEvent(false);


        public event Action<ViewModelBase> SelectedItemChanged;


        #region Properties


        private ObservableCollection<LeftPanelMenuItem> _items = new ObservableCollection<LeftPanelMenuItem>();
        public IEnumerable<LeftPanelMenuItem> Items { get => _items; }


        private LeftPanelMenuItem _selectedItem;
        public LeftPanelMenuItem SelectedItem
        {
            get => _selectedItem; set
            {
                if (_selectedItem != null && _selectedItem != value)
                {
                    _selectedItem.IsSelected = false;
                }
                _selectedItem = value;
                SelectedItemChanged?.Invoke(value.Content);
                OnPropertyChanged();
            }
        }





        #endregion Properties


        #region Constructors


        public LeftPanelViewModelBase()
        {
        }


        #endregion Constructors


        #region Public Methods


        public virtual void AddTabItem(string name, string icon, ViewModelBase content, int id = -1, double iconWidth = 20, double iconHeight = 20)
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

        public virtual void AddTabItem(LeftPanelMenuItem tabItem)
        {
            tabItem.SelectedEvent += OnSelectedTabItemChanged;
            _items.Add(tabItem);
        }

        public virtual void Clear()
        {
            _items.Clear();
        }

        public virtual void AddTabItems(IEnumerable<LeftPanelMenuItem> em)
        {
            foreach (var tabItem in em)
            {
                AddTabItem(tabItem);
            }
        }

        /// <summary>
        /// Выбирает элемент по индексу в коллекции.
        /// Индексация как у обычной коллекции с нуля.
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        public virtual LeftPanelMenuItem SelectItem(int index)
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;
            _items[index].IsSelected = true;
            return _items[index];
        }

        public virtual void SelectFirst()
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;

            _items[0].IsSelected = true;
        }

        public virtual void SelectLast()
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;

            _items[_items.Count - 1].IsSelected = true;
        }

        public virtual LeftPanelMenuItem GetByContentType(Type type)
        {
            return _items.FirstOrDefault(t => t.Content.GetType() == type);
        }


        #endregion Public Methods


        #region Private Methods


        protected virtual void OnSelectedTabItemChanged(LeftPanelMenuItem instance)
        {
            SelectedItem = instance;
        }


        #endregion Private Methods
    }
}
