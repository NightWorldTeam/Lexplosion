using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
	public interface IInstanceSource
    {
        public PrototypeInstance ContentManager { get; }
        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken);
        public CatalogResult<InstanceInfo> GetCatalog(InstanceSource type, ISearchParams searchParams);
        public InstanceSource SourceType { get; }
        public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion);
    }
}
