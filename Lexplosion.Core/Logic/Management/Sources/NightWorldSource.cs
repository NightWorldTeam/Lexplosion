using Lexplosion.Global;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    class NightWorldSource : IInstanceSource
    {
        public PrototypeInstance ContentManager { get => new NightworldInstance(); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new NightworldInstallManager(localId, updateOnlyBase, updateCancelToken);
        }

        public List<Objects.InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IProjectCategory categoriy, string searchFilter, CfSortField sortField, string gameVersion)
        {
            Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = NightWorldApi.GetInstancesList();
            var result = new List<Objects.InstanceInfo>();

            var i = 0;
            foreach (string nwModpack in nwInstances.Keys)
            {
                if (i < pageSize * (pageIndex + 1))
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
                            GameVersion = nwInstances[nwModpack].GameVersion,
                            WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + nwModpack,
                            LogoUrl = nwInstances[nwModpack].LogoUrl,
                            ExternalId = nwModpack
                        });
                    }
                }

                i++;
            }

            return result;
        }

        public InstanceSource SourceType { get => InstanceSource.Nightworld; }
    }
}
