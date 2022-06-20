using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceCreationViewModel : VMBase
    {
        public string[] ModCategoryNames { get; } = new string[19]
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
    }
}
