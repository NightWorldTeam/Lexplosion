using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ModalVMs.InstanceTransfer
{
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

            _exportViewModel = new ExportViewModel(mainVM)
            {
                InstanceName = instanceClient.Name,
                IsFullExport = false,
                InstanceClient = instanceClient,
                UnitsList = instanceClient.GetPathContent()
            };
        }
    }
}
