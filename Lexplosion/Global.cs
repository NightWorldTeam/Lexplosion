using System;
using System.Collections.Generic;
using Lexplosion.Gui.Windows;
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
            public static Dictionary<string, InstanceParametrs> Record; // все сборки
            public static Dictionary<string, InstanceAssets> Assets; // ассетсы всех модпаков
            public static Dictionary<string, string> ExternalIds; // список внешних айдишников модпаков (ключ - внешний id, значение - внутренний)

            public delegate void InstaceAddHander();
            public delegate void InstaceSetAssetsHandler(string id, InstanceAssets assets);
            public static event InstaceAddHander AddInstanceNofity;
            public static event InstaceSetAssetsHandler SetAssetsNofity;

            public static void AddInstance(string localId, InstanceParametrs parametrs, InstanceAssets assets, string externalId = "")
            {
                Record[localId] = parametrs;
                Assets[localId] = assets;

                if(externalId != "" && externalId != null)
                {
                    ExternalIds[externalId] = localId;
                }

                AddInstanceNofity?.Invoke();
            }

            public static void SetAssets(string id, InstanceAssets assets)
            {
                Assets[id] = assets;
                SetAssetsNofity?.Invoke(id, assets);
            }

            public static bool IsExistId(string id) => ExternalIds.ContainsKey(id);
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
            public const string ModpacksData = "https://night-world.org/minecraft/modpacks/";
            public const string VersionsData = "https://night-world.org/minecraft/versions/";
            public const string Upload = "https://night-world.org/minecraft/upload/";
            public const string LauncherParts = "https://night-world.org/assets/launcher/windows/";
            public const string LogicScripts = "https://night-world.org/requestProcessing/";
            public const string DataScripts = "https://night-world.org/launcherApi/";
        }

        public static string gamePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"\", "/") + "/" + "night-world";
        public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ";
        public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
        public const int version = 1574676433;
    }
}
