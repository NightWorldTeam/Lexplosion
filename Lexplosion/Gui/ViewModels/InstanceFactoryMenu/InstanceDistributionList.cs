using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.Models;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class InstanceDistribution : VMBase
    {
        private readonly FileReceiver _receiver;
        private readonly Action<ImportResult> _resultHandler;

        public string Name { get; private set; }
        public string Author { get; private set; }

        private DistributionState _state = DistributionState.InProcess;
        public DistributionState State 
        { 
            get => _state; private set 
            {
                _state = value;
                OnPropertyChanged();
            } 
        }

        private double _speed = 0.0;
        public double Speed 
        { 
            get => _speed; private set 
            {
                _speed = value;
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

        public InstanceDistribution(FileReceiver fileReceiver, Action<ImportResult> resultHandler)
        {
            _receiver = fileReceiver;
            _resultHandler = resultHandler;
            Name = fileReceiver.Name;
            Author = fileReceiver.OwnerLogin;
            State = fileReceiver.GetState;
            fileReceiver.ProcentUpdate += FileReceiver_ProcentUpdate;
            fileReceiver.SpeedUpdate += FileReceiver_SpeedUpdate;
        }

        public void Download() 
        {
            State = DistributionState.InProcess;
            var instanceClient = InstanceClient.Import(_receiver, _resultHandler);
            MainModel.Instance.AddInstanceForm(instanceClient, this);
        }

        public void CancelDownloading() 
        {
            
        }

        private void FileReceiver_SpeedUpdate(double value)
        {
            Speed = value;
        }

        private void FileReceiver_ProcentUpdate(double value)
        {
            Percentages = (byte)value;
        }
    }

    public sealed class InstanceSharingListViewModel : ModalVMBase
    {
        #region Properties


        public ObservableCollection<InstanceDistribution> CurrentInstanceDistribution { get; private set; } = new ObservableCollection<InstanceDistribution>();


        #endregion Properities


        #region Commands


        private RelayCommand _refreshListCommand;
        public RelayCommand RefreshListCommand
        {
            get => _refreshListCommand ?? (_refreshListCommand = new RelayCommand(obj =>
            {
                LoadInstanceDistribution();
            }));
        }

        private RelayCommand _downloadInstanceCommand;
        public RelayCommand DownloadInstanceCommand
        {
            get => _downloadInstanceCommand ?? (_downloadInstanceCommand = new RelayCommand(obj => 
            {
                if (obj is InstanceDistribution)
                {
                    var instance = (InstanceDistribution)obj;
                    instance.Download();
                }
            }));
        }

        private RelayCommand _cancelDownloadingInstanceCommand;
        public RelayCommand CancelDownloadingInstanceCommand
        {
            get => _cancelDownloadingInstanceCommand ?? (_cancelDownloadingInstanceCommand = new RelayCommand(obj =>
            {
                if (obj is InstanceDistribution)
                {
                    var instance = (InstanceDistribution)obj;
                    instance.CancelDownloading();
                }
            }));
        }

        public override RelayCommand CloseModalWindowCommand => new RelayCommand(obj =>
        {
            ModalWindowViewModelSingleton.Instance.Close();
        });

        #endregion Commands


        #region Constructors


        public InstanceSharingListViewModel()
        {
            LoadInstanceDistribution();
        }


        #endregion Constructors


        #region Private Methods


        public async void LoadInstanceDistribution() 
        {
            var receivers = await Task.Run(() => FileReceiver.GetDistributors());
            CurrentInstanceDistribution.Clear();
            foreach (var receiver in receivers)
            {
                CurrentInstanceDistribution.Add(new InstanceDistribution(receiver, DownloadResultHandler));
            }
        }


        private void DownloadResultHandler(ImportResult importResult) 
        {
            MainViewModel.ShowToastMessage("Download Sharing Instance", importResult.ToString());
        }


        #endregion Private Methods
    }
}
