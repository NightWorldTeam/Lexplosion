using System;

namespace Lexplosion.Gui.ViewModels
{
    public class PaginatorViewModel : VMBase
    {
        public (int min, int max) PageLimit = (0, 1638);

        private RelayCommand _nextPageCommand;
        private RelayCommand _prevPageCommand;
        private RelayCommand _textBoxPageIndexChanged;

        private bool _canGoBack = false;
        private bool _canGoNext = true;

        private ushort _pageIndex = 0;
        private ushort _pageNum = 1;

        private InstanceSource _source;

        private bool _isLoaded;

        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        private Action _pageIndexChangedAction;

        public PaginatorViewModel(Action action)
        {
            _pageIndexChangedAction = action;
        }

        public RelayCommand NextPageCommand
        {
            get => _nextPageCommand ?? (new RelayCommand(obj =>
            {
                if (PageIndex < PageLimit.max)
                {
                    if (!CanGoNext) CanGoNext = true;
                    if (!CanGoBack) CanGoBack = true;
                    PageIndex++;
                }
                if (PageIndex == PageLimit.max) CanGoNext = false;
            }));
        }

        public RelayCommand PrevPageCommand
        {
            get => _prevPageCommand ?? (new RelayCommand(obj =>
            {
                if (PageIndex - 1 >= 0)
                {
                    if (!CanGoNext) CanGoNext = true;
                    if (!CanGoBack) CanGoBack = true;
                    PageIndex--;
                }
                else if (PageIndex == 0) CanGoBack = false;
            }));
        }

        public RelayCommand TextBoxPageIndexChanged
        {
            get => _textBoxPageIndexChanged ?? (new RelayCommand(obj =>
            {
                if (PageNum < 1)
                {
                    PageNum = 1;
                }
                if (PageNum >= 1)
                {
                    PageIndex = PageNum--;
                }
                else PageIndex = 0;
            }));
        }

        public ushort PageIndex
        {
            get => _pageIndex; set
            {
                _pageIndex = value;
                PageNum = value++;
                OnPropertyChanged(nameof(PageIndex));
                _pageIndexChangedAction();
            }
        }

        public bool CanGoBack
        {
            get => _canGoBack; set
            {
                _canGoBack = value;
                OnPropertyChanged(nameof(CanGoBack));
            }
        }
        public bool CanGoNext
        {
            get => _canGoNext; set
            {
                _canGoNext = value;
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        public ushort PageNum
        {
            get => _pageNum; set
            {
                if (value <= 0)
                    _pageNum = 1;
                else
                    _pageNum = value;
                OnPropertyChanged(nameof(PageNum));
            }
        }

        public InstanceSource Source
        {
            get => _source; set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }
    }
}
