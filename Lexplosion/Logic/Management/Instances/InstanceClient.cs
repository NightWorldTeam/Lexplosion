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

    public class InstanceClient
    {
        public readonly InstanceSource Type;
        private string _externalId = null;
        private string _localId = null;

        private static Dictionary<string, InstanceClient> _installedInstances = new Dictionary<string, InstanceClient>();
        // ключ - вншений id, значение - внутренний id
        private static Dictionary<string, string> _externaLocalIdsPairs = new Dictionary<string, string>();

        //TODO: добавить настройки!

        #region info
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public byte[] Logo { get; private set; }
        public List<Category> Categories { get; private set; }
        public string GameVersion { get; private set; }
        public string Summary { get; private set; }
        public bool InLibrary { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public bool IsInstalled { get; private set; } = false;
        #endregion

        public event ProgressHandlerCallback ProgressHandler;
        public event ComplitedDownloadCallback ComplitedDownload;
        public event ComplitedLaunchCallback ComplitedLaunch;
        public event GameExitedCallback GameExited;

        /// <summary>
        /// Этот конструктор создаёт еще не установленную сборку. То есть используется для сборок из каталога
        /// </summary>
        /// <param name="type">Собста тип модпака</param>
        /// <param name="externalID">Его внешний айдишник</param>
        private InstanceClient(InstanceSource type, string externalID)
        {
            Type = type;
            _externalId = externalID;
        }

        /// <summary>
        /// Этот конструктор создаёт установленную сборку. То есть используется для сборок в библиотеке
        /// </summary>
        /// <param name="type">Собста тип модпака</param>
        /// <param name="externalID">Его внешний айдишник</param>
        /// /// <param name="externalID">Локальный айдишник</param>
        private InstanceClient(InstanceSource type, string externalID, string localId) : this(type, externalID)
        {
            _localId = localId;
        }

        /// <summary>
        /// Этот конструктор создаёт локальную сборку. Должен использоваться соотвественно только при создании локлаьной сборки
        /// </summary>
        /// <param name="name">Имя сборки</param>
        /// <param name="gameVersion">Версия игры</param>
        /// <param name="modloader">Тип модлоадера</param>
        /// <param name="modloaderVersion">Версия модлоадера. Это поле необходимо только если есть модлоадер</param>
        public InstanceClient(string name, InstanceSource type, string gameVersion, ModloaderType modloader, string modloaderVersion = null)
        {
            Name = name;
            Type = type;
            GameVersion = gameVersion;
            InLibrary = true;
            _localId = GenerateInstanceId();

            CreateFileStruct(modloader, modloaderVersion);
            _installedInstances[_localId] = this;

            SaveInstalledInstancesList();
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
                    Name = Name,
                    Type = Type,
                    NotDownloaded = true,
                };
            }

            // сохраняем этот список
            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instanesList.json", JsonConvert.SerializeObject(list));
        }

        public static void DefineInstalledInstances()
        {
            var list = DataFilesManager.GetFile<InstalledInstancesFormat>(WithDirectory.DirectoryPath + "/instanesList.json");

            if (list != null)
            {
                foreach (string localId in list.Keys)
                {
                    //проверяем установлен ли этот модпак и не содержит ли его id запрещенных символов
                    if (Directory.Exists(WithDirectory.DirectoryPath + "/instances/" + localId) && !Regex.IsMatch(localId.Replace("_", ""), @"[^a-zA-Z0-9]"))
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
                                _externaLocalIdsPairs[externalID] = localId;
                            }
                        }

                        //получаем асетсы модпаков
                        InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/assets.json");

                        if (assetsData != null && assetsData.mainImage != null)
                        {
                            string file = WithDirectory.DirectoryPath + "/instances-assets/" + assetsData.mainImage;
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
                            instance = new InstanceClient(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? "Unknown name",
                                Summary = assetsData.Summary ?? "",
                                Author = assetsData.author ?? "Unknown author",
                                Description = assetsData.description ?? "",
                                Categories = assetsData.categories ?? new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }
                        else
                        {
                            instance = new InstanceClient(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? "Unknown name",
                                Summary = "",
                                Author = "Unknown author",
                                Description = "",
                                Categories = new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }

                        instance.InLibrary = true;
                        instance.IsInstalled = true;
                        instance.CheckUpdates();
                        _installedInstances[localId] = instance;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает список модпаков для библиотки.
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
                        InstanceClient instanceClient;
                        if (_externaLocalIdsPairs.ContainsKey(nwModpack))
                        {
                            instanceClient = _installedInstances[_externaLocalIdsPairs[nwModpack]];
                            instanceClient.CheckUpdates();
                        }
                        else
                        {
                            instanceClient = new InstanceClient(InstanceSource.Nightworld, nwModpack)
                            {
                                Name = nwInstances[nwModpack].Name ?? "Unknown name",
                                Logo = null,
                                Categories = nwInstances[nwModpack].Categories ?? new List<Category>(),
                                GameVersion = nwInstances[nwModpack].GameVersion ?? "",
                                Summary = nwInstances[nwModpack].Summary ?? "",
                                Description = nwInstances[nwModpack].Description ?? "",
                                Author = nwInstances[nwModpack].Author ?? "Unknown author"
                            };
                        }

                        var e = new AutoResetEvent(false);
                        events.Add(e);
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, instanceClient, nwInstances[nwModpack].MainImage });

                        instances.Add(instanceClient);
                    }

                    i++;
                }
            }
            else if (type == InstanceSource.Curseforge)
            {
                List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, ModpacksCategories.All, searchFilter);
                foreach (var instance in curseforgeInstances)
                {
                    string author = "";
                    if (instance.authors != null && instance.authors.Count > 0 && instance.authors[0].name != null)
                    {
                        author = instance.authors[0].name;
                    }

                    InstanceClient instanceClient;
                    if (_externaLocalIdsPairs.ContainsKey(instance.id.ToString()))
                    {
                        instanceClient = _installedInstances[_externaLocalIdsPairs[instance.id.ToString()]];
                        instanceClient.CheckUpdates();
                    }
                    else
                    {
                        instanceClient = new InstanceClient(InstanceSource.Curseforge, instance.id.ToString())
                        {
                            Name = instance.name ?? "Unknown name",
                            Logo = null,
                            Categories = instance.categories,
                            GameVersion = (instance.gameVersionLatestFiles != null && instance.gameVersionLatestFiles.Count > 0) ? instance.gameVersionLatestFiles[0].gameVersion : "",
                            Summary = instance.summary ?? "",
                            Description = instance.summary ?? "",
                            Author = (instance.authors != null && instance.authors.Count > 0) ? instance.authors[0].name : "Unknown author"
                        };
                    }

                    if (instance.attachments != null && instance.attachments.Count > 0)
                    {
                        string url = instance.attachments[0].thumbnailUrl;
                        foreach (var attachment in instance.attachments)
                        {
                            if (attachment.isDefault)
                            {
                                url = attachment.thumbnailUrl;
                                break;
                            }
                        }

                        var e = new AutoResetEvent(false);
                        events.Add(e);
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, instanceClient, url });
                    }

                    instances.Add(instanceClient);
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
        /// Получает всю инфу о модпаке.
        /// </summary>
        /// <returns>InstanceData, содержащий в ебе данные на все случаи жизни</returns>
        public InstanceData GetFullInfo()
        {
            switch (Type)
            {
                case InstanceSource.Curseforge:
                    {
                        var data = CurseforgeApi.GetInstance(_externalId);
                        var images = new List<byte[]>();
                        using (var webClient = new WebClient())
                        {
                            foreach (var item in data.attachments)
                            {
                                try
                                {
                                    if (!item.isDefault && !item.url.Contains("avatars"))
                                        images.Add(webClient.DownloadData(item.url));
                                }
                                catch { }
                            }
                        }

                        return new InstanceData
                        {
                            Categories = data.categories,
                            Description = data.summary,
                            Summary = data.summary,
                            TotalDownloads = (long)data.downloadCount,
                            GameVersion = (data.gameVersionLatestFiles != null && data.gameVersionLatestFiles.Count > 0) ? data.gameVersionLatestFiles[0].gameVersion : "",
                            LastUpdate = DateTime.Parse(data.dateModified).ToString("dd MMM yyyy"),
                            Modloader = data.Modloader,
                            Images = images,
                            WebsiteUrl = data.websiteUrl
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
                            WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + _externalId
                        };
                    }
                default:
                    return new InstanceData
                    {
                        Categories = new List<Category>(),
                        Description = "This modpack is not have description but you can add it.",
                        Summary = "This modpack is not have description but you can add it.",
                        TotalDownloads = 0,
                        GameVersion = "",
                        LastUpdate = null,
                        Modloader = ModloaderType.None,
                        Images = WithDirectory.LoadMcScreenshots(_localId)
                    };

            }
        }

        /// <summary>
        /// Обновляет или скачивает сборку. Сборка должна быть добавлена в библиотеку.
        /// </summary>
        public void UpdateInstance()
        {
            Console.WriteLine("download 0");
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, 1, 0, 0);

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            instanceSettings.Merge(UserData.GeneralSettings, true);

            IPrototypeInstance instance;

            switch (Type)
            {
                case InstanceSource.Nightworld:
                    instance = new NightworldIntance(_localId, false);
                    break;
                case InstanceSource.Local:
                    instance = new LocalInstance(_localId);
                    break;
                case InstanceSource.Curseforge:
                    instance = new CurseforgeInstance(_localId, false);
                    break;
                default:
                    instance = null;
                    break;
            }

            InstanceInit result = instance.Check(out string gameVersion);
            if (result == InstanceInit.Successful)
            {
                string javaPath;
                if (instanceSettings.CustomJava == false)
                {
                    using (JavaChecker javaCheck = new JavaChecker(gameVersion))
                    {
                        if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                        {
                            ProgressHandler?.Invoke(DownloadStageTypes.Java, 0, 0, 0);
                            bool downloadResult = javaCheck.Update(delegate (int percent)
                            {
                                ProgressHandler?.Invoke(DownloadStageTypes.Java, 0, 0, percent);
                            });

                            if (!downloadResult)
                            {
                                ComplitedDownload?.Invoke(InstanceInit.JavaDownloadError, null, false);
                                return;
                            }
                        }

                        if (checkResult == JavaChecker.CheckResult.Successful)
                        {
                            javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                        }
                        else
                        {
                            ComplitedDownload?.Invoke(InstanceInit.JavaDownloadError, null, false);
                            return;
                        }
                    }
                }
                else
                {
                    javaPath = instanceSettings.JavaPath;
                }

                InitData res = instance.Update(javaPath, ProgressHandler);
                IsInstalled = (res.InitResult == InstanceInit.Successful);
                Console.WriteLine("RESULT " + res.InitResult);
                ComplitedDownload?.Invoke(res.InitResult, res.DownloadErrors, false);
            }
            else
            {
                ComplitedDownload?.Invoke(result, null, false);
            }
        }

        /// <summary>
        /// Запускает сборку. Если надо её докачивает
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
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, true);

                launchGame.Run(data, ComplitedLaunch, GameExited);
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
                SaveInstalledInstancesList();
            }
        }

        private void CheckUpdates()
        {
            var infoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + _localId + "/instancePlatformData.json");
            if (infoData == null || infoData.id == null)
            {
                UpdateAvailable = true;
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

                    List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(infoData.id); //получем информацию об этом модпаке

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
            Random rnd = new Random();

            string instanceId = Name;
            instanceId = instanceId.Replace(" ", "_");

            using (SHA1 sha = new SHA1Managed())
            {
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
            }

            return instanceId;
        }

        private static void ImageDownload(object state)
        {
            object[] array = state as object[];

            AutoResetEvent e = (AutoResetEvent)array[0];
            InstanceClient instanceInfo = (InstanceClient)array[1];
            string url = (string)array[2];

            try
            {
                using (var webClient = new WebClient())
                {
                    instanceInfo.Logo = webClient.DownloadData(url);
                }
            }
            catch
            {
                instanceInfo.Logo = null;
            }

            e.Set();
        }

        /// <summary>
        /// Создает необходимую структуру файлов для сборки при её добавлении в библиотеку (ну при создании локальной)
        /// </summary>
        public void CreateFileStruct(ModloaderType modloader, string modloaderVersion)
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

        public string GetDirectoryPath() => @"" + UserData.GeneralSettings.GamePath.Replace("/", @"\") + @"\instances\" + _localId;

        public Settings GetSettings() => DataFilesManager.GetSettings(_localId);

        public void SaveSettings(Settings settings) => DataFilesManager.SaveSettings(settings, _localId);
    }
}
