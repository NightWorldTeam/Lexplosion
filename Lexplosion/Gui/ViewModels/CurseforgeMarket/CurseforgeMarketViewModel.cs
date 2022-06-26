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

            Mods = new ObservableCollection<InstanceAddon>(InstanceAddon.GetAddonsCatalog(instanceClient.GetBaseData, 10, 0, AddonType.Mods));
        }
    }
}
