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

        private const string LogoFileName = "logo.png";

        private static Dictionary<string, InstanceClient> _installedInstances = new Dictionary<string, InstanceClient>();

        /// <summary>
        /// Содержит пары состоящие из внешнего и внутреннего id.
        /// </summary>
        private static Dictionary<string, string> _idsPairs = new Dictionary<string, string>();

        #region info
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public byte[] Logo { get; private set; } = null;
        public List<Category> Categories { get; private set; }
        public string GameVersion { get; private set; }
        public string Summary { get; private set; }
        public bool InLibrary { get; private set; }
        public bool UpdateAvailable { get; private set; }
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
        /// Этот конструктор создаёт локальную сборку. Должен использоваться только при создании локлаьной сборки.
        /// </summary>
        /// <param name="name">Название сборки</param>
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

            var assetsData = new InstanceAssets
            {
                Author = UserData.Login
            };

            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances-assets/" + _localId + "/assets.json", JsonConvert.SerializeObject(assetsData));
        }

        /// <summary>
        /// Возвращает основные данные модпака.
        /// </summary>
        public BaseInstanceData GetBaseData
        {
            get 
            {
                return new BaseInstanceData
                {
                    LocalId = _localId,
                    ExternalId = _externalId,
                    Type = Type,
                    GameVersion = GameVersion
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
                                Name = list[localId].Name ?? "Unknown name",
                                Summary = assetsData.Summary ?? "This modpack is not have description but you can add it.",
                                Author = assetsData.Author ?? "Unknown author",
                                Description = assetsData.Description ?? "This modpack is not have description but you can add it.",
                                Categories = assetsData.Categories ?? new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }
                        else
                        {
                            instance = new InstanceClient(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? "Unknown name",
                                Summary = "This modpack is not have description but you can add it.",
                                Author = "Unknown author",
                                Description = "This modpack is not have description but you can add it.",
                                Categories = new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }

                        instance.InLibrary = true;
                        instance.IsInstalled = list[localId].IsInstalled;
                        instance.CheckUpdates();
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
                        InstanceClient instanceClient;
                        if (_idsPairs.ContainsKey(nwModpack))
                        {
                            instanceClient = _installedInstances[_idsPairs[nwModpack]];
                            instanceClient.CheckUpdates();
                            instanceClient.DownloadLogo(nwInstances[nwModpack].MainImage);
                            instanceClient.SaveAssets();

                            if (nwInstances[nwModpack].Categories != null)
                                instanceClient.Categories = nwInstances[nwModpack].Categories;
                            if (nwInstances[nwModpack].GameVersion != null)
                                instanceClient.GameVersion = nwInstances[nwModpack].GameVersion;
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
                                Name = nwInstances[nwModpack].Name ?? "Unknown name",
                                Logo = null,
                                Categories = nwInstances[nwModpack].Categories ?? new List<Category>(),
                                GameVersion = nwInstances[nwModpack].GameVersion ?? "",
                                Summary = nwInstances[nwModpack].Summary ?? "",
                                Description = nwInstances[nwModpack].Description ?? "",
                                Author = nwInstances[nwModpack].Author ?? "Unknown author"
                            };

                            instanceClient.DownloadLogo(nwInstances[nwModpack].MainImage);
                        }

                        instanceClient.WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + nwModpack;

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
                    if (_idsPairs.ContainsKey(instance.id.ToString()))
                    {
                        instanceClient = _installedInstances[_idsPairs[instance.id.ToString()]];
                        instanceClient.CheckUpdates();

                        if (instance.categories != null)
                            instanceClient.Categories = instance.categories;
                        if (instance.gameVersionLatestFiles != null && instance.gameVersionLatestFiles.Count > 0)
                            instanceClient.GameVersion = instance.gameVersionLatestFiles[0].gameVersion;
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
                            Name = instance.name ?? "Unknown name",
                            Logo = null,
                            Categories = instance.categories,
                            GameVersion = (instance.gameVersionLatestFiles != null && instance.gameVersionLatestFiles.Count > 0) ? instance.gameVersionLatestFiles[0].gameVersion : "",
                            Summary = instance.summary ?? "",
                            Description = instance.summary ?? "",
                            Author = (instance.authors != null && instance.authors.Count > 0) ? instance.authors[0].name : "Unknown author"
                        };
                    }

                    instanceClient.WebsiteUrl = instance.websiteUrl;

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

                        instanceClient.DownloadLogo(url);
                        if (_idsPairs.ContainsKey(instance.id.ToString()))
                            instanceClient.SaveAssets();
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
            ProgressHandler?.Invoke(DownloadStageTypes.Prepare, 1, 0, 0);

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Update(ProgressHandler);

            if (data.InitResult == InstanceInit.Successful)
            {
                IsInstalled = (data.InitResult == InstanceInit.Successful);
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
                ComplitedDownload?.Invoke(data.InitResult, data.DownloadErrors, true);

                launchGame.Run(data, ComplitedLaunch, GameExited, Name);
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

        /// <summary>
        /// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo
        /// </summary>
        private void DownloadLogo(string url)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    Logo = webClient.DownloadData(url);
                }
            }
            catch { }
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
        /// Создает необходимую структуру файлов для сборки при её добавлении в библиотеку (ну при создании локальной)
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

        public void GetInstalledAddons(AddonType addonsType)
        {
        }

        //public static ImportResult ImportInstance(string zipFile, out List<string> errors, ProgressHandlerCallback ProgressHandler)
        //{ // TODO : этот метод полная хуйня блять, надо доделать, может даже переделать
        //    string instanceId;
        //    ImportResult res = WithDirectory.ImportInstance(zipFile, out errors, out instanceId);
        //    LocalInstance instance = new LocalInstance(instanceId);

        //    InstanceInit result = instance.Check(out string gameVersion); // TODO: тут вовзращать ошибки

        //    if (result == InstanceInit.Successful)
        //    {
        //        string javaPath;
        //        using (JavaChecker javaCheck = new JavaChecker(gameVersion))
        //        {
        //            if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
        //            {
        //                bool downloadResult = javaCheck.Update(delegate (int percent)
        //                {
        //                    ProgressHandler(DownloadStageTypes.Java, 0, 0, percent);
        //                });

        //                if (!downloadResult)
        //                {
        //                    return ImportResult.JavaDownloadError;
        //                }
        //            }

        //            if (checkResult == JavaChecker.CheckResult.Successful)
        //            {
        //                javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
        //            }
        //            else
        //            {
        //                return ImportResult.JavaDownloadError;
        //            }
        //        }

        //        instance.Update(javaPath, ProgressHandler);
        //    }

        //    // TODO: Тут вырезал строку
        //    /*
        //    if (Gui.PageType.Right.Menu.InstanceContainerPage.obj != null)
        //    {
        //        Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
        //        Gui.PageType.Right.Menu.InstanceContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId].Name, "NightWorld", "test", new List<string>());
        //    }
        //    */

        //    return res;
        //}
    }
}