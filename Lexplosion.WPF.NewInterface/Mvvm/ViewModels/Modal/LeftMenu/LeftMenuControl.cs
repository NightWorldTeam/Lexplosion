using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class LeftMenuControl : ModalViewModelBase
    {
        private readonly ObservableCollection<ModalLeftMenuTabItem> _tabItems = new ObservableCollection<ModalLeftMenuTabItem>();
        public IEnumerable<ModalLeftMenuTabItem> TabItems { get => _tabItems; }


        public ViewModelBase SelectedContent { get; private set; }
        public string TitleKey { get; private set; }


        private string _loaderPlaceholderKey;
        public string LoaderPlaceholderKey 
        {
            get => _loaderPlaceholderKey; set 
            {
                _loaderPlaceholderKey = value;
                OnPropertyChanged();
            }
        }

        private bool _isProcessActive;
        public bool IsProcessActive
        {
            get => _isProcessActive; set
            {
                _isProcessActive = value;
                OnPropertyChanged();
            }
        }


        #region Constructors


        public LeftMenuControl()
        {

        }

        public LeftMenuControl(IEnumerable<ModalLeftMenuTabItem> tabItems)
        {
            foreach (var item in tabItems)
            {
                item.SelectedEvent += OnCurrentContentChanged;
                _tabItems.Add(item);
            }
            if (_tabItems.Count > 0) 
            {
                OnCurrentContentChanged(_tabItems[0]);
            }
        }


        #endregion Constructors


        #region Public Methods


        public void AddTabItem(string titleKey, string iconKey, ViewModelBase content, bool isEnable = true)
        {
            _tabItems.Add(new ModalLeftMenuTabItem()
            {
                Id = _tabItems.Count,
                TitleKey = titleKey,
                IconKey = iconKey,
                Content = content,
                IsEnable = isEnable,
            });
        }


        #endregion Public Methods


        #region Private Methods


        private void OnCurrentContentChanged(ModalLeftMenuTabItem tabItem)
        {
            SelectedContent = tabItem.Content;
            TitleKey = tabItem.TitleKey;
            OnPropertyChanged(nameof(SelectedContent));
            OnPropertyChanged(nameof(TitleKey));
        }


        #endregion Private Methods
    }
}
