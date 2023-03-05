using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;
        private readonly MainViewModel _mainViewModel;
        private string _previousSearch = null;
        private bool _isInit = true;

        #region Properties


        public ObservableCollection<InstanceFormViewModel> InstanceList { get => _mainViewModel.Model.CurrentInstanceCatalog; }

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();

        #region Filter

        public Action<string, bool> SearchMethod { get; }

        private ObservableCollection<CurseforgeCategory> _categories;
        public ObservableCollection<CurseforgeCategory> Categories
        {
            get => _categories;
            set
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
        /// Curseforge, NightWorld
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

        private CurseforgeCategory _selectedCurseforgeCategory;
        public CurseforgeCategory SelectedCurseforgeCategory
        {
            get => _selectedCurseforgeCategory; set
            {
                _selectedCurseforgeCategory = value;
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


        #endregion Properties


        #region Commands


        private RelayCommand _onScrollCommand;
        public RelayCommand OnScrollCommand
        {
            get => _onScrollCommand ?? (_onScrollCommand = new RelayCommand(obj =>
            {
                // TODO: Возможно тяжелый код.
                foreach (var instance in _mainViewModel.Model.CurrentInstanceCatalog)
                {
                    instance.IsDropdownMenuOpen = false;
                }
            }));
        }


        #endregion Commands


        #region Constructors


        public CatalogViewModel(MainViewModel mainViewModel)
        {
            IsLoaded = false;
            _mainViewModel = mainViewModel;

            SearchMethod += InstancesPageLoading;
            PaginatorVM.PageChanged += InstancesPageLoading;

            // выбираем первый вариант из списка версий [Все версии]
            Lexplosion.Runtime.TaskRun(() =>
            {
                Categories = PrepareCategories();
                InstancesPageLoading();
                _isInit = false;
            });
        }


        #endregion Constructors


        #region Private Methods

        private ObservableCollection<CurseforgeCategory> PrepareCategories()
        {
            var categories = new ObservableCollection<CurseforgeCategory>(
                CurseforgeApi.GetCategories(CfProjectType.Modpacks)
            );

            SelectedCurseforgeCategory = categories[0];

            return categories;
        }


        private void SetSelectedInstanceSourceByIndex(byte value)
        {
            if (value == 0)
                SelectedInstanceSource = InstanceSource.Nightworld;
            else if (value == 1)
                SelectedInstanceSource = InstanceSource.Curseforge;
            else if (value == 2)
                SelectedInstanceSource = InstanceSource.Modrinth;
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
            Lexplosion.Runtime.TaskRun(() =>
            {
                var gameVersion = SelectedVersionIndex == 0 ? "" : MainViewModel.ReleaseGameVersions[SelectedVersionIndex + 1];

                var instances = InstanceClient.GetOutsideInstances(
                    SelectedInstanceSource,
                    _pageSize,
                    PaginatorVM.PageIndex - 1,
                    SelectedCurseforgeCategory.id,
                    searchText == null ? _previousSearch : searchText,
                    SelectedCfSortBy,
                    gameVersion
                    );


                _previousSearch = searchText == null ? _previousSearch : searchText;

                if (instances.Count == _pageSize) IsPaginatorVisible = true;
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        InstanceList.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (IsEmptyList) IsEmptyList = false;

                        InstanceList.Clear();

                        foreach (var instance in instances)
                        {
                            if (_mainViewModel.Model.IsLibraryContainsInstance(instance))
                            {
                                InstanceList.Add(_mainViewModel.Model.GetInstance(instance));
                            }
                            else
                            {
                                var instanceForm = new InstanceFormViewModel(_mainViewModel, instance);
                                InstanceList.Add(instanceForm);
                            }
                        }
                    });

                }
                IsLoaded = true;
            });
        }

        #endregion Private Methods
    }
}