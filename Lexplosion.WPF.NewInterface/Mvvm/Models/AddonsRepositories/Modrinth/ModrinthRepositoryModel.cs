using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public abstract class AdodonsRepositoryModel : ViewModelBase
    {
        private static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = "",
            ParentCategoryId = ""
        };


        #region Properties


        private readonly ObservableCollection<CategoryWrapper> _categories = new();
        private readonly ObservableCollection<IProjectCategory> _selectedCategories = new();
        private ObservableCollection<InstanceAddon> _addonsList = new();


        public IEnumerable<CategoryWrapper> Categories { get => _categories; }
        public IEnumerable<IProjectCategory> SelectedCategories { get => _selectedCategories; }
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        protected AdodonsRepositoryModel(BaseInstanceData instanceData, AddonType addonType)
        {
            
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected void LoadContent() 
        {
            
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

        public void InstallAddon()
        {

        }


        #endregion Public & Protected Methods 


        #region Private Methods





        #endregion Private Methods
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

        public ReadOnlyCollection<uint> PageSizes { get; } = new ReadOnlyCollection<uint>(new uint[]
        {
            6, 10, 16, 20, 50, 100
        });

        public ReadOnlyCollection<string> SortByItems { get; } = new ReadOnlyCollection<string>(new string[]
        {
            "Relevance", "Donwload count", "Follow count", "Recently published", "Recently updated"
        });


        //private readonly InstanceModelBase _instanceModelBase;
        private readonly BaseInstanceData _instanceData;
        private readonly AddonType _addonType;
        private ClientType _clientType;

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

        private string _selectedSortBy = "Relevance";
        public string SelectedSortBy
        {
            get => _selectedSortBy; set
            {
                _selectedSortBy = value;
                OnSortByChanged();
            }
        }

        private uint _pageSize = 10;
        public uint PageSize
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

        private readonly ObservableCollection<CategoryWrapper> _categories = new ObservableCollection<CategoryWrapper>();
        public IEnumerable<CategoryWrapper> Categories { get => _categories; }

        private readonly ObservableCollection<IProjectCategory> _selectedCategories = new ObservableCollection<IProjectCategory>();
        public IEnumerable<IProjectCategory> SelectedCategories;


        private ObservableCollection<InstanceAddon> _addonsList = new ObservableCollection<InstanceAddon>();
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public ModrinthRepositoryModel(BaseInstanceData instanceData, AddonType addonType)
        {
            _instanceData = instanceData;
            _clientType = _instanceData.Modloader;
            _addonType = addonType;

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
            foreach (var i in InstanceAddon.GetModrinthAddonsCatalog(_instanceData, (int)PageSize, (int)CurrentPageIndex, _addonType, AllCategory, SearchFilter))
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

        private void OnCurrentPageIndexChanged()
        {
            OnPropertyChanged(nameof(CurrentPageIndex));
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
