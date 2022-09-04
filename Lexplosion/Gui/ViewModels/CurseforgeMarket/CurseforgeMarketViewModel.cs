using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public sealed class AddonCategory
    {
        public int Id { get; }
        public AddonType Type { get; }
        public string Name { get; }
        public string ImageSource { get; }
        public bool HasSubcategory { get; }
        public List<AddonCategory> Subcategory { get; }

        private readonly string iconsSource = "pack://Application:,,,/assets/images/icons/curseforge/";

        public AddonCategory(int id, AddonType type, string name, string iconSource, List<AddonCategory> subcategory = null)
        {
            Id = id;
            Name = name;
            ImageSource = iconSource + type.ToString().ToLower() + "/" + iconSource.ToLower() + ".png";

            if (subcategory == null)
            {
                subcategory = new List<AddonCategory>();
                HasSubcategory = false;
            }
            else Subcategory = subcategory;
        }

        public static List<AddonCategory> GetCategories(AddonType type)
        {
            var result = new List<AddonCategory>();
            var i = -1;

            if (type == AddonType.Mods)
            {
                foreach (var value in Enum.GetValues(typeof(ModCategory)))
                {
                    var addonCategory = new AddonCategory(
                        i,
                        AddonType.Mods,
                        value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"),
                        value.ToString().Replace("__", string.Empty).Replace("_", string.Empty).Replace("CharAnd", string.Empty)
                        );

                    result.Add(addonCategory);
                    Console.WriteLine(addonCategory.ToString());
                    i++;
                }
                
            }
            else if (type == AddonType.Resourcepacks)
            {
                foreach (var value in Enum.GetValues(typeof(ResourcePacksCategory)))
                {
                    var addonCategory = new AddonCategory(
                        i,
                        AddonType.Resourcepacks,
                        value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"),
                        value.ToString().Replace("__", string.Empty).Replace("_", string.Empty).Replace("CharAnd", string.Empty)
                        );

                    result.Add(addonCategory);
                    Console.WriteLine(addonCategory.ToString());
                    i++;
                }
            }
            else if (type == AddonType.Maps)
            {
                foreach (var value in Enum.GetValues(typeof(WorldsCategory)))
                {
                    var addonCategory = new AddonCategory(
                        i,
                        AddonType.Maps,
                        value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"),
                        value.ToString().Replace("__", string.Empty).Replace("_", string.Empty).Replace("CharAnd", string.Empty)
                        );

                    result.Add(addonCategory);
                    Console.WriteLine(addonCategory.ToString());
                    i++;
                }
            }

            return result;
        }

        public override string ToString()
        {
            return 
                "AddonCategory:" +
                "\n    Id: " + this.Id + 
                "\n    Name: " + this.Name + 
                "\n    ImageSource: " + this.ImageSource;
        }
    }

    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;

        private readonly BaseInstanceData _baseInstanceData;

        private readonly ObservableCollection<InstanceAddon> _instanceAddons;

        private readonly AddonType _addonsType;

        private int _pageSize = 20;


        #region commands

        /// <summary>
        /// Закрывает CurseforgeMarket
        /// </summary>
        private RelayCommand _closePage;
        public RelayCommand ClosePageCommand
        {
            get => _closePage ?? (new RelayCommand(obj =>
            {
                _mainViewModel.UserProfile.IsShowInfoBar = true;
                InstanceAddon.ClearAddonsListCache();
                MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
            }));
        }

        /// <summary>
        /// Переводит пользователя на официальную страницу curseforge.
        /// </summary>
        public RelayCommand GoToCurseforgeCommand
        {
            get => new RelayCommand(obj =>
            {
                var link = (string)obj;

                try
                {
                    System.Diagnostics.Process.Start(link);
                }
                catch
                {
                    // message box here.
                }

            });
        }

        /// <summary>
        /// Вызывает установку мода.
        /// </summary>
        public RelayCommand InstallModCommand
        {
            get => new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;

                Lexplosion.Run.TaskRun(delegate
                {
                    DownloadAddonRes result = instanceAddon.InstallLatestVersion(out Dictionary<string, DownloadAddonRes> dependenciesResults);
                    App.Current.Dispatcher.Invoke(() => 
                    {
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

                        if (dependenciesResults != null)
                        {
                            foreach (string key in dependenciesResults.Keys)
                            {
                                DownloadAddonRes res = dependenciesResults[key];
                                if (res == DownloadAddonRes.Successful)
                                {
                                    _instanceAddons.Add(instanceAddon);
                                    MainViewModel.ShowToastMessage("Зависимый мод успешно установлен", 
                                        "Название: " + key + ".\nНеобходим для " + instanceAddon.Name);
                                }
                                else
                                {
                                    MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод",
                                        "Название: " + instanceAddon.Name + ".\nОшибка " + result + ".\nНеобходим для " + instanceAddon.Name, Controls.ToastMessageState.Error);
                                }
                            }
                        }
                    });
                });
                
            });
        }

        #endregion commands


        #region props 

        public ObservableCollection<AddonCategory> ModCategories { get; } = new ObservableCollection<AddonCategory>();
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

        private AddonCategory _selectedAddonCategory;
        public AddonCategory SelectedAddonCategory
        {
            get => _selectedAddonCategory; set 
            {
                _selectedAddonCategory = value;
                OnPropertyChanged();
                GetInitializeInstance();
            }
        }

        #endregion props


        public CurseforgeMarketViewModel(ObservableCollection<InstanceAddon> installedAddons, MainViewModel mainViewModel, InstanceClient instanceClient, AddonType addonsType)
        {
            _instanceAddons = installedAddons;
            _mainViewModel = mainViewModel;
            _addonsType = addonsType;
            mainViewModel.UserProfile.IsShowInfoBar = false;

            _baseInstanceData = instanceClient.GetBaseData;

            foreach (var addon in AddonCategory.GetCategories(addonsType))
            {
                ModCategories.Add(addon);
            }

            SelectedAddonCategory = ModCategories[0];

            InstanceAddons = new ObservableCollection<InstanceAddon>();

            SearchBoxVM.SearchChanged += GetInitializeInstance;
            PaginatorVM.PageChanged += GetInitializeInstance;
            GetInitializeInstance();
        }

        public async void GetInitializeInstance()
        {
            await Task.Run(() => InstancesPageLoading());
        }

        private void InstancesPageLoading()
        {
            IsLoaded = false;

            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = InstanceAddon.GetAddonsCatalog(
                    _baseInstanceData, 
                    _pageSize, 
                    PaginatorVM.PageIndex - 1, 
                    _addonsType, 
                    SelectedAddonCategory.Id, 
                    SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == _pageSize)
                {
                    IsPaginatorVisible = true;
                }
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceAddons.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    if (IsEmptyList)
                        IsEmptyList = false;

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceAddons.Clear();
                        Console.WriteLine("Начало загрузки модов");
                        foreach (var instance in instances)
                        {
                            InstanceAddons.Add(instance);
                        }
                        Console.WriteLine("Конец");
                    });
                }

                IsLoaded = true;
            });
        }
    }
}
