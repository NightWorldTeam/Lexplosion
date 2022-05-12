using System;

namespace Lexplosion.Gui.ViewModels
{
    public class SearchBoxViewModel : VMBase
    {
        private RelayCommand _searchCommand;

        private string _searchTextUncomfirmed = string.Empty;
        private string _searchTextComfirmed = string.Empty;
        private string _currentSearchText = string.Empty;

        private Action _searchChangedAction;

        private InstanceSource _selectedInstanceSource = InstanceSource.Curseforge;
        private int _selectedSourceIndex = 1;

        private bool _isLoaded;

        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        public InstanceSource SelectedInstanceSource
        {
            get => _selectedInstanceSource; set
            {
                _selectedInstanceSource = value;
                OnPropertyChanged(nameof(SelectedInstanceSource));
                _searchChangedAction();
            }
        }

        public RelayCommand SearchCommand
        {
            get => _searchCommand ?? (new RelayCommand(obj =>
            {
                if (_currentSearchText != SearchTextUncomfirmed)
                {
                    if (SearchTextUncomfirmed.Length != 0)
                    {
                        SearchTextComfirmed = SearchTextUncomfirmed;
                        _currentSearchText = SearchTextComfirmed;
                    }
                    else
                        _currentSearchText = "";
                    _searchChangedAction();
                }
            }));
        }
        public string SearchTextUncomfirmed
        {
            get => _searchTextUncomfirmed; set
            {
                _searchTextUncomfirmed = value;
                OnPropertyChanged(nameof(SearchTextUncomfirmed));
            }
        }

        public string SearchTextComfirmed
        {
            get => _searchTextComfirmed; set
            {
                _searchTextComfirmed = value;
                OnPropertyChanged(nameof(SearchTextComfirmed));
            }
        }

        public int SelectedSourceIndex
        {
            get => _selectedSourceIndex; set
            {
                _selectedSourceIndex = value;
                if (value == 0) SelectedInstanceSource = InstanceSource.Nightworld;
                if (value == 1) SelectedInstanceSource = InstanceSource.Curseforge;
                OnPropertyChanged(nameof(SelectedSourceIndex));
            }
        }

        public SearchBoxViewModel(Action action)
        {
            _searchChangedAction = action;
        }
    }
}