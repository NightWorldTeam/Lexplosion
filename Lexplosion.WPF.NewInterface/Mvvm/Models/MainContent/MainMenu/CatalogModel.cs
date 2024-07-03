using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.FIlterPanel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
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


        #endregion Properties


        #region Constructors


        public CatalogModel(IInstanceController instanceController)
        {
            _instanceController = instanceController;
            FilterPanel = new CatalogFilterPanel();
            FilterPanel.FilterChanged += OnFilterChanged;
            LoadPageContent();
        }


        #endregion Constructors


        #region Public & Properties Methods
        

        private void OnFilterChanged() 
        {
            Runtime.TaskRun(() =>
            {
                var instanceClientsTuple = GetInstanceClients(
                    SearchFilter, 
                    (int)CurrentPageIndex,
                    FilterPanel.SelectedSource.Value, 
                    FilterPanel.SelectedCategories.Count == 0 ? new IProjectCategory[] { AllCategory } : FilterPanel.SelectedCategories, 
                    FilterPanel.SelectedSortByParam.Value,
                    FilterPanel.SelectedVersion, 
                    false);

                IsEmptyPage = instanceClientsTuple.Item2 == 0;

                if (PageCount != instanceClientsTuple.Item2)
                    PageCount = instanceClientsTuple.Item2;

                _instanceController.Clear();

                foreach (var i in instanceClientsTuple.Item1)
                    _instanceController.Add(i);
                OnPropertyChanged(nameof(Instances));
            });
        }

        
        public void Paginate(uint scrollTo)
        {
            CurrentPageIndex = scrollTo;
            OnFilterChanged();
        }

        public void SearchFilterChanged(string searchFilter) 
        {
            SearchFilter = searchFilter;
            OnFilterChanged();
        }

        private void LoadPageContent() 
        {
            Runtime.TaskRun(() => 
            {
                var instanceClientsTuple = GetInstanceClients(
                    SearchFilter,
                    (int)CurrentPageIndex,
                    InstanceSource.Modrinth, 
                    new IProjectCategory[] { AllCategory },
                    (int)ModrinthSortField.Relevance,
                    new MinecraftVersion(), false);

                IsEmptyPage = instanceClientsTuple.Item2 == 0;

                if (PageCount != instanceClientsTuple.Item2)
                    PageCount = instanceClientsTuple.Item2;

                foreach (var i in instanceClientsTuple.Item1)
                    _instanceController.Add(i);

                OnPropertyChanged(nameof(Instances));
            });
        }


        /// TODO: Сделать метод приватным
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
        public (IEnumerable<InstanceClient>, uint) GetInstanceClients(string searchInput, int scrollTo, InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, int sortBy, MinecraftVersion gameVersion, bool isPaginatorInvoke = false)
        {
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

            return InstanceClient.GetOutsideInstances(source, searchParams);
        }


        #endregion Public & Properties Methods
    }
}

