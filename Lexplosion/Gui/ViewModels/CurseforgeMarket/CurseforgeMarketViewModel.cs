using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public class ModCategory
    {
        public string Name { get; set; }
        public string ImageSource { get; } = "pack://Application:,,,/assets/images/icons/curseforge/worldgen.png";
    }

    public class CurseforgeMarketViewModel : VMBase
    {
        private readonly string[] ModCategoryNames = new string[19]
        {
            "All Mods",
            "WorldGen",
            "Technology",
            "Magic",
            "Storage",
            "API and Library",
            "Adventure and RPG",
            "Map and Information",
            "Cosmetics",
            "Miscellaneous",
            "Addons",
            "Armor, Tools, and Weapon",
            "Server Utility",
            "Food",
            "Redstone",
            "Twitch Integration",
            "MCreator",
            "Utility & QoL",
            "Education"
        };

        private readonly MainViewModel _mainViewModel;

        private readonly BaseInstanceData _baseInstanceData;

        private int _pageSize = 10;

        #region commands

        private RelayCommand _closePage;

        public RelayCommand ClosePageCommand
        {
            get => _closePage ?? (new RelayCommand(obj =>
            {
                _mainViewModel.IsShowInfoBar = true;
                MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
            }));
        }

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

        public RelayCommand InstallModCommand
        {
            get => new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;

                instanceAddon.InstallLatestVersion();
            });
        }

        #endregion commands

        #region props 

        public ObservableCollection<ModCategory> ModCategories { get; } = new ObservableCollection<ModCategory>();
        public ObservableCollection<InstanceAddon> Mods { get; }

        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel();
        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();

        #endregion props

        public CurseforgeMarketViewModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            _mainViewModel = mainViewModel;
            mainViewModel.IsShowInfoBar = false;

            _baseInstanceData = instanceClient.GetBaseData;

            Mods = new ObservableCollection<InstanceAddon>();

            foreach (var name in ModCategoryNames)
            {
                ModCategories.Add(new ModCategory
                {
                    Name = name
                });
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
                var instances = InstanceAddon.GetAddonsCatalog(_baseInstanceData, _pageSize, PaginatorVM.PageIndex, AddonType.Mods, SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == 0)
                {

                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Mods.Clear();
                        foreach (var instance in instances)
                        {
                            Mods.Add(instance);
                        }
                    });

                }
            });
        }
    }
}
