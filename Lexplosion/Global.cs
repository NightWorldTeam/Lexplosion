using System;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;

namespace Lexplosion.Global
{
    static class GlobalData
    {
        private static User _user;

        public static User User
        {
            get => _user;
        }

        public static void SetUser(User user)
        {
            _user = user;
        }

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
            public const string InstallersData = "https://night-world.org/minecraft/additionalInstallers/";
            public const string JavaData = "https://night-world.org/minecraft/java/";
            public const string Upload = "https://night-world.org/minecraft/upload/";
            public const string LauncherParts = "https://night-world.org/assets/launcher/windows/";
            public const string LogicScripts = "https://night-world.org/api/user/";
            public const string Base = "https://night-world.org/";
            public const string Account = "https://night-world.org/api/account/";
        }

        public static string LauncherDataPath = Environment.ExpandEnvironmentVariables("%appdata%") + "/lexplosion-data";
        public static string gamePath = Environment.ExpandEnvironmentVariables("%appdata%") + "/" + ".lexplosion";
        public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ";
        public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
        public const int version = 1668546887;
        public const int CommandServerPort = 54352;
    }
}
