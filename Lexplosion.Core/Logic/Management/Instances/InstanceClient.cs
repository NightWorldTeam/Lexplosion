using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Sources;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using NightWorld.Tools.Minecraft.NBT.StorageFiles;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management.Instances
{
	/// <summary>
	/// Филиал ада в c#.
	/// </summary>
	public class InstanceClient : VMBase
	{
		public readonly InstanceSource Type;
		private string _externalId = null;
		private string _localId = null;
		private readonly PrototypeInstance _dataManager;
		private readonly IInstanceSource _instanceSource;
		private readonly AllServicesContainer _services;
		private CancellationTokenSource _cancelTokenSource = null;
		private LaunchGame _gameManager = null;

		private const string LogoFileName = "logo.png";
		private const string UnknownName = "Unknown name";
		private const string UnknownAuthor = "Unknown author";
		private const string NoDescription = "Описания нет, но мы надеемся что оно будет.";

		public static Func<byte[]> LogoGenerator { set; private get; }

		#region events
		/// <summary>
		/// Вызывается когда происходит изменение состояния скачивания
		/// </summary>
		public event DownloadHandlerCallback DownloadHandler;
		/// <summary>
		/// Вызывается когда происходит измнение состояния текущего класса
		/// </summary>
		public event Action<StateType> StateChanged;

		/// <summary>
		/// Используется, для того чтобы сообщить InstanceFormViewModel,
		/// что данные обновились, и нужно обновить инфу о данных.
		/// </summary>
		public event Action DataChanged;
		/// <summary>
		/// Обновляется после того как InstanceClient будет иметь завершенную версию;
		/// </summary>
		public event Action BuildFinished;
		/// <summary>
		/// Вызывается прискачивании одного из файлов сборки. string - имя файла, int - процент скачивания, DownloadFileProgress - стадия
		/// </summary>
		public event Action<string, int, DownloadFileProgress> FileDownloadEvent;

		public event Action NameChanged;
		public event Action SummaryChanged;
		public event Action DescriptionChanged;
		public event Action LogoChanged;
		public event Action GameVersionChanged;

		/// <summary>
		/// Вызывается когда у сборки обноавляются данные.
		/// Существует чтобы <see cref="ClientsManager"/> при срабатывании этого эвента перезаписывал список сборок в файл
		/// </summary>
		private Action _internalDataChanged;
		#endregion

		#region info


		private StateType _state = StateType.Default;
		public StateType State
		{
			get => _state; internal set
			{
				if (_state != value)
				{
					_state = value;
					StateChanged?.Invoke(value);
				}
			}
		}

		public string LocalId { get => _localId; }
		public string ExternalId { get => _externalId; }

		private string _name;
		public string Name
		{
			get => _name;
			private set
			{
				_name = value;
				NameChanged?.Invoke();
				OnPropertyChanged();
			}
		}

		private string _description;
		public string Description
		{
			get => _description;
			private set
			{
				_description = value;
				OnPropertyChanged();
			}
		}

		private byte[] _logo = null;
		public byte[] Logo
		{
			get => _logo;
			private set
			{
				_logo = value;
				try
				{
					if (value != null)
					{
						_logo = ImageTools.ResizeImage(value, 120, 120);
					}
					else
					{
						_logo = null;
					}
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite("[Error] Image resize failed", color: ConsoleColor.DarkGray);
					_logo = value;
				}

				LogoChanged?.Invoke();
				OnPropertyChanged();
			}
		}

		private IEnumerable<CategoryBase> _categories = new List<CategoryBase>();

		public IEnumerable<CategoryBase> Categories
		{
			get => _categories;
			private set
			{
				if (value != null) _categories = value;
			}
		}

		private MinecraftVersion _gameVersion;
		public MinecraftVersion GameVersion
		{
			get => _gameVersion;
			private set
			{
				_gameVersion = value;
				GameVersionChanged?.Invoke();
				OnPropertyChanged();
			}
		}

		private string _summary;
		public string Summary
		{
			get => _summary;
			private set
			{
				_summary = value;
				OnPropertyChanged();
				SummaryChanged?.Invoke();
				DataChanged?.Invoke();
			}
		}

		private string _author;
		public string Author
		{
			get => _author;
			private set
			{
				_author = value;
				OnPropertyChanged();
			}
		}

		private bool _createdLocally = false;
		/// <summary>
		/// Создана ли сборка локально. То есть существует ли у неё на диски вся файловая структура
		/// </summary>
		public bool CreatedLocally
		{
			get => _createdLocally;
			internal set
			{
				_createdLocally = value;
				OnPropertyChanged();
				DataChanged?.Invoke();
			}
		}

		public bool IsFictitious { get; private set; }

		private bool _updateAvailable = false;
		public bool UpdateAvailable
		{
			get => _updateAvailable;
			set
			{
				_updateAvailable = IsInstalled && value;
				OnPropertyChanged();
				DataChanged?.Invoke();
			}
		}

		private string _profileVersion = null;
		/// <summary>
		/// Описывает версию установленное сборки. Нужен только для того, чтобы снаружи можно было получить версию
		/// </summary>
		public string ProfileVersion
		{
			get => _profileVersion;
			private set
			{
				_profileVersion = value;
				OnPropertyChanged();
			}
		}

		public long DownloadCounts { get; internal set; } = -1;
		public bool HasDownloadCounts { get => DownloadCounts > -1; }

		private string _websiteUrl = null;
		public string WebsiteUrl
		{
			get => _websiteUrl;
			private set
			{
				_websiteUrl = value;
				DataChanged?.Invoke();
			}
		}

		private bool _isComplete = true;
		public bool IsComplete
		{
			get => _isComplete;
			internal set
			{
				_isComplete = value;
				OnPropertyChanged();
				if (value)
				{
					BuildFinished?.Invoke();
				}
			}
		}

		public bool IsSharing { get; private set; } = false;

		public bool IsInstalled { get; internal set; } = false;

		public string FolderPath { get => _services.DirectoryService.GetInstancePath(_localId); }

		#endregion

		/// <summary>
		/// Если нужно скачать какую-то определенную версию клиента при первом вызове Update, то её присваивать сюда.
		/// Если результат работаты будет удачным, то сюда присвоится значение null чтобы не произошло повторное перекачивание.
		/// Null означает что методу Update нужно действовать как обычно.
		/// </summary>
		private string _instanceVersionToDownload = null;

		/// <summary>
		/// Базовый конструктор, от него должны наследоваться все остальные
		/// </summary>
		/// <param name="source">Источник модпака</param>
		internal InstanceClient(IInstanceSource source, AllServicesContainer services, Action internalDataChanged)
		{
			Type = source.SourceType;
			_instanceSource = source;
			_services = services;
			_internalDataChanged = internalDataChanged;
			_dataManager = source.ContentManager;
		}

		/// <summary>
		/// Этот конструктор создаёт еще не установленную сборку. Используется для сборок из каталога
		/// </summary>
		/// <param name="source">Источник модпака</param>
		/// <param name="externalID">Внешний ID</param>
		internal InstanceClient(IInstanceSource source, AllServicesContainer services, Action internalDataChanged, string externalID) : this(source, services, internalDataChanged)
		{
			_externalId = externalID;
		}

		/// <summary>
		/// Этот конструктор создаёт установленную сборку. Используется для сборок в библиотеке.
		/// </summary>
		/// <param name="source">Источник модпака</param>
		/// <param name="externalID">Внешний ID</param>
		/// <param name="externalID">Локальный ID</param>
		internal InstanceClient(IInstanceSource source, AllServicesContainer services, Action internalDataChanged, string externalID, string localId) : this(source, services, internalDataChanged, externalID)
		{
			_localId = localId;
		}

		/// <summary>
		/// Этот конструктор создаёт локальную сборку. Должен использоваться только при создании локальной сборки и при импорте.
		/// </summary>
		/// <param name="name">Название сборки</param>
		/// <param name="source">Источник модпака</param>
		/// <param name="gameVersion">Версия игры</param>
		internal InstanceClient(string name, IInstanceSource source, AllServicesContainer services, Action internalDataChanged, MinecraftVersion gameVersion, string externalId, string localId) : this(source, services, internalDataChanged, externalId, localId)
		{
			Name = name;
			GameVersion = gameVersion;
		}

		internal void DeployLocally(ClientType modloader, bool isNwClient, string logoPath = null, string modloaderVersion = null, string optifineVersion = null, bool sodium = false, string profileVersion = null)
		{
			CreatedLocally = true;
			Author = Account.AnyFuckingLogin;
			Description = NoDescription;
			Summary = NoDescription;

			if (modloaderVersion == null) modloader = ClientType.Vanilla;

			try
			{
				if (Logo == null)
				{
					if (logoPath != null && File.Exists(logoPath))
					{
						Logo = File.ReadAllBytes(logoPath);
					}
					else
					{
						Logo = LogoGenerator?.Invoke();
					}
				}
			}
			catch
			{
				Logo = LogoGenerator?.Invoke();
			}

			AdditionalInstallerType? installer = null;
			string installerVer = null;
			if (optifineVersion != null)
			{
				installerVer = optifineVersion;
				installer = AdditionalInstallerType.Optifine;
			}

			CreateFileStruct(modloader, modloaderVersion, isNwClient, installer, installerVer, profileVersion);
			SaveAssets();

			if (sodium && (modloader == ClientType.Fabric || modloader == ClientType.Quilt))
			{
				ThreadPool.QueueUserWorkItem(delegate (object o)
				{
					var sodium = _services.MdApi.GetProject("AANobbMI");
					var addon = AddonsManager.GetManager(GetBaseData, _services).CreateModrinthAddon(sodium);
					var stateData = new DynamicStateHandler<SetValues<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>(delegate (SetValues<InstanceAddon, DownloadAddonRes> a, InstanceAddon.InstallAddonState b) { });
					addon.InstallLatestVersion(stateData, downloadDependencies: true);
				});
			}
		}

		/// <summary>
		/// Создает локальную структуру сборки.
		/// То есть помечает ее как созданную локлаьно и создает для нее всю файловую структуру
		/// </summary>
		internal void CreateLocalStruct(string localId)
		{
			_localId = localId;
			bool isNwClient = GlobalData.GeneralSettings.NwClientByDefault == true;
			CreateFileStruct(ClientType.Vanilla, string.Empty, isNwClient);
			SaveAssets();
			CreatedLocally = true;
		}

		internal void DeleteLocalStruct()
		{
			_services.DirectoryService.DeleteInstance(_localId);
			UpdateAvailable = false;
			CreatedLocally = false;
			IsInstalled = false;
		}

		internal void UpdateInfo(MinecraftServerInstance server, bool autoLogin)
		{
			string modpackVersion = server.ModpackInfo?.Version;
			AddGameServer(server, autoLogin);
			_instanceVersionToDownload = modpackVersion;

			if (server.Tags != null)
			{
				Categories = server.Tags.Select((x) => new SimpleCategory(x.Id, x.Name));
			}

			if (server.Description != null)
			{
				Description = server.Description;
				Summary = server.Description.Truncate(45);
			}

			SaveAssets();

			if (!string.IsNullOrWhiteSpace(server.IconUrl))
			{
				DownloadLogo(server.IconUrl, SaveAssets);
			}

			Settings settings = GetSettings();
			settings.IsAutoUpdate = string.IsNullOrWhiteSpace(modpackVersion);
			SaveSettings(settings);
		}

		internal void UpdateInfo(string name, MinecraftVersion gameVersion, byte[] logo, string instanceVersion, bool isInstalled)
		{
			//получаем асетсы модпака
			var assetsData = _services.DataFilesService.GetFile<InstanceAssetsFileDecodeFormat>(_services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId + "/assets.json");

			if (assetsData != null)
			{
				string file = _services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName;

				try
				{
					if (File.Exists(file))
					{
						logo = File.ReadAllBytes(file);
					}
				}
				catch { }
			}

			if (assetsData != null)
			{
				Name = name ?? UnknownName;
				Summary = assetsData.Summary ?? NoDescription;
				Author = assetsData.Author ?? UnknownAuthor;
				Description = assetsData.Description ?? NoDescription;
				Categories = assetsData.Categories;
				GameVersion = gameVersion;
				Logo = logo;
				_profileVersion = instanceVersion;
			}
			else
			{
				Name = name ?? UnknownName;
				Summary = NoDescription;
				Author = UnknownAuthor;
				Description = NoDescription;
				GameVersion = gameVersion;
				Logo = logo;
				_profileVersion = instanceVersion;
			}

			CreatedLocally = true;
			IsInstalled = isInstalled;
			//хуярим проверку обновлений в пуле потоков
			ThreadPool.QueueUserWorkItem(delegate (object state)
			{
				CheckUpdates();
			});
		}

		internal void UpdateInfo(string name, MinecraftVersion gameVersion, IEnumerable<CategoryBase> categories, string summary, string description, string author, string websiteUrl)
		{
			if (!string.IsNullOrEmpty(name))
				Name = name;
			else if (string.IsNullOrEmpty(Name))
				Name = UnknownName;

			if (!string.IsNullOrEmpty(summary))
				Summary = summary;
			else if (string.IsNullOrEmpty(Summary))
				Summary = NoDescription;

			if (!string.IsNullOrEmpty(description))
				Description = description;
			else if (string.IsNullOrEmpty(Description))
				Description = NoDescription;

			if (!string.IsNullOrEmpty(author))
				Author = author;
			else if (string.IsNullOrEmpty(Author))
				Author = UnknownAuthor;

			if (categories != null)
				Categories = categories;
			if (gameVersion != null)
				GameVersion = gameVersion;
			if (websiteUrl != null)
				WebsiteUrl = websiteUrl;
		}

		internal void MakeFictitiousClient(string tempName)
		{
			Name = tempName;
			IsFictitious = true;
			Author = UnknownAuthor;
			Summary = string.Empty;
			IsComplete = false;
		}

		internal void CompleteInitialization(InstanceInit initResult)
		{
			if (initResult == InstanceInit.Successful) IsComplete = true;
			IsFictitious = false;
			State = StateType.Default;
		}

		/// <summary>
		/// Возвращает основные данные модпака.
		/// </summary>
		public BaseInstanceData GetBaseData
		{
			get
			{
				MinecraftVersion gameVersion = null;
				string modloaderVersion = string.Empty;
				ClientType clientType = ClientType.Vanilla;
				string optifineVersion = null;
				bool IsNwClient = false;

				if (_localId != null)
				{
					VersionManifest manifest = _services.DataFilesService.GetManifest(_localId, false);
					if (manifest?.version != null)
					{
						gameVersion = manifest.version.GameVersionInfo;
						modloaderVersion = manifest.version.ModloaderVersion ?? string.Empty;
						clientType = manifest.version.ModloaderType;
						optifineVersion = manifest.version.AdditionalInstaller?.installerVersion;
						IsNwClient = manifest.version.IsNightWorldClient == true;
					}
				}

				return new BaseInstanceData
				{
					LocalId = _localId,
					ExternalId = _externalId,
					Type = Type,
					GameVersion = gameVersion,
					InLibrary = CreatedLocally,
					Author = Author,
					Categories = Categories,
					Description = Description,
					Name = _name,
					Summary = _summary,
					ModloaderVersion = modloaderVersion,
					Modloader = clientType,
					OptifineVersion = optifineVersion,
					IsNwClient = IsNwClient
				};
			}
		}

		/// <summary>
		/// Получает всю информацию о модпаке.
		/// </summary>
		/// <returns>InstanceData, содержащий в себе данные которые могут потребоваться</returns>
		public InstanceData GetFullInfo()
		{
			InstanceData fullInfo = _dataManager.GetFullInfo(_localId, _externalId);

			if (fullInfo == null)
			{
				return null;
			}

			if (fullInfo.Description == null)
				fullInfo.Description = NoDescription;
			if (fullInfo.Summary == null)
				fullInfo.Summary = NoDescription;

			return fullInfo;
		}

		public List<InstanceVersion> GetVersions()
		{
			return _dataManager.GetVersions(_externalId) ?? new List<InstanceVersion>();
		}

		/// <summary>
		/// Изменяет параметры установленного модпака
		/// </summary>
		/// <param name="data">Вся инфа.</param>
		/// <param name="logoPath">Путь до лого. Если логотип изменять не нужно, то null</param>
		public void ChangeParameters(BaseInstanceData data, string logoPath)
		{
			VersionManifest manifest = _services.DataFilesService.GetManifest(_localId, false);
			if (manifest != null)
			{
				manifest.version.ModloaderType = data.Modloader;
				manifest.version.ModloaderVersion = data.ModloaderVersion;
				manifest.version.GameVersionInfo = data.GameVersion;
				manifest.version.IsNightWorldClient = data.IsNwClient;

				if (manifest.version.ModloaderType == ClientType.Vanilla && data.OptifineVersion != null)
				{
					manifest.version.AdditionalInstaller = new AdditionalInstaller()
					{
						type = AdditionalInstallerType.Optifine,
						installerVersion = data.OptifineVersion
					};
				}

				if (string.IsNullOrWhiteSpace(data.OptifineVersion))
				{
					manifest.version.AdditionalInstaller = null;
				}

				_services.DataFilesService.SaveManifest(_localId, manifest);
			}

			if (logoPath != null)
			{
				SetLogo(logoPath);
			}

			Description = data.Description;
			GameVersion = data.GameVersion;
			Summary = data.Summary;
			Categories = data.Categories;
			SaveAssets();

			Name = data.Name;
			_internalDataChanged?.Invoke();
		}

		/// <summary>
		/// Останавливает игру
		/// </summary>
		public void StopGame()
		{
			_gameManager?.Stop();
			State = StateType.Default;
		}

		/// <summary>
		/// Отменяет скачивание сборки.
		/// </summary>
		public void CancelDownload()
		{
			State = StateType.DownloadInCancellation;
			_cancelTokenSource?.Cancel();
		}

		internal void DownloadStateHandler(StateType stageType, ProgressHandlerArguments data)
		{
			State = stageType;
			DownloadHandler?.Invoke(data);
		}

		/// <summary>
		/// Обновляет или скачивает сборку. Сборка должна быть добавлена в библиотеку.
		/// </summary>
		public ClientInitResult Update(string instanceVersion = null)
		{
			_cancelTokenSource = new CancellationTokenSource();
			State = StateType.DownloadPrepare;

			Settings instanceSettings = GetSettings();
			instanceSettings.Merge(GlobalData.GeneralSettings, true);

			instanceVersion ??= _instanceVersionToDownload;

			var generalSettings = GlobalData.GeneralSettings;
			var activeAccount = Account.ActiveAccount?.IsAuthed == true ? Account.ActiveAccount : null;
			var launchAccount = Account.LaunchAccount;

			LaunchGame launchGame = new LaunchGame(_localId, generalSettings, instanceSettings, activeAccount, launchAccount, _instanceSource, _services, _cancelTokenSource.Token);
			InitData data = launchGame.Update(DownloadStateHandler, FileDownloadEvent, instanceVersion);

			UpdateAvailable = data.UpdatesAvailable;
			if (data.InitResult == InstanceInit.IsCancelled)
			{
				ProfileVersion = string.IsNullOrWhiteSpace(data.ClientVersion) ? ProfileVersion : data.ClientVersion;
			}
			else
			{
				ProfileVersion = data.ClientVersion;
			}

			if (data.InitResult == InstanceInit.Successful)
			{
				IsInstalled = (data.InitResult == InstanceInit.Successful);
				_instanceVersionToDownload = null;

				_internalDataChanged?.Invoke(); // чтобы если сборка установилась то флаг IsInstalled сохранился
			}

			Runtime.DebugWrite("UpdateInstance-end " + data.InitResult);

			_cancelTokenSource = null;
			_gameManager?.DeleteCancellationToken();
			State = StateType.Default;

			return new ClientInitResult(data.InitResult, data.DownloadErrors);
		}

		/// <summary>
		/// Запускает сборку. Если надо её докачивает. Сборка должна быть доавлена в библиотеку
		/// </summary>
		public ClientRunResult Run()
		{
			_cancelTokenSource = new CancellationTokenSource();

			State = StateType.DownloadPrepare;

			Settings instanceSettings = GetSettings();
			instanceSettings.Merge(GlobalData.GeneralSettings, true);

			var generalSettings = GlobalData.GeneralSettings;
			var activeAccount = Account.ActiveAccount?.IsAuthed == true ? Account.ActiveAccount : null;
			var launchAccount = Account.LaunchAccount;

			_gameManager = new LaunchGame(_localId, generalSettings, instanceSettings, activeAccount, launchAccount, _instanceSource, _services, _cancelTokenSource.Token);
			InitData data = _gameManager.Initialization(DownloadStateHandler, FileDownloadEvent);

			UpdateAvailable = data.UpdatesAvailable;
			ProfileVersion = data.ClientVersion;

			bool runResult = false;

			if (data.InitResult == InstanceInit.Successful)
			{
				IsInstalled = true;
				_internalDataChanged?.Invoke(); // чтобы если сборка установилась то флаг IsInstalled сохранился
				State = StateType.Launching;

				runResult = _gameManager.Run(data, () => { _gameManager = null; State = StateType.Default; }, Name);
				_services.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
				// TODO: тут надо как-то определять что сборка обновилась и UpdateAvailable = false делать, если было обновление
			}

			Runtime.DebugWrite("Run-end " + data.InitResult);

			_cancelTokenSource = null;
			_gameManager?.DeleteCancellationToken();
			State = runResult ? StateType.GameRunning : StateType.Default;

			return new ClientRunResult(data.InitResult, data.DownloadErrors, runResult);
		}

		internal void CheckUpdates()
		{
			UpdateAvailable = _dataManager.CheckUpdates(_localId);
		}

		/// <summary>
		/// Ну бля, качает заглавную картинку (лого) по ссылке и записывает в переменную Logo. Делает это всё в пуле потоков.
		/// </summary>
		internal void DownloadLogo(string url, Action callback)
		{
			ThreadPool.QueueUserWorkItem(delegate (object state)
			{
				try
				{
					using (var webClient = new WebClient())
					{
						webClient.Proxy = null;
						Logo = webClient.DownloadData(url);
						callback();
					}
				}
				catch { }
			});
		}

		internal void SetLogo(string logoFilePath)
		{
			try
			{
				if (File.Exists(logoFilePath))
					Logo = File.ReadAllBytes(logoFilePath);
			}
			catch { }
		}

		internal void SetLogo(byte[] logoBytes) => Logo = logoBytes;

		/// <summary>
		/// Сохраняет асетсы клиента в файл.
		/// </summary>
		internal void SaveAssets()
		{
			try
			{
				if (!Directory.Exists(_services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId))
				{
					Directory.CreateDirectory(_services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId);
				}

				if (Logo != null)
				{
					File.WriteAllBytes(_services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName, Logo);
				}
			}
			catch { }

			string file = _services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId + "/assets.json";
			InstanceAssetsFileDecodeFormat assetsData_ = _services.DataFilesService.GetFile<InstanceAssetsFileDecodeFormat>(file);

			var categories_ = new List<SimpleCategory>();
			if (Categories != null)
			{
				foreach (var category in Categories)
				{
					categories_.Add(new SimpleCategory(category));
				}
			}

			var assetsData = new InstanceAssets
			{
				Author = Author,
				Categories = categories_,
				Description = Description,
				Images = (assetsData_ != null) ? assetsData_.Images : null,
				Summary = Summary
			};

			_services.DataFilesService.SaveFile(file, JsonConvert.SerializeObject(assetsData));
		}

		/// <summary>
		/// Создает необходимую структуру файлов для сборки при её добавлении в библиотеку (ну при создании локальной).
		/// </summary>
		/// <param name="modloader">Тип модлоадера</param>
		/// <param name="modloaderVersion">Версия модлоадера</param>
		/// <param name="additionalInstaller">Оптифайн. Если не нужен, то null</param>
		/// <param name="additionalInstallerVer">Версия оптифайна. null если не нужен</param>
		/// <param name="profileVersion">Версия сборки. Null если сборка локальная, или нужно поставить последнюю версию</param>
		private void CreateFileStruct(ClientType modloader, string modloaderVersion, bool isNwClient, AdditionalInstallerType? additionalInstaller = null, string additionalInstallerVer = null, string profileVersion = null)
		{
			// TODO: тут надо трай. И если будет исключение надо передавать ошибку

			if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

			VersionManifest manifest = new VersionManifest
			{
				version = new VersionInfo
				{
					GameVersionInfo = GameVersion,
					ModloaderVersion = modloaderVersion,
					ModloaderType = modloader,
					IsNightWorldClient = isNwClient,
				}
			};

			if (additionalInstaller != null && additionalInstallerVer != null)
			{
				manifest.version.AdditionalInstaller = new AdditionalInstaller()
				{
					type = additionalInstaller ?? AdditionalInstallerType.Optifine,
					installerVersion = additionalInstallerVer
				};
			}

			_services.DataFilesService.SaveManifest(_localId, manifest);

			if (_instanceSource != null)
			{
				InstancePlatformData instanceData = _instanceSource.CreateInstancePlatformData(_externalId, _localId, profileVersion);
				if (instanceData != null)
				{
					_services.DataFilesService.SavePlatfromData(_localId, instanceData);
				}
			}
			else if (Type != InstanceSource.Local)
			{
				var instanceData = new InstancePlatformData
				{
					id = _externalId
				};

				_services.DataFilesService.SavePlatfromData(_localId, instanceData);
			}
		}

		public string GetDirectoryPath()
		{
			return (_services.DirectoryService.DirectoryPath.Replace("/", @"\") + @"\instances\" + _localId).Replace(@"\\", @"\");
		}

		public Settings GetSettings()
		{
			return _services.DataFilesService.GetSettings(_localId);
		}

		public void SaveSettings(Settings settings)
		{
			_services.DataFilesService.SaveSettings(settings, _localId);
		}

		/// <summary>
		/// Возвращает список файлов и папок данной директории в папки модпака. 
		/// </summary>
		/// <param name="path">
		/// Путь до директории, содержание котрой необходимо вернуть. Путь указывается относительно папки модпака.
		/// Если нужно получить список элементов из корня папки модпака, то ничего передавать не надо.
		/// </param>
		/// <returns>Элементы данной директории. Ключ - путь относительно папки модпака, значение - описание элемента директории.</returns>
		public Dictionary<string, PathLevel> GetPathContent(string path = "/", PathLevel parentUnit = null)
		{
			Dictionary<string, PathLevel> pathContent = new Dictionary<string, PathLevel>();
			string dirPath = FolderPath;

			try
			{
				DirectoryInfo dir = new DirectoryInfo(dirPath + path);

				try
				{
					foreach (DirectoryInfo item in dir.GetDirectories())
					{
						pathContent["/" + item.Name] =
							new PathLevel(item.Name, false, path + "/" + item.Name, (item.Name == "mods" || item.Name == "scripts" || item.Name == "resources" || item.Name == "resourcepacks" || item.Name == "config"));
					}
				}
				catch { }

				try
				{
					string installedAddons = DataFilesManager.INSTALLED_ADDONS_FILE;
					string instancePlatformData = DataFilesManager.INSTANCE_PLATFORM_DATA_FILE;
					string instanceContent = DataFilesManager.INSTANCE_CONTENT_FILE;
					string lastUpdates = DataFilesManager.LAST_UPDATES_FILE;
					string manifest = DataFilesManager.MANIFEST_FILE;

					foreach (var item in dir.GetFiles())
					{
						if (item.Name != installedAddons && item.Name != lastUpdates && item.Name != manifest && item.Name != instanceContent && item.Name != instancePlatformData)
							pathContent["/" + item.Name] = new PathLevel(item.Name, true, path + "/" + item.Name);
					}
				}
				catch { }
			}
			catch { }

			return pathContent;
		}

		/// <summary>
		/// Раздать сборку друзьям
		/// </summary>
		/// <param name="exportList">Аналогично методу Export</param>
		/// <param name="distributor">Экземпляр раздачи</param>
		/// <returns>Результат подготовки к раздаче.</returns>
		public ExportResult Share(Dictionary<string, PathLevel> exportList, out FileDistributor distributor)
		{
			distributor = null;

			var activeAccount = Account.ActiveAccount;
			if (activeAccount == null || !activeAccount.IsAuthed)
			{
				return ExportResult.NotExistsValidAccount;
			}

			string uuid = activeAccount.UUID;
			string sessionToken = activeAccount.SessionToken;

			string shareDir = FileDistributor.GetSharesDir(_services.DirectoryService);
			try
			{
				if (!Directory.Exists(shareDir))
				{
					Directory.CreateDirectory(shareDir);
				}
			}
			catch
			{
				return ExportResult.ZipFileError;
			}

			var zipFile = shareDir + _localId + ".zip";
			ExportResult result = Export(exportList, zipFile, _name);

			if (result == ExportResult.Successful)
			{
				IsSharing = true;

				distributor = FileDistributor.CreateDistribution(zipFile, Name, uuid, sessionToken, _services);
				if (distributor == null) return ExportResult.ZipFileError;

				distributor.OnClosed += delegate ()
				{
					IsSharing = false;
				};

				return ExportResult.Successful;
			}
			else
			{
				return result;
			}
		}

		/// <summary>
		/// Экспортирует модпак.
		/// </summary>
		/// <param name="exportList">Список файлов и папок на экспорт. Ключ - путь относительно папки модпака, значение - описание элемента директории.</param>
		/// <param name="exportFile">Полноый путь к архиву, в который будет производиться экспорт.</param>
		/// <param name="name">Имя для экспорта.</param>
		/// <returns>Результат экспорта.</returns>
		public ExportResult Export(Dictionary<string, PathLevel> exportList, string exportFile, string name)
		{
			string dirPath = FolderPath;

			void ParsePathLevel(ref List<string> list, Dictionary<string, PathLevel> levelsList)
			{
				foreach (string key in levelsList.Keys)
				{
					PathLevel elem = levelsList[key];
					if (elem.IsSelected)
					{
						if (elem.IsFile)
						{
							list.Add(dirPath + levelsList[key].FullPath);
						}
						else
						{
							if (elem.UnitsList == null || elem.UnitsList.Count == 0)
							{
								string[] files;
								try
								{
									files = Directory.GetFiles(dirPath + levelsList[key].FullPath, "*", SearchOption.AllDirectories);
								}
								catch
								{
									files = new string[0];
								}

								foreach (string file in files)
								{
									list.Add(file);
								}
							}
							else
							{
								ParsePathLevel(ref list, elem.UnitsList);
							}
						}
					}
				}
			}

			List<string> filesList = new List<string>();
			ParsePathLevel(ref filesList, exportList);

			if (File.Exists(dirPath + "/" + DataFilesManager.INSTALLED_ADDONS_FILE))
			{
				filesList.Add(dirPath + "/" + DataFilesManager.INSTALLED_ADDONS_FILE);
			}

			VersionManifest instanceManifest = _services.DataFilesService.GetManifest(_localId, false);

			string logoPath = (Logo != null ? _services.DirectoryService.DirectoryPath + "/instances-assets/" + _localId + "/" + LogoFileName : null);
			var parameters = new ArchivedClientData
			{
				Author = Author,
				Description = Description,
				GameVersionInfo = instanceManifest?.version?.GameVersionInfo,
				ModloaderType = instanceManifest?.version?.ModloaderType ?? ClientType.Vanilla,
				ModloaderVersion = instanceManifest?.version?.ModloaderVersion,
				Name = name ?? Name,
				//Categories = Categories,
				Summary = Summary,
				LogoFileName = (logoPath != null ? LogoFileName : null),
				AdditionalInstallerType = instanceManifest?.version?.AdditionalInstaller?.type,
				AdditionalInstallerVersion = instanceManifest?.version?.AdditionalInstaller?.installerVersion,

			};

			string infoFileContent = JsonConvert.SerializeObject(parameters);

			var res = _services.DirectoryService.ExportInstance(_localId, filesList, exportFile, infoFileContent, logoPath);
			Runtime.DebugWrite(res);

			return res;
		}

		public void AddGameServer(MinecraftServerInstance server, bool autoLogin)
		{
			ServersDatManager serversDat = new ServersDatManager(FolderPath + "/servers.dat");

			if (!serversDat.ContsainsServer(server.Name, server.Address))
			{
				serversDat.AddServer(new ServersDatManager.ServerData(server.Name, server.Address));
				serversDat.SaveFile();
			}

			Settings settings = GetSettings();
			settings.AutoLoginServer = autoLogin ? server.Address : null;

			SaveSettings(settings);
		}

	}
}