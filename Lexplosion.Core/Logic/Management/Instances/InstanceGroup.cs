using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstanceGroup
    {
        #region static
        public static InstanceGroup AllInstances { get; } = new InstanceGroup();
        public static InstanceGroup UngroupedInstances { get; } = new InstanceGroup("UngroupedInstances", 1);

        private static HashSet<InstanceGroup> _allGroups = [InstanceGroup.AllInstances, InstanceGroup.UngroupedInstances];

        public static IReadOnlyCollection<InstanceGroup> GetAllGroups()
        {
            return _allGroups;
        }

        #endregion

        public string Name { get; private set; }
        public uint Id { get; private set; }

        public InstanceGroup(string name, uint id)
        {
            Name = name;
            Id = id;
        }

        public InstanceGroup()
        {
            Name = "AllInstances";
            Id = 0;
        }

        public void Save()
        {
            _allGroups.Add(this);
        }

        public override bool Equals(object obj)
        {
            InstanceGroup group = obj as InstanceGroup;
            if (group == null) return false;

            return group.Id == Id;
        }

        public override int GetHashCode() => (int)Id;
    }
}
