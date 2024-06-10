using Lexplosion.Common.Models;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lexplosion.Common.ViewModels.MainMenu
{
    public sealed class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;
        private readonly MainViewModel _mainViewModel;
        private string _previousSearch = null;
        private bool _isInit = true;
        private object _locker = new object();

        #region Properties

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public IEnumerable<InstanceFormViewModel> InstanceList { get => MainModel.Instance.CatalogController.PageInstances; }

        #region Filter

        public Action<string, bool> SearchMethod { get; }


        private Dictionary<InstanceSource, IList<IProjectCategory>> categoriesBySource = new Dictionary<InstanceSource, IList<IProjectCategory>>();

        private IList<IProjectCategory> _categories;
        public IList<IProjectCategory> Categories
        {
            get => _categories; set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public static List<string> CfSortToString { get; } = new List<string>()
        {
            ResourceGetter.GetString("featuredSortBy"),
            ResourceGetter.GetString("popularitySortBy"),
            ResourceGetter.GetString("lastUpdatedSortBy"),
            ResourceGetter.GetString("nameSortBy"),
            ResourceGetter.GetString("authorSortBy"),
            ResourceGetter.GetString("totalDownloadsFlSortBy"),
            ResourceGetter.GetString("categorySortBy"),
            ResourceGetter.GetString("gameVersionSortBy"),
        };

        private InstanceSource _selectedInstanceSource = InstanceSource.Curseforge;
        /// <summary>
        /// Ресурс откуда получаем данные.
        /// Curseforge, NightWorld, Modrinth
        /// </summary>
        public InstanceSource SelectedInstanceSource
        {
            get => _selectedInstanceSource; set
            {
                _selectedInstanceSource = value;
                OnPropertyChanged();
                if (!_isInit) SearchMethod?.Invoke(null, false);
            }
        }

        private string _searchTextUncomfirmed = string.Empty;
        /// <summary>
        /// Содержит текст, который пользователь ввел, но не запустил поиск.
        /// </summary>
        public string SearchTextUncomfirmed
        {
            get => _searchTextUncomfirmed; set
            {
                _searchTextUncomfirmed = value;
                OnPropertyChanged();
            }
        }

        private byte _selectedSourceIndex = 1;
        /// <summary>
        /// Индекс выбраного источника.
        /// 0 - NightWorld
        /// 1 - Curseforge
        /// 2 - Modrinth
        /// </summary>
        public byte SelectedSourceIndex
        {
            get => _selectedSourceIndex; set
            {
                _selectedSourceIndex = value;
                SetSelectedInstanceSourceByIndex(value);
                OnPropertyChanged();
            }
        }

        private IProjectCategory _selectedCategory;
        public IProjectCategory SelectedCategory
        {
            get => _selectedCategory; set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (!_isInit) SearchMethod?.Invoke(null, false);
            }
        }


        public CfSortField SelectedCfSortBy = CfSortField.Popularity;

        private string _selectedCfSortByString = CfSortToString[(int)CfSortField.Popularity - 1];
        public string SelectedCfSortByString
        {
            get => _selectedCfSortByString; set
            {
                _selectedCfSortByString = value;
                OnPropertyChanged();
                SelectedCfSortBy = (CfSortField)CfSortToString.IndexOf(value) + 1;
                if (!_isInit) SearchMethod?.Invoke(null, false);
            }
        }


        private int _selectedVersionIndex = 0;
        public int SelectedVersionIndex
        {
            get => _selectedVersionIndex; set
            {
                _selectedVersionIndex = value;
                OnPropertyChanged();
                if (!_isInit) SearchMethod?.Invoke(null, false);
            }
        }

        #endregion Filter


        private bool _isLoaded;
        /// <summary>
        /// <para>Отвечает на вопрос загрузилась ли страница.</para>
        /// <br><c>Загрузилась - true</c></br>
        /// <br><c>Загружается - false</c></br>
        /// </summary>
        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyList;
        /// <summary>
        /// <para>Отвечает на вопрос количество найденого контента равно 0?</para>
        /// </summary>
        public bool IsEmptyList
        {
            get => _isEmptyList; set
            {
                _isEmptyList = value;
                OnPropertyChanged();
            }
        }

        private bool _isPaginatorVisible = false;
        public bool IsPaginatorVisible
        {
            get => _isPaginatorVisible; set
            {
                _isPaginatorVisible = value;
                OnPropertyChanged();
            }
        }

        private string _loaderPlaceholder;
        public string LoaderPlaceholder
        {
            get => _loaderPlaceholder; set
            {
                _loaderPlaceholder = value;
                OnPropertyChanged();
            }
        }

        private bool _isPageIsEmptyAndNotFirst = false;
        public bool IsPageIsEmptyAndNotFirst
        {
            get => _isPageIsEmptyAndNotFirst; set
            {
                _isPageIsEmptyAndNotFirst = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _onScrollCommand;
        public RelayCommand OnScrollCommand
        {
            get => _onScrollCommand ?? (_onScrollCommand = new RelayCommand(obj =>
            {
                // TODO: Возможно тяжелый код.
                foreach (var instance in MainModel.Instance.CatalogController.PageInstances)
                {
                    instance.IsDropdownMenuOpen = false;
                }
            }));
        }


        private RelayCommand _resetPaginatorCommand;
        public RelayCommand ResetPaginatorCommand
        {
            get => _resetPaginatorCommand ?? (_resetPaginatorCommand = new RelayCommand(obj =>
            {
                PaginatorVM.PageNum = 1;
                PaginatorVM.ToGeneralPage();
                IsPaginatorVisible = true;
                IsPageIsEmptyAndNotFirst = false;
            }));
        }


        #endregion Commands


        #region Constructors


        public CatalogViewModel(MainViewModel mainViewModel)
        {
            LoaderPlaceholder = ResourceGetter.GetString("curseforgeDataLoading");
            IsLoaded = false;
            _mainViewModel = mainViewModel;

            SearchMethod += InstancesPageLoading;
            PaginatorVM.PageChanged += InstancesPageLoading;

            // выбираем первый вариант из списка версий [Все версии]
            Lexplosion.Runtime.TaskRun(() =>
            {
                Categories = PrepareCategories(InstanceSource.Curseforge);
                InstancesPageLoading();
                _isInit = false;
            });
        }


        #endregion Constructors


        #region Private Methods


        private IList<IProjectCategory> PrepareCategories(InstanceSource instanceSource)
        {
            IList<IProjectCategory> categories;

            if (categoriesBySource.TryGetValue(instanceSource, out categories))
            {
                return categories;
            }

            categories = new ObservableCollection<IProjectCategory>(
                CategoriesManager.GetModpackCategories(EnumManager.InstanceSourceToProjectSource(instanceSource))
            );

            SelectedCategory = categories[0];

            return categories;
        }


        private void SetSelectedInstanceSourceByIndex(byte value)
        {
            if (value == 0)
            {
                LoaderPlaceholder = ResourceGetter.GetString("nightworldDataLoading");
                SelectedInstanceSource = InstanceSource.Nightworld;
            }
            else if (value == 1)
            {
                LoaderPlaceholder = ResourceGetter.GetString("curseforgeDataLoading");
                Categories = PrepareCategories(InstanceSource.Curseforge);
                SelectedInstanceSource = InstanceSource.Curseforge;
            }
            else if (value == 2)
            {
                LoaderPlaceholder = ResourceGetter.GetString("modrinthDataLoading");
                Categories = PrepareCategories(InstanceSource.Modrinth);
                SelectedInstanceSource = InstanceSource.Modrinth;
            }
        }

        private void InstancesPageLoading(string searchText = "", bool isPaginatorInvoke = false)
        {
            if (!isPaginatorInvoke && searchText == _previousSearch)
            {
                IsLoaded = true;
                return;
            }

            if (!isPaginatorInvoke && PaginatorVM.PageIndex > 1)
            {
                PaginatorVM.PageIndex = 1;
            }

            IsLoaded = false;
            Lexplosion.Runtime.TaskRun((System.Threading.ThreadStart)(() =>
            {
                lock (_locker)
                {
                    var gameVersion = SelectedVersionIndex == 0 ? "" : MainViewModel.ReleaseGameVersions[SelectedVersionIndex + 1].Id;
                    Debug.WriteLine(SelectedInstanceSource);
                    var instances = InstanceClient.GetOutsideInstances(
                        SelectedInstanceSource,
                        _pageSize,
                        PaginatorVM.PageIndex - 1,
                        new IProjectCategory[1] { SelectedCategory },
                        searchText == null ? _previousSearch : searchText,
                        SelectedCfSortBy,
                        (string)gameVersion
                        ).Item1;


                    _previousSearch = searchText == null ? _previousSearch : searchText;

                    if (instances.Count == _pageSize) IsPaginatorVisible = true;
                    else if (instances.Count == 0)
                    {
                        IsPageIsEmptyAndNotFirst = true;
                        IsPaginatorVisible = false;
                    }
                    else
                    {
                        IsPageIsEmptyAndNotFirst = false;
                        IsPaginatorVisible = false;
                    }


                    if (instances.Count == 0)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainModel.Instance.CatalogController.Clear();
                            IsEmptyList = true;
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (IsEmptyList) IsEmptyList = false;

                            MainModel.Instance.CatalogController.Clear();

                            foreach (var instance in instances)
                            {
                                if (MainModel.Instance.LibraryController.IsLibraryContainsInstance(instance))
                                {
                                    MainModel.Instance.CatalogController.AddInstance(MainModel.Instance.LibraryController.GetInstance(instance));
                                }
                                else
                                {
                                    var instanceForm = new InstanceFormViewModel(_mainViewModel, instance);
                                    MainModel.Instance.CatalogController.AddInstance(instanceForm);
                                }
                            }
                        });

                    }
                    IsLoaded = true;
                }
            }));
        }


        #endregion Private Methods
    }
}