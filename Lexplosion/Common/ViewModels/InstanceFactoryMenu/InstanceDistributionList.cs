using Lexplosion.Common.ModalWindow;
using Lexplosion.Common.Models.Controllers;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.Views.Windows;
using Lexplosion.Logic.FileSystem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class InstanceSharingListViewModel : ModalVMBase
    {
        #region Properties


        public IEnumerable<InstanceDistribution> CurrentInstanceDistribution => ShareController.Instance.FileReceivers;


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
                    instance.CancelDownload();
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


        #region Public & Protected Methods


        public async void LoadInstanceDistribution()
        {
            var receivers = await Task.Run(() => FileReceiver.GetDistributors());

            foreach (var receiver in receivers)
            {
                if (!ShareController.Instance.IsReceiverContains(receiver.Id)) { 
                    ShareController.Instance.AddFileReceiver(new InstanceDistribution(receiver, DownloadResultHandler));
                }
            }
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void DownloadResultHandler(ImportResult importResult)
        {
            MainViewModel.ShowToastMessage("Download Sharing Instance", importResult.ToString());
        }


        #endregion Private Methods
    }
}
