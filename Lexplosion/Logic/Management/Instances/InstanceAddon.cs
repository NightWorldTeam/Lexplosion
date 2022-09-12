using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Net;
using System.Collections.Concurrent;
using System;
using System.Linq;
using Tommy;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstanceAddon : VMBase
    {
        private const string UnknownName = "Без названия";

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

        #region info
        public string Name { get; private set; } = "";
        public string Author { get; private set; } = "";
        public string Description { get; private set; } = "";
        public string WebsiteUrl { get; private set; } = null;
        public bool IsUrlExist { get; set; }
        public string FileName { get; private set; } = "";
        public string Version { get; private set; } = "";
        public int DownloadCount { get; private set; } = 0;
        public string LastUpdated { get; private set; } = "";

        private bool _updateAvailable = false;
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            private set
            {
                _updateAvailable = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnable = true;
        public bool IsEnable 
        {
            get => _isEnable;
            set
            {
                _isEnable = value;
                Disable();
                OnPropertyChanged();
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
            Console.WriteLine("CHACHE");
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
            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // проходимся по аддонам с курсфорджа
                int i = 0;
                foreach (CurseforgeAddonInfo addon in addonsList)
                {
                    int addonId = addon.id;
                    bool isInstalled = (installedAddons.ContainsKey(addonId) && installedAddons[addonId].IsExists(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/"));

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
                    _chacheSemaphore.WaitOne();
                    if (_addonsCatalogChache != null)
                    {
                        _addonsCatalogChache[addonId] = instanceAddon;
                    }
                    _chacheSemaphore.Release();

                    i++;
                }
            }

            return addons;
        }

        private void DeleteAddon()
        {
            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                InstalledAddonInfo addon = installedAddons[_modInfo.id];
                installedAddons.TryRemove(_modInfo.id);
                addon.RemoveFromDir(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/");
                installedAddons.Save();
            }
        }

        private DownloadAddonRes InstallAddon(CurseforgeFileInfo addonInfo, bool downloadDependencies, out Dictionary<string, DownloadAddonRes> dependenciesResults)
        {
            dependenciesResults = null;
            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                _installingSemaphore.WaitOne(_modInfo.id);
                _installingAddons[_modInfo.id] = new Pointer<InstanceAddon>
                {
                    Point = this
                };
                _installingSemaphore.Release(_modInfo.id);

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

                    // удаляем старый файл
                    if (installedAddons[addonInfo.modId] != null && installedAddons[addonInfo.modId].ActualPath != ressult.Value1.ActualPath)
                    {
                        try
                        {
                            string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                            InstalledAddonInfo installedAddon = installedAddons[addonInfo.modId];
                            if (installedAddon.IsExists(path))
                            {
                                installedAddon.RemoveFromDir(path);
                            }
                        }
                        catch { }
                    }

                    installedAddons[addonInfo.modId] = ressult.Value1;

                    //так же скачиваем зависимости
                    if (addonInfo.dependencies.Count > 0 && downloadDependencies)
                    {
                        List<Dictionary<string, int>> dependencies = addonInfo.dependencies;
                        dependenciesResults = new Dictionary<string, DownloadAddonRes>();

                        // проходимся по спику завиисимостей и скачиваем все зависимые аддоны
                        int i = 0, count = dependencies.Count;
                        while (i < count)
                        {
                            var value = dependencies[i];
                            if (value.ContainsKey("relationType") && value["relationType"] == 3 && value.ContainsKey("modId") && !installedAddons.ContainsKey(value["modId"]))
                            {
                                List<CurseforgeFileInfo> files = CurseforgeApi.GetProjectFiles(value["modId"].ToString(), _gameVersion, _modpackInfo.Modloader);
                                var file = GetLastFile(_modpackInfo.GameVersion, files, _modpackInfo);
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
                                        if (addonPointer.Point != null)
                                        {
                                            addonPointer.Point.IsInstalled = true;
                                        }

                                        // удаляем старый файл на всякий случай
                                        if (installedAddons[res.Value1.ProjectID] != null && installedAddons[res.Value1.ProjectID].ActualPath != res.Value1.ActualPath)
                                        {
                                            try
                                            {
                                                string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                                                InstalledAddonInfo installedAddon = installedAddons[res.Value1.ProjectID];
                                                if (installedAddon.IsExists(path))
                                                {
                                                    installedAddon.RemoveFromDir(path);
                                                }
                                            }
                                            catch { }
                                        }

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

                                    dependenciesResults[file.displayName] = res.Value2;
                                }
                            }

                            i++;
                        }
                    }
                }
                else
                {
                    return ressult.Value2;
                }

                installedAddons.Save();
            }

            return DownloadAddonRes.Successful;
        }

        private static CurseforgeFileInfo GetLastFile(string gameVersion, List<CurseforgeFileInfo> addonInfo, BaseInstanceData instanceData)
        {
            CurseforgeFileInfo file = null;
            if (addonInfo != null)
            {
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
            }

            return file;
        }

        public DownloadAddonRes InstallLatestVersion(out Dictionary<string, DownloadAddonRes> dependenciesResults, bool downloadDependencies = true)
        {
            IsInstalling = true;
            var file = GetLastFile(_modpackInfo.GameVersion, _modInfo?.latestFiles, _modpackInfo);
            if (file == null)
            {
                file = GetLastFile(_modpackInfo.GameVersion, CurseforgeApi.GetProjectFiles(_modInfo.id.ToString(), _gameVersion, _modpackInfo.Modloader), _modpackInfo);
                if (file != null)
                {
                    return InstallAddon(file, downloadDependencies, out dependenciesResults);
                }
                else
                {
                    IsInstalling = false;
                }

                dependenciesResults = null;
            }
            else
            {
                return InstallAddon(file, downloadDependencies, out dependenciesResults);
            }

            return DownloadAddonRes.FileVersionError;
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

            string clientPath = WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/";
            List<InstanceAddon> addons = new List<InstanceAddon>();

            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            HashSet<int> existsCfMods = new HashSet<int>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsAddons = new Dictionary<string, ValuePair<InstanceAddon, int>>(); // ключ - имя файла, значение - экзмепляр и айдишник. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.

            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
                // если он существует, то добавляем в existsAddons и actualAddonsList.
                foreach (int installedAddonId in installedAddons.Keys)
                {
                    InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                    if (installedAddon.Type == AddonType.Mods) // с модами нужно поебаться и проверить
                    {
                        // Пока что пихаем только скачанные с курсфорджа моды. если айдишник больше -1, то аддон скачан с курсфорджа
                        if (installedAddon.IsExists(clientPath) && installedAddonId > -1)
                        {
                            existsCfMods.Add(installedAddonId);
                            actualAddonsList[installedAddonId] = installedAddon;
                            existsAddons[installedAddon.ActualPath] = new ValuePair<InstanceAddon, int>
                            {
                                Value1 = null,
                                Value2 = installedAddonId
                            };
                        }
                    }
                    else //всё остальное не тогаем. Просто перекидывеам в новый список
                    {
                        actualAddonsList[installedAddonId] = installedAddon;
                    }
                }

                // теперь получаем инфу об известных нам модов с курсфорджа
                if (existsCfMods.Count > 0)
                {
                    List<CurseforgeAddonInfo> cfData = CurseforgeApi.GetAddonsInfo(existsCfMods.ToArray());

                    if (cfData != null)
                    {
                        foreach (CurseforgeAddonInfo addon in cfData)
                        {
                            int projectId = addon.id;
                            if (existsCfMods.Contains(projectId))
                            {
                                InstalledAddonInfo info = actualAddonsList[projectId];
                                var obj = new InstanceAddon(addon, modpackInfo)
                                {
                                    Author = addon.GetAuthorName,
                                    Description = addon.summary,
                                    Name = addon.name,
                                    Version = "",
                                    WebsiteUrl = addon.links?.websiteUrl,
                                    IsUrlExist = (addon.links?.websiteUrl != null)
                                };

                                // проверяем наличие обновлений для мода
                                if (modpackInfo.Type == InstanceSource.Local)
                                {
                                    CurseforgeFileInfo lastFile = GetLastFile(modpackInfo.GameVersion, addon.latestFiles, modpackInfo);
                                    if (lastFile != null && lastFile.id > actualAddonsList[projectId].FileID)
                                    {
                                        obj.UpdateAvailable = true;
                                    }
                                }

                                existsAddons[info.ActualPath] = new ValuePair<InstanceAddon, int> // пихаем аддон в этот список именно в этом месте на всякий случай. вдруг долбаебы с курсфорджа вернут мне не весь список, который я запросил
                                {
                                    Value1 = obj,
                                    Value2 = projectId
                                };

                                if (addon.logo != null)
                                {
                                    obj.DownloadLogo(addon.logo.url);
                                }
                                addons.Add(obj);
                            }
                        }
                    }
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(clientPath + "mods", "*.*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    return addons;
                }

                int generatedAddonId = -1; // тут хранится следующий следющий сгенерированный айдишник. По сути переменная нужна чисто для оптимизации
                                           // Теперь проходмся по всем файлам в папке. Создаем InstalledAddonInfo для тех аддонов, которых не было в списке, или инфа о которых не была получена с курсфорджа
                foreach (string fileAddr in files)
                {
                    string fileAddr_ = fileAddr.Replace('\\', '/');
                    string extension = Path.GetExtension(fileAddr_);
                    bool isJar = (extension == ".jar"), isDisable = (extension == ".disable");
                    if (isJar || isDisable)
                    {
                        string xyi = fileAddr_.Replace(WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/", "");

                        string filename = "";
                        try
                        {
                            filename = Path.GetFileName(fileAddr);
                        }
                        catch { }

                        // аддон есть в папке, но нет в списке, или он есть и в папке и в списке, но скачан нее с курсфорджа, то нужно добавить, так же генерируем айдишник для него
                        // ну или просто запрос был не успешным
                        bool notContains = !existsAddons.ContainsKey(xyi);
                        if (notContains || existsAddons[xyi].Value1 == null)
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
                                            entry = zip.GetEntry("fabric.mod.json");
                                            if (entry != null)
                                            {
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

                            // определяем айдишник
                            int addonId;
                            if (notContains) // мод есть в папке, но нет в списке, значит установлен собственноручно
                            {
                                // собстна генерируем айдишник
                                addonId = generatedAddonId;
                                while (actualAddonsList.ContainsKey(addonId))
                                {
                                    addonId--;
                                }
                                generatedAddonId = addonId - 1;
                            }
                            else // мод есть в спсике, берем его айдишник
                            {
                                addonId = existsAddons[xyi].Value2;
                            }

                            actualAddonsList[addonId] = new InstalledAddonInfo
                            {
                                FileID = notContains ? -1 : actualAddonsList[existsAddons[xyi].Value2].FileID,
                                ProjectID = addonId,
                                Type = AddonType.Mods,
                                IsDisable = isDisable,
                                Path = isJar ? xyi : xyi.Remove(xyi.Length - 8) // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                            };

                            addons.Add(new InstanceAddon(addonId, modpackInfo)
                            {
                                Author = authors,
                                Description = description,
                                Name = displayName,
                                FileName = filename,
                                Version = (!version.Contains("{") ? version : ""),
                                _isEnable = isJar
                            });
                        }
                        else
                        {
                            existsAddons[xyi].Value1.FileName = filename;
                            existsAddons[xyi].Value1._isEnable = isJar;
                        }
                    }
                }

                installedAddons.Save();
            }
                
            return addons;
        }

        public static List<InstanceAddon> GetInstalledResourcepacks(BaseInstanceData modpackInfo)
        {
            string clientPath = WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/";
            List<InstanceAddon> addons = new List<InstanceAddon>();

            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            HashSet<int> existsCfMods = new HashSet<int>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsAddons = new Dictionary<string, ValuePair<InstanceAddon, int>>(); // ключ - имя файла, значение - экзмепляр и айдишник. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.

            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
                // если он существует, то добавляем в existsAddons и actualAddonsList.
                foreach (int installedAddonId in installedAddons.Keys)
                {
                    InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                    if (installedAddon.Type == AddonType.Resourcepacks)
                    {
                        // Пока что пихаем только скачанные с курсфорджа моды. если айдишник больше -1, то аддон скачан с курсфорджа
                        if (installedAddon.IsExists(clientPath) && installedAddonId > -1)
                        {
                            existsCfMods.Add(installedAddonId);
                            actualAddonsList[installedAddonId] = installedAddon;
                            existsAddons[installedAddon.ActualPath] = new ValuePair<InstanceAddon, int>
                            {
                                Value1 = null,
                                Value2 = installedAddonId
                            };
                        }
                    }
                    else //всё остальное не тогаем. Просто перекидывеам в новый список
                    {
                        actualAddonsList[installedAddonId] = installedAddon;
                    }
                }

                // теперь получаем инфу об известных нам ресурпаков с курсфорджа
                if (existsCfMods.Count > 0)
                {
                    List<CurseforgeAddonInfo> cfData = CurseforgeApi.GetAddonsInfo(existsCfMods.ToArray());

                    if (cfData != null)
                    {
                        foreach (CurseforgeAddonInfo addon in cfData)
                        {
                            int projectId = addon.id;
                            if (existsCfMods.Contains(projectId))
                            {
                                InstalledAddonInfo info = actualAddonsList[projectId];
                                var obj = new InstanceAddon(addon, modpackInfo)
                                {
                                    Author = addon.GetAuthorName,
                                    Description = addon.summary,
                                    Name = addon.name,
                                    Version = "",
                                    WebsiteUrl = addon.links?.websiteUrl,
                                    IsUrlExist = (addon.links?.websiteUrl != null)
                                };

                                // проверяем наличие обновлений для ресурпака
                                if (modpackInfo.Type == InstanceSource.Local)
                                {
                                    CurseforgeFileInfo lastFile = GetLastFile(modpackInfo.GameVersion, addon.latestFiles, modpackInfo);
                                    if (lastFile != null && lastFile.id > actualAddonsList[projectId].FileID)
                                    {
                                        obj.UpdateAvailable = true;
                                    }
                                }

                                existsAddons[info.ActualPath] = new ValuePair<InstanceAddon, int> // пихаем аддон в этот список именно в этом месте на всякий случай. вдруг долбаебы с курсфорджа вернут мне не весь список, который я запросил
                                {
                                    Value1 = obj,
                                    Value2 = projectId
                                };

                                if (addon.logo != null)
                                {
                                    obj.DownloadLogo(addon.logo.url);
                                }
                                addons.Add(obj);
                            }
                        }
                    }
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(clientPath + "resourcepacks", "*.*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    return addons;
                }

                int generatedAddonId = -1; // тут хранится следующий следющий сгенерированный айдишник. По сути переменная нужна чисто для оптимизации
                                           // Теперь проходмся по всем файлам в папке. Создаем InstalledAddonInfo для тех аддонов, которых не было в списке, или инфа о которых не была получена с курсфорджа
                foreach (string fileAddr in files)
                {
                    string fileAddr_ = fileAddr.Replace('\\', '/');
                    string extension = Path.GetExtension(fileAddr_);
                    bool isZip = (extension == ".zip"), isDisable = (extension == ".disable");
                    if (isZip || isDisable)
                    {
                        string xyi = fileAddr_.Replace(WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/", "");

                        string filename = "";
                        try
                        {
                            filename = Path.GetFileName(fileAddr);
                        }
                        catch { }

                        // аддон есть в папке, но нет в списке, или он есть и в папке и в списке, но скачан нее с курсфорджа, то нужно добавить, так же генерируем айдишник для него
                        // ну или просто запрос был не успешным
                        bool notContains = !existsAddons.ContainsKey(xyi);
                        if (notContains || existsAddons[xyi].Value1 == null)
                        {
                            string displayName = UnknownName, authors = "", version = "", description = "", modId = "";

                            // определяем айдишник
                            int addonId;
                            if (notContains) // мод есть в папке, но нет в списке, значит установлен собственноручно
                            {
                                // собстна генерируем айдишник
                                addonId = generatedAddonId;
                                while (actualAddonsList.ContainsKey(addonId))
                                {
                                    addonId--;
                                }
                                generatedAddonId = addonId - 1;
                            }
                            else // мод есть в спсике, берем его айдишник
                            {
                                addonId = existsAddons[xyi].Value2;
                            }

                            actualAddonsList[addonId] = new InstalledAddonInfo
                            {
                                FileID = notContains ? -1 : actualAddonsList[existsAddons[xyi].Value2].FileID,
                                ProjectID = addonId,
                                Type = AddonType.Mods,
                                IsDisable = isDisable,
                                Path = isZip ? xyi : xyi.Remove(xyi.Length - 8) // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                            };

                            addons.Add(new InstanceAddon(addonId, modpackInfo)
                            {
                                Author = authors,
                                Description = description,
                                Name = displayName,
                                FileName = filename,
                                Version = (!version.Contains("{") ? version : ""),
                                _isEnable = isZip
                            });
                        }
                        else
                        {
                            existsAddons[xyi].Value1.FileName = filename;
                            existsAddons[xyi].Value1._isEnable = isZip;
                        }
                    }
                }

                installedAddons.Save();
            }

            return addons;
        }

        public static List<InstanceAddon> GetInstalledWorlds(BaseInstanceData modpackInfo)
        {
            string clientPath = WithDirectory.DirectoryPath + "/instances/" + modpackInfo.LocalId + "/";
            List<InstanceAddon> addons = new List<InstanceAddon>();

            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            HashSet<int> existsCfMods = new HashSet<int>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsAddons = new Dictionary<string, ValuePair<InstanceAddon, int>>(); // ключ - имя файла, значение - экзмепляр и айдишник. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.

            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
                // если он существует, то добавляем в existsAddons и actualAddonsList.
                foreach (int installedAddonId in installedAddons.Keys)
                {
                    InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                    if (installedAddon.Type == AddonType.Maps) // с модами нужно поебаться и проверить
                    {
                        // Пока что пихаем только скачанные с курсфорджа моды. если айдишник больше -1, то аддон скачан с курсфорджа
                        if (installedAddon.IsExists(clientPath) && installedAddonId > -1)
                        {
                            existsCfMods.Add(installedAddonId);
                            actualAddonsList[installedAddonId] = installedAddon;
                            existsAddons[installedAddon.ActualPath] = new ValuePair<InstanceAddon, int>
                            {
                                Value1 = null,
                                Value2 = installedAddonId
                            };
                        }
                    }
                    else //всё остальное не тогаем. Просто перекидывеам в новый список
                    {
                        actualAddonsList[installedAddonId] = installedAddon;
                    }
                }

                // теперь получаем инфу об известных нам ресурпаков с курсфорджа
                if (existsCfMods.Count > 0)
                {
                    List<CurseforgeAddonInfo> cfData = CurseforgeApi.GetAddonsInfo(existsCfMods.ToArray());

                    if (cfData != null)
                    {
                        foreach (CurseforgeAddonInfo addon in cfData)
                        {
                            int projectId = addon.id;
                            if (existsCfMods.Contains(projectId))
                            {
                                InstalledAddonInfo info = actualAddonsList[projectId];
                                var obj = new InstanceAddon(addon, modpackInfo)
                                {
                                    Author = addon.GetAuthorName,
                                    Description = addon.summary,
                                    Name = addon.name,
                                    Version = "",
                                    WebsiteUrl = addon.links?.websiteUrl,
                                    IsUrlExist = (addon.links?.websiteUrl != null)
                                };

                                // проверяем наличие обновлений для карты
                                if (modpackInfo.Type == InstanceSource.Local)
                                {
                                    CurseforgeFileInfo lastFile = GetLastFile(modpackInfo.GameVersion, addon.latestFiles, modpackInfo);
                                    if (lastFile != null && lastFile.id > actualAddonsList[projectId].FileID)
                                    {
                                        obj.UpdateAvailable = true;
                                    }
                                }

                                existsAddons[info.ActualPath] = new ValuePair<InstanceAddon, int> // пихаем аддон в этот список именно в этом месте на всякий случай. вдруг долбаебы с курсфорджа вернут мне не весь список, который я запросил
                                {
                                    Value1 = obj,
                                    Value2 = projectId
                                };

                                if (addon.logo != null)
                                {
                                    obj.DownloadLogo(addon.logo.url);
                                }
                                addons.Add(obj);
                            }
                        }
                    }
                }

                installedAddons.Save();
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
                try
                {
                    using (var webClient = new WebClient())
                    {

                        Logo = ImageTools.ResizeImage(webClient.DownloadData(url), 40, 40);
                    }
                }
                catch { }
            });
        }

        private void Disable()
        {
            int projectID = _projectId;
            string instanceId = _modpackInfo.LocalId;

            using (InstalledAddons addons = InstalledAddons.Get(instanceId))
            {
                addons.DisableAddon(projectID, !_isEnable, delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.ActualPath, dir + data.Path);
                        }
                    }
                    catch { }
                }, 
                delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.Path, dir + data.ActualPath);
                        }
                    }
                    catch { }
                });

                addons.Save();
            }
        }

        public DownloadAddonRes Update()
        {
            DownloadAddonRes result = InstallLatestVersion(out _, false);
            if (result == DownloadAddonRes.Successful)
                UpdateAvailable = false;

            return result;
        }
    }
}
