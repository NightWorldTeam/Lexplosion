using Lexplosion.Common.Models.Controllers;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ShareInstanceViewModel : ExportBase
    {
        #region Properties


        public IEnumerable<FileDistributionWrapper> ActiveShareProcess => ShareController.Instance.ActiveShareProcess;


        private bool _isAlreadySharing;
        public bool IsAlreadySharing 
        {
            get => _isAlreadySharing; private set 
            {
                _isAlreadySharing = value;
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
                wrapper.FileDistribution.Stop();
                ShareController.Instance.RemoveActiveShareProcess(wrapper);
            }));
        }


        #endregion Commands


        #region Constructors


        public ShareInstanceViewModel(InstanceClient instanceClient) : base(instanceClient)
        {

        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void Action()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var fileDistribution = _instanceClient.Share(UnitsList);
                var wrapper = new FileDistributionWrapper(_instanceClient.Name, fileDistribution);
                App.Current.Dispatcher.Invoke(() => { 
                    ShareController.Instance.AddActiveShareProcess(wrapper);
                    IsAlreadySharing = true;
                });
            });
        }


        #endregion Public & Protected Methods
    }
}
