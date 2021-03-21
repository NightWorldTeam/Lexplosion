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
        public static Dictionary<string, string> PacksList;
        public static Dictionary<string, ModpackAssets> profilesAssets;
    }

    class LauncherAssets
    {
        public int version = 0;
        public Dictionary<string, ModpackAssets> data;

    }

    public class ModpackAssets //Асесты модпака на главной странице (описание, картинки)
    {
        public string description;
        public List<string> images;
        public string xmx;
        public string xms;
    }

    class ModpackFiles
    {
        public Dictionary<string, WithFolder> data;
        public VersionInfo version;
        public Dictionary<string, string> libraries;
        public Dictionary<string, string> natives;
    }

    class FilesList : ModpackFiles
    {
        public string code;
        public string str;
    }

    class VersionInfo
    {
        public FileInfo minecraftJar;
        public string arguments;
        public string gameVersion;
        public string assetsVersion;
        public string assetsIndexes;
        public bool security;
        public string mainClass;
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

    static class Updates
    {
        static public Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
        static public List<string> natives = new List<string>();
        static public List<string> libraries = new List<string>();
        static public bool minecraftJar = false;
        static public bool assetsObjects = false;
        static public bool assetsIndexes = false;
        static public bool assetsVirtual = false;
        static public List<string> oldFiles = new List<string>();
    }

    class InitData
    {
        public ModpackFiles files;
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
