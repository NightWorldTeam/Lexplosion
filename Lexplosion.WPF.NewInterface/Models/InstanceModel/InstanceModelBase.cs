using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Models.InstanceModel
{
    public class InstanceModelBase
    {
        private InstanceClient _instanceClient;


        #region Events


        public event Action<string> NameChanged;

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


        private string _name;
        public string Name 
        { 
            get => _name; set 
            {
                _name = value;
                //NameChanged?.Invoke(value);
            }
        }

        public string Author { get; protected set; }
        public string ShortDescription { get; protected set; }
        public string Description { get; protected set; }
        public byte[] Logo { get; protected set; }
        public IEnumerable<IProjectCategory> Tags { get; protected set; }


        public bool IsLaunched { get; private set; }
        public bool IsInstalled { get => _instanceClient.IsInstalled; }


        #endregion Properties


        #region Constructors


        public InstanceModelBase(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            Name = _instanceClient.Name;
            ShortDescription = _instanceClient.Summary;
            Author = _instanceClient.Author;
            Description = _instanceClient.Description;
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

        /// <summary>
        /// Удаляет сборку.
        /// Если сборка только добавлена в библиотеку (не установлена), то сборка будет удалена из библиотеке.
        /// Иначе если сборка установлена, то она будет удалена полностью.
        /// </summary>
        public void Delete()
        {
            _instanceClient.Delete();
            DeletedEvent?.Invoke(_instanceClient.LocalId);
        }


        #endregion Public Methods
    }
}
