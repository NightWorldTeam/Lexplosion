using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Network
{
    static class NightWorldApi
    {
        /// <summary>
        /// этот класс нужен для декодирования json в GetInstanceManifest
        /// </summary>
        private class ProtectedNightWorldManifest : NightWorldManifest, ProtectedManifest
        {
            public string code { get; set; }
            public string str { get; set; }
        }

        public class FullInstanceInfo : InstanceInfo
        {
            public long DownloadCounts;
            public string WebsiteUrl;
            public List<string> Images;
            public long LastUpdate;
            public ClientType Modloader;
        }

        public class InstanceInfo
        {
            public string Name;
            public string LogoUrl;
            public string Author;
            public long Version;
            public string Description;
            public string Summary;
            public List<Category> Categories;
            public string GameVersion;
        }

        public static Dictionary<string, InstanceInfo> GetInstancesList()
        {
            try
            {
                string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData);
                Dictionary<string, InstanceInfo> list = JsonConvert.DeserializeObject<Dictionary<string, InstanceInfo>>(answer);

                return list;
            }
            catch
            {
                return new Dictionary<string, InstanceInfo>();
            }
        }

        public static int GetInstanceVersion(string id)
        {
            try
            {
                string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(id) + "/version");
                Int32.TryParse(answer, out int idInt);

                return idInt;
            }
            catch
            {
                return 0;
            }
        }

        public static FullInstanceInfo GetInstanceInfo(string id)
        {
            try
            {
                string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(id) + "/info");
                FullInstanceInfo info = JsonConvert.DeserializeObject<FullInstanceInfo>(answer);

                return info;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Получет манифест для NightWorld модпаков
        /// </summary>
        /// <param name="instanceId"></param>
        public static NightWorldManifest GetInstanceManifest(string instanceId) // TODO: одинаковые блоки кода в этих двух функция вынести в другую функцию
        {
            var filesData = ToServer.ProtectedRequest<ProtectedNightWorldManifest>(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(instanceId) + "/manifest");

            if (filesData == null) return null;

            NightWorldManifest ret = new NightWorldManifest
            {
                data = filesData.data,
                version = filesData.version,
                CustomVersion = filesData.CustomVersion
            };

            return ret;
        }

        public static VersionManifest GetVersionManifest(string modpackId)
        {
            var filesData = ToServer.ProtectedRequest<ProtectedVersionManifest>(LaunсherSettings.URL.ModpacksData + modpackId + "/versionManifest");

            if (filesData == null) return null;

            Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
            foreach (string lib in filesData.libraries.Keys)
            {
                if (filesData.libraries[lib].os == null || filesData.libraries[lib].os.Contains("windows"))
                {
                    libraries[lib] = new LibInfo
                    {
                        notArchived = filesData.libraries[lib].notArchived,
                        url = filesData.libraries[lib].url,
                        obtainingMethod = filesData.libraries[lib].obtainingMethod,
                        isNative = filesData.libraries[lib].isNative
                    };
                }
            }

            return new VersionManifest
            {
                version = filesData.version,
                libraries = libraries
            };
        }

        public static PlayerData GetPlayerData(string uuid)
        {
            string data = ToServer.HttpPost(LaunсherSettings.URL.Account + "getPlayerData", new Dictionary<string, string>
            {
                ["playerUUID"] = uuid
            });

            if (data != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<PlayerData>(data);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
