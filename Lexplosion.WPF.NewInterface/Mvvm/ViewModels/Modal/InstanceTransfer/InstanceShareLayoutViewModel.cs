using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceShareLayoutViewModel : ViewModelBase
    {
        public IEnumerable<TabItemModel> Tabs { get; }


        public InstanceShareLayoutViewModel(InstanceSharesController controller, InstanceClient instanceClient, NotifyCallback notify = null)
        {
            var instanceShare = new InstanceShareViewModel(instanceClient, controller, notify);
            var activeShares = new ActiveSharesViewModel(controller, notify);

            Tabs = [
                new TabItemModel("Share", instanceShare, true), 
                new TabItemModel("ActiveShares", activeShares, false)
            ];
        }
    }
}
