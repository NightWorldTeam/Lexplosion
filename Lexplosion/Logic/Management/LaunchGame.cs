using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
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

        /// <summary>
        /// Выполняется при запуске процесса игры
        /// </summary>
        public static event Action<string> GameStartEvent;
        /// <summary>
        /// Выполняется после GameStartEvent, когда у майкнрафт появляется окно.
        /// </summary>
        public static event Action GameStartedEvent;
        /// <summary>
        /// Выполняется при завершении процесса игры
        /// </summary>
        public static event Action GameStopEvent;

        private static object loocker = new object();

        private CancellationToken _updateCancelToken;

        public LaunchGame(string instanceId, Settings instanceSettings, InstanceSource type, CancellationToken updateCancelToken)
        {
            classInstance = this;

            instanceSettings.Merge(UserData.GeneralSettings, true);

            _settings = instanceSettings;
            _instanceId = instanceId;
            _type = type;

            _updateCancelToken = updateCancelToken;
        }

        public static event Action<Player> UserConnected;
        public static event Action<Player> UserDisconnected;
        public static event Action<OnlineGameStatus, string> StateChanged;

        private ConcurrentDictionary<string, Player> _connectedPlayers = new ConcurrentDictionary<string, Player>();

        private static bool GuiIsExists(int processId)
        {
            bool isExists = false;

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                NativeMethods.EnumThreadWindows(thread.Id, (hWnd, lParam) =>
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

            string accountType = UserData.User.AccountType.ToString();
            foreach (string lib in data.Libraries.Keys)
            {
                if (lib.Contains("auth"))
                {

                }
                var activation = data.Libraries[lib].activationConditions;
                if (activation?.accountTypes == null || activation.accountTypes.Contains(accountType))
                {
                    command += "\"" + _settings.GamePath + "/libraries/" + lib + "\";";
                }   
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
                    Lexplosion.Runtime.AddImportantTask();

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
                        //StateChanged?.Invoke(OnlineGameStatus.None, "");
                    }
                    catch { }

                    GameStopEvent?.Invoke();

                    lock (removeImportantTaskLocker)
                    {
                        if (!removeImportantTaskMark)
                        {
                            removeImportantTaskMark = true;
                            Lexplosion.Runtime.RemoveImportantTask();
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

                    GameExited(_instanceId);
                };

                _processIsWork = process.Start();
                process.BeginOutputReadLine();

                // отслеживаем появление окна
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    while (_processIsWork)
                    {
                        Thread.Sleep(1000);

                        try
                        {
                            if (GuiIsExists(process.Id))
                            {
                                ComplitedLaunch(_instanceId, true);
                                GameStartedEvent?.Invoke();

                                gameVisible = true;
                                break;
                            }
                        }
                        catch
                        {
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

        public InitData Update(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, Action downloadStarted, string version = null, bool onlyBase = false)
        {
            IInstallManager instance;

            switch (_type)
            {
                case InstanceSource.Nightworld:
                    instance = new NightworldInstallManager(_instanceId, onlyBase, _updateCancelToken);
                    break;
                case InstanceSource.Local:
                    instance = new LocalInstallManager(_instanceId, _updateCancelToken);
                    break;
                case InstanceSource.Curseforge:
                    instance = new CurseforgeInstallManager(_instanceId, onlyBase, _updateCancelToken);
                    break;
                default:
                    instance = null;
                    break;
            }

            instance.FileDownloadEvent += fileDownloadHandler;
            instance.DownloadStarted += downloadStarted;

            InstanceInit result = instance.Check(out long releaseIndex, version);

            if (_updateCancelToken.IsCancellationRequested)
            {
                return new InitData
                {
                    InitResult = InstanceInit.IsCancelled
                };
            }

            if (result != InstanceInit.Successful)
            {
                return new InitData
                {
                    InitResult = result
                };
            }   

            if (_settings.CustomJava == false)
            {
                using (JavaChecker javaCheck = new JavaChecker(releaseIndex, _updateCancelToken))
                {
                    if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                    {
                        progressHandler?.Invoke(DownloadStageTypes.Java, new ProgressHandlerArguments()
                        {
                            StagesCount = 0,
                            Stage = 0,
                            Procents = 0,
                            FilesCount = 0,
                            TotalFilesCount = 1
                        });

                        bool downloadResult = javaCheck.Update(delegate (int percent, string fileName)
                        {
                            progressHandler?.Invoke(DownloadStageTypes.Java, new ProgressHandlerArguments()
                            {
                                StagesCount = 0,
                                Stage = 0,
                                Procents = percent,
                                FilesCount = 0,
                                TotalFilesCount = 1
                            });

                            fileDownloadHandler?.Invoke(fileName, percent, DownloadFileProgress.PercentagesChanged);
                        });

                        if (_updateCancelToken.IsCancellationRequested)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.IsCancelled
                            };
                        }

                        // TODO: намутить вызов удачного или неудачного fileDownloadHandler при окончании скачивнаия
                        if (!downloadResult)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.JavaDownloadError
                            };
                        }
                    }

                    if (_updateCancelToken.IsCancellationRequested)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.IsCancelled
                        };
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


        public InitData Initialization(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, Action downloadStarted)
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
                    return Update(progressHandler, fileDownloadHandler, downloadStarted, null, (_settings.AutoUpdate == false));
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
                StateChanged?.Invoke(OnlineGameStatus.None, "");
            }
            catch { }

            lock (removeImportantTaskLocker)
            {
                if (!removeImportantTaskMark)
                {
                    removeImportantTaskMark = true;
                    Lexplosion.Runtime.RemoveImportantTask();
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
