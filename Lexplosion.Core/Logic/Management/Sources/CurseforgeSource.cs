using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    class CurseforgeSource : IInstanceSource
    {
        public PrototypeInstance ContentManager { get => new CurseforgeInstance(); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new CurseforgeInstallManager(localId, updateOnlyBase, updateCancelToken);
        }

        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IProjectCategory categoriy, string searchFilter, CfSortField sortField, string gameVersion)
        {
            List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, categoriy.Id, sortField, searchFilter, gameVersion);
            var result = new List<InstanceInfo>();

            foreach (var instance in curseforgeInstances)
            {
                // проверяем версию игры
                if (instance.latestFilesIndexes != null && instance.latestFilesIndexes.Count > 0 && instance.latestFilesIndexes[0].gameVersion != null)
                {
                    string author = null;
                    if (instance.authors != null && instance.authors.Count > 0)
                    {
                        author = instance.authors[0].name;
                    }

                    result.Add(new InstanceInfo()
                    {
                        Name = instance.name,
                        Author = author,
                        Categories = instance.categories,
                        Summary = instance.summary,
                        Description = instance.summary,
                        GameVersion = instance.latestFilesIndexes[0].gameVersion,
                        WebsiteUrl = instance.links?.websiteUrl,
                        LogoUrl = instance.logo?.url,
                        ExternalId = instance.id
                    });
                }
            }

            return result;
        }

        public InstanceSource SourceType { get => InstanceSource.Curseforge; }
    }
}
