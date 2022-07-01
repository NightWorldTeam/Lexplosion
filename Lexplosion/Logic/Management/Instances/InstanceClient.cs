using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;

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
                _logo = value;
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
        public bool InLibrary { get; private set; } = false;

        private bool _updateAvailable = false;
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;
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

        /// <summary>
        /// Этот конструктор создаёт еще не установленную сборку. Используется для сборок из каталога
        /// </summary>
        /// <param name="type">Тип модпака</param>
        /// <param name="externalID">Внешний ID</param>
        private InstanceClient(InstanceSource type, string externalID)
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
        private InstanceClient(string name, InstanceSource type, string gameVersion)
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
        /// <param name="modloaderVersion">Версия модлоадера. Это поле необходимо только если есть модлоадер</param>
        public static InstanceClient CreateClient(string name, InstanceSource type, string gameVersion, ModloaderType modloader, string modloaderVersion = null)
        {
            var client = new InstanceClient(name, type, gameVersion)
            {
                InLibrary = true,
                Author = UserData.Login,
                Description = NoDescription
            };

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
                    ModloaderVersion = manifest?.version?.gameVersion ?? "",
                    Modloader = manifest?.version.modloaderType ?? ModloaderType.None
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
                        byte[] logo = null;

                        //получаем вншний айдшник, если этот модпак не локлаьный
                        if (list[localId].Type != InstanceSource.Local)
                        {
                            InstancePlatformData data = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + localId + "/instancePlatformData.json");
                            if (data != null)
                            {
                                externalID = data.id;
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
                                Logo = logo
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
                                Logo = logo
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
        public static List<InstanceClient> GetOutsideInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            Console.WriteLine("UploadInstances " + pageIndex);

            var instances = new List<InstanceClient>();
            var events = new List<AutoResetEvent>();

            if (type == InstanceSource.Nightworld)
            {
                Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = NightWorldApi.GetInstancesList();

                var i = 0;
                foreach (string nwModpack in nwInstances.Keys)
                {
                    if (i < pageSize * (pageIndex + 1))
                    {
                        // проверяем версию игры
                        if (nwInstances[nwModpack].GameVersion != null)
                        {
                            InstanceClient instanceClient;
                            if (_idsPairs.ContainsKey(nwModpack))
                            {
                                instanceClient = _installedInstances[_idsPairs[nwModpack]];
                                instanceClient.CheckUpdates();
                                instanceClient.DownloadLogo(nwInstances[nwModpack].MainImage, delegate 
                                {
                                    instanceClient.SaveAssets();
                                });

                                if (nwInstances[nwModpack].Categories != null)
                                    instanceClient.Categories = nwInstances[nwModpack].Categories;
                                if (nwInstances[nwModpack].Summary != null)
                                    instanceClient.Summary = nwInstances[nwModpack].Summary;
                                if (nwInstances[nwModpack].Description != null)
                                    instanceClient.Description = nwInstances[nwModpack].Description;
                                if (nwInstances[nwModpack].Author != null)
                                    instanceClient.Author = nwInstances[nwModpack].Author;
                            }
                            else
                            {
                                instanceClient = new InstanceClient(InstanceSource.Nightworld, nwModpack)
                                {
                                    Name = nwInstances[nwModpack].Name ?? UnknownName,
                                    Logo = null,
                                    Categories = nwInstances[nwModpack].Categories ?? new List<Category>(),
                                    GameVersion = nwInstances[nwModpack].GameVersion ?? "",
                                    Summary = nwInstances[nwModpack].Summary ?? "",
                                    Description = nwInstances[nwModpack].Description ?? "",
                                    Author = nwInstances[nwModpack].Author ?? UnknownAuthor
                                };

                                instanceClient.DownloadLogo(nwInstances[nwModpack].MainImage, delegate { });
                            }

                            instanceClient.WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + nwModpack;

                            instances.Add(instanceClient);
                        }
                    }

                    i++;
                }
            }
            else if (type == InstanceSource.Curseforge)
            {
                List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, ModpacksCategories.All, searchFilter);
                foreach (var instance in curseforgeInstances)
                {
                    // проверяем версию игры
                    if (instance.latestFilesIndexes != null && instance.latestFilesIndexes.Count > 0 && instance.latestFilesIndexes[0].gameVersion != null)
                    {
                        string author = "";
                        if (instance.authors != null && instance.authors.Count > 0 && instance.authors[0].name != null)
                        {
                            author = instance.authors[0].name;
                        }

                        InstanceClient instanceClient;
                        if (_idsPairs.ContainsKey(instance.id.ToString()))
                        {
                            instanceClient = _installedInstances[_idsPairs[instance.id.ToString()]];
                            instanceClient.CheckUpdates();

                            if (instance.categories != null)
                                instanceClient.Categories = instance.categories;
                            if (instance.summary != null)
                            {
                                instanceClient.Summary = instance.summary;
                                instanceClient.Description = instance.summary;
                            }
                            if (instance.authors != null && instance.authors.Count > 0)
                                instanceClient.Author = instance.authors[0].name;
                        }
                        else
                        {
                            instanceClient = new InstanceClient(InstanceSource.Curseforge, instance.id.ToString())
                            {
                                Name = instance.name ?? UnknownName,
                                Logo = null,
                                Categories = instance.categories,
                                GameVersion = instance.latestFilesIndexes[0].gameVersion,
                                Summary = instance.summary ?? "",
                                Description = instance.summary ?? "",
                                Author = instance.GetAuthorName
                            };
                        }

                        instanceClient.WebsiteUrl = instance.links.websiteUrl;

                        if (instance.logo != null && instance.logo.url != null)
                        {
                            instanceClient.DownloadLogo(instance.logo.url, delegate 
                            {
                                if (_idsPairs.ContainsKey(instance.id.ToString()))
                                    instanceClient.SaveAssets();
                            });
                            
                        }

                        instances.Add(instanceClient);
                    }      
                }
            }

            foreach (var e in events)
            {
                e.WaitOne();
            }

            Console.WriteLine("UploadInstances End " + pageIndex);

            return instances;
        }

        /// <summary>
        /// Получает всю информацию о модпаке.
        /// </summary>
        /// <returns>InstanceData, содержащий в себе данные которые могут потребоваться</returns>
        public InstanceData GetFullInfo()
        {
            switch (Type)
            {
                case InstanceSource.Curseforge:
                    {
                        var data = CurseforgeApi.GetInstance(_externalId);
                        var images = new List<byte[]>();
                        if (data.screenshots != null)
                        {
                            using (var webClient = new WebClient())
                            {
                                foreach (var item in data.screenshots)
                                {
                                    try
                                    {
                                        images.Add(webClient.DownloadData(item.url));
                                    }
                                    catch { }
                                }
                            }
                        }

                        var projectFileId = data.latestFilesIndexes?[0]?.fileId;

                        return new InstanceData
                        {
                            Categories = data.categories,
                            Description = data.summary,
                            Summary = data.summary,
                            TotalDownloads = (long)data.downloadCount,
                            GameVersion = (data.latestFilesIndexes != null && data.latestFilesIndexes.Count > 0) ? data.latestFilesIndexes[0].gameVersion : "",
                            LastUpdate = (data.dateModified != null) ? DateTime.Parse(data.dateModified).ToString("dd MMM yyyy") : "",
                            Modloader = data.ModloaderType,
                            Images = images,
                            WebsiteUrl = data.links.websiteUrl,
                            Changelog = (projectFileId != null) ? (CurseforgeApi.GetProjectChangelog(_externalId, projectFileId.ToString()) ?? "") : ""
                        };
                    }
                case InstanceSource.Nightworld:
                    {
                        var data = NightWorldApi.GetInstanceInfo(_externalId);
                        var images = new List<byte[]>();

                        if (data.Images != null)
                        {
                            using (var webClient = new WebClient())
                            {
                                foreach (var item in data.Images)
                                {
                                    try
                                    {
                                        images.Add(webClient.DownloadData(item));
                                    }
                                    catch { }
                                }
                            }
                        }

                        return new InstanceData
                        {
                            Categories = data.Categories,
                            Description = data.Description,
                            Summary = data.Summary,
                            TotalDownloads = data.DownloadCounts,
                            GameVersion = data.GameVersion,
                            LastUpdate = (new DateTime(1970, 1, 1).AddSeconds(data.LastUpdate)).ToString("dd MMM yyyy"),
                            Modloader = data.Modloader,
                            Images = images,
                            WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + _externalId,
                            Changelog = ""
                        };
                    }
                default:
                    {
                        VersionManifest instanceManifest = DataFilesManager.GetManifest(_localId, false);
                        InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/assets.json");
                        return new InstanceData
                        {
                            Categories = new List<Category>(),
                            Description = assetsData?.Description ?? NoDescription,
                            Summary = assetsData?.Summary ?? NoDescription,
                            TotalDownloads = 0,
                            GameVersion = GameVersion,
                            LastUpdate = null,
                            Modloader = instanceManifest?.version?.modloaderType ?? ModloaderType.None,
                            Images = WithDirectory.LoadMcScreenshots(_localId)
                        };
                    }
            }
        }

        /// <summary>
        /// Изменяет параметры клиента. Клиент должен быть добавлен в библиотеку.
        /// Если параметр не нужно менять, то в него передавать null.
        /// Если нужно сменить тип модлоадера, то обязательно ещё нужно передать его версию.
        /// </summary>
        /// <param name="name">Им клиента.</param>
        /// <param name="desc">Описание.</param>
        /// <param name="gameVersion">Версия игры.</param>
        /// <param name="summary">Краткое описание.</param>
        /// <param name="categories">Категории.</param>
        /// <param name="modloader">Тип модлоадера. Если его нужно изменить, то обязательно нужно передать и modloaderVersion.</param>
        /// <param name="modloaderVersion">Версия модлоадера.</param>s
        public void ChangeParameters(BaseInstanceData data)
        {
            VersionManifest manifest = DataFilesManager.GetManifest(_localId, false);
            if (manifest != null)
            {
                manifest.version.modloaderType = data.Modloader;
                manifest.version.modloaderVersion = data.ModloaderVersion;
                manifest.version.gameVersion = data.GameVersion;
                DataFilesManager.SaveManifest(_localId, manifest);
            }

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
        public void UpdateInstance()
        {
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, 1, 0, 0);

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Update(ProgressHandler);

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = (data.InitResult == InstanceInit.Successful);
                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
            }

            ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, false);
        }

        /// <summary>
        /// Запускает сборку. Если надо её докачивает. Сборка должна быть доавлена в библиотеку
        /// </summary>
        public void Run()
        {
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, 1, 0, 0);

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Initialization(ProgressHandler);

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = true;
                SaveInstalledInstancesList(); // чтобы если сборка установилась то флаг IsInstalled сохранился
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, true);

                launchGame.Run(data, ComplitedLaunch, GameExited, Name, true);
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
            else
            {
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, false);
            }
        }

        /// <summary>
        /// Добавляет сборку в библиотеку
        /// </summary>
        public void AddToLibrary()
        {
            if (!InLibrary)
            {
                _localId = GenerateInstanceId();
                CreateFileStruct(ModloaderType.None, "");
                _installedInstances[_localId] = this;
                _idsPairs[_externalId] = _localId;
                SaveInstalledInstancesList();
                SaveAssets();
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

            switch (Type)
            {
                case InstanceSource.Curseforge:
                    if (!Int32.TryParse(infoData.id, out _))
                    {
                        UpdateAvailable = true;
                        return;
                    }

                    List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetProjectFiles(infoData.id); //получем информацию об этом модпаке

                    //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии 
                    foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
                    {
                        if (ver.id > infoData.instanceVersion)
                        {
                            UpdateAvailable = true;
                            return;
                        }
                    }
                    break;
            }

            UpdateAvailable = false;
            return;
        }

        private string GenerateInstanceId()
        {
            string instanceId = Name;
            instanceId = instanceId.Replace(" ", "_");

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
        public Dictionary<string, PathLevel> GetPathContent(string path = "/")
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
                        pathContent["/" + item.Name] = new PathLevel
                        {
                            IsFile = false
                        };
                    }
                }
                catch { }

                try
                {
                    foreach (var item in dir.GetFiles())
                    {
                        pathContent["/" + item.Name] = new PathLevel
                        {
                            IsFile = true
                        };
                    }
                }
                catch { }
            }
            catch { }

            return pathContent;
        }

        /// <summary>
        /// Экспортирует модпак.
        /// </summary>
        /// <param name="exportList">Список файлов и папок на экспорт. Ключ - путь относительно папки модпака, значение - описание элемента директории.</param>
        /// <param name="exportFile">Полноый путь к архиву, в который будет производиться экспорт.</param>
        /// <returns>Результат экспорта.</returns>
        public ExportResult Export(Dictionary<string, PathLevel> exportList, string exportFile)
        {
            string dirPath = WithDirectory.DirectoryPath + "/instances/" + _localId;

            void ParsePathLevel(ref List<string> list, Dictionary<string, PathLevel> levelsList)
            {
                foreach (string key in exportList.Keys)
                {
                    PathLevel elem = exportList[key];
                    if (elem.IsFile)
                    {
                        list.Add(dirPath + key);
                    }
                    else
                    {
                        if (elem.AllUnits)
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

            List<string> filesList = new List<string>();
            ParsePathLevel(ref filesList, exportList);

            VersionManifest instanceManifest = DataFilesManager.GetManifest(_localId, false);

            string logoPath = (Logo != null ? WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName : null);
            var parameters = new ArchivedClientData
            {
                Author = Author,
                Description = Description,
                GameVersion = instanceManifest?.version?.gameVersion,
                ModloaderType = instanceManifest?.version?.modloaderType ?? ModloaderType.None,
                ModloaderVersion = instanceManifest?.version?.modloaderVersion,
                Name = Name,
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
        public static ImportResult Import(string zipFile)
        {
            ArchivedClientData parameters;
            string unzipPath;

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
                        Logo = logo,
                        Categories = parameters.Categories
                    };

                    client.CreateFileStruct(parameters.ModloaderType, parameters.ModloaderVersion);
                    client.SaveAssets();

                    _installedInstances[client._localId] = client;
                    client.SaveInstalledInstancesList();

                    res = WithDirectory.MoveUnpackedInstance(client._localId, unzipPath);
                }       
            }

            return res;
        }
    }
}