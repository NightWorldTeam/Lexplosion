using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{

    public class CfCategory : VMBase
    {
        private readonly CurseforgeCategory _curseforgeCategory;

        #region Properities

        public string Name { get; }
        public int Id { get; }
        public byte[] ImageBytes { get; private set; }
        public bool HasSubCategories { get; }

        private bool _hasSubcategies;
        public bool HasSubcategories
        {
            get => _hasSubcategies; private set
            {
                _hasSubcategies = value;
                OnPropertyChanged();
            }
        }

        private CfCategory[] _cfSubcategories;
        public CfCategory[] CfSubCategories
        {
            get => _cfSubcategories; set
            {
                _cfSubcategories = value;
                OnPropertyChanged();
                Console.WriteLine(CfSubCategories != null ? CfSubCategories.Length : "null");
                HasSubcategories = value != null;
            }
        }

        #endregion Properities

        #region Constructors

        public CfCategory(CurseforgeCategory curseforgeCategory, CfCategory[] categories = null)
        {
            _curseforgeCategory = curseforgeCategory;
            Name = curseforgeCategory.name;
            Id = curseforgeCategory.id;
            ImageBytes = null;
            HasSubCategories = categories != null;
            CfSubCategories = categories;
        }

        #endregion Constructors
    }

    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;
        private readonly CfProjectType _projectType;
        private readonly ObservableCollection<InstanceAddon> _instanceAddons;
        private readonly BaseInstanceData _baseInstanceData;

        private int _pageSize = 20;

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

            //foreach (var cfc in CfCategories) 
            //{
            //    cfc.LoadImage();
            //}
        }


        #region Properties

        public ObservableCollection<InstanceAddon> InstanceAddons { get; } = new ObservableCollection<InstanceAddon>();

        #region Navigation

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public Action<string> SearchMethod { get; }

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
            }
        }

        private CfCategory _subCategorySelected;
        public CfCategory SubCategorySelected
        {
            get => _subCategorySelected; set
            {
                _subCategorySelected = value;
                OnPropertyChanged();
            }
        }
        #endregion Categories


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


        #region Public & Protected Methods
        #endregion Public & Protected Methods


        #region Private Methods

        private void LoadContent()
        {
            Lexplosion.Run.TaskRun(() => { 
                var curseforgeCategories = CurseforgeApi.GetCategories(_projectType);

                App.Current.Dispatcher.Invoke(() => { 
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

                    SelectedCategory = CfCategories[CfCategories.Count - 1];
                    InstancePageLoading();
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
            Lexplosion.Run.TaskRun(delegate
            {
                DownloadAddonRes result = instanceAddon.InstallLatestVersion(out Dictionary<string, ValuePair<InstanceAddon, DownloadAddonRes>> dependenciesResults);
                App.Current.Dispatcher.Invoke(() =>
                {
                    /*  */
                    if (result == DownloadAddonRes.Successful)
                    {
                        _instanceAddons.Add(instanceAddon);
                        MainViewModel.ShowToastMessage("Мод успешно установлен. Не за что.", "Название: " + instanceAddon.Name, TimeSpan.FromSeconds(5d));
                    }
                    else
                    {
                        MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод",
                            "Название: " + instanceAddon.Name + ".\nОшибка " + result, Controls.ToastMessageState.Error);
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

        private async void InstancePageLoading(string searchText = "")
        {
            // запускаем заставку загрузки
            IsLoaded = false;

            Console.WriteLine(searchText);

            var instances = await Task.Run(() => InstanceAddon.GetAddonsCatalog(_baseInstanceData, _pageSize, PaginatorVM.PageIndex - 1,
                AddonType.Mods, SubCategorySelected == null ? SelectedCategory.Id : SubCategorySelected.Id, searchText)
            );

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
        }

        #endregion Private Methods   
    }
}
