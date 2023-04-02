using Lexplosion.Tools;
using System;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class PaginatorViewModel : VMBase
    {
        public SetValues<int, int> PageLimit = new SetValues<int, int>
        {
            Value1 = 0,
            Value2 = 1638
        }; //min = value1, max = value2


        private InstanceSource _source;
        public event Action<string, bool> PageChanged;


        #region Command


        private RelayCommand _nextPageCommand;
        public RelayCommand NextPageCommand
        {
            get => _nextPageCommand ?? (new RelayCommand(obj =>
            {
                if (PageIndex < PageLimit.Value2)
                {
                    if (!CanGoNext) CanGoNext = true;
                    if (!CanGoBack) CanGoBack = true;
                    PageIndex++;
                }
                else if (PageIndex == 1) CanGoBack = false;
            }));
        }

        private RelayCommand _prevPageCommand;
        public RelayCommand PrevPageCommand
        {
            get => _prevPageCommand ?? (new RelayCommand(obj =>
            {
                if (PageIndex > 1)
                {
                    if (!CanGoNext) CanGoNext = true;
                    if (!CanGoBack) CanGoBack = true;
                    PageIndex--;
                }
                else if (PageIndex == 1) CanGoBack = false;
            }));
        }

        private RelayCommand _textBoxPageIndexChanged;
        public RelayCommand TextBoxPageIndexChanged
        {
            get => _textBoxPageIndexChanged ?? (new RelayCommand(obj =>
            {
                PageIndex = PageNum--;
            }));
        }


        #endregion Commands


        #region Properties


        private ushort _pageIndex = 1;
        public ushort PageIndex
        {
            get => _pageIndex; set
            {
                _pageIndex = value;
                PageChanged?.Invoke(null, true);
                OnPropertyChanged();
            }
        }

        private bool _canGoBack = false;
        public bool CanGoBack
        {
            get => _canGoBack; set
            {
                _canGoBack = value;
                OnPropertyChanged();
            }
        }

        private bool _canGoNext = true;
        public bool CanGoNext
        {
            get => _canGoNext; set
            {
                _canGoNext = value;
                OnPropertyChanged();
            }
        }

        public ushort PageNum
        {
            get => PageIndex; set
            {
                PageIndex = value;
                OnPropertyChanged();
            }
        }

        public InstanceSource Source
        {
            get => _source; set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties
    }
}
