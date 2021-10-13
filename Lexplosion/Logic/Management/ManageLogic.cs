using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows.Controls;
using System.ComponentModel;

namespace Lexplosion.Logic.Management
{
    static class ManageLogic
    {
        public delegate void ProgressHandlerDelegate(int procents);
        public static event ProgressHandlerDelegate ProgressHandler;

        public delegate void ComplitedDownloadDelegate(InstanceInit result, List<string> downloadErrors);
        public static event ComplitedDownloadDelegate ComplitedDownload;

        public static AuthCode Auth(string login, string password, bool saveUser)
        {
            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    UserData.login = response["login"];
                    UserData.UUID = response["UUID"];
                    UserData.accessToken = response["accesToken"];
                    UserData.PaswordSHA = Сryptography.Sha256(password);

                    if (saveUser)
                    {
                        UserData.settings["login"] = login;
                        UserData.settings["password"] = password;

                        DataFilesManager.SaveSettings(UserData.settings);
                    }

                    UserData.isAuthorized = true;

                    return AuthCode.Successfully;
                }
                else
                {
                    return AuthCode.DataError;
                }
            }
            else
            {
                return AuthCode.NoConnect;
            }
        }

        public static void DefineListInstances()
        {
            UserData.Instances.List = DataFilesManager.GetInstancesList();
            UserData.Instances.ExternalIds = new Dictionary<string, string>();
            UserData.Instances.Assets = new Dictionary<string, InstanceAssets>();

            foreach (string instance in UserData.Instances.List.Keys)
            {
                //получаем внешние айдишники всех не локальных модпаков
                if(UserData.Instances.List[instance].Type != InstanceSource.Local)
                {
                    InstancePlatformData data = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + instance + "/instancePlatformData.json");
                    if (data != null && data.id != null)
                    {
                        UserData.Instances.ExternalIds[data.id] = instance;
                    }
                }

                //получаем асетсы модпаков
                InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.directory + "/instances-assets/" + instance + "/assets.json");

                if (assetsData != null && File.Exists(WithDirectory.directory + "/instances-assets/" + assetsData.mainImage))
                {
                    UserData.Instances.Assets[instance] = new InstanceAssets
                    {
                        mainImage = "/" + assetsData.mainImage, // TODO: если эти значения null то заменять на пустую строку 
                        description = assetsData.description,
                        author = assetsData.author
                    };
                }
            }
        }

        public static void UpdateInstance(string instanceId)
        {
            Lexplosion.Run.ThreadRun(delegate ()
            {
                InstanceSource type = UserData.Instances.List[instanceId].Type;
                IPrototypeInstance instance;

                switch (type)
                {
                    case InstanceSource.Nightworld:
                        instance = new NightworldIntance(instanceId, false, ProgressHandler);
                        break;
                    case InstanceSource.Local:
                        instance = new LocalInstance(instanceId);
                        break;
                    case InstanceSource.Curseforge:
                        instance = new CurseforgeInstance(instanceId, false, ProgressHandler);
                        break;
                    default:
                        instance = null;
                        break;
                }

                InstanceInit result = instance.Check();
                if (result == InstanceInit.Successful)
                {
                    InitData res = instance.Update();
                    ComplitedDownload(res.InitResult, res.DownloadErrors);
                }
                else
                {
                    ComplitedDownload(result, null);
                }

                foreach (Delegate d in ProgressHandler.GetInvocationList())
                {
                    ProgressHandler -= (ProgressHandlerDelegate)d;
                }
            });
        }

        public static void СlientManager(string instanceId)
        {
            if (LaunchGame.runnigInstance != "")
            {
                LaunchGame.KillProcess();
                // TODO: Тут вырезал строку
                //Gui.PageType.Right.Menu.InstanceContainerPage.obj.LaunchButtonBlock = false; //разлочиваем кнопку запуска

                return;
            }

            LaunchGame.runnigInstance = instanceId;
            InstanceSource type = UserData.Instances.List[instanceId].Type;

            // MainWindow.Obj.SetProcessBar("Выполняется запуск игры");

            Dictionary<string, string> xmx = new Dictionary<string, string>();
            xmx["eos"] = "2700";
            xmx["tn"] = "2048";
            xmx["oth"] = "2048";
            xmx["lt"] = "512";

            int k = 0;
            int c = 0;
            if (xmx.ContainsKey(instanceId) && int.TryParse(xmx[instanceId], out k) && int.TryParse(UserData.settings["xmx"], out c))
            {
                if (c < k)
                    MainWindow.Obj.SetMessageBox("Клиент может не запуститься из-за малого количества выделенной памяти. Рекомендуется выделить " + xmx[instanceId] + "МБ", "Предупреждение");
            }

            Lexplosion.Run.ThreadRun(delegate ()
            {
                Run(instanceId, type);
            });

            void Run(string initModPack, InstanceSource instype)
            {
                Dictionary<string, string> instanceSettings = DataFilesManager.GetSettings(initModPack);
                InitData data = LaunchGame.Initialization(initModPack, instanceSettings, instype, ProgressHandler);

                if (data.InitResult == InstanceInit.Successful)
                {    
                    string command = LaunchGame.CreateCommand(initModPack, data, instanceSettings);
                    LaunchGame.Run(command, initModPack);
                    DataFilesManager.SaveSettings(UserData.settings);

                    /*MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.launchedModpack = MainWindow.Obj.selectedModpack;
                        MainWindow.Obj.IsInstalled[MainWindow.Obj.selectedModpack] = true;
                        //ClientManagement.Content = "Остановить";
                    });*/
                }
                else
                {
                    /*string errorsText = "\n\n" + string.Join("\n", data.Errors) + "\n";

                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Не удалось загрузить следующие файлы:" + errorsText, "Ошибка 960");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });*/
                }

                //Gui.PageType.Right.Menu.InstanceContainerPage.obj.LaunchButtonBlock = false; //разлочиваем кнопку запуска
            }
        }

        public static string GenerateInstanceId(string instanceName)
        {
            Random rnd = new Random();

            string instanceId = instanceName;
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

                    if (UserData.Instances.List.ContainsKey(instanceId))
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
                        while (UserData.Instances.List.ContainsKey(instanceId_));
                        instanceId = instanceId_;
                    }
                } 
                else if (UserData.Instances.List.ContainsKey(instanceId))
                {
                    string instanceId_ = instanceId;
                    int i = 0;
                    do
                    {
                        instanceId_ = instanceId + "_" + i;
                        i++;
                    }
                    while (UserData.Instances.List.ContainsKey(instanceId_));

                    instanceId = instanceId_;
                }
            }

            return instanceId;
        }

        public static bool ChckIntanceUpdates(string instanceId, InstanceSource type)
        {
            var infoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + instanceId + "/instancePlatformData.json");
            if(infoData == null || infoData.id == null)
            {
                return true;
            }

            switch (type)
            {
                case InstanceSource.Curseforge:
                    if(!Int32.TryParse(infoData.id, out _))
                    {
                        return true;
                    }

                    List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(infoData.id); //получем информацию об этом модпаке

                    //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии 
                    foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
                    {
                        if (ver.id > infoData.instanceVersion)
                        {
                            return true;
                        }
                    }
                    break;
            }

            return false;
        } 

        public static string CreateInstance(string name, InstanceSource type, string gameVersion, string forge, string externalId = "")
        {
            string instanceId = GenerateInstanceId(name);

            UserData.Instances.AddInstance(instanceId, new InstanceParametrs
            {
                Name = name,
                Type = type
            }, null, externalId);

            DataFilesManager.SaveInstancesList(UserData.Instances.List);
            Directory.CreateDirectory(WithDirectory.directory + "/instances/" + instanceId);

            VersionManifest manifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    gameVersion = gameVersion,
                    forgeVersion = forge
                }
            };
            DataFilesManager.SaveManifest(instanceId, manifest);

            if(type != InstanceSource.Local)
            {
                var instanceData = new InstancePlatformData
                {
                    id = externalId
                };

                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + instanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(instanceData));
            }

            return instanceId;
        }

        public static List<OutsideInstance> GetOutsideInstances(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter = "")
        {
            List<string> CategoriesListConverter(List<CurseforgeInstanceInfo.Category> categories)
            {
                List<string> znfvrdfga = new List<string>();
                foreach (var c in categories)
                {
                    znfvrdfga.Add(c.name);
                }

                return znfvrdfga;
            }

            List<OutsideInstance> Instances = new List<OutsideInstance>();

            if (type == InstanceSource.Nightworld)
            {
                Dictionary<string, NWInstanceInfo> nwInstances = NightWorldApi.GetInstancesList();
                int i = 0;
                foreach (string nwModpack in nwInstances.Keys)
                {
                    if (i >= pageSize * pageIndex)
                    {
                        OutsideInstance instanceInfo = new OutsideInstance()
                        {
                            Name = nwInstances[nwModpack].name ?? "Uncnown name",
                            Author = nwInstances[nwModpack].author ?? "",
                            MainImageUrl = nwInstances[nwModpack].mainImage, // TODO: url до картинке может быть битым и не только тут
                            Categories = nwInstances[nwModpack].categories ?? new List<string>(),
                            Description = nwInstances[nwModpack].description ?? "",
                            DownloadCount = 0,
                            Type = InstanceSource.Nightworld,
                            Id = nwModpack
                        };

                        instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(nwModpack);

                        if (instanceInfo.IsInstalled)
                        {
                            instanceInfo.UpdateAvailable = ChckIntanceUpdates(UserData.Instances.List[UserData.Instances.ExternalIds[nwModpack]].Name, InstanceSource.Nightworld);
                        }

                        Instances.Add(instanceInfo);
                    }

                    i++;
                }
            }
            else if (type == InstanceSource.Curseforge)
            {
                List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex*pageSize, ModpacksCategories.All, searchFilter);
                foreach (var instance in curseforgeInstances)
                {
                    OutsideInstance instanceInfo = new OutsideInstance()
                    {
                        Name = instance.name,
                        Author = instance.authors[0].name, // TODO: тут может быть null
                        MainImageUrl = instance.attachments[0].thumbnailUrl, // TODO: тут тоже может быть null
                        Categories = CategoriesListConverter(instance.categories),
                        Description = instance.summary,
                        DownloadCount = instance.downloadCount,
                        Type = InstanceSource.Curseforge,
                        Id = instance.id.ToString()
                    };

                    instanceInfo.IsInstalled = UserData.Instances.ExternalIds.ContainsKey(instance.id.ToString());

                    if (instanceInfo.IsInstalled)
                    {
                        instanceInfo.UpdateAvailable = ChckIntanceUpdates(UserData.Instances.List[UserData.Instances.ExternalIds[instance.id.ToString()]].Name, InstanceSource.Curseforge);
                    }

                    Instances.Add(instanceInfo);
                }
            }

            return Instances;
        }
    }
}
