using System;
using System.Collections.Generic;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Global
{
    static class UserData
    {
        public static string login = "";
        public static string UUID = "00000000-0000-0000-0000-000000000000";
        public static string PaswordSHA = "";
        public static string accessToken = "null";
        public static bool isAuthorized = false;
        public static bool offline = false;
        public static bool noUpdate = false;
        /*
        javaPath, gamePath, 
        xmx, xms, 
        windowWidth, windowHeight, 
        noUpdate, showConsole, 
        hiddenMode, gameArgs, 
        selectedModpack, launcherAssetsV, 
        login, password
        */
        public static Dictionary<string, string> settings; //общие настройки

        public static class Instances
        {
            public static Dictionary<string, InstanceParametrs> List; // все сборки
            public static Dictionary<string, InstanceAssets> Assets; // ассетсы всех модпаков
            public static Dictionary<string, string> ExternalIds; // список внешних айдишников модпаков (ключ - внешний id, значение - внутренний)
            public delegate void InstaceAddHander();
            public static event InstaceAddHander Nofity;

            public static void AddInstance(string localId, InstanceParametrs parametrs, InstanceAssets assets, string externalId = "")
            {
                List[localId] = parametrs;
                Assets[localId] = assets;

                if(externalId != "" && externalId != null)
                {
                    ExternalIds[externalId] = localId;
                }
                Nofity?.Invoke();
            }

            public static void SetAssets(string id, InstanceAssets assets)
            {
                Assets[id] = assets;
            }
        }
    }

    class InstanceParametrs
    {
        public string Name;
        public InstanceSource Type;
    }

    static class LaunсherSettings
    {
        public struct URL
        {
            public const string ModpacksData = "http://nw-prod/minecraft/modpacks/";
            public const string VersionsData = "http://nw-prod/minecraft/versions/";
            public const string Upload = "http://nw-prod/minecraft/upload/";
            public const string LauncherParts = "https://night-world.org/assets/launcher/windows/";
            public const string LogicScripts = "http://nw-prod/requestProcessing/";
            public const string DataScripts = "http://nw-prod/launcherApi/";
        }

        public static string gamePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"\", "/") + "/" + "night-world";
        public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ";
        public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
        public const int version = 1574676433;
    }
}
