using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories
{
    public sealed class CategoryWrapper : ViewModelBase
    {
        public event Action<IProjectCategory, bool> SelectedEvent;

        private IProjectCategory _category { get; }
        public string Name { get => _category.Name; }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set 
            {
                _isSelected = value;
                OnPropertyChanged();
                OnSelectedChanged(value);
            }
        }

        public CategoryWrapper(IProjectCategory category)
        {
            _category = category;
        }

        private void OnSelectedChanged(bool value) 
        {
            SelectedEvent?.Invoke(_category, value);
        }
        
    }

    public sealed class ModrinthRepositoryModel : ViewModelBase
    {
        private static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = "",
            ParentCategoryId = ""
        };


        private readonly AddonType _addonType;
        private readonly ClientType _clientType;
        private readonly string _gameVersion;


        #region Properties


        private bool _isClearFilters = false;

        private string _searchFilter = "";
        public string SearchFilter 
        {
            get => _searchFilter; set 
            {
                _searchFilter = value;
                OnSearchFilterChanged();
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

        private int _pageIndex = 0;
        public int PageIndex
        {
            get => _pageIndex; set
            {
                _pageIndex = value;
                OnPageIndexChanged();
            }
        }

        private readonly ObservableCollection<CategoryWrapper> _categories;
        public IEnumerable<CategoryWrapper> Categories { get => _categories; }

        private readonly ObservableCollection<IProjectCategory> _selectedCategories = new ObservableCollection<IProjectCategory>();
        public IEnumerable<IProjectCategory> SelectedCategories;


        private ObservableCollection<ModrinthProjectInfo> _addonsList;
        public IEnumerable<ModrinthProjectInfo> AddonsList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public ModrinthRepositoryModel(AddonType addonType, ClientType clientType, string gameVersion)
        {
            _addonType = addonType;
            _clientType = clientType;
            _gameVersion = gameVersion;

            _selectedCategories.CollectionChanged += OnSelectedCategoriesCollectionChanged;

            if (_clientType == ClientType.Forge)
            {
                AllCategory.ClassId = ((int)_addonType.ToCfProjectType()).ToString();
                AllCategory.ParentCategoryId = ((int)_addonType.ToCfProjectType()).ToString();
            }
            else if (_clientType == ClientType.Fabric)
            {
                AllCategory.ClassId = "";
                AllCategory.ParentCategoryId = "";
            }

            _categories = new ObservableCollection<CategoryWrapper>(PrepareCategories());
        }

        private void OnSelectedCategoriesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isClearFilters)
                return;

            LoadPage();
        }


        #endregion Constructors


        #region Public Methods


        public void LoadPage()
        {
            _addonsList = new ObservableCollection<ModrinthProjectInfo>(
                ModrinthApi.GetAddonsList(
                    PageSize, PageIndex, _addonType, SelectedCategories, _clientType, "", _gameVersion)
                );
        }

        public void ClearFilters() 
        {
            _isClearFilters = true;
            foreach (var category in Categories) 
            {
                category.IsSelected = false;
            }
            _isClearFilters = false;
            LoadPage();
        }


        #endregion Public Methods


        #region Private Methods


        private IEnumerable<CategoryWrapper> PrepareCategories()
        {
            var categories = ModrinthApi.GetCategories();
            yield return new CategoryWrapper(AllCategory);
            foreach (var category in categories)
            {
                if (category.ClassId == "mod")
                {
                    var categoryWrapper = new CategoryWrapper(category);
                    categoryWrapper.SelectedEvent += OnSelectedCategoryChanged;
                    yield return categoryWrapper;
                }
            }
        }


        private void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected)
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

        private void OnSearchFilterChanged() 
        {
            LoadPage();
        }

        private void OnPageIndexChanged() 
        {
            LoadPage();
        }

        private void OnPageSizeChanged() 
        {
            LoadPage();
        }


        #endregion Private Methods
    }

    public sealed class ModrinthRepositoryViewModel : ViewModelBase
    {
        public ModrinthRepositoryModel Model { get; }


        #region Commands


        private RelayCommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand 
        {
            get => _clearFiltersCommand ?? (_clearFiltersCommand = new RelayCommand(obj => 
            {
                Model.ClearFilters();
            }));
        }

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand 
        {
            get => _searchCommand ?? (_searchCommand = new RelayCommand(obj => 
            {
                Model.SearchFilter = ((string)obj);
            }));
        }


        #endregion Commands


        #region Constructors


        public ModrinthRepositoryViewModel(AddonType addonType, ClientType clientType, string gameVersion)
        {
            Model = new ModrinthRepositoryModel(addonType, clientType, gameVersion);
        }


        #endregion Constructors
    }
}
