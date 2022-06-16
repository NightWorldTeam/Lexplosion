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

    public class Mod 
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public double DownloadCount { get; set; }
        public string LastUpdated { get; set; }
        public string CreatedTime { get; set; }
    }

    public class CurseforgeMarketViewModel : VMBase
    {
        private string[] ModCategoryNames = new string[19] 
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
        public ObservableCollection<Mod> Mods { get; } = new ObservableCollection<Mod>();

        #endregion props

        public CurseforgeMarketViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            mainViewModel.IsShowInfoBar = false;
            foreach (var name in ModCategoryNames) 
            {
                ModCategories.Add(new ModCategory 
                {
                    Name = name
                });
            }

            for (var i = 0; i < 10; i++) 
            {
                if (i % 2 == 0)
                {
                    Mods.Add(new Mod
                    {
                        Name = "Just Enough Items (JEI)",
                        Author = "mezz",
                        DownloadCount = 167.7,
                        LastUpdated = "Updated 3 days ago",
                        CreatedTime = "Created Nov 24, 2015",
                        Description = "View Items and Recipes"
                    });
                }
                else 
                {
                    Mods.Add(new Mod
                    {
                        Name = "MrCrayfish's Furniture Mod",
                        Author = "MrCrayfish",
                        DownloadCount = 43.1,
                        LastUpdated = "Updated 23 hours ago",
                        CreatedTime = "Created Apr 7, 2013",
                        Description = "Adds over 80 unique pieces of furniture into Minecraft!"
                    });
                }
            }
        }
    }
}
