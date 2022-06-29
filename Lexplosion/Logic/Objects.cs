using System.Collections.Generic;
using System.Windows.Media.Imaging;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects
{
    /// <summary>
    /// Асесты модпака на главной странице (описание, картинки)
    /// </summary>
    public class InstanceAssets
    {
        public string Description;
        public string Author;
        public List<string> Images;
        public string Xmx;
        public string Xms;
        public List<Category> Categories;
        public string Summary;
    }

    public class Category
    {
        public int categoryId;
        public string name { get; set; }
    }

    /// <summary>
    /// Этот класс хранят инфу об установленном с курсфорджа аддоне
    /// </summary>
    public class InstalledAddonInfo
    {
        public long ProjectID;
        public long FileID;
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
        public string Changelog { get; set; }
    }
}
