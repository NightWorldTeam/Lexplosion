using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.FIlterPanel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent
{
	public sealed class CatalogModel : VMBase
    {
        private static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            ClassId = "",
            ParentCategoryId = "",
            Name = "All"
        };


        private readonly IInstanceController _instanceController;


        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
		private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


		#region Properties


		public IEnumerable<InstanceModelBase> Instances { get => _instanceController.Instances; }


        public CatalogFilterPanel FilterPanel { get; }
        public uint ItemsPerPage { get; set; } = 10;


        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyPage;
        public bool IsEmptyPage
        {
            get => _isEmptyPage; set
            {
                _isEmptyPage = value;
                OnPropertyChanged();
            }
        }

        private uint _currentPageIndex = 0;
        public uint CurrentPageIndex
        {
            get => _currentPageIndex; set
            {
                _currentPageIndex = value;
                OnPropertyChanged();
            }
        }

        private uint _pageCount = 100;
        public uint PageCount
        {
            get => _pageCount; set 
            {
                _pageCount = value;
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


        public CatalogModel(AppCore appCore, IInstanceController instanceController)
        {
            _instanceController = instanceController;
            FilterPanel = new CatalogFilterPanel(appCore, () => OnFilterChanged());
            _resetEvent.Set();
        }


        #endregion Constructors


        #region Public & Properties Methods
        

        private void OnFilterChanged(bool paginatorChanged = false) 
        {
            IsLoading = true;
            _instanceController.Clear();

            if (!paginatorChanged) 
            {
                CurrentPageIndex = 0;            
            }

            Runtime.TaskRun(() =>
            {
                _resetEvent.WaitOne();
                var catalogResult = GetInstanceClients(
                    SearchFilter, 
                    (int)CurrentPageIndex,
                    FilterPanel.SelectedSource.Value, 
                    FilterPanel.SelectedCategories.Count == 0 ? new IProjectCategory[] { AllCategory } : FilterPanel.SelectedCategories, 
                    FilterPanel.SelectedSortByParam.Value,
                    FilterPanel.SelectedVersion, 
                    false);

                IsEmptyPage = catalogResult.Count == 0;

                if (PageCount != catalogResult.TotalCount)
                    PageCount = (uint)(catalogResult.TotalCount > 10 ? (catalogResult.TotalCount / ItemsPerPage) : catalogResult.TotalCount);

                _instanceController.Clear();

                foreach (var i in catalogResult)
                    _instanceController.Add(i);
                OnPropertyChanged(nameof(Instances));
                IsLoading = false;
            });
        }

        
        public void Paginate(uint scrollTo)
        {
            CurrentPageIndex = scrollTo;
            OnFilterChanged(true);
        }

        public void SearchFilterChanged(string searchFilter) 
        {
            SearchFilter = searchFilter;
            OnFilterChanged();
        }

        /// <summary>
        /// Return a list of instances from curseforge/modrinth
        /// </summary>
        /// <param name="searchInput">SearchBox text</param>
        /// <param name="scrollTo">Page Index</param>
        /// <param name="source">Curseforge/Modrinth/Nightworld</param>
        /// <param name="selectedCategories">Selected Category(ies)</param>
        /// <param name="sortBy">CfSortField or ModrinthSortField</param>
        /// <param name="gameVersion">version of minecraft</param>
        /// <param name="isPaginatorInvoke">Is page index changed?</param>
        /// <returns>Tuple[IEnumerable InstanceClient & InstanceClient count </returns>
        private CatalogResult<InstanceClient> GetInstanceClients(string searchInput, int scrollTo, InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, int sortBy, MinecraftVersion gameVersion, bool isPaginatorInvoke = false)
        {
            if (gameVersion == null)
                return new CatalogResult<InstanceClient>();

            ISearchParams searchParams = null;

            switch(source) 
            {
                case InstanceSource.Modrinth:
                    searchParams = new ModrinthSearchParams(searchInput, gameVersion.Id, selectedCategories, (int)ItemsPerPage, (int)CurrentPageIndex, (ModrinthSortField)sortBy);
                    break;
                case InstanceSource.Curseforge:
                    var version = gameVersion.Id;
                    version = version == "All" ? string.Empty : version;
                    searchParams = new CurseforgeSearchParams(searchInput, version, selectedCategories, (int)ItemsPerPage, (int)CurrentPageIndex, (CfSortField)sortBy);
                    break;
                case InstanceSource.Nightworld:
                    searchParams = new NightWorldSearchParams((int)ItemsPerPage, (int)CurrentPageIndex);
                    break;
            }

            return _clientsManager.GetOutsideInstances(source, searchParams);
        }


        #endregion Public & Properties Methods
    }
}

