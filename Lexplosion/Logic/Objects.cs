using System.Collections.Generic;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects 
{

    public class InstanceAssets //Асесты модпака на главной странице (описание, картинки)
    {
        public string description;
        public List<string> images;
        public string mainImage;
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
        public Dictionary<string, string> minecraftJar;
        public string arguments;
        public string gameVersion;
        public string assetsVersion;
        public string assetsIndexes;
        public string mainClass;

    }

    class VersionInfo: LocalVersionInfo
    {
        public new FileInfo minecraftJar;
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

}
