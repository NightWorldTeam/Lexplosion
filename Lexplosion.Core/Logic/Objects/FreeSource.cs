using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Objects.FreeSource
{
    public class InstanceManifest : ArchivedClientData
    {
        public InstalledAddonsFormat Addons;
    }

    public class FileDesc
    {
        [JsonProperty("sha512")]
        public string Sha512;

        [JsonProperty("size")]
        public long Size;

        public FileDesc(string sha512, long size)
        {
            Sha512 = sha512;
            Size = size;
        }

        public override int GetHashCode()
        {
            if (Sha512 == null) return (int)Size;
            return Sha512.GetHashCode() ^ RotateLeft((int)Size);
        }

        private int RotateLeft(int value)
        {
            return (value << 16) | (value >> 16);
        }

        public override bool Equals(object obj)
        {
            if (obj is FileDesc value)
            {
                return Sha512 == value.Sha512 && Size == value.Size;
            }

            return false;
        }

        public override string ToString()
        {
            return "(sha512: " + Sha512 + ", size: " + Size + ")";
        }
    }

    public class SourceMap
    {
        [JsonProperty("modpacksListUrl")]
        public string ModpacksListUrl;

        [JsonProperty("modpackManifestUrl")]
        public string ModpackManifestUrl;

        [JsonProperty("modpackVersionsListUrl")]
        public string ModpackVersionsListUrl;

        [JsonProperty("modpackVersionManifestUrl")]
        public string ModpackVersionManifestUrl;

        [JsonIgnore]
        public string SourceId;
    }

    public class SourceManifest
    {
        public SourceMap sourceMap;
    }

    public class ModpackVersion
    {
        [JsonProperty("downloadUrl")]
        public string DownloadUrl;

        [JsonProperty("loadDate")]
        public long LoadDate;

        [JsonProperty("modpackId")]
        public string ModpackId;

        [JsonProperty("version")]
        public string Version;
    }

    public class MidpackVersionsList
    {
        [JsonProperty("latestVersion")]
        public string LatestVersion;

        [JsonProperty("allVersions")]
        public Dictionary<string, ModpackVersion> AllVersions;
    }

    public class ModpackManifest
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("updateDate")]
        public long UpdateDate;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("Summary")]
        public string Summary;

        [JsonProperty("gameVersion")]
        public string GameVersion;

        private ClientType _core;

        [JsonProperty("core")]
        public ClientType Core
        {
            get => _core;
            set
            {
                if (!Enum.IsDefined(typeof(ClientType), (int)value))
                {
                    _core = ClientType.Vanilla;
                }
                else
                {
                    _core = value;
                }
            }
        }

        [JsonProperty("logoUrl")]
        public string LogoUrl;

        [JsonProperty("images")]
        public List<string> Images;
    }

    public class LocalIdData
    {
        public string Id;
        public string SourceUrl;

        public static LocalIdData Load(string idData)
        {
            try
            {
                return JsonConvert.DeserializeObject<LocalIdData>(idData);
            }
            catch
            {
                return null;
            }
        }

        public string EncodeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
