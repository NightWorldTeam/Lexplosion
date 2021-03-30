using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Lexplosion.Logic.Objects;
using Lexplosion.Gui.Windows;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic.Management
{

    static class LaunchGame
    {
        private static Process process = new Process();
        public static bool isRunning = false;
        public static string runnigInstance = "";

        public static string FormCommand(string instanceId, VersionInfo versionInfo, string versionFile, List<string> libraries, Dictionary<string, string> instanceSettings)
        {
            int number;
            if (!instanceSettings.ContainsKey("xmx") || !Int32.TryParse(instanceSettings["xmx"], out number))
            {
                instanceSettings["xmx"] = UserData.settings["xmx"];
            }

            if (!instanceSettings.ContainsKey("xms") || !Int32.TryParse(instanceSettings["xms"], out number))
            {
                instanceSettings["xms"] = UserData.settings["xms"];
            }

            string command;
            string versionPath = UserData.settings["gamePath"] + "/instances/" + instanceId + "/version/" + versionFile;

            if (!instanceSettings.ContainsKey("gameArgs"))
                instanceSettings["gameArgs"] = UserData.settings["gameArgs"];

            if (instanceSettings["gameArgs"].Length > 0 && instanceSettings["gameArgs"][instanceSettings["gameArgs"].Length - 1] != ' ')
                instanceSettings["gameArgs"] += " ";

            command = @" -Djava.library.path=" + UserData.settings["gamePath"] + "/instances/" + instanceId + "/version/natives -cp ";

            foreach (string lib in libraries)
            {
                command += UserData.settings["gamePath"] + "/libraries/" + lib + ";";
            }

            command += versionPath + @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Xmx" + instanceSettings["xmx"] + "M -Xms" + instanceSettings["xms"] + "M " + instanceSettings["gameArgs"];
            command += versionInfo.mainClass + " --username " + UserData.login + " --version " + versionInfo.gameVersion;
            command += " --gameDir " + UserData.settings["gamePath"] + "/instances/" + instanceId;
            command += " --assetsDir " + UserData.settings["gamePath"] + "/assets";
            command += " --assetIndex " + versionInfo.assetsVersion;
            command += " --uuid " + UserData.UUID + " --accessToken " + UserData.accessToken + " --userProperties [] --userType legacy ";
            command += versionInfo.arguments;
            command += " --width " + UserData.settings["windowWidth"] + " --height " + UserData.settings["windowHeight"];

            return command.Replace(@"\", "/");
        }


        public static bool Run(string command, string instanceId)
        {

            if (UserData.settings["showConsole"] == "true")
            {
                MainWindow.Obj.Dispatcher.Invoke(delegate
                {
                    if (!ConsoleWindow.isShow)
                    {
                        ConsoleWindow.Window.Show();
                        ConsoleWindow.isShow = true;
                    }

                    ConsoleWindow.Window.Update("Выполняется запуск игры...");
                    ConsoleWindow.Window.Update(command);
                });

            }

            bool launcherVisible = true;
            bool gameVisible = false;
            string consoleText = "";

            try
            {

                MainWindow.Obj.Dispatcher.Invoke(delegate
                {
                    //MainWindow.window.InitProgressBar.Visibility = Visibility.Visible;
                });

                process.StartInfo.FileName = UserData.settings["javaPath"];
                process.StartInfo.WorkingDirectory = UserData.settings["gamePath"] + "/instances/" + instanceId;
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.EnableRaisingEvents = true;

                void BeforeLaunch(object s, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        consoleText += e.Data + "\n";
                        string[] words = e.Data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        if (words.Length > 1 && words[words.Length - 2] == " LWJGL Version")
                        {
                            MainWindow.Obj.Dispatcher.Invoke(delegate
                            {
                                //MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            });

                            if (UserData.settings["hiddenMode"] == "true")
                            {
                                MainWindow.Obj.Dispatcher.Invoke(delegate { MainWindow.Obj.Hide(); });
                                launcherVisible = false;
                            }

                            process.OutputDataReceived -= BeforeLaunch;

                            gameVisible = true;
                            consoleText = "";
                        }
                    }

                }

                void WriteToConsole(object s, DataReceivedEventArgs e)
                {
                    if (ConsoleWindow.isShow)
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate
                        {
                            ConsoleWindow.Window.Update(e.Data);
                        });

                    }
                    else
                    {
                        process.OutputDataReceived -= WriteToConsole;
                    }
                }

                process.OutputDataReceived += BeforeLaunch;

                if (UserData.settings["showConsole"] == "true")
                    process.OutputDataReceived += WriteToConsole;

                process.Exited += (sender, ea) =>
                {
                    if (!gameVisible)
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate
                        {
                            //MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            MainWindow.Obj.SetMessageBox("Возникла ошибка при запуске игры.");

                            if (!ConsoleWindow.isShow)
                            {
                                ConsoleWindow.Window.Show();
                                ConsoleWindow.isShow = true;
                            }
                            ConsoleWindow.Window.Update(consoleText);
                        });

                        consoleText = "";
                    }

                    if (!launcherVisible)
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate { MainWindow.Obj.Show(); });
                    }

                    MainWindow.Obj.Dispatcher.Invoke(delegate
                    {
                        //MainWindow.window.ClientManagement.Content = "Играть";
                        //MainWindow.window.launchedModpack = "";
                        //MainWindow.window.ClientManagement.IsEnabled = true;
                    });

                    process = new Process();
                    isRunning = false;
                    runnigInstance = "";

                };

                process.Start();
                process.BeginOutputReadLine();
                isRunning = true;

                return true;

            } catch {
                MainWindow.Obj.Dispatcher.Invoke(delegate
                {
                    MainWindow.Obj.SetMessageBox("Сбой запуска! Не удалось запустить процесс.");
                });
                return false;
            }

        }

        public static InitData Initialization(string instanceId, Dictionary<string, string> instanceSettings)
        {

            InitData Error(string error)
            {
                return new InitData
                {
                    errors = new List<string>() { error },
                    files = null
                };
            }

            try
            {
                SetDefaultSettings();

                if (!UserData.settings.ContainsKey("javaPath"))
                    return Error("javaPathError");

                WithDirectory.Create(UserData.settings["gamePath"]);
                List<string> errors = new List<string>();
                InstanceFiles files = null;

                if (!UserData.settings.ContainsKey("gamePath") || !Directory.Exists(UserData.settings["gamePath"]) || !UserData.settings["gamePath"].Contains(":"))
                    return Error("gamePathError");

                bool isLocal = instanceSettings.ContainsKey("isLocal") && instanceSettings["isLocal"] == "true";
                bool updateInstance = instanceSettings.ContainsKey("update") && instanceSettings["update"] == "true";
                bool noUpdate = UserData.settings["noUpdate"] == "false" || (instanceSettings.ContainsKey("noUpdate") && instanceSettings["noUpdate"] == "false");

                if (updateInstance)
                    DataFilesManager.DeleteLastUpdates(instanceId);

                if (!UserData.offline && (updateInstance || noUpdate))
                {
                    //если модпак локальный, то получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
                    if (!isLocal) 
                    {
                        files = ToServer.GetFilesList(instanceId, isLocal);
                    }
                    else
                    {
                        files = DataFilesManager.GetFilesList(instanceId);
                        files = ToServer.GetFilesList(files.version.gameVersion, isLocal);
                    }

                    if (files == null || !WithDirectory.Check(files, instanceId))
                        return null;


                    if (WithDirectory.countFiles > 0)
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate{
                            //MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            //MainWindow.window.GridLoadingWindow.Visibility = Visibility.Visible;
                        });

                        errors = WithDirectory.Update(files, instanceId, MainWindow.Obj);
                        WithDirectory.countFiles = 0;
                    }

                    files.data = null;
                    files.natives = null;

                    DataFilesManager.SaveFilesList(instanceId, files);
                    //MainWindow.window.Dispatcher.Invoke(delegate { MainWindow.window.GridLoadingWindow.Visibility = Visibility.Collapsed; });

                } 
                else 
                {
                    files = DataFilesManager.GetFilesList(instanceId);
                }

                if (updateInstance)
                {
                    instanceSettings["update"] = "false";
                    DataFilesManager.SaveSettings(instanceSettings, instanceId);
                }

                return new InitData
                {
                    errors = errors,
                    files = files
                };

            } catch {
                return null;
            }

        }

        public static void KillProcess()
        {
            process.Kill();
            isRunning = false;
        }

        public static void SetDefaultSettings()
        {
            /*<!-- получение директории до джавы -->*/
            if (!UserData.settings.ContainsKey("javaPath") || string.IsNullOrWhiteSpace(UserData.settings["javaPath"]))
            {
                try
                {
                    using (RegistryKey jre = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        RegistryKey java = jre.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");
                        UserData.settings["javaPath"] = (java.OpenSubKey(java.GetValue("CurrentVersion").ToString()).GetValue("JavaHome").ToString() + @"/bin/javaw.exe").Replace(@"\", "/");
                    }

                } catch {
                    UserData.settings["javaPath"] = "";
                }
            }

            //определение директории игры
            if (!UserData.settings.ContainsKey("gamePath"))
                UserData.settings["gamePath"] = LaunсherSettings.gamePath;

            //установка озу для процесса
            if (!UserData.settings.ContainsKey("xmx"))
            {
                if (UserData.settings["javaPath"].Contains("Program Files (x86)"))
                    UserData.settings["xmx"] = "512";
                else
                    UserData.settings["xmx"] = "1024";
            }

            if (!UserData.settings.ContainsKey("xms"))
                UserData.settings["xms"] = "256";

            //установка размера окна 
            if (!UserData.settings.ContainsKey("windowWidth"))
                UserData.settings["windowWidth"] = "854";

            if (!UserData.settings.ContainsKey("windowHeight"))
                UserData.settings["windowHeight"] = "480";

            //режим скачивания обновлений 
            if (!UserData.settings.ContainsKey("noUpdate"))
                UserData.settings["noUpdate"] = "false";

            //скрытие консоли
            if (!UserData.settings.ContainsKey("showConsole"))
                UserData.settings["showConsole"] = "false";

            //скрытие лаунчера при запуске
            if (!UserData.settings.ContainsKey("hiddenMode"))
                UserData.settings["hiddenMode"] = "false";

            //java Args
            if (!UserData.settings.ContainsKey("gameArgs"))
                UserData.settings["gameArgs"] = "";

            //выбранный модпак
            if (!UserData.settings.ContainsKey("selectedModpack"))
                UserData.settings["selectedModpack"] = "0";
        }
    }
}
