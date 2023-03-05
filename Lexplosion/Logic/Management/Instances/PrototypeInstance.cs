using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Instances
{
    abstract class PrototypeInstance
    {
        public class Info
        {
            public string Name;
            public string Author;
            public IEnumerable<IProjectCategory> Categories;
            public string Summary;
            public string Description;
            public string GameVersion;
            public string WebsiteUrl;
            public string LogoUrl;
            public string ExternalId;
        }

        public abstract bool CheckUpdates(InstancePlatformData infoData, string localId);

        public abstract InstanceData GetFullInfo(string localId, string externalId);

        public abstract List<InstanceVersion> GetVersions(string externalId);

        public static List<Info> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IProjectCategory categoriy, string searchFilter, CfSortField sortField, string gameVersion)
        {
            switch (type)
            {
                case InstanceSource.Nightworld:
                    return NightworldInstance.GetCatalog(pageSize, pageIndex);
                case InstanceSource.Curseforge:
                    return CurseforgeInstance.GetCatalog(pageSize, pageIndex, categoriy, searchFilter, sortField, gameVersion);
                case InstanceSource.Modrinth:
                    return ModrinthInstance.GetCatalog(pageSize, pageIndex, categoriy, searchFilter, "", gameVersion);
                default:
                    return null;
            }
        }

        public static Info GetInstance(InstanceSource type, string instanceId)
        {
            switch (type)
            {
                case InstanceSource.Nightworld:
                    return NightworldInstance.GetInstance(instanceId);
                default:
                    return null;
            }
        }
    }
}
