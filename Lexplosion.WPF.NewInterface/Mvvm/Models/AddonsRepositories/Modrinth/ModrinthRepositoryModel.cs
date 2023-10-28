using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
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

        public int[] PageSizes { get; } = new int[]
        {
            6, 10, 16, 20, 50, 100
        };

        public string[] SortByItems { get; } = new string[]
        {
            "Relevance", "Donwload count", "Follow count", "Recently published", "Recently updated"
        };

        private readonly InstanceModelBase _instanceModelBase;
        private readonly AddonType _addonType;
        private readonly ClientType _clientType;

        private bool _isClearFilters = false;


        #region Properties


        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnSearchFilterChanged();
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

        private string _selectedSortBy = "Relevance";
        public string SelectedSortBy
        {
            get => _selectedSortBy; set
            {
                _selectedSortBy = value;
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


        private readonly ObservableCollection<CategoryWrapper> _categories = new ObservableCollection<CategoryWrapper>();
        public IEnumerable<CategoryWrapper> Categories { get => _categories; }

        private readonly ObservableCollection<IProjectCategory> _selectedCategories = new ObservableCollection<IProjectCategory>();
        public IEnumerable<IProjectCategory> SelectedCategories;


        private ObservableCollection<InstanceAddon> _addonsList = new ObservableCollection<InstanceAddon>();
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public ModrinthRepositoryModel(InstanceModelBase instanceModelBase, AddonType addonType)
        {
            _instanceModelBase = instanceModelBase;
            _addonType = addonType;
            _clientType = instanceModelBase.InstanceData.Modloader;

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


        #endregion Constructors


        #region Public Methods


        public void LoadPage()
        {
            //(IEnumerable<ModrinthProjectInfo>, int) hits;
            //if (_selectedCategories.Count > 0)
            //{
            //    hits = ModrinthApi.GetAddonsList(
            //            PageSize,
            //            PageIndex,
            //            _addonType,
            //            SelectedCategories,
            //            _clientType,
            //            "",
            //            _minecraftVersion.Id);
            //    //_addonsList = new ObservableCollection<InstanceAddon>(hits.Item1);
            //}
            //else
            //{
            //    hits = ModrinthApi.GetAddonsList(
            //        PageSize, PageIndex, _addonType, new IProjectCategory[] { AllCategory }, _clientType, "", _minecraftVersion.Id);
            //    /_addonsList = new ObservableCollection<ModrinthProjectInfo>(hits.Item1);
            //}


            //var s = InstanceAddon.GetAddonsCatalog(null, PageSize, PageIndex, _addonType, SelectedCategories, "");

            _addonsList.Clear();
            foreach (var i in InstanceAddon.GetModrinthAddonsCatalog(_instanceModelBase.InstanceData, PageSize, PageIndex, _addonType, AllCategory, SearchFilter))
            {
                _addonsList.Add(i);
            }

            //AllCategory.ClassId = ((int)_addonType.ToCfProjectType()).ToString();
            //AllCategory.ParentCategoryId = ((int)_addonType.ToCfProjectType()).ToString();
            //var test = CurseforgeApi.GetInstances(10, 0, "all", CfSortField.Popularity, "", "1.19.2");
            //Runtime.DebugWrite("total hits count: " + hits.Item2);
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

        public void InstallAddon(InstanceAddon modrinthProjectInfo)
        {

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

        private void OnSortByChanged()
        {
            OnPropertyChanged(nameof(SelectedSortBy));
            LoadPage();
        }

        private void OnSearchFilterChanged()
        {
            OnPropertyChanged(nameof(SearchFilter));
            LoadPage();
        }

        private void OnPageIndexChanged()
        {
            OnPropertyChanged(nameof(PageIndex));
            LoadPage();
        }

        private void OnPageSizeChanged()
        {
            OnPropertyChanged(nameof(PageSize));
            LoadPage();
        }

        private void OnSelectedCategoriesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isClearFilters)
                return;

            LoadPage();
        }


        #endregion Private Methods
    }
}
