using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management
{
    static class ManageLogic
    {
        public static AuthCode Auth(string login, string password, bool saveUser)
        {
            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    UserData.Login = response["login"];
                    UserData.UUID = response["UUID"];
                    UserData.AccessToken = response["accesToken"];

                    if (saveUser)
                    {
                        DataFilesManager.SaveAccount(login, password);
                    }

                    UserData.IsAuthorized = true;
                    UserStatusSetter.SetBaseStatus(UserStatusSetter.Statuses.Online);

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
            UserData.Instances.Record = DataFilesManager.GetInstancesList();
            UserData.Instances.ExternalIds = new Dictionary<string, string>();
            UserData.Instances.Assets = new Dictionary<string, InstanceAssets>();

            foreach (string instance in UserData.Instances.Record.Keys)
            {
                //получаем внешние айдишники всех не локальных модпаков
                if (UserData.Instances.Record[instance].Type != InstanceSource.Local)
                {
                    InstancePlatformData data = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + instance + "/instancePlatformData.json");
                    if (data != null && data.id != null)
                    {
                        UserData.Instances.ExternalIds[data.id] = instance;
                    }
                }

                //получаем асетсы модпаков
                InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.DirectoryPath + "/instances-assets/" + instance + "/assets.json");

                if (assetsData != null && File.Exists(WithDirectory.DirectoryPath + "/instances-assets/" + assetsData.mainImage))
                {
                    assetsData.mainImage = "/" + assetsData.mainImage;
                    // TODO: если значения в этом классе null то заменять на пустую строку 
                    UserData.Instances.Assets[instance] = assetsData;
                }
            }
        }

        public static void UpdateInstance(string instanceId, ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload)
        {
            ProgressHandler(DownloadStageTypes.Prepare, 1, 0, 0);

            Settings instanceSettings = DataFilesManager.GetSettings(instanceId);
            instanceSettings.Merge(UserData.GeneralSettings, true);

            InstanceSource type = UserData.Instances.Record[instanceId].Type;
            IPrototypeInstance instance;

            switch (type)
            {
                case InstanceSource.Nightworld:
                    instance = new NightworldIntance(instanceId, false);
                    break;
                case InstanceSource.Local:
                    instance = new LocalInstance(instanceId);
                    break;
                case InstanceSource.Curseforge:
                    instance = new CurseforgeInstance(instanceId, false);
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
                            if (!javaCheck.Update())
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
                ComplitedDownload(res.InitResult, res.DownloadErrors, false);
            }
            else
            {
                ComplitedDownload(result, null, false);
            }
        }

        public static void СlientManager(string instanceId, ProgressHandlerCallback ProgressHandler, ComplitedDownloadCallback ComplitedDownload, ComplitedLaunchCallback ComplitedLaunch, GameExitedCallback GameExited)
        {
            InstanceSource type = UserData.Instances.Record[instanceId].Type;

            // MainWindow.Obj.SetProcessBar("Выполняется запуск игры");
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

            Settings instanceSettings = DataFilesManager.GetSettings(instanceId);
            LaunchGame launchGame = new LaunchGame(instanceId, instanceSettings, type);
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

                    if (UserData.Instances.Record.ContainsKey(instanceId))
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
                        while (UserData.Instances.Record.ContainsKey(instanceId_));
                        instanceId = instanceId_;
                    }
                }
                else if (UserData.Instances.Record.ContainsKey(instanceId))
                {
                    string instanceId_ = instanceId;
                    int i = 0;
                    do
                    {
                        instanceId_ = instanceId + "_" + i;
                        i++;
                    }
                    while (UserData.Instances.Record.ContainsKey(instanceId_));

                    instanceId = instanceId_;
                }
            }

            return instanceId;
        }

        public static bool CheckIntanceUpdates(string instanceId, InstanceSource type)
        {
            var infoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/instancePlatformData.json");
            if (infoData == null || infoData.id == null)
            {
                return true;
            }

            switch (type)
            {
                case InstanceSource.Curseforge:
                    if (!Int32.TryParse(infoData.id, out _))
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
            string instanceId = GenerateInstanceId(name);

            UserData.Instances.AddInstance(instanceId, new InstanceParametrs
            {
                Name = name,
                Type = type,
                UpdateAvailable = false
            }, null, externalId);

            DataFilesManager.SaveInstancesList(UserData.Instances.Record);
            Directory.CreateDirectory(WithDirectory.DirectoryPath + "/instances/" + instanceId);

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

            if (type != InstanceSource.Local)
            {
                var instanceData = new InstancePlatformData
                {
                    id = externalId
                };

                DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(instanceData));
            }

            return instanceId;
        }

        public static ImportResult ImportInstance(string zipFile, out List<string> errors, ProgressHandlerCallback ProgressHandler)
        { // TODO : этот метод полная хуйня блять, надо доделать, может даже переделать
            string instanceId;
            ImportResult res = WithDirectory.ImportInstance(zipFile, out errors, out instanceId);
            LocalInstance instance = new LocalInstance(instanceId);

            InstanceInit result = instance.Check(out string gameVersion); // TODO: тут вовзращать ошибки

            if (result == InstanceInit.Successful)
            {
                string javaPath;
                using (JavaChecker javaCheck = new JavaChecker(gameVersion))
                {
                    if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                    {
                        if (!javaCheck.Update())
                        {
                            return ImportResult.JavaDownloadError;
                        }
                    }

                    if (checkResult == JavaChecker.CheckResult.Successful)
                    {
                        javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                    }
                    else
                    {
                        return ImportResult.JavaDownloadError;
                    }
                }

                instance.Update(javaPath, ProgressHandler);
            }

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
