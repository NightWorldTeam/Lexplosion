using System.Collections.Generic;

namespace Lexplosion.Common
{
    public class Constants
    {
        public const string VKDefaultUrl = @"https://vk.com/";
        public const string VKGroupUrl = VKDefaultUrl + "nightworld_offical";
        public const string VKGroupToChatUrl = VKDefaultUrl + "im?media=&sel=-155979422";

        public const string DiscordDefaultUrl = @"https://dicord.com/";
        public const string MCRUDiscordUrl = "https://discord.gg/eW24EnkDB7";
        //public const string DiscordServerInviteUrl = "https://discord.gg/FfSWhMWxsj";

        public const string BoostyUrl = "";
        public const string NightWorldOfficalWebsiteUrl = "https://night-world.org/";

        public static string[] ScreenResolutions { get; set; } = new string[20]
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024",
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
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
