﻿using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories.Groups;
using Lexplosion.Logic.Management.Addons;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public abstract class AddonsRepositoryModelBase : ViewModelBase
    {
        protected static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = string.Empty,
            ParentCategoryId = string.Empty
        };

        private bool _isInitialized;

        protected BaseInstanceData _instanceData;
        protected readonly ProjectSource _projectSource;
        protected readonly AddonType _addonType;

        protected readonly ObservableCollection<CategoryWrapper> _categories = new();
        protected readonly ObservableCollection<IProjectCategory> _selectedCategories = new();
        protected ObservableCollection<InstanceAddon> _addonsList = new();
        protected ObservableCollection<CategoryGroup> _categoriesGroups = new();
        protected ObservableCollection<Core.Objects.Modloader> _modloaders = new();
        protected ObservableCollection<Core.Objects.Modloader> _selectedModloaders = new();

        
        protected bool _isClearFilters = false;


        #region Properties


        public abstract ReadOnlyCollection<int> PageSizes { get; }


        public IEnumerable<CategoryWrapper> Categories { get => _categories; }
        public IEnumerable<IProjectCategory> SelectedCategories { get => _selectedCategories; }
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }
        public IEnumerable<CategoryGroup> CategoriesGroups { get => _categoriesGroups; }
        public IEnumerable<Core.Objects.Modloader> Modloaders { get => _modloaders; }
        public IEnumerable<Core.Objects.Modloader> SelectedModloaders { get => _selectedModloaders; }


        public IEnumerable<SortByParamObject> SortByParams { get; protected set; }


        private bool _isModelSelected = false;
        public bool IsModelSelected
        {
            get => _isModelSelected;
            set
            {
                _isModelSelected = value;
                if (_isModelSelected && _isInitialized)
                    LoadContent();
            }
        }


        private uint _pageCount = 1;
        public uint PageCount
        {
            get => _pageCount; set
            {
                _pageCount = value;
                OnPropertyChanged();
            }
        }


        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnSearchFilterChanged();
            }
        }

        private short _selectedSortByIndex = 0;
        public short SelectedSortByIndex
        {
            get => _selectedSortByIndex; set
            {
                _selectedSortByIndex = value;
                OnSortByChanged();
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize; set
            {
                _pageSize = value;
                OnPageSizeChanged();
            }
        }

        private uint _currentPageIndex;
        public uint CurrentPageIndex
        {
            get => _currentPageIndex; set
            {
                _currentPageIndex = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading 
        { 
            get => _isLoading; set 
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        protected AddonsRepositoryModelBase(ProjectSource projectSource, BaseInstanceData instanceData, AddonType addonType, bool isDefaultSelected = false)
        {
            _projectSource = projectSource;
            _instanceData = instanceData;
            _addonType = addonType;

            IsModelSelected = isDefaultSelected;

            foreach (Modloader value in Enum.GetValues(typeof(Modloader)))
            {
                Core.Objects.Modloader modloader = new(value.ToString(), value);
                
                modloader.SelectedChanged += OnModloaderSelectedChanged;

                if (instanceData.Modloader != ClientType.Vanilla && (int)instanceData.Modloader == (int)value)
                {
                    modloader.IsSelected = true;
                    modloader.CanBeSelected = false;
                    //_selectedModloaders.Add(modloader);
                }
                _modloaders.Add(modloader);
            }

            _isInitialized = true;
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected abstract ISearchParams BuildSearchParams();
        protected abstract List<IProjectCategory> GetCategories();


        protected virtual void OnModloaderSelectedChanged(Core.Objects.Modloader modloader, bool isSelected)
        {
            if (isSelected && !_selectedModloaders.Contains(modloader))
            {
                _selectedModloaders.Add(modloader);
            }
            else
            {
                _selectedModloaders.Remove(modloader);
            }
        }

        public void Paginate(uint scrollTo)
        {
            CurrentPageIndex = scrollTo;
            LoadContent();
        }

        protected void LoadContent()
        {
            IsLoading = true;
            Runtime.DebugConsoleWrite(CurrentPageIndex, color: ConsoleColor.Yellow);
            Runtime.TaskRun(() =>
            {
                var catalog = AddonsManager.GetManager(_instanceData)
                    .GetAddonsCatalog(_projectSource, _addonType, BuildSearchParams());

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList.Clear();
                    foreach (var i in catalog)
                    {
                        _addonsList.Add(i);
                    }
                    PageCount = (uint)(catalog.TotalCount / PageSize);
                    IsLoading = false;
                });
            });
        }


        public void ClearFilters()
        {
            /*            _isClearFilters = true;
                        foreach (var category in Categories)
                        {
                            category.IsSelected = false;
                        }
                        _isClearFilters = false;
                        LoadPage();*/
        }

        public void InstallAddon(InstanceAddon instanceAddon)
        {
            //instanceAddon.InstallLatestVersion();
        }


        #endregion Public & Protected Methods 


        #region Private Methods


        protected virtual void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected)
        {
            if (isSelected)
            {
                _selectedCategories.Add(category);
            }
            else
            {
                _selectedCategories.Remove(category);
            }
        }

        private void OnSortByChanged()
        {
            OnPropertyChanged(nameof(SelectedSortByIndex));
            LoadContent();
        }

        private void OnSearchFilterChanged()
        {
            OnPropertyChanged(nameof(SearchFilter));
            LoadContent();
        }

        private void OnCurrentPageIndexChanged()
        {
            OnPropertyChanged(nameof(CurrentPageIndex));
            LoadContent();
        }

        private void OnPageSizeChanged()
        {
            OnPropertyChanged(nameof(PageSize));
            LoadContent();
        }


        #endregion Private Methods
    }

}
