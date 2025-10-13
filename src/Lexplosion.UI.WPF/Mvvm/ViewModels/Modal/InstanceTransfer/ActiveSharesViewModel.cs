using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class ActiveSharesModel : ViewModelBase
    {
        private readonly InstanceSharesController _controller;


        public IReadOnlyCollection<DistributedInstance> DistributedInstances { get => _controller.ActiveShares; }
        public bool IsEmpty { get => DistributedInstances.Count == 0; }
        public string Author { get => Account.ActiveAccount.Login; }


        public ActiveSharesModel(InstanceSharesController controller)
        {
            _controller = controller;
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


    public sealed class ActiveSharesViewModel : ViewModelBase, ILimitedAccess
    {
        public ActiveSharesModel Model { get; }


        #region  Commands


        private RelayCommand _stopSharingCommand;
        public ICommand StopSharingCommand
        {
            get => RelayCommand.GetCommand<DistributedInstance>(ref _stopSharingCommand, Model.StopShare);
        }


        #endregion Commands


        public ActiveSharesViewModel(InstanceSharesController controller)
        {
            Model = new ActiveSharesModel(controller);
        }

        public bool HasAccess => throw new System.NotImplementedException();

        public void RefreshAccessData()
        {

        }
    }
}
