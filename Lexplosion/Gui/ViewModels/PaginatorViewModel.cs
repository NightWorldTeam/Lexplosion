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

        private int _pageIndex = 0;
        private int _pageNum = 1;

        private InstanceSource _source;

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
                if (PageIndex > 0) 
                {
                    if (!CanGoNext) CanGoNext = true;
                    if (!CanGoBack) CanGoBack = true;
                    PageIndex--;
                }
                if (PageIndex == 0) CanGoBack = false;
            }));
        }

        public RelayCommand TextBoxPageIndexChanged 
        {
            get => _textBoxPageIndexChanged ?? (new RelayCommand(obj => 
            {
                if (PageNum == 0) 
                {
                    PageNum = 1;
                }
                if (PageNum >= 1)
                {
                    PageIndex = PageNum - 1;
                }
                else PageIndex = 0;
            }));
        }

        public int PageIndex
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

        public int PageNum 
        {
            get => _pageNum; set 
            {
                _pageNum = value;
                OnPropertyChanged(nameof(PageNum));
            }
        }

        public InstanceSource Source 
        {
            get => _source; set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }
    }
}
