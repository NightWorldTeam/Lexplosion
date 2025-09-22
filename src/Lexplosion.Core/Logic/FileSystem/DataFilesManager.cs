using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using System.Text.RegularExpressions;

namespace Lexplosion.Logic.FileSystem
{
    public class DataFilesManager
    {
        public const string INSTALLED_ADDONS_FILE = "installedAddons.nwdat";
        public const string INSTANCE_PLATFORM_DATA_FILE = "instancePlatformData.nwdat";
        public const string INSTANCE_CONTENT_FILE = "instanceContent.nwdat";
        public const string LAST_UPDATES_FILE = "lastUpdates.nwdat";
        public const string MANIFEST_FILE = "manifest.nwdat";

        public const string INSTALLED_ADDONS_FILE_OLD = "installedAddons.json";
        public const string INSTANCE_PLATFORM_DATA_FILE_OLD = "instancePlatformData.json";
        public const string INSTANCE_CONTENT_FILE_OLD = "instanceContent.json";
        public const string LAST_UPDATES_FILE_OLD = "lastUpdates.json";
        public const string MANIFEST_FILE_OLD = "manifest.json";

        public const string INSTANCES_GROUPS_FILE = "instancesGroups.json";

        private readonly WithDirectory _withDirectory;

        public DataFilesManager(WithDirectory withDirectory)
        {
            _withDirectory = withDirectory;
        }

        public void SaveSettings(Settings data, string instanceId = "")
        {
            string file;
            data.ItIsNotShit = true;

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

                string path = _withDirectory.GetInstancePath(instanceId);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "instanceSettings.json";
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

        public Settings GetSettings(string instanceId = "")
        {
            string file;
            if (instanceId == "")
            {
                file = LaunсherSettings.LauncherDataPath + "/settings.json";
            }
            else
            {
                file = _withDirectory.GetInstancePath(instanceId) + "instanceSettings.json";

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
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return new Settings();
            }
        }

        public bool DeleteLastUpdates(string instanceId) //Эта функция удаляет файл lastUpdates.json
        {
            try
            {
                string instancePath = _withDirectory.GetInstancePath(instanceId);
                if (File.Exists(instancePath + LAST_UPDATES_FILE))
                {
                    File.Delete(instancePath + LAST_UPDATES_FILE);
                }

                return true;

            }
            catch { return false; }
        }

        public LastUpdates GetLastUpdates(string instanceId)
        {
            var data = TryGetFile<LastUpdates>(instanceId, LAST_UPDATES_FILE, LAST_UPDATES_FILE_OLD);
            return data ?? new LastUpdates();
        }

        public void SaveLastUpdates(string instanceId, LastUpdates updates)
        {
            SaveFile(_withDirectory.GetInstancePath(instanceId) + LAST_UPDATES_FILE, JsonConvert.SerializeObject(updates));
        }

        public int GetUpgradeToolVersion()
        {
            if (!File.Exists(_withDirectory.DirectoryPath + "/up-version.txt"))
                return -1;

            try
            {
                using (FileStream fstream = File.OpenRead(_withDirectory.DirectoryPath + "/up-version.txt"))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return Int32.Parse(Encoding.UTF8.GetString(fileBytes));
                }

            }
            catch { return -1; }

        }

        public void SetUpgradeToolVersion(int version)
        {
            try
            {
                if (!File.Exists(_withDirectory.DirectoryPath + "/up-version.txt"))
                    File.Create(_withDirectory.DirectoryPath + "/up-version.txt").Close();

                using (FileStream fstream = new FileStream(_withDirectory.DirectoryPath + "/up-version.txt", FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(version.ToString());
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }

        }

        public bool SaveFile(string name, string content)
        {
            try
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
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получет и декодирует содержиоме JSON файла
        /// </summary>
        /// <typeparam name="T">Тип, к которому привести JSON</typeparam>
        /// <param name="file">Путь до файла</param>
        /// <returns>Декодированные данные</returns>
        public T GetFile<T>(string file)
        {
            try
            {
                string fileContent = GetFile(file);
                if (fileContent == null) return default;
                return JsonConvert.DeserializeObject<T>(fileContent);
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return default;
            }
        }

        /// <summary>
        /// Получет содержимое текстового файла
        /// </summary>
        /// <param name="file">Путь до файла</param>
        /// <returns>Текстовые данные</returns>
        public string GetFile(string file)
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

                        return Encoding.UTF8.GetString(fileBytes);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                return null;
            }
        }

        public void SaveManifest(string instanceId, VersionManifest data)
        {
            SaveFile(_withDirectory.GetInstancePath(instanceId) + MANIFEST_FILE, JsonConvert.SerializeObject(data));
            if (data.libraries != null)
            {
                if (data.version.AdditionalInstaller != null)
                {
                    var baseLibs = new Dictionary<string, LibInfo>();
                    var additionalLibs = new Dictionary<string, LibInfo>();

                    // в этом цикле разъединяем либрариесы и дополнительный отправляем в отадельные файлы
                    foreach (var key in data.libraries.Keys)
                    {
                        LibInfo value = data.libraries[key];
                        if (value.additionalInstallerType == null)
                        {
                            baseLibs[key] = value;
                        }
                        else
                        {
                            additionalLibs[key] = value;
                        }
                    }

                    SaveFile(_withDirectory.DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json", JsonConvert.SerializeObject(baseLibs));

                    if (additionalLibs != null && additionalLibs.Count > 0)
                    {
                        SaveFile(_withDirectory.DirectoryPath + "/versions/additionalLibraries/" + data.version.AdditionalInstaller.GetLibName + ".json", JsonConvert.SerializeObject(additionalLibs));
                    }
                }
                else
                {
                    SaveFile(_withDirectory.DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json", JsonConvert.SerializeObject(data.libraries));
                }
            }
        }

        public VersionManifest GetManifest(string instanceId, bool includingLibraries)
        {
            VersionManifest data = TryGetFile<VersionManifest>(instanceId, MANIFEST_FILE, MANIFEST_FILE_OLD);
            if (data == null) return null;

            if (includingLibraries)
            {
                var librariesData = GetFile<Dictionary<string, LibInfo>>(_withDirectory.DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json") ?? new Dictionary<string, LibInfo>();

                var installer = data.version?.AdditionalInstaller;
                if (installer != null)
                {
                    var additionallibrarieData = GetFile<Dictionary<string, LibInfo>>(_withDirectory.DirectoryPath + "/versions/additionalLibraries/" + installer?.GetLibName + ".json");

                    if (additionallibrarieData != null)
                    {
                        foreach (var lib in additionallibrarieData.Keys)
                        {
                            librariesData[lib] = additionallibrarieData[lib];
                            librariesData[lib].additionalInstallerType = installer.type;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                data.libraries = librariesData;
            }

            return data;
        }

        private T TryGetFile<T>(string instanceId, string fileName, string oldFileName)
        {
            string filePath = _withDirectory.GetInstancePath(instanceId) + fileName;

            var data = GetFile<T>(filePath);
            if (data == null)
            {
                string oldFilePath = _withDirectory.GetInstancePath(instanceId) + oldFileName;
                data = GetFile<T>(oldFilePath);
                if (data == null) return default(T);

                try
                {
                    File.Move(oldFilePath, filePath);
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                }
            }

            return data;
        }

        public InstalledAddonsFormat GetInstalledAddons(string instanceId)
        {
            var data = TryGetFile<InstalledAddonsFormat>(instanceId, INSTALLED_ADDONS_FILE, INSTALLED_ADDONS_FILE_OLD);
            return data ?? new InstalledAddonsFormat();
        }

        public void SaveInstalledAddons(string instanceId, InstalledAddonsFormat data)
        {
            string path = _withDirectory.GetInstancePath(instanceId) + INSTALLED_ADDONS_FILE;
            SaveFile(path, JsonConvert.SerializeObject(data));
        }

        public InstancePlatformData GetPlatfromData(string instanceId)
        {
            // Это должно было делаться методом TryGetFile, но из-за косяка с выпоском обновы пришлось делать этот костыль.
            // TODO: где-нибудь через 2 месяца, когда большая часть пользователей уже запустит нвоый лаунчер и этот код откработает,
            // то можно возвращаться на TryGetFile
            string filePath = _withDirectory.GetInstancePath(instanceId) + INSTANCE_PLATFORM_DATA_FILE;

            var data = GetFile<InstancePlatformData>(filePath);
            if (data == null || string.IsNullOrWhiteSpace(data.id) || string.IsNullOrWhiteSpace(data.instanceVersion))
            {
                string oldFilePath = _withDirectory.GetInstancePath(instanceId) + INSTANCE_PLATFORM_DATA_FILE_OLD;
                data = GetFile<InstancePlatformData>(oldFilePath);
                if (data == null) return default(InstancePlatformData);

                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Move(oldFilePath, filePath);
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                }
            }

            return data;
        }

        public T GetExtendedPlatfromData<T>(string instanceId) where T : InstancePlatformData
        {
            return TryGetFile<T>(instanceId, INSTANCE_PLATFORM_DATA_FILE, INSTANCE_PLATFORM_DATA_FILE_OLD);
        }

        public void SavePlatfromData(string instanceId, InstancePlatformData content)
        {
            SaveFile(_withDirectory.GetInstancePath(instanceId) + INSTANCE_PLATFORM_DATA_FILE, JsonConvert.SerializeObject(content));
        }

        public InstanceContentFile GetInstanceContent(string instanceId)
        {
            return TryGetFile<InstanceContentFile>(instanceId, INSTANCE_CONTENT_FILE, INSTANCE_CONTENT_FILE_OLD);
        }

        public void SaveInstanceContent(string instanceId, InstanceContentFile content)
        {
            SaveFile(_withDirectory.GetInstancePath(instanceId) + INSTANCE_CONTENT_FILE, JsonConvert.SerializeObject(content));
        }

        public string CreateAcceptableGamePath(string path, out bool newDirIsEmpty)
        {
            newDirIsEmpty = true;
            try
            {
                // заменяем обратный слеш на нормальный слеш
                path = path.Replace('\\', '/');
                // сокращаем n-ное количество слешей до 1
                path = Regex.Replace(path, @"\/+", "/").Trim();
                // убираем слеш в конце
                path = path.TrimEnd('/');

                if (!Directory.Exists(path) || DirectoryHelper.IsDirectoryEmpty(path)) return path;

                string instancesPath = path + "/instances";
                if (!Directory.Exists(instancesPath)) return _withDirectory.CreateValidPath(path);

                int directoryCount = Directory.GetDirectories(instancesPath).Length;
                if (directoryCount < 1) return _withDirectory.CreateValidPath(path);

                if (!File.Exists(path + "/instanesList.json")) return _withDirectory.CreateValidPath(path);

                var data = GetFile<InstalledInstancesFormat>(path + "/instanesList.json");
                if (data == null) return _withDirectory.CreateValidPath(path);

                newDirIsEmpty = false;
                return path;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);

                return _withDirectory.CreateValidPath(path);
            }
        }

        public HashSet<InstalledInstancesGroup> GetGroups()
        {
            return GetFile<HashSet<InstalledInstancesGroup>>($"{_withDirectory.DirectoryPath}/{INSTANCES_GROUPS_FILE}") ?? new();
        }

        public void SaveGroupInfo(InstalledInstancesGroup instanceGroup)
        {
            var allGroups = GetGroups();
            allGroups.Remove(instanceGroup);
            allGroups.Add(instanceGroup);

            SaveFile($"{_withDirectory.DirectoryPath}/{INSTANCES_GROUPS_FILE}", JsonConvert.SerializeObject(allGroups));
        }

        public void RewriteGroupsInfo(IEnumerable<InstalledInstancesGroup> clients)
        {
            SaveFile($"{_withDirectory.DirectoryPath}/{INSTANCES_GROUPS_FILE}", JsonConvert.SerializeObject(clients));
        }

        public long GetLastViewedNewsId()
        {
            string content = GetFile(LaunсherSettings.LauncherDataPath + "/lastViewedNewsId");
            if (content == null) return -1;
            long id = -1;
            long.TryParse(content, out id);

            return id;
        }

        public void SaveLastViewedNewsId(long id)
        {
            SaveFile(LaunсherSettings.LauncherDataPath + "/lastViewedNewsId", id.ToString());
        }

    }
}
