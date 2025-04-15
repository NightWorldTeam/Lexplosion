using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class LeftMenuControl : ModalViewModelBase
    {
        private readonly Dictionary<ViewModelBase, bool> _loadingPages = new();

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
            get => _isProcessActive; private set
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
            AddTabItems(tabItems, true);
        }


        #endregion Constructors


        #region Public Methods


        public void AddTabItems(IEnumerable<ModalLeftMenuTabItem> tabItems, bool isSelectFirst = false)
        {
            foreach (var item in tabItems)
            {
                item.SelectedEvent += OnCurrentContentChanged;
                _tabItems.Add(item);
                _loadingPages[item.Content] = false;
            }

            if (_tabItems.Count > 0 && isSelectFirst)
            {
                var firstItem = _tabItems[0];
                firstItem.IsSelected = true;
                _tabItems[0] = firstItem;
            }
        }


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

            _loadingPages[_tabItems.Last().Content] = false;
        }



        public void PageLoadingStatusChange(bool isLoading)
        {
            _loadingPages[SelectedContent] = isLoading;
            IsProcessActive = isLoading;
        }

        public void NavigateTo(int index)
        {
            OnCurrentContentChanged(_tabItems[index], true);
        }

        #endregion Public Methods


        #region Private Methods


        private void OnCurrentContentChanged(ModalLeftMenuTabItem tabItem, bool state)
        {
            if (state)
            {
                SelectedContent = tabItem.Content;
                TitleKey = tabItem.TitleKey;

                if (_loadingPages.TryGetValue(SelectedContent, out var isLoading))
                {
                    IsProcessActive = isLoading;
                }

                OnPropertyChanged(nameof(SelectedContent));
                OnPropertyChanged(nameof(TitleKey));
            }
        }


        #endregion Private Methods
    }
}
