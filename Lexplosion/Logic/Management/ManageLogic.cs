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
using System.Threading;
using static Lexplosion.Logic.Network.CurseforgeApi;

namespace Lexplosion.Logic.Management
{
    static class ManageLogic
    {
        public delegate void ProgressHandlerCallback(int stagesCount, int stage, int procents);
        public delegate void ComplitedDownloadCallback(InstanceInit result, List<string> downloadErrors, bool launchGame);
        public delegate void ComplitedLaunchCallback(string instanceId, bool successful);
        public delegate void GameExitedCallback(string instanceId);

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

        public static void UpdateInstance(string instanceId, ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload)
        {
            ProgressHandler(1, 0, 0);

            Lexplosion.Run.TaskRun(delegate ()
            {
                InstanceSource type = UserData.Instances.List[instanceId].Type;
                IPrototypeInstance instance;

                switch (type)
                {
                    case InstanceSource.Nightworld:
                        instance = new NightworldIntance(instanceId, false, ProgressHandler);
                        break;
                    case InstanceSource.Local:
                        instance = new LocalInstance(instanceId, ProgressHandler);
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
                    ComplitedDownload(res.InitResult, res.DownloadErrors, false);
                }
                else
                {
                    ComplitedDownload(result, null, false);
                }
            });
        }

        public static void СlientManager(string instanceId, ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload, ComplitedLaunchCallback ComplitedLaunch, GameExitedCallback GameExited)
        {
            InstanceSource type = UserData.Instances.List[instanceId].Type;

            // MainWindow.Obj.SetProcessBar("Выполняется запуск игры");
            ProgressHandler(1, 0, 0);

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

            Lexplosion.Run.TaskRun(delegate ()
            {
                Run(instanceId, type, ProgressHandler);
            });

            void Run(string initModPack, InstanceSource instype, ProgressHandlerCallback progressHandler)
            {
                Dictionary<string, string> instanceSettings = DataFilesManager.GetSettings(initModPack);
                InitData data = LaunchGame.Initialization(initModPack, instanceSettings, instype, progressHandler);

                if (data.InitResult == InstanceInit.Successful)
                {
                    ComplitedDownload(data.InitResult, data.DownloadErrors, true);

                    string command = LaunchGame.CreateCommand(initModPack, data, instanceSettings);
                    LaunchGame.Run(command, initModPack, ComplitedLaunch, GameExited);
                    DataFilesManager.SaveSettings(UserData.settings);
                }
                else
                {
                    ComplitedDownload(data.InitResult, data.DownloadErrors, false);
                }
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

        public static bool CheckIntanceUpdates(string instanceId, InstanceSource type)
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

        public static string CreateInstance(string name, InstanceSource type, string gameVersion, ModloaderType modloader, string modloaderVersion, string externalId = "")
        {   
            Console.WriteLine(externalId);
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
                    modloaderVersion = modloaderVersion,
                    modloaderType = modloader
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

        public static bool InstallAddon(int projectID, int fileID, string instanceId, string gameVersion)
        {
            var installedAddons = DataFilesManager.GetFile<Dictionary<string, InstalledAddonInfo>>(WithDirectory.directory + "/instances/" + instanceId + "/installedAddons.json");
            if (installedAddons == null)
            {
                installedAddons = new Dictionary<string, InstalledAddonInfo>();
            }

            Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)> addonsList
                = CurseforgeApi.DownloadAddon(projectID, fileID, "/instances/" + instanceId + "/", true, gameVersion);
            
            foreach (string file in addonsList.Keys)
            {
                if(addonsList[file].Item2 == DownloadAddonRes.Successful)
                {
                    installedAddons[file] = addonsList[file].Item1;
                }    
            }

            DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + instanceId + "/installedAddons.json", JsonConvert.SerializeObject(installedAddons));

            return true;
        }

        public static ImportResult ImportInstance(string zipFile, out List<string> errors, ProgressHandlerCallback ProgressHandler)
        {
            string instanceId;
            ImportResult res = WithDirectory.ImportInstance(zipFile, out errors, out instanceId);
            LocalInstance instance = new LocalInstance(instanceId, ProgressHandler);

            instance.Check(); // TODO: тут вовзращать ошибки
            instance.Update();

            // TODO: Тут вырезал строку
            /*
            if (Gui.PageType.Right.Menu.InstanceContainerPage.obj != null)
            {
                Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
                Gui.PageType.Right.Menu.InstanceContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId].Name, "NightWorld", "test", new List<string>());
            }
            */

            return res;
        }
    }
}
