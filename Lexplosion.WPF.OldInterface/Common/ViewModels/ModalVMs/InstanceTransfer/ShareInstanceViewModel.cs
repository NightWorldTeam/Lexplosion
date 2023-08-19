using Lexplosion.Common.Models.Controllers;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Controls;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ShareInstanceViewModel : ExportBase
    {
        #region Properties


        public IEnumerable<FileDistributionWrapper> ActiveShareProcess => ShareController.Instance.ActiveShareProcess;
        public bool IsAlreadySharing => _instanceClient.IsSharing;
        public ShareController ShareCtrl => ShareController.Instance;

        private bool _isPrepareToShare;
        public bool IsPrepareToShare
        {
            get => _isPrepareToShare; set
            {
                _isPrepareToShare = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _stopInstanceSharingCommand;
        public RelayCommand StopInstanceSharingCommand
        {
            get => _stopInstanceSharingCommand ?? (_stopInstanceSharingCommand = new RelayCommand(obj =>
            {
                var wrapper = (FileDistributionWrapper)obj;
                wrapper.FileDistribution?.Stop();
                ShareController.Instance.RemoveActiveShareProcess(wrapper);
                OnPropertyChanged(nameof(IsAlreadySharing));
            }));
        }


        #endregion Commands


        #region Constructors


        public ShareInstanceViewModel(InstanceClient instanceClient, DoNotificationCallback doNotification = null) : base(instanceClient, doNotification)
        {
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void Action()
        {
            IsPrepareToShare = true;
            Lexplosion.Runtime.TaskRun(() =>
            {
                var result = _instanceClient.Share(UnitsList, out FileDistributor fileDistribution);

                ExportResultHandler(result);

                App.Current.Dispatcher.Invoke(() =>
                {
                    var wrapper = new FileDistributionWrapper(_instanceClient.Name, fileDistribution);
                    ShareController.Instance.AddActiveShareProcess(wrapper);
                    OnPropertyChanged(nameof(IsAlreadySharing));
                    IsPrepareToShare = false;
                });
            });
        }


        #endregion Public & Protected Methods
    }
}
