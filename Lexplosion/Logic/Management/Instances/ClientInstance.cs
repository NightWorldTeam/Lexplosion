using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

    class ClientInstance
    {
        public readonly InstanceSource Type;
        private string _externalId = null;
        private string _localId = null;

        private static Dictionary<string, ClientInstance> _installedInstances = new Dictionary<string, ClientInstance>();

        #region info
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public byte[] Logo { get; private set; }
        public List<Category> Categories { get; private set; }
        public string GameVersion { get; private set; }
        public string Summary { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public bool NotDownloaded { get; private set; } = false;
        #endregion

        /// <summary>
        /// Этот конструктор создаёт еще не установленную сборку. То есть используется для сборок из каталога
        /// </summary>
        /// <param name="type">Собста тип модпака</param>
        /// <param name="externalID">Его внешний айдишник</param>
        private ClientInstance(InstanceSource type, string externalID)
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
        private ClientInstance(InstanceSource type, string externalID, string localId) : this(type, externalID)
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
        public ClientInstance(string name, InstanceSource type, string gameVersion, ModloaderType modloader, string modloaderVersion = null)
        {
            Name = name;
            Type = type;
            GameVersion = gameVersion;
            string localId = GenerateInstanceId();

            Directory.CreateDirectory(WithDirectory.DirectoryPath + "/instances/" + localId);

            VersionManifest manifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    gameVersion = gameVersion,
                    modloaderVersion = modloaderVersion,
                    modloaderType = modloader
                }
            };

            _installedInstances[localId] = this;
            DataFilesManager.SaveManifest(localId, manifest);

            // деаем список всех установленных сборок
            var list = new InstalledInstancesFormat();
            foreach (var inst in _installedInstances.Keys)
            {
                list[inst] = new InstalledInstance
                {
                    Name = name,
                    Type = type,
                    NotDownloaded = false,
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

                        ClientInstance intance;
                        if (assetsData != null)
                        {
                            intance = new ClientInstance(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? "Uncnown name",
                                Summary = assetsData.Summary ?? "",
                                Author = assetsData.author ?? "Uncnown author",
                                Description = assetsData.description ?? "",
                                Categories = assetsData.categories ?? new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }
                        else
                        {
                            intance = new ClientInstance(list[localId].Type, externalID, localId)
                            {
                                Name = list[localId].Name ?? "Uncnown name",
                                Summary = "",
                                Author = "Uncnown author",
                                Description = "",
                                Categories = new List<Category>(),
                                GameVersion = "1.10.2",
                                Logo = logo
                            };
                        }

                        intance.CheckUpdates();
                        _installedInstances[localId] = intance;
                    }
                }
            }
        }

        public static List<ClientInstance> GetInstalledInstances()
        {
            List<ClientInstance> list = new List<ClientInstance>();
            foreach(ClientInstance instance in _installedInstances.Values)
            {
                list.Add(instance);
            }

            return list;
        }

        public static List<ClientInstance> GetOutsideInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            Console.WriteLine("UploadInstances " + pageIndex);

            List<ClientInstance> Instances = new List<ClientInstance>();
            List<AutoResetEvent> events = new List<AutoResetEvent>();

            if (type == InstanceSource.Nightworld)
            {
                Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = NightWorldApi.GetInstancesList();

                int i = 0;
                foreach (string nwModpack in nwInstances.Keys)
                {
                    if (i < pageSize * (pageIndex + 1))
                    {
                        var clientIinstance = new ClientInstance(InstanceSource.Nightworld, nwModpack)
                        {
                            Name = nwInstances[nwModpack].Name ?? "Uncnown name",
                            Logo = null,
                            Categories = nwInstances[nwModpack].Categories ?? new List<Category>(),
                            GameVersion = nwInstances[nwModpack].GameVersion ?? "",
                            Summary = nwInstances[nwModpack].Summary ?? "",
                            Description = nwInstances[nwModpack].Description ?? "",
                            Author = nwInstances[nwModpack].Author ?? "Uncnown author"
                        };

                        var e = new AutoResetEvent(false);
                        events.Add(e);
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, clientIinstance, nwInstances[nwModpack].MainImage });

                        clientIinstance.IsInstalled = _installedInstances.ContainsKey(nwModpack);
                        if (clientIinstance.IsInstalled)
                        {
                            clientIinstance.CheckUpdates();
                        }

                        Instances.Add(clientIinstance);
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

                    var clientIinstance = new ClientInstance(InstanceSource.Curseforge, instance.id.ToString())
                    {
                        Name = instance.name ?? "Unknown name",
                        Logo = null,
                        Categories = instance.categories,
                        GameVersion = (instance.gameVersionLatestFiles != null && instance.gameVersionLatestFiles.Count > 0) ? instance.gameVersionLatestFiles[0].gameVersion : "",
                        Summary = instance.summary ?? "",
                        Description = instance.summary ?? "",
                        Author = (instance.authors != null && instance.authors.Count > 0) ? instance.authors[0].name : "Uncnown author"
                    };

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
                        ThreadPool.QueueUserWorkItem(ImageDownload, new object[] { e, clientIinstance, url });
                    }

                    clientIinstance.IsInstalled = _installedInstances.ContainsKey(instance.id.ToString());
                    if (clientIinstance.IsInstalled)
                    {
                        clientIinstance.CheckUpdates();
                    }

                    Instances.Add(clientIinstance);
                }
            }

            foreach (var e in events)
            {
                e.WaitOne();
            }

            Console.WriteLine("UploadInstances End " + pageIndex);

            return Instances;
        }

        public InstanceData GetRestData()
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
                        Description = null,
                        Summary = null,
                        TotalDownloads = 0,
                        GameVersion = "",
                        LastUpdate = null,
                        Modloader = ModloaderType.None,
                        Images = WithDirectory.LoadMcScreenshots(_localId)
                    };

            }
        }

        public void UpdateInstance(ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload)
        {
            ProgressHandler(DownloadStageTypes.Prepare, 1, 0, 0);

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
                            ProgressHandler(DownloadStageTypes.Java, 0, 0, 0);
                            bool downloadResult = javaCheck.Update(delegate (int percent)
                            {
                                ProgressHandler(DownloadStageTypes.Java, 0, 0, percent);
                            });

                            if (!downloadResult)
                            {
                                ComplitedDownload(InstanceInit.JavaDownloadError, null, false);
                                return;
                            }
                        }

                        if (checkResult == JavaChecker.CheckResult.Successful)
                        {
                            javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                        }
                        else
                        {
                            ComplitedDownload(InstanceInit.JavaDownloadError, null, false);
                            return;
                        }
                    }
                }
                else
                {
                    javaPath = instanceSettings.JavaPath;
                }

                InitData res = instance.Update(javaPath, ProgressHandler);
                Console.WriteLine("RESULT " + res.InitResult);
                ComplitedDownload(res.InitResult, res.DownloadErrors, false);
            }
            else
            {
                ComplitedDownload(result, null, false);
            }
        }

        public void Run(ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload, ComplitedLaunchCallback ComplitedLaunch, GameExitedCallback GameExited)
        {
            ProgressHandler(DownloadStageTypes.Prepare, 1, 0, 0);

            Dictionary<string, string> xmx = new Dictionary<string, string>();
            xmx["eos"] = "2700";
            xmx["tn"] = "2048";
            xmx["oth"] = "2048";
            xmx["lt"] = "512";

            /*int k = 0;
            int c = 0;
            if (xmx.ContainsKey(instanceId) && int.TryParse(xmx[instanceId], out k) && int.TryParse(UserData.Settings["xmx"], out c))
            {
                if (c < k)
                    MainWindow.Obj.SetMessageBox("Клиент может не запуститься из-за малого количества выделенной памяти. Рекомендуется выделить " + xmx[instanceId] + "МБ", "Предупреждение");
            }*/

            Settings instanceSettings = DataFilesManager.GetSettings(_localId);
            LaunchGame launchGame = new LaunchGame(_localId, instanceSettings, Type);
            InitData data = launchGame.Initialization(ProgressHandler);

            if (data.InitResult == InstanceInit.Successful)
            {
                ComplitedDownload(data.InitResult, data.DownloadErrors, true);

                launchGame.Run(data, ComplitedLaunch, GameExited);
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
            else
            {
                ComplitedDownload(data.InitResult, data.DownloadErrors, false);
            }
        }

        public void AddToLibrary()
        {
            _localId = GenerateInstanceId();
            _installedInstances[_localId] = this;
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
            ClientInstance instanceInfo = (ClientInstance)array[1];
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
    }
}
