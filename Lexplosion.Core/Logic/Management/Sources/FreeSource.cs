using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Sources
{
    class FreeSource : IInstanceSource
    {
        private static object _locker = new();
        private static Dictionary<string, SourceMap> _maps = new();

        private static SourceMap GetSourceMap(InstancePlatformData infoData)
        {
            lock (_locker)
            {
                var idData = LocalIdData.Load(infoData?.id);
                if (string.IsNullOrWhiteSpace(idData?.Id) || string.IsNullOrWhiteSpace(idData.SourceUrl))
                {
                    return null;
                }

                if (_maps.ContainsKey(idData.SourceUrl))
                {
                    return _maps[idData.SourceUrl];
                }

                string sourceUrl = idData.SourceUrl;
                string sourceId = null;
                if (sourceUrl.StartsWith("proxy,"))
                {
                    sourceId = sourceUrl.ReplaceFirst("proxy,", string.Empty);
                    sourceUrl = ToServer.HttpGet("http://192.168.0.110/api/freeSources/" + sourceId + "/mapUrl");
                }

                string result = ToServer.HttpGet(sourceUrl);
                if (result == null)
                {
                    return null;
                }

                var data = JsonConvert.DeserializeObject<SourceManifest>(result);
                if (data == null) return null;

                var map = data.sourceMap;
                if (map == null) return null;

                map.SourceId = sourceId;
                _maps[idData.SourceUrl] = map;

                return map;
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
            var content = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + localId + "/instancePlatformData.json");
            return new FreeSourceInstanceInstallManager(GetSourceMap(content), localId, updateOnlyBase, updateCancelToken);
        }
    }
}
