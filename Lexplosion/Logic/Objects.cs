using System.Collections.Generic;
using System.Windows.Media.Imaging;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects // TODO: позаменять классы на структуры
{
    /// <summary>
    /// Структура файла, в котором хранятся установленные аддоны (installedAddons.json)
    /// </summary>
    class InstalledAddons : Dictionary<int, InstalledAddonInfo> { }

    /// <summary>
    /// Структура файла lastUpdates.json
    ///<summary>
    public class LastUpdates : Dictionary<string, long> { }

    public class InstanceProperties : OutsideInstance
    {
        public BitmapImage Logo { get; set; }
        public bool IsInstanceAddedToLibrary;
        public bool IsDownloadingInstance;
    }

    /// <summary>
    /// Асесты модпака на главной странице (описание, картинки)
    /// </summary>
    public class InstanceAssets
    {
        public string description { get; set; }
        public string author { get; set; }
        public List<string> images;
        public string mainImage;
        public string xmx;
        public string xms;
        public List<Category> categories { get; set; }
    }

    /// <summary>
    /// Информация о майкрафтовской либе
    /// </summary>
    public class LibInfo
    {
        public bool notArchived;
        public string url;
        public List<List<string>> obtainingMethod;
        public bool isNative;
    }

    class LocalNightWorldManifest
    {
        public class WithFolder
        {
            public Dictionary<string, FileInfo> objects;
            public List<string> oldFiles;
            public bool security;
            public long folderVersion;
        }

        public Dictionary<string, WithFolder> data = new Dictionary<string, WithFolder>();
        public bool CustomVersion;
    }

    class NightWorldManifest : LocalNightWorldManifest
    {
        public class Version
        {
            public string modloaderVersion;
            public ModloaderType modloaderType;
            public string gameVersion;
            public bool security;
        }

        public Version version;
    }

    public class VersionManifest
    {
        public VersionInfo version;
        public Dictionary<string, LibInfo> libraries;
    }

    public class VersionInfo : LocalVersionInfo
    {
        public new FileInfo minecraftJar;
        public string CustomVersionName;
        public bool security;
        public long librariesLastUpdate;

        /// <summary>
        /// Эта функция возвращает имя для файла либрариесов (файлы .lver, что хранят версию либрариесов и файлы .json, которые хранят список либрариесов для конкретной версии игры).
        /// У каждой версии игры своё имя для файлов с информацией о либрариесах
        /// </summary>
        public string GetLibName
        {
            get
            {
                if (CustomVersionName != null)
                    return CustomVersionName;

                string endName = "";
                if (modloaderType == ModloaderType.Fabric)
                {
                    endName = "-Fabric-" + modloaderVersion;
                }
                else if (modloaderType == ModloaderType.Forge)
                {
                    endName = "-Forge-" + modloaderVersion;
                }

                return gameVersion + endName;
            }         
        }
    }

    /// <summary>
    /// Локальный манифест модпака, который хранится в файле manifest.json
    /// </summary>
    public class LocalVersionInfo
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

    public class FileInfo
    {
        public string name;
        public string url;
        public string sha1;
        public long size;
        public long lastUpdate;
        public bool notArchived;
    }

    class InstancePlatformData
    {
        public string id;
        public int instanceVersion;
    }

    /// <summary>
    /// Нужен для передачи данных между методами при запуске игры.
    /// </summary>
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
        public string name { get; set; }
    }

    /// <summary>
    /// Описывает проект курсфорджа. Дочерние классы используются при декодировании Json
    /// </summary>
    public abstract class CurseforgeProjectInfo
    {
        public class GameVersion
        {
            public string gameVersion;
            public int projectFileId;
        }

        public class LatestFile
        {
            public long id;
            public List<string> gameVersion;
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
            public string url;
        }

        public int id;
        public string name;
        public List<LatestFile> latestFiles;
        public string summary;
        public float downloadCount { get; set; }
        public string dateModified;
        public string websiteUrl;
        public List<Attachment> attachments;
        public List<Category> categories;
        public List<Author> authors;
    }

    /// <summary>
    /// Описывает модпак с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeInstanceInfo : CurseforgeProjectInfo
    {
        public List<GameVersion> gameVersionLatestFiles;
        public ModloaderType Modloader;
    }

    /// <summary>
    /// Описывает мод с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeModInfo : CurseforgeProjectInfo
    {
        public class GameVersionMod : GameVersion
        {
            public int modLoader;
        }

        public List<GameVersionMod> gameVersionLatestFiles;
        public ModloaderType Modloader;
    }

    public class InstanceParametrs
    {
        public string Name { get; set; }
        public InstanceSource Type;
        public bool UpdateAvailable;
        public bool IsInstalled { get; set; } = true;
    }

    public class OutsideInstance : InstanceParametrs
    {
        public string Id; // id от Type
        public string LocalId; // айди созданой в лаунчере сборки
        public InstanceAssets InstanceAssets { get; set; }
        public byte[] MainImage;
        public List<Category> Categories { get; set; }
        public float DownloadCount { get; set; }
        public string GameVersion { get; set; }
    }

    class CurseforgeFileInfo
    {
        public int id;
        public string downloadUrl;
        public string fileName;
    }

    public class DataLibInfo : LibInfo
    {
        public string os;
    }

    public struct MCVersionInfo
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    /// <summary>
    /// Этот класс хранят инфу об установленном с курсфорджа аддоне
    /// </summary>
    public class InstalledAddonInfo
    {
        public int ProjectID;
        public int FileID;
        public AddonType Type;
        public string Path;
        public bool IsDisable = false;

        public string ActualPath
        {
            get
            {
                if (IsDisable && Path != null)
                    return Path + ".disable";
                return Path;
            }
        }
    }

    /// <summary>
    /// Этот класс хранят инфу о версии джавы.
    /// </summary>
    public class JavaVersion
    {
        public string LastGameVersion;
        public string JavaName;
        public long LastUpdate;
        public string ExecutableFile = "/bin/javaw.exe";
    }

    public class InstanceData
    {
        public string GameVersion { get; set; }
        public string LastUpdate { get; set; }
        public long TotalDownloads { get; set; }
        public ModloaderType Modloader { get; set; }
        public string Description { get; set; }
        public List<Category> Categories { get; set; }
        public List<byte[]> Images { get; set; }
        public string WebsiteUrl { get; set; }
        public string Summary { get; set; }
    }
}
