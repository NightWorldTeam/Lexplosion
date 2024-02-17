using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Global;
using System;

namespace Lexplosion.Logic.Management.Sources
{
    class FreeSource : IInstanceSource
    {
        private static object _locker = new();
        private static Dictionary<string, SourceMap> _maps = new();

        public string SourceId = null;
        public string SourceUrl = null;

        public FreeSource() { }
        public FreeSource(string sourceId, string sourceUrl)
        {
            SourceId = sourceId;
            SourceUrl = sourceUrl;
        }

        private static SourceMap GetSourceMap(FreeSourcePlatformData infoData)
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
                    sourceUrl = ToServer.HttpGet(LaunсherSettings.URL.Base + "api/freeSources/" + infoData.sourceId + "/mapUrl");
                    if (string.IsNullOrWhiteSpace(sourceUrl))
                    {
                        return null;
                    }
                }

                try
                {
                    string result = ToServer.HttpGet(sourceUrl);
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

        public PrototypeInstance ContentManager => new FreeInstance(GetSourceMap);

        public InstanceSource SourceType => InstanceSource.FreeSource;

        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IEnumerable<IProjectCategory> categories, string searchFilter, CfSortField sortField, string gameVersion)
        {
            return new List<InstanceInfo>();
        }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            var content = DataFilesManager.GetFile<FreeSourcePlatformData>(WithDirectory.DirectoryPath + "/instances/" + localId + "/instancePlatformData.json");
            return new FreeSourceInstanceInstallManager(GetSourceMap(content), localId, updateOnlyBase, updateCancelToken);
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
