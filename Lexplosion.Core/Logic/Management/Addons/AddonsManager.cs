using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.IO;
using Tommy;
using Newtonsoft.Json;
using Lexplosion.Tools;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Core.Logic.Management.Addons.AddonsCatalogParams;
using Lexplosion.Core.Logic.Management.Addons;

namespace Lexplosion.Logic.Management.Addons
{
	public class AddonsManager
    {
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

        private const string UNKNOWN_NAME = "Без названия";
        private const string DISABLE_FILE_EXTENSION = ".disable";

        private InstaceAddonsSynchronizer _synchronizer = new();
        private BaseInstanceData _modpackInfo;

        private object _watchingLocker = new object();
        private FileSystemWatcher _modsDirectoryWathcer;
        private FileSystemWatcher _resourcepacksDirectoryWathcer;


        public event Action<InstanceAddon> AddonAdded
        {
            add => _synchronizer.AddonAdded += value;
            remove => _synchronizer.AddonAdded -= value;
        }

        public event Action<InstanceAddon> AddonRemoved
        {
            add => _synchronizer.AddonRemoved += value;
            remove => _synchronizer.AddonRemoved -= value;
        }

        internal AddonsManager(BaseInstanceData instanceData)
        {
            _modpackInfo = instanceData;
        }

        private static Dictionary<string, AddonsManager> _managers = new();
        private static object _getManagerLocker = new object();

        public static AddonsManager GetManager(BaseInstanceData instanceData)
        {
            lock (_getManagerLocker)
            {
                _managers.TryGetValue(instanceData.LocalId, out AddonsManager manager);
                if (manager != null) return manager;

                manager = new AddonsManager(instanceData);
                _managers[instanceData.LocalId] = manager;

                return manager;
            }
        }

        /// <summary>
        /// Начинает наблюдение за добавлением или удалением файлов аддонов.
        /// Когда произойдет добавление или удаление, то отработает либо эвент AddonAdded, либо AddonRemoved соотвественно. 
        /// </summary>
        public void StartWathingDirecoty()
        {
            lock (_watchingLocker)
            {
                _modsDirectoryWathcer?.Dispose();
                _resourcepacksDirectoryWathcer?.Dispose();

                try
                {
                    string modsPath = WithDirectory.GetInstancePath(_modpackInfo.LocalId) + "mods";
                    if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);

                    _modsDirectoryWathcer = new FileSystemWatcher(modsPath);
                    _modsDirectoryWathcer.Created += (object sender, FileSystemEventArgs e) =>
                    {
                        _synchronizer.Xer(() => OnAddonFileAdded(e.FullPath, AddonType.Mods, ".jar", DefineIternalModInfo));
                    };
                    _modsDirectoryWathcer.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                }

                string resourcespacksPath = WithDirectory.GetInstancePath(_modpackInfo.LocalId) + "resourcepacks";
                try
                {
                    if (!Directory.Exists(resourcespacksPath))
                    {
                        Directory.CreateDirectory(resourcespacksPath);
                    }

                    _resourcepacksDirectoryWathcer = new FileSystemWatcher(resourcespacksPath);
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

                        _synchronizer.Xer(() => OnAddonFileAdded(e.FullPath, AddonType.Resourcepacks, ".zip", addonInfo));
                    };
                    _resourcepacksDirectoryWathcer.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                }
            }
        }

        /// <summary>
        /// Прекращает наблюдение, начатое методом <see cref="StartWathingDirecoty"/>.
        /// </summary>
        public void StopWatchingDirectory()
        {
            lock (_watchingLocker)
            {
                _modsDirectoryWathcer?.Dispose();
                _modsDirectoryWathcer = null;

                _resourcepacksDirectoryWathcer?.Dispose();
                _resourcepacksDirectoryWathcer = null;
            }
        }

        private void OnAddonFileAdded(string filePath, AddonType type, string fileExtension, IternalAddonInfoGetter addonInfoGetter)
        {
            Runtime.DebugWrite("OnAddonFileAdded");
            _synchronizer.Xer(() =>
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    string fileName;
                    try
                    {
                        fileName = Path.GetFileName(filePath);
                    }
                    catch (Exception ex)
                    {
                        Runtime.DebugWrite(ex);
                        return;
                    }

                    if (fileName == null) return;
                    if (_synchronizer.InstalledAddonContains((type, fileName))) return;

                    var addon = AddonFileHandle(filePath, type, fileExtension, addonInfoGetter);
                    if (addon != null) _synchronizer.AddInstalledAddon(addon);

                });
            });
        }

        /// <summary>
        /// Добавляет в клиент аддоны
        /// </summary>
        /// <param name="locations">Пути к аддонам</param>
        /// <param name="type">Тип аддонов</param>
        /// <param name="addons">Результрующий список. 
        /// Если количество аддонов меньше 10, то здесь будет null и InstanceAddon'ы надо будет получить через эвент AddonAdded.
        /// Если же количество аддонов больше или равно 10, то эвент AddonAdded не отработает и InstanceAddon'ы надо брать от сюда</param>
        /// <returns>Требуется ли полное обновление списка. 
        /// Если количество аддонов больше или равно 10, то список надо будет обновить полностью и здесь будет true, иначе false</returns>
        public bool AddAddons(IEnumerable<string> locations, AddonType type, out IList<InstanceAddon> addons)
        {
            addons = null;

            string path = WithDirectory.GetInstancePath(_modpackInfo.LocalId);
            if (type == AddonType.Mods)
                path += "mods/";
            else if (type == AddonType.Resourcepacks)
                path += "resourcepacks/";
            else if (type == AddonType.Shaders)
                path += "shaderspacks/";
            else if (type == AddonType.Maps)
                path += "saves/";
            else
                return false;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            bool bigQuantity = locations.Count() > 10;
            if (bigQuantity) StopWatchingDirectory();

            foreach (string location in locations)
            {
                try
                {
                    if (File.Exists(location))
                    {
                        File.Copy(location, path + Path.GetFileName(location));
                    }
                }
                catch (Exception e)
                {
                    Runtime.DebugWrite("File copy error " + e);
                }
            }

            if (!bigQuantity) return false;

            addons = GetInstalledAddons(type);

            if (bigQuantity) StartWathingDirecoty();

            return true;
        }

        /// <summary>
        /// Вовзращает каталог аддонов
        /// </summary>
        /// <param name="projectSource">Тип источника в котором искать. (Curseforge или Modrinth, других нету).</param>
        /// <param name="type">Тип аддона.</param>
        /// <param name="searchParams">Параметры поиска.</param>
        /// <returns>Собстна список аддонов.</returns>
        public CatalogResult<InstanceAddon> GetAddonsCatalog(ProjectSource projectSource, AddonType type, ISearchParams searchParams)
        {
            Runtime.DebugWrite($"GetAddonsCatalog {projectSource} {type}");
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

                        return GetCurseforgeAddonsCatalog(type, sParams);
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

                        return GetModrinthAddonsCatalog(type, sParams);
                    }
                default:
                    return new();

            }
        }

        private CatalogResult<InstanceAddon> GetCurseforgeAddonsCatalog(AddonType type, CurseforgeSearchParams sParams)
        {
            var catalogParams = new CurseforgeAddonsCatalogParams(type, sParams, _modpackInfo);

            return GetAddonsCatalog(catalogParams);
        }

        private CatalogResult<InstanceAddon> GetModrinthAddonsCatalog(AddonType type, ModrinthSearchParams sParams)
        {
            var catalogParams = new ModrinthAddonsCatalogParams(type, sParams, _modpackInfo);

            return GetAddonsCatalog(catalogParams);
        }


        private CatalogResult<InstanceAddon> GetAddonsCatalog<TAddonInfo, TSearchParams>(AddonsCatalogParamsBase<TAddonInfo, TSearchParams> catalogParams)
            where TAddonInfo : IAddonProjectInfo
            where TSearchParams : ISearchParams
        {
            _synchronizer.InitAddonsListChache();

            string instanceId = _modpackInfo.LocalId;
            var addons = new List<InstanceAddon>();

            // получаем спсиок всех аддонов с курсфорджа
            CatalogResult<TAddonInfo> addonsList = catalogParams.GetCatalog();

            // получаем список установленных аддонов
            using (InstalledAddons installedAddons = InstalledAddons.Get(_modpackInfo.LocalId))
            {
                // проходимся по аддонам с курсфорджа
                int i = 0;
                foreach (TAddonInfo addon in addonsList)
                {
                    string addonId = catalogParams.GetAddonId(addon);
                    bool isInstalled = (installedAddons.ContainsKey(addonId) && installedAddons[addonId].IsExists(WithDirectory.InstancesPath + instanceId + "/"));

                    InstanceAddon instanceAddon;
                    _synchronizer.InstallingSemaphore.WaitOne(addonId);
                    if (_synchronizer.InstallingAddons.ContainsKey(addonId))
                    {
                        if (_synchronizer.InstallingAddons[addonId].Point == null)
                        {
                            IPrototypeAddon prototypeAddon = catalogParams.CreateAddonPrototypeCreate(addon);
                            instanceAddon = new InstanceAddon(catalogParams.Type, _synchronizer, prototypeAddon, _modpackInfo)
                            {
                                IsInstalled = isInstalled,
                                DownloadCount = catalogParams.GetDownloadCounts(addon),
                                LastUpdated = catalogParams.GetLastUpdate(addon)
                            };

                            if (installedAddons.ContainsKey(addonId))
                            {
                                prototypeAddon.CompareVersions(installedAddons[addonId].FileID, () =>
                                {
                                    instanceAddon.UpdateAvailable = true;
                                });
                            }

                            _synchronizer.InstallingAddons[addonId].Point = instanceAddon;
                            instanceAddon.IsInstalling = true;
                        }
                        else
                        {
                            instanceAddon = _synchronizer.InstallingAddons[addonId].Point;
                            instanceAddon.LogoUrl = catalogParams.GetLogoUrl(addon);
                            instanceAddon.LoadAdditionalData(catalogParams.GetLogoUrl(addon));
                        }
                    }
                    else
                    {
                        IPrototypeAddon prototypeAddon = catalogParams.CreateAddonPrototypeCreate(addon);
                        instanceAddon = new InstanceAddon(catalogParams.Type, _synchronizer, prototypeAddon, _modpackInfo)
                        {
                            IsInstalled = isInstalled,
                            DownloadCount = catalogParams.GetDownloadCounts(addon),
                            LastUpdated = catalogParams.GetLastUpdate(addon)
                        };

                        if (installedAddons.ContainsKey(addonId))
                        {
                            prototypeAddon.CompareVersions(installedAddons[addonId].FileID, () =>
                            {
                                instanceAddon.UpdateAvailable = true;
                            });
                        }
                    }
                    _synchronizer.InstallingSemaphore.Release(addonId);

                    addons.Add(instanceAddon);
                    _synchronizer.ChacheSemaphore.WaitOne();
                    if (_synchronizer.AddonsCatalogChache != null)
                    {
                        _synchronizer.AddonsCatalogChache[addonId] = instanceAddon;
                    }
                    _synchronizer.ChacheSemaphore.Release();

                    i++;
                }
            }

            return new(addons, addonsList.TotalCount);
        }

        public InstanceAddon CreateModrinthAddon(ModrinthProjectInfo projectInfo)
        {
            IPrototypeAddon addonPrototype = new ModrinthAddon(_modpackInfo, projectInfo);
            addonPrototype.DefineLatestVersion();
            return new InstanceAddon(projectInfo.GetAddonType, _synchronizer, addonPrototype, _modpackInfo);
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
        /// Возвращает список аддонов. 
        /// </summary>
        /// <param name="type">Тип аддонов</param>
        /// <returns>Лист адднов определенного типа указанной сборки</returns>
        /// <exception cref="ArgumentException">Сообщает о том, что был передан тип аддонов который не рассматривается в методе.</exception>
        public IList<InstanceAddon> GetInstalledAddons(AddonType type)
        {
            return type switch
            {
                AddonType.Mods => GetInstalledMods(),
                AddonType.Maps => GetInstalledWorlds(),
                AddonType.Resourcepacks => GetInstalledResourcepacks(),
                AddonType.Shaders => new(),
                _ => throw new ArgumentException("Ты еблан блять?")
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InstanceAddon CreateAddonData(AddonType type, IPrototypeAddon prototypeAddon, string projectId, Dictionary<string, SetValues<InstanceAddon, string, ProjectSource>> existsAddons, List<InstanceAddon> addons, InstalledAddonsFormat actualAddonsList, ProjectSource addonSourse)
        {
            InstalledAddonInfo info = actualAddonsList[projectId];
            var obj = new InstanceAddon(type, _synchronizer, prototypeAddon, _modpackInfo)
            {
                Version = ""
            };

            // проверяем наличие обновлений для мода
            if (_modpackInfo.Type == InstanceSource.Local)
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
        private List<InstanceAddon> InstalledAddonsHandle(AddonType addonType, string folderName, string fileExtension, IternalAddonInfoGetter infoHandler)
        {
            _synchronizer.AddonsHandleSemaphore.WaitOne();

            string clientPath = WithDirectory.GetInstancePath(_modpackInfo.LocalId);
            var addons = new List<InstanceAddon>();

            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            var existsCfMods = new HashSet<string>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsMdMods = new HashSet<string>(); // аналогично existsCfMods, но для модринфа
            var existsUnknownAddons = new HashSet<string>(); // аддоны, которые существуют (есть в папке и спсике), но не имеют источника
            var existsAddons = new Dictionary<string, SetValues<InstanceAddon, string, ProjectSource>>(); // ключ - имя файла, значение - экзмепляр,айдишник и источник проекта. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.

            using (InstalledAddons installedAddons = InstalledAddons.Get(_modpackInfo.LocalId))
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
                                IPrototypeAddon prototypeAddon = new CurseforgeAddon(_modpackInfo, addon);
                                CreateAddonData(addonType, prototypeAddon, projectId, existsAddons, addons, actualAddonsList, ProjectSource.Curseforge);
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
                            IPrototypeAddon prototypeAddon = new ModrinthAddon(_modpackInfo, addon);
                            CreateAddonData(addonType, prototypeAddon, projectId, existsAddons, addons, actualAddonsList, ProjectSource.Modrinth);
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
                    _synchronizer.AddonsHandleSemaphore.Release();
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
                        string xyi = fileAddr_.Replace(WithDirectory.GetInstancePath(_modpackInfo.LocalId), "");
                        if (!existsUnknownAddons.Contains(xyi) && !existsAddons.ContainsKey(xyi))
                        {
                            unknownAddons.Add(fileAddr);
                        }
                    }
                }

                Dictionary<string, IPrototypeAddon> addonsData = null;
                if (unknownAddons.Count > 0)
                {
                    addonsData = AddonsPrototypesCreater.CreateFromFiles(_modpackInfo, unknownAddons);
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
                        string xyi = fileAddr_.Replace(WithDirectory.GetInstancePath(_modpackInfo.LocalId), "");

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

                                    var obj = CreateAddonData(addonType, addonData, addonData.ProjectId, existsAddons, addons, actualAddonsList, addonData.Source);
                                    obj.FileName = filename;
                                    obj.SetIsEnable = isAddonExtension;

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

                            var inastnceAddon = new InstanceAddon(addonType, _synchronizer, addonId, _modpackInfo)
                            {
                                Author = authors,
                                Description = description,
                                Name = displayName,
                                FileName = filename,
                                Version = (!version.Contains("{") ? version : ""),
                                SetIsEnable = isAddonExtension
                            };

                            addons.Add(inastnceAddon);

                            _synchronizer.AddInstalledAddonWithoutEvent(inastnceAddon);
                        }
                        else
                        {
                            // тут пытаемся получить инфу о моде
                            //infoHandler(fileAddr_, out string displayName, out string authors, out string version, out string description, out string modId);
                            InstanceAddon obj = existsAddons[xyi].Value1;
                            obj.FileName = filename;
                            obj.SetIsEnable = isAddonExtension;
                            //obj.Version = version;
                            //if (string.IsNullOrWhiteSpace(obj.Author)) obj.Author = authors;
                        }
                    }
                }

                Runtime.DebugWrite("End");

                installedAddons.Save(actualAddonsList);
            }

            Runtime.DebugWrite(addonType + " " + addons.Count);
            _synchronizer.AddonsHandleSemaphore.Release();
            return addons;
        }

        private InstanceAddon AddonFileHandle(string filePath, AddonType addonType, string fileExtension, IternalAddonInfoGetter infoHandler)
        {
            _synchronizer.AddonsHandleSemaphore.WaitOne();
            // определяем айдишник
            int addonId_;
            string addonId;

            string fileAddr_ = filePath.Replace('\\', '/');
            string extension = Path.GetExtension(fileAddr_);
            bool isAddonExtension = (extension == fileExtension), isDisable = (extension == DISABLE_FILE_EXTENSION);

            if (!isAddonExtension && !isDisable)
            {
                _synchronizer.AddonsHandleSemaphore.Release();
                return null;
            }

            string filename = "";
            try
            {
                filename = Path.GetFileName(filePath);
            }
            catch { }

            string xyi = fileAddr_.Replace(WithDirectory.GetInstancePath(_modpackInfo.LocalId), "");

            IPrototypeAddon addonData = AddonsPrototypesCreater.CreateFromFile(_modpackInfo, filePath);
            if (addonData == null) return null;

            using (InstalledAddons installedAddons = InstalledAddons.Get(_modpackInfo.LocalId))
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

                    var obj = new InstanceAddon(addonType, _synchronizer, addonData, _modpackInfo)
                    {
                        Version = ""
                    };

                    // проверяем наличие обновлений для мода
                    if (_modpackInfo.Type == InstanceSource.Local)
                    {
                        addonData.CompareVersions(addonData.FileId, () =>
                        {
                            obj.UpdateAvailable = true;
                        });
                    }

                    obj.FileName = filename;
                    obj.SetIsEnable = isAddonExtension;

                    _synchronizer.AddonsHandleSemaphore.Release();
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

                _synchronizer.AddonsHandleSemaphore.Release();
                return new InstanceAddon(addonType, _synchronizer, addonId, _modpackInfo)
                {
                    Author = authors,
                    Description = description,
                    Name = displayName,
                    FileName = filename,
                    Version = (!version.Contains("{") ? version : ""),
                    SetIsEnable = isAddonExtension
                };
            }
        }


        /// <summary>
        /// Возвращает список модов. При вызове так же сохраняет список модов, 
        /// анализирует папку mods и пихает в список моды которые были в папке, но которых не было в списке.
        /// </summary>
        /// <param name="modpackInfo">Инфа о модпаке с которого нужно получить список модов</param>
        public List<InstanceAddon> GetInstalledMods()
        {
            return InstalledAddonsHandle(AddonType.Mods, "mods", ".jar", DefineIternalModInfo);
        }

        public List<InstanceAddon> GetInstalledResourcepacks()
        {
            IternalAddonInfoGetter addonInfo = delegate (string fileAddr, out string displayName, out string authors, out string version, out string description, out string modId)
            {
                displayName = UNKNOWN_NAME;
                authors = "";
                version = "";
                description = "";
                modId = "";
            };

            return InstalledAddonsHandle(AddonType.Resourcepacks, "resourcepacks", ".zip", addonInfo);
        }

        public List<InstanceAddon> GetInstalledWorlds()
        {
            string clientPath = WithDirectory.GetInstancePath(_modpackInfo.LocalId);
            List<InstanceAddon> addons = new List<InstanceAddon>();
            InstalledAddonsFormat actualAddonsList = new InstalledAddonsFormat(); //актуальный список аддонов, то есть те аддоны которы действительно существуют и есть в списке. В конце именно этот спсиок будет сохранен в файл
            var existsCfMods = new HashSet<string>(); // айдишники модов которые действителньо существуют (есть и в списке и в папке) и скачаны с курсфорджа
            var existsAddons = new Dictionary<string, SetValues<InstanceAddon, string>>(); // ключ - имя файла, значение - экзмепляр и айдишник. Этот список нужен чтобы при прохожднии циклом по папке быстро определить был ли этот аддон в списке.
            using (InstalledAddons installedAddons = InstalledAddons.Get(_modpackInfo.LocalId))
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
                                IPrototypeAddon prototypeAddon = new CurseforgeAddon(_modpackInfo, addon);

                                InstalledAddonInfo info = actualAddonsList[projectId];
                                var obj = new InstanceAddon(AddonType.Maps, _synchronizer, prototypeAddon, _modpackInfo)
                                {
                                    Version = ""
                                };

                                // проверяем наличие обновлений для мода
                                if (_modpackInfo.Type == InstanceSource.Local)
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
        /// Очищает сохранённый список аддонов. Нужно вызывать при закрытии каталога чтобы очистить память.
        /// </summary>
        public void ClearAddonsListCache() => _synchronizer.ClearAddonsListCache();
    }
}
