using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Instances
{
    // Структура файла с установленными модпаками (instanesList.json)
    using InstalledInstancesFormat = Dictionary<string, InstalledInstance>;

    public class InstanceClient : VMBase
    {
        private class ArchivedClientData
        {
            public string GameVersion;
            public string Name;
            public string Description;
            public string Author;
            public ModloaderType ModloaderType;
            public string ModloaderVersion;
            public List<Category> Categories;
            public string Summary;
            public string LogoFileName;
        }

        public readonly InstanceSource Type;
        private string _externalId = null;
        private string _localId = null;
        private readonly PrototypeInstance _dataManager;

        private const string LogoFileName = "logo.png";
        private const string UnknownName = "Unknown name";
        private const string UnknownAuthor = "Unknown author";
        private const string NoDescription = "Описания нет, но мы надеемся что оно будет.";

        private static Dictionary<string, InstanceClient> _installedInstances = new Dictionary<string, InstanceClient>();

        /// <summary>
        /// Содержит пары состоящие из внешнего и внутреннего id.
        /// </summary>
        private static Dictionary<string, string> _idsPairs = new Dictionary<string, string>();

        #region info

        public string LocalId
        {
            get
            {
                return _localId;
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
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
                if (value != null)
                {
                    _logo = ImageTools.ResizeImage(value, 120, 120);
                }
                else
                {
                    _logo = null;
                }

                OnPropertyChanged();
            }
        }

        private List<Category> _categories = null;
        public List<Category> Categories
        {
            get => _categories;
            private set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        private string _gameVersion;
        public string GameVersion
        {
            get => _gameVersion;
            private set
            {
                _gameVersion = value;
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
            }
        }

        public string Author { get; private set; }

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
        public string ProfileVersion
        {
            get => _profileVersion;
            private set
            {
                _profileVersion = value;
                OnPropertyChanged();
            }
        }

        public bool IsInstalled { get; private set; } = false;
        public string WebsiteUrl { get; private set; } = null;

        #endregion

        public event ProgressHandlerCallback ProgressHandler;
        public event ComplitedDownloadCallback ComplitedDownload;
        public event ComplitedLaunchCallback ComplitedLaunch;
        public event GameExitedCallback GameExited;
        public event Action StateChanged;
        public event Action<string, int, DownloadFileProgress> FileDownloadEvent;

        /// <summary>
        /// Базовый конструктор, от него должны наследоваться все остальные
        /// </summary>
        /// <param name="type">Тип модпака</param>
        private InstanceClient(InstanceSource type)
        {
            switch (type)
            {
                case InstanceSource.Nightworld:
                    _dataManager = new NightworldInstance();
                    break;
                case InstanceSource.Curseforge:
                    _dataManager = new CurseforgeInstance();
                    break;
                default:
                    _dataManager = new LocalInstance();
                    break;
            }
        }

        /// <summary>
        /// Этот конструктор создаёт еще не установленную сборку. Используется для сборок из каталога
        /// </summary>
        /// <param name="type">Тип модпака</param>
        /// <param name="externalID">Внешний ID</param>
        private InstanceClient(InstanceSource type, string externalID) : this(type)
        {
            Type = type;
            _externalId = externalID;
        }

        /// <summary>
        /// Этот конструктор создаёт установленную сборку. Используется для сборок в библиотеке.
        /// </summary>
        /// <param name="type">Тип модпака</param>
        /// <param name="externalID">Внешний ID</param>
        /// <param name="externalID">Локальный ID</param>
        private InstanceClient(InstanceSource type, string externalID, string localId) : this(type, externalID)
        {
            _localId = localId;
        }

        /// <summary>
        /// Этот конструктор создаёт локальную сборку. Должен использоваться только при создании локальной сборки и при импорте.
        /// </summary>
        /// <param name="name">Название сборки</param>
        /// <param name="gameVersion">Версия игры</param>
        private InstanceClient(string name, InstanceSource type, string gameVersion) : this(type)
        {
            Name = name;
            Type = type;
            GameVersion = gameVersion;
            _localId = GenerateInstanceId();
        }

        /// <summary>
        /// Этот метод создаёт локальную сборку. Должен использоваться только при создании локальной сборки.
        /// </summary>
        /// <param name="name">Название сборки</param>
        /// <param name="gameVersion">Версия игры</param>
        /// <param name="modloader">Тип модлоадера</param>
        /// <param name="logoPath">Путь до логотипа. Если устанавливать не надо, то null.</param>
        /// <param name="modloaderVersion">Версия модлоадера. Это поле необходимо только если есть модлоадер</param>
        public static InstanceClient CreateClient(string name, InstanceSource type, string gameVersion, ModloaderType modloader, string logoPath, string modloaderVersion = null)
        {
            var client = new InstanceClient(name, type, gameVersion)
            {
                InLibrary = true,
                Author = UserData.User.Login,
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

            client.CreateFileStruct(modloader, modloaderVersion);
            client.SaveAssets();

            _installedInstances[client._localId] = client;
            client.SaveInstalledInstancesList();

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
                    GameVersion = GameVersion,
                    InLibrary = InLibrary,
                    Author = Author,
                    Categories = Categories,
                    Description = Description,
                    Name = _name,
                    Summary = _summary,
                    ModloaderVersion = manifest?.version?.modloaderVersion ?? "",
                    Modloader = manifest?.version.modloaderType ?? ModloaderType.Vanilla
                };
            }
        }

        /// <summary>
        /// Сохраняем список установленных сборок (библиотеку) в файл instanesList.json.
        /// </summary>
        private void SaveInstalledInstancesList()
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
                        (instanceManifest != null && instanceManifest.version != null && instanceManifest.version.gameVersion != null);

                    //проверяем имеется ли манифест, не содержит ли его id запрещенных символов
                    if (manifestIsCorrect && !Regex.IsMatch(localId.Replace("_", ""), @"[^a-zA-Z0-9]"))
                    {
                        string externalID = null;
                        string instanceVersion = null;
                        byte[] logo = null;

                        //получаем вншний айдшник и версию, если этот модпак не локлаьный
                        if (list[localId].Type != InstanceSource.Local)
                        {
                            InstancePlatformData data = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + localId + "/instancePlatformData.json");
                            if (data != null)
                            {
                                externalID = data.id;
                                instanceVersion = data.instanceVersion.ToString();
                                _idsPairs[externalID] = localId;
                            }
                        }

                        //получаем асетсы модпаков
                        InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/assets.json");

                        if (assetsData != null)
                        {
                            string file = WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/" + LogoFileName;
                            if (File.Exists(file))
                            {
                                //try
                                {
                                    logo = File.ReadAllBytes(file);
                                }
                                //catch { }
                            }
                        }

                        InstanceClient instance;
                        if (assetsData != null)
                        {
                            instance = new InstanceClient(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? UnknownName,
                                Summary = assetsData.Summary ?? NoDescription,
                                Author = assetsData.Author ?? UnknownAuthor,
                                Description = assetsData.Description ?? NoDescription,
                                Categories = assetsData.Categories ?? new List<Category>(),
                                GameVersion = instanceManifest.version?.gameVersion,
                                Logo = logo,
                                _profileVersion = instanceVersion
                            };
                        }
                        else
                        {
                            instance = new InstanceClient(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? UnknownName,
                                Summary = NoDescription,
                                Author = UnknownAuthor,
                                Description = NoDescription,
                                Categories = new List<Category>(),
                                GameVersion = instanceManifest.version?.gameVersion,
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
        public static List<InstanceClient> GetInstalledInstances()
        {
            var list = new List<InstanceClient>();
            foreach (InstanceClient instance in _installedInstances.Values)
            {
                list.Add(instance);
            }

            return list;
        }

        /// <summary>
        /// Возвращает список модпаков для каталога.
        /// </summary>
        /// <returns>Список внешних модпаков.</returns>
        public static List<InstanceClient> GetOutsideInstances(InstanceSource type, int pageSize, int pageIndex, int categoriy, string searchFilter = "")
        {
            Console.WriteLine("UploadInstances " + pageIndex);

            var instances = new List<InstanceClient>();
            List<PrototypeInstance.Info> catalog = PrototypeInstance.GetCatalog(type, pageSize, pageIndex, categoriy, searchFilter);

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
                    if (instance.Author != null)
                        instanceClient.WebsiteUrl = instance.WebsiteUrl;

                    instanceClient.DownloadLogo(instance.LogoUrl, instanceClient.SaveAssets);
                }
                else
                {
                    instanceClient = new InstanceClient(type, instance.ExternalId)
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
            Console.WriteLine("UploadInstances End " + pageIndex);

            return instances;
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
                    instanceClient = new InstanceClient(type, instance.ExternalId)
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
                manifest.version.modloaderType = data.Modloader;
                manifest.version.modloaderVersion = data.ModloaderVersion;
                manifest.version.gameVersion = data.GameVersion;
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
        /// Обновляет или скачивает сборку. Сборка должна быть добавлена в библиотеку.
        /// </summary>
        public void UpdateInstance(string instanceVersion = null)
        {
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, new ProgressHandlerArguments());

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            instanceSettings.Merge(UserData.GeneralSettings, true);

            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Update(ProgressHandler, FileDownloadEvent, instanceVersion);

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = (data.InitResult == InstanceInit.Successful);
                UpdateAvailable = false;
                if (instanceVersion != null)
                {
                    ProfileVersion = instanceVersion;
                }

                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
            }

            ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, false);
            Console.WriteLine("UpdateInstance-end " + data.InitResult);
        }

        /// <summary>
        /// Запускает сборку. Если надо её докачивает. Сборка должна быть доавлена в библиотеку
        /// </summary>
        public void Run()
        {
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, new ProgressHandlerArguments());

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            instanceSettings.Merge(UserData.GeneralSettings, true);

            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Initialization(ProgressHandler, FileDownloadEvent);

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = true;
                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, true);

                launchGame.Run(data, ComplitedLaunch, GameExited, Name, UserData.User.AccountType == AccountType.NightWorld);
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
                // TODO: тут надо как-то определять что сборка обновилась и UpdateAvailable = false делать, если было обновление
            }
            else
            {
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, false);
            }

            Console.WriteLine("Run-end " + data.InitResult);
        }

        /// <summary>
        /// Добавляет сборку в библиотеку
        /// </summary>
        public void AddToLibrary()
        {
            if (!InLibrary)
            {
                _localId = GenerateInstanceId();
                CreateFileStruct(ModloaderType.Vanilla, "");
                _installedInstances[_localId] = this;
                _idsPairs[_externalId] = _localId;
                SaveInstalledInstancesList();
                SaveAssets();
                InLibrary = true;
            }
        }

        private void CheckUpdates()
        {
            var infoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + _localId + "/instancePlatformData.json");
            if (infoData == null || infoData.id == null)
            {
                UpdateAvailable = false;
                return;
            }

            UpdateAvailable = _dataManager.CheckUpdates(infoData);
            return;
        }

        private string GenerateInstanceId()
        {
            string instanceId = Name;
            instanceId = instanceId.Replace(" ", "_");

            // переводим русские символы в транслит
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                instanceId = instanceId.Replace(rus_up[i], lat_up[i]);
                instanceId = instanceId.Replace(rus_low[i], lat_low[i]);
            }

            if (Regex.IsMatch(instanceId.Replace("_", ""), @"[^a-zA-Z0-9]"))
            {
                int j = 0;
                while (j < instanceId.Length)
                {
                    if (Regex.IsMatch(instanceId[j].ToString(), @"[^a-zA-Z0-9]") && instanceId[j] != '_')
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
                            instanceId_ = instanceId + "__" + i;
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

            return instanceId;
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
            //try
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
            //catch { }

            string file = WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/assets.json";
            InstanceAssets assetsData_ = DataFilesManager.GetFile<InstanceAssets>(file);

            var assetsData = new InstanceAssets
            {
                Author = Author,
                Categories = Categories,
                Description = Description,
                Images = (assetsData_ != null) ? assetsData_.Images : null,
                Summary = Summary
            };

            DataFilesManager.SaveFile(file, JsonConvert.SerializeObject(assetsData));
        }

        /// <summary>
        /// Создает необходимую структуру файлов для сборки при её добавлении в библиотеку (ну при создании локальной).
        /// </summary>
        private void CreateFileStruct(ModloaderType modloader, string modloaderVersion)
        {
            Directory.CreateDirectory(WithDirectory.DirectoryPath + "/instances/" + _localId);

            VersionManifest manifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    gameVersion = GameVersion,
                    modloaderVersion = modloaderVersion,
                    modloaderType = modloader
                }
            };
            DataFilesManager.SaveManifest(_localId, manifest);

            if (Type != InstanceSource.Local)
            {
                var instanceData = new InstancePlatformData
                {
                    id = _externalId
                };

                DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + _localId + "/instancePlatformData.json", JsonConvert.SerializeObject(instanceData));
            }
        }

        public string GetDirectoryPath()
        {
            return @"" + WithDirectory.DirectoryPath.Replace("/", @"\") + @"\instances\" + _localId;
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
            string dirPath = WithDirectory.DirectoryPath + "/instances/" + _localId;

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
                        if (path == "/" && item.Name != "installedAddons.json" && item.Name != "lastUpdates.json" && item.Name != "manifest.json" && item.Name != "instanceContent.json" && item.Name != "instancePlatformData.json")
                            pathContent["/" + item.Name] = new PathLevel(item.Name, true, path + "/" + item.Name);
                    }
                }
                catch { }
            }
            catch { }

            return pathContent;
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
            InLibrary = false;
            SaveInstalledInstancesList();
        }

        /// <summary>
        /// Экспортирует модпак.
        /// </summary>
        /// <param name="exportList">Список файлов и папок на экспорт. Ключ - путь относительно папки модпака, значение - описание элемента директории.</param>
        /// <param name="exportFile">Полноый путь к архиву, в который будет производиться экспорт.</param>
        /// <returns>Результат экспорта.</returns>
        public ExportResult Export(Dictionary<string, PathLevel> exportList, string exportFile, string name)
        {
            string dirPath = WithDirectory.DirectoryPath + "/instances/" + _localId;

            void ParsePathLevel(ref List<string> list, Dictionary<string, PathLevel> levelsList)
            {
                foreach (string key in exportList.Keys)
                {
                    PathLevel elem = exportList[key];
                    if (elem.IsSelected)
                    {
                        if (elem.IsFile)
                        {
                            list.Add(dirPath + key);
                        }
                        else
                        {
                            if (elem.UnitsList == null)
                            {
                                string[] files;
                                try
                                {
                                    files = Directory.GetFiles(dirPath + key, "*", SearchOption.AllDirectories);
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
                GameVersion = instanceManifest?.version?.gameVersion,
                ModloaderType = instanceManifest?.version?.modloaderType ?? ModloaderType.Vanilla,
                ModloaderVersion = instanceManifest?.version?.modloaderVersion,
                Name = name ?? Name,
                Categories = Categories,
                Summary = Summary,
                LogoFileName = (logoPath != null ? LogoFileName : null)
            };

            return WithDirectory.ExportInstance<ArchivedClientData>(_localId, filesList, exportFile, parameters, logoPath);
        }

        /// <summary>
        /// Импортирует модпак из архива.
        /// </summary>
        /// <param name="zipFile">Путь до zip файла, содержащего клиент.</param>
        /// <returns>Результат импорта.</returns>
        public static ImportResult Import(string zipFile, out InstanceClient instanceClient)
        {
            ArchivedClientData parameters;
            string unzipPath;
            instanceClient = null;

            ImportResult res = WithDirectory.UnzipInstance(zipFile, out parameters, out unzipPath);
            if (res == ImportResult.Successful)
            {
                if (parameters != null && parameters.Name != null && parameters.GameVersion != null)
                {
                    byte[] logo = null;
                    if (parameters.LogoFileName != null)
                    {
                        try
                        {
                            string file = unzipPath + parameters.LogoFileName;
                            if (File.Exists(unzipPath + parameters.LogoFileName))
                            {
                                logo = File.ReadAllBytes(file);
                            }
                        }
                        catch { }
                    }

                    var client = new InstanceClient(parameters.Name, InstanceSource.Local, parameters.GameVersion)
                    {
                        InLibrary = true,
                        Author = parameters.Author ?? UnknownAuthor,
                        Description = parameters.Description,
                        Summary = parameters.Summary,
                        Logo = logo,
                        Categories = parameters.Categories
                    };

                    client.CreateFileStruct(parameters.ModloaderType, parameters.ModloaderVersion);
                    res = WithDirectory.MoveUnpackedInstance(client._localId, unzipPath);

                    if (res == ImportResult.Successful)
                    {
                        client.SaveAssets();
                        _installedInstances[client._localId] = client;
                        client.SaveInstalledInstancesList();
                        instanceClient = client;
                    }
                    else
                    {
                        WithDirectory.DeleteInstance(client._localId);
                    }
                }       
            }

            return res;
        }
    }
}