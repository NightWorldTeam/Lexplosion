using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public sealed class InstanceDistributionArgs
    {
        public InstanceDistributionArgs(FileReceiver fileReceiver, Action<InstanceInit> resultHandler, Action<InstanceDistribution> removeFileReceiver, LibraryController libraryController, InstanceSharesController instanceSharesController)
        {
            FileReceiver = fileReceiver;
            ResultHandler = resultHandler;
            RemoveFileReceiver = removeFileReceiver;
            LibraryController = libraryController;
            InstanceSharesController = instanceSharesController;
        }

        /// <summary>
        /// Файл.
        /// </summary>
        public FileReceiver FileReceiver { get; }
        /// <summary>
        /// Обработчки результата
        /// </summary>
        public Action<InstanceInit> ResultHandler { get; }
        /// <summary>
        /// 
        /// </summary>
        public Action<InstanceDistribution> RemoveFileReceiver { get; }
        public LibraryController LibraryController { get; }
        public InstanceSharesController InstanceSharesController { get; }
    }

    public sealed class InstanceDistribution : ObservableObject
    {
        public event Action<InstanceInit> DownloadFinished;

        private readonly InstanceDistributionArgs _args;
        private readonly FileReceiver _receiver;
        private readonly Action<InstanceInit> _resultHandler;
        private InstanceClient _instanceClient;
        private readonly Action<InstanceDistribution> _removeFileReceiver;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


        #region Properties


        public string Id => _receiver.Id;
        public string Name { get => _receiver.Name; }
        public string Author { get => _receiver.OwnerLogin; }

        // TODO: после завершения скачивания нужно, чтобы перезапускать не пришлось
        private double _speed = 0.0000;
        public double Speed
        {
            get => _speed; private set
            {
                _speed = Math.Round(value, 3);
                OnPropertyChanged();
            }
        }

        private byte _percentages = 0;
        public byte Percentages
        {
            get => _percentages; private set
            {
                _percentages = value;
                OnPropertyChanged();
            }
        }

        private bool _isDownloadSuccessful;
        public bool IsDownloadSuccessful
        {
            get => _isDownloadSuccessful; private set
            {
                _isDownloadSuccessful = value;
                OnPropertyChanged();
            }
        }

        private StateType _instanceState = StateType.InQueue;
        public StateType InstanceState
        {
            get => _instanceState; private set
            {
                _instanceState = value;
                OnPropertyChanged();
            }
        }


        private bool _isDownloadStarted;
        public bool IsDownloadStarted
        {
            get => _isDownloadStarted; private set
            {
                _isDownloadStarted = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceDistribution(InstanceDistributionArgs args)
        {
            _args = args;
            _receiver = args.FileReceiver;
            _resultHandler = args.ResultHandler;
            _removeFileReceiver = args.RemoveFileReceiver;

            _receiver.ProcentUpdate += FileReceiver_ProcentUpdate;
            _receiver.SpeedUpdate += FileReceiver_SpeedUpdate;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Скачивание
        /// </summary>
        public void Download()
        {
            var dynamicStateHandler = new DynamicStateData<ImportInterruption, InterruptionType>();
			var importData = new ImportData(dynamicStateHandler.GetHandler, (init) => DownloadResultHandler(init));

            _instanceClient = _clientsManager.Import(_receiver, importData);
            _instanceClient.StateChanged += OnStateChanged;

            IsDownloadStarted = true;
            _args.LibraryController.Add(_instanceClient, this);

            var s = _args.LibraryController.Get(_instanceClient);
            s.DeletedEvent += OnDeletedInstance;
        }

        private void OnStateChanged(StateType state)
        {
            InstanceState = state; 
        }

        /// <summary>
        /// Отменяет скачивание раздаваемой сборки.
        /// </summary>
        public void CancelDownload()
        {
            _receiver.CancelDownload();
            IsDownloadSuccessful = false;
            IsDownloadStarted = false;
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Обновляет значение скорости скачивания данных.
        /// </summary>
        /// <param name="value">Текущаяя скорость</param>
        private void FileReceiver_SpeedUpdate(double value)
        {
            Speed = value;
        }

        /// <summary>
        /// Обновление процента прогресса.
        /// </summary>
        /// <param name="value"></param>
        private void FileReceiver_ProcentUpdate(double value)
        {
            Percentages = (byte)value;
        }

        /// <summary>
        /// Обрабатывает результат скачинвание раздачи сборки.
        /// </summary>
        /// <param name="result"></param>
        private void DownloadResultHandler(ClientInitResult clientInitResult)
        {
            var result = clientInitResult.State;
            DownloadFinished?.Invoke(result);
            IsDownloadStarted = false;
            if (result == InstanceInit.Successful)
            {
                IsDownloadSuccessful = true;
            }
            else
            {
                IsDownloadSuccessful = false;
                _removeFileReceiver(this);
            }

            _resultHandler.Invoke(result);
            switch (result)
            {
                case InstanceInit.Successful:
                    break;
                default:
                    _args.LibraryController.Remove(_instanceClient);
                    break;
            }
        }

        private void OnDeletedInstance(object obj)
        {
            InstanceState = StateType.InQueue;
            Percentages = 0;
            Speed = 0;
        }


        #endregion Private Methods
    }
}