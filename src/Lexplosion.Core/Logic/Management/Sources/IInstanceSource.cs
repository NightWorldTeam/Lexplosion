using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Threading;
using System.Threading.Tasks;

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
    /// <summary>
    /// Optional asynchronous catalog search without changing the installer interface.
    /// </summary>
    public interface IAsyncInstanceSource
    {
        Task<CatalogResult<InstanceInfo>> GetCatalogAsync(InstanceSource type, ISearchParams searchParams,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
