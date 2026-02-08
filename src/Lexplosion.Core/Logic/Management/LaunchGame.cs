using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Sources;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.FileSystem.Services;
using static Lexplosion.Logic.Objects.CommonClientData.NightWorldClientData;

namespace Lexplosion.Logic.Management
{
	public class LaunchGame
	{
		private Process _process = null;
		private OnlineGameGateway _gameGateway = null;

		private string _instanceId;
		private Settings _settings;
		private IInstanceSource _source;
		private readonly INightWorldFileServicesContainer _services;
		private readonly WithDirectory _withDirectory;
		private readonly DataFilesManager _dataFilesManager;
		private string _javaPath = string.Empty;
		private bool _processIsWork;

		private static LaunchGame _classInstance = null;

		private bool _onlineGameStopedMark = true;
		private object _onlineGameStpedEventLocker = new object();

		public string GameVersion { get; private set; } = null;
		public string GameClientName { get; private set; } = string.Empty;

		private static object loocker = new object();

		private CancellationToken _updateCancelToken;

		private string _customJavaPath = null;
		private string _keyStorePath;

		private Account _activeAccount;
		private Account _launchAccount;

		private Settings _generalSettings;

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
					_process.ErrorDataReceived += ProcessDataHandle;
				}

				_processDataReceived += value;
			}
			remove
			{
				_processDataReceived -= value;

				if (_processDataReceived == null && _process != null)
				{
					_process.OutputDataReceived -= ProcessDataHandle;
					_process.ErrorDataReceived -= ProcessDataHandle;
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
		/// <summary>
		/// Отрабатывает когда запускается сетевая игра
		/// </summary>
		public static event Action OnlineGameSystemStarted;
		/// <summary>
		/// Отрабатывает когда завершается сетевая игра
		/// </summary>
		public static event Action OnlineGameSystemStoped;

		#endregion

		public Settings ClientSettings
		{
			get => _settings;
		}

		public string InstanceId
		{
			get => _instanceId;
		}

		public LaunchGame(string instanceId, Settings generalSettings, Settings instanceSettings, Account activeAccount, Account launchAccount, IInstanceSource source, INightWorldFileServicesContainer services, CancellationToken updateCancelToken)
		{
			if (_classInstance == null)
				_classInstance = this;

			_generalSettings = generalSettings;
			_activeAccount = activeAccount;
			_launchAccount = launchAccount;

			// Сохраняем JavaPath для этой сборки. У настроек сборок нельзя установить Java17Path,
			// поэтому в JavaPath может храниться как новая версия джавы, так и старая.
			_customJavaPath = instanceSettings.JavaPath;
			instanceSettings.Merge(_generalSettings, true);

			_settings = instanceSettings;
			_instanceId = instanceId;
			_source = source;
			_services = services;
			_updateCancelToken = updateCancelToken;

			_withDirectory = services.DirectoryService;
			_dataFilesManager = services.DataFilesService;
		}

		private ConcurrentDictionary<string, Player> _connectedPlayers = new ConcurrentDictionary<string, Player>();

		private void ProcessDataHandle(object s, DataReceivedEventArgs e)
		{
			_processDataReceived?.Invoke(e.Data);
		}

		private bool GuiIsExists(int processId)
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
			if (argument == null) return string.Empty;

			string strValue = argument.GetSting;
			if (strValue != null) return strValue;

			MinecraftArgumentObject obj = argument.GetObject;
			if (obj?.Value == null || obj.Value.Length < 1) return string.Empty;
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
							//битность ситемы подходит
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

			return string.Empty;
		}

		private bool CheckActivationConditions(ActivationConditions activation, bool isNwClient, bool isNwSkinSystem, string accountType, string modloaderType)
		{
			if (activation == null) return true;
			bool byAccountType = (activation?.accountTypes == null || activation.accountTypes.Contains(accountType));
			bool byNwClient = (activation?.nightWorldClient == null || activation.nightWorldClient == isNwClient);
			bool byClientType = (activation?.clientTypes == null || activation.clientTypes.Contains(modloaderType));
			bool bySkinSystem = activation?.nightWorldSkinSystem == null || (activation.nightWorldSkinSystem == isNwSkinSystem);

			return byAccountType && byNwClient && byClientType && bySkinSystem;
		}

		private string CreateCommand(InitData data)
		{
			var builder = new RunCommandBuilder();

			string gamePath = _settings.GamePath.Replace('\\', '/') + "/";
			gamePath = gamePath.Replace("//", "/");
			string versionPath = gamePath + "instances/" + _instanceId + "/version/" + data.VersionFile.MinecraftJar.name;

			bool isNwClient = (data.VersionFile?.NightWorldClientData != null) && data.VersionFile.IsNightWorldClient && _launchAccount.AccountType == AccountType.NightWorld;
			bool isNwSkinSystem = _launchAccount.AccountType == AccountType.NightWorld && _settings.IsNightWorldSkinSystem != false;
			string modloaderType = data.VersionFile.ModloaderType.ToString();
			string accountType = _launchAccount.AccountType.ToString();

			string libs = string.Empty;
			foreach (string lib in data.Libraries.Keys)
			{
				var activation = data.Libraries[lib].activationConditions;

				bool isActivated = CheckActivationConditions(activation, isNwClient, isNwSkinSystem, accountType, modloaderType);
				if (isActivated && !data.Libraries[lib].notLaunch)
				{
					libs += "\"" + gamePath + "libraries/" + lib + "\";";
				}
			}

			libs += "\"" + versionPath + "\" ";

			string mainClass = data.VersionFile.MainClass;

			var installer = data.VersionFile.AdditionalInstaller;
			if (installer != null)
			{
				if (!string.IsNullOrWhiteSpace(installer.jvmArguments))
				{
					builder.AddJvmArgs(installer.jvmArguments);
				}

				if (!string.IsNullOrWhiteSpace(installer.arguments))
				{
					builder.AddGameArgs(installer.arguments);
				}

				mainClass = installer.mainClass;
			}

			if (!string.IsNullOrWhiteSpace(_settings.AutoLoginServer))
			{
				if (_settings.AutoLoginServer.Contains(":"))
				{
					string[] parts = _settings.AutoLoginServer.Split(':');
					string ip = parts[0];
					string port = parts[1];

					builder.AddGameArgs($"--server \"{ip}\" --port \"{port}\" --quickPlayMultiplayer \"{_settings.AutoLoginServer}\"");
				}
				else
				{
					builder.AddGameArgs($"--server \"{_settings.AutoLoginServer}\" --quickPlayMultiplayer \"{_settings.AutoLoginServer}\"");
				}
			}

			builder.AddGameArgs(_settings.GameArgs);

			if ((isNwClient && !string.IsNullOrWhiteSpace(data.VersionFile.NightWorldClientData.MainClass)))
				mainClass = data.VersionFile.NightWorldClientData.MainClass;

			if (data.VersionFile.AdditionaArguments != null)
			{
				foreach (var argument in data.VersionFile.AdditionaArguments)
				{
					if (!CheckActivationConditions(argument.ActivationConditions, isNwClient, isNwSkinSystem, accountType, modloaderType)) continue;

					if (argument.Type == AdditionaMinecraftArgument.ArgumentType.Jvm)
					{
						builder.AddJvmArgs(argument.Value);
					}
					else if (argument.Type == AdditionaMinecraftArgument.ArgumentType.Game)
					{
						builder.AddGameArgs(argument.Value);
					}
				}
			}

			NightWorldClientData.ComlexArgument[] nwClientComplexArguments = null;
			if (isNwClient)
			{
				var nwClientData = data.VersionFile.NightWorldClientData;
				NightWorldClientData.Arguments arguments = nwClientData.GetByClientType(data.VersionFile.ModloaderType);
				if (arguments != null)
				{
					builder.AddGameArgs(arguments.Minecraft);
					builder.AddJvmArgs(arguments.Jvm);
					nwClientComplexArguments = arguments.Complex;
				}
			}

			string command;
			if (data.VersionFile.BasicArguments != null)
			{
				foreach (MinecraftArgument arg in data.VersionFile.BasicArguments.Jvm)
				{
					string param = ParseCommandArgument(arg);
					if (!string.IsNullOrWhiteSpace(param))
					{
						builder.AddJvmArgs(param);
					}
				}

				if (_keyStorePath != null)
				{
					builder.AddJvmArgs("-Djavax.net.ssl.trustStore=\"" + _keyStorePath + "\"");
				}

				builder.AddJvmArgs(_settings.JVMArgs);
				builder.AddJvmArgs(@"-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90");
				builder.AddJvmArgs("-Dhttp.agent=\"Mozilla/5.0\"");
				builder.AddJvmArgs("-Djava.net.preferIPv4Stack=true");
				builder.AddJvmArgs($"-Xmx{_settings.Xmx}M -Xms{_settings.Xms}M");

				foreach (MinecraftArgument arg in data.VersionFile.BasicArguments.Game)
				{
					string param = ParseCommandArgument(arg);
					if (!string.IsNullOrWhiteSpace(param))
					{
						builder.AddGameArgs(param);
					}
				}

				builder.AddGameArgs("--width " + _settings.WindowWidth + " --height " + _settings.WindowHeight);

				command = builder.Build(mainClass);
				command = command.Replace("${auth_player_name}", _launchAccount.Login);
				command = command.Replace("${version_name}", data.VersionFile.GameVersion);
				command = command.Replace("${game_directory}", "\"" + gamePath + "instances/" + _instanceId + "\"");
				command = command.Replace("${assets_root}", "\"" + gamePath + "assets" + "\"");
				command = command.Replace("${assets_index_name}", data.VersionFile.AssetsVersion);
				command = command.Replace("${auth_uuid}", _launchAccount.UUID);
				command = command.Replace("${auth_access_token}", _launchAccount.AccessToken);
				command = command.Replace("${user_type}", "mojang");
				command = command.Replace("${version_type}", "release");
				command = command.Replace("${natives_directory}", "\"" + gamePath + "natives/" + (data.VersionFile.CustomVersionName ?? data.VersionFile.GameVersion) + "\"");
				command = command.Replace("${launcher_name}", "nw-lexplosion");
				command = command.Replace("${launcher_version}", "1.0.1.1");
				command = command.Replace("${classpath}", libs);
			}
			else
			{
				builder.AddJvmArgs($"-Djava.library.path=\"{gamePath}natives/{(data.VersionFile.CustomVersionName ?? data.VersionFile.GameVersion)}\" -cp {libs}");
				builder.AddJvmArgs(_settings.JVMArgs);

				if (_keyStorePath != null)
				{
					builder.AddJvmArgs("-Djavax.net.ssl.trustStore=\"" + _keyStorePath + "\"");
				}

				builder.AddJvmArgs(@"-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true -XX:TargetSurvivorRatio=90");
				builder.AddJvmArgs("-Dhttp.agent=\"Mozilla/5.0\"");
				builder.AddJvmArgs("-Djava.net.preferIPv4Stack=true");
				builder.AddJvmArgs("-Xmx" + _settings.Xmx + "M -Xms" + _settings.Xms + "M");
				builder.AddGameArgs("--username " + _launchAccount.Login + " --version " + data.VersionFile.GameVersion);
				builder.AddGameArgs("--gameDir \"" + gamePath + "instances/" + _instanceId + "\"");
				builder.AddGameArgs("--assetsDir \"" + gamePath + "assets" + "\"");
				builder.AddGameArgs("--assetIndex " + data.VersionFile.AssetsVersion);
				builder.AddGameArgs("--uuid " + _launchAccount.UUID + " --accessToken " + _launchAccount.AccessToken + " --userProperties [] --userType mojang");
				builder.AddGameArgs("--width " + _settings.WindowWidth + " --height " + _settings.WindowHeight);

				command = builder.Build(mainClass);
			}

			if (nwClientComplexArguments != null)
			{
				foreach (var arg in nwClientComplexArguments)
				{
					if (arg?.InsertValue == null || arg.InsertPlace == null) continue;

					if (arg.InsertType == NightWorldClientData.ComplexArgumentType.After)
					{
						command = command.Replace(arg.InsertPlace, arg.InsertPlace + arg.InsertValue);
					}
					else if (arg.InsertType == NightWorldClientData.ComplexArgumentType.Before)
					{
						command = command.Replace(arg.InsertPlace, arg.InsertValue + arg.InsertPlace);
					}
				}
			}

			command = command.Replace("${version_file}", data.VersionFile.MinecraftJar.name);
			command = command.Replace("${library_directory}", gamePath + "libraries");
			command = command.Replace("${mainClass}", mainClass);
			command = command.Replace("${appearanceElementsDir}", gamePath + "appearanceElements");
			command = command.Replace("${mainClass}", mainClass);
			command = command.Replace("${appearanceElementsDir}", gamePath + "appearanceElements");

			return command;
		}

		private void CreateJavaKeyStore(string javaPath)
		{
			try
			{
				javaPath = javaPath.Replace("//", "/").Replace("/", "\\");

				javaPath = Path.GetDirectoryName(javaPath);
				if (javaPath.EndsWith("bin"))
				{
					javaPath = Path.GetDirectoryName(javaPath);
				}

				string keyStoreRoot = _settings.GamePath + "/java/keystore/";
				string keyStorePath = keyStoreRoot + Cryptography.Sha256(javaPath);
				string keyStoreFile = (keyStorePath + "/cacerts");
				keyStoreFile = keyStoreFile.Replace("//", "/");

				string mainCertFile = _settings.GamePath + "/java/keystore/night-world.org.crt";
				string mirrorCertFile = _settings.GamePath + "/java/keystore/(v1)mirror.night-world.org.crt";
				Runtime.DebugWrite("mainCertFile: " + mainCertFile);
				Runtime.DebugWrite("mirrorCertFile: " + mirrorCertFile);

				bool keyStoreToUpdate = false;
				byte[] mainCertificate = _services.WebService.LoadCertificate(Global.LaunсherSettings.URL.Base);
				byte[] mirrorCertificate = _services.WebService.LoadCertificate(Global.LaunсherSettings.URL.MirrorBase);

				//проверяем существует ли сертификат для night-world.org на диске и сверяем его хэш с сертификатом на сервере
				if (!File.Exists(mainCertFile) || (mainCertificate != null && Cryptography.Sha256(mainCertificate) != Cryptography.FileSha256(mainCertFile)))
				{
					Runtime.DebugWrite("Main certificate is not valid");

					if (mainCertificate != null)
					{
						keyStoreToUpdate = true;
						if (Directory.Exists(keyStoreRoot))
						{
							Directory.Delete(keyStoreRoot, true);
							Directory.CreateDirectory(keyStoreRoot);
						}
						else
						{
							Directory.CreateDirectory(keyStoreRoot);
						}

						File.WriteAllBytes(mainCertFile, mainCertificate);
					}
					else
					{
						Runtime.DebugWrite("Error. mainCertificate is null");
					}
				}

				// просто проверяем есть ли сертификат для mirror.night-world.org на диске
				if (!File.Exists(mirrorCertFile))
				{
					if (mirrorCertificate != null)
					{
						keyStoreToUpdate = true;
						File.WriteAllBytes(mirrorCertFile, mirrorCertificate);
					}
					else
					{
						Runtime.DebugWrite("Error. mirrorCertificate is null");
					}
				}

				if (keyStoreToUpdate || !File.Exists(keyStoreFile))
				{
					Runtime.DebugWrite("Keystore is not exists");
					if (keyStoreToUpdate || !Directory.Exists(keyStorePath))
					{
						Directory.CreateDirectory(keyStorePath);
					}

					bool result = AddToTrustCacerts(javaPath, mainCertFile, keyStoreFile, "nightworld_cer");
					Runtime.DebugWrite($"nightworld_cer add result: {result}");

					bool result2 = AddToTrustCacerts(javaPath, mirrorCertFile, keyStoreFile, "mirror_nightworld_cer");
					Runtime.DebugWrite($"mirror_nightworld_cer add result: {result2}");

					if (result || result2) _keyStorePath = keyStoreFile;
				}

				Runtime.DebugWrite("Keystore file: " + keyStoreFile);
				_keyStorePath = keyStoreFile;
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
			}
		}

		private bool AddToTrustCacerts(string javaPath, string certFile, string keyStoreFile, string certAlias)
		{
			string baseKeyStoreFile = javaPath.Replace("/", "\\") + "\\lib\\security\\cacerts";
			if (!File.Exists(keyStoreFile))
			{
				File.Copy(baseKeyStoreFile, keyStoreFile.Replace("/", "\\"));
			}

			string keyTool = javaPath + "/bin/keytool.exe";
			string command = $"-import -noprompt -trustcacerts -alias {certAlias} -file \"{certFile}\" -keystore \"{keyStoreFile}\" -storepass changeit";
			Runtime.DebugWrite("Add to keystore command: \"" + keyTool + "\" " + command);
			return Utils.StartProcess(command, Utils.ProcessExecutor.Java, keyTool);
		}

		public bool Run(InitData data, Action gameExited, string gameClientName)
		{
			GameClientName = gameClientName;
			GameVersion = data?.VersionFile?.GameVersion;

			string command = CreateCommand(data);

			_process = new Process();

			if (_activeAccount != null)
			{
				lock (loocker)
				{
					var serverData = new ControlServerData(LaunсherSettings.ServerIp);
					_gameGateway = new OnlineGameGateway(_activeAccount.UUID, _activeAccount.SessionToken, _services.WebService, serverData, _generalSettings.NetworkDirectConnection);

					_onlineGameStopedMark = false;
					OnlineGameSystemStarted?.Invoke();

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
							},
							_services.NwApi
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
			_activeAccount?.SetInGameStatus(GameClientName);

			_processDataReceived?.Invoke("Выполняется запуск игры...");
			_processDataReceived?.Invoke($"\"{_javaPath}\" {command}");

			Runtime.DebugWrite("Run javaPath " + _javaPath);
			Runtime.DebugWrite($"Minecraft run command: \"{_javaPath}\" {command}");

			try
			{
				_process.StartInfo.FileName = _javaPath;
				_process.StartInfo.WorkingDirectory = _settings.GamePath + "/instances/" + _instanceId;
				_process.StartInfo.Arguments = command;
				_process.StartInfo.RedirectStandardOutput = true;
				_process.EnableRaisingEvents = true;
				_process.StartInfo.EnvironmentVariables["_JAVA_OPTIONS"] = "";
				_process.StartInfo.UseShellExecute = false;
				_process.StartInfo.CreateNoWindow = true;
				_process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

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
					_activeAccount?.SetOnlineStatus();

					lock (_onlineGameStpedEventLocker)
					{
						if (!_onlineGameStopedMark)
						{
							_onlineGameStopedMark = true;
							OnlineGameSystemStoped?.Invoke();
						}
					}

					_classInstance = null;
					gameExited();
				};

				_processIsWork = _process.Start();
				_process.BeginOutputReadLine();

				lock (loocker)
				{
					_gameGateway?.Initialization(_process.Id);
				}

				// отслеживаем появление окна
				while (_processIsWork)
				{
					Thread.Sleep(1000);

					try
					{
						if (GuiIsExists(_process.Id))
						{
							OnGameStarted?.Invoke(this);
							return true;
						}
					}
					catch
					{
						break;
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				_processIsWork = false;
				Runtime.DebugWrite(ex);

				return false;
			}
		}

		public InitData Update(ProgressHandler progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler, string version = null, bool onlyBase = false)
		{
			IInstallManager instance = _source.GetInstaller(_instanceId, onlyBase, _updateCancelToken);
			instance.FileDownloadEvent += fileDownloadHandler;

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
				using (JavaChecker javaCheck = new JavaChecker(javaVersionName, _services, _updateCancelToken))
				{
					if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
					{
						progressHandler?.Invoke(StateType.DownloadJava, new ProgressHandlerArguments()
						{
							StagesCount = 0,
							Stage = 0,
							Procents = 0,
							FilesCount = 0,
							TotalFilesCount = 1
						});

						bool downloadResult = javaCheck.Update(delegate (int percent, int file, int filesCount, string fileName)
						{
							progressHandler?.Invoke(StateType.DownloadJava, new ProgressHandlerArguments()
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
						_javaPath = _withDirectory.DirectoryPath + "/java/versions/" + javaVersion.JavaName + javaVersion.ExecutableFile;
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

			CreateJavaKeyStore(_javaPath);
			return instance.Update(_javaPath, progressHandler);
		}

		public InitData Initialization(ProgressHandler progressHandler, Action<string, int, DownloadFileProgress> fileDownloadHandler)
		{
			try
			{
				_withDirectory.Create(_settings.GamePath);
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

				VersionManifest files = _dataFilesManager.GetManifest(_instanceId, true);
				bool versionIsStatic = files?.version?.IsStatic == true;

				if (!versionIsStatic && _services.NwApi.ServerIsOnline())
				{
					data = Update(progressHandler, fileDownloadHandler, null, (_settings.IsAutoUpdate == false));
				}
				else
				{
					if (files?.version?.MinecraftJar != null && files.libraries != null)
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
							using (JavaChecker javaCheck = new JavaChecker(files.version.JavaVersionName, _services, _updateCancelToken, true))
							{
								JavaVersion javaInfo = javaCheck.GetJavaInfo();
								if (javaInfo?.JavaName == null || javaInfo.ExecutableFile == null)
								{
									return new InitData
									{
										InitResult = InstanceInit.JavaDownloadError
									};
								}

								_javaPath = _withDirectory.DirectoryPath + "/java/versions/" + javaInfo.JavaName + javaInfo.ExecutableFile;
							}
						}

						CreateJavaKeyStore(_javaPath);

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
				StateChanged?.Invoke(OnlineGameStatus.None, string.Empty);
			}
			catch { }

			lock (_onlineGameStpedEventLocker)
			{
				if (!_onlineGameStopedMark)
				{
					_onlineGameStopedMark = true;
					OnlineGameSystemStoped?.Invoke();
				}
			}
		}

		private void _RebotOnlineGame()
		{
			if (_gameGateway != null)
			{
				try
				{
					_gameGateway.StopWork();
				}
				catch { }

				var serverData = new ControlServerData(LaunсherSettings.ServerIp);
				_gameGateway = new OnlineGameGateway(_activeAccount.UUID, _activeAccount.SessionToken, _services.WebService, serverData, _generalSettings.NetworkDirectConnection);
				_gameGateway.Initialization(_classInstance._process.Id);
			}
		}

		public static void RebootOnlineGame()
		{
			if (_classInstance != null)
			{
				lock (loocker)
				{
					_classInstance._RebotOnlineGame();
				}
				StateChanged?.Invoke(OnlineGameStatus.None, string.Empty);
			}

		}
	}
}