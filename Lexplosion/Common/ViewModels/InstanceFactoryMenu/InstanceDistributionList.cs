using Lexplosion.Common.ModalWindow;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Logic.FileSystem;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
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
