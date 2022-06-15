using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Instances
{
    class InstalledInstance
    {
        public string Name;
        public bool IsInstalled;
        public InstanceSource Type;
    }

    /// <summary>
    /// Содержит основную инфу о модпаке.
    /// </summary>
    public class BaseInstanceData
    {
        public InstanceSource Type;
        public string GameVersion;
        public string LocalId;
        public string ExternalId;
    }
}
