using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Instances
{
	class LocalInstance : PrototypeInstance
	{
		private readonly IFileServicesContainer _services;

		public LocalInstance(IFileServicesContainer services)
		{
			_services = services;
		}

		public override bool CheckUpdates(string localId)
		{
			return false;
		}

		public override InstanceData GetFullInfo(string localId, string externalId)
		{
			if (localId == null) return null;

			VersionManifest instanceManifest = _services.DataFilesService.GetManifest(localId, false);
			var assetsData = _services.DataFilesService.GetFile<InstanceAssetsFileDecodeFormat>(_services.DirectoryService.DirectoryPath + "/instances-assets/" + localId + "/assets.json");

			return new InstanceData
			{
				Source = InstanceSource.Local,
				Categories = new List<IProjectCategory>(),
				Description = assetsData?.Description,
				Summary = assetsData?.Summary,
				TotalDownloads = 0,
				GameVersion = instanceManifest?.version?.GameVersion,
				LastUpdate = null,
				Modloader = instanceManifest?.version?.ModloaderType ?? ClientType.Vanilla,
				Images = _services.DirectoryService.LoadMcScreenshots(localId)
			};
		}

		public override List<InstanceVersion> GetVersions(string externalId)
		{
			return null;
		}
	}
}
