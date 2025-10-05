using System.Collections.Generic;
using System.Threading;
using System;
using Newtonsoft.Json;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Global;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.FileSystem.Services;

namespace Lexplosion.Logic.Management.Sources
{
	class FreeSource : IInstanceSource
	{
		private static object _locker = new();
		private static Dictionary<string, SourceMap> _maps = new();

		public string SourceId = null;
		public string SourceUrl = null;
		private readonly IPlatfromServiceContainer _services;

		public FreeSource() { }
		public FreeSource(string sourceId, string sourceUrl, IPlatfromServiceContainer services)
		{
			SourceId = sourceId;
			SourceUrl = sourceUrl;
			_services = services;
		}

		private SourceMap GetSourceMap(FreeSourcePlatformData infoData)
		{
			lock (_locker)
			{
				if (infoData?.IsValid() != true)
				{
					return null;
				}

				string sourceUrl;
				if (infoData.SourceUrlIsExists)
				{
					if (_maps.ContainsKey(infoData.sourceUrl))
					{
						return _maps[infoData.sourceUrl];
					}

					sourceUrl = infoData.sourceUrl;
				}
				else
				{
					sourceUrl = _services.WebService.HttpGet(LaunсherSettings.URL.Base + "api/freeSources/" + infoData.sourceId + "/mapUrl");
					if (string.IsNullOrWhiteSpace(sourceUrl))
					{
						return null;
					}
				}

				try
				{
					string result = _services.WebService.HttpGet(sourceUrl);
					if (result == null)
					{
						return null;
					}

					var data = JsonConvert.DeserializeObject<SourceManifest>(result);
					if (data == null) return null;

					var map = data.sourceMap;
					if (map == null) return null;

					Uri uri = new Uri(sourceUrl);
					map.BaseUrl = sourceUrl.ReplaceLast(uri.AbsolutePath, string.Empty);

					_maps[sourceUrl] = map;
					return map;
				}
				catch
				{
					return null;
				}
			}
		}

		public PrototypeInstance ContentManager => new FreeInstance(GetSourceMap, _services);

		public InstanceSource SourceType => InstanceSource.FreeSource;

		public CatalogResult<InstanceInfo> GetCatalog(InstanceSource type, ISearchParams searchParams)
		{
			return new();
		}

		public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
		{
			var content = _services.DataFilesService.GetExtendedPlatfromData<FreeSourcePlatformData>(localId);
			return new FreeSourceInstanceInstallManager(GetSourceMap(content), localId, updateOnlyBase, _services, updateCancelToken);
		}

		public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion)
		{
			return new FreeSourcePlatformData
			{
				id = externalId,
				sourceId = SourceId,
				sourceUrl = SourceUrl,
				instanceVersion = instanceVersion,
			};
		}
	}
}
