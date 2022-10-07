using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.Logic.Network
{
    static class NightWorldApi
    {
        private class DataNInstanceManifest : NightWorldManifest //этот класс нужен для декодирования json в GetInstanceManifest
        {
            public string code;
            public string str;
        }

        public class FullInstanceInfo : InstanceInfo
        {
            public long DownloadCounts;
            public string WebsiteUrl;
            public List<string> Images;
            public long LastUpdate;
            public ModloaderType Modloader;
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

        // Функция получает манифест для NightWorld модпаков
        public static NightWorldManifest GetInstanceManifest(string instanceId) // TODO: одинаковые блоки кода в этих двух функция вынести в другую функцию
        {
            Runtime.DebugWrite("GET MANIFEST " + instanceId);
            string[] chars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string str = "";
            string str2 = "";
            Random rnd = new Random();

            for (int i = 0; i < 32; i++)
            {
                str += chars[rnd.Next(0, chars.Length)];
                str2 += chars[rnd.Next(0, chars.Length)];
            }

            using (SHA1 sha = new SHA1Managed())
            {
                string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

                int d = 32 - key.Length;
                for (int i = 0; i < d; i++)
                {
                    key += str2[i];
                }

                Dictionary<string, string> data = new Dictionary<string, string> 
                {
                    ["str"] = str,
                    ["str2"] = str2,
                    ["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
                };

                try
                {
                    string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(instanceId) + "/manifest", data);

                    if (answer != null)
                    {
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));
                        DataNInstanceManifest filesData = JsonConvert.DeserializeObject<DataNInstanceManifest>(answer);

                        if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                        {

                            NightWorldManifest ret = new NightWorldManifest
                            {
                                data = filesData.data,
                                version = filesData.version,
                                CustomVersion = filesData.CustomVersion
                            };

                            return ret;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static VersionManifest GetVersionManifest(string modpackId)
        {
            string[] chars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string str = "";
            string str2 = "";
            Random rnd = new Random();

            for (int i = 0; i < 32; i++)
            {
                str += chars[rnd.Next(0, chars.Length)];
                str2 += chars[rnd.Next(0, chars.Length)];
            }

            using (SHA1 sha = new SHA1Managed())
            {
                string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

                int d = 32 - key.Length;
                for (int i = 0; i < d; i++)
                {
                    key += str2[i];
                }

                Dictionary<string, string> data = new Dictionary<string, string> 
                { 
                    ["str"] = str,
                    ["str2"] = str2,
                    ["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
                };

                try
                {
                    Runtime.DebugWrite("URL " + LaunсherSettings.URL.ModpacksData + modpackId + "/versionManifest");
                    string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + modpackId + "/versionManifest", data);

                    if (answer != null)
                    {
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

                        DataVersionManifest filesData = JsonConvert.DeserializeObject<DataVersionManifest>(answer);

                        if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                        {
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

                            VersionManifest ret = new VersionManifest
                            {
                                version = filesData.version,
                                libraries = libraries
                            };

                            return ret;
                        }
                        else
                        {
                            MessageBox.Show("null1");
                            return null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("null2");
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
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
