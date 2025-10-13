using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    internal class LocalSource : IInstanceSource
    {
        private readonly IFileServicesContainer _services;

        public LocalSource(IFileServicesContainer services)
        {
            _services = services;
        }

        public PrototypeInstance ContentManager { get => new LocalInstance(_services); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new LocalInstallManager(localId, _services, updateCancelToken);
        }

        public CatalogResult<InstanceInfo> GetCatalog(InstanceSource type, ISearchParams searchParams)
        {
            return new();
        }

        public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion)
        {
            return null;
        }

        public InstanceSource SourceType { get => InstanceSource.Local; }
    }
}
