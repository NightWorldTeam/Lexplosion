using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NightWorld.Tools.Minecraft.NBT.StorageFiles;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Management.Sources;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Importers;

namespace Lexplosion.Logic.Management.Instances
{
    /// <summary>
    /// Филиал ада в c#.
    /// </summary>
    public class InstanceClient : VMBase
    {
        public readonly InstanceSource Type;
        private string _externalId = null;
        private string _localId = null;
        private readonly PrototypeInstance _dataManager;
        private readonly IInstanceSource _instanceSource;

        private CancellationTokenSource _cancelTokenSource = null;
        private LaunchGame _gameManager = null;

        private const string LogoFileName = "logo.png";
        private const string UnknownName = "Unknown name";
        private const string UnknownAuthor = "Unknown author";
        private const string NoDescription = "Описания нет, но мы надеемся что оно будет.";

        private static Dictionary<string, InstanceClient> _installedInstances = new Dictionary<string, InstanceClient>();

        /// <summary>
        /// Содержит пары состоящие из внешнего и внутреннего id.
        /// </summary>
        private static Dictionary<string, string> _idsPairs = new Dictionary<string, string>();

        #region events
        public event ProgressHandlerCallback ProgressHandler;
        public event DownloadComplitedCallback DownloadComplited;
        public event LaunchComplitedCallback LaunchComplited;
        public event GameExitedCallback GameExited;

        /// <summary>
        /// Используется, для того чтобы сообщить InstanceFormViewModel,
        /// что данные обновились, и нужно обновить инфу о данных.
        /// </summary>
        public event Action StateChanged;
        /// <summary>
        /// Обновляется после того как InstanceClient будет иметь завершенную версию;
        /// </summary>
        public event Action BuildFinished;
        public event Action<string, int, DownloadFileProgress> FileDownloadEvent;
        public event Action DownloadStarted;
        public event Action DownloadCanceled;
        public static event Action Created;
        /// <summary>
        /// 
        /// </summary>
        public event Action NameChanged;
        public event Action SummaryChanged;
        public event Action DescriptionChanged;
        public event Action LogoChanged;
        public event Action GameVersionChanged;
        #endregion

        #region info
        public string LocalId { get => _localId; }

        private string _name;
        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
                NameChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            private set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private byte[] _logo = null;
        public byte[] Logo
        {
            get => _logo;
            private set
            {
                try
                {
                    if (value != null)
                    {
                        _logo = ImageTools.ResizeImage(value, 120, 120);
                    }
                    else
                    {
                        _logo = null;
                    }
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite(ex);
                    _logo = null;
                }

                LogoChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        public IEnumerable<CategoryBase> Categories { get; private set; }

        private MinecraftVersion _gameVersion;
        public MinecraftVersion GameVersion
        {
            get => _gameVersion;
            private set
            {
                _gameVersion = value;
                GameVersionChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        private string _summary;
        public string Summary
        {
            get => _summary;
            private set
            {
                _summary = value;
                OnPropertyChanged();
                SummaryChanged?.Invoke();
                StateChanged?.Invoke();
            }
        }

        private string _author;
        public string Author
        {
            get => _author;
            private set
            {
                _author = value;
                OnPropertyChanged();
            }
        }

        private bool _inLibrary = false;
        public bool InLibrary
        {
            get => _inLibrary;
            private set
            {
                _inLibrary = value;
                OnPropertyChanged();
                StateChanged?.Invoke();
            }
        }

        private bool _updateAvailable = false;
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;
                OnPropertyChanged();
                StateChanged?.Invoke();
            }
        }

        private string _profileVersion = null;
        /// <summary>
        /// Описывает версию установленное сборки. Нужен только для того, чтобы снаружи можно было получить версию
        /// </summary>
        public string ProfileVersion
        {
            get => _profileVersion;
            private set
            {
                _profileVersion = value;
                OnPropertyChanged();
            }
        }

        private string _websiteUrl = null;
        public string WebsiteUrl
        {
            get => _websiteUrl;
            private set
            {
                _websiteUrl = value;
                StateChanged?.Invoke();
            }
        }

        private bool _isComplete = true;
        public bool IsComplete
        {
            get => _isComplete;
            private set
            {
                _isComplete = value;
                OnPropertyChanged();
                if (value)
                {
                    BuildFinished?.Invoke();
                }
            }
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            private set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }

        public bool IsSharing { get; private set; } = false;

        public bool IsInstalled { get; private set; } = false;

        public string FolderPath { get => WithDirectory.GetInstancePath(_localId); }

        #endregion

        /// <summary>
        /// Если нужно скачать какую-то определенную версию клиента при первом вызове Update, то её присваивать сюда.
        /// Если результат работаты будет удачным, то сюда присвоится значение null чтобы не произошло повторное перекачивание.
        /// Null означает что методу Update нужно действовать как обычно.
        /// </summary>
        private string _instanceVersionToDownload = null;

        /// <summary>
        /// Базовый конструктор, от него должны наследоваться все остальные
        /// </summary>
        /// <param name="source">Источник модпака</param>
        private InstanceClient(IInstanceSource source)
        {
            Type = source.SourceType;
            _instanceSource = source;
            _dataManager = source.ContentManager;

            GameExited += delegate (string _)
            {
                _gameManager = null;
            };
        }

        /// <summary>
        /// Этот конструктор создаёт еще не установленную сборку. Используется для сборок из каталога
        /// </summary>
        /// <param name="source">Источник модпака</param>
        /// <param name="externalID">Внешний ID</param>
        private InstanceClient(IInstanceSource source, string externalID) : this(source)
        {
            _externalId = externalID;
        }

        /// <summary>
        /// Этот конструктор создаёт установленную сборку. Используется для сборок в библиотеке.
        /// </summary>
        /// <param name="source">Источник модпака</param>
        /// <param name="externalID">Внешний ID</param>
        /// <param name="externalID">Локальный ID</param>
        private InstanceClient(IInstanceSource source, string externalID, string localId) : this(source, externalID)
        {
            _localId = localId;
        }

        /// <summary>
        /// Этот конструктор создаёт локальную сборку. Должен использоваться только при создании локальной сборки и при импорте.
        /// </summary>
        /// <param name="name">Название сборки</param>
        /// <param name="source">Источник модпака</param>
        /// <param name="gameVersion">Версия игры</param>
        private InstanceClient(string name, IInstanceSource source, MinecraftVersion gameVersion, string externalID) : this(source, externalID)
        {
            Name = name;
            GameVersion = gameVersion;
            GenerateInstanceId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IInstanceSource CreateSourceFactory(InstanceSource type)
        {
            switch (type)
            {
                case InstanceSource.Local:
                    return new LocalSource();
                case InstanceSource.Nightworld:
                    return new NightWorldSource();
                case InstanceSource.Curseforge:
                    return new CurseforgeSource();
                case InstanceSource.Modrinth:
                    return new ModrinthSource();
                case InstanceSource.FreeSource:
                    return new FreeSource();
                default:
                    {
                        Runtime.DebugWrite("CreateSourceFactory error. type: " + type);
                        return new LocalSource();
                    }
            }
        }

        /// <summary>
        /// Этот метод создаёт локальную сборку. Должен использоваться только при создании локальной сборки.
        /// </summary>
        /// <param name="name">Название сборки</param>
        /// <param name="gameVersion">Версия игры</param>
        /// <param name="modloader">Тип модлоадера</param>
        /// <param name="logoPath">Путь до логотипа. Если устанавливать не надо, то null.</param>
        /// <param name="modloaderVersion">Версия модлоадера. Это поле необходимо только если есть модлоадер</param>
        /// <param name="optifineVersion">Версия оптифайна. Если оптифайн не нужен - то null.</param>
        /// <param name="sodium">Устанавливать ли sodium</param>
        public static InstanceClient CreateClient(string name, InstanceSource type, MinecraftVersion gameVersion, ClientType modloader, string logoPath = null, string modloaderVersion = null, string optifineVersion = null, bool sodium = false)
        {
            return CreateClient(CreateSourceFactory(type), name, type, gameVersion, modloader, logoPath: logoPath, modloaderVersion: modloaderVersion, optifineVersion: optifineVersion, sodium: sodium);
        }

        private static InstanceClient CreateClient(IInstanceSource source, string name, InstanceSource type, MinecraftVersion gameVersion, ClientType modloader, string logoPath = null, string modloaderVersion = null, string optifineVersion = null, bool sodium = false, string externalId = null)
        {
            if (modloaderVersion == null) modloader = ClientType.Vanilla;

            var client = new InstanceClient(name, source, gameVersion, externalId)
            {
                InLibrary = true,
                Author = Account.AnyFuckingLogin,
                Description = NoDescription,
                Summary = NoDescription
            };

            try
            {
                if (logoPath != null && File.Exists(logoPath))
                {
                    client.Logo = File.ReadAllBytes(logoPath);
                }
            }
            catch { }

            AdditionalInstallerType? installer = null;
            string installerVer = null;
            if (optifineVersion != null)
            {
                installerVer = optifineVersion;
                installer = AdditionalInstallerType.Optifine;
            }

            client.CreateFileStruct(modloader, modloaderVersion, installer, installerVer);
            client.SaveAssets();

            _installedInstances[client._localId] = client;
            SaveInstalledInstancesList();

            if (sodium && (modloader == ClientType.Fabric || modloader == ClientType.Quilt))
            {
                ThreadPool.QueueUserWorkItem(delegate (object o)
                {
                    var sodium = ModrinthApi.GetProject("AANobbMI");
                    var addon = InstanceAddon.CreateModrinthAddon(client.GetBaseData, sodium);
                    var stateData = new DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>(delegate (SetValues<InstanceAddon, DownloadAddonRes> a, InstanceAddon.InstallAddonState b) { });
                    addon.InstallLatestVersion(stateData, downloadDependencies: true);
                });
            }

            Created?.Invoke();

            return client;
        }

        public static InstanceClient CreateClient(MinecraftServerInstance server, bool autoLogin)
        {
            string name = server.Name;
            var minecraftVersion = new MinecraftVersion(server.GameVersion);

            IInstanceSource source;
            string externalId = null;
            string modpackVersion = null;
            if (server.InstanceSource == InstanceSource.FreeSource)
            {
                source = new FreeSource(server.ModpackInfo.SourceId, null);
            }
            else
            {
                source = CreateSourceFactory(server.InstanceSource);
            }

            externalId = server.ModpackInfo.ModpackId;
            modpackVersion = server.ModpackInfo.Version;

            var client = CreateClient(source, name, server.InstanceSource, minecraftVersion, ClientType.Vanilla, externalId: externalId);
            client.AddGameServer(server, autoLogin);
            client._instanceVersionToDownload = modpackVersion;

            if (server.Tags != null)
            {
                client.Categories = server.Tags.Select((x) => new SimpleCategory(x.Id, x.Name));
            }

            if (server.Description != null)
            {
                client.Description = server.Description;
                client.Summary = server.Description.Truncate(45);
            }

            client.SaveAssets();

            if (!string.IsNullOrWhiteSpace(server.IconUrl))
            {
                client.DownloadLogo(server.IconUrl, client.SaveAssets);
            }

            Settings settings = client.GetSettings();
            settings.IsAutoUpdate = true;
            client.SaveSettings(settings);

            return client;
        }

        /// <summary>
        /// Возвращает основные данные модпака.
        /// </summary>
        public BaseInstanceData GetBaseData
        {
            get
            {
                VersionManifest manifest = DataFilesManager.GetManifest(_localId, false);

                return new BaseInstanceData
                {
                    LocalId = _localId,
                    ExternalId = _externalId,
                    Type = Type,
                    GameVersion = manifest?.version?.GameVersionInfo,
                    InLibrary = InLibrary,
                    Author = Author,
                    Categories = Categories ?? new List<CategoryBase>(),
                    Description = Description,
                    Name = _name,
                    Summary = _summary,
                    ModloaderVersion = manifest?.version?.ModloaderVersion ?? string.Empty,
                    Modloader = manifest?.version.ModloaderType ?? ClientType.Vanilla,
                    OptifineVersion = manifest?.version?.AdditionalInstaller?.installerVersion
                };
            }
        }

        /// <summary>
        /// Сохраняем список установленных сборок (библиотеку) в файл instanesList.json.
        /// </summary>
        public static void SaveInstalledInstancesList()
        {
            // деаем список всех установленных сборок
            var list = new InstalledInstancesFormat();
            foreach (var inst in _installedInstances.Keys)
            {
                list[inst] = new InstalledInstance
                {
                    Name = _installedInstances[inst].Name,
                    Type = _installedInstances[inst].Type,
                    IsInstalled = _installedInstances[inst].IsInstalled,
                };
            }

            // сохраняем этот список
            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instanesList.json", JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// Заполняет список установленных сборок. Вызывается 1 раз, в Main при запуске лаунчера
        /// </summary>
        public static void DefineInstalledInstances()
        {
            var list = DataFilesManager.GetFile<InstalledInstancesFormat>(WithDirectory.DirectoryPath + "/instanesList.json");

            if (list != null)
            {
                foreach (string localId in list.Keys)
                {
                    VersionManifest instanceManifest = DataFilesManager.GetManifest(localId, false);
                    bool manifestIsCorrect =
                        (instanceManifest != null && instanceManifest.version != null && instanceManifest.version.GameVersion != null);

                    //проверяем имеется ли манифест, не содержит ли его id запрещенных символов
                    if (manifestIsCorrect && !ForbiddenIsCharsExists(localId))
                    {
                        IInstanceSource sourceFactory = CreateSourceFactory(list[localId].Type);
                        if (sourceFactory == null)
                        {
                            continue;
                        }

                        string externalID = null;
                        string instanceVersion = null;
                        byte[] logo = null;

                        //получаем вншний айдшник и версию, если этот модпак не локлаьный
                        if (list[localId].Type != InstanceSource.Local)
                        {
                            InstancePlatformData data = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.InstancesPath + localId + "/instancePlatformData.json");
                            if (data?.instanceVersion != null && data.id != null)
                            {
                                externalID = data.id;
                                instanceVersion = data.instanceVersion;
                                _idsPairs[externalID] = localId;
                            }
                        }

                        //получаем асетсы модпаков
                        var assetsData = DataFilesManager.GetFile<InstanceAssetsFileDecodeFormat>(WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/assets.json");

                        if (assetsData != null)
                        {
                            string file = WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/" + LogoFileName;
                            if (File.Exists(file))
                            {
                                try
                                {
                                    logo = File.ReadAllBytes(file);
                                }
                                catch { }
                            }
                        }

                        InstanceClient instance;
                        if (assetsData != null)
                        {
                            instance = new InstanceClient(sourceFactory, externalID, localId)
                            {
                                Name = list[localId].Name ?? UnknownName,
                                Summary = assetsData.Summary ?? NoDescription,
                                Author = assetsData.Author ?? UnknownAuthor,
                                Description = assetsData.Description ?? NoDescription,
                                Categories = assetsData.Categories ?? new List<SimpleCategory>(),
                                GameVersion = instanceManifest.version?.GameVersionInfo,
                                Logo = logo,
                                _profileVersion = instanceVersion
                            };
                        }
                        else
                        {
                            instance = new InstanceClient(sourceFactory, externalID, localId)
                            {
                                Name = list[localId].Name ?? UnknownName,
                                Summary = NoDescription,
                                Author = UnknownAuthor,
                                Description = NoDescription,
                                Categories = new List<CategoryBase>(),
                                GameVersion = instanceManifest.version?.GameVersionInfo,
                                Logo = logo,
                                _profileVersion = instanceVersion
                            };
                        }

                        instance.InLibrary = true;
                        instance.IsInstalled = list[localId].IsInstalled;
                        //хуярим проверку обновлений в пуле потоков
                        ThreadPool.QueueUserWorkItem(delegate (object state)
                        {
                            instance.CheckUpdates();
                        });
                        _installedInstances[localId] = instance;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает список модпаков для библиотеки.
        /// </summary>
        /// <returns>Список установленных модпаков.</returns>
        public static List<InstanceClient> GetInstalledInstances(InstanceGroup group)
        {
            return new List<InstanceClient>(_installedInstances.Values);
        }

        public static List<InstanceClient> GetInstalledInstances() => GetInstalledInstances(InstanceGroup.AllInstances);

        /// <summary>
        /// Возвращает список модпаков для каталога.
        /// </summary>
        /// <returns>Список внешних модпаков.</returns>
        public static (List<InstanceClient>, uint) GetOutsideInstances(InstanceSource type, ISearchParams searchParams)
        {
            Runtime.DebugWrite("UploadInstances " + searchParams.PageIndex);

            IInstanceSource source = CreateSourceFactory(type);

            var instances = new List<InstanceClient>();
            List<InstanceInfo> catalog = source.GetCatalog(type, searchParams);

            foreach (var instance in catalog)
            {
                InstanceClient instanceClient;
                if (_idsPairs.ContainsKey(instance.ExternalId))
                {
                    instanceClient = _installedInstances[_idsPairs[instance.ExternalId]];
                    instanceClient.CheckUpdates();

                    if (instance.Name != null)
                        instanceClient.Name = instance.Name;
                    if (instance.Categories != null)
                        instanceClient.Categories = instance.Categories;
                    if (instance.Summary != null)
                        instanceClient.Summary = instance.Summary;
                    if (instance.Description != null)
                        instanceClient.Description = instance.Description;
                    if (instance.Author != null)
                        instanceClient.Author = instance.Author;
                    if (instance.WebsiteUrl != null)
                        instanceClient.WebsiteUrl = instance.WebsiteUrl;

                    instanceClient.DownloadLogo(instance.LogoUrl, instanceClient.SaveAssets);
                }
                else
                {
                    instanceClient = new InstanceClient(source, instance.ExternalId)
                    {
                        Name = instance.Name ?? UnknownName,
                        Logo = null,
                        Categories = instance.Categories,
                        GameVersion = instance.GameVersion,
                        Summary = instance.Summary ?? NoDescription,
                        Description = instance.Description ?? NoDescription,
                        Author = instance.Author,
                        WebsiteUrl = instance.WebsiteUrl
                    };

                    instanceClient.DownloadLogo(instance.LogoUrl, delegate { });
                }

                instances.Add(instanceClient);
            }

            return (instances, (uint)100);
        }

        /// <summary>
        /// Получает внешнюю сборку по id
        /// </summary>
        /// <returns>Экземпляр клиента.</returns>
        public static InstanceClient GetInstance(InstanceSource type, string instanceId)
        {
            PrototypeInstance.Info instance = PrototypeInstance.GetInstance(type, instanceId);

            InstanceClient instanceClient;
            if (instance != null)
            {
                // TODO: тут пока нет необходимости получать лого, но потом она может появиться
                if (_idsPairs.ContainsKey(instance.ExternalId))
                {
                    instanceClient = _installedInstances[_idsPairs[instance.ExternalId]];
                    instanceClient.CheckUpdates();

                    if (instance.Name != null)
                        instanceClient.Name = instance.Name;
                    if (instance.Categories != null)
                        instanceClient.Categories = instance.Categories;
                    if (instance.Summary != null)
                        instanceClient.Summary = instance.Summary;
                    if (instance.Description != null)
                        instanceClient.Description = instance.Description;
                    if (instance.Author != null)
                        instanceClient.Author = instance.Author;
                    if (instance.Author != null)
                        instanceClient.WebsiteUrl = instance.WebsiteUrl;
                }
                else
                {
                    instanceClient = new InstanceClient(CreateSourceFactory(type), instance.ExternalId)
                    {
                        Name = instance.Name ?? UnknownName,
                        Logo = null,
                        Categories = instance.Categories,
                        GameVersion = new MinecraftVersion(instance.GameVersion),
                        Summary = instance.Summary ?? NoDescription,
                        Description = instance.Description ?? NoDescription,
                        Author = instance.Author,
                        WebsiteUrl = instance.WebsiteUrl
                    };
                }

                return instanceClient;
            }

            return null;
        }

        /// <summary>
        /// Получает всю информацию о модпаке.
        /// </summary>
        /// <returns>InstanceData, содержащий в себе данные которые могут потребоваться</returns>
        public InstanceData GetFullInfo()
        {
            InstanceData fullInfo = _dataManager.GetFullInfo(_localId, _externalId);

            if (fullInfo == null)
            {
                return null;
            }

            if (fullInfo.Description == null)
                fullInfo.Description = NoDescription;
            if (fullInfo.Summary == null)
                fullInfo.Summary = NoDescription;

            return fullInfo;
        }

        public List<InstanceVersion> GetVersions()
        {
            return _dataManager.GetVersions(_externalId) ?? new List<InstanceVersion>();
        }

        /// <summary>
        /// Изменяет параметры установленного модпака
        /// </summary>
        /// <param name="data">Вся инфа.</param>
        /// <param name="logoPath">Путь до лого. Если логотип изменять не нужно, то null</param>
        public void ChangeParameters(BaseInstanceData data, string logoPath)
        {
            VersionManifest manifest = DataFilesManager.GetManifest(_localId, false);
            if (manifest != null)
            {
                manifest.version.ModloaderType = data.Modloader;
                manifest.version.ModloaderVersion = data.ModloaderVersion;
                manifest.version.GameVersionInfo = data.GameVersion;

                if (manifest.version.ModloaderType == ClientType.Vanilla && data.OptifineVersion != null)
                {
                    manifest.version.AdditionalInstaller = new AdditionalInstaller()
                    {
                        type = AdditionalInstallerType.Optifine,
                        installerVersion = data.OptifineVersion
                    };
                }

                if (string.IsNullOrWhiteSpace(data.OptifineVersion))
                {
                    manifest.version.AdditionalInstaller = null;
                }

                DataFilesManager.SaveManifest(_localId, manifest);
            }

            try
            {
                if (logoPath != null && File.Exists(logoPath))
                {
                    Logo = File.ReadAllBytes(logoPath);
                }
            }
            catch { }

            Description = data.Description;
            GameVersion = data.GameVersion;
            Summary = data.Summary;
            Categories = data.Categories;
            SaveAssets();

            Name = data.Name;
            SaveInstalledInstancesList();
        }

        /// <summary>
        /// Останавливает игру
        /// </summary>
        public void StopGame()
        {
            _gameManager?.Stop();
        }

        /// <summary>
        /// Отменяет скачивание сборки.
        /// </summary>
        public void CancelDownload()
        {
            _cancelTokenSource?.Cancel();
        }

        /// <summary>
        /// Обновляет или скачивает сборку. Сборка должна быть добавлена в библиотеку.
        /// </summary>
        public void Update(string instanceVersion = null)
        {
            _cancelTokenSource = new CancellationTokenSource();
            ProgressHandler?.Invoke(StageType.Prepare, new ProgressHandlerArguments());

            IsUpdating = true;

            Settings instanceSettings = GetSettings();
            instanceSettings.Merge(GlobalData.GeneralSettings, true);

            instanceVersion ??= _instanceVersionToDownload;

            var generalSettings = GlobalData.GeneralSettings;
            var activeAccount = Account.ActiveAccount?.IsAuthed == true ? Account.ActiveAccount : null;
            var launchAccount = Account.LaunchAccount;

            LaunchGame launchGame = new LaunchGame(_localId, generalSettings, instanceSettings, activeAccount, launchAccount, _instanceSource, _cancelTokenSource.Token);
            InitData data = launchGame.Update(ProgressHandler, FileDownloadEvent, DownloadStarted, instanceVersion);

            UpdateAvailable = data.UpdatesAvailable;
            ProfileVersion = data.ClientVersion;

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = (data.InitResult == InstanceInit.Successful);
                _instanceVersionToDownload = null;

                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
            }
            else if (data.InitResult == InstanceInit.IsCancelled)
            {
                DownloadCanceled?.Invoke();
            }

            IsUpdating = false;

            DownloadComplited?.Invoke(data.InitResult, data.DownloadErrors, false);
            Runtime.DebugWrite("UpdateInstance-end " + data.InitResult);

            _cancelTokenSource = null;
            _gameManager?.DeleteCancellationToken();
        }

        /// <summary>
        /// Запускает сборку. Если надо её докачивает. Сборка должна быть доавлена в библиотеку
        /// </summary>
        public void Run()
        {
            _cancelTokenSource = new CancellationTokenSource();

            ProgressHandler?.Invoke(StageType.Prepare, new ProgressHandlerArguments());

            IsUpdating = true;

            Settings instanceSettings = GetSettings();
            instanceSettings.Merge(GlobalData.GeneralSettings, true);

            var generalSettings = GlobalData.GeneralSettings;
            var activeAccount = Account.ActiveAccount?.IsAuthed == true ? Account.ActiveAccount : null;
            var launchAccount = Account.LaunchAccount;

            _gameManager = new LaunchGame(_localId, generalSettings, instanceSettings, activeAccount, launchAccount, _instanceSource, _cancelTokenSource.Token);
            InitData data = _gameManager.Initialization(ProgressHandler, FileDownloadEvent, DownloadStarted);

            UpdateAvailable = data.UpdatesAvailable;
            ProfileVersion = data.ClientVersion;

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = true;
                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
                DownloadComplited?.Invoke(data.InitResult, data.DownloadErrors, true);

                _gameManager.Run(data, LaunchComplited, GameExited, Name);
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                // TODO: тут надо как-то определять что сборка обновилась и UpdateAvailable = false делать, если было обновление
            }
            else
            {
                DownloadComplited?.Invoke(data.InitResult, data.DownloadErrors, false);
            }

            IsUpdating = false;

            Runtime.DebugWrite("Run-end " + data.InitResult);

            _cancelTokenSource = null;
            _gameManager?.DeleteCancellationToken();
        }

        /// <summary>
        /// Добавляет сборку в библиотеку
        /// </summary>
        public void AddToLibrary()
        {
            if (!InLibrary)
            {
                GenerateInstanceId();
                CreateFileStruct(ClientType.Vanilla, string.Empty);
                _installedInstances[_localId] = this;
                _idsPairs[_externalId] = _localId;
                SaveInstalledInstancesList();
                SaveAssets();
                InLibrary = true;
            }
        }

        private void CheckUpdates()
        {
            UpdateAvailable = _dataManager.CheckUpdates(_localId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ForbiddenIsCharsExists(string str)
        {
            str = str.Replace("_", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace(" ", "").Replace(".", "");
            return Regex.IsMatch(str, @"[^a-zA-Z0-9]");
        }

        private void GenerateInstanceId()
        {
            string instanceId = Name.ToLower();

            // переводим русские символы в транслит
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                instanceId = instanceId.Replace(rus_up[i], lat_low[i]).Replace(rus_low[i], lat_low[i]);
            }

            instanceId = instanceId.Replace("&", "AND");

            if (ForbiddenIsCharsExists(instanceId))
            {
                int j = 0;
                while (j < instanceId.Length)
                {
                    if (ForbiddenIsCharsExists(instanceId[j].ToString()))
                    {
                        instanceId = instanceId.Replace(instanceId[j], '_');
                    }
                    j++;
                }

                if (_installedInstances.ContainsKey(instanceId))
                {
                    string instanceId_ = instanceId;
                    int i = 0;
                    do
                    {
                        if (i > 0)
                        {
                            instanceId_ = instanceId + " (" + i + ")";
                        }
                        i++;
                    }
                    while (_installedInstances.ContainsKey(instanceId_));
                    instanceId = instanceId_;
                }
            }
            else if (_installedInstances.ContainsKey(instanceId))
            {
                string instanceId_ = instanceId;
                int i = 0;
                do
                {
                    instanceId_ = instanceId + "_" + i;
                    i++;
                }
                while (_installedInstances.ContainsKey(instanceId_));

                instanceId = instanceId_;
            }

            _localId = instanceId;
        }

        /// <summary>
        /// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo. Делает это всё в пуле потоков.
        /// </summary>
        private void DownloadLogo(string url, Action callback)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Proxy = null;
                        Logo = webClient.DownloadData(url);
                        callback();
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Сохраняет асетсы клиента в файл.
        /// </summary>
        private void SaveAssets()
        {
            try
            {
                if (!Directory.Exists(WithDirectory.DirectoryPath + "/instances-assets/" + _localId))
                {
                    Directory.CreateDirectory(WithDirectory.DirectoryPath + "/instances-assets/" + _localId);
                }

                if (Logo != null)
                {
                    File.WriteAllBytes(WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName, Logo);
                }
            }
            catch { }

            string file = WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/assets.json";
            InstanceAssetsFileDecodeFormat assetsData_ = DataFilesManager.GetFile<InstanceAssetsFileDecodeFormat>(file);

            var categories_ = new List<SimpleCategory>();
            if (Categories != null)
            {
                foreach (var category in Categories)
                {
                    categories_.Add(new SimpleCategory(category));
                }
            }

            var assetsData = new InstanceAssets
            {
                Author = Author,
                Categories = categories_,
                Description = Description,
                Images = (assetsData_ != null) ? assetsData_.Images : null,
                Summary = Summary
            };

            DataFilesManager.SaveFile(file, JsonConvert.SerializeObject(assetsData));
        }

        /// <summary>
        /// Создает необходимую структуру файлов для сборки при её добавлении в библиотеку (ну при создании локальной).
        /// </summary>
        /// <param name="modloader">Тип модлоадера</param>
        /// <param name="modloaderVersion">Версия модлоадера</param>
        /// <param name="additionalInstaller">Оптифайн. Если не нужен, то null</param>
        /// <param name="additionalInstallerVer">Версия оптифайна. null если не нужен</param>
        private void CreateFileStruct(ClientType modloader, string modloaderVersion, AdditionalInstallerType? additionalInstaller = null, string additionalInstallerVer = null)
        {
            // TODO: тут надо трай. И если будет исключение надо передавать ошибку

            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            VersionManifest manifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    GameVersionInfo = GameVersion,
                    ModloaderVersion = modloaderVersion,
                    ModloaderType = modloader
                }
            };

            if (additionalInstaller != null && additionalInstallerVer != null)
            {
                manifest.version.AdditionalInstaller = new AdditionalInstaller()
                {
                    type = additionalInstaller ?? AdditionalInstallerType.Optifine,
                    installerVersion = additionalInstallerVer
                };
            }

            DataFilesManager.SaveManifest(_localId, manifest);

            if (_instanceSource != null)
            {
                InstancePlatformData instanceData = _instanceSource.CreateInstancePlatformData(_externalId, _localId, null);
                if (instanceData != null)
                {
                    DataFilesManager.SaveFile(FolderPath + "/instancePlatformData.json", JsonConvert.SerializeObject(instanceData));
                }
            }
            else if (Type != InstanceSource.Local)
            {
                var instanceData = new InstancePlatformData
                {
                    id = _externalId
                };

                DataFilesManager.SaveFile(FolderPath + "/instancePlatformData.json", JsonConvert.SerializeObject(instanceData));
            }
        }

        public string GetDirectoryPath()
        {
            return (WithDirectory.DirectoryPath.Replace("/", @"\") + @"\instances\" + _localId).Replace(@"\\", @"\");
        }

        public Settings GetSettings()
        {
            return DataFilesManager.GetSettings(_localId);
        }

        public void SaveSettings(Settings settings)
        {
            DataFilesManager.SaveSettings(settings, _localId);
        }

        /// <summary>
        /// Удаляет сборку к хуям.
        /// </summary>
        public void Delete()
        {
            WithDirectory.DeleteInstance(_localId);
            _installedInstances.Remove(_localId);
            if (_externalId != null)
            {
                _idsPairs.Remove(_externalId);
            }
            UpdateAvailable = false;
            InLibrary = false;
            IsInstalled = false;
            SaveInstalledInstancesList();
        }

        /// <summary>
        /// Возвращает список файлов и папок данной директории в папки модпака. 
        /// </summary>
        /// <param name="path">
        /// Путь до директории, содержание котрой необходимо вернуть. Путь указывается относительно папки модпака.
        /// Если нужно получить список элементов из корня папки модпака, то ничего передавать не надо.
        /// </param>
        /// <returns>Элементы данной директории. Ключ - путь относительно папки модпака, значение - описание элемента директории.</returns>
        public Dictionary<string, PathLevel> GetPathContent(string path = "/", PathLevel parentUnit = null)
        {
            Dictionary<string, PathLevel> pathContent = new Dictionary<string, PathLevel>();
            string dirPath = FolderPath;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(dirPath + path);

                try
                {
                    foreach (DirectoryInfo item in dir.GetDirectories())
                    {
                        pathContent["/" + item.Name] =
                            new PathLevel(item.Name, false, path + "/" + item.Name, (item.Name == "mods" || item.Name == "scripts" || item.Name == "resources" || item.Name == "resourcepacks" || item.Name == "config"));
                    }
                }
                catch { }

                try
                {
                    foreach (var item in dir.GetFiles())
                    {
                        if (item.Name != "installedAddons.json" && item.Name != "lastUpdates.json" && item.Name != "manifest.json" && item.Name != "instanceContent.json" && item.Name != "instancePlatformData.json")
                            pathContent["/" + item.Name] = new PathLevel(item.Name, true, path + "/" + item.Name);
                    }
                }
                catch { }
            }
            catch { }

            return pathContent;
        }

        /// <summary>
        /// Раздать сборку друзьям
        /// </summary>
        /// <param name="exportList">Аналогично методу Export</param>
        /// <param name="distributor">Экземпляр раздачи</param>
        /// <returns>Результат подготовки к раздаче.</returns>
        public ExportResult Share(Dictionary<string, PathLevel> exportList, out FileDistributor distributor)
        {
            distributor = null;

            var activeAccount = Account.ActiveAccount;
            if (activeAccount == null || !activeAccount.IsAuthed)
            {
                return ExportResult.NotExistsValidAccount;
            }

            string uuid = activeAccount.UUID;
            string sessionToken = activeAccount.SessionToken;

            string shareDir = FileDistributor.SharesDir;
            try
            {
                if (!Directory.Exists(shareDir))
                {
                    Directory.CreateDirectory(shareDir);
                }
            }
            catch
            {
                return ExportResult.ZipFileError;
            }

            var zipFile = shareDir + _localId + ".zip";
            ExportResult result = Export(exportList, zipFile, _name);

            if (result == ExportResult.Successful)
            {
                IsSharing = true;

                distributor = FileDistributor.CreateDistribution(zipFile, Name, uuid, sessionToken);
                if (distributor == null) return ExportResult.ZipFileError;

                distributor.OnClosed += delegate ()
                {
                    IsSharing = false;
                };

                return ExportResult.Successful;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Экспортирует модпак.
        /// </summary>
        /// <param name="exportList">Список файлов и папок на экспорт. Ключ - путь относительно папки модпака, значение - описание элемента директории.</param>
        /// <param name="exportFile">Полноый путь к архиву, в который будет производиться экспорт.</param>
        /// <param name="name">Имя для экспорта.</param>
        /// <returns>Результат экспорта.</returns>
        public ExportResult Export(Dictionary<string, PathLevel> exportList, string exportFile, string name)
        {
            string dirPath = FolderPath;

            void ParsePathLevel(ref List<string> list, Dictionary<string, PathLevel> levelsList)
            {
                foreach (string key in levelsList.Keys)
                {
                    PathLevel elem = levelsList[key];
                    if (elem.IsSelected)
                    {
                        if (elem.IsFile)
                        {
                            list.Add(dirPath + levelsList[key].FullPath);
                        }
                        else
                        {
                            if (elem.UnitsList == null || elem.UnitsList.Count == 0)
                            {
                                string[] files;
                                try
                                {
                                    files = Directory.GetFiles(dirPath + levelsList[key].FullPath, "*", SearchOption.AllDirectories);
                                }
                                catch
                                {
                                    files = new string[0];
                                }

                                foreach (string file in files)
                                {
                                    list.Add(file);
                                }
                            }
                            else
                            {
                                ParsePathLevel(ref list, elem.UnitsList);
                            }
                        }
                    }
                }
            }

            List<string> filesList = new List<string>();
            ParsePathLevel(ref filesList, exportList);

            if (File.Exists(dirPath + "/installedAddons.json"))
            {
                filesList.Add(dirPath + "/installedAddons.json");
            }

            VersionManifest instanceManifest = DataFilesManager.GetManifest(_localId, false);

            string logoPath = (Logo != null ? WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName : null);
            var parameters = new ArchivedClientData
            {
                Author = Author,
                Description = Description,
                GameVersionInfo = instanceManifest?.version?.GameVersionInfo,
                ModloaderType = instanceManifest?.version?.ModloaderType ?? ClientType.Vanilla,
                ModloaderVersion = instanceManifest?.version?.ModloaderVersion,
                Name = name ?? Name,
                //Categories = Categories,
                Summary = Summary,
                LogoFileName = (logoPath != null ? LogoFileName : null),
                AdditionalInstallerType = instanceManifest?.version?.AdditionalInstaller?.type,
                AdditionalInstallerVersion = instanceManifest?.version?.AdditionalInstaller?.installerVersion,

            };

            var res = WithDirectory.ExportInstance<ArchivedClientData>(_localId, filesList, exportFile, parameters, logoPath);
            Runtime.DebugWrite(res);

            return res;
        }

        private static ImportResult Import(in InstanceClient client, string zipFile)
        {
            CancellationTokenSource tes = new CancellationTokenSource();

            var executor = new ImportExecutor(zipFile, true, GlobalData.GeneralSettings, tes.Token, client.ProgressHandler);
            ImportResult res = executor.Prepeare(out PrepeareResult result);

            if (res != ImportResult.Successful) return res;

            byte[] logo = null;
            if (result.LogoPath != null)
            {
                try
                {
                    logo = File.ReadAllBytes(result.LogoPath);
                }
                catch { }
            }

            client.Name = result.Name;
            client.Logo = logo;
            client.GameVersion = result.GameVersionInfo;
            client.Author = result.Author ?? UnknownAuthor;
            client.Description = result.Description ?? NoDescription;
            client.Summary = result.Summary ?? NoDescription;
            client.Logo = logo;
            client.GenerateInstanceId();

            client.CreateFileStruct(ClientType.Vanilla, string.Empty);
            res = executor.Import(client._localId, out IReadOnlyCollection<string> errors);

            client.DownloadComplited?.Invoke(InstanceInit.Successful, (List<string>)errors, false);

            if (res != ImportResult.Successful) return res;

            client.SaveAssets();
            _installedInstances[client._localId] = client;
            SaveInstalledInstancesList();
            client.IsComplete = true;
            return ImportResult.Successful;

            //if (Path.GetExtension(zipFile) == ".mrpack")
            //{
            //    CancellationTokenSource tes = new CancellationTokenSource();
            //    var importManager = new ModrinthImportManager(zipFile, true, GlobalData.GeneralSettings, tes.Token);
            //    importManager.Prepeare((StageType stageType, ProgressHandlerArguments data) => { }, out var result);

            //    client.Name = result.Name;
            //    client.GenerateInstanceId();

            //    importManager.SetInstanceId(client.LocalId);
            //    importManager.Import((StageType stageType, ProgressHandlerArguments data) => { }, out var errors);

            //    return ImportResult.Successful;
            //}

            //ArchivedClientData parameters;
            //string unzipPath;

            //ImportResult res = WithDirectory.UnzipInstance(zipFile, out parameters, out unzipPath);
            //if (res == ImportResult.Successful)
            //{
            //    if (parameters != null && parameters.Name != null && parameters.GameVersion != null)
            //    {
            //        byte[] logo = null;
            //        if (parameters.LogoFileName != null)
            //        {
            //            try
            //            {
            //                string file = unzipPath + parameters.LogoFileName;
            //                if (File.Exists(unzipPath + parameters.LogoFileName))
            //                {
            //                    logo = File.ReadAllBytes(file);
            //                }
            //            }
            //            catch { }
            //        }

            //        client.Name = parameters.Name;
            //        client.GameVersion = parameters.GameVersionInfo;
            //        client.Author = parameters.Author ?? UnknownAuthor;
            //        client.Description = parameters.Description;
            //        client.Summary = parameters.Summary;
            //        client.Logo = logo;
            //        client.GenerateInstanceId();

            //        client.CreateFileStruct(parameters.ModloaderType, parameters.ModloaderVersion, parameters.AdditionalInstallerType, parameters.AdditionalInstallerVersion);
            //        res = WithDirectory.MoveUnpackedInstance(client._localId, unzipPath);

            //        if (res == ImportResult.Successful)
            //        {
            //            client.SaveAssets();
            //            _installedInstances[client._localId] = client;
            //            SaveInstalledInstancesList();
            //            client.IsComplete = true;
            //        }
            //        else
            //        {
            //            WithDirectory.DeleteInstance(client._localId);
            //        }
            //    }
            //    else
            //    {
            //        res = ImportResult.GameVersionError;
            //    }
            //}

            //return res;
        }

        public static InstanceClient Import(string zipFile, Action<ImportResult> callback)
        {
            var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local))
            {
                Name = "Importing...",
                InLibrary = true,
                Author = UnknownAuthor,
                Summary = string.Empty,
                IsComplete = false,
                IsUpdating = true
            };

            Lexplosion.Runtime.TaskRun(delegate ()
            {
                callback(Import(client, zipFile));
                client.IsUpdating = false;
            });

            return client;
        }

        public static InstanceClient Import(FileReceiver reciver, Action<ImportResult> callback)
        {
            return Import(reciver, callback, (_) => { });
        }

        public static InstanceClient Import(FileReceiver reciver, Action<ImportResult> callback, Action<DownloadShareState> stateHandler)
        {
            var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local))
            {
                Name = reciver.Name,
                InLibrary = true,
                Author = UnknownAuthor,
                Summary = string.Empty,
                IsComplete = false,
                IsUpdating = true
            };

            Lexplosion.Runtime.TaskRun(delegate ()
            {
                reciver.StateChanged += () =>
                {
                    var state = reciver.State;
                    DownloadShareState resState = DownloadShareState.InQueue;
                    switch (state)
                    {
                        case FileReceiver.DistributionState.InQueue:
                            resState = DownloadShareState.InQueue;
                            break;
                        case FileReceiver.DistributionState.InConnect:
                            resState = DownloadShareState.InConnect;
                            break;
                        case FileReceiver.DistributionState.InProcess:
                            resState = DownloadShareState.PostProcessing;
                            break;
                    }

                    stateHandler(resState);
                };

                FileRecvResult result = WithDirectory.ReceiveFile(reciver, out string file);
                if (result == FileRecvResult.Successful)
                {
                    stateHandler(DownloadShareState.PostProcessing);
                    callback(Import(in client, file));
                }
                else
                {
                    callback(result == FileRecvResult.Canceled ? ImportResult.Canceled : ImportResult.DownloadError);
                }

                client.IsUpdating = false;
            });

            return client;
        }

        public static InstanceClient Import(Uri fileURL, Action<ImportResult> callback)
        {
            var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local))
            {
                Name = "Importing...",
                InLibrary = true,
                Author = UnknownAuthor,
                Summary = string.Empty,
                IsComplete = false,
                IsUpdating = true
            };

            Lexplosion.Runtime.TaskRun(delegate ()
            {
                string downloadUrl = null;
                try
                {
                    if (fileURL.Host == "drive.google.com")
                    {
                        string[] parts = fileURL.PathAndQuery.Split('/');
                        if (parts.Length < 4 || string.IsNullOrWhiteSpace(parts[3]))
                        {
                            callback(ImportResult.WrongUrl);
                            return;
                        }

                        downloadUrl = "https://drive.google.com/uc?export=download&confirm=no_antivirus&id=" + parts[3];

                        if (ToServer.IsHtmlPage(downloadUrl))
                        {
                            string data = ToServer.HttpGet(downloadUrl);
                            if (string.IsNullOrWhiteSpace(data))
                            {
                                callback(ImportResult.WrongUrl);
                                return;
                            }

                            IEnumerable<string> GetSubStrings(string input, string start, string end)
                            {
                                Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
                                MatchCollection matches = r.Matches(input);
                                foreach (Match match in matches)
                                    yield return match.Groups[1].Value;
                            }

                            string GetStrBetweenStrings(string input, string start, string end)
                            {
                                return Regex.Match(input, Regex.Escape(start) + "(.*?)" + Regex.Escape(end)).Groups[1].Value;
                            }

                            string urlBase = null;
                            string formHead = null;
                            foreach (string pageForm in GetSubStrings(data, "<form", ">"))
                            {
                                if (pageForm.Contains("id=\"download-form\"") && pageForm.Contains("action=\""))
                                {
                                    urlBase = GetStrBetweenStrings(pageForm, "action=\"", "\"");
                                    formHead = "<form" + pageForm + ">";
                                    break;
                                }
                            }

                            if (urlBase == null)
                            {
                                callback(ImportResult.WrongUrl);
                                return;
                            }

                            urlBase += "?";
                            data = GetStrBetweenStrings(data, formHead, "</form>");
                            foreach (string htmlInput in GetSubStrings(data, "<input", ">"))
                            {
                                if (htmlInput.Contains("name=\"") && htmlInput.Contains("value=\""))
                                {
                                    string name = GetStrBetweenStrings(htmlInput, "name=\"", "\"");
                                    string value = GetStrBetweenStrings(htmlInput, "value=\"", "\"");
                                    urlBase += Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value) + "&";
                                }
                            }

                            downloadUrl = urlBase;
                        }
                    }
                    else if (fileURL.Host == "yadi.sk" || fileURL.Host == "disk.yandex.ru" || fileURL.Host == "disk.yandex.com" || fileURL.Host == "disk.yandex.by")
                    {
                        string queryUrl = "https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=" + Uri.EscapeDataString(fileURL.ToString());

                        string result = ToServer.HttpGet(queryUrl);
                        if (result == null)
                        {
                            callback(ImportResult.WrongUrl);
                            return;
                        }

                        var data = JsonConvert.DeserializeObject<JToken>(result);
                        downloadUrl = data["href"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                    callback(ImportResult.WrongUrl);
                    return;
                }

                string tempDir = WithDirectory.CreateTempDir();
                bool res = WithDirectory.DownloadFile(downloadUrl, "instance_file", tempDir, new TaskArgs
                {
                    CancelToken = (new CancellationTokenSource()).Token,
                    PercentHandler = (int pr) => { }
                });

                if (!res)
                {
                    callback(ImportResult.DownloadError);
                    return;
                }

                ImportResult impurtRes = Import(client, tempDir + "instance_file");

                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Exception " + ex);
                }

                callback(impurtRes);
                client.IsUpdating = false;
            });

            return client;
        }

        public void AddGameServer(MinecraftServerInstance server, bool autoLogin)
        {
            ServersDatManager serversDat = new ServersDatManager(FolderPath + "/servers.dat");

            if (!serversDat.ContsainsServer(server.Name, server.Address))
            {
                serversDat.AddServer(new ServersDatManager.ServerData(server.Name, server.Address));
                serversDat.SaveFile();
            }

            Settings settings = GetSettings();
            settings.AutoLoginServer = autoLogin ? server.Address : null;

            SaveSettings(settings);
        }

        public void AddToGroup(InstanceGroup group)
        {

        }

        public void RemoveFromGroup(InstanceGroup group)
        {

        }
    }
}