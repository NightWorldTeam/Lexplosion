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

        public delegate void ComplitedDownloadDelegate(Dictionary<string, string> errors);
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

        /*public static void DefineListInstances()
        {
            if (UserData.InstancesList == null)
            {
                if (!UserData.offline)
                {
                    UserData.InstancesList = ToServer.GetModpaksList();

                    Dictionary<string, string> temp = DataFilesManager.GetModpaksList();
                    foreach(string key in temp.Keys)
                    {

                        if (!UserData.InstancesList.ContainsKey(key))
                        {
                            UserData.InstancesList[key] = temp[key];
                        }

                    }

                    Run.ThreadRun(delegate ()
                    {
                        DataFilesManager.SaveModpaksList(UserData.InstancesList);
                    });

                }
                else
                {
                    UserData.InstancesList = DataFilesManager.GetModpaksList();

                }

            }
        }*/

        public static void DefineListInstances()
        {
            if (UserData.InstancesList == null)
            {
                UserData.InstancesList = DataFilesManager.GetInstancesList();
                UserData.CursforgeInstances = new Dictionary<int, string>();
            }

            UserData.instancesAssets = new Dictionary<string, InstanceAssets>();

            foreach (string instance in UserData.InstancesList.Keys)
            {
                //получаем курсовские айди всех установленных курсфорджевских модпаков
                if(UserData.InstancesList[instance].Type == InstanceType.Curseforge)
                {
                    CfJsonBase data = DataFilesManager.GetFile<CfJsonBase>(WithDirectory.directory + "/instances/" + instance + "/cursforgeData.json");
                    if (data != null && data.InfoData != null && data.InfoData.ContainsKey("cursforgeId"))
                    {
                        UserData.CursforgeInstances[data.InfoData["cursforgeId"]] = instance;
                    }
                }

                //получаем асетсы модпаков
                InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.directory + "/instances-assets/" + instance + "/assets.json");

                if (assetsData != null && File.Exists(WithDirectory.directory + "/instances-assets/" + instance + "/" + assetsData.mainImage))
                {
                    UserData.instancesAssets[instance] = new InstanceAssets
                    {
                        mainImage = "/" + instance + "/" + assetsData.mainImage
                    };
                }
            }

        }

        public static void DownloadInstance(string instanceId, InstanceType type)
        {
            Lexplosion.Run.ThreadRun(delegate ()
            {
                IPrototypeInstance instance;

                switch (type)
                {
                    case InstanceType.Nightworld:
                        instance = new NightworldIntance(instanceId);
                        break;
                    case InstanceType.Local:
                        instance = new LocalInstance(instanceId);
                        break;
                    case InstanceType.Curseforge:
                        instance = new CurseforgeInstance(instanceId, ProgressHandler);
                        break;
                    default:
                        instance = null;
                        break;
                }

                string result = instance.Check();
                instance.Update(); // TODO: тут выводить ошибки

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

            InstanceType type = UserData.InstancesList[instanceId].Type;

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

            void Run(string initModPack, InstanceType instype)
            {
                Dictionary<string, string> instanceSettings = DataFilesManager.GetSettings(initModPack);

                InitData data = LaunchGame.Initialization(initModPack, instanceSettings, instype, ProgressHandler);

                if (data.Errors.Contains("javaPathError"))
                {
                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Не удалось определить путь до Java!", "Ошибка 940");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                    return;

                }
                else if (data.Errors.Contains("gamePathError"))
                {
                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Ошибка при определении игровой директории!", "Ошибка 950");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                    return;

                }
                /*else if (UserData.offline && (UserData.settings.ContainsKey(instanceId + "-update") && UserData.settings[instanceId + "-update"] == "true")) //если лаунчер запущен в оффлайн режиме и выбранный модпак поставлен на обновление
                {
                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Клиент поставлен на обновление, но лаунчер запущен в оффлайн режиме! Войдите в онлайн режим.", "Ошибка 980");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                    return;

                }
                else if ((data.files == null && (UserData.offline || UserData.settings["noUpdate"] == "true")) && !(UserData.settings.ContainsKey(instanceId + "-update") && UserData.settings[instanceId + "-update"] == "true"))
                { //если  data.files равно null при вылюченных обновлениях или при оффлайн игре. При том модпак не стоит на обновлении
                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Вы должны хотя бы 1 раз запустить клиент в онлайн режиме и с включенными обновлениями!", "Ошибка 970");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                    //return;
                    MessageBox.Show("3");

                }
                else if (data.files == null)
                {
                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Не удалось запустить игру!", "Ошибка 930");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                    //return;
                    MessageBox.Show("4");
                }*/

                if (data.Errors.Count == 0)
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
                    string errorsText = "\n\n" + string.Join("\n", data.Errors) + "\n";

                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Не удалось загрузить следующие файлы:" + errorsText, "Ошибка 960");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                }

                data = null;

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

                    if (UserData.InstancesList.ContainsKey(instanceId))
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
                        while (UserData.InstancesList.ContainsKey(instanceId_));
                        instanceId = instanceId_;
                    }
                } 
                else if (UserData.InstancesList.ContainsKey(instanceId))
                {
                    string instanceId_ = instanceId;
                    int i = 0;
                    do
                    {
                        instanceId_ = instanceId + "_" + i;
                        i++;
                    }
                    while (UserData.InstancesList.ContainsKey(instanceId_));

                    instanceId = instanceId_;
                }
            }

            return instanceId;
        }


        public static InitData UpdateInstance(string instanceId)
        {
            InitData Error(string error)
            {
                return new InitData
                {
                    Errors = new List<string>() { error },
                    VersionFile = null,
                };
            }

            InstanceType type = UserData.InstancesList[instanceId].Type;

            IPrototypeInstance instance;
            switch (type)
            {
                case InstanceType.Nightworld:
                    instance = new NightworldIntance(instanceId);
                    break;
                case InstanceType.Local:
                    instance = new LocalInstance(instanceId);
                    break;
                case InstanceType.Curseforge:
                    instance = new CurseforgeInstance(instanceId, ProgressHandler);
                    break;
                default:
                    instance = null;
                    break;

            }

            string result = instance.Check();
            if (result == "")
            {
                return instance.Update();
            }
            else
            {
                return Error(result);
            }
        }

        public static bool ChckIntanceUpdates(string instanceId, InstanceType type)
        {
            /*switch (type)
            {
                case InstanceType.Curseforge:
                    List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(InstanceData.InfoData["cursforgeId"]); //получем информацию об этом модпаке

                    //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии
                    foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
                    {
                        if (ver.id > InstanceData.InfoData["instanceVersion"])
                        {
                            InstanceData.InfoData["instanceVersion"] = ver.id;
                            Info = ver;
                        }
                    }
                    break;
            }*/

            return false;
        } 

        public static string CreateInstance(string name, InstanceType type, string gameVersion, string forge, int cursforgeId = 0)
        {
            string instanceId = GenerateInstanceId(name);

            UserData.InstancesList[instanceId] = new InstanceParametrs
            {
                Name = name,
                Type = type
            };

            DataFilesManager.SaveInstancesList(UserData.InstancesList);
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

            if(type == InstanceType.Curseforge)
            {
                var instanceData = new CurseforgeInstance.CurseforgeLocalInfo
                {
                    InfoData = new Dictionary<string, int>()
                    {
                        ["cursforgeId"] = cursforgeId
                    },
                    LocalFiles = new List<string>()
                };

                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + instanceId + "/cursforgeData.json", JsonConvert.SerializeObject(instanceData));
            }

            return instanceId;
        }


        // эта хуйня нужна чисто чтобы json модпака курса в методе DefineListInstances декодировать
        class CfJsonBase
        {
            public Dictionary<string, int> InfoData;
        }
    }
}
