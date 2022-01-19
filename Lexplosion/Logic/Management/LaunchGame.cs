using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Lexplosion.Logic.Objects;
using Lexplosion.Gui.Windows;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System.Windows;

namespace Lexplosion.Logic.Management
{
    static class LaunchGame // TODO: возможно из статично класса перевести в обычный 
    {
        private static Process process = null;
        private static Gateway gameGateway = null;
        public static string runnigInstance = "";

        public static string CreateCommand(string instanceId, InitData data, Dictionary<string, string> instanceSettings)
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
            string versionPath = UserData.settings["gamePath"] + "/instances/" + instanceId + "/version/" + data.VersionFile.minecraftJar.name;

            if (!instanceSettings.ContainsKey("gameArgs"))
                instanceSettings["gameArgs"] = UserData.settings["gameArgs"];

            if (instanceSettings["gameArgs"].Length > 0 && instanceSettings["gameArgs"][instanceSettings["gameArgs"].Length - 1] != ' ')
                instanceSettings["gameArgs"] += " ";

            command = @" -Djava.library.path=" + UserData.settings["gamePath"] + "/instances/" + instanceId + "/version/natives -cp ";

            //Не ебу в чём проблема, но если guava-17.0.jar в списках либраресов на последних местах, то 1.7.10 тупа не запускается. Что за шиза, не понимаю
            //Но этот костыль решает проблему
            if (data.Libraries.ContainsKey("com/google/guava/guava/17.0/guava-17.0.jar"))
            {
                command += UserData.settings["gamePath"] + "/libraries/com/google/guava/guava/17.0/guava-17.0.jar;";
                data.Libraries.Remove("com/google/guava/guava/17.0/guava-17.0.jar");
            }

            foreach (string lib in data.Libraries.Keys)
            {
                command += UserData.settings["gamePath"] + "/libraries/" + lib + ";";
            }

            command += versionPath + @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Xmx" + instanceSettings["xmx"] + "M -Xms" + instanceSettings["xms"] + "M " + instanceSettings["gameArgs"];
            command += data.VersionFile.mainClass + " --username " + UserData.login + " --version " + data.VersionFile.gameVersion;
            command += " --gameDir " + UserData.settings["gamePath"] + "/instances/" + instanceId;
            command += " --assetsDir " + UserData.settings["gamePath"] + "/assets";
            command += " --assetIndex " + data.VersionFile.assetsVersion;
            command += " --uuid " + UserData.UUID + " --accessToken " + UserData.accessToken + " --userProperties [] --userType legacy ";
            command += data.VersionFile.arguments;
            command += " --width " + UserData.settings["windowWidth"] + " --height " + UserData.settings["windowHeight"];

            return command.Replace(@"\", "/");
        }

        public static bool Run(string command, string instanceId)
        {
            process = new Process();
            gameGateway = new Gateway();

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

                            // TODO: перенести это в ConsoleWindow
                            if (!ConsoleWindow.isShow) 
                            {
                                ConsoleWindow.Window.Show();
                                ConsoleWindow.isShow = true;
                            }
                            ConsoleWindow.Window.Update(consoleText);
                            ConsoleWindow.Window.Update(command);
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

                    process.Dispose();
                    gameGateway.StopWork();

                    gameGateway = null;
                    process = null;
                    runnigInstance = "";
                };

                process.Start();
                process.BeginOutputReadLine();

                gameGateway.Initialization(process.Id);

                return true;
            } 
            catch 
            {
                MainWindow.Obj.Dispatcher.Invoke(delegate
                {
                    MainWindow.Obj.SetMessageBox("Сбой запуска! Не удалось запустить процесс.");
                });

                gameGateway = null;
                process = null;
                runnigInstance = "";

                return false;
            }
        }

        public static InitData Initialization(string instanceId, Dictionary<string, string> instanceSettings, InstanceSource type, ManageLogic.ProgressHandlerDelegate progressHandler)
        {
            InitData Error(InstanceInit init)
            {
                return new InitData
                {
                    InitResult = init
                };
            }

            try
            {
                SetDefaultSettings();

                if (!UserData.settings.ContainsKey("javaPath")) // TODO: тут скачивать джаву
                    return null;

                WithDirectory.Create(UserData.settings["gamePath"]);
                InitData data = null;

                if (!UserData.settings.ContainsKey("gamePath") || !Directory.Exists(UserData.settings["gamePath"]) || !UserData.settings["gamePath"].Contains(":"))
                    return Error(InstanceInit.GamePathError);

                bool autoUpdate = instanceSettings.ContainsKey("autoUpdate") && instanceSettings["autoUpdate"] == "true";

                if (!UserData.offline)
                {
                    IPrototypeInstance instance;
                    switch (type)
                    {
                        case InstanceSource.Nightworld:
                            instance = new NightworldIntance(instanceId, !autoUpdate, progressHandler);
                            break;
                        case InstanceSource.Local:
                            instance = new LocalInstance(instanceId, progressHandler);
                            break;
                        case InstanceSource.Curseforge:
                            instance = new CurseforgeInstance(instanceId, !autoUpdate, progressHandler);
                            break;
                        default:
                            instance = null;
                            break;
                    }

                    InstanceInit result = instance.Check();
                    if (result == InstanceInit.Successful)
                    {
                        data = instance.Update();
                    }
                    else
                    {
                        return Error(result);
                    }
                }
                else
                {
                    VersionManifest files = DataFilesManager.GetManifest(instanceId, true);

                    if (files != null)
                    {
                        data = new InitData
                        {
                            VersionFile = files.version,
                            Libraries = files.libraries
                        };
                    }
                    else
                    {
                        return Error(InstanceInit.ManifestError);
                    }
                }

                return data;
            } 
            catch 
            {
                return Error(InstanceInit.UnknownError);
            }
        }

        public static void KillProcess()
        {
            try
            {
                process.Kill(); // TODO: тут иногда крашится (ввроде если ошибка скачивания была)
                process.Dispose();
            } 
            catch { }

            try
            {
                gameGateway.StopWork();
            }
            catch { }

            gameGateway = null;
            process = null;
            runnigInstance = "";
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
                UserData.settings["selectedModpack"] = "";
        }
    }
}
