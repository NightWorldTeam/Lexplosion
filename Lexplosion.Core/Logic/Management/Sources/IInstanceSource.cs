using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    public interface IInstanceSource
    {
        public PrototypeInstance ContentManager { get; }
        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken);
        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IEnumerable<IProjectCategory> categories, string searchFilter, CfSortField sortField, string gameVersion);
        public InstanceSource SourceType { get; }
    }
}
