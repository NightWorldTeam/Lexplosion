using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class OverviewModel : VMBase
    {
        private readonly string _id;
        private readonly InstanceSource _source;
        public InstanceData InstanceData { get; private set; }

        public OverviewModel(string id, InstanceSource source)
        {
            _source = source;
            _id = id;
            InstanceData = ManageLogic.GetInstanceData(source, id);
        }
    }
}

