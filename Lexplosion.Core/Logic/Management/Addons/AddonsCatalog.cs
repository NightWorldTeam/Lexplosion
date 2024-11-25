using System.Collections.Generic;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Logic.Management.Addons
{
    public readonly struct AddonsCatalog
    {
        public IList<InstanceAddon> List { get; }
        public int TotalHits { get; } = -1;

        public AddonsCatalog(IList<InstanceAddon> list, int totalHast)
        {
            List = list ?? new List<InstanceAddon>();
            TotalHits = totalHast;
        }
    }
}
