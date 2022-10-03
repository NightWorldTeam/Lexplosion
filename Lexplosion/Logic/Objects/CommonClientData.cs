using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Это свойство возвращает имя для файла либрариесов (файлы .lver, что хранят версию либрариесов и файлы .json, которые хранят список либрариесов для конкретной версии игры).
        /// У каждой версии игры своё имя для файлов с информацией о либрариесах
        /// </summary>
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
    }

    /// <summary>
    /// Локальный манифест модпака, который хранится в файле manifest.json
    /// </summary>
    public class LocalVersionInfo
    {
        public Dictionary<string, string> minecraftJar;
        public long releaseIndex;
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
