using Lexplosion.Gui.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class ShareInstanceViewModel : ExportBase
    {
        public ShareInstanceViewModel(InstanceClient instanceClient) : base(instanceClient)
        {

        }

        protected override void Action()
        {
            _instanceClient.Share(UnitsList);
        }
    }
}
