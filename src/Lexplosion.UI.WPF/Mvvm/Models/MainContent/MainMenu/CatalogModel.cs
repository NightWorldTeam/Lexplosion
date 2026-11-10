using Lexplosion.Logic.Management;
using Lexplosion.Logic.Lifecycle;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.FIlterPanel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent
{
	public sealed class CatalogModel : VMBase, IAsyncLoadable
    {
        private static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            ClassId = "",
            ParentCategoryId = "",
            Name = "All"
        };


        private readonly IInstanceController _instanceController;


        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _loadCancellation;
        private Task _loadTask;
        private int _requestVersion;
        private bool _activated;
        private bool _loaded;
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
        }


        #endregion Constructors


        #region Public & Properties Methods
        

        public Task EnsureLoadedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // The filter panel is data-driven; start its first fetch only when visible.
            _ = FilterPanel.EnsureCategoriesLoadedAsync(cancellationToken);

            if (_loaded)
                return Task.CompletedTask;

            if (_loadTask != null && !_loadTask.IsCompleted)
                return _loadTask;

            return RefreshAsync(cancellationToken);
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _activated = true;
            _loaded = false;
            int version = Interlocked.Increment(ref _requestVersion);

            // Snapshot the UI filters before the request leaves the Dispatcher.
            var searchFilter = SearchFilter;
            int pageIndex = (int)CurrentPageIndex;
            var source = FilterPanel.SelectedSource.Value;
            var categories = FilterPanel.SelectedCategories.Count == 0
                ? new IProjectCategory[] { AllCategory }
                : FilterPanel.SelectedCategories.Cast<IProjectCategory>().ToArray();
            int sortBy = FilterPanel.SelectedSortByParam.Value;
            var gameVersion = FilterPanel.SelectedVersion;

            if (_loadCancellation != null)
                _loadCancellation.Cancel();

            _loadCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _loadTask = LoadAsync(version, searchFilter, pageIndex, source, categories, sortBy,
                gameVersion, _loadCancellation);
            return _loadTask;
        }

        private async Task LoadAsync(int version, string searchFilter, int pageIndex,
            InstanceSource source, IProjectCategory[] categories, int sortBy,
            MinecraftVersion gameVersion, CancellationTokenSource cancellation)
        {
            var token = cancellation.Token;
            IsLoading = true;

            try
            {
                // The list of available game versions may arrive after the view.
                // Its existing FilterChanged event will request a new load.
                if (gameVersion == null)
                    return;

                await _loadLock.WaitAsync(token);
                CatalogResult<InstanceClient> result;
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Network search is asynchronous; synchronous logo processing
                    // remains off the WPF thread in ClientsManager.
                    result = await GetInstanceClientsAsync(
                        searchFilter, pageIndex, source, categories, sortBy, gameVersion, token);
                }
                finally
                {
                    _loadLock.Release();
                }

                token.ThrowIfCancellationRequested();
                if (version != Volatile.Read(ref _requestVersion))
                    return;

                IsEmptyPage = result.Count == 0;
                PageCount = (uint)(result.TotalCount > 10
                    ? result.TotalCount / ItemsPerPage : result.TotalCount);

                if (_instanceController is CatalogController catalogController)
                {
                    catalogController.ReplaceAll(result);
                }
                else
                {
                    _instanceController.Clear();
                    foreach (var instance in result)
                        _instanceController.Add(instance);
                }

                OnPropertyChanged(nameof(Instances));
                _loaded = true;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Only the latest request may publish a result.
            }
            catch (Exception error)
            {
                Runtime.DebugWrite("Catalog loading failed: " + error.GetType().Name);
            }
            finally
            {
                if (version == Volatile.Read(ref _requestVersion))
                {
                    _loadCancellation = null;
                    IsLoading = false;
                }
                cancellation.Dispose();
            }
        }

        private void OnFilterChanged(bool paginatorChanged = false)
        {
            if (!paginatorChanged)
                CurrentPageIndex = 0;

            _loaded = false;
            if (_activated)
                _ = RefreshAsync();
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
                    searchParams = new ModrinthSearchParams(searchInput, gameVersion.Id, selectedCategories, (int)ItemsPerPage, scrollTo, (ModrinthSortField)sortBy);
                    break;
                case InstanceSource.Curseforge:
                    var version = gameVersion.Id;
                    version = version == "All" ? string.Empty : version;
                    searchParams = new CurseforgeSearchParams(searchInput, version, selectedCategories, (int)ItemsPerPage, scrollTo, (CfSortField)sortBy);
                    break;
                case InstanceSource.Nightworld:
                    searchParams = new NightWorldSearchParams((int)ItemsPerPage, scrollTo);
                    break;
            }

            return _clientsManager.GetOutsideInstances(source, searchParams);
        }


        private async Task<CatalogResult<InstanceClient>> GetInstanceClientsAsync(string searchInput, int scrollTo,
            InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, int sortBy,
            MinecraftVersion gameVersion, CancellationToken cancellationToken)
        {
            if (gameVersion == null)
                return new CatalogResult<InstanceClient>();

            ISearchParams searchParams = null;
            switch (source)
            {
                case InstanceSource.Modrinth:
                    searchParams = new ModrinthSearchParams(searchInput, gameVersion.Id, selectedCategories,
                        (int)ItemsPerPage, scrollTo, (ModrinthSortField)sortBy);
                    break;
                case InstanceSource.Curseforge:
                    var version = gameVersion.Id;
                    version = version == "All" ? string.Empty : version;
                    searchParams = new CurseforgeSearchParams(searchInput, version, selectedCategories,
                        (int)ItemsPerPage, scrollTo, (CfSortField)sortBy);
                    break;
                case InstanceSource.Nightworld:
                    searchParams = new NightWorldSearchParams((int)ItemsPerPage, scrollTo);
                    break;
            }

            if (searchParams == null)
                return new CatalogResult<InstanceClient>();

            return await _clientsManager.GetOutsideInstancesAsync(source, searchParams, cancellationToken);
        }

        #endregion Public & Properties Methods
    }
}

