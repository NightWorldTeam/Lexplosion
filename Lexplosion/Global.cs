using System;
using System.Collections.Generic;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Global
{
    static class UserData
    {
        public static string login = "";
        public static string UUID = "00000000-0000-0000-0000-000000000000";
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
        public static Dictionary<string, InstanceParametrs> InstancesList; // все сборки
        public static Dictionary<string, InstanceAssets> instancesAssets; // ассетсы всех модпаков
        public static Dictionary<string, string> ExternalIds; // список внешних айдишников модпаков (ключ - внешний id, значение - внутренний)
    }

    class InstanceParametrs
    {
        public string Name;
        public InstanceType Type;
    }

    static class LaunсherSettings
    {
        public static string gamePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"\", "/") + "/" + "night-world";
        public const string serverUrl = "https://night-world.org/rest/launcher/";
        public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ";
        public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
        public const int version = 1574676433;
    }
}
