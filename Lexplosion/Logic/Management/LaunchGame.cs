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

namespace Lexplosion.Logic.Management
{
    static class LaunchGame // TODO: возможно из статично класса перевести в обычный 
    {
        private static Process process = null;
        private static Gateway gameGateway = null;

        public static string CreateCommand(string instanceId, InitData data, Dictionary<string, string> instanceSettings)
        {
            int number;
            if (!instanceSettings.ContainsKey("xmx") || !Int32.TryParse(instanceSettings["xmx"], out number))
            {
                instanceSettings["xmx"] = UserData.Settings["xmx"];
            }

            if (!instanceSettings.ContainsKey("xms") || !Int32.TryParse(instanceSettings["xms"], out number))
            {
                instanceSettings["xms"] = UserData.Settings["xms"];
            }

            string command;
            string versionPath = UserData.Settings["gamePath"] + "/instances/" + instanceId + "/version/" + data.VersionFile.minecraftJar.name;

            if (!instanceSettings.ContainsKey("gameArgs"))
                instanceSettings["gameArgs"] = UserData.Settings["gameArgs"];

            if (instanceSettings["gameArgs"].Length > 0 && instanceSettings["gameArgs"][instanceSettings["gameArgs"].Length - 1] != ' ')
                instanceSettings["gameArgs"] += " ";

            command = " -Djava.library.path=\"" + UserData.Settings["gamePath"] + "/natives/" + data.VersionFile.gameVersion + "\" -cp ";

            /*//Не ебу в чём проблема, но если guava-17.0.jar в списках либраресов на последних местах, то 1.7.10 тупа не запускается. Что за шиза, не понимаю
            //Но этот костыль решает проблему
            if (data.Libraries.ContainsKey("com/google/guava/guava/17.0/guava-17.0.jar"))
            {
                command += UserData.Settings["gamePath"] + "/libraries/com/google/guava/guava/17.0/guava-17.0.jar;";
                data.Libraries.Remove("com/google/guava/guava/17.0/guava-17.0.jar");
            }*/

            foreach (string lib in data.Libraries.Keys)
            {
                //if (!data.Libraries[lib].isNative)
                {
                    command += "\"" + UserData.Settings["gamePath"] + "/libraries/" + lib + "\";";
                }
            }

            command += "\"" + versionPath + "\"";
            command +=  @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Dhttp.agent=\"Mozilla/5.0\"";
            command += " -Xmx" + instanceSettings["xmx"] + "M -Xms" + instanceSettings["xms"] + "M " + instanceSettings["gameArgs"];
            command += data.VersionFile.mainClass + " --username " + UserData.Login + " --version " + data.VersionFile.gameVersion;
            command += " --gameDir \"" + UserData.Settings["gamePath"] + "/instances/" + instanceId + "\"";
            command += " --assetsDir \"" + UserData.Settings["gamePath"] + "/assets" + "\"";
            command += " --assetIndex " + data.VersionFile.assetsVersion;
            command += " --uuid " + UserData.UUID + " --accessToken " + UserData.AccessToken + " --userProperties [] --userType legacy ";
            command += data.VersionFile.arguments;
            command += " --width " + UserData.Settings["windowWidth"] + " --height " + UserData.Settings["windowHeight"];

            return command.Replace(@"\", "/");
        }

        public static bool Run(string command, string instanceId, ManageLogic.ComplitedLaunchCallback ComplitedLaunch, ManageLogic.GameExitedCallback GameExited)
        {
            process = new Process();
            gameGateway = new Gateway(UserData.UUID, UserData.AccessToken, "194.61.2.176");

            UserStatusSetter.GameStart(UserData.Instances.Record[instanceId].Name);

            if (UserData.Settings["showConsole"] == "true")
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
                process.StartInfo.FileName = UserData.Settings["javaPath"];
                process.StartInfo.WorkingDirectory = UserData.Settings["gamePath"] + "/instances/" + instanceId;
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.EnableRaisingEvents = true;

                void BeforeLaunch(object s, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        consoleText += e.Data + "\n";

                        if (e.Data.Contains(" LWJGL Version") || e.Data.Contains("Launching target 'fmlclient' with arguments") || e.Data.Contains("Narrator library for x64 successfully loaded"))
                        {
                            ComplitedLaunch(instanceId, true);

                            if (UserData.Settings["hiddenMode"] == "true")
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

                if (UserData.Settings["showConsole"] == "true")
                    process.OutputDataReceived += WriteToConsole;

                process.Exited += (sender, ea) =>
                {
                    UserStatusSetter.GameStop();

                    if (!gameVisible)
                    {
                        MainWindow.Obj.Dispatcher.Invoke(delegate
                        {
                            ComplitedLaunch(instanceId, false);

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

                    try
                    {
                        process.Dispose();
                    }
                    catch { }

                    try
                    {
                        gameGateway.StopWork();
                    }
                    catch { }

                    GameExited(instanceId);

                    process = null;
                    gameGateway = null;
                };

                process.Start();
                process.BeginOutputReadLine();

                gameGateway.Initialization(process.Id);

                return true;
            } 
            catch 
            {
                ComplitedLaunch(instanceId, false);

                gameGateway = null;
                process = null;

                return false;
            }
        }

        public static InitData Initialization(string instanceId, Dictionary<string, string> instanceSettings, InstanceSource type, ManageLogic.ProgressHandlerCallback progressHandler)
        {
            InitData Error(InstanceInit init)
            {
                return new InitData
                {
                    InitResult = init
                };
            }

            //try
            //{
                SetDefaultSettings();

                if (!UserData.Settings.ContainsKey("javaPath")) // TODO: тут скачивать джаву
                    return null;

                WithDirectory.Create(UserData.Settings["gamePath"]);
                InitData data = null;

                if (!UserData.Settings.ContainsKey("gamePath") || !Directory.Exists(UserData.Settings["gamePath"]) || !UserData.Settings["gamePath"].Contains(":"))
                    return Error(InstanceInit.GamePathError);

                bool autoUpdate = instanceSettings.ContainsKey("autoUpdate") && instanceSettings["autoUpdate"] == "true";

                if (!UserData.Offline)
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
            //} 
            //catch 
            //{
            //    return Error(InstanceInit.UnknownError);
            //}
        }

        public static void KillProcess()
        {
            UserStatusSetter.GameStop();

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
        }

        public static void SetDefaultSettings()
        {
            /*<!-- получение директории до джавы -->*/
            if (!UserData.Settings.ContainsKey("javaPath") || string.IsNullOrWhiteSpace(UserData.Settings["javaPath"]))
            {
                try
                {
                    using (RegistryKey jre = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        RegistryKey java = jre.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");
                        UserData.Settings["javaPath"] = (java.OpenSubKey(java.GetValue("CurrentVersion").ToString()).GetValue("JavaHome").ToString() + @"/bin/javaw.exe").Replace(@"\", "/");
                    }

                } catch {
                    UserData.Settings["javaPath"] = "";
                }
            }

            //определение директории игры
            if (!UserData.Settings.ContainsKey("gamePath"))
                UserData.Settings["gamePath"] = LaunсherSettings.gamePath;

            //установка озу для процесса
            if (!UserData.Settings.ContainsKey("xmx"))
            {
                if (UserData.Settings["javaPath"].Contains("Program Files (x86)"))
                    UserData.Settings["xmx"] = "512";
                else
                    UserData.Settings["xmx"] = "1024";
            }

            if (!UserData.Settings.ContainsKey("xms"))
                UserData.Settings["xms"] = "256";

            //установка размера окна 
            if (!UserData.Settings.ContainsKey("windowWidth"))
                UserData.Settings["windowWidth"] = "854";

            if (!UserData.Settings.ContainsKey("windowHeight"))
                UserData.Settings["windowHeight"] = "480";

            //режим скачивания обновлений 
            if (!UserData.Settings.ContainsKey("noUpdate"))
                UserData.Settings["noUpdate"] = "false";

            //скрытие консоли
            if (!UserData.Settings.ContainsKey("showConsole"))
                UserData.Settings["showConsole"] = "false";

            //скрытие лаунчера при запуске
            if (!UserData.Settings.ContainsKey("hiddenMode"))
                UserData.Settings["hiddenMode"] = "false";

            //java Args
            if (!UserData.Settings.ContainsKey("gameArgs"))
                UserData.Settings["gameArgs"] = "";

            //выбранный модпак
            if (!UserData.Settings.ContainsKey("selectedModpack"))
                UserData.Settings["selectedModpack"] = "";
        }
    }
}
