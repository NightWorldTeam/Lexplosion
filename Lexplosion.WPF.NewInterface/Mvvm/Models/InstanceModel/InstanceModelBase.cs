using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public class InstancePresentationInfo 
    {
        
    }

    public enum InstanceModelStateProperty
    {
        Name,
        GameVersion,
        Modloader,
        Logo,
        Summary,
        Description,
        IsInstalled,
        IsDownloading,
        InLibrary,
        State
    }

    public class InstanceModelBase : ViewModelBase
    {
        private readonly InstanceClient _instanceClient;
        private readonly LaunchModel LaunchModel;
        private readonly DownloadModel DownloadModel;
        private readonly Action<InstanceClient> _exportFunc;

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
        public event Action<InstanceModelStateProperty> PropertyStateChanged;

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
            } 
        }

        private void SetState(InstanceState state, [CallerMemberName] string methodName = null) 
        {
            Runtime.DebugWrite(_state + " " + methodName);
            State = state;
        }

        public bool IsDownloading { get => DownloadModel.IsActive; }
        public string LocalId { get => _instanceClient.LocalId; }

        public string Name { get => _instanceClient.Name; }
        public string Author { get => _instanceClient.Author; }
        public string Summary { get => _instanceClient.Summary.Replace('\n', ' '); }
        public string Description { get => _instanceClient.Description; }
        public byte[] Logo { get; private set; }
        public IEnumerable<IProjectCategory> Tags { get; }
        public InstanceSource Source { get => _instanceClient.Type; }
        public string TotalDonwloads { get => _instanceClient.GetFullInfo().TotalDownloads.ToString(); }

        public bool IsLaunched { get; private set; }
        public bool IsInstalled { get => _instanceClient.IsInstalled; }
        public bool InLibrary { get => _instanceClient.InLibrary; }

        public MinecraftVersion GameVersion { get => _instanceClient.GameVersion; }

        public void UpdateInLibrary() 
        {
            OnPropertyChanged(nameof(InLibrary));
        }

        public BaseInstanceData InstanceData
        {
            get
            {
                var s = _instanceClient.GetBaseData;
                Runtime.DebugWrite(s.GetHashCode());
                return s;
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceModelBase(InstanceClient instanceClient, Action<InstanceClient> exportFunc)
        {
            _instanceClient = instanceClient;
            _exportFunc = exportFunc;
            LaunchModel = new LaunchModel(instanceClient);

            LaunchModel.LaunchStarted += OnLaunchStarted;
            LaunchModel.LaunchCompleted += OnLaunchCompleted;
            LaunchModel.Closed += OnGameClosed;

            DownloadModel = new DownloadModel(instanceClient);

            DownloadModel.Started += OnDownloadStarted;
            DownloadModel.Completed += OnDownloadCompleted;
            DownloadModel.Canceled += OnDownloadCanceled;
            DownloadModel.ProgressChanged += OnDownloadProgressChanged;

            _instanceClient.NameChanged += OnNameChanged;
            _instanceClient.LogoChanged += OnLogoChanged;
            _instanceClient.StateChanged += OnStateClientChanged;

            Logo = _instanceClient.Logo;
            Runtime.DebugWrite(Logo == null ? "Null" : Logo.Length.ToString());
            Tags = _instanceClient.Categories;
        }


        #endregion Constructors


        #region Public Methods


        public bool CheckInstanceClient(InstanceClient instanceClient) 
        {
            return _instanceClient == instanceClient;
        }


        /// <summary>
        /// Запускает сборку. При успешном выполнении отрабатывает эвент Launched.
        /// </summary>
        public void Run()
        {
            LaunchModel.Run();
            GameLaunched?.Invoke();
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Закрывает сборку. При успешном выполнении отрабатывает эвент Closed.
        /// </summary>
        public void Close()
        {
            LaunchModel.Close();
            GameClosed?.Invoke();
            DataChanged?.Invoke();
        }


        /// <summary>
        /// Запускает скачивание сборки. 
        /// Перед выполнением отрабатывает эвент DownloadStartedEvent.
        /// При успешном выполнение отрабатывает эвент DownloadCompleted.
        /// </summary>
        public void Download()
        {
            if (!InLibrary)
            {
                AddToLibrary();
            }

            DownloadModel.Download();
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Отменяет скачивание сборки.
        /// </summary>
        public void CancelDownload()
        {
            DownloadModel.DownloadCancel();
            if (State != InstanceState.DownloadCanceling)
                SetState(InstanceState.DownloadCanceling);

            DownloadCanceled?.Invoke();
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Обновляет сборку.
        /// </summary>
        public void Update()
        {
            _instanceClient.Update();
            DataChanged?.Invoke();
        }


        /// <summary>
        /// Добавляет сборку в библиотеку.
        /// </summary>
        public void AddToLibrary()
        {
            _instanceClient.AddToLibrary();
            GlobalAddedToLibrary?.Invoke(this);
            AddedToLibraryEvent?.Invoke(this);
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Открывает папку с игрой.
        /// </summary>
        public void OpenFolder()
        {
            Process.Start("explorer", _instanceClient.GetDirectoryPath());
        }

        /// <summary>
        /// Открывает веб страницу для сборки, если она есть.
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
        /// Вызывает открытие модального окна для экспорта сборки.
        /// </summary>
        public void Export()
        {
            _exportFunc(_instanceClient);
        }

        /// <summary>
        /// Удаляет сборку.
        /// Если сборка только добавлена в библиотеку (не установлена), то сборка будет удалена из библиотеки.
        /// Если сборка установлена, то она будет удалена полностью.
        /// </summary>
        public void Delete()
        {
            _instanceClient.Delete();
            // TODO: ПОДПИСАТЬСЯ НА эвент и удалять через него.
            GlobalDeletedEvent?.Invoke(this);
            DeletedEvent?.Invoke(this);
        }


        // TODO: Переделать настройки в getter/setter;

        /// <summary>
        /// Сохраняет настройки сборки.
        /// </summary>
        /// <param name="settings">Настройки которые требуется сохранить.</param>
        public void SaveSettings(Logic.Settings settings)
        {
            _instanceClient.SaveSettings(settings);
        }

        /// <summary>
        /// Возвращает настройки данного клиента.
        /// </summary>
        /// <returns>Экземляр класса Settings для данного клиента.</returns>
        public Logic.Settings GetSettings()
        {
            return _instanceClient.GetSettings();
        }


        /// <summary>
        /// Отключает оптифайн у данного клиента.
        /// </summary>
        public void DisableOptifine()
        {

        }

        public void ChangeOverviewParameters(BaseInstanceData baseInstance, string logoPath = null)
        {
            _instanceClient.ChangeParameters(baseInstance, logoPath);
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Установка addon'а
        /// </summary>
        public void InstallAddon()
        {

        }


        #endregion Public Methods


        #region Private Methods


        private void OnDownloadStarted()
        {
            DownloadStarted?.Invoke();
            DataChanged?.Invoke();
        }


        private void OnDownloadProgressChanged(StageType stageType, ProgressHandlerArguments progressHandlerArguments) 
        {
            // Данный код вызывается при скачивании и запуске.
            // Поэтому мы будет при StageType.Prepare изменять состояние клиента на Preparing; 
            // Иначе устанавливаем состояние клиента Downloading;
            if (stageType == StageType.Prepare && State != InstanceState.Preparing)
            {
                SetState(InstanceState.Preparing);
            }
            else if (stageType != StageType.Prepare && State != InstanceState.Downloading)
            {
                SetState(State = InstanceState.Downloading);
            }

            DownloadProgressChanged?.Invoke(stageType, progressHandlerArguments);
            DataChanged?.Invoke();
        }

        private void OnDownloadCanceled()
        {
            SetState(InstanceState.Default);
            DownloadCanceled?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnDownloadCompleted(InstanceInit init, IEnumerable<string> errors, bool isRun)
        {
            if (isRun)
            {
                SetState(InstanceState.Launching);
            }
            else 
            {
                SetState(InstanceState.Default);
            }

            DownloadComplited?.Invoke(init, errors, isRun);
            DataChanged?.Invoke();
        }


        private void OnLaunchStarted()
        {
            GameLaunched?.Invoke();
            DataChanged?.Invoke();
        }

        private void OnLaunchCompleted(bool isSuccessful)
        {
            if (isSuccessful)
            {
                SetState(InstanceState.Running);
            }
            GameLaunchCompleted?.Invoke(isSuccessful);
            DataChanged?.Invoke();
        }

        private void OnGameClosed()
        {
            GameClosed?.Invoke();
            Runtime.DebugWrite("Game Closed");
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
        }


        #endregion Private Methods
    }
}
