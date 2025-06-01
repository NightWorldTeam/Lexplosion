using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Extensions;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public sealed class DownloadingData : ObservableObject
    {
        /// <summary>
        /// Текущий этап
        /// </summary>
        private StageType _stage;
        public StageType Stage
        {
            get => _stage; set
            {
                _stage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Всего этапов
        /// </summary>
        private int _totalStages;
        public int TotalStages
        {
            get => _totalStages; set
            {
                _totalStages = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Активный этап
        /// </summary>
        private int _currentStage;
        public int CurrentStage
        {
            get => _currentStage; set
            {
                _currentStage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StageFormatted));
            }
        }
        /// <summary>
        /// Всего файлов
        /// </summary>
        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles; set
            {
                _totalFiles = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Текущие количество скаченных файлов
        /// </summary>
        private int _filesCounts;
        public int FilesCounts
        {
            get => _filesCounts; set
            {
                _filesCounts = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Процент скачивания
        /// </summary>
        private int _persentages;
        public int Percentages
        {
            get => _persentages; set
            {
                _persentages = value;
                OnPropertyChanged();
            }
        }


        public string StageFormatted { get; }

        public DownloadingData()
        {
            StageFormatted = $"{FilesCounts}/{TotalFiles}";
            OnPropertyChanged(string.Empty);
        }
    }

    /// <summary>
    /// Перечисление состояний формы.
    /// </summary>
    public enum InstanceState
    {
        Default,
        Downloading,
        DownloadCanceling,
        Launching,
        Preparing,
        Running
    }

	public class InstanceModelBase : ViewModelBase, IEquatable<InstanceClient>
	{
		private readonly InstanceClient _instanceClient;
		private readonly ClientsManager _clientsManager = Runtime.ClientsManager;
		private readonly Action<InstanceClient> _exportFunc;
		private readonly Action<InstanceModelBase> _setRunningGame;

		private readonly AppCore _appCore;


        private Action _openAddonPage;


        #region Events


        /// <summary>
        /// Эвент для контроллеров для добавление в библиотеку.
        /// </summary>
        public static event Action<InstanceModelBase> GlobalAddedToLibrary;
        /// <summary>
        /// Эвент для контроллеров для удаление из библиотеки.
        /// </summary>
        public static event Action<InstanceModelBase> GlobalDeletedEvent;


        public event Action StateChanged;
        public event Action DataChanged;

        public event Action NameChanged;
        public event Action GameVersionChanged;
        public event Action ModloaderChanged;
        public event Action LogoChanged;
        public event Action SummaryChanged;
        public event Action DescriptionChanged;
        public event Action IsInstalledChanged;
        public event Action InLibraryChanged;


        // < -- Эвенты процесса скачивания -- > //

        /// <summary>
        /// Процесс скачивания клиента игры был запущен.
        /// </summary>
        public event Action DownloadStarted;
        /// <summary>
        /// Процесс скачивания клиента игры был отменен.
        /// </summary>
        public event Action DownloadCanceled;
        /// <summary>
        /// Информация о прогресс было изменено.
        /// </summary>
        public event Action<StageType, ProgressHandlerArguments> DownloadProgressChanged;
        /// <summary>
        /// Процесс скачивания клиента игры был завершен.
        /// </summary>
        public event Action<InstanceInit, IEnumerable<string>, bool> DownloadComplited;

        // < -- Эвенты процесса запуска/закрытия -- > //

        /// <summary>
        /// Клиент игры был запущен.
        /// </summary>
        public event Action GameLaunched;
        /// <summary>
        /// Запуск клиента игры завершен.
        /// </summary>
        public event Action<bool> GameLaunchCompleted;
        /// <summary>
        /// Клиент игры был закрыт,из вне.
        /// </summary>
        public event Action GameClosed;

        public event Action GameClosedByLauncher;

        /// <summary>
        /// Эвент добавление в библиотеки, 
        /// например для анимаций.
        /// </summary>
        public event Action<InstanceModelBase> AddedToLibraryEvent;
        /// <summary>
        /// Эвент удаление сборки из библиотеки, 
        /// например для анимаций.
        /// </summary>
        public event Action<InstanceModelBase> DeletedEvent;


        #endregion Events


        #region Properties


        private InstanceState _state;
        public InstanceState State
        {
            get => _state; private set
            {
                _state = value;
                StateChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        public string LocalId { get => _instanceClient.LocalId; }
        public string WebsiteUrl { get => _instanceClient.WebsiteUrl; }


        #region Visual Data


        public string Name { get => _instanceClient.Name; }
        public string Author { get => _instanceClient.Author; }
        public string Summary { get => _instanceClient.Summary.Replace('\n', ' '); }
        public string Description { get => _instanceClient.Description; }
        public byte[] Logo { get; private set; }
        public IEnumerable<IProjectCategory> Tags { get; }
        public InstanceSource Source { get => _instanceClient.Type; }
        public bool HasTotalDownloads { get; private set; }
        public string TotalDownloads { get; private set; }
        public bool IsLocal { get => Source == InstanceSource.Local; }
        public string ClientVersion { get => _instanceClient.ProfileVersion; }
        public InstanceData PageData { get; private set; }
        public bool IsInstanceCompleted { get => _instanceClient.IsComplete; }

        private bool _isShareDownloading;
        public bool IsShareDownloading
        {
            get => _isShareDownloading; set
            {
                _isShareDownloading = value;
                OnPropertyChanged(nameof(IsShareDownloading));
            }
        }

        
        #endregion Visual Data


        private bool _isLaunching;
        /// <summary>
        /// Запускается ли игра.
        /// </summary>
        public bool IsLaunching
        {
            get => _isLaunching; private set
            {
                _isLaunching = value;
                OnPropertyChanged(nameof(AnyProcessActive));
                OnPropertyChanged();
            }
        }

        private bool _isLaunched;
        /// <summary>
        /// Запущена ли игра. TODO: Replace to "IsRunning"
        /// </summary>
        public bool IsLaunched
        {
            get => _isLaunched; private set
            {
                _isLaunched = value;
                OnPropertyChanged();
            }
        }

		public bool IsInstalled { get => _instanceClient.IsInstalled; }
		public bool InLibrary { get => _instanceClient.CreatedLocally || _instanceClient.IsFictitious; }
		public string DirectoryPath { get => _instanceClient.GetDirectoryPath(); }
		public bool HasAvailableUpdate { get => _instanceClient.UpdateAvailable && _instanceClient.IsInstalled; }

        public bool IsImporting
        {
            get => !_instanceClient.IsComplete;
        }

        public MinecraftVersion GameVersion { get => _instanceClient.GameVersion ?? new MinecraftVersion("1.20.1"); }


        public BaseInstanceData BaseData
        {
            get
            {
                var s = _instanceClient.GetBaseData;
                return s;
            }
        }

        private InstanceData _addionalData;
        public InstanceData AdditionalData
        {
            get
            {
                return _addionalData ?? (_addionalData = _instanceClient.GetFullInfo());
            }
        }


        #region Processes


        /// <summary>
        /// Активен хотя бы один из процессов
        /// </summary>
        public bool AnyProcessActive { get => IsPrepare || IsDownloading || IsLaunching || DownloadCancelling || IsImporting; }


        private bool _isPrepare;
        /// <summary>
        /// Подготовкак к скачиванию/запуску
        /// </summary>
        public bool IsPrepare
        {
            get => _isPrepare; set
            {
                _isPrepare = value;
                OnPropertyChanged(nameof(AnyProcessActive));
                OnPropertyChanged();
            }
        }


        private bool _isDownloading;
        /// <summary>
        /// Состояние скачивания
        /// </summary>
        public bool IsDownloading
        {
            get => _isDownloading; set
            {
                _isDownloading = value;
                OnPropertyChanged();
            }
        }

        private bool _downloadCancelling;
        /// <summary>
        /// Процесс отмены скачивания
        /// </summary>
        public bool DownloadCancelling
        {
            get => _downloadCancelling; set
            {
                _downloadCancelling = value;
                OnPropertyChanged(nameof(AnyProcessActive));
                OnPropertyChanged();
            }
        }


        #endregion Processes


        /// <summary>
        /// Данные скачивания
        /// </summary>
        public DownloadingData DownloadingData { get; set; } = new();
        public InstanceDistribution InstanceDistribution { get; }
        public ImportData? ImportData { get; }


        #endregion Properties


        #region Constructors


        public InstanceModelBase(AppCore appCore, InstanceClient instanceClient, Action<InstanceClient> exportFunc, Action<InstanceModelBase> setRunningGame, InstanceDistribution instanceDistribution = null, ImportData? importData = null)
        {
            _appCore = appCore;

            _instanceClient = instanceClient;
            _exportFunc = exportFunc;
            InstanceDistribution = instanceDistribution;
            ImportData = importData;

            if (instanceDistribution != null)
            {
                instanceDistribution.DownloadFinished += OnShareDownloadFinished;
                IsShareDownloading = true;
            }

            _instanceClient.NameChanged += OnNameChanged;
            _instanceClient.LogoChanged += OnLogoChanged;
            _instanceClient.StateChanged += OnStateClientChanged;
            _instanceClient.ProgressHandler += OnDownloadProgressChanged;
            _instanceClient.DownloadStarted += OnDownloadStarted;
            _instanceClient.Initialized += OnDownloadCompleted;
            _instanceClient.LaunchComplited += OnLaunchComplited;
            _instanceClient.BuildFinished += OnBuildFinished;
            _instanceClient.GameExited += OnGameExited;

            Logo = _instanceClient.Logo;
            TotalDownloads = _instanceClient.DownloadCounts.LongToString();
            HasTotalDownloads = _instanceClient.HasDownloadCounts;
            var versionTag = new SimpleCategory { Name = GameVersion?.Id ?? "" };
            var tags = _instanceClient.Categories.ToList() ?? new List<CategoryBase>();
            tags.Insert(0, versionTag);
            Tags = tags;
        }

        private void OnGameExited(string instanceId)
        {
            IsLaunched = false;
            State = InstanceState.Default;
        }

        /// <summary>
        /// Скачивание раздачи завершено
        /// </summary>
        private void OnShareDownloadFinished(ImportResult obj)
        {
            IsShareDownloading = false;
        }

        private void OnSharingStateChanged()
        {
            OnPropertyChanged(nameof(IsShareDownloading));
        }

        private void OnDownloadStarted()
        {
            IsDownloading = true;

            DownloadStarted?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnLaunchComplited(string instanceId, bool isSuccessful)
        {
            if (isSuccessful)
            {
                IsLaunched = true;
                SetState(InstanceState.Running);

                _appCore.MessageService.Success("InstanceLaunchedSuccessfulNotification", true, _instanceClient.Name);
            }
            else
            {
                SetState(InstanceState.Default);
                _appCore.MessageService.Error("InstanceLaunchedUnsuccessfulNotification", true, _instanceClient.Name);
                IsLaunched = false;
            }

            IsLaunching = false;
            GameLaunchCompleted?.Invoke(isSuccessful);
            DataChanged?.Invoke();
        }


        #endregion Constructors


        #region Public Methods


        #region Download


        /// <summary>
        /// Запустить скачивание сборки. 
        /// Перед выполнением отрабатывает эвент DownloadStartedEvent.
        /// При успешном выполнение отрабатывает эвент DownloadCompleted.
        /// </summary>
        /// <param name="version">Версия сборки сборки (not minecraft)</param>
        public void Download(string? version = null)
        {
            if (!InLibrary)
            {
                AddToLibrary();
            }

            IsDownloading = true;

            Lexplosion.Runtime.TaskRun(() =>
            {
                _instanceClient.Update(version);
            });
            DownloadStarted?.Invoke();

            OnPropertyChanged(nameof(AnyProcessActive));
            DataChanged?.Invoke();
        }

		/// <summary>
		/// Добавить сборку в библиотеку.
		/// </summary>
		public void AddToLibrary()
		{
			_clientsManager.AddToLibrary(_instanceClient);
			GlobalAddedToLibrary?.Invoke(this);
			AddedToLibraryEvent?.Invoke(this);
			DataChanged?.Invoke();
		}

        /// <summary>
        /// Отменить текущие скачивание
        /// </summary>
        public void CancelDownload()
        {
            IsDownloading = false;
            Runtime.TaskRun(() => _instanceClient.CancelDownload());

            if (State != InstanceState.DownloadCanceling)
                SetState(InstanceState.DownloadCanceling);

            DownloadCancelling = true;
            OnPropertyChanged(nameof(IsDownloading));

            DownloadCanceled?.Invoke();
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Обновить сборку.
        /// </summary>
        public void Update()
        {
            Runtime.TaskRun(() =>
            {
                _instanceClient.Update();
                DataChanged?.Invoke();
            });
        }


        #endregion Download


        #region  Launch


        /// <summary>
        /// Запустить сборку. При успешном выполнении отрабатывает эвент Launched.
        /// </summary>
        public void Run()
        {
            // TODO: !!!IMPORTANT сделать бесконечный progressbar и вместо описания фразу "Идет авторизация запускаемого аккаунта..."
            var launchAcc = Account.LaunchAccount;

            // если запускамый аккаунт отсутствует, то выкидываем уведомление об этом 
            if (launchAcc == null)
            {
                _appCore.MessageService.Warning("$\"Не удалось запустить {Name}\", \"Запускаемый аккаунт не выбран, он требуется для запуска клиента.\"");
                return;
            }

            if (!launchAcc.IsAuthed)
            {
                Runtime.TaskRun(() =>
                {
                    var authResult = launchAcc.Auth();

                    if (authResult == AuthCode.Successfully)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (!IsLaunching)
                            {
                                IsLaunching = true;
                                Runtime.TaskRun(() => _instanceClient.Run());
                            }
                            GameLaunched?.Invoke();
                        });
                    }
                    else
                    {
                        _appCore.MessageService.Error($"Ошибка аккаунта: {authResult}");
                    }
                });
                return;
            }

            if (!IsLaunching)
            {
                IsLaunching = true;
                Runtime.TaskRun(() => _instanceClient.Run());
            }
            GameLaunched?.Invoke();
        }

        /// <summary>
        /// Закрыть сборку. При успешном выполнении отрабатывает эвент Closed.
        /// </summary>
        public void Close()
        {
            _instanceClient.StopGame();

            IsLaunching = false;
            IsLaunched = false;
            IsDownloading = false;

            GameClosed?.Invoke();
            DataChanged?.Invoke();
            State = InstanceState.Default;
        }

        #endregion Launch


        public bool CheckInstanceClient(InstanceClient instanceClient)
        {
            return _instanceClient == instanceClient;
        }

        /// <summary>
        /// Открыть папку с игрой.
        /// </summary>
        public void OpenFolder()
        {
            System.Diagnostics.Process.Start("explorer", _instanceClient.GetDirectoryPath());
        }

        /// <summary>
        /// Открыть веб-страницу для сборки, если она есть.
        /// </summary>
        public void GoToWebsite()
        {
            try
            {
                System.Diagnostics.Process.Start(_instanceClient.WebsiteUrl);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Открыть модальное окна для экспорта сборки.
        /// </summary>
        public void Export()
        {
            _exportFunc(_instanceClient);
        }

		/// <summary>
		/// Удалять сборку.
		/// Если сборка только добавлена в библиотеку (не установлена), то сборка будет удалена из библиотеки.
		/// Если сборка установлена, то она будет удалена полностью.
		/// </summary>
		public void Delete()
		{
			if (_clientsManager.LibrarySize == 1) return;

			_appCore.ModalNavigationStore.Open(new ConfirmActionViewModel(
					_appCore.Resources("DeletingInstance") as string,
					string.Format(_appCore.Resources("DeletingInstanceDescription") as string, Name),
					_appCore.Resources("YesIWantDeleteInstance") as string,
					(obj) =>
					{
						// TODO: ПОДПИСАТЬСЯ НА эвент и удалять через него.
						GlobalDeletedEvent?.Invoke(this);
						_clientsManager.DeleteFromLibrary(_instanceClient);
						DeletedEvent?.Invoke(this);
					}));
		}

        public Logic.Settings Settings
        {
            get => _instanceClient.GetSettings(); set
            {
                _instanceClient.SaveSettings(value);
            }
        }

        /// <summary>
        /// Задает новые данные о сборке
        /// </summary>
        /// <param name="baseInstance">Новые данные о сборке</param>
        /// <param name="logoPath">Путь до лого</param>
        public void ChangeOverviewParameters(BaseInstanceData baseInstance, string logoPath = null)
        {
            _instanceClient.ChangeParameters(baseInstance, logoPath);
            DataChanged?.Invoke();
            LogoChanged?.Invoke();
        }

        /// <summary>
        /// Загружает данные о сборке которые необходимы только профилю
        /// </summary>
        public void PrepareDataForProfile()
        {
            Runtime.TaskRun(() =>
            {
                PageData = _instanceClient.GetFullInfo();
                PageData.TotalDownloads.ToString();
                OnPropertyChanged(nameof(TotalDownloads));
            });
        }


        public bool Equals(InstanceClient other)
        {
            if (other == null || other == null)
            {
                return false;
            }

            return _instanceClient == other;
        }


        /// <summary>
        /// Добавить сервер в игру
        /// </summary>
        public void AddServer(MinecraftServerInstance server, bool isAutoLogin)
        {
            _instanceClient.AddGameServer(server, isAutoLogin);
        }

        /// <summary>
        /// Получить все версии сборки
        /// </summary>
        public IEnumerable<InstanceVersion> GetInstanceVersions()
        {
            return _instanceClient.GetVersions();
        }

        public void CancelShareInstanceDownloading()
        {
            InstanceDistribution.CancelDownload();
        }

        public void CancelImport()
        {
            if (ImportData.HasValue) 
            {
                ImportData.Value.CancelImport();
                DeletedEvent?.Invoke(this);
            }
        }

        #endregion Public Methods


        #region Private Methods


        #region Handlers





        #endregion Handlers


        private void OnDownloadProgressChanged(StageType stageType, ProgressHandlerArguments progressHandlerArguments)
        {
            // Данный код вызывается при скачивании и запуске.
            // Поэтому мы будет при StageType.Prepare изменять состояние клиента на Preparing; 
            // Иначе устанавливаем состояние клиента Downloading;
            if (stageType == StageType.Prepare && State != InstanceState.Preparing)
            {
                SetState(InstanceState.Preparing);
                IsPrepare = true;
            }
            else if (stageType != StageType.Prepare)
            {
                SetState(State = InstanceState.Downloading);
                if (IsPrepare)
                {
                    IsPrepare = false;
                }
            }

            if (DownloadingData == null)
            {
                DownloadingData = new();
                OnPropertyChanged(nameof(DownloadingData));
            }

            DownloadingData.Stage = stageType;
            DownloadingData.CurrentStage = progressHandlerArguments.Stage;
            DownloadingData.TotalStages = progressHandlerArguments.StagesCount;
            DownloadingData.FilesCounts = progressHandlerArguments.FilesCount;
            DownloadingData.TotalFiles = progressHandlerArguments.TotalFilesCount;
            DownloadingData.Percentages = progressHandlerArguments.Procents;

            DownloadProgressChanged?.Invoke(stageType, progressHandlerArguments);
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Завершнение скачивание файлов
        /// </summary>
        /// <param name="init"></param>
        /// <param name="errors"></param>
        /// <param name="isRun"></param>
        private void OnDownloadCompleted(InstanceInit init, IEnumerable<string> errors, bool isRun)
        {
            IsDownloading = false;

            if (IsPrepare)
            {
                IsPrepare = false;
            }

            if (isRun && init == InstanceInit.Successful)
            {
                SetState(InstanceState.Launching);
            }
            else
            {
                SetState(InstanceState.Default);
                IsLaunched = false;
                IsLaunching = false;
            }

            DownloadingData = null;
            OnPropertyChanged(nameof(DownloadingData));
            OnPropertyChanged(nameof(IsDownloading));
            DownloadComplited?.Invoke(init, errors, isRun);
            DataChanged?.Invoke();

            if (ImportData.HasValue) 
            {
                return;
            }

            switch (init)
            {
                case InstanceInit.Successful:
                    {
                        _appCore.MessageService.Success("Instance_HasBeenInstalledSuccessful", true, Name);
                    }
                    break;
                case InstanceInit.DownloadFilesError:
                    {
                        // TODO: В будещем переделать ToastMessage на работу с ключами
                        var title = _appCore.Resources("FailedToDownloadSomeFiles") as string;
                        var notifyContent = _appCore.Resources("FailedToDownloadFollowingFiles:_") as string;
                        if (errors.Count() > 0)
                        {
                            notifyContent = string.Format(notifyContent, errors.Cast<object>().ToArray());
                        }

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.CurseforgeIdError:
                    {
                        var title = _appCore.Resources("CurseforgeErrorTitle") as string;
                        var notifyContent = _appCore.Resources("ExternalIdIncorrect") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.NightworldIdError:
                    {
                        var title = _appCore.Resources("NightWorldErrorTitle") as string;
                        var notifyContent = _appCore.Resources("ExternalIdIncorrect") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ServerError:
                    {
                        var title = _appCore.Resources("ServerError") as string;
                        var notifyContent = _appCore.Resources("FailedToGetDataFromServer") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.GuardError:
                    {
                        var title = _appCore.Resources("GuardErrorTitle") as string;
                        var notifyContent = _appCore.Resources("FileVerificationFailed") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.VersionError:
                    {
                        var title = _appCore.Resources("VersionErrorTitle") as string;
                        var notifyContent = _appCore.Resources("VersionVerificationFailed") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ForgeVersionError:
                    {
                        var title = _appCore.Resources("ForgeVersionErrorTitle") as string;
                        var notifyContent = _appCore.Resources("ModloaderVerificationFailed") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.GamePathError:
                    {
                        var title = _appCore.Resources("GamePathErrorTitle") as string;
                        var notifyContent = _appCore.Resources("InvalidGameDirectory") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ManifestError:
                    {
                        var title = _appCore.Resources("ManifestErrorTitle") as string;
                        var notifyContent = _appCore.Resources("FailedLoadInstanceManifest") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.JavaDownloadError:
                    {
                        var title = _appCore.Resources("JavaDownloadErrorTitle") as string;
                        var notifyContent = _appCore.Resources("TrySetCustomJavaPath") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.IsCancelled:
                    {
                        var title = _appCore.Resources("InstanceDownloadCanceledSuccessfully") as string;
                        var notifyContent = string.Format(_appCore.Resources("InstanceName:_") as string, Name);

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));

                        OnDownloadCanceled();
                        break;
                    }
                default:
                    {
                        var title = _appCore.Resources("UnknownErrorTitle") as string;
                        var notifyContent = _appCore.Resources("UnknownErrorTryRestartLauncher") as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
            }
        }

        private void OnLaunchStarted()
        {
            IsLaunching = true;
            DataChanged?.Invoke();
        }

        private void OnGameClosed()
        {
            IsLaunching = false;
            IsLaunched = false;

            GameClosed?.Invoke();
            SetState(InstanceState.Default);
            DataChanged?.Invoke();
        }


        ///
        /// <! -- Presentation Info -- !> 
        ///


        private void OnNameChanged()
        {
            OnPropertyChanged(nameof(Name));
            NameChanged?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnLogoChanged()
        {
            Logo = _instanceClient.Logo;
            OnPropertyChanged(nameof(Logo));
            LogoChanged?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnSummaryChanged()
        {
            OnPropertyChanged(nameof(Summary));
            SummaryChanged?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnDescriptionChanged()
        {
            OnPropertyChanged(nameof(Description));
            DescriptionChanged?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnStateClientChanged()
        {
            //StateChanged?.Invoke();
            OnPropertyChanged(string.Empty);
        }

        /// <summary>
        /// Отмена скачивания завершена
        /// </summary>
        private void OnDownloadCanceled()
        {
            SetState(InstanceState.Default);

            IsLaunching = false;
            IsLaunched = false;
            //IsDownloading = false;


            DownloadCanceled?.Invoke();
            DataChanged?.Invoke();
            DownloadCancelling = false;
            OnPropertyChanged(nameof(IsLaunching));
            OnPropertyChanged(nameof(IsLaunched));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(AnyProcessActive));
        }


        private void SetState(InstanceState state, [CallerMemberName] string methodName = null)
        {
            State = state;
        }

        /// <summary>
        /// Вызывается в момент когда InstanceClient получает статус завершенной/незавершенной версии версию;
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OnBuildFinished()
        {
            OnPropertyChanged(nameof(IsImporting));
            OnPropertyChanged(nameof(IsInstanceCompleted));
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Logo));
        }


        #endregion Private Methods
    }
}