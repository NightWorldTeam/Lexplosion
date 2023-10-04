using Lexplosion.Logic.Management;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public class MinecraftArgumentObject
    {
        public class Rule
        {
            public enum Access
            {
                NotAllow,
                [JsonProperty("allow")]
                Allow
            }

            public class OS
            {
                public enum SysArch
                {
                    Unknown,
                    x64,
                    x86,
                    x32
                }

                [JsonProperty("name")]
                public string Name;
                [JsonProperty("version")]
                public string Version;
                [JsonProperty("arch")]
                public SysArch Arch;
            }

            [JsonProperty("action")]
            public Access Action;
            [JsonProperty("os")]
            public OS Os;
        }

        [JsonIgnore]
        public string[] Value;

        [JsonProperty("rules")]
        public List<Rule> Rules;

        public object value
        {
            get => Value; set
            {
                if (value is JArray)
                {
                    JArray jarray = (JArray)value;
                    string[] result = new string[jarray.Count];

                    int i = 0;
                    foreach (JToken item in jarray)
                    {
                        result[i] = item.ToString();
                        i++;
                    }

                    Value = result;
                }
                else if (value is string)
                {
                    Value = new string[1] { (string)value };
                }
            }
        }
    }

    public class MinecraftArgument
    {
        [JsonIgnore]
        public string GetSting { get => _stringValue; }

        [JsonIgnore]
        public MinecraftArgumentObject GetObject { get => _objectValue; }

        private string _stringValue;
        private MinecraftArgumentObject _objectValue;

        public MinecraftArgument(string value)
        {
            _stringValue = value;
        }

        public MinecraftArgument(JObject obj)
        {
            _objectValue = obj.ToObject<MinecraftArgumentObject>();
        }
    }

    public class DefaultMinecraftArguments
    {
        [JsonIgnore]
        public IEnumerable<MinecraftArgument> Game;
        public List<object> game
        {
            get
            {
                List<object> reult = null;
                if (Game != null)
                {
                    reult = new List<object>();
                    foreach (MinecraftArgument item in Game)
                    {
                        string str = item.GetSting;
                        if (str != null)
                        {
                            reult.Add(str);
                        }
                        else
                        {
                            reult.Add(item.GetObject);
                        }
                    }
                }

                return reult;
            }
            set
            {
                var args = new List<MinecraftArgument>();

                if (value != null)
                {
                    foreach (var obj in value)
                    {
                        if (obj != null && obj is string)
                        {
                            args.Add(new MinecraftArgument((string)obj));
                        }
                    }
                }

                Game = args;
            }
        }

        [JsonIgnore]
        public IEnumerable<MinecraftArgument> Jvm;

        public List<object> jvm
        {
            get
            {
                List<object> reult = null;
                if (Jvm != null)
                {
                    reult = new List<object>();
                    foreach (MinecraftArgument item in Jvm)
                    {
                        string str = item.GetSting;
                        if (str != null)
                        {
                            reult.Add(str);
                        }
                        else
                        {
                            reult.Add(item.GetObject);
                        }
                    }
                }

                return reult;
            }
            set
            {
                var args = new List<MinecraftArgument>();

                if (value != null)
                {
                    foreach (var obj in value)
                    {
                        if (obj != null)
                        {
                            if (obj is string)
                            {
                                args.Add(new MinecraftArgument((string)obj));
                            }
                            else if (obj is JObject)
                            {
                                args.Add(new MinecraftArgument((JObject)obj));
                            }
                        }
                    }
                }

                Jvm = args;
            }
        }
    }

    /// <summary>
    /// Основная часть манифеста клиента
    /// </summary>
    public class VersionInfo
    {
        public FileInfo minecraftJar;
        public bool isStatic;
        public long releaseIndex;
        /// <summary>
        /// Аргументы игры
        /// </summary>
        public string arguments;
        /// <summary>
        /// Аргументы для java
        /// </summary>
        public string jvmArguments;
        public DefaultMinecraftArguments defaultArguments;
        /// <summary>
        /// Версия игры в виде строки
        /// </summary>
        public string gameVersion;

        private MinecraftVersion _gameVersionInfo;

        /// <summary>
        /// Вся информация о версии игры
        /// </summary>
        public MinecraftVersion gameVersionInfo
        {
            get
            {
                if (_gameVersionInfo?.IsNan != false)
                {
                    _gameVersionInfo = new MinecraftVersion(gameVersion);
                }

                return _gameVersionInfo;
            }
            set
            {
                _gameVersionInfo = value;
                if (!string.IsNullOrWhiteSpace(value?.Id))
                {
                    gameVersion = value.Id;
                }
            }
        }

        public string assetsVersion;
        public string assetsIndexes;
        public string mainClass;
        public string modloaderVersion;
        public ClientType modloaderType;
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
                if (modloaderType != ClientType.Vanilla)
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

    public class InstancePlatformData
    {
        public string id;
        public string instanceVersion;
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

    public class InstanceContent
    {
        public InstalledAddonsFormat InstalledAddons;
        public List<string> Files { get; set; }
        public bool FullClient = false;
    }

    /// <summary>
    /// Используется для хранения класса InstanceContent в файле. 
    /// Нужно это чтобы не хранить в файле информацию из InstanceContent, которая нужна только в рантайме.
    /// Есть толь ко у сборок Curseforge и Modrinth
    /// </summary>
    public class InstanceContentFile
    {
        public List<string> InstalledAddons;
        public List<string> Files { get; set; }
        public bool FullClient = false;
    }
}
