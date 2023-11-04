using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Instances
{
    class LocalInstance : PrototypeInstance
    {
        public override bool CheckUpdates(InstancePlatformData infoData, string localId)
        {
            return false;
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            VersionManifest instanceManifest = DataFilesManager.GetManifest(localId, false);
            InstanceAssets assetsData = DataFilesManager.GetFile<InstanceAssets>(WithDirectory.DirectoryPath + "/instances-assets/" + localId + "/assets.json");

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
                Images = WithDirectory.LoadMcScreenshots(localId)
            };
        }

        public override List<InstanceVersion> GetVersions(string externalId)
        {
            return null;
        }
    }
}
