using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceShareLayoutViewModel : ViewModelBase
    {
        // TODO: Для чего этот класс?
        public IEnumerable<TabItemModel> Tabs { get; }


        public InstanceShareLayoutViewModel(AppCore appCore, InstanceSharesController controller, InstanceClient instanceClient)
        {
            var instanceShare = new InstanceShareViewModel(appCore, instanceClient, controller, (i) => { Runtime.DebugConsoleWrite("Я хз тут должно чет происходить или нет>?!", type: DebugWriteType.Warning); });
            var activeShares = new ActiveSharesViewModel(controller);

            Tabs = [
                new TabItemModel("Share", instanceShare, true), 
                new TabItemModel("ActiveShares", activeShares, false)
            ];
        }
    }
}
