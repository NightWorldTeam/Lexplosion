using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Instances
{
    abstract class PrototypeInstance
    {
        public class Info
        {
            public string Name;
            public string Author;
            public List<Category> Categories;
            public string Summary;
            public string Description;
            public string GameVersion;
            public string WebsiteUrl;
            public string LogoUrl;
            public string ExternalId;
        }

        public abstract bool CheckUpdates(InstancePlatformData infoData);

        public abstract InstanceData GetFullInfo(string localId, string externalId);

        public static List<Info> GetCatalog(InstanceSource type, int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter)
        {
            switch (type)
            {
                case InstanceSource.Nightworld:
                    return NightworldInstance.GetCatalog(pageSize, pageIndex, categoriy, searchFilter);
                case InstanceSource.Curseforge:
                    return CurseforgeInstance.GetCatalog(pageSize, pageIndex, categoriy, searchFilter);
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
                case InstanceSource.Curseforge:
                    return null;
                default:
                    return null;
            }
        }
    }
}
