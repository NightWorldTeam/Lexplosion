using Lexplosion.Gui.ModalWindow;
using Lexplosion.Logic.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public class DistributionInstance 
    {
        public string Name { get; set; }
    }

    public sealed class InstanceDistributionListViewModel : ModalVMBase
    {
        public void GetD() 
        {
            //var test = FileReceiver.GetDistributors();
            //var recv = test[0];
            //recv.SpeedUpdate += (double val) =>
            //{
            //    Runtime.DebugWrite("Speed " + val);
            //};
            //recv.ProcentUpdate += (double val) =>
            //{
            //    //Runtime.DebugWrite("ProcentUpdate " + val);
            //};

            //Runtime.TaskRun(() =>
            //{
            //    Instances.InstanceClient.Import(recv, out Instances.InstanceClient instanceClient);
            //    Runtime.DebugWrite("EMPORT " + instanceClient.Name);
            //});
        }
    }
}
