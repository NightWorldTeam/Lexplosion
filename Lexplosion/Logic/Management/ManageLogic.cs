using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System.Windows;

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

            }
        }

        public static void DownloadInstance(string instanceId, string instanceName, InstanceType type)
        {
            UserData.InstancesList[instanceId] = new InstanceParametrs
            {
                Name = instanceName,
                Type = type
            };

            DataFilesManager.SaveModpaksList(UserData.InstancesList);

            Lexplosion.Run.ThreadRun(delegate ()
            {
                Run(instanceId, type);
            });

            void Run(string initModPack, InstanceType instype)
            {
                Dictionary<string, string> instanceSettings = DataFilesManager.GetSettings(initModPack);

                InitData data = LaunchGame.Initialization(initModPack, instanceSettings, instype);
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

                if (data.Errors.Count != 0)
                {
                    string errorsText = "\n\n" + string.Join("\n", data.Errors) + "\n";

                    MainWindow.Obj.Dispatcher.Invoke(delegate {
                        MainWindow.Obj.SetMessageBox("Не удалось загрузить следующие файлы:" + errorsText, "Ошибка 960");
                        //InitProgressBar.Visibility = Visibility.Collapsed;
                    });
                }

            }

        }

        public static void СlientManager(string instanceId, InstanceType type)
        {
            if (LaunchGame.runnigInstance != "")
            {
                LaunchGame.KillProcess();
                Gui.Pages.Right.Menu.InstanceContainerPage.obj.LaunchButtonBlock = false; //разлочиваем кнопку запуска

                return;
            }

            LaunchGame.runnigInstance = instanceId;

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

                InitData data = LaunchGame.Initialization(initModPack, instanceSettings, instype);

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

                    Gui.Pages.Right.Menu.InstanceContainerPage.obj.LaunchButtonBlock = false; //разлочиваем кнопку запуска

            }
        }

    }

}
