using Lexplosion.Global;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
	class NightWorldSource : IInstanceSource
	{
		public PrototypeInstance ContentManager { get => new NightworldInstance(); }

		public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
		{
			return new NightworldInstallManager(localId, updateOnlyBase, NetworkServicesManager.MinecraftInfo, updateCancelToken);
		}

		public CatalogResult<Objects.InstanceInfo> GetCatalog(InstanceSource type, ISearchParams searchParams)
		{
			Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = NightWorldApi.GetInstancesList();
			var result = new List<Objects.InstanceInfo>();

			var i = 0;
			foreach (string nwModpack in nwInstances.Keys)
			{
				if (i < searchParams.PageSize * (searchParams.PageIndex + 1))
				{
					// проверяем версию игры
					if (nwInstances[nwModpack].GameVersion != null)
					{
						result.Add(new Objects.InstanceInfo()
						{
							Name = nwInstances[nwModpack].Name,
							Author = nwInstances[nwModpack].Author,
							Categories = nwInstances[nwModpack].Categories,
							Summary = nwInstances[nwModpack].Summary,
							Description = nwInstances[nwModpack].Description,
							GameVersion = new MinecraftVersion(nwInstances[nwModpack].GameVersion),
							WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + nwModpack,
							LogoUrl = nwInstances[nwModpack].LogoUrl,
							ExternalId = nwModpack
						});
					}
				}

				i++;
			}

			return new(result, 1);
		}

		public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion)
		{
			return new InstancePlatformData
			{
				id = externalId,
				instanceVersion = instanceVersion,
			};
		}

		public InstanceSource SourceType { get => InstanceSource.Nightworld; }
	}
}
