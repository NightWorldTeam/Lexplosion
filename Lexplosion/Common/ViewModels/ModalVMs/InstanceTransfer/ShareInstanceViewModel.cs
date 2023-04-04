using Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ShareInstanceViewModel : ExportBase
    {
        public ShareInstanceViewModel(InstanceClient instanceClient) : base(instanceClient)
        {

        }

        protected override void Action()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                _instanceClient.Share(UnitsList);
            });
        }
    }
}
