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
        public string Name { get; }
        public string ImageSource { get; }

        public AddonCategory(int id, string name)
        {
            Id = id;
            Name = name;
            ImageSource = String.Format("pack://Application:,,,/assets/images/icons/curseforge/{0}.png", Name.ToLower());
        }

        public static List<AddonCategory> GetCategories(AddonType type)
        {
            var result = new List<AddonCategory>();
            var i = -1;

            if (type == AddonType.Mods)
            {
                foreach (var value in Enum.GetValues(typeof(ModCategory)))
                {
                    Console.WriteLine(value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"));
                    result.Add(new AddonCategory(i, value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&")));
                    i++;
                }
                
            }
            else if (type == AddonType.Resourcepacks)
            {
                foreach (var value in Enum.GetValues(typeof(ResourcePacksCategory)))
                {
                    Console.WriteLine(value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"));
                }
            }
            else if (type == AddonType.Maps)
            {
                foreach (var value in Enum.GetValues(typeof(WorldsCategory)))
                {
                    Console.WriteLine(value.ToString().Replace("__", ", ").Replace('_', ' ').Replace("CharAnd", "&"));
                }
            }

            return result;
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
                    instanceAddon.InstallLatestVersion();
                    App.Current.Dispatcher.Invoke(() => 
                    { 
                        _instanceAddons.Add(instanceAddon);
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

            foreach (var addon in AddonCategory.GetCategories(AddonType.Mods))
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
