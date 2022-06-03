using System;
using System.Collections.Generic;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Newtonsoft.Json;

namespace Lexplosion.Global
{
    static class UserData
    {
        public static string Login = "";
        public static string UUID = "00000000-0000-0000-0000-000000000000";
        public static string AccessToken = "null";
        public static bool IsAuthorized = false;
        public static bool Offline = false;
        public static bool NoUpdate = false;

        public static Settings GeneralSettings { get; private set; } // инициализируется в методе Main

        public static void InitSetting()
        {
            GeneralSettings = Settings.GetDefault();
            GeneralSettings.Merge(DataFilesManager.GetSettings());
        }
    }

    static class LaunсherSettings
    {
        public struct URL
        {
            public const string ModpacksData = "https://night-world.org/minecraft/modpacks/";
            public const string VersionsData = "https://night-world.org/minecraft/versions/";
            public const string JavaData = "https://night-world.org/minecraft/java/";
            public const string Upload = "https://night-world.org/minecraft/upload/";
            public const string LauncherParts = "https://night-world.org/assets/launcher/windows/";
            public const string LogicScripts = "https://night-world.org/api/user/";
            public const string Base = "https://night-world.org/";
        }

        public static string LauncherDataPath = Environment.ExpandEnvironmentVariables("%appdata%") + "/night-world";
        public static string gamePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"\", "/") + "/" + "night-world";
        public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ";
        public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
        public const int version = 1574676433;
    }
}
