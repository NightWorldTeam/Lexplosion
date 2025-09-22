using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args
{
    public class MultiplayerLayoutArgs
    {
        public readonly Action OpenAccountFactory;
        public readonly SelectInstanceForServerArgs SelectInstanceForServerArgs;

        public MultiplayerLayoutArgs(Action openAccountFactory, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            OpenAccountFactory = openAccountFactory;
            SelectInstanceForServerArgs = selectInstanceForServerArgs;
        }
    }
}
