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
using System.Runtime.CompilerServices;

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

        private string _websiteUrl = null;
        public string WebsiteUrl
        {
            get => _websiteUrl;
            set
            {
                _websiteUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUrlExist));
            }
        }

        public bool IsUrlExist
        {
            get
            {
                return !string.IsNullOrEmpty(_websiteUrl);
            }
        }

        #endregion

        private readonly CurseforgeAddonInfo _modInfo;
        private readonly BaseInstanceData _modpackInfo;
        private readonly int _projectId;
        private readonly string _gameVersion;

        /// <summary>
        /// Создает экземпляр аддона с курсфорджа.
        /// </summary>
        private InstanceAddon(CurseforgeAddonInfo modInfo, BaseInstanceData modpackInfo)
        {
            _modInfo = modInfo;
            _projectId = modInfo.id;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion;

            Author = modInfo.GetAuthorName;
            Description = modInfo.summary;
            Name = modInfo.name;
            WebsiteUrl = modInfo.links?.websiteUrl;

            DownloadLogo(modInfo.logo?.url);
        }

        /// <summary>
        /// Создает экземпляр аддона не с курсфорджа.
        /// </summary>    
        private InstanceAddon(int projectId, BaseInstanceData modpackInfo)
        {
            _modInfo = null;
            _projectId = projectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion;
        }

        /// <summary>
        /// Тут хранится список аддонов из метода GetAddonsCatalog. При каждом вызове GetAddonsCatalog этот список обновляется.
        /// Этот кэш необходим чтобы не перессоздавать InstanceClient для зависимого мода, при его скачивании.
        /// </summary>
        private static Dictionary<int, InstanceAddon> _addonsCatalogChache;

        /// <summary>
        /// Аддоны, которые устанавливаются в данный момент. После окончания установки они удаляются из этого списка.
        /// Нужно чтобы не создавать новый InstanceClient для тех модов, которые прямо сейчас скачиваются.
        /// Ключ - локальный id данной сборки + id мода. Значение - поинтер на InstanceAddon.
        /// </summary>
        private static ConcurrentDictionary<string, Pointer<InstanceAddon>> _installingAddons = new ConcurrentDictionary<string, Pointer<InstanceAddon>>();
        private static KeySemaphore<string> _installingSemaphore = new KeySemaphore<string>();
        private static Semaphore _chacheSemaphore = new Semaphore(1, 1);


        /// <summary>
        /// Этот метод нужен просто для удобства
        /// </summary>
        /// <param name="instanceData">данные модпака</param>
        /// <param name="addnId">айдишник мода</param>
        /// <returns>Ключ по которому искать этот мод в _installingAddons. (формат: _modpackInfo.LocalId + addnId)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetAddonKey(BaseInstanceData instanceData,int addnId)
        {
            return instanceData.LocalId + addnId;
        }

        /// <summary>
        /// Очищает сохранённый список аддонов. Нужно вызывать при закрытии каталога чтобы очистить память.
        /// </summary>
        public static void ClearAddonsListCache()
        {
            Runtime.DebugWrite("CHACHE");
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
            List<CurseforgeAddonInfo> addonsList = CurseforgeApi.GetAddonsList(pageSize, index * pageSize, type, category, modpackInfo.Modloader, searchFilter, modpackInfo.GameVersion);

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
                    string addonKey = GetAddonKey(modpackInfo, addonId);
                    _installingSemaphore.WaitOne(addonKey);
                    if (_installingAddons.ContainsKey(addonKey))
                    {
                        if (_installingAddons[addonKey].Point == null)
                        {
                            instanceAddon = new InstanceAddon(addon, modpackInfo)
                            {
                                IsInstalled = isInstalled,
                                UpdateAvailable = (installedAddons.ContainsKey(addonId) && (installedAddons[addonId].FileID < lastFileID)), // если установленная версия аддона меньше последней - значит доступно обновление
                                DownloadCount = (int)addon.downloadCount,
                                LastUpdated = DateTime.Parse(addon.dateModified).ToString("dd MMM yyyy")
                            };

                            _installingAddons[addonKey].Point = instanceAddon;
                            instanceAddon.IsInstalling = true;
                        }
                        else
                        {
                            instanceAddon = _installingAddons[addonKey].Point;
                            instanceAddon.DownloadLogo(addon.logo?.url);
                        }
                    }
                    else
                    {
                        instanceAddon = new InstanceAddon(addon, modpackInfo)
                        {
                            IsInstalled = isInstalled,
                            UpdateAvailable = (installedAddons.ContainsKey(addonId) && (installedAddons[addonId].FileID < lastFileID)), // если установленная версия аддона меньше последней - значит доступно обновление
                            DownloadCount = (int)addon.downloadCount,
                            LastUpdated = DateTime.Parse(addon.dateModified).ToString("dd MMM yyyy")
                        };
                    }
                    _installingSemaphore.Release(addonKey);

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

        public void Delete()
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

        private CancellationTokenSource _cancelTokenSource = null;

        /// <summary>
        /// Отменяет скачивание аддона.
        /// </summary>
        public void CancellDownload()
        {
            _cancelTokenSource?.Cancel();
        }

        public enum InstallAddonState
        {
            StartDownload,
            EndDownload
        }

        private void InstallAddon(CurseforgeFileInfo addonInfo, bool downloadDependencies, DynamicStateHandler<ValuePair<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler)
        {
            _cancelTokenSource = new CancellationTokenSource();
            stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.StartDownload);

            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                string addonKey = GetAddonKey(_modpackInfo, _modInfo.id);

                _installingSemaphore.WaitOne(addonKey);
                _installingAddons[addonKey] = new Pointer<InstanceAddon>
                {
                    Point = this
                };
                _installingSemaphore.Release(addonKey);

                var taskArgs = new TaskArgs
                {
                    PercentHandler = delegate (int percentages)
                    {
                        DownloadPercentages = percentages;
                    },
                    CancelToken = _cancelTokenSource.Token
                };

                if (_cancelTokenSource.Token.IsCancellationRequested)
                {
                    stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>
                    {
                        Value1 = this,
                        Value2 = DownloadAddonRes.IsCanselled
                    }, InstallAddonState.EndDownload);

                    return;
                }

                //так же скачиваем зависимости
                if (addonInfo.dependencies.Count > 0 && downloadDependencies)
                {
                    foreach (var dependencie in addonInfo.dependencies)
                    {
                        if (dependencie.ContainsKey("relationType") && dependencie["relationType"] == 3 && dependencie.ContainsKey("modId") && !installedAddons.ContainsKey(dependencie["modId"]))
                        {
                            Lexplosion.Runtime.TaskRun(delegate ()
                            {
                                int modId = dependencie["modId"];  

                                Pointer<InstanceAddon> addonPointer = new Pointer<InstanceAddon>();
                                addonPointer.Point = null;
                                _chacheSemaphore.WaitOne();
                                if (_addonsCatalogChache.ContainsKey(modId))
                                {
                                    addonPointer.Point = _addonsCatalogChache[modId];
                                    addonPointer.Point.IsInstalling = true;
                                }
                                _chacheSemaphore.Release();

                                string modKey = GetAddonKey(_modpackInfo, modId);

                                _installingSemaphore.WaitOne(modKey);
                                _installingAddons[modKey] = addonPointer;
                                _installingSemaphore.Release(modKey);

                                InstanceAddon addonInstance;
                                if (addonPointer.Point == null)
                                {
                                    var cfData = CurseforgeApi.GetAddonInfo(modId.ToString());

                                    _installingSemaphore.WaitOne(modKey);
                                    addonInstance = new InstanceAddon(cfData, _modpackInfo);
                                    _installingSemaphore.Release(modKey);
                                }
                                else
                                {
                                    addonInstance = addonPointer.Point;
                                }

                                addonInstance.InstallLatestVersion(stateHandler, true);
                            });
                        }             
                    }
                }

                var ressult = CurseforgeApi.DownloadAddon(addonInfo, (AddonType)_modInfo.classId, "instances/" + instanceId + "/", taskArgs);
                IsInstalling = false;

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    var addonData = CurseforgeApi.GetAddonInfo(addonInfo.modId.ToString());
                    WebsiteUrl = addonData?.links?.websiteUrl;
                });

                _installingSemaphore.WaitOne(addonKey);
                _installingAddons.TryRemove(addonKey, out _);
                _installingSemaphore.Release(addonKey);

                if (ressult.Value2 == DownloadAddonRes.Successful)
                {
                    IsInstalled = true;

                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.Successful
                        }, InstallAddonState.EndDownload);

                        return;
                    }

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
                }
                else
                {
                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>()
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.IsCanselled
                        }, InstallAddonState.EndDownload);
                    }
                    else
                    {
                        stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>()
                        {
                            Value1 = this,
                            Value2 = ressult.Value2
                        }, InstallAddonState.EndDownload);
                    }             

                    return;
                }

                installedAddons.Save();
            }

            _cancelTokenSource = null;
            stateHandler.ChangeState(new ValuePair<InstanceAddon, DownloadAddonRes>()
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.EndDownload);
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

        public void InstallLatestVersion(DynamicStateHandler<ValuePair<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, bool downloadDependencies = true)
        {
            IsInstalling = true;
            var file = GetLastFile(_modpackInfo.GameVersion, _modInfo?.latestFiles, _modpackInfo);
            if (file == null)
            {
                file = GetLastFile(_modpackInfo.GameVersion, CurseforgeApi.GetProjectFiles(_modInfo.id.ToString(), _gameVersion, _modpackInfo.Modloader), _modpackInfo);
                if (file != null)
                {
                    InstallAddon(file, downloadDependencies, stateHandler);
                }
                else
                {
                    IsInstalling = false;
                }
            }
            else
            {
                InstallAddon(file, downloadDependencies, stateHandler);
            }
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
                                    Version = ""
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

                installedAddons.Save(actualAddonsList);
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
                                    Version = ""
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

                installedAddons.Save(actualAddonsList);
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
                                    Version = ""
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

                                addons.Add(obj);
                            }
                        }
                    }
                }

                installedAddons.Save(actualAddonsList);
            }

            return addons;
        }

        /// <summary>
        /// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo. Делает это всё в пуле потоков.
        /// </summary>
        private void DownloadLogo(string url)
        {
            if (url != null)
            {
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    try
                    {
                        using (var webClient = new WebClient())
                        {

                            Logo = ImageTools.ResizeImage(webClient.DownloadData(url), 80, 80);
                        }
                    }
                    catch { }
                });
            }
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
                            File.Move(dir + data.Path + ".disable", dir + data.Path);
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
                            File.Move(dir + data.Path, dir + data.Path + ".disable");
                        }
                    }
                    catch { }
                });

                addons.Save();
            }
        }

        public DownloadAddonRes Update()
        {
            var stateData = new DynamicStateData<ValuePair<InstanceAddon, DownloadAddonRes>, InstallAddonState>();

            var result = DownloadAddonRes.UncnownError;
            stateData.StateChanged += (arg, state) =>
            {
                if (state == InstallAddonState.EndDownload)
                {
                    result = arg.Value2;
                }
            };

            InstallLatestVersion(stateData.GetHandler, false);

            if (result == DownloadAddonRes.Successful)
                UpdateAvailable = false;

            return result;
        }
    }
}
