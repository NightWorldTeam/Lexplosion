using System;
using System.Collections.Generic;

namespace Lexplosion.Objects
{

    static class UserData
    {
        public static string login = "";
        public static string UUID = "00000000-0000-0000-0000-000000000000";
        public static string accessToken = "null";
        public static bool offline = false;
        public static bool noUpdate = false;
        public static Dictionary<string, string> settings; //общие настройки
        public static Dictionary<string, string> InstancesList; // все профили
        public static Dictionary<string, InstanceAssets> profilesAssets; // TODO: переименовать эту переменную
    }

    public class InstanceAssets //Асесты модпака на главной странице (описание, картинки)
    {
        public string description;
        public List<string> images;
        public string xmx;
        public string xms;
    }

    class InstanceLocalFiles
    {
        public LocalVersionInfo version;
    }

    class InstanceFiles
    {
        public Dictionary<string, WithFolder> data = new Dictionary<string, WithFolder>();
        public VersionInfo version;
        public Dictionary<string, string> natives;
        public List<string> libraries;
    }

    class LocalVersionInfo
    {
        public FileInfo minecraftJar;
        public string arguments;
        public string gameVersion;
        public string assetsVersion;
        public string assetsIndexes;
        public string mainClass;

    }

    class VersionInfo: LocalVersionInfo
    {
        public bool security;
        public string librariesUrl;
        public int librariesLastUpdate;
        public string nativesUrl;
        public int nativesLastUpdate;
    }

    class FileInfo
    {
        public string name;
        public string url;
        public string sha1;
        public int size;
        public int lastUpdate;
    }

    class WithFolder
    {
        public Dictionary<string, FileInfo> objects;
        public List<string> oldFiles;
        public bool security;
        public int folderVersion;

    }

    class InitData
    {
        public InstanceFiles files;
        public List<string> errors;
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
