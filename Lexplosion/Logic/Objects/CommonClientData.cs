using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lexplosion.Logic.Objects.CommonClientData
{
    /// <summary>
    /// Структура файла lastUpdates.json
    ///<summary>
    public class LastUpdates : Dictionary<string, long> { }

    /// <summary>
    /// Информация о майкрафтовской либе
    /// </summary>
    public class LibInfo
    {
        public class ActivationConditions
        {
            public List<string> accountTypes = null;
        }

        public bool notArchived;
        public string url;
        public List<List<string>> obtainingMethod;
        public bool isNative;
        public ActivationConditions activationConditions;
        public bool notLaunch;

        /// <summary>
        /// Эта залупа нужно чтобы просто пометить либрариес, что он относится к дополнительному инсталлеру, для того чтобы опотом их можно было сохранить отдельно.
        /// Если он не относится к дополнительным, то эта хуйня будет null
        /// </summary>
        [JsonIgnore]
        public AdditionalInstallerType? additionalInstallerType;
    }

    /// <summary>
    /// Манифест клиента. Имнно этот класс будет сохранен в manifest.json
    /// </summary>
    public class VersionManifest
    {
        public VersionInfo version;
        public Dictionary<string, LibInfo> libraries;

        /// <summary>
        /// Т.к либрариесы хранятся в другом файле, то прописываем эту хуйню чтобы они не сереализовались в json и не торчали просто так в manifest.json
        /// </summary>
        public bool ShouldSerializelibraries() => false;
    }

    /// <summary>
    /// Основная часть манифеста клиента
    /// </summary>
    public class VersionInfo
    {
        public FileInfo minecraftJar;
        public bool isStatic;
        public long releaseIndex;
        public string arguments;
        public string jvmArguments;
        public string gameVersion;
        public string assetsVersion;
        public string assetsIndexes;
        public string mainClass;
        public string modloaderVersion;
        public ModloaderType modloaderType;
        public AdditionalInstaller additionalInstaller;
        public string CustomVersionName;
        public bool security;
        public long librariesLastUpdate;

        /// <summary>
        /// Это свойство возвращает имя для файла либрариесов (файлы .lver, что хранят версию либрариесов и файлы .json, которые хранят список либрариесов для конкретной версии игры).
        /// У каждой версии игры своё имя для файлов с информацией о либрариесах
        /// </summary>
        [JsonIgnore]
        public string GetLibName
        {
            get
            {
                if (CustomVersionName != null)
                    return CustomVersionName;

                string endName = "";
                if (modloaderType != ModloaderType.Vanilla)
                {
                    endName = "-" + modloaderType.ToString() + "-" + modloaderVersion;
                }

                return gameVersion + endName;
            }
        }

        //Прописываем какие поля нужно проигнорировать
        public bool ShouldSerializeCustomVersionName() => false;
        public bool ShouldSerializesecurity() => false;
        public bool ShouldSerializelibrariesLastUpdate() => false;
    }

    /// <summary>
    /// Класс, описывающий дополнительынй инсталлер (вроде оптифайна)
    /// </summary>
    public class AdditionalInstaller
    {
        public string arguments;
        public string jvmArguments;
        public string mainClass;
        public AdditionalInstallerType type;
        public string installerVersion;
        public long librariesLastUpdate;
        public string gameVersion;

        /// <summary>
        /// Аналогично GetLibName из VersionInfo.
        /// </summary>
        [JsonIgnore]
        public string GetLibName
        {
            get
            {
                return gameVersion + "-" + type + "-" + installerVersion;
            }
        }
    }

    public class AdditionalInstallerManifest
    {
        public AdditionalInstaller version;
        public Dictionary<string, LibInfo> libraries;
    }

    public class FileInfo
    {
        public string name;
        public string url;
        public string sha1;
        public long size;
        public long lastUpdate;
        public bool notArchived;

        public bool ShouldSerializeurl() => false;
        public bool ShouldSerializesha1() => false;
        public bool ShouldSerializesize() => false;
        public bool ShouldSerializelastUpdate() => false;
        public bool ShouldSerializenotArchived() => false;
    }

    class InstancePlatformData
    {
        public string id;
        public int instanceVersion;
    }

    /// <summary>
    /// Нужен для передачи данных между методами при запуске игры.
    /// </summary>
    public class InitData
    {
        public InstanceInit InitResult;
        public List<string> DownloadErrors;
        public VersionInfo VersionFile;
        public Dictionary<string, LibInfo> Libraries;
        public bool UpdatesAvailable;
        public string ClientVersion = "";
    }

    public struct MCVersionInfo
    {
        public string type { get; set; }
        public string id { get; set; }
    }
}
