using System.Collections.Generic;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects
{
    class InstanceAssets //Асесты модпака на главной странице (описание, картинки)
    {
        public string description;
        public string author;
        public List<string> images;
        public string mainImage;
        public string xmx;
        public string xms;
    }

    public class LibInfo
    {
        public bool notArchived;
        public string url;
    }

    class VersionManifest
    {
        public VersionInfo version;
        public Dictionary<string, string> natives;
        public Dictionary<string, LibInfo> libraries;
    }

    class NInstanceManifest : VersionManifest
    {
        public class WithFolder
        {
            public Dictionary<string, FileInfo> objects;
            public List<string> oldFiles;
            public bool security;
            public int folderVersion;
        }

        public Dictionary<string, NInstanceManifest.WithFolder> data = new Dictionary<string, NInstanceManifest.WithFolder>();
    }

    class InstancePlatformData
    {
        public string id;
        public int instanceVersion;
    }

    class LocalVersionInfo
    {
        public Dictionary<string, string> minecraftJar;
        public string arguments;
        public string gameVersion;
        public string assetsVersion;
        public string assetsIndexes;
        public string mainClass;
        public string forgeVersion;
    }

    class VersionInfo : LocalVersionInfo
    {
        public new FileInfo minecraftJar;
        public bool security;
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
        public bool notArchived;
    }

    class InitData
    {
        public InstanceInit InitResult;
        public List<string> DownloadErrors;
        public VersionInfo VersionFile;
        public Dictionary<string, LibInfo> Libraries;
    }

    /*public class Category
    {
        public int categoryId;
        public string name;
    }*/

    public class CurseforgeInstanceInfo
    {
        public class Category
        {
            public int categoryId;
            public string name;
        }

        public class Author
        {
            public string name;
            public string url;
        }

        public class Attachment
        {
            public int id;
            public bool isDefault;
            public string thumbnailUrl;
        }

        public int id;
        public string name;
        public List<Attachment> attachments;
        public List<Category> categories;
        public List<Author> authors;
        public string summary;
        public float downloadCount;
    }

    class OutsideInstance
    {
        public InstanceType Type;
        public bool UpdateAvailable;
        public bool IsInstalled;
        public string Name;
        public string Author;
        public string MainImageUrl;
        public List<string> Categories;
        public float DownloadCount;
        public string Description;
        public string Id;
    }

    class ModInfo
    {
        public int id;
        public string fileName;
        public string downloadUrl;
        public string displayName;
    }

    //Манифест модпака курсфорджа InstanceManifest
    class InstanceManifest
    {
        public class McVersionInfo
        {
            public string version;
            public List<ModLoaders> modLoaders;
        }

        public class ModLoaders
        {
            public string id;
            public bool primary;
        }

        public class FileData
        {
            public int projectID;
            public int fileID;

        }

        public McVersionInfo minecraft;
        public string name;
        public string version;
        public string author;
        public List<FileData> files;
    }

    class CurseforgeFileInfo
    {
        public int id;
        public string downloadUrl;
        public string fileName;
    }

    class NWInstanceInfo
    {
        public string name;
        public string mainImage;
        public string author;
        public int version;
    }

    public class DataLibInfo : LibInfo
    {
        public string os;
    }

}
