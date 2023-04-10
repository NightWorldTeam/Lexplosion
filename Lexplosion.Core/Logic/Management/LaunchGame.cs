using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System;
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
    public class LaunchGame
    {
        private Process _process = null;
        private Gateway _gameGateway = null;

        private string _instanceId;
        private Settings _settings;
        private InstanceSource _type;
        private string _javaPath = "";
        private bool _processIsWork;

        private static LaunchGame _classInstance = null;

        private bool _removeImportantTaskMark = true;
        private object _removeImportantTaskLocker = new object();

        public string GameVersion { get; private set; } = null;
        public string GameClientName { get; private set; } = "";

        private static object loocker = new object();

        private CancellationToken _updateCancelToken;

        private string _customJavaPath = null;

        #region events
        /// <summary>
        /// Выполняется при запуске процесса игры
        /// </summary>
        public static event Action<LaunchGame> OnGameProcessStarted;
        /// <summary>
        /// Выполняется после GameStartEvent, когда у майкнрафт появляется окно.
        /// </summary>
        public static event Action<LaunchGame> OnGameStarted;
        /// <summary>
        /// Выполняется при завершении процесса игры
        /// </summary>
        public static event Action<LaunchGame> OnGameStoped;

        private Action<string> _processDataReceived;

        /// <summary>
        /// Отрабатывает когда поялвяются данные из консоли майкрафта.
        /// </summary>
        public event Action<string> ProcessDataReceived
        {
            add
            {
                if (_processDataReceived == null && _process != null)
                {
                    _process.OutputDataReceived += ProcessDataHandle;
                }

                _processDataReceived += value;
            }
            remove
            {
                _processDataReceived -= value;

                if (_processDataReceived == null && _process != null)
                {
                    _process.OutputDataReceived -= ProcessDataHandle;
                }
            }
        }

        /// <summary>
        /// Отрабатывает когда к сетевой игре подключается игрок
        /// </summary>
        public static event Action<Player> UserConnected;
        /// <summary>
        /// Отрабатывает когда игрок отлючается от сетевой игры
        /// </summary>
        public static event Action<Player> UserDisconnected;
        /// <summary>
        /// Отрабатывает когда сетевая игра меняет свой статус
        /// </summary>
        public static event Action<OnlineGameStatus, string> StateChanged;

        #endregion

        public Settings ClientSettings
        {
            get => _settings;
        }

        public string InstanceId
        {
            get => _instanceId;
        }

        public LaunchGame(string instanceId, Settings instanceSettings, InstanceSource type, CancellationToken updateCancelToken)
        {
            if (_classInstance == null)
                _classInstance = this;

            // Сохраняем JavaPath для этой сборки. У настроек сборок нельзя установить Java17Path,
            // поэтому в JavaPath может храниться как новая версия джавы, так и старая.
            _customJavaPath = instanceSettings.JavaPath;
            instanceSettings.Merge(GlobalData.GeneralSettings, true);

            _settings = instanceSettings;
            _instanceId = instanceId;
            _type = type;

            _updateCancelToken = updateCancelToken;
        }

        private ConcurrentDictionary<string, Player> _connectedPlayers = new ConcurrentDictionary<string, Player>();

        private void ProcessDataHandle(object s, DataReceivedEventArgs e)
        {
            _processDataReceived?.Invoke(e.Data);
        }

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

            string accountType = GlobalData.User.AccountType.ToString();
            foreach (string lib in data.Libraries.Keys)
            {
                var activation = data.Libraries[lib].activationConditions;
                if ((activation?.accountTypes == null || activation.accountTypes.Contains(accountType)) && !data.Libraries[lib].notLaunch)
                {
                    command += "\"" + _settings.GamePath + "/libraries/" + lib + "\";";
                }
            }

            command += "\"" + versionPath + "\" ";

            string mainClass = data.VersionFile.mainClass;

            string additionalInstallerArgumentsBefore = "";
            string additionalInstallerArgumentsAfter = " ";

            var installer = data.VersionFile.additionalInstaller;
            if (installer != null)
            {
                if (!string.IsNullOrEmpty(installer.jvmArguments))
                {
                    additionalInstallerArgumentsBefore += installer.jvmArguments + " ";
                }

                if (!string.IsNullOrEmpty(installer.arguments))
                {
                    additionalInstallerArgumentsAfter += installer.arguments + " ";
                }

                mainClass = installer.mainClass;
            }

            command += additionalInstallerArgumentsBefore;

            string jvmArgs = data.VersionFile.jvmArguments ?? "";
            jvmArgs = jvmArgs.Replace("${version_file}", data.VersionFile.minecraftJar.name);
            jvmArgs = jvmArgs.Replace("${library_directory}", "\"" + _settings.GamePath + "/libraries\"");

            command += jvmArgs;
            command += @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
            command += " -Dhttp.agent=\"Mozilla/5.0\"";
            command += " -Xmx" + _settings.Xmx + "M -Xms" + _settings.Xms + "M " + _settings.GameArgs;
            command += mainClass + " --username " + GlobalData.User.Login + " --version " + data.VersionFile.gameVersion;
            command += " --gameDir \"" + _settings.GamePath + "/instances/" + _instanceId + "\"";
            command += " --assetsDir \"" + _settings.GamePath + "/assets" + "\"";
            command += " --assetIndex " + data.VersionFile.assetsVersion;
            command += " --uuid " + GlobalData.User.UUID + " --accessToken " + GlobalData.User.AccessToken + " --userProperties [] --userType legacy ";
            command += data.VersionFile.arguments;
            command += " --width " + _settings.WindowWidth + " --height " + _settings.WindowHeight;
            command += additionalInstallerArgumentsAfter;

            //TODO: сделать функционал для автоматического коннекта - command += "--server 192.168.1.114 --port 55538";

            return command.Replace('\\', '/');
        }

        public bool Run(InitData data, LaunchComplitedCallback ComplitedLaunch, GameExitedCallback GameExited, string gameClientName, bool onlineGame)
        {
            GameClientName = gameClientName;
            GameVersion = data?.VersionFile?.gameVersion;

            string command = CreateCommand(data);

            _process = new Process();
            if (onlineGame)
            {
                lock (loocker)
                {
                    _gameGateway = new Gateway(GlobalData.User.UUID, GlobalData.User.SessionToken, LaunсherSettings.ServerIp, GlobalData.GeneralSettings.NetworkDirectConnection);
                    _removeImportantTaskMark = false;
                    Lexplosion.Runtime.AddImportantTask();

                    _gameGateway.ConnectingUser += delegate (string uuid)
                    {
                        var player = new Player(uuid,
                            delegate
                            {
                                this._gameGateway?.KickClient(uuid);
                            },
                            delegate
                            {
                                this._gameGateway?.UnkickClient(uuid);
                            }
                        );

                        _connectedPlayers[uuid] = player;
                        UserConnected?.Invoke(player);
                    };

                    _gameGateway.DisconnectedUser += delegate (string uuid)
                    {
                        _connectedPlayers.TryRemove(uuid, out Player player);
                        UserDisconnected?.Invoke(player);
                    };

                    _gameGateway.StatusChanged += StateChanged;
                }
            }

            OnGameProcessStarted?.Invoke(this);

            _processDataReceived?.Invoke("Выполняется запуск игры...");
            _processDataReceived?.Invoke(command);

            bool gameVisible = false;

            try
            {
                _process.StartInfo.FileName = _javaPath;
                _process.StartInfo.WorkingDirectory = _settings.GamePath + "/instances/" + _instanceId;
                _process.StartInfo.Arguments = command;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.UseShellExecute = false;
                _process.EnableRaisingEvents = true;

                _process.Exited += (sender, ea) =>
                {
                    _processIsWork = false;

                    try
                    {
                        _process.Dispose();
                    }
                    catch { }

                    try
                    {
                        lock (loocker)
                        {
                            _gameGateway?.StopWork();
                            _gameGateway = null;
                        }
                        //StateChanged?.Invoke(OnlineGameStatus.None, "");
                    }
                    catch { }

                    OnGameStoped?.Invoke(this);

                    lock (_removeImportantTaskLocker)
                    {
                        if (!_removeImportantTaskMark)
                        {
                            _removeImportantTaskMark = true;
                            Lexplosion.Runtime.RemoveImportantTask();
                        }
                    }

                    if (!gameVisible)
                    {
                        ComplitedLaunch(_instanceId, false);
                    }

                    _classInstance = null;
                    GameExited(_instanceId);
                };

                _processIsWork = _process.Start();
                _process.BeginOutputReadLine();

                // отслеживаем появление окна
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    while (_processIsWork)
                    {
                        Thread.Sleep(1000);

                        try
                        {
                            if (GuiIsExists(_process.Id))
                            {
                                ComplitedLaunch(_instanceId, true);
                                OnGameStarted?.Invoke(this);

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
                    _gameGateway?.Initialization(_process.Id);
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
                case InstanceSource.Modrinth:
                    instance = new ModrinthInstallManager(_instanceId, onlyBase, _updateCancelToken);
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

            bool javaIsNotDefined = true;
            if (_settings.IsCustomJava == true)
            {
                if (!string.IsNullOrEmpty(_customJavaPath))
                {
                    // _customJavaPath хранит путь до джавы конкретно для этой сборки. По этому если джава кастомная, то юзаем этот путь.
                    // Если не использовать _customJavaPath, то для новых версий мы можем выбрать Java17Path, даже если в настройках
                    // сборки прописана конретная версия джавы, а это нам не надо.
                    _javaPath = _customJavaPath;
                    javaIsNotDefined = false;
                }
                else
                {
                    string javaPath = JavaChecker.DefinePath(_settings.JavaPath, _settings.Java17Path, releaseIndex);
                    if (!string.IsNullOrEmpty(javaPath))
                    {
                        _javaPath = javaPath;
                        javaIsNotDefined = false;
                    }
                }
            }

            if (javaIsNotDefined)
            {
                using (JavaChecker javaCheck = new JavaChecker(releaseIndex, _updateCancelToken))
                {
                    if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
                    {
                        progressHandler?.Invoke(StageType.Java, new ProgressHandlerArguments()
                        {
                            StagesCount = 0,
                            Stage = 0,
                            Procents = 0,
                            FilesCount = 0,
                            TotalFilesCount = 1
                        });

                        bool downloadResult = javaCheck.Update(delegate (int percent, string fileName)
                        {
                            progressHandler?.Invoke(StageType.Java, new ProgressHandlerArguments()
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

            return instance.Update(_javaPath, progressHandler);
        }

        public InitData Initialization(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, Action downloadStarted)
        {
            try
            {
                WithDirectory.Create(_settings.GamePath);
                InitData data = null;
                // Было измененно с !_settings.GamePath.Contains(":") - -на)--> !(_settings.GamePath.IndexOf(':') >= 0) 
                if (!Directory.Exists(_settings.GamePath) || !(_settings.GamePath.IndexOf(':') >= 0))
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.GamePathError
                    };
                }

                VersionManifest files = DataFilesManager.GetManifest(_instanceId, true);
                bool versionIsStatic = files?.version?.isStatic == true;

                if (!versionIsStatic && ToServer.ServerIsOnline())
                {
                    data = Update(progressHandler, fileDownloadHandler, downloadStarted, null, (_settings.IsAutoUpdate == false));
                }
                else
                {
                    if (files?.version != null && files.libraries != null)
                    {
                        bool javaIsNotDefined = true;
                        if (_settings.IsCustomJava == true)
                        {
                            if (!string.IsNullOrEmpty(_customJavaPath))
                            {
                                _javaPath = _customJavaPath;
                                javaIsNotDefined = false;
                            }
                            else
                            {
                                string javaPath = JavaChecker.DefinePath(_settings.JavaPath, _settings.Java17Path, files.version.releaseIndex);
                                if (!string.IsNullOrEmpty(javaPath))
                                {
                                    _javaPath = javaPath;
                                    javaIsNotDefined = false;
                                }
                            }
                        }

                        if (javaIsNotDefined)
                        {
                            using (JavaChecker javaCheck = new JavaChecker(files.version.releaseIndex, _updateCancelToken, true))
                            {
                                JavaVersion javaInfo = javaCheck.GetJavaInfo();
                                if (javaInfo?.JavaName == null || javaInfo.ExecutableFile == null)
                                {
                                    return new InitData
                                    {
                                        InitResult = InstanceInit.JavaDownloadError
                                    };
                                }

                                _javaPath = WithDirectory.DirectoryPath + "/java/" + javaInfo.JavaName + javaInfo.ExecutableFile;
                            }
                        }

                        data = new InitData
                        {
                            VersionFile = files.version,
                            Libraries = files.libraries,
                            UpdatesAvailable = false,
                            InitResult = InstanceInit.Successful
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
            catch
            {
                return new InitData
                {
                    InitResult = InstanceInit.UnknownError
                };
            }
        }

        public void Stop()
        {
            _classInstance = null;
            _processIsWork = false;

            OnGameStoped?.Invoke(this);

            try
            {
                _process.Kill(); // TODO: тут иногда крашится (ввроде если ошибка скачивания была)
                _process.Dispose();
            }
            catch { }

            try
            {
                lock (loocker)
                {
                    _gameGateway?.StopWork();
                    _gameGateway = null;
                }
                StateChanged?.Invoke(OnlineGameStatus.None, "");
            }
            catch { }

            lock (_removeImportantTaskLocker)
            {
                if (!_removeImportantTaskMark)
                {
                    _removeImportantTaskMark = true;
                    Lexplosion.Runtime.RemoveImportantTask();
                }
            }
        }

        public static void RebootOnlineGame()
        {
            if (_classInstance != null)
            {
                lock (loocker)
                {
                    if (_classInstance._gameGateway != null)
                    {
                        try
                        {
                            _classInstance._gameGateway.StopWork();
                        }
                        catch { }

                        _classInstance._gameGateway = new Gateway(GlobalData.User.UUID, GlobalData.User.SessionToken, LaunсherSettings.ServerIp, GlobalData.GeneralSettings.NetworkDirectConnection);
                        _classInstance._gameGateway.Initialization(_classInstance._process.Id);
                    }
                }
                StateChanged?.Invoke(OnlineGameStatus.None, "");
            }

        }
    }
}
