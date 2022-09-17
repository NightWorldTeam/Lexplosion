using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public class CfCategory : CfCategoryBase
    {
        public CfCategory(CurseforgeCategory curseforgeCategory) : base(curseforgeCategory)
        {
        }
    }

    public sealed class CfParentCategory : CfCategory
    {
        private List<CfCategory> _cfSubCategories;
        public List<CfCategory> CfSubCategories 
        { 
            get => _cfSubCategories; set 
            {
                _cfSubCategories = value;
                OnPropertyChanged();
            }
        }

        public CfParentCategory(CurseforgeCategory curseforgeCategory, List<CfCategory> categories) : base(curseforgeCategory)
        {
            CfSubCategories = categories;
        }
    }


    public abstract class CfCategoryBase : VMBase
    {
        private readonly CurseforgeCategory _curseforgeCategory;

        public string Name { get; }
        public int Id { get; }
        public byte[] ImageBytes { get; private set; }

        public CfCategoryBase(CurseforgeCategory curseforgeCategory) 
        {
            _curseforgeCategory = curseforgeCategory;
            Name = curseforgeCategory.name;
            Id = curseforgeCategory.id;
            ImageBytes = null;
            //IsChildCategory = _curseforgeCategory.parentCategoryId != _curseforgeCategory.classId;
        }

        public void LoadImage() 
        {
            ImageBytes = ImageTools.GetImageBytesByUrl(_curseforgeCategory.iconUrl);
        }

        public CfParentCategory GetParentCategory()
        {
            return new CfParentCategory(_curseforgeCategory, new List<CfCategory> { });
        }

        public CfCategory GetCfCategory() 
        {
            return new CfCategory(_curseforgeCategory);
        }
    }

    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;
        private readonly CfProjectType _projectType;
        private readonly ObservableCollection<InstanceAddon> _instanceAddons;
        private readonly BaseInstanceData _baseInstanceData;

        public CurseforgeMarketViewModel(MainViewModel mainViewModel, InstanceClient instanceClient, CfProjectType addonsType, ObservableCollection<InstanceAddon> installedAddons)
        {
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;
            _projectType = addonsType;
            _instanceAddons = installedAddons;
            _baseInstanceData = instanceClient.GetBaseData;
            LoadCategories();
            IsLoaded = true;

            Lexplosion.Run.TaskRun(() => 
            {
                Thread.Sleep(5000);
                foreach (var cfc in ModCategories) 
                {
                    cfc.LoadImage();
                }
            });
        }


        #region Properties

        public ObservableCollection<CfCategoryBase> ModCategories { get; } = new ObservableCollection<CfCategoryBase>();
        public ObservableCollection<InstanceAddon> InstanceAddons { get; }

        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel();
        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();

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

        private int _selectedCategoryId = -1;
        public int SelectedCategoryId
        {
            get => _selectedCategoryId; set
            {
                _selectedCategoryId = value;
                OnPropertyChanged();
            }
        }

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

        private RelayCommand _goToCurserforgeCommand;
        public RelayCommand GoToCurserforgeCommand 
        {
            get => _goToCurserforgeCommand ?? (_goToCurserforgeCommand = new RelayCommand(obj => 
            {
                var link = (string)obj;
                GoToCurseforge(link);
            }));
        }

        private RelayCommand _installModCommand;
        public RelayCommand InstallModCommand 
        {
            get => _installModCommand ?? (_installModCommand = new RelayCommand(obj => 
            {
                InstallAddon((InstanceAddon)obj);
            }));
        }

        #endregion Commands


        #region Public & Protected Methods
        #endregion Public & Protected Methods


        #region Private Methods

        private void LoadCategories() 
        {
            Lexplosion.Run.TaskRun(() => 
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

                    List<CfParentCategory> parentCategories = new List<CfParentCategory>();
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
                                    categoryInstance = category.GetParentCategory();
                                    parentCategories.Add((CfParentCategory)categoryInstance);
                                }
                                ModCategories.Add(categoryInstance);
                            }
                        }
                        else 
                        {
                            foreach (var parentCategory in parentCategories) 
                            {
                                parentCategory.CfSubCategories = sortedByIdCategories[parentCategory.Id];
                            }
                        }
                    }



                    foreach (var so in sortedByIdCategories) { 
                        var str = "";
                        foreach (var s in so.Value) 
                        {
                            str += s.Name + " ";
                        }

                        Console.WriteLine("Key: " + so.Key + " Value: " + str);
                    }
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

        #endregion Private Methods   
    }
}
