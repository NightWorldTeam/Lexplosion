using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects
{
    public abstract class InstanceAssetsBase
    {
        public string Description { get; set; }
        public string Author { get; set; }
        public List<string> Images { get; set; }
        public string Xmx { get; set; }
        public string Xms { get; set; }
        public string Summary { get; set; }
    }

    /// <summary>
    /// Асесты модпака на главной странице (описание, картинки)
    /// </summary>
    public class InstanceAssets : InstanceAssetsBase
    {
        public IEnumerable<CategoryBase> Categories { get; set; }
    }

    public class InstanceAssetsFileDecodeFormat : InstanceAssetsBase
    {
        public List<SimpleCategory> Categories { get; set; }
    }

    /// <summary>
    /// Этот класс хранят инфу об установленном с курсфорджа аддоне
    /// </summary>
    public class InstalledAddonInfo
    {
        public string ProjectID;
        public string FileID;
        public AddonType Type;
        public ProjectSource Source;
        public string Path;
        public bool IsDisable = false;

        [JsonIgnore]
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
                try
                {
                    return File.Exists(instancePath + ActualPath);
                }
                catch
                {
                    return false;
                }
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
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Этот класс хранят инфу о версии джавы.
    /// </summary>
    public class JavaVersion
    {
        public long LastReleaseIndex;
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
        public ClientType Modloader { get; set; }
        public string Description { get; set; }
        public IEnumerable<IProjectCategory> Categories { get; set; }
        public List<byte[]> Images { get; set; }
        public string WebsiteUrl { get; set; }
        public string Summary { get; set; }
        public string Changelog { get; set; }
    }

    public class AcccountsFormat
    {
        public class Profile
        {
            public string Login;
            public string AccessData;
        }

        public AccountType SelectedProfile;
        public Dictionary<AccountType, Profile> Profiles;
    }

    public class InstanceVersion
    {
        public string FileName { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }
        public ReleaseType Status { get; set; }
        public bool CanInstall { get; set; } = true;
    }

    /// <summary>
    /// Структура файла, в котором хранятся установленные аддоны (installedAddons.json)
    /// Ключ - курсфордж id.
    /// </summary>
    public class InstalledAddonsFormat : Dictionary<string, InstalledAddonInfo> { }

    public class DistributionData
    {
        public string Name;
        public string PublicRsaKey;
        public string ConfirmWord;
    }

    public interface IProjectCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Id типа аддона.
        /// </summary>
        public string ClassId { get; set; }//TODO: на нулл проверку намутить
        /// <summary>
        /// Id родительской категории, 
        /// Если не содержит родительскую категорию, содержит classId
        /// </summary>
        public string ParentCategoryId { get; set; } //TODO: на нулл проверку намутить
    }

    public abstract class CategoryBase : IProjectCategory
    {
        public abstract string Id { get; set; }
        public abstract string Name { get; set; }
        public abstract string ClassId { get; set; }
        public abstract string ParentCategoryId { get; set; }
    }

    public class SimpleCategory : CategoryBase
    {
        public override string Id
        {
            //get
            //{
            //    return Name;
            //}
            //set { }
            get; set;
        }
        public override string Name { get; set; }
        public override string ClassId { get; set; }
        public override string ParentCategoryId { get; set; }

        public SimpleCategory() { }

        public SimpleCategory(CategoryBase category)
        {
            Id = category.Id;
            Name = category.Name;
            ClassId = category.ClassId;
            ParentCategoryId = category.ParentCategoryId;
        }
    }

}
