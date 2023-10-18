using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories
{
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
        private readonly MinecraftVersion _minecraftVersion;


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

        private readonly ObservableCollection<CategoryWrapper> _categories = new ObservableCollection<CategoryWrapper>();
        public IEnumerable<CategoryWrapper> Categories { get => _categories; }

        private readonly ObservableCollection<IProjectCategory> _selectedCategories = new ObservableCollection<IProjectCategory>();
        public IEnumerable<IProjectCategory> SelectedCategories;


        private ObservableCollection<ModrinthProjectInfo> _addonsList = new ObservableCollection<ModrinthProjectInfo>();
        public IEnumerable<ModrinthProjectInfo> AddonsList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public ModrinthRepositoryModel(AddonType addonType, ClientType clientType, MinecraftVersion minecraftVersion)
        {
            _addonType = addonType;
            _clientType = clientType;
            _minecraftVersion = minecraftVersion;

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

            PrepareCategories();
            LoadPage();
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
            (IEnumerable<ModrinthProjectInfo>, int) hits;
            if (_selectedCategories.Count > 0)
            {
                hits = ModrinthApi.GetAddonsList(
                        PageSize,
                        PageIndex,
                        _addonType,
                        SelectedCategories,
                        _clientType,
                        "",
                        _minecraftVersion.Id);
                _addonsList = new ObservableCollection<ModrinthProjectInfo>(hits.Item1);
            }
            else
            {
                hits = ModrinthApi.GetAddonsList(
                    PageSize, PageIndex, _addonType, new IProjectCategory[] { AllCategory }, _clientType, "", _minecraftVersion.Id);
                _addonsList = new ObservableCollection<ModrinthProjectInfo>(hits.Item1);
            }


            //AllCategory.ClassId = ((int)_addonType.ToCfProjectType()).ToString();
            //AllCategory.ParentCategoryId = ((int)_addonType.ToCfProjectType()).ToString();
            var test = CurseforgeApi.GetInstances(10, 0, "all", CfSortField.Popularity, "", "1.19.2");
            Runtime.DebugWrite("total hits count: " + hits.Item2);
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


        private void PrepareCategories()
        {
            var categories = ModrinthApi.GetCategories();

            foreach (var category in categories)
            {
                if (category.ClassId == "mod")
                {
                    var categoryWrapper = new CategoryWrapper(category);
                    categoryWrapper.SelectedEvent += OnSelectedCategoryChanged;
                    _categories.Add(categoryWrapper);
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
        private readonly NavigateCommand<ViewModelBase> _backToInstanceProfile;

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

        private RelayCommand _backToInstanceProfileCommand;
        public ICommand BackToInstanceProfileCommand 
        {
            get => RelayCommand.GetCommand(ref _backToInstanceProfileCommand, (obj) => { _backToInstanceProfile.Execute(obj); });
        }


        #endregion Commands


        #region Constructors


        public ModrinthRepositoryViewModel(NavigateCommand<ViewModelBase> backCommand, AddonType addonType, ClientType clientType, MinecraftVersion gameVersion)
        {
            _backToInstanceProfile = backCommand;
            Model = new ModrinthRepositoryModel(addonType, clientType, gameVersion);
        }


        #endregion Constructors
    }
}
