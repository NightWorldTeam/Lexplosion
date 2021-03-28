using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Logic
{
    static class ManageLogic
    {
        public static void DefineListInstances()
        {
            if (UserData.InstancesList == null)
            {
                if (!UserData.offline)
                {
                    UserData.InstancesList = ToServer.GetModpaksList();

                    Dictionary<string, string> temp = WithDirectory.GetModpaksList();
                    foreach(string key in temp.Keys)
                    {
                        if (!UserData.InstancesList.ContainsKey(key))
                        {
                            UserData.InstancesList[key] = temp[key];
                        }

                    }

                    new Thread(delegate () {
                        WithDirectory.SaveModpaksList(UserData.InstancesList);
                    }).Start();

                }
                else
                {
                    UserData.InstancesList = WithDirectory.GetModpaksList();

                }

            }
        }

        public static void СlientManager()
        {

            if (LaunchGame.isRunning)
            {
                LaunchGame.KillProcess();
                return;
            }

            MainWindow.Obj.SetProcessBar("Выполняется запуск игры");

            if (UserData.InstancesList.ContainsKey(MainWindow.Obj.selectedModpack))
            {
                Dictionary<string, string> xmx = new Dictionary<string, string>();
                xmx["eos"] = "2700";
                xmx["tn"] = "2048";
                xmx["oth"] = "2048";
                xmx["lt"] = "512";

                int k = 0;
                int c = 0;
                if (xmx.ContainsKey(MainWindow.Obj.selectedModpack) && int.TryParse(xmx[MainWindow.Obj.selectedModpack], out k) && int.TryParse(UserData.settings["xmx"], out c))
                {
                    if (c < k)
                        MainWindow.Obj.SetMessageBox("Клиент может не запуститься из-за малого количества выделенной памяти. Рекомендуется выделить " + xmx[MainWindow.Obj.selectedModpack] + "МБ", "Предупреждение");
                }

                new Thread(delegate () {
                    Run(MainWindow.Obj.selectedModpack);
                }).Start();

                void Run(string initModPack)
                {
                    Dictionary<string, string> instanceSettings = WithDirectory.GetSettings(initModPack);
                    InitData data = LaunchGame.Initialization(initModPack, instanceSettings);

                    if (data != null)
                    {

                        if (data.errors.Contains("javaPathError"))
                        {
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Не удалось определить путь до Java!", "Ошибка 940");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (data.errors.Contains("gamePathError"))
                        {
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Ошибка при определении игровой директории!", "Ошибка 950");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (UserData.offline && (UserData.settings.ContainsKey(MainWindow.Obj.selectedModpack + "-update") && UserData.settings[MainWindow.Obj.selectedModpack + "-update"] == "true"))
                        { //если лаунчер запущен в оффлайн режиме и выбранный модпак поставлен на обновление
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Клиент поставлен на обновление, но лаунчер запущен в оффлайн режиме! Войдите в онлайн режим.", "Ошибка 980");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if ((data.files == null && (UserData.offline || UserData.settings["noUpdate"] == "true")) && !(UserData.settings.ContainsKey(MainWindow.Obj.selectedModpack + "-update") && UserData.settings[MainWindow.Obj.selectedModpack + "-update"] == "true"))
                        { //если  data.files равно null при вылюченных обновлениях или при оффлайн игре. При том модпак не стоит на обновлении
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Вы должны хотя бы 1 раз запустить клиент в онлайн режиме и с включенными обновлениями!", "Ошибка 970");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;

                        }
                        else if (data.files == null)
                        {
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Не удалось запустить игру!", "Ошибка 930");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                            return;
                        }

                        string errorsText = "\n\n";
                        foreach (string error in data.errors)
                            errorsText += error + "\n";

                        if (errorsText == "\n\n")
                        {
                            string command = LaunchGame.FormCommand(initModPack, data.files.version, data.files.version.minecraftJar.name, data.files.libraries, instanceSettings);
                            LaunchGame.Run(command, initModPack);
                            WithDirectory.SaveSettings(UserData.settings);

                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.launchedModpack = MainWindow.Obj.selectedModpack;
                                MainWindow.Obj.IsInstalled[MainWindow.Obj.selectedModpack] = true;
                                //ClientManagement.Content = "Остановить";
                            });

                        }
                        else
                        {
                            MainWindow.Obj.Dispatcher.Invoke(delegate {
                                MainWindow.Obj.SetMessageBox("Не удалось загрузить следующие файлы:" + errorsText, "Ошибка 960");
                                //InitProgressBar.Visibility = Visibility.Collapsed;
                            });
                        }

                        data = null;

                    }
                    else
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate {
                            MainWindow.Obj.SetMessageBox("Не удалось запустить игру!", "Ошибка 930");
                            //InitProgressBar.Visibility = Visibility.Collapsed;
                        });
                    }

                }

            }
        }

    }
}
