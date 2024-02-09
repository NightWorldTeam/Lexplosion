using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Tools;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management.Instances
{
    class FreeInstance : PrototypeInstance
    {
        private Func<InstancePlatformData, SourceMap> _urlGetter;

        public FreeInstance(Func<InstancePlatformData, SourceMap> urlGetter)
        {
            _urlGetter = urlGetter;
        }

        public override bool CheckUpdates(InstancePlatformData infoData, string localId)
        {
            try
            {
                string url = _urlGetter(infoData)?.ModpackVersionsListUrl?.Replace("${modpackId}", LocalIdData.Load(infoData?.id).Id);
                if (url == null)
                {
                    return false;
                }

                string result = ToServer.HttpPost(url);
                if (result == null)
                {
                    return false;
                }

                var version = JsonConvert.DeserializeObject<MidpackVersionsList>(result);
                if (string.IsNullOrWhiteSpace(version?.LatestVersion) || infoData.instanceVersion == version.LatestVersion)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            var idData = LocalIdData.Load(externalId);

            var content = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + localId + "/instancePlatformData.json");

            string url = _urlGetter(content)?.ModpackManifestUrl;

            ModpackManifest manifest = null;
            try
            {
                string result = ToServer.HttpGet(url.Replace("${modpackId}", idData?.Id));
                manifest = JsonConvert.DeserializeObject<ModpackManifest>(result);
            }
            catch { }

            var images = new List<byte[]>();
            if (manifest?.Images != null && manifest.Images.Count > 0)
            {
                var perfomer = new TasksPerfomer(3, manifest.Images.Count);
                foreach (var item in manifest.Images)
                {
                    perfomer.ExecuteTask(delegate ()
                    {
                        using (var webClient = new WebClient())
                        {
                            try
                            {
                                images.Add(webClient.DownloadData(item));
                            }
                            catch { }
                        }
                    });
                }

                perfomer.WaitEnd();
            }

            return new InstanceData
            {
                Source = InstanceSource.Local,
                Categories = new List<IProjectCategory>(),
                Description = manifest?.Description,
                Summary = manifest?.Summary,
                TotalDownloads = 0,
                GameVersion = manifest?.GameVersion,
                LastUpdate = null,
                Modloader = manifest?.Core ?? ClientType.Vanilla,
                Images = images
            };
        }

        public override List<InstanceVersion> GetVersions(string externalId)
        {
            return null;
        }
    }
}
