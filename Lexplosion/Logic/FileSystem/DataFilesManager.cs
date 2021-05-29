using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Lexplosion.Logic.FileSystem.WithDirectory;

namespace Lexplosion.Logic.FileSystem
{
    static class DataFilesManager
    {
        public static void SaveSettings(Dictionary<string, string> data, string instanceId = "")
        {
            string file;

            if (instanceId == "")
            {
                string path = Environment.ExpandEnvironmentVariables("%appdata%") + "/night-world";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/settings.json";
            }
            else
            {
                string path = directory + "/instances/" + instanceId;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/instanceSettings.json";
            }

            try
            {
                Dictionary<string, string> settings = GetSettings(instanceId);
                if (settings != null)
                {
                    foreach (string key in data.Keys)
                    {
                        settings[key] = data[key];

                    }
                        
                }
                else
                {
                    settings = data;
                }

                if (settings.ContainsKey("password"))
                    settings["password"] = Convert.ToBase64String(AesСryp.Encode(settings["password"], Encoding.Default.GetBytes(LaunсherSettings.passwordKey), Encoding.Default.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));

                using (FileStream fstream = new FileStream(file, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }
        }

        public static Dictionary<string, string> GetSettings(string instanceId = "")
        {
            string file;
            if (instanceId == "")
            {
                file = Environment.ExpandEnvironmentVariables("%appdata%") + "/night-world/settings.json";
            }
            else
            {
                file = directory + "/instances/" + instanceId + "/instanceSettings.json";

                if (!File.Exists(file))
                {
                    return new Dictionary<string, string>();
                }
            }

            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(fileBytes));

                    if (settings.ContainsKey("password"))
                        settings["password"] = AesСryp.Decode(Convert.FromBase64String(settings["password"]), Encoding.Default.GetBytes(LaunсherSettings.passwordKey), Encoding.Default.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16)));

                    return settings;
                }

            }
            catch
            {
                return new Dictionary<string, string>();
            }

        }
        public static Dictionary<string, InstanceAssets> GetLauncherAssets()
        {
            try
            {
                var data = GetFile<Dictionary<string, InstanceAssets>>(directory + "/launcherAssets.json");

                return data;
            }
            catch { return new Dictionary<string, InstanceAssets>(); }
        }

        public static bool InstanceIsInstalled(string instanceId)
        {
            try
            {
                return Directory.Exists(directory + "/instances/" + instanceId);
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteLastUpdates(string instanceId) //Эта функция удаляет файл lastUpdates.json
        {
            try
            {
                if (File.Exists(directory + "/instances/" + instanceId + "/lastUpdates.json"))
                {
                    File.Delete(directory + "/instances/" + instanceId + "/lastUpdates.json");
                }

                return true;

            }
            catch { return false; }
        }

        public static int GetUpgradeToolVersion()
        {

            if (!File.Exists(directory + "/up-version.txt"))
                return -1;

            try
            {
                using (FileStream fstream = File.OpenRead(directory + "/up-version.txt"))
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
                if (!File.Exists(directory + "/up-version.txt"))
                    File.Create(directory + "/up-version.txt").Close();

                using (FileStream fstream = new FileStream(directory + "/up-version.txt", FileMode.Create))
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
            try
            {
                string dirName = Path.GetDirectoryName(name);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                using (FileStream fstream = new FileStream(name, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

                return true;

            }
            catch
            {
                return false;
            }

        }

        public static T GetFile<T>(string file)
        {
            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(fileBytes));

                }

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

        public static void SaveFilesList(string instanceId, InstanceFiles data)
        {
            InstanceLocalFiles dataLocal = new InstanceLocalFiles
            {
                version = new LocalVersionInfo()
                {
                    minecraftJar = new Dictionary<string, string>
                    {
                        ["name"] = data.version.minecraftJar.name
                    },

                    arguments = data.version.arguments,
                    gameVersion = data.version.gameVersion,
                    assetsVersion = data.version.assetsVersion,
                    assetsIndexes = data.version.assetsIndexes,
                    mainClass = data.version.mainClass

                }
            };

            SaveFile(directory + "/instances/" + instanceId + "/" + "filesList.json", JsonConvert.SerializeObject(dataLocal));
            SaveFile(directory + "/versions/libraries/" + data.version.gameVersion + ".json", JsonConvert.SerializeObject(data.libraries));
        }

        public static InstanceFiles GetFilesList(string instanceId)
        {
            InstanceFiles data = GetFile<InstanceFiles>(directory + "/instances/" + instanceId + "/" + "filesList.json");
            if (data == null)
            {
                return null;
            }

            List<string> librariesData = GetFile<List<string>>(directory + "/versions/libraries/" + data.version.gameVersion + ".json");
            if (librariesData == null)
            {
                return null;
            }

            data.libraries = librariesData;

            return data;

        }

        public static Dictionary<string, string> GetModpaksList()
        {
            Dictionary<string, string> baseList = GetFile<Dictionary<string, string>>(directory + "/instanesList.json");
            Dictionary<string, string> list = new Dictionary<string, string>();

            if (baseList != null)
            {
                foreach (string key in baseList.Keys)
                {
                    if (Directory.Exists(directory + "/instances/" + key))
                    {
                        list[key] = baseList[key];
                    }
                }
            }

            return list;

        }

        public static void SaveModpaksList(Dictionary<string, string> content)
        {
            SaveFile(directory + "/instanesList.json", JsonConvert.SerializeObject(content));
        }

    }

}
