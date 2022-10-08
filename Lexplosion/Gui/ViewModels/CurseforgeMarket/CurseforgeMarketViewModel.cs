using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;
        private readonly CfProjectType _projectType;
        private readonly ObservableCollection<InstanceAddon> _instanceAddons;
        private readonly BaseInstanceData _baseInstanceData;

        private int _pageSize = 20;
        private string _previousSearch = null;
        private bool _isInit = true;

        public CurseforgeMarketViewModel(MainViewModel mainViewModel, InstanceClient instanceClient, CfProjectType addonsType, ObservableCollection<InstanceAddon> installedAddons)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.UserProfile.IsShowInfoBar = false;

            _instanceClient = instanceClient;
            _projectType = addonsType;
            _instanceAddons = installedAddons;
            _baseInstanceData = instanceClient.GetBaseData;

            // передача делегата загрузки при поиске по тексту
            SearchMethod += InstancePageLoading;
            PaginatorVM.PageChanged += InstancePageLoading;

            // загрузка категорий
            LoadContent();

            IsLoaded = false;
        }


        #region Properties

        public ObservableCollection<InstanceAddon> InstanceAddons { get; } = new ObservableCollection<InstanceAddon>();
        public ObservableCollection<DownloadAddonFile> DownloadAddonFiles { get; } = new ObservableCollection<DownloadAddonFile>();


        #region Navigation

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public Action<string, bool> SearchMethod { get; }

        #endregion Navigation

        #region load fields

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

        #endregion load fields


        #region Categories 

        public ObservableCollection<CfCategory> CfCategories { get; } = new ObservableCollection<CfCategory>();

        private CfCategory _selectedCategory;
        public CfCategory SelectedCategory
        {
            get => _selectedCategory; set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (!_selectedCategory.HasSubCategories)
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

        #endregion Commands


        #region Private Methods

        private void LoadContent()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var curseforgeCategories = CurseforgeApi.GetCategories(_projectType);

                App.Current.Dispatcher.Invoke(() =>
                {
                    var sortedByIdCategories = new Dictionary<int, List<CfCategory>>();

                    foreach (var cfc in curseforgeCategories)
                    {
                        if (!sortedByIdCategories.ContainsKey(cfc.parentCategoryId))
                        {
                            sortedByIdCategories.Add(cfc.parentCategoryId, new List<CfCategory> { new CfCategory(cfc) });
                        }
                        else
                        {
                            sortedByIdCategories[cfc.parentCategoryId].Add(new CfCategory(cfc));
                        }
                    }

                    List<CfCategory> parentCategories = new List<CfCategory>();
                    foreach (var cfc in sortedByIdCategories.Keys)
                    {
                        // cfc == 6 -> Mods not Subcategory
                        if (cfc == (int)_projectType)
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

        private void ClosePage()
        {
            _mainViewModel.UserProfile.IsShowInfoBar = true;
            InstanceAddon.ClearAddonsListCache();
            MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
        }

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
            DownloadAddonFiles.Add(new DownloadAddonFile(instanceAddon));
            OnPropertyChanged(nameof(IsDownloadingSomething));

            Lexplosion.Runtime.TaskRun(delegate
            {
                var result = instanceAddon.InstallLatestVersion(out Dictionary<string, ValuePair<InstanceAddon, DownloadAddonRes>> dependenciesResults);
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (result == DownloadAddonRes.Successful)
                    {
                        _instanceAddons.Add(instanceAddon);
                        MainViewModel.ShowToastMessage("Мод успешно установлен. Не за что.", "Название: " + instanceAddon.Name, TimeSpan.FromSeconds(5d));
                        DownloadAddonFile.Remove(DownloadAddonFiles, instanceAddon);
                        OnPropertyChanged(nameof(IsDownloadingSomething));
                    }
                    else if (result == DownloadAddonRes.IsCanselled)
                    {
                        MainViewModel.ShowToastMessage("Скачивание аддона было отменено.",
                            "Название аддона: " + instanceAddon.Name, Controls.ToastMessageState.Notification);
                        DownloadAddonFile.Remove(DownloadAddonFiles, instanceAddon);
                        OnPropertyChanged(nameof(IsDownloadingSomething));
                    }
                    else
                    {
                        MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод",
                            "Название: " + instanceAddon.Name + ".\nОшибка " + result, Controls.ToastMessageState.Error);
                        DownloadAddonFile.Remove(DownloadAddonFiles, instanceAddon);
                        OnPropertyChanged(nameof(IsDownloadingSomething));
                    }

                    /* обработка установки зависимых модов */
                    if (dependenciesResults != null)
                    {
                        foreach (string key in dependenciesResults.Keys)
                        {
                            ValuePair<InstanceAddon, DownloadAddonRes> data = dependenciesResults[key];
                            if (data.Value2 == DownloadAddonRes.Successful)
                            {
                                if (data.Value1 != null) _instanceAddons.Add(data.Value1);
                                MainViewModel.ShowToastMessage("Зависимый мод успешно установлен",
                                    "Название: " + key + ".\nНеобходим для " + instanceAddon.Name);
                            }
                            else
                            {
                                MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод",
                                    "Название: " + key + ".\nОшибка " + result + ".\nНеобходим для " + instanceAddon.Name, Controls.ToastMessageState.Error);
                            }
                        }
                    }
                });
            });
        }

        private async void InstancePageLoading(string searchText = "", bool isPaginator = false)
        {
            // запускаем заставку загрузки
            if (!isPaginator && searchText == _previousSearch && searchText != null)
            {
                IsLoaded = true;
                return;
            }

            IsLoaded = false;

            Lexplosion.Runtime.TaskRun(() => 
            {
                var instances = InstanceAddon.GetAddonsCatalog(_baseInstanceData, _pageSize, PaginatorVM.PageIndex - 1,
                    (AddonType)(int)_projectType, SubCategorySelected == null ? SelectedCategory.Id : SubCategorySelected.Id, searchText == null ? "" : searchText
                    );

                App.Current.Dispatcher.Invoke(() => 
                { 
                    _previousSearch = searchText == null ? "" : searchText;

                    IsPaginatorVisible = instances.Count == _pageSize;

                    // если аддоны не найдены
                    if (instances.Count == 0)
                    {
                        InstanceAddons.Clear();
                        IsEmptyList = true;
                    }
                    else
                    {
                        IsEmptyList = !IsEmptyList;
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
