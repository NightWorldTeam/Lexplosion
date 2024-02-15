using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    internal class LocalSource : IInstanceSource
    {
        public PrototypeInstance ContentManager { get => new LocalInstance(); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new LocalInstallManager(localId, updateCancelToken);
        }

        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IEnumerable<IProjectCategory> categories, string searchFilter, CfSortField sortField, string gameVersion)
        {
            return null;
        }

        public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion)
        {
            return null;
        }

        public InstanceSource SourceType { get => InstanceSource.Local; }
    }
}
