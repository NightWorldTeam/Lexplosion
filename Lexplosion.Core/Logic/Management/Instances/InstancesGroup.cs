using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Instances
{
    public readonly struct InstancesGroup
    {
        public static InstancesGroup AllInstances { get; } = new InstancesGroup();
        public static InstancesGroup UngroupedInstances { get; } = new InstancesGroup("Ungrouped Instances", 1);

        public string Name { get; }
        public uint Id { get; }

        public InstancesGroup(string name, uint id)
        {
            Name = name;
            Id = id;
        }

        public InstancesGroup()
        {
            Name = "All instances";
            Id = 0;
        }
    }
}
