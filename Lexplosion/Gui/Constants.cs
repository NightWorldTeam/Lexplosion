using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui
{
    public class Constants
    {
        public static string[] ScreenResolutions { get; } = new string[20]
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024",
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
        };

        public static readonly string[] ModCategoryNames = new string[19]
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

        public static string[] ModpacksCategoryNames { get; } = new string[16]
        {
            "Tech",
            "Magic",
            "Sci-Fi",
            "Adventure and RPG",
            "Exploration",
            "Mini Game",
            "Quests",
            "Hardcore",
            "Map Based",
            "Small/Ligth",
            "Extra/Large",
            "Combat / PVP",
            "Multiplayer",
            "FTB Offical Pack",
            "Skyblock",
            "Vanilla+"
        };

        public static readonly Dictionary<string, double> TagSizes = new Dictionary<string, double>()
        {
            { "Tech", 36.5333333333333 },
            { "Magic", 46.9233333333333},
            { "Sci-Fi", 49.88},
            { "Adventure and RPG", 132.433333333333},
            { "Exploration", 80.4466666666667},
            { "Mini Game", 77.18},
            { "Quests", 51.3233333333333},
            { "Hardcore", 66.5366666666667},
            { "Map Based", 78.2066666666667},
            { "Small / Light", 88.28},
            { "Extra Large", 78.74},
            { "Combat / PvP", 95.0533333333333},
            { "Multiplayer", 80.5133333333333},
            { "FTB Official Pack", 113.16},
            { "Skyblock", 64.4766666666667},
            { "Vanilla+", 49.71}
        };
    }
}
