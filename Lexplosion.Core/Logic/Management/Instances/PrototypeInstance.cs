using Lexplosion.Logic.Objects;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Instances
{
    public abstract class PrototypeInstance
    {
        public class Info
        {
            public string Name;
            public string Author;
            public IEnumerable<CategoryBase> Categories;
            public string Summary;
            public string Description;
            public string GameVersion;
            public string WebsiteUrl;
            public string LogoUrl;
            public string ExternalId;
        }

        public abstract bool CheckUpdates(string localId);

        public abstract InstanceData GetFullInfo(string localId, string externalId);

        public abstract List<InstanceVersion> GetVersions(string externalId);

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
