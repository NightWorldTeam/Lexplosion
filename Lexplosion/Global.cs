using System;
using System.Collections.Generic;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Newtonsoft.Json;
using Lexplosion.Logic.Management;

namespace Lexplosion.Global
{
    static class UserData
    {
        private static User _user;
        public static bool IsAuth { get; private set; } = false;

        public static AuthCode Auth(string login, string password, bool saveUser)
        {
            _user = new User();
            IsAuth = true;
            LaunchGame.GameStartEvent += _user.GameStart;
            LaunchGame.GameStopEvent += _user.GameStop;
            Lexplosion.Run.ExitEvent += _user.Exit;

            return _user.Auth(login, password, saveUser);
        }

        public static string Login
        {
            get =>_user.Login;
        }
        public static string UUID
        {
            get => _user.UUID;
        }
        public static string AccessToken
        {
            get => _user.AccessToken;
        }
        public static string SessionToken
        {
            get => _user.SessionToken;
        }
        public static AccountType AccountType
        {
            get => _user.AccountType;
        }
        public static ActivityStatus Status
        {
            get => _user.Status;
        }

        public static readonly bool Offline = false;

        public static void ChangeBaseStatus(ActivityStatus status)
        {

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
