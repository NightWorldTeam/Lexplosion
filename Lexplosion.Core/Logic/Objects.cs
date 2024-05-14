using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Lexplosion.Logic.Management;
using Newtonsoft.Json;

//тут хранятся всякие лайтовые классы, в основном нужные для передачи данных и для декодирования JSON
namespace Lexplosion.Logic.Objects
{
    public class InstalledInstance
    {
        public string Name;
        public bool IsInstalled;
        public InstanceSource Type;
    }

    /// <summary>
    /// Структура файла с установленными модпаками (instanesList.json)
    /// </summary>
    public class InstalledInstancesFormat : Dictionary<string, InstalledInstance> { }

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

    public class InstanceInfo
    {
        public string Name;
        public string Author;
        public IEnumerable<CategoryBase> Categories;
        public string Summary;
        public string Description;
        public string WebsiteUrl;
        public string LogoUrl;
        public string ExternalId;
        public MinecraftVersion GameVersion;
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
            if (Type == AddonType.Mods || Type == AddonType.Resourcepacks || Type == AddonType.Shaders)
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
        public string JavaName;
        public long LastUpdate;
        public string ExecutableFile = "/bin/javaw.exe";
        public string ManifestUrl;

        public JavaVersion(string name, JavaVersionManifest.JavaVersionDesc javaManifest)
        {
            JavaName = name;

            if (javaManifest?.VersionInfo?.Released != null)
                LastUpdate = DateTimeOffset.Parse(javaManifest.VersionInfo.Released).ToUnixTimeSeconds();

            ManifestUrl = javaManifest?.Manifest?.Url;
        }

        public JavaVersion() { }
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
    /// Ключ - id.
    /// </summary>
    public class InstalledAddonsFormat : Dictionary<string, InstalledAddonInfo> { } // TODO: перенести в CommonClientData

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

    public abstract class CategoryBase : IProjectCategory, IEquatable<CategoryBase>
    {
        public abstract string Id { get; set; }
        public abstract string Name { get; set; }
        public abstract string ClassId { get; set; }
        public abstract string ParentCategoryId { get; set; }
        
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CategoryBase))
                return false;

            return Equals(obj as CategoryBase);
        }

        public bool Equals(CategoryBase other)
        {
            return Name.ToLower() == other.Name.ToLower();
        }

        public override int GetHashCode()
        {
            return Name.ToLower().GetHashCode();
        }
    }

    public class SimpleCategory : CategoryBase
    {
        public override string Id { get; set; }
        public override string Name { get; set; }
        public override string ClassId { get; set; }
        public override string ParentCategoryId { get; set; }

        public SimpleCategory() { }

        public SimpleCategory(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public SimpleCategory(CategoryBase category)
        {
            Id = category.Id;
            Name = category.Name;
            ClassId = category.ClassId;
            ParentCategoryId = category.ParentCategoryId;
        }
    }

    /// <summary>
    /// Манифест джава версий, который мы получаем от моджанга
    /// Используется только для получения данных с сервера
    /// </summary>
    public class JavaVersionManifest
    {
        private Dictionary<string, JavaVersionDesc> SeterValueParser(Dictionary<string, List<JavaVersionDesc>> value)
        {
            Dictionary<string, JavaVersionDesc> result = new();
            if (value != null)
            {
                foreach (var key in value.Keys)
                {
                    if (value[key] != null && value[key].Count > 0)
                    {
                        result[key] = value[key][0];
                    }
                }
            }

            return result;
        }

        [JsonProperty("windows-x64")]
        public Dictionary<string, List<JavaVersionDesc>> Windows_x64_Setter
        {
            set => _windows_x64 = SeterValueParser(value);
        }

        [JsonProperty("windows-x86")]
        public Dictionary<string, List<JavaVersionDesc>> Windows_x86_Setter
        {
            set => _windows_x86 = SeterValueParser(value);
        }

        private Dictionary<string, JavaVersionDesc> _windows_x64;
        private Dictionary<string, JavaVersionDesc> _windows_x86;

        [JsonIgnore]
        public Dictionary<string, JavaVersionDesc> Windows_x64
        {
            get => _windows_x64;
        }

        [JsonIgnore]
        public Dictionary<string, JavaVersionDesc> Windows_x84
        {
            get => _windows_x86;
        }

        [JsonIgnore]
        public Dictionary<string, JavaVersionDesc> GetWindowsActual
        {
            get
            {
                return System.Environment.Is64BitOperatingSystem ? _windows_x64 : _windows_x86;
            }
        }

        public class JavaVersionDesc
        {
            public class JavaManifest
            {
                [JsonProperty("url")]
                public string Url;
                [JsonProperty("sha1")]
                public string Sha1;
            }

            public class JavaVersionInfo
            {
                [JsonProperty("name")]
                public string SimpleName;
                [JsonProperty("released")]
                public string Released;
            }

            [JsonProperty("manifest")]
            public JavaManifest Manifest;

            [JsonProperty("version")]
            public JavaVersionInfo VersionInfo;
        }
    }

    public class JavaFiles
    {
        public enum UnitType
        {
            [JsonProperty("directory")]
            Directory,
            [JsonProperty("file")]
            File
        }

        public class Unit
        {
            public class DownloadsWays
            {
                [JsonProperty("lzma")]
                public DownloadWay Lzma;
                [JsonProperty("raw")]
                public DownloadWay Raw;
            }

            public class DownloadWay
            {
                [JsonProperty("sha1")]
                public string Sha1;
                [JsonProperty("size")]
                public string Size;
                [JsonProperty("url")]
                public string DownloadUrl;
            }

            [JsonProperty("type")]
            public UnitType Type;

            [JsonProperty("executable")]
            public bool Executable;

            [JsonProperty("downloads")]
            public DownloadsWays Downloads;
        }

        [JsonProperty("files")]
        public Dictionary<string, Unit> Files;

    }

}
