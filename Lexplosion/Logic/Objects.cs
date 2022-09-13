using System.Collections.Generic;
using System.IO;
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

        public bool IsExists(string instancePath)
        {
            if (Type == AddonType.Mods || Type == AddonType.Resourcepacks)
            {
                //try
                {
                    return File.Exists(instancePath + ActualPath);
                }
                //catch
                //{
                //    return false;
                //}
            }
            else if (Type == AddonType.Maps)
            {
                try
                {
                    return Directory.Exists(instancePath + ActualPath);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public void RemoveFromDir(string instancePath)
        {
            if (Type == AddonType.Mods || Type == AddonType.Resourcepacks)
            {
                try
                {
                    File.Delete(instancePath + ActualPath);
                }
                catch { }
            }
            else if (Type == AddonType.Maps)
            {
                try
                {
                    Directory.Delete(instancePath + ActualPath, true);
                } catch{ }
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
        public InstanceSource Source { get; set; }
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

    public class AuthResult 
    {
        public AuthCode Status;
        public string Login;
        public string UUID;
        public string AccesToken;
        public string SessionToken;
    }

    public class AcccountsFormat
    {
        public class Profile
        {
            public string Login;
            public string Password;
        }

        public AccountType SelectedProfile;
        public Dictionary<AccountType, Profile> Profiles;
    }

    public class InstanceVersion
    {
        public string FileName { get; set; }
        public string Id;
        public string Date { get; set; }
        public ReleaseType Status { get; set; }
    }

}
