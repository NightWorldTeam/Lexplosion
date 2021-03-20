using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Lexplosion.Gui;
using Microsoft.Win32;
using Lexplosion.Objects;

namespace Lexplosion.Logic
{

    static class LaunchGame
    {
        private static Process process = new Process();
        public static bool isRunning = false;

        public static string FormCommand(string modpack, VersionInfo versionInfo, string versionFile, Dictionary<string, string> libraries, Dictionary<string, string> profileSettings)
        {
            int number;
            if (!profileSettings.ContainsKey("xmx") || !Int32.TryParse(profileSettings["xmx"], out number))
            {
                profileSettings["xmx"] = UserData.settings["xmx"];
            }

            if (!profileSettings.ContainsKey("xms") || !Int32.TryParse(profileSettings["xms"], out number))
            {
                profileSettings["xms"] = UserData.settings["xms"];
            }

            string command;
            string versionPath = UserData.settings["gamePath"] + "/modpacks/" + modpack + "/version/" + versionFile;

            if (!profileSettings.ContainsKey("gameArgs"))
                profileSettings["gameArgs"] = UserData.settings["gameArgs"];

            if (profileSettings["gameArgs"].Length > 0 && profileSettings["gameArgs"][profileSettings["gameArgs"].Length - 1] != ' ')
                profileSettings["gameArgs"] += " ";

            command = @" -Djava.library.path=" + UserData.settings["gamePath"] + "/modpacks/" + modpack + "/version/natives -cp ";

            foreach (string lib in libraries.Keys)
            {
                if (libraries[lib] == "all" || libraries[lib] == "windows")
                {
                    command += UserData.settings["gamePath"] + "/libraries/" + lib + ";";
                }
            }

            command += versionPath + @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Xmx" + profileSettings["xmx"] + "M -Xms" + profileSettings["xms"] + "M " + profileSettings["gameArgs"];
            command += versionInfo.mainClass + " --username " + UserData.login + " --version " + versionInfo.gameVersion;
            command += " --gameDir " + UserData.settings["gamePath"] + "/modpacks/" + modpack;
            command += " --assetsDir " + UserData.settings["gamePath"] + "/assets";
            command += " --assetIndex " + versionInfo.assetsVersion;
            command += " --uuid " + UserData.UUID + " --accessToken " + UserData.accessToken + " --userProperties [] --userType legacy ";
            command += versionInfo.arguments;
            command += " --width " + UserData.settings["windowWidth"] + " --height " + UserData.settings["windowHeight"];

            return command.Replace(@"\", "/");
        }


        public static bool Run(string command, string modpack)
        {
            if (UserData.settings["showConsole"] == "true")
            {
                MainWindow.window.Dispatcher.Invoke(delegate
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

                MainWindow.window.Dispatcher.Invoke(delegate
                {
                    MainWindow.window.InitProgressBar.Visibility = Visibility.Visible;
                });

                process.StartInfo.FileName = UserData.settings["javaPath"];
                process.StartInfo.WorkingDirectory = UserData.settings["gamePath"] + "/modpacks/" + modpack;
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
                            MainWindow.window.Dispatcher.Invoke(delegate
                            {
                                MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            });

                            if (UserData.settings["hiddenMode"] == "true")
                            {
                                MainWindow.window.Dispatcher.Invoke(delegate { MainWindow.window.Hide(); });
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
                        MainWindow.window.Dispatcher.Invoke(delegate
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
                        MainWindow.window.Dispatcher.Invoke(delegate
                        {
                            MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            MainWindow.window.SetMessageBox("Возникла ошибка при запуске игры.");

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
                        MainWindow.window.Dispatcher.Invoke(delegate { MainWindow.window.Show(); });
                    }

                    MainWindow.window.Dispatcher.Invoke(delegate
                    {
                        MainWindow.window.ClientManagement.Content = "Играть";
                        MainWindow.window.launchedModpack = "";
                        MainWindow.window.ClientManagement.IsEnabled = true;
                    });

                    isRunning = false;
                    process = new Process();
                };

                process.Start();
                process.BeginOutputReadLine();
                isRunning = true;

                return true;

            } catch {
                MainWindow.window.Dispatcher.Invoke(delegate
                {
                    MainWindow.window.SetMessageBox("Сбой запуска! Не удалось запустить процесс.");
                });
                return false;
            }

        }

        public static InitData Initialization(string modpack, Dictionary<string, string> profileSettings)
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
                ModpackFiles files = null;

                if (!UserData.settings.ContainsKey("gamePath") || !Directory.Exists(UserData.settings["gamePath"]) || !UserData.settings["gamePath"].Contains(":"))
                    return Error("gamePathError");

                bool updateModpack = profileSettings.ContainsKey("update") && profileSettings["update"] == "true";
                bool noUpdate = UserData.settings["noUpdate"] == "false" || (profileSettings.ContainsKey("noUpdate") && profileSettings["noUpdate"] == "false");

                if (updateModpack)
                    WithDirectory.DeleteLastUpdates(modpack);

                if (!UserData.offline && (updateModpack || noUpdate))
                {
                    files = ToServer.GetFilesList(modpack);

                    if (files == null || !WithDirectory.Check(files, modpack))
                        return null;

                    if (WithDirectory.countFiles > 0)
                    {
                        MainWindow.window.Dispatcher.Invoke(delegate{
                            MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
                            MainWindow.window.GridLoadingWindow.Visibility = Visibility.Visible;
                        });

                        errors = WithDirectory.Update(files, modpack, MainWindow.window);
                        WithDirectory.countFiles = 0;
                    }

                    files.data = null;
                    files.natives = null;

                    WithDirectory.SaveFilesList(modpack, files);
                    MainWindow.window.Dispatcher.Invoke(delegate { MainWindow.window.GridLoadingWindow.Visibility = Visibility.Collapsed; });

                    if (updateModpack)
                    {
                        profileSettings["update"] = "false";
                        WithDirectory.SaveSettings(profileSettings, modpack);
                    }

                } else {
                    files = WithDirectory.GetFilesList(modpack);
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
