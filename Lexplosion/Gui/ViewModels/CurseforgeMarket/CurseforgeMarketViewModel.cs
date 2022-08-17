using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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


        public AddonCategory(int id, AddonType type, string name, string iconSource, List<AddonCategory> subcategory = null)
        {
            Id = id;
            Name = name;
            ImageSource = String.Format("pack://Application:,,,/assets/images/icons/curseforge/{0}/{1}.png", type.ToString().ToLower(), iconSource.ToLower());

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
            return String.Format("AddonCategory:\n    Id: {0}\n   Name: {1}\n    ImageSource: {2}", this.Id, this.Name, this.ImageSource);
        }
    }

    public class CurseforgeMarketViewModel : VMBase
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
                            MainViewModel.ShowToastMessage("Мод успешно установлен. Не за что.", "Название: " + instanceAddon.Name);
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

        #endregion props

        public CurseforgeMarketViewModel(ObservableCollection<InstanceAddon> installedAddons, MainViewModel mainViewModel, InstanceClient instanceClient, AddonType addonsType)
        {
            _instanceAddons = installedAddons;
            _mainViewModel = mainViewModel;
            _addonsType = addonsType;
            mainViewModel.UserProfile.IsShowInfoBar = false;

            _baseInstanceData = instanceClient.GetBaseData;

            InstanceAddons = new ObservableCollection<InstanceAddon>();

            foreach (var addon in AddonCategory.GetCategories(addonsType))
            {
                ModCategories.Add(addon);
            }

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
            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = InstanceAddon.GetAddonsCatalog(_baseInstanceData, _pageSize, PaginatorVM.PageIndex - 1, _addonsType, -1, SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == 0)
                {

                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceAddons.Clear();
                        foreach (var instance in instances)
                        {
                            InstanceAddons.Add(instance);
                        }
                    });

                }

                IsLoaded = true;
            });
        }
    }
}
