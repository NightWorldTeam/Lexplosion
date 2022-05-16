using System.Collections.Generic;
using System.Windows.Media.Imaging;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects // TODO: позаменять классы на структуры
{
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

    public class Category
    {
        public int categoryId;
        public string name { get; set; }
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
