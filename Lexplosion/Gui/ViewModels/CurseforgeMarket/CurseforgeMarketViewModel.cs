using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly Action<string, string, uint, byte> _doNotification = (header, message, time, type) => { };
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;
        private readonly CfProjectType _projectType;
        private readonly FactoryDLCModel _factoryDLCModel;
        private readonly BaseInstanceData _baseInstanceData;
        private readonly FactoryDLCVM _factoryDLCVM;

        private int _pageSize = 20;
        private string _previousSearch = null;
        private bool _isInit = true;

        private static readonly Dictionary<InstanceClient, ObservableCollection<DownloadAddonFile>> InstallingAddons = new Dictionary<InstanceClient, ObservableCollection<DownloadAddonFile>>();
        private static object _installingAddonsLocker = new object();

        public CurseforgeMarketViewModel(MainViewModel mainViewModel, InstanceClient instanceClient, CfProjectType addonsType, FactoryDLCModel factoryDLCModel, FactoryDLCVM factoryDLCVM, Action<string, string, uint, byte> doNotification = null)
        {
            _doNotification = doNotification ?? _doNotification;
            _mainViewModel = mainViewModel;
            _mainViewModel.UserData.IsShowInfoBar = false;

            _instanceClient = instanceClient;
            _projectType = addonsType;
            _factoryDLCModel = factoryDLCModel;
            _baseInstanceData = instanceClient.GetBaseData;
            _factoryDLCVM = factoryDLCVM;
            // передача делегата загрузки при поиске по тексту
            SearchMethod += InstancePageLoading;
            PaginatorVM.PageChanged += InstancePageLoading;

            // загрузка категорий
            LoadContent();

            lock (_installingAddonsLocker)
            {
                if (!InstallingAddons.ContainsKey(_instanceClient))
                {
                    DownloadAddonFiles = new ObservableCollection<DownloadAddonFile>();
                }
                else
                {
                    DownloadAddonFiles = InstallingAddons[_instanceClient];
                }
            }

            IsLoaded = false;
        }

        #region Properties

        public ObservableCollection<InstanceAddon> InstanceAddons { get; } = new ObservableCollection<InstanceAddon>();
        public ObservableCollection<DownloadAddonFile> DownloadAddonFiles { get; }


        #region Navigation

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public Action<string, bool> SearchMethod { get; }

        #endregion Navigation

        
        #region Load fields

        private bool _isLoaded;

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

        #endregion Load fields


        #region Categories


        public ObservableCollection<CfCategory> CfCategories { get; } = new ObservableCollection<CfCategory>();

        private CfCategory _selectedCategory;
        public CfCategory SelectedCategory
        {
            get => _selectedCategory; set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (!_selectedCategory.HasSubcategories)
                    SubCategorySelected = null;
                if (!_isInit)
                {
                    SearchMethod?.Invoke(null, false);
                }
            }
        }

        private CfCategory _subCategorySelected;
        public CfCategory SubCategorySelected
        {
            get => _subCategorySelected; set
            {
                _subCategorySelected = value;
                OnPropertyChanged();
                if (!_isInit)
                {
                    SearchMethod?.Invoke(null, false);
                }
            }
        }

        #endregion Categories

        public bool IsDownloadingSomething { get => DownloadAddonFiles.Count != 0; }


        #endregion Properties


        #region Commands

        private RelayCommand _closePageCommand;

        public RelayCommand ClosePageCommand
        {
            get => _closePageCommand ?? (_closePageCommand = new RelayCommand(obj =>
            {
                ClosePage();
            }));
        }

        private RelayCommand _goToCurseforgeCommand;

        public RelayCommand GoToCurseforgeCommand
        {
            get => _goToCurseforgeCommand ?? (_goToCurseforgeCommand = new RelayCommand(obj =>
            {
                var link = (string)obj;
                GoToCurseforge(link);
            }));
        }

        private RelayCommand _installAddonCommand;

        public RelayCommand InstallAddonCommand
        {
            get => _installAddonCommand ?? (_installAddonCommand = new RelayCommand(obj =>
            {
                InstallAddon((InstanceAddon)obj);
            }));
        }

        private RelayCommand _cancelAddonDownload;

        public RelayCommand CancelAddonDownload
        {
            get => _cancelAddonDownload ?? (_cancelAddonDownload = new RelayCommand(obj =>
            {
                ((InstanceAddon)obj).CancellDownload();
            }));
        }

        #endregion Commands


        #region Private Methods

        /// <summary>
        /// Загрузка категорий.
        /// </summary>
        private void LoadContent()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var curseforgeCategories = CurseforgeApi.GetCategories(_projectType);

                App.Current.Dispatcher.Invoke(() =>
                {
                    var sortedByIdCategories = new Dictionary<string, List<CfCategory>>();

                    foreach (var cfc in curseforgeCategories)
                    {
                        if (!sortedByIdCategories.ContainsKey(cfc.ParentCategoryId))
                        {
                            sortedByIdCategories.Add(cfc.ParentCategoryId, new List<CfCategory> { new CfCategory(cfc) });
                        }
                        else
                        {
                            sortedByIdCategories[cfc.ParentCategoryId].Add(new CfCategory(cfc));
                        }
                    }

                    List<CfCategory> parentCategories = new List<CfCategory>();
                    foreach (var cfc in sortedByIdCategories.Keys)
                    {
                        // cfc == 6 -> Mods not Subcategory
                        if (cfc == ((int)_projectType).ToString())
                        {
                            foreach (var category in sortedByIdCategories[cfc])
                            {
                                var categoryInstance = category;
                                if (sortedByIdCategories.ContainsKey(category.Id))
                                {
                                    parentCategories.Add(category);
                                }
                                if (categoryInstance.Name == "Fabric" && categoryInstance.Name != _baseInstanceData.Modloader.ToString())
                                    continue;
                                CfCategories.Add(categoryInstance);
                            }
                        }
                        else
                        {
                            foreach (var parentCategory in parentCategories)
                            {
                                foreach (var cfCategory in CfCategories)
                                {
                                    if (cfCategory.Name == parentCategory.Name)
                                    {
                                        cfCategory.CfSubCategories = sortedByIdCategories[parentCategory.Id].ToArray();
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    SelectedCategory = CfCategories[0];
                    InstancePageLoading();
                    _isInit = false;
                });
            });
        }

        /// <summary>
        /// Закрытие страницы.
        /// </summary>
        private void ClosePage()
        {
            _mainViewModel.UserData.IsShowInfoBar = true;
            InstanceAddon.ClearAddonsListCache();
            MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
        }

        /// <summary>
        /// Переход на страницу curseforge'a.
        /// </summary>
        /// <param name="link"></param>
        private void GoToCurseforge(string link)
        {
            try
            {
                System.Diagnostics.Process.Start(link);
            }
            catch
            {
                // message box here.
            }
        }

        private void InstallAddon(InstanceAddon instanceAddon)
        {
            var stateData = new DynamicStateData<ValuePair<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>();

            stateData.StateChanged += (arg, state) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    InstanceAddon addonInstance = arg.Value1;
                    if (state == InstanceAddon.InstallAddonState.StartDownload)
                    {
                        lock (_installingAddonsLocker)
                        {
                            DownloadAddonFiles.Add(new DownloadAddonFile(addonInstance));
                            InstallingAddons[_instanceClient] = DownloadAddonFiles;
                        }

                        OnPropertyChanged(nameof(IsDownloadingSomething));
                    }
                    else
                    {
                        if (arg.Value2 == DownloadAddonRes.Successful)
                        {
                            string text, title;
                            if (instanceAddon == addonInstance)
                            {
                                title = "Мод успешно установлен. Не за что";
                                text = "Название: " + addonInstance.Name;
                            }
                            else
                            {
                                title = "Необходимый мод успешно установлен";
                                text = "Название: " + addonInstance.Name + ".\nНеобходим для " + instanceAddon.Name;
                            }

                            _factoryDLCModel.InstalledAddons.Add(addonInstance);
                            _factoryDLCVM.CurrentAddonModel.IsEmptyList = false;
                            _doNotification(title, text, 5, 0);
                        }
                        else if (arg.Value2 == DownloadAddonRes.IsCanselled)
                        {
                            _doNotification("Скачивание аддона было отменено", "Название аддона: " + addonInstance.Name, 0, 1);
                        }
                        else
                        {
                            _doNotification("Извиняемся, не удалось установить мод", "Название: " + addonInstance.Name + ".\nОшибка " + arg.Value2, 0, 1);
                        }

                        lock (_installingAddonsLocker)
                        {
                            DownloadAddonFile.Remove(DownloadAddonFiles, addonInstance);
                            if (DownloadAddonFiles.Count == 0)
                            {
                                InstallingAddons.Remove(_instanceClient);
                            }
                        }

                        OnPropertyChanged(nameof(IsDownloadingSomething));
                    }
                });
            };

            Lexplosion.Runtime.TaskRun(delegate
            {
                instanceAddon.InstallLatestVersion(stateData.GetHandler);
            });
        }

        private void InstancePageLoading(string searchText = "", bool isPaginatorInvoke = false)
        {
            // запускаем заставку загрузки
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
                var instances = InstanceAddon.GetAddonsCatalog(
                    _baseInstanceData,
                    _pageSize,
                    PaginatorVM.PageIndex - 1,
                    (AddonType)(int)_projectType,
                    SubCategorySelected == null ? SelectedCategory.Id : SubCategorySelected.Id,
                    searchText == null ? _previousSearch : searchText
                    );

                App.Current.Dispatcher.Invoke(() =>
                {
                    _previousSearch = searchText == null ? _previousSearch : searchText;

                    IsPaginatorVisible = instances.Count == _pageSize;

                    // если аддоны не найдены
                    if (instances.Count == 0)
                    {
                        InstanceAddons.Clear();
                        IsEmptyList = true;
                    }
                    else
                    {
                        IsEmptyList = false;
                        InstanceAddons.Clear();

                        foreach (var instance in instances)
                        {
                            InstanceAddons.Add(instance);
                        }
                    }

                    IsLoaded = true;
                });
            });
        }

        #endregion Private Methods

    }
}