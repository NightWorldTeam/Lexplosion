using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Addons
{
    public readonly struct AddonsCatalog
    {
        public IList<InstanceAddon> List { get; } = new List<InstanceAddon>();
        public int TotalHits { get; } = -1;

        public AddonsCatalog(IList<InstanceAddon> list, int totalHast)
        {
            List = list;
            this.TotalHits = totalHast;
        }
    }
}
