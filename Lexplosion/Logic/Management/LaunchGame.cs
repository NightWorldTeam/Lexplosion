using System.Diagnostics;
using System.IO;
using Lexplosion.Logic.Objects;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System.Collections.Generic;
using System;

namespace Lexplosion.Logic.Management
{
    class LaunchGame
    {
        private Process process = null;
        private Gateway gameGateway = null;

        private string _instanceId;
        private Settings _settings;
        private InstanceSource _type;

        private static LaunchGame classInstance = null;

        private bool removeImportantTaskMark = false;
        private object removeImportantTaskLocker = new object();

        public LaunchGame(string instanceId, Settings instanceSettings, InstanceSource type)
        {
            classInstance = this;

            instanceSettings.Merge(UserData.GeneralSettings, true);

            _settings = instanceSettings;
            _instanceId = instanceId;
            _type = type;
        }

        private string CreateCommand(InitData data)
        {
            string command;
            string versionPath = _settings.GamePath + "/instances/" + _instanceId + "/version/" + data.VersionFile.minecraftJar.name;

            if (_settings.GameArgs.Length > 0 && _settings.GameArgs[_settings.GameArgs.Length - 1] != ' ')
                _settings.GameArgs += " ";

            command = " -Djava.library.path=\"" + _settings.GamePath + "/natives/" + (data.VersionFile.CustomVersionName ?? data.VersionFile.gameVersion) + "\" -cp ";

            foreach (string lib in data.Libraries.Keys)
            {
                command += "\"" + _settings.GamePath + "/libraries/" + lib + "\";";
            }

            command += "\"" + versionPath + "\"";
            command += @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Dhttp.agent=\"Mozilla/5.0\"";
            command += " -Xmx" + _settings.Xmx + "M -Xms" + _settings.Xms + "M " + _settings.GameArgs;
            command += data.VersionFile.mainClass + " --username " + UserData.Login + " --version " + data.VersionFile.gameVersion;
            command += " --gameDir \"" + _settings.GamePath + "/instances/" + _instanceId + "\"";
            command += " --assetsDir \"" + _settings.GamePath + "/assets" + "\"";
            command += " --assetIndex " + data.VersionFile.assetsVersion;
            command += " --uuid " + UserData.UUID + " --accessToken " + UserData.AccessToken + " --userProperties [] --userType legacy ";
            command += data.VersionFile.arguments;
            command += " --width " + _settings.WindowWidth + " --height " + _settings.WindowHeight;

            return command.Replace(@"\", "/");
        }

        public bool Run(InitData data, ComplitedLaunchCallback ComplitedLaunch, GameExitedCallback GameExited)
        {
            string command = CreateCommand(data);

            process = new Process();
            gameGateway = new Gateway(UserData.UUID, UserData.AccessToken, "194.61.2.176");
            Lexplosion.Run.AddImportantTask();

            UserStatusSetter.GameStart(UserData.Instances.Record[_instanceId].Name);

            if (_settings.ShowConsole == true)
            {
                //App.Current.Dispatcher.Invoke(delegate ()
                //{
                //    if (!ConsoleWindow.isShow)
                //    {
                //        ConsoleWindow.Window.Show();
                //        ConsoleWindow.isShow = true;
                //    }

                //    ConsoleWindow.Window.Update("Выполняется запуск игры...");
                //    ConsoleWindow.Window.Update(command);
                //});
            }

            bool launcherVisible = true;
            bool gameVisible = false;
            string consoleText = "";

            try
            {
                process.StartInfo.FileName = _settings.JavaPath;
                process.StartInfo.WorkingDirectory = _settings.GamePath + "/instances/" + _instanceId;
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
                            ComplitedLaunch(_instanceId, true);

                            if (_settings.HiddenMode == true)
                            {
                                //MainWindow.Obj.Dispatcher.Invoke(delegate { MainWindow.Obj.Hide(); });
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
                    //if (ConsoleWindow.isShow)
                    //{
                    //    MainWindow.Obj.Dispatcher.Invoke(delegate
                    //    {
                    //        ConsoleWindow.Window.Update(e.Data);
                    //    });
                    //}
                    //else
                    //{
                    //    process.OutputDataReceived -= WriteToConsole;
                    //}
                }

                process.OutputDataReceived += BeforeLaunch;

                if (_settings.ShowConsole == true)
                    process.OutputDataReceived += WriteToConsole;

                process.Exited += (sender, ea) =>
                {
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

                    UserStatusSetter.GameStop();

                    lock (removeImportantTaskLocker)
                    {
                        if (!removeImportantTaskMark)
                        {
                            removeImportantTaskMark = true;
                            Lexplosion.Run.RemoveImportantTask();
                        }
                    }                  

                    if (!gameVisible)
                    {
                        App.Current.Dispatcher.Invoke(delegate ()
                        {
                            ComplitedLaunch(_instanceId, false);

                            // TODO: перенести это в ConsoleWindow
                            //if (!ConsoleWindow.isShow)
                            //{
                            //    ConsoleWindow.Window.Show();
                            //    ConsoleWindow.isShow = true;
                            //}
                            //ConsoleWindow.Window.Update(consoleText);
                            //ConsoleWindow.Window.Update(command);
                        });

                        consoleText = "";
                    }

                    if (!launcherVisible)
                    {
                        //App.Current.Dispatcher.Invoke(delegate { MainWindow.Obj.Show(); });
                    }

                    GameExited(_instanceId);
                };

                process.Start();
                process.BeginOutputReadLine();

                gameGateway.Initialization(process.Id);

                return true;
            }
            catch
            {
                ComplitedLaunch(_instanceId, false);

                return false;
            }
        }

        public InitData Initialization(ProgressHandlerCallback progressHandler)
        {
            InitData Error(InstanceInit init)
            {
                return new InitData
                {
                    InitResult = init
                };
            }

            //try
            {
                WithDirectory.Create(_settings.GamePath);
                InitData data = null;

                if (!Directory.Exists(_settings.GamePath) || !_settings.GamePath.Contains(":"))
                    return Error(InstanceInit.GamePathError);

                bool autoUpdate = (_settings.AutoUpdate == true);

                if (!UserData.Offline)
                {
                    IPrototypeInstance instance;
                    switch (_type)
                    {
                        case InstanceSource.Nightworld:
                            instance = new NightworldIntance(_instanceId, !autoUpdate);
                            break;
                        case InstanceSource.Local:
                            instance = new LocalInstance(_instanceId);
                            break;
                        case InstanceSource.Curseforge:
                            instance = new CurseforgeInstance(_instanceId, !autoUpdate);
                            break;
                        default:
                            instance = null;
                            break;
                    }

                    InstanceInit result = instance.Check(out string gameVersion);
                    if (result == InstanceInit.Successful)
                    {
                        if (_settings.CustomJava == false)
                        {
                            using (JavaChecker javaCheck = new JavaChecker(gameVersion))
                            {
                                if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                                {
                                    progressHandler(DownloadStageTypes.Java, 0, 0, 0);

                                    bool updateResult = javaCheck.Update(delegate (int percent)
                                    {
                                        progressHandler(DownloadStageTypes.Java, 0, 0, percent);
                                    });

                                    if (!updateResult)
                                    {
                                        return Error(InstanceInit.JavaDownloadError);
                                    }
                                }

                                if (checkResult == JavaChecker.CheckResult.Successful)
                                {
                                    _settings.JavaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                                }
                                else
                                {
                                    return Error(InstanceInit.JavaDownloadError);
                                }
                            }
                        }

                        data = instance.Update(_settings.JavaPath, progressHandler);
                    }
                    else
                    {
                        return Error(result);
                    }
                }
                else
                {
                    VersionManifest files = DataFilesManager.GetManifest(_instanceId, true);

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
            //catch 
            //{
            //    return Error(InstanceInit.UnknownError);
            //}
        }

        public static void GameStop()
        {
            classInstance.Stop();
            classInstance = null;
        }

        private void Stop()
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

            lock (removeImportantTaskLocker)
            {
                if (!removeImportantTaskMark)
                {
                    removeImportantTaskMark = true;
                    Lexplosion.Run.RemoveImportantTask();
                }
            }
        }
    }
}
