using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Net;
using System.Collections.Concurrent;
using System;
using Tommy;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using static Lexplosion.Logic.Network.CurseforgeApi;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstanceAddon : VMBase
    {
        private const string UnknownName = "Без названия";

        #region info
        public string Name { get; private set; } = "";
        public string Author { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool UpdateAvailable { get; private set; } = false;
        public string WebsiteUrl { get; private set; } = null;
        public string FileName { get; private set; } = "";
        public string Version { get; private set; } = "";
        public int DownloadCount { get; private set; } = 0;
        public string LastUpdated { get; private set; } = "";

        private bool _isEnable = true;
        public bool IsEnable 
        {
            get => _isEnable;
            set
            {
                Disable();
                _isEnable = value;
            }
        }

        private bool _isInstalled = false;
        public bool IsInstalled
        {
            get => _isInstalled;
            private set
            {
                _isInstalled = value;
                OnPropertyChanged();
            }
        }

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
        #endregion

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

        /// <summary>
        /// Тут хранится список аддонов из метода GetAddonsCatalog. При каждом вызове GetAddonsCatalog этот список обновляется.
        /// </summary>
        private static Dictionary<int, InstanceAddon> _addonsCatalogChache;
        /// <summary>
        /// Аддоны, которые устанавливаются в данный момент. После окончания установки они удаляются из этого списка.
        /// </summary>
        private static ConcurrentDictionary<int, Pointer<InstanceAddon>> _installingAddons = new ConcurrentDictionary<int, Pointer<InstanceAddon>>(); // TODO: для ключа еще использвтаь модпак, а не только id
        private static KeySemaphore<int> _installingSemaphore = new KeySemaphore<int>();
        private static Semaphore _chacheSemaphore = new Semaphore(1, 1);

        /// <summary>
        /// Очищает сохранённый список аддонов. Нужно вызывать при закрытии каталога чтобы очистить память.
        /// </summary>
        public static void ClearAddonsListCache()
        {
            _chacheSemaphore.WaitOne();
            _addonsCatalogChache = null;
            _chacheSemaphore.Release();
        }

        /// <summary>
        /// Возвращает список аддонов с курсфорджа.
        /// </summary>
        /// <param name="modpackInfo">Класс BaseInstanceData, описывающий модпак, для которого нужно получить каталог адднов.</param>
        /// <param name="pageSize"Размер страницы></param>
        /// <param name="index">Индекс</param>
        /// <param name="type">Тип аддона</param>
        /// <param name="category">Категория. По умолчанию -1 (при -1 все категории)</param>
        /// <param name="searchFilter">Поиск названия</param>
        /// <returns>Собстна список аддонов.</returns>
        public static List<InstanceAddon> GetAddonsCatalog(BaseInstanceData modpackInfo, int pageSize, int index, AddonType type, int category = -1, string searchFilter = "")
        {
            _addonsCatalogChache = new Dictionary<int, InstanceAddon>();

            string instanceId = modpackInfo.LocalId;
            var addons = new List<InstanceAddon>();

            // получаем спсиок всех аддонов с курсфорджа
            List<CurseforgeAddonInfo> addonsList = CurseforgeApi.GetAddonsList(pageSize, index, type, category, modpackInfo.Modloader, searchFilter, modpackInfo.GameVersion);

            // получаем список установленных аддонов
            var installedAddons = GetInstalledAddons(instanceId);

            // проходимся по аддонам с курсфорджа
            int i = 0;
            foreach (CurseforgeAddonInfo addon in addonsList)
            {
                int addonId = addon.id;
                bool isInstalled =
                    (installedAddons.ContainsKey(addonId) &&
                    File.Exists(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddons[addonId].ActualPath));

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

                InstanceAddon instanceAddon;
                _installingSemaphore.WaitOne(addonId);
                if (_installingAddons.ContainsKey(addonId))
                {
                    if (_installingAddons[addonId].Point == null)
                    {
                        instanceAddon = new InstanceAddon(addon, modpackInfo)
                        {
                            Description = addon.summary,
                            Name = addon.name,
                            IsInstalled = isInstalled,
                            Author = addon.GetAuthorName,
                            WebsiteUrl = addon.links.websiteUrl,
                            UpdateAvailable = (installedAddons.ContainsKey(addonId) && (installedAddons[addonId].FileID < lastFileID)), // если установленная версия аддона меньше последней - значит доступно обновление
                            DownloadCount = (int)addon.downloadCount,
                            LastUpdated = DateTime.Parse(addon.dateModified).ToString("dd MMM yyyy")
                        };

                        _installingAddons[addonId].Point = instanceAddon;
                        instanceAddon.IsInstalling = true;
                    }
                    else
                    {
                        instanceAddon = _installingAddons[addon.id].Point;
                    }
                }
                else
                {
                    instanceAddon = new InstanceAddon(addon, modpackInfo)
                    {
                        Description = addon.summary,
                        Name = addon.name,
                        IsInstalled = isInstalled,
                        Author = addon.GetAuthorName,
                        WebsiteUrl = addon.links.websiteUrl,
                        UpdateAvailable = (installedAddons.ContainsKey(addonId) && (installedAddons[addonId].FileID < lastFileID)), // если установленная версия аддона меньше последней - значит доступно обновление
                        DownloadCount = (int)addon.downloadCount,
                        LastUpdated = DateTime.Parse(addon.dateModified).ToString("dd MMM yyyy")
                    };
                }
                _installingSemaphore.Release(addonId);

                instanceAddon.DownloadLogo(addon.logo.url);

                addons.Add(instanceAddon);
                _addonsCatalogChache[addonId] = instanceAddon;
                i++;
            }

            return addons;
        }

        private void DeleteAddon()
        {
            //try
            //{
            //    string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddon.ActualPath;

            //    if (File.Exists(path))
            //    {
            //        File.Delete(path);
            //    }
            //}
            //catch { }
        }

        private bool InstallAddon(CurseforgeFileInfo addonInfo)
        {
            string instanceId = _modpackInfo.LocalId;
            var installedAddons = GetInstalledAddons(instanceId);

            _installingSemaphore.WaitOne(_modInfo.id);
            _installingAddons[_modInfo.id] = new Pointer<InstanceAddon>
            {
                Point = this
            };
            _installingSemaphore.Release(_modInfo.id);
            IsInstalling = true; 

            var ressult = CurseforgeApi.DownloadAddon(addonInfo, (AddonType)_modInfo.classId, "instances/" + instanceId + "/", delegate (int percentages)
            {
                DownloadPercentages = percentages;
            });

            IsInstalling = false;
            _installingSemaphore.WaitOne(_modInfo.id);
            _installingAddons.TryRemove(_modInfo.id, out _);
            _installingSemaphore.Release(_modInfo.id);

            if (ressult.Value2 == DownloadAddonRes.Successful)
            {
                IsInstalled = true;
                installedAddons[addonInfo.modId] = ressult.Value1;

                //так же скачиваем зависимости
                if (addonInfo.dependencies.Count > 0)
                {
                    List<Dictionary<string, int>> dependencies = addonInfo.dependencies;

                    // проходимся по спику завиисимостей и скачиваем все зависимые аддоны
                    int i = 0, count = dependencies.Count;
                    while (i < count)
                    {
                        var value = dependencies[i];
                        if (value.ContainsKey("relationType") && value["relationType"] == 3 && value.ContainsKey("modId"))
                        {
                            List<CurseforgeFileInfo> files = CurseforgeApi.GetProjectFiles(value["modId"].ToString());
                            var file = GetLastFile(_modpackInfo.GameVersion, files);
                            if (file != null)
                            {
                                Pointer<InstanceAddon> addonPointer = new Pointer<InstanceAddon>();
                                addonPointer.Point = null;
                                _chacheSemaphore.WaitOne();
                                if (_addonsCatalogChache.ContainsKey(file.modId))
                                {
                                    addonPointer.Point = _addonsCatalogChache[file.modId];
                                    addonPointer.Point.IsInstalling = true;
                                }
                                _chacheSemaphore.Release();

                                _installingSemaphore.WaitOne(file.modId);
                                _installingAddons[file.modId] = addonPointer;
                                _installingSemaphore.Release(file.modId);

                                var res = CurseforgeApi.DownloadAddon(file, (AddonType)_modInfo.classId, "instances/" + instanceId + "/", delegate (int percentages)
                                {
                                    if (addonPointer.Point != null)
                                    {
                                        addonPointer.Point.DownloadPercentages = percentages;
                                    }
                                });

                                if (addonPointer.Point != null)
                                {
                                    addonPointer.Point.IsInstalling = false;
                                    _installingSemaphore.WaitOne(file.modId);
                                    _installingAddons.TryRemove(file.modId, out _);
                                    _installingSemaphore.Release(file.modId);
                                }
                                
                                if (res.Value2 == DownloadAddonRes.Successful)
                                {
                                    IsInstalled = true;
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

        class McmodInfo
        {
            public string modid;
            public string name;
            public string description;
            public string version;
            public List<string> authorList;
            public List<string> authors;
        }

        class McmodInfoContainer
        {
            public List<McmodInfo> modList;
        }

        class FabricModJson
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
                else if (table[parameter].IsString)
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
                    string displayName = UnknownName, authors = "", version = "", description = "", modId = "";

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
                                        if (table != null)
                                        {
                                            displayName = getParameterValue(table, "displayName");
                                            authors = getParameterValue(table, "authors");
                                            version = getParameterValue(table, "version");
                                            description = getParameterValue(table, "description");
                                            modId = getParameterValue(table, "modId");
                                        }
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
                                            List<McmodInfo> data;
                                            try
                                            {
                                                data = JsonConvert.DeserializeObject<List<McmodInfo>>(text);
                                            }
                                            catch
                                            {
                                                data = JsonConvert.DeserializeObject<McmodInfoContainer>(text)?.modList;
                                            }

                                            if (data != null && data.Count > 0)
                                            {
                                                McmodInfo modInfo = data[0];
                                                displayName = modInfo?.name ?? "";
                                                version = modInfo?.version ?? "";
                                                description = modInfo?.description ?? "";
                                                modId = modInfo?.modid ?? "";
                                                authors = (modInfo?.authorList != null && modInfo.authorList.Count > 0) ? modInfo.authorList[0] : ((modInfo?.authors != null && modInfo.authors.Count > 0) ? modInfo.authors[0] : "");
                                            }
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
                                                FabricModJson modInfo = JsonConvert.DeserializeObject<FabricModJson>(text);
                                                displayName = modInfo?.name ?? "";
                                                version = modInfo?.version ?? "";
                                                description = modInfo?.description ?? "";
                                                modId = modInfo?.id ?? "";
                                                authors = (modInfo?.authors != null && modInfo.authors.Count > 0) ? modInfo.authors[0] : "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    int addonId;
                    string xyi = fileAddr_.Replace(WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/", "");
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

                    string filename = "";
                    try
                    {
                        filename = Path.GetFileName(fileAddr);
                    } catch { }

                    addons.Add(new InstanceAddon(addonId, modpackInfo)
                    {
                        Author = authors,
                        Description = description,
                        Name = displayName,
                        FileName = filename,
                        Version = (!version.Contains("{") ? version : "")
                    });
                }
            }

            SaveInstalledAddons(modpackInfo.LocalId, actualAddonsList);

            return addons;
        }

        public static List<InstanceAddon> GetInstalledResourcepacks(BaseInstanceData modpackInfo)
        {
            return new List<InstanceAddon>();
        }

        public static List<InstanceAddon> GetInstalledWorlds(BaseInstanceData modpackInfo)
        {
            return new List<InstanceAddon>();
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

        private void Disable()
        {
            int projectID = _projectId;
            string instanceId = _modpackInfo.LocalId;

            var installedAddons = GetInstalledAddons(instanceId);
            if (installedAddons.ContainsKey(projectID))
            {
                //try
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
                //catch { }
            }
        }
    }
}
