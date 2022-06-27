using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Tommy;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.FileSystem;
using static Lexplosion.Logic.Network.CurseforgeApi;
using System.Threading;
using System.Net;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstanceAddon : VMBase
    {
        public string Name { get; private set; } = "";
        public string Author { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool IsInstalled { get; private set; } = false;
        public bool UpdateAvailable { get; private set; } = false;
        public string WebsiteUrl { get; private set; } = null;

        public byte[] _logo = null;

        public byte[] Logo
        {
            get => _logo;
            set
            {
                _logo = value;
                OnPropertyChanged();
            }
        }

        private bool _isInstalling = false;
        public bool IsInstalling 
        {
            get
            {
                return _isInstalling;
            }
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
            }
        }

        private int _downloadPercentages = 0;
        public int DownloadPercentages
        {
            get
            {
                return _downloadPercentages;
            }

            set
            {
                _downloadPercentages = value;
                OnPropertyChanged();
            }
        }

        private readonly CurseforgeAddonInfo _modInfo;
        private readonly BaseInstanceData _modpackInfo;
        private readonly int _projectId;
        private readonly string _gameVersion;

        private InstanceAddon(CurseforgeAddonInfo modInfo, BaseInstanceData modpackInfo)
        {
            _modInfo = modInfo;
            _projectId = modInfo.id;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion;
        }

        private InstanceAddon(int projectId, BaseInstanceData modpackInfo)
        {
            _modInfo = null;
            _projectId = projectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion;
        }

        private static InstalledAddons GetInstalledAddons(string instanceId)
        {
            var data = DataFilesManager.GetFile<InstalledAddons>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json");
            if (data == null)
                return new InstalledAddons();
            return data;
        }

        private static void SaveInstalledAddons(string instanceId, InstalledAddons data)
        {
            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json", JsonConvert.SerializeObject(data));
        }

        public static List<InstanceAddon> GetAddonsCatalog(BaseInstanceData modpackInfo, int pageSize, int index, AddonType type, int category = -1, string searchFilter = "")
        {
            string instanceId = modpackInfo.LocalId;
            var addons = new List<InstanceAddon>();

            // получаем спсиок всех аддонов с курсфорджа
            List<CurseforgeAddonInfo> addonsList = CurseforgeApi.GetAddonsList(pageSize, index, type, category, searchFilter, modpackInfo.GameVersion);
            if (addonsList == null)
                return addons;

            // получаем список установленных аддонов
            var installedAddons = GetInstalledAddons(instanceId);

            // проходимся по аддонам с курсфорджа
            int i = 0;
            foreach (CurseforgeAddonInfo addon in addonsList)
            {
                bool isInstalled =
                    (installedAddons.ContainsKey(addon.id) &&
                    File.Exists(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddons[addon.id].ActualPath));

                int lastFileID = 0;
                if (isInstalled)
                {
                    // ищем последнюю версию аддона
                    foreach (var addonVersion in addon.latestFilesIndexes)
                    {
                        if (addonVersion.gameVersion == modpackInfo.GameVersion)
                        {
                            lastFileID = addonVersion.fileId;
                            break;
                        }
                    }
                }

                var instanceAddon = new InstanceAddon(addon, modpackInfo)
                {
                    Description = addon.summary,
                    Name = addon.name,
                    IsInstalled = isInstalled,
                    Author = addon.GetAuthorName,
                    WebsiteUrl = addon.links.websiteUrl,
                    UpdateAvailable = (installedAddons.ContainsKey(addon.id) && (installedAddons[addon.id].FileID < lastFileID)) // если установленная версия аддона меньше последней - значит доступно обновление
                };

                instanceAddon.DownloadLogo(addon.logo.url);

                addons.Add(instanceAddon);
                i++;
            }

            return addons;
        }

        private void DeleteAddon(string instanceId, int projectID, InstalledAddonInfo installedAddon)
        {
            try
            {
                string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddon.ActualPath;

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        private bool InstallAddon(CurseforgeFileInfo addonInfo)
        {
            string instanceId = _modpackInfo.LocalId;
            var installedAddons = GetInstalledAddons(instanceId);

            IsInstalling = true;
            var ressult = CurseforgeApi.DownloadAddon(addonInfo, (AddonType)_modInfo.classId, "instances/" + instanceId + "/", delegate (int percentages)
            {
                DownloadPercentages = percentages;
            });
            IsInstalling = false;

            if (ressult.Value2 == DownloadAddonRes.Successful)
            {
                installedAddons[addonInfo.modId] = ressult.Value1;

                //так же скачиваем зависимости
                if (addonInfo.dependencies.Count > 0)
                {
                    List<Dictionary<string, int>> dependencies = addonInfo.dependencies;

                    // проходимся по спику завиисимостей и скачиваем все зависимые аддоны
                    int i = 0, count = dependencies.Count;
                    while(i < count)
                    {
                        var value = dependencies[i];
                        if (value.ContainsKey("relationType") && value["relationType"] == 3 && value.ContainsKey("modId"))
                        {
                            Console.WriteLine("download " + value["modId"]);
                            List<CurseforgeFileInfo> files = CurseforgeApi.GetProjectFiles(value["modId"].ToString());
                            var file = GetLastFile(_modpackInfo.GameVersion, files);
                            if (file != null)
                            {
                                var res = CurseforgeApi.DownloadAddon(file, (AddonType)_modInfo.classId, "instances/" + instanceId + "/", delegate (int percentages) { });
                                if (res.Value2 == DownloadAddonRes.Successful)
                                {
                                    installedAddons[res.Value1.ProjectID] = res.Value1;

                                    // в список зависимостей добавляем зависимости и этого аддона, если они есть
                                    if (file.dependencies.Count > 0)
                                    {
                                        foreach (var val in file.dependencies)
                                        {
                                            dependencies.Add(val);
                                            i++;
                                        }
                                    }
                                }
                            }
                        }

                        i++;
                    }
                }

                SaveInstalledAddons(instanceId, installedAddons);
            }

            return true;
        }

        private static CurseforgeFileInfo GetLastFile(string gameVersion, List<CurseforgeFileInfo> addonInfo)
        {
            CurseforgeFileInfo file = null;
            int maxId = 0;
            foreach (var fileInfo in addonInfo)
            {
                if (fileInfo.gameVersions != null && fileInfo.gameVersions.Contains(gameVersion) && maxId < fileInfo.id)
                {
                    file = fileInfo;
                    maxId = fileInfo.id;
                    break;
                }
            }

            return file;
        }

        public void InstallLatestVersion()
        {
            var file = GetLastFile(_modpackInfo.GameVersion, _modInfo.latestFiles);
            if (file == null)
            {
                file = GetLastFile(_modpackInfo.GameVersion, CurseforgeApi.GetProjectFiles(_modInfo.id.ToString()));
                if (file != null)
                {
                    InstallAddon(file);
                }
            }
            else
            {
                InstallAddon(file);
            }
        }

        class mcmodInfo
        {
            public string modid;
            public string name;
            public string description;
            public string version;
            public List<string> authorList;
        }

        class fabricModJson
        {
            public string id;
            public string name;
            public string description;
            public string version;
            public List<string> authors;
        }

        /// <summary>
        /// Возвращает список модов. При вызове так же сохраняет спсиок модов, 
        /// анализирует папку mods и пихает в список моды которые были в папке, но которых не было в списке.
        /// </summary>
        /// <param name="modpackInfo">Инфа о модпаке с которого нужно получить список модов</param>
        public static List<InstanceAddon> GetInstalledMods(BaseInstanceData modpackInfo)
        {
            string getParameterValue(TomlTable table, string parameter)
            {
                if (table["mods"][0][parameter].IsString)
                    return table["mods"][0][parameter];
                else if (table[parameter])
                    return table[parameter];
                else
                    return "";
            }

            string modsPath = WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/mods/";
            List<InstanceAddon> addons = new List<InstanceAddon>();

            InstalledAddons actualAddonsList = new InstalledAddons(); //актуальный список аддонов, то есть те аддоны которы действительно существует. В конце именно этот спсиок будет сохранен в файл
            var existsAddons = new Dictionary<string, int>(); // ключ - имя файла, значение - айди. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке installedAddons.

            // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
            // если он существует, то добавляем в existsAddons и actualAddonsList.
            InstalledAddons installedAddons = GetInstalledAddons(modpackInfo.LocalId);
            foreach (int installedAddonId in installedAddons.Keys)
            {
                InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                if (installedAddon.Type == AddonType.Mods) // с модами нужно поебаться и проверить
                {
                    if (File.Exists(modsPath + "/" + installedAddon.ActualPath))
                    {
                        actualAddonsList[installedAddonId] = installedAddon; // аддон действительно существует, добавляем в список
                        existsAddons[installedAddon.ActualPath] = installedAddonId;
                    }
                }
                else //всё остальное не тогаем. Просто перекидывеам в новый список
                {
                    existsAddons[installedAddon.ActualPath] = installedAddonId;
                }
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(modsPath, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                return addons;
            }

            int generatedAddonId = -1; // тут хранится следующий следющий сгенерированный айдишник. По сути переменная нужна чисто для оптимизации
            // Теперь проходмся по всем файлам в папке
            foreach (string fileAddr in files)
            {
                string fileAddr_ = fileAddr.Replace('\\', '/');
                string extension = Path.GetExtension(fileAddr_);
                if (extension == ".jar" || extension == ".disable")
                {
                    string displayName = "", authors = "", version = "", description = "", modId = "";

                    // тут пытаемся получить инфу о моде
                    try
                    {
                        using (ZipArchive zip = ZipFile.Open(fileAddr_, ZipArchiveMode.Read))
                        {
                            ZipArchiveEntry entry = zip.GetEntry("META-INF/mods.toml");
                            if (entry != null)
                            {
                                using (Stream file = entry.Open())
                                {
                                    using (TextReader text = new StreamReader(file))
                                    {
                                        TomlTable table = TOML.Parse(text);
                                        displayName = getParameterValue(table, "displayName");
                                        authors = getParameterValue(table, "authors");
                                        version = getParameterValue(table, "version");
                                        description = getParameterValue(table, "description");
                                        modId = getParameterValue(table, "modId");
                                    }
                                }
                            }
                            else
                            {
                                entry = zip.GetEntry("mcmod.info");
                                if (entry != null)
                                {
                                    using (Stream file = entry.Open())
                                    {
                                        using (StreamReader reader = new StreamReader(file, encoding: Encoding.UTF8))
                                        {
                                            string text = reader.ReadToEnd();
                                            mcmodInfo modInfo = JsonConvert.DeserializeObject<mcmodInfo>(text);
                                            displayName = modInfo?.name ?? "";
                                            version = modInfo?.version ?? "";
                                            description = modInfo?.description ?? "";
                                            modId = modInfo?.modid ?? "";
                                            authors = (modInfo?.authorList != null) ? string.Join(", ", modInfo.authorList) : "";
                                        }
                                    }

                                }
                                else
                                {
                                    if (entry != null)
                                    {
                                        entry = zip.GetEntry("fabric.mod.json");
                                        using (Stream file = entry.Open())
                                        {
                                            using (StreamReader reader = new StreamReader(file, encoding: Encoding.UTF8))
                                            {
                                                string text = reader.ReadToEnd();
                                                fabricModJson modInfo = JsonConvert.DeserializeObject<fabricModJson>(text);
                                                displayName = modInfo?.name ?? "";
                                                version = modInfo?.version ?? "";
                                                description = modInfo?.description ?? "";
                                                modId = modInfo?.id ?? "";
                                                authors = (modInfo?.authors != null) ? string.Join(", ", modInfo.authors) : "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    int addonId;
                    string xyi = fileAddr_.Replace(WithDirectory.DirectoryPath + "/" + modpackInfo.LocalId + "/", "");
                    if (!existsAddons.ContainsKey(xyi)) // аддон есть в папке, но нет в списке, нужно добавить, так же генерируем айдишник для него
                    {
                        // собстна генерируем айдишник
                        addonId = generatedAddonId;
                        while (existsAddons.ContainsKey(xyi))
                        {
                            addonId--;
                        }
                        generatedAddonId = addonId - 1;

                        actualAddonsList[addonId] = new InstalledAddonInfo
                        {
                            FileID = -1,
                            ProjectID = addonId,
                            Type = AddonType.Mods,
                            IsDisable = (extension == ".disable"),
                            Path = (extension == ".jar") ? xyi : xyi.Remove(xyi.Length - 8) // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                        };
                    }
                    else // аддон есть везде, берём его айдишник
                    {
                        addonId = existsAddons[xyi];
                    }

                    addons.Add(new InstanceAddon(addonId, modpackInfo)
                    {
                        Author = authors,
                        Description = description,
                        Name = displayName,
                    });
                }
            }


            return addons;
        }

        /// <summary>
        /// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo. Делает это всё в пуле потоков.
        /// </summary>
        private void DownloadLogo(string url)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                //try
                {
                    using (var webClient = new WebClient())
                    {
                        Logo = webClient.DownloadData(url);
                    }
                }
                //catch { }
            });
        }

        public void Disable()
        {
            int projectID = _projectId;
            string instanceId = _modpackInfo.LocalId;

            var installedAddons = GetInstalledAddons(instanceId);
            if (installedAddons.ContainsKey(projectID))
            {
                try
                {
                    var installedAddon = installedAddons[projectID];
                    if (installedAddon.IsDisable)
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (File.Exists(dir + installedAddon.ActualPath))
                        {
                            File.Move(dir + installedAddon.ActualPath, dir + installedAddon.Path);
                            installedAddon.IsDisable = false;

                            SaveInstalledAddons(instanceId, installedAddons);
                        }
                    }
                    else
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (File.Exists(dir + installedAddon.Path))
                        {
                            installedAddon.IsDisable = true;
                            File.Move(dir + installedAddon.Path, dir + installedAddon.ActualPath);

                            SaveInstalledAddons(instanceId, installedAddons);
                        }
                    }
                }
                catch { }
            }
        }
    }
}
