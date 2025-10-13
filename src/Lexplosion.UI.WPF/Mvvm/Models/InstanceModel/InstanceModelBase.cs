using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Notifications;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Extensions;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel
{
    public class InstanceModelBase : ViewModelBase, IEquatable<InstanceClient>
    {
        public readonly Guid Id;
        private readonly InstanceClient _instanceClient;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;
        private readonly Action<InstanceClient> _exportFunc;
        private readonly Action<InstanceModelBase> _setRunningGame;
        private InstancesGroup _instancesGroup;

        private readonly AppCore _appCore;

        private Action _openAddonPage;

        private StateType _instanceState;


        #region Events


        /// <summary>
        /// Эвент для контроллеров для добавление в библиотеку.
        /// </summary>
        public static event Action<InstanceModelBase> GlobalAddedToLibrary;
        /// <summary>
        /// Эвент для контроллеров для удаление из библиотеки.
        /// </summary>
        public static event Action<InstanceModelBase> GlobalDeletedEvent;
        /// <summary>
        /// Эвент для контроллеров для удаление из группы.
        /// </summary>
        public static event Action<InstanceModelBase> GlobalGroupRemovedEvent;

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
        public event Action<StateType, ProgressHandlerArguments> DownloadProgressChanged;
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


        private readonly Action<InstanceClient, ImportData> _addToLibrary;


        #region Properties


        private StateType _state;
        public StateType State
        {
            get => _state; private set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        public InstanceClient InstanceClient { get => _instanceClient; }

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

        public bool IsImporting
        {
            get => !_instanceClient.IsComplete;
        }

        public MinecraftVersion GameVersion { get => _instanceClient.GameVersion ?? new MinecraftVersion("1.20.1"); }


        public BaseInstanceData BaseData
        {
            get => _instanceClient.GetBaseData;
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
                OnPropertyChanged(nameof(AnyProcessActive));
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


        public bool IsSelectedGroupDefault { get; private set; }
        /// <summary>
        /// Количество доступных групп сборок больше 1
        /// </summary>
        public bool AvailableGroupsForAdding { get => _clientsManager.GetExistsGroups().Count > 1; }


        #endregion Properties


        #region Constructors


        public InstanceModelBase(InstanceModelArgs instanceModelArgs)
        {
            Id = Guid.NewGuid();
            _appCore = instanceModelArgs.AppCore;

            _instanceClient = instanceModelArgs.InstanceClient;
            _exportFunc = instanceModelArgs.ExportFunc;
            InstanceDistribution = instanceModelArgs.InstanceDistribution;
            ImportData = instanceModelArgs.ImportData;
            _addToLibrary = instanceModelArgs.AddToLibrary;

            OnStateChanged(instanceModelArgs.InstanceClient.State);
            instanceModelArgs.InstanceClient.StateChanged += OnStateChanged;

            if (instanceModelArgs.Group != null)
            {
                IsSelectedGroupDefault = instanceModelArgs.Group.IsDefaultGroup;
                _instancesGroup = instanceModelArgs.Group;
            }

            if (InstanceDistribution != null)
            {
                InstanceDistribution.DownloadFinished += OnShareDownloadFinished;
                IsShareDownloading = true;
            }

            _instanceClient.NameChanged += OnNameChanged;
            _instanceClient.LogoChanged += OnLogoChanged;
            _instanceClient.DownloadHandler += OnDownloadProgressChanged;
            _instanceClient.BuildFinished += OnBuildFinished;

            _clientsManager.GroupAdded += OnInstancesGroupAdded;
            _clientsManager.GroupDeleted += OnInstancesGroupDeleted;

            Logo = _instanceClient.Logo;
            TotalDownloads = _instanceClient.DownloadCounts.LongToString();
            HasTotalDownloads = _instanceClient.HasDownloadCounts;
            var versionTag = new SimpleCategory { Name = GameVersion?.Id ?? "" };
            var tags = _instanceClient.Categories.ToList() ?? new List<CategoryBase>();
            tags.Insert(0, versionTag);
            Tags = tags;
        }

        private void OnStateChanged(StateType stageType)
        {
            Runtime.DebugWrite($"Id: {Id} | IC: {_instanceClient.GetHashCode()} | prev: {State} | now: {stageType}", color: ConsoleColor.Magenta);
            State = stageType;
            switch (stageType)
            {
                case StateType.Default:
                    IsShareDownloading = false;
                    IsPrepare = false;
                    IsDownloading = false;
                    IsLaunching = false;
                    IsLaunched = false;
                    DownloadCancelling = false;
                    OnPropertyChanged(_instanceClient, nameof(_instanceClient.IsInstalled));
                    break;
                case StateType.DownloadPrepare:
                    IsPrepare = true;
                    IsDownloading = true;
                    break;
                case StateType.DownloadClient:
                    IsPrepare = false;
                    IsDownloading = true;
                    break;
                case StateType.DownloadJava:
                    IsDownloading = true;
                    break;
                case StateType.DownloadInCancellation:
                    {
                        IsDownloading = false;
                        DownloadCancelling = true;

                        DownloadCanceled?.Invoke();
                        DataChanged?.Invoke();
                    }
                    break;
                case StateType.Launching:
                    IsDownloading = false;
                    break;
                case StateType.GameRunning:
                    IsDownloading = false;
                    IsLaunching = true;
                    break;
                default:
                    break;
            }
            OnPropertyChanged(string.Empty);
        }

        /// <summary>Add commentMore actions
        /// Завершнение скачивание файлов
        /// </summary>
        /// <param name="init"></param>
        /// <param name="errors"></param>
        /// <param name="isRun"></param>
        private void OnDownloadCompleted(InstanceInit init, IEnumerable<string> errors, bool isRun)
        {
            Runtime.DebugWrite(Id, color: ConsoleColor.Red);
            IsDownloading = false;

            if (IsPrepare)
            {
                IsPrepare = false;
            }

            if (!isRun && init != InstanceInit.Successful)
            {
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
                        var title = _appCore.Resources["FailedToDownloadSomeFiles"] as string;
                        var notifyContent = _appCore.Resources["FailedToDownloadFollowingFiles:_"] as string;
                        if (errors.Count() > 0)
                        {
                            notifyContent = string.Format(notifyContent, errors.Cast<object>().ToArray());
                        }

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.CurseforgeIdError:
                    {
                        var title = _appCore.Resources["CurseforgeErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["ExternalIdIncorrect"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.NightworldIdError:
                    {
                        var title = _appCore.Resources["NightWorldErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["ExternalIdIncorrect"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ServerError:
                    {
                        var title = _appCore.Resources["ServerError"] as string;
                        var notifyContent = _appCore.Resources["FailedToGetDataFromServer"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.GuardError:
                    {
                        var title = _appCore.Resources["GuardErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["FileVerificationFailed"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.VersionError:
                    {
                        var title = _appCore.Resources["VersionErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["VersionVerificationFailed"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ForgeVersionError:
                    {
                        var title = _appCore.Resources["ForgeVersionErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["ModloaderVerificationFailed"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.GamePathError:
                    {
                        var title = _appCore.Resources["GamePathErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["InvalidGameDirectory"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.ManifestError:
                    {
                        var title = _appCore.Resources["ManifestErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["FailedLoadInstanceManifest"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.JavaDownloadError:
                    {
                        var title = _appCore.Resources["JavaDownloadErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["TrySetCustomJavaPath"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
                case InstanceInit.IsCancelled:
                    {
                        var title = _appCore.Resources["InstanceDownloadCanceledSuccessfully"] as string;
                        var notifyContent = string.Format(_appCore.Resources["InstanceName:_"] as string, Name);

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));

                        OnDownloadCanceled();
                        break;
                    }
                default:
                    {
                        var title = _appCore.Resources["UnknownErrorTitle"] as string;
                        var notifyContent = _appCore.Resources["UnknownErrorTryRestartLauncher"] as string;

                        _appCore.NotificationService.Notify(new SimpleNotification(title, notifyContent, type: NotificationType.Error));
                    }
                    break;
            }
        }

        private void RunGame()
        {
            var isDownloaded = false;

            Action<StateType> stateHandler = (state)
                => isDownloaded = state == StateType.DownloadJava || state == StateType.DownloadClient;

            Runtime.TaskRun(() =>
            {
                _instanceClient.StateChanged += stateHandler;
                var result = _instanceClient.Run();
                _instanceClient.StateChanged -= stateHandler;
                _appCore.UIThread(() =>
                {
                    if (isDownloaded)
                    {
                        OnDownloadCompleted(result.InitResult.State, result.InitResult.DownloadErrors, true);
                    }

                    OnLaunchComplited(result.RunResult);
                });
            });
        }

        private void OnLaunchComplited(bool isSuccessful)
        {
            IsDownloading = false;
            IsPrepare = false;

            if (isSuccessful)
            {
                IsLaunched = true;
                _appCore.MessageService.Success("InstanceLaunchedSuccessfulNotification", true, _instanceClient.Name);
            }
            else
            {
                _appCore.MessageService.Error("InstanceLaunchedUnsuccessfulNotification", true, _instanceClient.Name);
                IsLaunched = false;
            }

            IsLaunching = false;
            GameLaunchCompleted?.Invoke(isSuccessful);
            DataChanged?.Invoke();
        }

        public void UpdateInstancesGroup(InstancesGroup group)
        {
            _instancesGroup = group;
            IsSelectedGroupDefault = group.IsDefaultGroup;
        }

        private void OnInstancesGroupDeleted(InstancesGroup group)
        {
            OnPropertyChanged(nameof(AvailableGroupsForAdding));
        }

        private void OnInstancesGroupAdded(InstancesGroup group)
        {
            OnPropertyChanged(nameof(AvailableGroupsForAdding));
        }

        /// <summary>
        /// Скачивание раздачи завершено
        /// </summary>
        private void OnShareDownloadFinished(InstanceInit obj)
        {
            IsShareDownloading = false;
        }

        private void OnSharingStateChanged()
        {
            OnPropertyChanged(nameof(IsShareDownloading));
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

            Update(version);

            OnPropertyChanged(nameof(AnyProcessActive));
            DownloadStarted?.Invoke();
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Добавить сборку в библиотеку.
        /// </summary>
        public void AddToLibrary()
        {
            _clientsManager.AddToLibrary(_instanceClient);
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Отменить текущие скачивание
        /// </summary>
        public void CancelDownload()
        {
            Runtime.TaskRun(() => _instanceClient.CancelDownload());
        }

        /// <summary>
        /// Обновить сборку.
        /// </summary>
        public void Update(string version = null)
        {
            IsPrepare = true;
            Runtime.TaskRun(() =>
            {
                var result = _instanceClient.Update(version);
                _appCore.UIThread(() =>
                {
                    OnDownloadCompleted(result.State, result.DownloadErrors, false);
                    DataChanged?.Invoke();
                });
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
                _appCore.MessageService.Warning("NoLaunchAccountDescription", true, Name);
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
                            }
                            RunGame();
                            GameLaunched?.Invoke();
                        });
                    }
                    else
                    {
                        _appCore.MessageService.Error(string.Format("AccountError", true, authResult));
                    }
                });
                return;
            }

            if (!IsLaunching)
            {
                IsLaunching = true;
                RunGame();
            }
            GameLaunched?.Invoke();
        }

        /// <summary>
        /// Закрыть сборку. При успешном выполнении отрабатывает эвент Closed.
        /// </summary>
        public void Close()
        {
            _instanceClient.StopGame();

            GameClosed?.Invoke();
            DataChanged?.Invoke();
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
            _appCore.ModalNavigationStore.Open(new ConfirmActionViewModel(
                    _appCore.Resources["DeletingInstance"] as string,
                    string.Format(_appCore.Resources["DeletingInstanceDescription"] as string, Name),
                    _appCore.Resources["YesIWantDeleteInstance"] as string,
                    (obj) =>
                    {
                        GlobalDeletedEvent?.Invoke(this);
                        Runtime.TaskRun(() =>
                        {
                            _clientsManager.DeleteFromLibrary(_instanceClient);
                        });
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

        public void CancelByImportData()
        {
            if (ImportData.HasValue)
            {
                ImportData.Value.CancelImport();
                DeletedEvent?.Invoke(this);
            }

            if (InstanceDistribution != null)
            {
                InstanceDistribution.CancelDownload();
            }
        }


        public void RemoveFromGroup()
        {
            _instancesGroup.RemoveInstance(_instanceClient);
            _instancesGroup.SaveGroupInfo();
            GlobalGroupRemovedEvent?.Invoke(this);
        }


        public void OpenInstanceToGroupsConfigurator()
        {
            var viewModel = new InstanceGroupsConfiguratorViewModel(this, _clientsManager);
            _appCore.ModalNavigationStore.Open(viewModel);
        }

        public void OpenCoping()
        {
            var viewModel = new InstanceCopyViewModel(_appCore, _clientsManager, this, _addToLibrary);
            _appCore.ModalNavigationStore.Open(viewModel);
        }


        #endregion Public Methods


        #region Private Methods


        private void OnDownloadProgressChanged(ProgressHandlerArguments progressHandlerArguments)
        {
            if (!IsDownloading)
            {
                IsDownloading = true;
                DownloadStarted?.Invoke();
                DataChanged?.Invoke();
            }

            if (DownloadingData == null)
            {
                DownloadingData = new();
                OnPropertyChanged(nameof(DownloadingData));
            }

            DownloadingData.Stage = State;
            DownloadingData.CurrentStage = progressHandlerArguments.Stage;
            DownloadingData.TotalStages = progressHandlerArguments.StagesCount;
            DownloadingData.FilesCounts = progressHandlerArguments.FilesCount;
            DownloadingData.TotalFiles = progressHandlerArguments.TotalFilesCount;
            DownloadingData.Percentages = progressHandlerArguments.Procents;

            DownloadProgressChanged?.Invoke(_instanceState, progressHandlerArguments);
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

        /// <summary>
        /// Отмена скачивания завершена
        /// </summary>
        private void OnDownloadCanceled()
        {
            IsLaunching = false;
            IsLaunched = false;

            DownloadCanceled?.Invoke();
            DataChanged?.Invoke();
            DownloadCancelling = false;
            OnPropertyChanged(nameof(IsLaunching));
            OnPropertyChanged(nameof(IsLaunched));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(AnyProcessActive));
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

        internal void UpdateExportFunc()
        {
            throw new NotImplementedException();
        }


        #endregion Private Methods
    }
}