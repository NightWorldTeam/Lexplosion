using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Management.Sources;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public abstract class AddonsRepositoryModelBase : ViewModelBase
    {
        protected static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = "",
            ParentCategoryId = ""
        };


        protected BaseInstanceData _instanceData;
        protected readonly InstanceSource _instanceSource;
        protected readonly AddonType _addonType;


        protected bool _isClearFilters = false;


        #region Properties


        protected readonly ObservableCollection<CategoryWrapper> _categories = new();
        protected readonly ObservableCollection<IProjectCategory> _selectedCategories = new();
        protected ObservableCollection<InstanceAddon> _addonsList = new();


        public IEnumerable<CategoryWrapper> Categories { get => _categories; }
        public IEnumerable<IProjectCategory> SelectedCategories { get => _selectedCategories; }
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }


        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnSearchFilterChanged();
            }
        }

        private byte _selectedSortByIndex = 0;
        public byte SelectedSortByIndex
        {
            get => _selectedSortByIndex; set
            {
                _selectedSortByIndex = value;
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


        #endregion Properties


        #region Constructors


        protected AddonsRepositoryModelBase(InstanceSource instanceSource, BaseInstanceData instanceData, AddonType addonType)
        {
            _instanceSource = instanceSource;
            _instanceData = instanceData;
            _addonType = addonType;

            PrepareCategories();
            LoadContent();
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected abstract ISearchParams BuildSearchParams();
        protected abstract List<IProjectCategory> GetCategories();


        protected void LoadContent()
        {
            Runtime.TaskRun(() =>
            {
                var addons = InstanceAddon.GetAddonsCatalog(_instanceSource, _instanceData, _addonType, BuildSearchParams());

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList.Clear();
                    foreach (var i in addons)
                    {
                        _addonsList.Add(i);
                    }
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


        private void PrepareCategories()
        {
            Runtime.TaskRun(() =>
            {
                var categories = GetCategories();

                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var category in categories)
                    {
                        if (category.ClassId == "mod")
                        {
                            var categoryWrapper = new CategoryWrapper(category);
                            categoryWrapper.SelectedEvent += OnSelectedCategoryChanged;
                            _categories.Add(categoryWrapper);
                        }
                    }
                });
            });
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

    public sealed class AddonsRepositoryModel : AddonsRepositoryModelBase
    {
        public AddonsRepositoryModel(InstanceSource instanceSource, BaseInstanceData instanceData, AddonType addonType) 
            : base(instanceSource, instanceData, addonType)
        {

        }

        public void ApplyCategories() 
        {
            LoadContent();
        }

        protected override ISearchParams BuildSearchParams()
        {

            return new ModrinthSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (ModrinthSortField)SelectedSortByIndex,
                new List<Modloader> { _instanceData.Modloader.ToModloader() });
        }

        protected override List<IProjectCategory> GetCategories()
        {
            if (_instanceSource == InstanceSource.Modrinth)
                return ModrinthApi.GetCategories().ToList<IProjectCategory>();

            if (_instanceSource == InstanceSource.Curseforge)
                return CurseforgeApi.GetCategories(_addonType.ToCfProjectType()).ToList<IProjectCategory>();

            return [];
        }
    }
}
