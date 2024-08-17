using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class ActiveSharesModel : ViewModelBase
    {
        private readonly InstanceSharesController _controller;
        private readonly NotifyCallback _nofity;


        public IReadOnlyCollection<DistributedInstance> DistributedInstances { get => _controller.ActiveShares; }
        public bool IsEmpty { get => DistributedInstances.Count == 0;}


        public ActiveSharesModel(InstanceSharesController controller, NotifyCallback notify = null)
        {
            _controller = controller;
            _nofity = notify;
            _controller.ActiveSharesListChanged += () =>
            {
                OnPropertyChanged(nameof(DistributedInstances));
                OnPropertyChanged(nameof(IsEmpty));
            };
        }


        public void StopShare(DistributedInstance distributedInstance)
        {
            _controller.RemoveActiveShare(distributedInstance);
        }
    }


    public sealed class ActiveSharesViewModel : ViewModelBase
    {
        public ActiveSharesModel Model { get; }


        #region  Commands


        private RelayCommand _stopSharingCommand;
        public ICommand StopSharingCommand
        {
            get => RelayCommand.GetCommand<DistributedInstance>(ref _stopSharingCommand, Model.StopShare);
        }


        #endregion Commands


        public ActiveSharesViewModel(InstanceSharesController controller, NotifyCallback notify = null)
        {
            Model = new ActiveSharesModel(controller, notify);
        }
    }
}
