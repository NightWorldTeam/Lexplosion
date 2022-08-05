using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using static Lexplosion.Logic.FileSystem.WithDirectory;

namespace Lexplosion.Logic.FileSystem
{
    static class DataFilesManager
    {
        private class LocalVersionManifest //нужен для декодирования json
        {
            public LocalVersionInfo version;
        }

        public static void SaveAccount(string login, string password)
        {
            password = Convert.ToBase64String(AesСryp.Encode(password, Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));
            SaveFile(LaunсherSettings.LauncherDataPath + "/account.json", JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                ["login"] = login,
                ["password"] = password
            }));
        }

        public static void GetAccount(out string login, out string password)
        {
            var data = GetFile<Dictionary<string, string>>(LaunсherSettings.LauncherDataPath + "/account.json");
            if (data != null && data.ContainsKey("login"))
            {
                login = data["login"];
            }
            else
            {
                login = null;
            }

            if (data != null && data.ContainsKey("password") && data["password"] != null)
            {
                try
                {
                    password = AesСryp.Decode(Convert.FromBase64String(data["password"]), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16)));
                }
                catch
                {
                    password = null;
                }
            }
            else
            {
                password = null;
            }
        }

        public static void SaveSettings(Settings data, string instanceId = "")
        {
            string file;

            if (instanceId == "")
            {
                string path = LaunсherSettings.LauncherDataPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/settings.json";
            }
            else
            {
                data.GamePath = null;

                string path = DirectoryPath + "/instances/" + instanceId;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/instanceSettings.json";
            }

            try
            {
                Settings settings = GetSettings(instanceId);
                if (settings != null)
                {
                    settings.Merge(data);
                }
                else
                {
                    settings = data;
                }

                using (FileStream fstream = new FileStream(file, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }
        }

        public static Settings GetSettings(string instanceId = "")
        {
            string file;
            if (instanceId == "")
            {
                file = LaunсherSettings.LauncherDataPath + "/settings.json";
            }
            else
            {
                file = DirectoryPath + "/instances/" + instanceId + "/instanceSettings.json";

                if (!File.Exists(file))
                {
                    return new Settings();
                }
            }

            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    Settings settings = JsonConvert.DeserializeObject<Settings>(Encoding.UTF8.GetString(fileBytes));
                    if (instanceId != "") settings.GamePath = null;

                    return settings ?? new Settings();
                }
            }
            catch
            {
                return new Settings();
            }
        }

        public static bool DeleteLastUpdates(string instanceId) //Эта функция удаляет файл lastUpdates.json
        {
            try
            {
                if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json"))
                {
                    File.Delete(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json");
                }

                return true;

            }
            catch { return false; }
        }

        public static int GetUpgradeToolVersion()
        {
            if (!File.Exists(DirectoryPath + "/up-version.txt"))
                return -1;

            try
            {
                using (FileStream fstream = File.OpenRead(DirectoryPath + "/up-version.txt"))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return Int32.Parse(Encoding.UTF8.GetString(fileBytes));

                }

            }
            catch { return -1; }

        }

        public static void SetUpgradeToolVersion(int version)
        {
            try
            {
                if (!File.Exists(DirectoryPath + "/up-version.txt"))
                    File.Create(DirectoryPath + "/up-version.txt").Close();

                using (FileStream fstream = new FileStream(DirectoryPath + "/up-version.txt", FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(version.ToString());
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }

        }

        public static bool SaveFile(string name, string content)
        {
            //try
            {
                string dirName = Path.GetDirectoryName(name);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                using (FileStream fstream = new FileStream(name, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

                return true;

            }
            /*catch
            {
                return false;
            }*/
        }

        public static T GetFile<T>(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (FileStream fstream = File.OpenRead(file))
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(fileBytes));
                    }
                }

                return default;
            }
            catch
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                return default;
            }
        }

        public static void SaveManifest(string instanceId, VersionManifest data)
        {
            string minecraftJar = "";
            if (data.version.minecraftJar != null)
            {
                minecraftJar = data.version.minecraftJar.name;
            }

            LocalVersionManifest dataLocal = new LocalVersionManifest
            {
                version = new LocalVersionInfo()
                {
                    minecraftJar = new Dictionary<string, string>
                    {
                        ["name"] = minecraftJar
                    },

                    arguments = data.version.arguments,
                    gameVersion = data.version.gameVersion,
                    assetsVersion = data.version.assetsVersion,
                    assetsIndexes = data.version.assetsIndexes,
                    mainClass = data.version.mainClass,
                    modloaderVersion = data.version.modloaderVersion,
                    modloaderType = data.version.modloaderType
                }
            };

            SaveFile(DirectoryPath + "/instances/" + instanceId + "/" + "manifest.json", JsonConvert.SerializeObject(dataLocal));
            if (data.libraries != null)
            {
                SaveFile(DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json", JsonConvert.SerializeObject(data.libraries));
            }
        }

        public static VersionManifest GetManifest(string instanceId, bool includingLibraries)
        {
            VersionManifest data = GetFile<VersionManifest>(DirectoryPath + "/instances/" + instanceId + "/" + "manifest.json");
            if (data == null)
            {
                return null;
            }

            if (includingLibraries)
            {
                Dictionary<string, LibInfo> librariesData = GetFile<Dictionary<string, LibInfo>>(DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json");
                if (librariesData == null)
                {
                    librariesData = new Dictionary<string, LibInfo>();
                }

                data.libraries = librariesData;
            }

            return data;
        }

        public static InstalledAddonsFormat GetInstalledAddons(string instanceId)
        {
            string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json";

            var data = DataFilesManager.GetFile<InstalledAddonsFormat>(path);
            if (data == null)
            {
                return new InstalledAddonsFormat();
            }

            return data;
        }

        public static void SaveInstalledAddons(string instanceId, InstalledAddonsFormat data)
        {
            string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json";
            DataFilesManager.SaveFile(path, JsonConvert.SerializeObject(data));
        }
    }
}
