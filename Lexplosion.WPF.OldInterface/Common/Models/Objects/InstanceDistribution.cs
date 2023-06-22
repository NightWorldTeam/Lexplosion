using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using System;


namespace Lexplosion.Common.Models.Objects
{
    public enum InstanceDistributionStates
    {
        InQuque,
        Downloading,
        DownloadComplitedSuccessful,
        DownloadComplitedError,
    }

    public sealed class InstanceDistribution : VMBase
    {
        private readonly FileReceiver _receiver;
        private readonly Action<ImportResult> _resultHandler;
        private InstanceClient _instanceClient;


        #region Properties


        public string Id => _receiver.Id;
        public string Name { get; }
        public string Author { get; }

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

        private InstanceDistributionStates _instanceState = InstanceDistributionStates.InQuque;
        public InstanceDistributionStates InstanceState 
        {
            get => _instanceState; private set 
            {
                _instanceState = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Constructors


        public InstanceDistribution(FileReceiver fileReceiver, Action<ImportResult> resultHandler)
        {
            _receiver = fileReceiver;
            _resultHandler = resultHandler;
            Name = fileReceiver.Name;
            Author = fileReceiver.OwnerLogin;
            fileReceiver.ProcentUpdate += FileReceiver_ProcentUpdate;
            fileReceiver.SpeedUpdate += FileReceiver_SpeedUpdate;
        }


        #endregion Constructors
            

        #region Public Methods


        public void Download()
        {
            InstanceState = InstanceDistributionStates.Downloading;
            _instanceClient = InstanceClient.Import(_receiver, (result) => 
                {
                    if (result == ImportResult.Successful) 
                    {
                        InstanceState = InstanceDistributionStates.DownloadComplitedSuccessful;
                    }
                    DownloadResultHandler(result);
                }
            );
            MainModel.Instance.AddInstanceForm(_instanceClient, this).OnDeleted += OnDeletedInstance;
        }

        public void CancelDownload()
        {
            _receiver.CancelDownload();
            InstanceState = InstanceDistributionStates.InQuque;
        }


        #endregion Public Methods


        #region Private Methods


        private void FileReceiver_SpeedUpdate(double value)
        {
            Speed = value;
        }

        private void FileReceiver_ProcentUpdate(double value)
        {
            Percentages = (byte)value;
        }

        private void DownloadResultHandler(ImportResult result) 
        {
            _resultHandler.Invoke(result);
            switch (result) 
            {
                case ImportResult.Successful:
                    break;
                default:
                    MainModel.Instance.LibraryController.RemoveByInstanceClient(_instanceClient);
                    break;
            }
        }

        private void OnDeletedInstance() 
        {
            InstanceState = InstanceDistributionStates.InQuque;
            Percentages = 0;
            Speed = 0;
        }


        #endregion Private Methods
    }
}
