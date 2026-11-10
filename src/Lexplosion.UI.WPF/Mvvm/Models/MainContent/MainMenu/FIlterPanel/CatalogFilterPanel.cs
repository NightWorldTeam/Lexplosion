using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.TranslatableObjects;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.FIlterPanel
{
    public class CatalogFilterPanel : ObservableObject
    {
        public event Action FilterChanged;


        private readonly AppCore _appCore;
        private readonly bool _isInitialized;
        private Task _categoriesTask;
        private CancellationTokenSource _categoriesCancellation;
        private InstanceSource _requestedCategorySource;
        private InstanceSource? _loadedCategorySource;
        private int _categoryRequestVersion;


        #region Properties


        public ITranslatableObject<InstanceSource> NoneSource { get; } = new InstanceSourceObject("AllSource", InstanceSource.None);
        public MinecraftVersion AllVersion { get; } = new MinecraftVersion("All", MinecraftVersion.VersionType.Release);

        private IList<SortByParamObject> _sortByParams = new ObservableCollection<SortByParamObject>();
        public IList<SortByParamObject> SortByParams
        {
            get => _sortByParams; set
            {
                _sortByParams = value;
                SelectedSortByParam = value.FirstOrDefault();
                OnPropertyChanged();
            }

        }

        private SortByParamObject _selectedSortByParam;
        public SortByParamObject SelectedSortByParam
        {
            get => _selectedSortByParam; set
            {
                _selectedSortByParam = value;
                FilterChangedExecuteEvent();
                OnPropertyChanged();
            }
        }

        public IList<CategoryBase> AvailableCategories { get; } = new ObservableCollection<CategoryBase>();
        public ISet<CategoryBase> SelectedCategories { get; } = new HashSet<CategoryBase>();
        public IList<ITranslatableObject<InstanceSource>> Sources { get; } = new ObservableCollection<ITranslatableObject<InstanceSource>>();
        public CollectionViewSource VersionCollectionViewSource { get; } = new();


        private MinecraftVersion _selectedVersion;
        public MinecraftVersion SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                FilterChangedExecuteEvent();
                OnPropertyChanged();
            }
        }


        private ITranslatableObject<InstanceSource> _selectedSource;
        public ITranslatableObject<InstanceSource> SelectedSource
        {
            get => _selectedSource; set
            {
                _selectedSource = value;

                if (_isInitialized) 
                {
                    UpdateCategories(value.Value);
                }

                switch (value.Value)
                {
                    case InstanceSource.Modrinth:
                        {
                            _sortByParams.Clear();
                            foreach (var index in Enum.GetValues(typeof(ModrinthSortField)))
                            {
                                var name = Enum.GetName(typeof(ModrinthSortField), index);

                                _sortByParams.Add(new SortByParamObject($"Modrinth{name}", (int)index));
                            }

                            SelectedSortByParam = _sortByParams.First();
                        }
                        break;
                    case InstanceSource.Curseforge:
                        {
                            SortByParams.Clear();
                            foreach (var index in Enum.GetValues(typeof(CfSortField)))
                            {
                                var name = Enum.GetName(typeof(CfSortField), index);

                                var elem = new SortByParamObject($"Curseforge{name}", (int)index);
                                if ((int)index == (int)CfSortField.Popularity) SelectedSortByParam = elem;

                                _sortByParams.Add(elem);
                            }

                        }
                        break;
                    case InstanceSource.Nightworld:
                        _sortByParams.Clear();
                        FilterChangedExecuteEvent();
                        break;
                }


                OnPropertyChanged();
            }
        }


        private bool _isCategoryLoading;
        public bool IsCategoryLoading
        {
            get => _isCategoryLoading; set
            {
                _isCategoryLoading = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public CatalogFilterPanel(AppCore appCore, Action filterChange)
        {
            _appCore = appCore;

            Sources.Add(new InstanceSourceObject("Curseforge", InstanceSource.Curseforge));
            Sources.Add(new InstanceSourceObject("Modrinth", InstanceSource.Modrinth));
            Sources.Add(new InstanceSourceObject("NightWorld", InstanceSource.Nightworld));

            SelectedSource = Sources[0];

            FilterChanged += filterChange;

            if (MainViewModel.AllGameVersions == null)
            {
                MainViewModel.AllVersionsLoaded += MainViewModel_AllVersionsLoaded;
            }
            else
            {
                MainViewModel_AllVersionsLoaded();
            }
            _isInitialized = true;
        }

        private void MainViewModel_AllVersionsLoaded()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var versions = new ObservableCollection<MinecraftVersion>(MainViewModel.AllGameVersions);
                versions.Insert(0, AllVersion);
                SelectedVersion = versions[0];

                VersionCollectionViewSource.Source = versions;
                VersionCollectionViewSource.View.Filter = (mv) => ((mv as MinecraftVersion).Type == MinecraftVersion.VersionType.Release);
            });
        }


        #endregion Constructors


        public Task EnsureCategoriesLoadedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var source = SelectedSource.Value;
            if (_loadedCategorySource == source)
                return Task.CompletedTask;

            if (_categoriesTask != null && !_categoriesTask.IsCompleted && _requestedCategorySource == source)
                return _categoriesTask;

            return LoadCategoriesAsync(source, cancellationToken);
        }

        private void UpdateCategories(InstanceSource instanceSource)
        {
            _ = LoadCategoriesAsync(instanceSource, CancellationToken.None);
        }

        private Task LoadCategoriesAsync(InstanceSource instanceSource, CancellationToken cancellationToken)
        {
            int version = Interlocked.Increment(ref _categoryRequestVersion);
            _requestedCategorySource = instanceSource;
            _loadedCategorySource = null;

            if (_categoriesCancellation != null)
                _categoriesCancellation.Cancel();

            _categoriesCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IsCategoryLoading = true;
            AvailableCategories.Clear();

            _categoriesTask = LoadCategoriesCoreAsync(version, instanceSource, _categoriesCancellation);
            return _categoriesTask;
        }

        private async Task LoadCategoriesCoreAsync(int version, InstanceSource instanceSource, CancellationTokenSource cancellation)
        {
            var token = cancellation.Token;
            try
            {
                var categories = await Runtime.ServicesContainer.CategoriesService.GetModpackCategoriesAsync(
                    EnumManager.InstanceSourceToProjectSource(instanceSource), token);

                token.ThrowIfCancellationRequested();
                if (version != Volatile.Read(ref _categoryRequestVersion) || SelectedSource.Value != instanceSource)
                    return;

                if (categories != null)
                {
                    foreach (var category in categories.Where(item => item.Id != "-1"))
                        AvailableCategories.Add(category);
                }

                _loadedCategorySource = instanceSource;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // A newer source selection has superseded this request.
            }
            catch (Exception error)
            {
                Runtime.DebugWrite("Catalog categories loading failed: " + error.GetType().Name);
            }
            finally
            {
                if (version == Volatile.Read(ref _categoryRequestVersion))
                {
                    _categoriesCancellation = null;
                    IsCategoryLoading = false;
                }

                cancellation.Dispose();
            }
        }

        #region Public Methods


        public void ExecuteFilter()
        {
            SelectedSource = Sources[0];
        }


        public void FilterChangedExecuteEvent([CallerMemberName] string member = "")
        {
            FilterChanged?.Invoke();
            Runtime.DebugWrite($"{member} FilterChanged Executed");
        }


        #endregion Public Methods
    }
}
