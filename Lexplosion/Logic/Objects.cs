using System.Collections.Generic;
using System.Windows.Media.Imaging;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects // TODO: позаменять классы на структуры
{
    public class InstanceProperties : OutsideInstance
    {
        public BitmapImage Logo;
        public List<string> InstanceTags;
        public bool IsInstanceAddedToLibrary;
        public bool IsDownloadingInstance;
    }

    public class InstanceAssets //Асесты модпака на главной странице (описание, картинки)
    {
        public string description;
        public string author;
        public List<string> images;
        public string mainImage;
        public string xmx;
        public string xms;
        public List<Category> categories;
    }

    public class LibInfo
    {
        public bool notArchived;
        public string url;
        public List<List<string>> obtainingMethod;
        public bool isNative;
    }

    class VersionManifest
    {
        public VersionInfo version;
        //public Dictionary<string, string> natives;
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
        public string modloaderVersion;
        public ModloaderType modloaderType;
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

    public class Category
    {
        public int categoryId;
        public string name;
    }

    public class CurseforgeInstanceInfo
    {
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
            public string url;
        }

        public class GameVersion
        {
            public string gameVersion;
        }

        public class LatestFile
        {
            public long id;
            public List<string> gameVersion;
        }

        public int id;
        public string name;
        public List<Attachment> attachments;
        public List<Category> categories;
        public List<Author> authors;
        public List<GameVersion> gameVersionLatestFiles;
        public string summary;
        public float downloadCount;
        public string dateModified;
        public string websiteUrl;
        public List<LatestFile> LatestFiles;

        public ModloaderType Modloader;
    }

    public class InstanceParametrs
    {
        public string Name;
        public InstanceSource Type;
        public bool UpdateAvailable;
    }

    public class OutsideInstance : InstanceParametrs
    {
        public string Id; // id от Type
        public string LocalId; // айди созданой в лаунчере сборки
        public InstanceAssets InstanceAssets;
        public byte[] MainImage;
        public bool IsInstalled;
        public List<string> Categories;
        public float DownloadCount;
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
        public string description;
        public List<string> categories;
    }

    public class DataLibInfo : LibInfo
    {
        public string os;
    }

    public struct MCVersionInfo
    {
        public string type;
        public string id;
    }
}
