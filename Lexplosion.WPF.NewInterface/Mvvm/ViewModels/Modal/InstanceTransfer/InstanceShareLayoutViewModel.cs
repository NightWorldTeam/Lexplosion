using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceShareLayoutViewModel : ViewModelBase
    {
        // TODO: Для чего этот класс?
        public IEnumerable<TabItemModel> Tabs { get; }


        public InstanceShareLayoutViewModel(InstanceSharesController controller, InstanceClient instanceClient)
        {
            // TODO: IMPORTANT
            var instanceShare = new InstanceShareViewModel(instanceClient, controller, (i) => { Runtime.DebugConsoleWrite("Я хз тут должно чет происходить или нет>?!", type: DebugWriteType.Warning); });
            var activeShares = new ActiveSharesViewModel(controller);

            Tabs = [
                new TabItemModel("Share", instanceShare, true), 
                new TabItemModel("ActiveShares", activeShares, false)
            ];
        }
    }
}
