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
using Lexplosion.Logic.Management.Sources;

namespace Lexplosion.Logic.Management
{
    public class LaunchGame
    {
        private Process _process = null;
        private OnlineGameGateway _gameGateway = null;

        private string _instanceId;
        private Settings _settings;
        private IInstanceSource _source;
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
        /// Выполняется после OnGameProcessStarted, когда у майкнрафт появляется окно.
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

        public LaunchGame(string instanceId, Settings instanceSettings, IInstanceSource source, CancellationToken updateCancelToken)
        {
            if (_classInstance == null)
                _classInstance = this;

            // Сохраняем JavaPath для этой сборки. У настроек сборок нельзя установить Java17Path,
            // поэтому в JavaPath может храниться как новая версия джавы, так и старая.
            _customJavaPath = instanceSettings.JavaPath;
            instanceSettings.Merge(GlobalData.GeneralSettings, true);

            _settings = instanceSettings;
            _instanceId = instanceId;
            _source = source;

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

        private string ParseCommandArgument(MinecraftArgument argument)
        {
            if (argument == null) return "";

            string strValue = argument.GetSting;
            if (strValue != null) return strValue;

            MinecraftArgumentObject obj = argument.GetObject;
            if (obj?.Value == null || obj.Value.Length < 1) return "";
            if (obj.Rules == null || obj.Rules.Count < 1) return string.Join(" ", obj.Value);

            bool isAllowed = false;
            foreach (MinecraftArgumentObject.Rule rule in obj.Rules)
            {
                if (rule == null) continue;
                if (rule.Action != MinecraftArgumentObject.Rule.Access.Allow) continue;

                // если есть правило OS и она windows, то проверяем еще Arch и Version
                if (rule.Os?.Name == "windows")
                {
                    bool isX64System = Environment.Is64BitOperatingSystem;

                    //если есть правило битности системы
                    if (rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x64 || rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x32 || rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x86)
                    {
                        if ((isX64System && rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x64) || (!isX64System && rule.Os.Arch != MinecraftArgumentObject.Rule.OS.SysArch.x64))
                        {
                            //бидность ситемы подходит
                            isAllowed = true;
                            break;
                        }
                        else
                        {
                            // не подоходит
                            continue;
                        }
                    }

                    // если есть правило версии системы
                    if (rule.Os.Version != null)
                    {
                        // TODO: когда-нибудь реализовать проверку версии os
                        continue;
                    }

                    //если дошли до сюда, то значит правила Arch и Version нету, а OS у нас подходит
                    isAllowed = true;
                    break;
                }

                //если есть правило битности системы
                if (rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x64 || rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x32 || rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x86)
                {
                    bool isX64System = Environment.Is64BitOperatingSystem;

                    if ((isX64System && rule.Os.Arch == MinecraftArgumentObject.Rule.OS.SysArch.x64) || (!isX64System && rule.Os.Arch != MinecraftArgumentObject.Rule.OS.SysArch.x64))
                    {
                        //бидность ситемы подходит
                        isAllowed = true;
                        break;
                    }
                    else
                    {
                        // не подоходит
                        continue;
                    }
                }
            }

            if (isAllowed) return string.Join(" ", obj.Value);

            return "";
        }

        private string CreateCommand(InitData data)
        {
            string gamePath = _settings.GamePath.Replace('\\', '/') + "/";
            gamePath = gamePath.Replace("//", "/");
            string versionPath = gamePath + "instances/" + _instanceId + "/version/" + data.VersionFile.MinecraftJar.name;

            if (_settings.GameArgs.Length > 0 && _settings.GameArgs[_settings.GameArgs.Length - 1] != ' ')
                _settings.GameArgs += " ";

            string accountType = GlobalData.User.AccountType.ToString();
            string libs = "";
            foreach (string lib in data.Libraries.Keys)
            {
                var activation = data.Libraries[lib].activationConditions;


                bool byAccountType = (activation?.accountTypes == null || activation.accountTypes.Contains(accountType));
                bool byNwClient = activation?.nightWorldClient == null || (activation.nightWorldClient == data.VersionFile.IsNightWorldClient);
                bool byModloader = (activation?.clientTypes == null || activation.clientTypes.Contains(data.VersionFile.ModloaderType.ToString()));

                if (byAccountType && byNwClient && byModloader && !data.Libraries[lib].notLaunch)
                {
                    libs += "\"" + gamePath + "libraries/" + lib + "\";";
                }
            }

            libs += "\"" + versionPath + "\" ";

            string mainClass = data.VersionFile.MainClass;

            string additionalInstallerArgumentsBefore = "";
            string additionalInstallerArgumentsAfter = " ";

            var installer = data.VersionFile.AdditionalInstaller;
            if (installer != null)
            {
                if (!string.IsNullOrWhiteSpace(installer.jvmArguments))
                {
                    additionalInstallerArgumentsBefore += installer.jvmArguments + " ";
                }

                if (!string.IsNullOrWhiteSpace(installer.arguments))
                {
                    additionalInstallerArgumentsAfter += installer.arguments + " ";
                }

                mainClass = installer.mainClass;
            }

            string jvmArgs = data.VersionFile.JvmArguments ?? "";
            jvmArgs = jvmArgs.Replace("${version_file}", data.VersionFile.MinecraftJar.name);
            jvmArgs = jvmArgs.Replace("${library_directory}", gamePath + "libraries");

            string command;
            if (data.VersionFile.DefaultArguments != null)
            {
                command = "";
                foreach (MinecraftArgument arg in data.VersionFile.DefaultArguments.Jvm)
                {
                    string param = ParseCommandArgument(arg);
                    if (!string.IsNullOrWhiteSpace(param))
                    {
                        command += " " + param;
                    }
                }

                command += additionalInstallerArgumentsBefore;
                command += jvmArgs;
                command += @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
                command += " -Dhttp.agent=\"Mozilla/5.0\"";
                command += " -Djava.net.preferIPv4Stack=true";
                command += " -Xmx" + _settings.Xmx + "M -Xms" + _settings.Xms + "M " + _settings.GameArgs;
                command += mainClass + " ";

                foreach (MinecraftArgument arg in data.VersionFile.DefaultArguments.Game)
                {
                    string param = ParseCommandArgument(arg);
                    if (!string.IsNullOrWhiteSpace(param))
                    {
                        command += " " + param;
                    }
                }

                command += " " + data.VersionFile.Arguments;
                command += " --width " + _settings.WindowWidth + " --height " + _settings.WindowHeight;
                command += additionalInstallerArgumentsAfter;

                command = command.Replace("${auth_player_name}", GlobalData.User.Login);
                command = command.Replace("${version_name}", data.VersionFile.GameVersion);
                command = command.Replace("${game_directory}", "\"" + gamePath + "instances/" + _instanceId + "\"");
                command = command.Replace("${assets_root}", "\"" + gamePath + "assets" + "\"");
                command = command.Replace("${assets_index_name}", data.VersionFile.AssetsVersion);
                command = command.Replace("${auth_uuid}", GlobalData.User.UUID);
                command = command.Replace("${auth_access_token}", GlobalData.User.AccessToken);
                command = command.Replace("${user_type}", "legacy");
                command = command.Replace("${version_type}", "release");
                command = command.Replace("${natives_directory}", "\"" + gamePath + "natives/" + (data.VersionFile.CustomVersionName ?? data.VersionFile.GameVersion) + "\"");
                command = command.Replace("${launcher_name}", "nw-lexplosion");
                command = command.Replace("${launcher_version}", "0.7.9");
                command = command.Replace("${classpath}", " " + libs);
            }
            else
            {
                command = " -Djava.library.path=\"" + gamePath + "natives/" + (data.VersionFile.CustomVersionName ?? data.VersionFile.GameVersion) + "\" -cp ";
                command += libs;
                command += additionalInstallerArgumentsBefore;

                command += jvmArgs;
                command += @" -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90";
                command += " -Dhttp.agent=\"Mozilla/5.0\"";
                command += " -Djava.net.preferIPv4Stack=true";
                command += " -Xmx" + _settings.Xmx + "M -Xms" + _settings.Xms + "M " + _settings.GameArgs;
                command += mainClass + " --username " + GlobalData.User.Login + " --version " + data.VersionFile.GameVersion;
                command += " --gameDir \"" + gamePath + "instances/" + _instanceId + "\"";
                command += " --assetsDir \"" + gamePath + "assets" + "\"";
                command += " --assetIndex " + data.VersionFile.AssetsVersion;
                command += " --uuid " + GlobalData.User.UUID + " --accessToken " + GlobalData.User.AccessToken + " --userProperties [] --userType legacy ";
                command += data.VersionFile.Arguments;
                command += " --width " + _settings.WindowWidth + " --height " + _settings.WindowHeight;
                command += additionalInstallerArgumentsAfter;
            }

            //TODO: сделать функционал для автоматического коннекта - command += "--server 192.168.1.114 --port 55538";

            return command;
        }

        public bool Run(InitData data, LaunchComplitedCallback ComplitedLaunch, GameExitedCallback GameExited, string gameClientName, bool onlineGame)
        {
            GameClientName = gameClientName;
            GameVersion = data?.VersionFile?.GameVersion;

            string command = CreateCommand(data);

            _process = new Process();

            if (onlineGame)
            {
                lock (loocker)
                {
                    var serverData = new ControlServerData(LaunсherSettings.ServerIp);
                    _gameGateway = new OnlineGameGateway(GlobalData.User.UUID, GlobalData.User.SessionToken, serverData, GlobalData.GeneralSettings.NetworkDirectConnection);

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

            Runtime.DebugWrite("Run javaPath " + _javaPath);

            bool gameVisible = false;

            try
            {
                _process.StartInfo.FileName = _javaPath;
                _process.StartInfo.WorkingDirectory = _settings.GamePath + "/instances/" + _instanceId;
                _process.StartInfo.Arguments = command;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.EnableRaisingEvents = true;
                _process.StartInfo.EnvironmentVariables["_JAVA_OPTIONS"] = "";
                _process.StartInfo.UseShellExecute = false;

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
            catch (Exception ex)
            {
                _processIsWork = false;
                ComplitedLaunch(_instanceId, false);
                Runtime.DebugWrite(ex);

                return false;
            }
        }

        public InitData Update(ProgressHandlerCallback progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, Action downloadStarted, string version = null, bool onlyBase = false)
        {
            IInstallManager instance = _source.GetInstaller(_instanceId, onlyBase, _updateCancelToken);

            instance.FileDownloadEvent += fileDownloadHandler;
            instance.DownloadStarted += downloadStarted;

            InstanceInit result = instance.Check(out string javaVersionName, version);

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
            if (_settings.IsCustomJava == true || _settings.IsCustomJava17 == true)
            {
                if (!string.IsNullOrWhiteSpace(_customJavaPath))
                {
                    // _customJavaPath хранит путь до джавы конкретно для этой сборки. По этому если джава кастомная, то юзаем этот путь.
                    // Если не использовать _customJavaPath, то для новых версий мы можем выбрать Java17Path, даже если в настройках
                    // сборки прописана конретная версия джавы, а это нам не надо.
                    _javaPath = _customJavaPath;
                    javaIsNotDefined = false;
                }
                else
                {
                    string javaPath = JavaChecker.DefinePath(_settings.JavaPath, _settings.Java17Path, javaVersionName);
                    if (!string.IsNullOrWhiteSpace(javaPath))
                    {
                        _javaPath = javaPath;
                        javaIsNotDefined = false;
                    }
                }
            }

            if (javaIsNotDefined)
            {
                using (JavaChecker javaCheck = new JavaChecker(javaVersionName, _updateCancelToken))
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

                        bool downloadResult = javaCheck.Update(delegate (int percent, int file, int filesCount, string fileName)
                        {
                            progressHandler?.Invoke(StageType.Java, new ProgressHandlerArguments()
                            {
                                StagesCount = 0,
                                Stage = 0,
                                Procents = percent,
                                FilesCount = file,
                                TotalFilesCount = filesCount
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
                        _javaPath = WithDirectory.DirectoryPath + "/java/versions/" + javaVersion.JavaName + javaVersion.ExecutableFile;
                        Runtime.DebugWrite("JavaPath " + _javaPath);
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
                bool pathIsExists = Directory.Exists(_settings.GamePath);
                if (!pathIsExists || !(_settings.GamePath.IndexOf(':') >= 0))
                {
                    Runtime.DebugWrite("GamePathError. Path: " + _settings.GamePath + ", is exists " + pathIsExists);
                    return new InitData
                    {
                        InitResult = InstanceInit.GamePathError
                    };
                }

                VersionManifest files = DataFilesManager.GetManifest(_instanceId, true);
                bool versionIsStatic = files?.version?.IsStatic == true;

                if (!versionIsStatic && ToServer.ServerIsOnline())
                {
                    data = Update(progressHandler, fileDownloadHandler, downloadStarted, null, (_settings.IsAutoUpdate == false));
                }
                else
                {
                    if (files?.version != null && files.libraries != null)
                    {
                        bool javaIsNotDefined = true;
                        if (_settings.IsCustomJava == true || _settings.IsCustomJava17 == true)
                        {
                            if (!string.IsNullOrWhiteSpace(_customJavaPath))
                            {
                                _javaPath = _customJavaPath;
                                javaIsNotDefined = false;
                            }
                            else
                            {
                                string javaPath = JavaChecker.DefinePath(_settings.JavaPath, _settings.Java17Path, files.version.JavaVersionName);
                                if (!string.IsNullOrWhiteSpace(javaPath))
                                {
                                    _javaPath = javaPath;
                                    javaIsNotDefined = false;
                                }
                            }
                        }

                        if (javaIsNotDefined)
                        {
                            using (JavaChecker javaCheck = new JavaChecker(files.version.JavaVersionName, _updateCancelToken, true))
                            {
                                JavaVersion javaInfo = javaCheck.GetJavaInfo();
                                if (javaInfo?.JavaName == null || javaInfo.ExecutableFile == null)
                                {
                                    return new InitData
                                    {
                                        InitResult = InstanceInit.JavaDownloadError
                                    };
                                }

                                _javaPath = WithDirectory.DirectoryPath + "/java/versions/" + javaInfo.JavaName + javaInfo.ExecutableFile;
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
                        Runtime.DebugWrite("Minifest error " + (files?.version != null) + " " + (files.libraries != null));
                        return new InitData
                        {
                            InitResult = InstanceInit.ManifestError
                        };
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("UnknownError. Exception " + ex);
                return new InitData
                {
                    InitResult = InstanceInit.UnknownError
                };
            }
        }

        public void DeleteCancellationToken()
        {
            _updateCancelToken = new CancellationToken();
        }

        public void Stop()
        {
            _classInstance = null;
            _processIsWork = false;

            OnGameStoped?.Invoke(this);

            try
            {
                _process?.Kill(); // TODO: тут иногда крашится (ввроде если ошибка скачивания была)
                _process?.Dispose();
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

                        var serverData = new ControlServerData(LaunсherSettings.ServerIp);
                        _classInstance._gameGateway = new OnlineGameGateway(GlobalData.User.UUID, GlobalData.User.SessionToken, serverData, GlobalData.GeneralSettings.NetworkDirectConnection);
                        _classInstance._gameGateway.Initialization(_classInstance._process.Id);
                    }
                }
                StateChanged?.Invoke(OnlineGameStatus.None, "");
            }

        }
    }
}
