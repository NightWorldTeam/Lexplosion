using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Tommy;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects.Modrinth;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstanceAddon : VMBase
    {
        private const string UNKNOWN_NAME = "Без названия";
        private const string DISABLE_FILE_EXTENSION = ".disable";

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

        public event Action LoadLoaded;

        private string _author = "";
        public string Author
        {
            get
            {
                return _addonPrototype?.AuthorName ?? _author;
            }
            set
            {
                _author = value;
            }
        }

        public string Name { get; private set; } = "";
        public string Description { get; private set; } = "";
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

        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            private set
            {
                _fileName = value;
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
                LoadLoaded?.Invoke();
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
            get => !string.IsNullOrWhiteSpace(_websiteUrl);
        }

        private string _logoUrl = null;
        public string LogoUrl
        {
            get => _logoUrl;
            set
            {
                _logoUrl = value;
                OnPropertyChanged();
            }
        }

        public ProjectSource Source
        {
            get
            {
                return (_addonPrototype is CurseforgeAddon) ? ProjectSource.Curseforge : ((_addonPrototype is ModrinthAddon) ? ProjectSource.Modrinth : ProjectSource.None);
            }
        }

        private IEnumerable<IProjectCategory> _categories = new List<CategoryBase>();
        public IEnumerable<IProjectCategory> Categories
        {
            get => _categories;
            private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        #endregion

        //private readonly CurseforgeAddonInfo _modInfo;
        private readonly BaseInstanceData _modpackInfo;
        private readonly IPrototypeAddon _addonPrototype;
        private readonly string _projectId;
        private readonly string _gameVersion;

        /// <summary>
        /// Создает экземпляр аддона с курсфорджа.
        /// </summary>
        private InstanceAddon(IPrototypeAddon addonPrototype, BaseInstanceData modpackInfo)
        {
            _addonPrototype = addonPrototype;
            _projectId = addonPrototype.ProjectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion.Id;

            Author = addonPrototype.AuthorName;
            Description = addonPrototype.Description;
            Name = addonPrototype.Name;
            WebsiteUrl = addonPrototype.WebsiteUrl;

            LogoUrl = addonPrototype.LogoUrl;

            LoadAdditionalData(addonPrototype.LogoUrl);

            addonPrototype.OnInfoUpdated += delegate ()
            {
                OnPropertyChanged(nameof(Author));
            };
        }

        /// <summary>
        /// Создает экземпляр аддона не с курсфорджа.
        /// </summary>    
        private InstanceAddon(string projectId, BaseInstanceData modpackInfo)
        {
            _addonPrototype = null;
            _projectId = projectId;
            _modpackInfo = modpackInfo;
            _gameVersion = modpackInfo.GameVersion.Id;
        }

        /// <summary>
        /// Тут хранится список аддонов из метода GetAddonsCatalog. При каждом вызове GetAddonsCatalog этот список обновляется.
        /// Этот кэш необходим чтобы не перессоздавать InstanceClient для зависимого мода, при его скачивании.
        /// </summary>
        private static Dictionary<string, InstanceAddon> _addonsCatalogChache;

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
        private static string GetAddonKey(BaseInstanceData instanceData, string addnId)
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
        /// Отвечает за синхронизацию загрузки установленны аддонов
        /// </summary>
        private static KeySemaphore<string> _addonsHandleSemaphore = new();

        private static object _watchingLocker = new object();
        private static FileSystemWatcher _modsDirectoryWathcer;
        private static FileSystemWatcher _resourcepacksDirectoryWathcer;
        private static BaseInstanceData _wathingInstanceData;

        /// <summary>
        /// Начинает наблюдение за добавлением или удалением файлов аддонов.
        /// Когда произойдет добавление или удаление, то отработает либо эвент AddonAdded, либо AddonRemoved соотвественно. 
        /// </summary>
        /// <param name="instanceData">Данные сборки, за папкой которой нужно следить</param>
        public static void StartWathingDirecoty(BaseInstanceData instanceData)
        {
            lock (_watchingLocker)
            {
                _modsDirectoryWathcer?.Dispose();
                _resourcepacksDirectoryWathcer?.Dispose();

                _wathingInstanceData = instanceData;

                _modsDirectoryWathcer = new FileSystemWatcher(WithDirectory.InstancesPath + instanceData.LocalId + "/mods");
                _modsDirectoryWathcer.Created += (object sender, FileSystemEventArgs e) =>
                {
                    OnAddonFileAdded(e.FullPath, AddonType.Mods, ".jar", DefineIternalModInfo);
                };
                _modsDirectoryWathcer.EnableRaisingEvents = true;

                _resourcepacksDirectoryWathcer = new FileSystemWatcher(WithDirectory.InstancesPath + instanceData.LocalId + "/resourcepacks");
                _resourcepacksDirectoryWathcer.Created += (object sender, FileSystemEventArgs e) =>
                {
                    IternalAddonInfoGetter addonInfo = delegate (string fileAddr, out string displayName, out string authors, out string version, out string description, out string modId)
                    {
                        displayName = UNKNOWN_NAME;
                        authors = "";
                        version = "";
                        description = "";
                        modId = "";
                    };

                    OnAddonFileAdded(e.FullPath, AddonType.Resourcepacks, ".zip", addonInfo);
                };
                _resourcepacksDirectoryWathcer.EnableRaisingEvents = true;
            }     
        }

        /// <summary>
        /// Прекращает наблюдение, начатое методом StartWathingDirecoty.
        /// </summary>
        public static void StopWatchingDirectory()
        {
            lock (_watchingLocker)
            {
                _modsDirectoryWathcer?.Dispose();
                _modsDirectoryWathcer = null;

                _resourcepacksDirectoryWathcer?.Dispose();
                _resourcepacksDirectoryWathcer = null;

                _wathingInstanceData = null;
            }         
        }

        private static void OnAddonFileAdded(string filePath, AddonType type, string fileExtension, IternalAddonInfoGetter addonInfoGetter)
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                if (_wathingInstanceData != null)
                {
                    var addon = AddonFileHandle(_wathingInstanceData, filePath, type, fileExtension, addonInfoGetter);
                    if (addon != null)
                    {
                        AddonAdded?.Invoke(addon);
                    }
                }
            });
        }

        public static event Action<InstanceAddon> AddonAdded;
        public static event Action<InstanceAddon> AddonRemoved;

        public static InstanceAddon CreateModrinthAddon(BaseInstanceData modpackInfo, ModrinthProjectInfo projectInfo)
        {
            IPrototypeAddon addonPrototype = new ModrinthAddon(modpackInfo, projectInfo);
            addonPrototype.DefineLatestVersion();
            return new InstanceAddon(addonPrototype, modpackInfo);
        }

        /// <summary>
        /// Вовзращает каталог аддонов
        /// </summary>
        /// <param name="projectSource">Тип источника в котором искать. (Curseforge или Modrinth, других нету).</param>
        /// <param name="modpackInfo">Класс BaseInstanceData, описывающий модпак, для которого нужно получить каталог адднов.</param>
        /// <param name="type">Тип аддона.</param>
        /// <param name="searchParams">Параметры поиска.</param>
        /// <returns>Собстна список аддонов.</returns>
        public static AddonsCatalog GetAddonsCatalog(ProjectSource projectSource, BaseInstanceData modpackInfo, AddonType type, ISearchParams searchParams)
        {
            switch (projectSource)
            {
                case ProjectSource.Curseforge:
                    {
                        CurseforgeSearchParams sParams;
                        if (searchParams is CurseforgeSearchParams)
                        {
                            sParams = (CurseforgeSearchParams)searchParams;
                        }
                        else
                        {
                            sParams = new CurseforgeSearchParams();
                        }

                        return GetCurseforgeAddonsCatalog(modpackInfo, type, sParams);
                    }

                case ProjectSource.Modrinth:
                    {
                        ModrinthSearchParams sParams;
                        if (searchParams is ModrinthSearchParams)
                        {
                            sParams = (ModrinthSearchParams)searchParams;
                        }
                        else
                        {
                            sParams = new ModrinthSearchParams();
                        }

                        return GetModrinthAddonsCatalog(modpackInfo, type, sParams);
                    }
                default:
                    return new AddonsCatalog();

            }
        }

        /// <summary>
        /// Возвращает список аддонов с курсфорджа.
        /// </summary>
        /// <param name="modpackInfo">Класс BaseInstanceData, описывающий модпак, для которого нужно получить каталог адднов.</param>
        /// <param name="pageSize">Размер страницы></param>
        /// <param name="index">Индекс</param>
        /// <param name="type">Тип аддона</param>
        /// <param name="category">Категория. По умолчанию -1 (при -1 все категории)</param>
        /// <param name="searchFilter">Поиск названия</param>
        /// <returns>Собстна список аддонов.</returns>
        public static List<InstanceAddon> GetAddonsCatalog(BaseInstanceData modpackInfo, int pageSize, int index, AddonType type, CategoryBase category, string searchFilter = "")
        {
            var searchParams = new CurseforgeSearchParams(searchFilter, modpackInfo.GameVersion.Id, new List<CategoryBase>() { category }, pageSize, index, CfSortField.Popularity, new List<ClientType>() { modpackInfo.Modloader });
            return (List<InstanceAddon>)GetCurseforgeAddonsCatalog(modpackInfo, type, searchParams).List;
            //var searchParams = new ModrinthSearchParams(searchFilter, modpackInfo.GameVersion.Id, new List<CategoryBase>() { category }, pageSize, index, ModrinthSortField.Relevance, new List<ClientType>() { modpackInfo.Modloader });
            //return GetModrinthAddonsCatalog(modpackInfo, type, searchParams);
        }

        private static AddonsCatalog GetCurseforgeAddonsCatalog(BaseInstanceData modpackInfo, AddonType type, CurseforgeSearchParams sParams)
        {
            int totalHits = -1;
            Func<List<CurseforgeAddonInfo>> getCatalog = () =>
            {
                (List<CurseforgeAddonInfo>, int) addonsList = CurseforgeApi.GetAddonsList(type, sParams);
                totalHits = addonsList.Item2;
                return addonsList.Item1;
            };

            Func<CurseforgeAddonInfo, IPrototypeAddon> addonPrototypeCreate = (CurseforgeAddonInfo addonInfo) =>
            {
                return new CurseforgeAddon(modpackInfo, addonInfo);
            };

            Func<CurseforgeAddonInfo, string> getAddonId = (CurseforgeAddonInfo addonInfo) => addonInfo.id;
            Func<CurseforgeAddonInfo, int> getDownloadCounts = (CurseforgeAddonInfo addonInfo) => (int)addonInfo.downloadCount;
            Func<CurseforgeAddonInfo, string> getLastUpdate = (CurseforgeAddonInfo addonInfo) =>
            {
                try
                {
                    return DateTime.Parse(addonInfo.dateModified).ToString("dd MMM yyyy");
                }
                catch
                {
                    return String.Empty;
                }
            };
            Func<CurseforgeAddonInfo, string> getLogoUrl = (CurseforgeAddonInfo addonInfo) => addonInfo.logo?.url;

            var catalog = GetAddonsCatalog(modpackInfo, type, sParams, getCatalog, addonPrototypeCreate, getAddonId, getDownloadCounts, getLastUpdate, getLogoUrl);
            return new AddonsCatalog(catalog, totalHits);
        }

        private static AddonsCatalog GetModrinthAddonsCatalog(BaseInstanceData modpackInfo, AddonType type, ModrinthSearchParams sParams)
        {
            int totalHits = -1;
            Func<List<ModrinthProjectInfo>> getCatalog = () =>
            {
                (List<ModrinthProjectInfo>, int) addonsList = ModrinthApi.GetAddonsList(type, sParams);
                totalHits = addonsList.Item2;
                return addonsList.Item1;
            };

            Func<ModrinthProjectInfo, IPrototypeAddon> addonPrototypeCreate = (ModrinthProjectInfo addonInfo) =>
            {
                return new ModrinthAddon(modpackInfo, addonInfo);
            };

            Func<ModrinthProjectInfo, string> getAddonId = (ModrinthProjectInfo addonInfo) => addonInfo.ProjectId;
            Func<ModrinthProjectInfo, int> getDownloadCounts = (ModrinthProjectInfo addonInfo) => addonInfo.Downloads;
            Func<ModrinthProjectInfo, string> getLastUpdate = (ModrinthProjectInfo addonInfo) =>
            {
                try
                {
                    return DateTime.Parse(addonInfo.Updated).ToString("dd MMM yyyy");
                }
                catch
                {
                    return String.Empty;
                }
            };
            Func<ModrinthProjectInfo, string> getLogoUrl = (ModrinthProjectInfo addonInfo) => addonInfo.LogoUrl;

            var catalog = GetAddonsCatalog(modpackInfo, type, sParams, getCatalog, addonPrototypeCreate, getAddonId, getDownloadCounts, getLastUpdate, getLogoUrl);
            return new AddonsCatalog(catalog, totalHits);
        }

        private static List<InstanceAddon> GetAddonsCatalog<TAddonInfo>(BaseInstanceData modpackInfo, AddonType type, ISearchParams searchParams, Func<List<TAddonInfo>> getCatalog, Func<TAddonInfo, IPrototypeAddon> addonPrototypeCreate, Func<TAddonInfo, string> getAddonId, Func<TAddonInfo, int> getDownloadCounts, Func<TAddonInfo, string> getLastUpdate, Func<TAddonInfo, string> getLogoUrl)
        {
            _addonsCatalogChache = new Dictionary<string, InstanceAddon>();

            string instanceId = modpackInfo.LocalId;
            var addons = new List<InstanceAddon>();

            // получаем спсиок всех аддонов с курсфорджа
            List<TAddonInfo> addonsList = getCatalog();

            // получаем список установленных аддонов
            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // проходимся по аддонам с курсфорджа
                int i = 0;
                foreach (TAddonInfo addon in addonsList)
                {
                    string addonId = getAddonId(addon);
                    bool isInstalled = (installedAddons.ContainsKey(addonId) && installedAddons[addonId].IsExists(WithDirectory.InstancesPath + instanceId + "/"));

                    InstanceAddon instanceAddon;
                    string addonKey = GetAddonKey(modpackInfo, addonId);
                    _installingSemaphore.WaitOne(addonKey);
                    if (_installingAddons.ContainsKey(addonKey))
                    {
                        if (_installingAddons[addonKey].Point == null)
                        {
                            IPrototypeAddon prototypeAddon = addonPrototypeCreate(addon);
                            instanceAddon = new InstanceAddon(prototypeAddon, modpackInfo)
                            {
                                IsInstalled = isInstalled,
                                DownloadCount = getDownloadCounts(addon),
                                LastUpdated = getLastUpdate(addon)
                            };

                            if (installedAddons.ContainsKey(addonId))
                            {
                                prototypeAddon.CompareVersions(installedAddons[addonId].FileID, () =>
                                {
                                    instanceAddon.UpdateAvailable = true;
                                });
                            }

                            _installingAddons[addonKey].Point = instanceAddon;
                            instanceAddon.IsInstalling = true;
                        }
                        else
                        {
                            instanceAddon = _installingAddons[addonKey].Point;
                            instanceAddon.LogoUrl = getLogoUrl(addon);
                            instanceAddon.LoadAdditionalData(getLogoUrl(addon));
                        }
                    }
                    else
                    {
                        IPrototypeAddon prototypeAddon = addonPrototypeCreate(addon);
                        instanceAddon = new InstanceAddon(prototypeAddon, modpackInfo)
                        {
                            IsInstalled = isInstalled,
                            DownloadCount = getDownloadCounts(addon),
                            LastUpdated = getLastUpdate(addon)
                        };

                        if (installedAddons.ContainsKey(addonId))
                        {
                            prototypeAddon.CompareVersions(installedAddons[addonId].FileID, () =>
                            {
                                instanceAddon.UpdateAvailable = true;
                            });
                        }
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
                InstalledAddonInfo addon = installedAddons[_projectId];
                installedAddons.TryRemove(_projectId);
                addon.RemoveFromDir(WithDirectory.InstancesPath + instanceId + "/");
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

        /// <summary>
        /// Проверяет не устанавливается ли аддон в данный момент
        /// </summary>
        /// <param name="_modpackInfo">Инфа о модпаке</param>
        /// <param name="addonId">Айди</param>
        /// <returns>true - устанавливается. Не устанавливается - false</returns>
        private static bool CheckInstalling(BaseInstanceData modpackInfo, string addonId)
        {
            string key = GetAddonKey(modpackInfo, addonId);
            _installingSemaphore.WaitOne(key);
            if (_installingAddons.ContainsKey(key))
            {
                _installingSemaphore.Release(key);
                return true;
            }
            else
            {
                _installingSemaphore.Release(key);
                return false;
            }
        }

        private void InstallAddon(bool downloadDependencies, DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, bool isDependencie)
        {
            // если такой аддон уже скачивается - выходим нахуй
            if (!isDependencie && CheckInstalling(_modpackInfo, _addonPrototype.ProjectId)) return;

            _cancelTokenSource = new CancellationTokenSource();
            stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.StartDownload);

            string instanceId = _modpackInfo.LocalId;
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                string addonKey = GetAddonKey(_modpackInfo, _addonPrototype.ProjectId);

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
                    stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                    {
                        Value1 = this,
                        Value2 = DownloadAddonRes.IsCanselled
                    }, InstallAddonState.EndDownload);

                    return;
                }

                //так же скачиваем зависимости
                List<AddonDependencie> dependencies = _addonPrototype.Dependecies;
                if (dependencies.Count > 0 && downloadDependencies)
                {
                    foreach (var dependencie in dependencies)
                    {
                        if (!installedAddons.ContainsKey(dependencie.AddonId))
                        {
                            Lexplosion.Runtime.TaskRun(delegate ()
                            {
                                string modId = dependencie.AddonId;

                                // если такой аддон уже скачивается - выходим нахуй
                                if (CheckInstalling(_modpackInfo, modId)) return;

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
                                    IPrototypeAddon addon = dependencie.AddonPrototype;
                                    addon.DefineDefaultVersion();

                                    _installingSemaphore.WaitOne(modKey);
                                    addonInstance = new InstanceAddon(addon, _modpackInfo);
                                    _installingSemaphore.Release(modKey);
                                }
                                else
                                {
                                    addonInstance = addonPointer.Point;
                                }

                                addonInstance.InstallLatestVersion(stateHandler, true, true);
                            });
                        }
                    }
                }

                var ressult = _addonPrototype.Install(taskArgs);
                if (ressult.Value2 != DownloadAddonRes.Successful)
                {
                    stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                    {
                        Value1 = this,
                        Value2 = ressult.Value2
                    }, InstallAddonState.EndDownload);

                    return;
                }

                FileName = Path.GetFileName(ressult.Value1.ActualPath);
                IsInstalling = false;

                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    WebsiteUrl = _addonPrototype.WebsiteUrl;
                });

                _installingSemaphore.WaitOne(addonKey);
                _installingAddons.TryRemove(addonKey, out _);
                _installingSemaphore.Release(addonKey);

                if (ressult.Value2 == DownloadAddonRes.Successful)
                {
                    IsInstalled = true;

                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.Successful
                        }, InstallAddonState.EndDownload);

                        return;
                    }

                    // удаляем старый файл
                    if (installedAddons[_addonPrototype.ProjectId] != null)
                    {
                        if (installedAddons[_addonPrototype.ProjectId].ActualPath != ressult.Value1.ActualPath)
                        {
                            try
                            {
                                string path = WithDirectory.InstancesPath + instanceId + "/";
                                InstalledAddonInfo installedAddon = installedAddons[_addonPrototype.ProjectId];
                                if (installedAddon.IsExists(path))
                                {
                                    installedAddon.RemoveFromDir(path);
                                }
                            }
                            catch { }
                        }

                        if (installedAddons[_addonPrototype.ProjectId].IsDisable)
                        {
                            try
                            {
                                string dir = WithDirectory.InstancesPath + instanceId + "/";
                                File.Move(dir + ressult.Value1.ActualPath, dir + ressult.Value1.Path + DISABLE_FILE_EXTENSION);
                                ressult.Value1.IsDisable = true;

                                FileName = Path.GetFileName(ressult.Value1.ActualPath);
                            }
                            catch { }
                        }
                    }

                    installedAddons[_addonPrototype.ProjectId] = ressult.Value1;
                }
                else
                {
                    if (_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
                        {
                            Value1 = this,
                            Value2 = DownloadAddonRes.IsCanselled
                        }, InstallAddonState.EndDownload);
                    }
                    else
                    {
                        stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
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
            stateHandler.ChangeState(new SetValues<InstanceAddon, DownloadAddonRes>()
            {
                Value1 = this,
                Value2 = DownloadAddonRes.Successful
            }, InstallAddonState.EndDownload);
        }

        public void InstallLatestVersion(DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState> stateHandler, bool downloadDependencies = true, bool isDependencie = false)
        {
            IsInstalling = true;
            _addonPrototype.DefineLatestVersion();
            InstallAddon(downloadDependencies, stateHandler, isDependencie);
            IsInstalling = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static InstanceAddon CreateAddonData(IPrototypeAddon prototypeAddon, string projectId, Dictionary<string, SetValues<InstanceAddon, string, ProjectSource>> existsAddons, List<InstanceAddon> addons, InstalledAddonsFormat actualAddonsList, BaseInstanceData modpackInfo, ProjectSource addonSourse)
        {
            InstalledAddonInfo info = actualAddonsList[projectId];
            var obj = new InstanceAddon(prototypeAddon, modpackInfo)
            {
                Version = ""
            };

            // проверяем наличие обновлений для мода
            if (modpackInfo.Type == InstanceSource.Local)
            {
                prototypeAddon.CompareVersions(actualAddonsList[projectId].FileID, () =>
                {
                    obj.UpdateAvailable = true;
                });
            }

            existsAddons[info.ActualPath] = new SetValues<InstanceAddon, string, ProjectSource> // пихаем аддон в этот список именно в этом месте на всякий случай. вдруг долбаебы с курсфорджа вернут мне не весь список, который я запросил
            {
                Value1 = obj,
                Value2 = projectId,
                Value3 = addonSourse
            };

            addons.Add(obj);

            return obj;
        }

        private delegate void IternalAddonInfoGetter(string fileAddr, out string displayName, out string authors, out string version, out string description, out string modId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<InstanceAddon> InstalledAddonsHandle(BaseInstanceData modpackInfo, AddonType addonType, string folderName, string fileExtension, IternalAddonInfoGetter infoHandler)
        {
            _addonsHandleSemaphore.WaitOne(modpackInfo.LocalId);

            string clientPath = WithDirectory.InstancesPath + modpackInfo.LocalId + "/";
            List<InstanceAddon> addons = new List<InstanceAddon>();

            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            var existsCfMods = new HashSet<string>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsMdMods = new HashSet<string>(); // аналогично existsCfMods, но для модринфа
            var existsUnknownAddons = new HashSet<string>(); // аддоны, которые существуют (есть в папке и спсике), но не имеют источника
            var existsAddons = new Dictionary<string, SetValues<InstanceAddon, string, ProjectSource>>(); // ключ - имя файла, значение - экзмепляр,айдишник и источник проекта. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.

            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
                // если он существует, то добавляем в existsAddons и actualAddonsList.
                foreach (string installedAddonId in installedAddons.Keys)
                {
                    InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                    if (installedAddon.Type == addonType) // с модами нужно поебаться и проверить
                    {
                        // Пока что пихаем только скачанные с курсфорджа моды. если айдишник больше -1, то аддон скачан с курсфорджа
                        if (installedAddon.IsExists(clientPath))
                        {
                            if (installedAddon.Source == ProjectSource.Curseforge)
                            {
                                if (installedAddonId.ToInt32() > 0)
                                {
                                    existsCfMods.Add(installedAddonId);
                                    actualAddonsList[installedAddonId] = installedAddon;
                                    existsAddons[installedAddon.ActualPath] = new SetValues<InstanceAddon, string, ProjectSource>
                                    {
                                        Value1 = null,
                                        Value2 = installedAddonId,
                                        Value3 = ProjectSource.Curseforge
                                    };
                                }
                                else // айдишник кривой и не может соотвествовать курсфорджу
                                {
                                    installedAddons[installedAddonId].Source = ProjectSource.None;
                                }
                            }
                            else if (installedAddon.Source == ProjectSource.Modrinth)
                            {
                                existsMdMods.Add(installedAddonId);
                                actualAddonsList[installedAddonId] = installedAddon;
                                existsAddons[installedAddon.ActualPath] = new SetValues<InstanceAddon, string, ProjectSource>
                                {
                                    Value1 = null,
                                    Value2 = installedAddonId,
                                    Value3 = ProjectSource.Modrinth
                                };
                            }
                            else
                            {
                                existsUnknownAddons.Add(installedAddon.ActualPath);
                            }
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
                            string projectId = addon.id;
                            if (existsCfMods.Contains(projectId))
                            {
                                IPrototypeAddon prototypeAddon = new CurseforgeAddon(modpackInfo, addon);
                                CreateAddonData(prototypeAddon, projectId, existsAddons, addons, actualAddonsList, modpackInfo, ProjectSource.Curseforge);
                            }
                        }
                    }
                }

                // теперь получаем инфу об известных нам модов с модринфа
                if (existsMdMods.Count > 0)
                {
                    List<ModrinthProjectInfo> mdData = ModrinthApi.GetProjects(existsMdMods.ToArray());

                    foreach (ModrinthProjectInfo addon in mdData)
                    {
                        string projectId = addon.ProjectId;
                        if (existsMdMods.Contains(projectId))
                        {
                            IPrototypeAddon prototypeAddon = new ModrinthAddon(modpackInfo, addon);
                            CreateAddonData(prototypeAddon, projectId, existsAddons, addons, actualAddonsList, modpackInfo, ProjectSource.Modrinth);
                        }
                    }
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(clientPath + folderName, "*.*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    _addonsHandleSemaphore.Release(modpackInfo.LocalId);
                    return addons;
                }

                var unknownAddons = new List<string>();
                foreach (string fileAddr in files)
                {
                    string fileAddr_ = fileAddr.Replace('\\', '/');
                    string extension = Path.GetExtension(fileAddr_);
                    bool isAddonExtension = (extension == fileExtension), isDisable = (extension == DISABLE_FILE_EXTENSION);
                    if (isAddonExtension || isDisable)
                    {
                        string xyi = fileAddr_.Replace(WithDirectory.InstancesPath + modpackInfo.LocalId + "/", "");
                        if (!existsUnknownAddons.Contains(xyi) && !existsAddons.ContainsKey(xyi))
                        {
                            unknownAddons.Add(fileAddr);
                        }
                    }
                }

                Dictionary<string, IPrototypeAddon> addonsData = null;
                if (unknownAddons.Count > 0)
                {
                    addonsData = AddonsPrototypesCreater.CreateFromFiles(modpackInfo, unknownAddons);
                    Runtime.DebugWrite("addonsData lenght " + addonsData.Count);
                }

                int generatedAddonId = -1; // тут хранится следующий следющий сгенерированный айдишник. По сути переменная нужна чисто для оптимизации
                                           // Теперь проходмся по всем файлам в папке. Создаем InstalledAddonInfo для тех аддонов, которых не было в списке, или инфа о которых не была получена с курсфорджа
                foreach (string fileAddr in files)
                {
                    string fileAddr_ = fileAddr.Replace('\\', '/');
                    string extension = Path.GetExtension(fileAddr_);
                    bool isAddonExtension = (extension == fileExtension), isDisable = (extension == DISABLE_FILE_EXTENSION);
                    if (isAddonExtension || isDisable)
                    {
                        string xyi = fileAddr_.Replace(WithDirectory.InstancesPath + modpackInfo.LocalId + "/", "");

                        string filename = "";
                        try
                        {
                            filename = Path.GetFileName(fileAddr);
                        }
                        catch { }

                        // аддон есть в папке, но нет в списке, или он есть и в папке и в списке, но скачан нее с курсфорджа, то нужно добавить, так же генерируем айдишник для него
                        // ну или просто запрос был не успешным
                        bool noSource = !existsAddons.ContainsKey(xyi);
                        if (noSource || existsAddons[xyi].Value1 == null)
                        {
                            // определяем айдишник
                            int addonId_;
                            string addonId;
                            if (noSource) // мод есть в папке, но нет в списке, значит установлен собственноручно
                            {
                                if (addonsData != null && addonsData.ContainsKey(fileAddr))
                                {
                                    IPrototypeAddon addonData = addonsData[fileAddr];

                                    addonData.DefineDefaultVersion();
                                    string addon_projectId = addonData.ProjectId;

                                    actualAddonsList[addon_projectId] = new InstalledAddonInfo
                                    {
                                        FileID = addonData.FileId,
                                        ProjectID = addon_projectId,
                                        Type = addonType,
                                        IsDisable = isDisable,
                                        Path = isAddonExtension ? xyi : xyi.Remove(xyi.Length - 8), // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                                        Source = addonData.Source
                                    };

                                    var obj = CreateAddonData(addonData, addonData.ProjectId, existsAddons, addons, actualAddonsList, modpackInfo, addonData.Source);
                                    obj.FileName = filename;
                                    obj._isEnable = isAddonExtension;

                                    continue;
                                }

                                // собстна генерируем айдишник
                                addonId_ = generatedAddonId;
                                while (actualAddonsList.ContainsKey(addonId_.ToString()))
                                {
                                    addonId_--;
                                }
                                generatedAddonId = addonId_ - 1;
                                addonId = addonId_.ToString();
                            }
                            else // мод есть в спсике, берем его айдишник
                            {
                                addonId = existsAddons[xyi].Value2;
                            }

                            Runtime.DebugWrite("unknown addon " + fileAddr_);

                            actualAddonsList[addonId] = new InstalledAddonInfo
                            {
                                FileID = noSource ? "-1" : actualAddonsList[existsAddons[xyi].Value2].FileID,
                                ProjectID = addonId,
                                Type = addonType,
                                IsDisable = isDisable,
                                Path = isAddonExtension ? xyi : xyi.Remove(xyi.Length - 8), // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                                Source = noSource ? ProjectSource.None : existsAddons[xyi].Value3
                            };

                            // тут пытаемся получить инфу о моде
                            infoHandler(fileAddr_, out string displayName, out string authors, out string version, out string description, out string modId);

                            addons.Add(new InstanceAddon(addonId, modpackInfo)
                            {
                                Author = authors,
                                Description = description,
                                Name = displayName,
                                FileName = filename,
                                Version = (!version.Contains("{") ? version : ""),
                                _isEnable = isAddonExtension
                            });
                        }
                        else
                        {
                            // тут пытаемся получить инфу о моде
                            //infoHandler(fileAddr_, out string displayName, out string authors, out string version, out string description, out string modId);
                            InstanceAddon obj = existsAddons[xyi].Value1;
                            obj.FileName = filename;
                            obj._isEnable = isAddonExtension;
                            //obj.Version = version;
                            //if (string.IsNullOrWhiteSpace(obj.Author)) obj.Author = authors;
                        }
                    }
                }

                Runtime.DebugWrite("End");

                installedAddons.Save(actualAddonsList);
            }

            Runtime.DebugWrite(addonType + " " + addons.Count);
            _addonsHandleSemaphore.Release(modpackInfo.LocalId);
            return addons;
        }

        private static InstanceAddon AddonFileHandle(BaseInstanceData modpackInfo, string filePath, AddonType addonType, string fileExtension, IternalAddonInfoGetter infoHandler)
        {
            _addonsHandleSemaphore.WaitOne(modpackInfo.LocalId);
            // определяем айдишник
            int addonId_;
            string addonId;

            string fileAddr_ = filePath.Replace('\\', '/');
            string extension = Path.GetExtension(fileAddr_);
            bool isAddonExtension = (extension == fileExtension), isDisable = (extension == DISABLE_FILE_EXTENSION);

            if (!isAddonExtension && !isDisable)
            {
                _addonsHandleSemaphore.Release(modpackInfo.LocalId);
                return null;
            }

            string filename = "";
            try
            {
                filename = Path.GetFileName(filePath);
            }
            catch { }

            string xyi = fileAddr_.Replace(WithDirectory.InstancesPath + modpackInfo.LocalId + "/", "");

            IPrototypeAddon addonData = AddonsPrototypesCreater.CreateFromFile(modpackInfo, filePath);

            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                if (addonData != null)
                {
                    addonData.DefineDefaultVersion();
                    string addon_projectId = addonData.ProjectId;

                    installedAddons[addon_projectId] = new InstalledAddonInfo
                    {
                        FileID = addonData.FileId,
                        ProjectID = addon_projectId,
                        Type = addonType,
                        IsDisable = isDisable,
                        Path = isAddonExtension ? xyi : xyi.Remove(xyi.Length - 8), // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                        Source = addonData.Source
                    };

                    var obj = new InstanceAddon(addonData, modpackInfo)
                    {
                        Version = ""
                    };

                    // проверяем наличие обновлений для мода
                    if (modpackInfo.Type == InstanceSource.Local)
                    {
                        addonData.CompareVersions(addonData.FileId, () =>
                        {
                            obj.UpdateAvailable = true;
                        });
                    }

                    obj.FileName = filename;
                    obj._isEnable = isAddonExtension;

                    _addonsHandleSemaphore.Release(modpackInfo.LocalId);
                    return obj;
                }

                // собстна генерируем айдишник
                addonId_ = -1;
                while (installedAddons.ContainsKey(addonId_.ToString()))
                {
                    addonId_--;
                }
                addonId = addonId_.ToString();

                Runtime.DebugWrite("unknown addon " + fileAddr_);

                installedAddons[addonId] = new InstalledAddonInfo
                {
                    FileID = "-1",
                    ProjectID = addonId,
                    Type = addonType,
                    IsDisable = isDisable,
                    Path = isAddonExtension ? xyi : xyi.Remove(xyi.Length - 8), // если аддон выключен, то в спсиок его путь помещаем без расширения .disable
                    Source = ProjectSource.None
                };

                // тут пытаемся получить инфу о моде
                infoHandler(fileAddr_, out string displayName, out string authors, out string version, out string description, out string modId);

                installedAddons.Save();

                _addonsHandleSemaphore.Release(modpackInfo.LocalId);
                return new InstanceAddon(addonId, modpackInfo)
                {
                    Author = authors,
                    Description = description,
                    Name = displayName,
                    FileName = filename,
                    Version = (!version.Contains("{") ? version : ""),
                    _isEnable = isAddonExtension
                };
            }
        }

        /// <summary>
        /// Возвращает список аддонов. 
        /// </summary>
        /// <param name="type">Тип аддонов</param>
        /// <param name="baseInstanceData">Информация о модпаке аддоны которого нужно получить</param>
        /// <returns>Лист адднов определенного типа указанной сборки</returns>
        /// <exception cref="ArgumentException">Сообщает о том, что был передан тип аддонов который не рассматривается в методе.</exception>
        public static IList<InstanceAddon> GetInstalledAddons(AddonType type, BaseInstanceData baseInstanceData)
        {
            return type switch
            {
                AddonType.Mods => GetInstalledMods(baseInstanceData),
                AddonType.Maps => GetInstalledWorlds(baseInstanceData),
                AddonType.Resourcepacks => GetInstalledResourcepacks(baseInstanceData),
                AddonType.Shaders => new(),
                _ => throw new ArgumentException("Ты передал мне тип который тут не расматривается")
            };
        }

        private static void DefineIternalModInfo(string fileAddr, out string displayName, out string authors, out string version, out string description, out string modId)
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

            displayName = UNKNOWN_NAME;
            authors = "";
            version = "";
            description = "";
            modId = "";

            // тут пытаемся получить инфу о моде
            try
            {
                using (ZipArchive zip = ZipFile.Open(fileAddr, ZipArchiveMode.Read))
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
        }

        /// <summary>
        /// Возвращает список модов. При вызове так же сохраняет список модов, 
        /// анализирует папку mods и пихает в список моды которые были в папке, но которых не было в списке.
        /// </summary>
        /// <param name="modpackInfo">Инфа о модпаке с которого нужно получить список модов</param>
        public static List<InstanceAddon> GetInstalledMods(BaseInstanceData modpackInfo)
        {
            return InstalledAddonsHandle(modpackInfo, AddonType.Mods, "mods", ".jar", DefineIternalModInfo);
        }

        public static List<InstanceAddon> GetInstalledResourcepacks(BaseInstanceData modpackInfo)
        {
            IternalAddonInfoGetter addonInfo = delegate (string fileAddr, out string displayName, out string authors, out string version, out string description, out string modId)
            {
                displayName = UNKNOWN_NAME;
                authors = "";
                version = "";
                description = "";
                modId = "";
            };

            return InstalledAddonsHandle(modpackInfo, AddonType.Resourcepacks, "resourcepacks", ".zip", addonInfo);
        }

        public static List<InstanceAddon> GetInstalledWorlds(BaseInstanceData modpackInfo)
        {
            string clientPath = WithDirectory.InstancesPath + modpackInfo.LocalId + "/";
            List<InstanceAddon> addons = new List<InstanceAddon>();
            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            var existsCfMods = new HashSet<string>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsAddons = new Dictionary<string, SetValues<InstanceAddon, string>>(); // ключ - имя файла, значение - экзмепляр и айдишник. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.
            using (InstalledAddons installedAddons = InstalledAddons.Get(modpackInfo.LocalId))
            {
                // Составляем список известных нам аддонов. То есть читаем спсиок аддонов из файла, проходимся по каждому
                // если он существует, то добавляем в existsAddons и actualAddonsList.
                foreach (string installedAddonId in installedAddons.Keys)
                {
                    InstalledAddonInfo installedAddon = installedAddons[installedAddonId];
                    if (installedAddon.Type == AddonType.Maps) // с модами нужно поебаться и проверить
                    {
                        // Пока что пихаем только скачанные с курсфорджа моды. если айдишник больше -1, то аддон скачан с курсфорджа
                        if (installedAddon.IsExists(clientPath) && installedAddonId.ToInt32() > -1)
                        {
                            existsCfMods.Add(installedAddonId);
                            actualAddonsList[installedAddonId] = installedAddon;
                            existsAddons[installedAddon.ActualPath] = new SetValues<InstanceAddon, string>
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
                            string projectId = addon.id;
                            if (existsCfMods.Contains(projectId))
                            {
                                IPrototypeAddon prototypeAddon = new CurseforgeAddon(modpackInfo, addon);

                                InstalledAddonInfo info = actualAddonsList[projectId];
                                var obj = new InstanceAddon(prototypeAddon, modpackInfo)
                                {
                                    Version = ""
                                };

                                // проверяем наличие обновлений для мода
                                if (modpackInfo.Type == InstanceSource.Local)
                                {
                                    prototypeAddon.CompareVersions(actualAddonsList[projectId].FileID, () =>
                                    {
                                        obj.UpdateAvailable = true;
                                    });
                                }

                                existsAddons[info.ActualPath] = new SetValues<InstanceAddon, string> // пихаем аддон в этот список именно в этом месте на всякий случай. вдруг долбаебы с курсфорджа вернут мне не весь список, который я запросил
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
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Proxy = null;
                        Logo = ImageTools.ResizeImage(webClient.DownloadData(url), 80, 80);
                    }
                }
                catch { }
            }
        }

        private void LoadAdditionalData(string logoUrl)
        {
            Categories = _addonPrototype.LoadCategories();
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                DownloadLogo(logoUrl);
            });
        }

        private void Disable()
        {
            string projectID = _projectId;
            string instanceId = _modpackInfo.LocalId;

            using (InstalledAddons addons = InstalledAddons.Get(instanceId))
            {
                addons.DisableAddon(projectID, !_isEnable, delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = WithDirectory.InstancesPath + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.Path + DISABLE_FILE_EXTENSION, dir + data.Path);
                        }
                    }
                    catch { }
                },
                delegate (InstalledAddonInfo data)
                {
                    try
                    {
                        string dir = WithDirectory.InstancesPath + instanceId + "/";
                        if (data.IsExists(dir))
                        {
                            File.Move(dir + data.Path, dir + data.Path + DISABLE_FILE_EXTENSION);
                        }
                    }
                    catch { }
                });

                FileName = Path.GetFileName(addons[projectID]?.ActualPath);

                addons.Save();
            }
        }

        public DownloadAddonRes Update()
        {
            var stateData = new DynamicStateData<SetValues<InstanceAddon, DownloadAddonRes>, InstallAddonState>();

            var result = DownloadAddonRes.unknownError;
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
