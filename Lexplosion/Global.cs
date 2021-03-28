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
        public static bool offline = false;
        public static bool noUpdate = false;
        public static Dictionary<string, string> settings; //общие настройки
        public static Dictionary<string, string> InstancesList; // все игровые профили
        public static Dictionary<string, InstanceAssets> instancesAssets;
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
