using Lexplosion.Logic;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Models.InstanceModel
{
    public class InstanceModelBase : ViewModelBase
    {
        private InstanceClient _instanceClient;


        #region Events


        public event Action StageChanged;

        public event Action NameChanged;
        public event Action GameVersionChanged;
        public event Action ModloaderChanged;
        public event Action LogoChanged;
        public event Action SummaryChanged;
        public event Action DescriptionChanged;

        public event Action<string> DownloadStartedEvent;
        public event Action<string> DownloadComplitedEvent;
        public event Action<string> DownloadCanceledEvent;
        public event Action<string> GameLaunchedEvent;
        public event Action<string> GameClosedEvent;
        public event Action<string> DeletedEvent;


        #endregion Events


        #region Properties


        private LaunchModel _launchModel;
        private DownloadModel _downloadModel;


        public string LocalId { get => _instanceClient.LocalId; }

        public string Name { get => _instanceClient.Name; }
        public string Author { get => _instanceClient.Author; }
        public string Summary { get => _instanceClient.Summary; }
        public string Description { get => _instanceClient.Description; }
        public byte[] Logo { get; }
        public IEnumerable<IProjectCategory> Tags { get; }
        public InstanceSource Type { get => _instanceClient.Type; }


        public bool IsLaunched { get; private set; }
        public bool IsInstalled { get => _instanceClient.IsInstalled; }
        public bool InLibrary { get => _instanceClient.InLibrary; }

        public BaseInstanceData InstanceData { get => _instanceClient.GetBaseData; }


        #endregion Properties


        #region Constructors


        public InstanceModelBase(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            _launchModel = new LaunchModel(instanceClient);
            _instanceClient.NameChanged += OnNameChanged;
            _instanceClient.LogoChanged += OnLogoChanged;
            _instanceClient.StateChanged += OnStateChanged;


            Logo = _instanceClient.Logo;
            Runtime.DebugWrite(Logo == null ? "Null" : Logo.Length.ToString());
            Tags = _instanceClient.Categories;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает сборку. При успешном выполнении отрабатывает эвент Launched.
        /// </summary>
        public void Run() 
        {
            _launchModel.Run();
            GameLaunchedEvent?.Invoke(_instanceClient.LocalId);
        }

        /// <summary>
        /// Закрывает сборку. При успешном выполнении отрабатывает эвент Closed.
        /// </summary>
        public void Close()
        {
            _launchModel.Close();
            GameClosedEvent?.Invoke(_instanceClient.LocalId);
        }


        /// <summary>
        /// Запускает скачивание сборки. 
        /// Перед выполнением отрабатывает эвент DownloadStartedEvent.
        /// При успешном выполнение отрабатывает эвент DownloadCompleted.
        /// </summary>
        public void Download()
        {
            _downloadModel.Download("");
            DownloadStartedEvent?.Invoke(_instanceClient.LocalId);
        }

        /// <summary>
        /// Отменяет скачивание сборки.
        /// </summary>
        public void CancelDownload()
        {
            _downloadModel.DownloadCancel();
            DownloadCanceledEvent?.Invoke(_instanceClient.LocalId);
        }

        /// <summary>
        /// Обновляет сборку.
        /// </summary>
        public void Update()
        {
            _instanceClient.Update();
        }


        /// <summary>
        /// Добавляет сборку в библиотеку.
        /// </summary>
        public void AddToLibrary()
        {
            _instanceClient.AddToLibrary();
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

        public void Export() 
        {
            
        }

        /// <summary>
        /// Удаляет сборку.
        /// Если сборка только добавлена в библиотеку (не установлена), то сборка будет удалена из библиотеки.
        /// Если сборка установлена, то она будет удалена полностью.
        /// </summary>
        public void Delete()
        {
            _instanceClient.Delete();
            DeletedEvent?.Invoke(_instanceClient.LocalId);
        }


        public void SaveSettings(Settings settings)
        {
            _instanceClient.SaveSettings(settings);
        }

        public Settings GetSettings()
        {
            return _instanceClient.GetSettings();
        }

        public void DisableOptifine()
        {

        }

        public void ChangeOverviewParameters(BaseInstanceData baseInstance, string logoPath)
        {
            _instanceClient.ChangeParameters(baseInstance, logoPath);
        }


        #endregion Public Methods


        #region Private Methods


        private void OnNameChanged()
        {
            OnPropertyChanged(nameof(Name));
            NameChanged?.Invoke();
        }

        private void OnLogoChanged()
        {
            OnPropertyChanged(nameof(Logo));
            LogoChanged?.Invoke();
        }

        private void OnSummaryChanged() 
        {
            OnPropertyChanged(nameof(Summary));
            SummaryChanged?.Invoke();
        }

        private void OnDescriptionChanged() 
        {
            OnPropertyChanged(nameof(Description));
            DescriptionChanged?.Invoke();
        }

        private void OnStateChanged() 
        {
            StageChanged?.Invoke();
        }


        #endregion Private Methods
    }
}
