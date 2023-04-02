using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.ViewModels.ModalVMs.InstanceTransfer
{
    // TODO: для чего данный класс?
    public class InstanceTransferViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;
        private readonly VMBase _exportViewModel;
        private readonly VMBase _shareViewModel;

        public InstanceTransferViewModel(MainViewModel mainVM, InstanceClient instanceClient)
        {
            _mainViewModel = mainVM;
            _instanceClient = instanceClient;

            _exportViewModel = new ExportViewModel(instanceClient);
        }
    }
}