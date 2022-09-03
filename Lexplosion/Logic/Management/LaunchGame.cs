using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Lexplosion.Logic.Objects;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Management.Installers;

namespace Lexplosion.Logic.Management
{
    class LaunchGame
    {
        private Process process = null;
        private Gateway gameGateway = null;

        private string _instanceId;
        private Settings _settings;
        private InstanceSource _type;
        private string _javaPath = "";
        private bool _processIsWork;

        private static LaunchGame classInstance = null;

        private bool removeImportantTaskMark = true;
        private object removeImportantTaskLocker = new object();

        public static event Action<string> GameStartEvent;
        public static event Action GameStopEvent;

        private static object loocker = new object();

        public LaunchGame(string instanceId, Settings instanceSettings, InstanceSource type)
        {
            classInstance = this;

            instanceSettings.Merge(UserData.GeneralSettings, true);

            _settings = instanceSettings;
            _instanceId = instanceId;
            _type = type;
        }

        public static event Action<Player> UserConnected;
        public static event Action<Player> UserDisconnected;
        public static event Action<OnlineGameStatus, string> StateChanged;

        private ConcurrentDictionary<string, Player> _connectedPlayers = new ConcurrentDictionary<string, Player>();

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        private static bool GuiIsExists(int processId)
        {
            bool isExists = false;

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id, (hWnd, lParam) =>
                {
                    isExists = true;
                    return false;
                }, IntPtr.Zero);

            return isExists;
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
            command += data.VersionFile.mainClass + " --username " + UserData.User.Login + " --version " + data.VersionFile.gameVersion;
            command += " --gameDir \"" + _settings.GamePath + "/instances/" + _instanceId + "\"";
            command += " --assetsDir \"" + _settings.GamePath + "/assets" + "\"";
            command += " --assetIndex " + data.VersionFile.assetsVersion;
            command += " --uuid " + UserData.User.UUID + " --accessToken " + UserData.User.AccessToken + " --userProperties [] --userType legacy ";
            command += data.VersionFile.arguments;
            command += " --width " + _settings.WindowWidth + " --height " + _settings.WindowHeight;

            return command.Replace(@"\", "/");
        }

        public bool Run(InitData data, ComplitedLaunchCallback ComplitedLaunch, GameExitedCallback GameExited, string gameClientName, bool onlineGame)
        {
            string command = CreateCommand(data);

            process = new Process();
            if (onlineGame)
            {
                lock (loocker)
                {
                    gameGateway = new Gateway(UserData.User.UUID, UserData.User.SessionToken, "194.61.2.176", _settings.OnlineGameDirectConnection);
                    removeImportantTaskMark = false;
                    Lexplosion.Run.AddImportantTask();

                    gameGateway.ConnectingUser += delegate (string uuid)
                    {
                        var player = new Player(uuid,
                            delegate
                            {
                                this.gameGateway?.KickClient(uuid);
                            },
                            delegate
                            {
                                this.gameGateway?.UnkickClient(uuid);
                            }
                        );

                        _connectedPlayers[uuid] = player;
                        UserConnected?.Invoke(player);
                    };

                    gameGateway.DisconnectedUser += delegate (string uuid)
                    {
                        _connectedPlayers.TryRemove(uuid, out Player player);
                        UserDisconnected?.Invoke(player);
                    };

                    gameGateway.StateChanged += StateChanged;
                }  
            }
            
            GameStartEvent?.Invoke(gameClientName);

            if (_settings.ShowConsole == true)
            {
                App.Current.Dispatcher.Invoke(delegate ()
                {
                    Console.WriteLine("Выполняется запуск игры...");
                    Console.WriteLine(command);
                });
            }

            bool launcherVisible = true;
            bool gameVisible = false;

            try
            {
                process.StartInfo.FileName = _javaPath;
                process.StartInfo.WorkingDirectory = _settings.GamePath + "/instances/" + _instanceId;
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.EnableRaisingEvents = true;

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

                    if (_settings.ShowConsole == true)
                    {
                        Console.WriteLine(e.Data);
                    }
                }

                if (_settings.ShowConsole == true)
                    process.OutputDataReceived += WriteToConsole;

                process.Exited += (sender, ea) =>
                {
                    _processIsWork = false;

                    try
                    {
                        process.Dispose();
                    }
                    catch { }

                    try
                    {
                        lock (loocker)
                        {
                            gameGateway?.StopWork();
                            gameGateway = null;
                        }                
                    }
                    catch { }

                    GameStopEvent();

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

                            Console.WriteLine(command);
                        });
                    }

                    if (!launcherVisible)
                    {
                        //App.Current.Dispatcher.Invoke(delegate { MainWindow.Obj.Show(); });
                    }

                    GameExited(_instanceId);
                };

                _processIsWork = process.Start();
                process.BeginOutputReadLine();

                // отслеживаем появление окна
                Lexplosion.Run.TaskRun(delegate ()
                {
                    while (_processIsWork)
                    {
                        Thread.Sleep(1000);

                        if (GuiIsExists(process.Id))
                        {
                            ComplitedLaunch(_instanceId, true);

                            if (_settings.HiddenMode == true)
                            {
                                //MainWindow.Obj.Dispatcher.Invoke(delegate { MainWindow.Obj.Hide(); });
                                launcherVisible = false;
                            }

                            gameVisible = true;
                            break;
                        }   
                    }
                });

                lock (loocker)
                {
                    gameGateway?.Initialization(process.Id);
                }
                    
                return true;
            }
            catch
            {
                _processIsWork = false;
                ComplitedLaunch(_instanceId, false);

                return false;
            }
        }

        public InitData Update(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, bool onlyBase = false)
        {
            IInstallManager instance;

            switch (_type)
            {
                case InstanceSource.Nightworld:
                    instance = new NightworldInstallManager(_instanceId, onlyBase);
                    break;
                case InstanceSource.Local:
                    instance = new LocalInstallManager(_instanceId);
                    break;
                case InstanceSource.Curseforge:
                    instance = new CurseforgeInstallManager(_instanceId, onlyBase);
                    break;
                default:
                    instance = null;
                    break;
            }

            instance.FileDownloadEvent += fileDownloadHandler;
            InstanceInit result = instance.Check(out string gameVersion);

            if (result != InstanceInit.Successful)
            {
                return new InitData
                {
                    InitResult = result
                };
            }   

            if (_settings.CustomJava == false)
            {
                using (JavaChecker javaCheck = new JavaChecker(gameVersion))
                {
                    if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                    {
                        progressHandler?.Invoke(DownloadStageTypes.Java, 0, 0, 0);
                        bool downloadResult = javaCheck.Update(delegate (int percent)
                        {
                            progressHandler?.Invoke(DownloadStageTypes.Java, 0, 0, percent);
                        });

                        if (!downloadResult)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.JavaDownloadError
                            };
                        }
                    }

                    if (checkResult == JavaChecker.CheckResult.Successful)
                    {
                        _javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                    }
                    else
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.JavaDownloadError
                        };
                    }
                }
            }
            else
            {
                _javaPath = _settings.JavaPath;
            }

            return instance.Update(_javaPath, progressHandler);
        }


        public InitData Initialization(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler)
        {
            //try
            {
                WithDirectory.Create(_settings.GamePath);
                InitData data = null;

                if (!Directory.Exists(_settings.GamePath) || !_settings.GamePath.Contains(":"))
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.GamePathError
                    };
                }

                if (!UserData.Offline)
                {
                    return Update(progressHandler, fileDownloadHandler, (_settings.AutoUpdate == false));
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
                        return new InitData
                        {
                            InitResult = InstanceInit.ManifestError
                        };
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
            _processIsWork = false;

            GameStopEvent?.Invoke();

            try
            {
                process.Kill(); // TODO: тут иногда крашится (ввроде если ошибка скачивания была)
                process.Dispose();
            }
            catch { }

            try
            {
                lock (loocker)
                {
                    gameGateway?.StopWork();
                    gameGateway = null;
                }               
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

        public static void RebootOnlineGame()
        {
            if (classInstance != null)
            {
                lock (loocker)
                {
                    if (classInstance.gameGateway != null)
                    {
                        try
                        {
                            classInstance.gameGateway.StopWork();
                        }
                        catch { }

                        classInstance.gameGateway = new Gateway(UserData.User.UUID, UserData.User.SessionToken, "194.61.2.176", classInstance._settings.OnlineGameDirectConnection);
                        classInstance.gameGateway.Initialization(classInstance.process.Id);
                    }
                }
            }
        }
    }
}
