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
        // чтобы не создавать объект каждый вызов метода GetCatalog 
        private readonly IProjectCategory _modpacksAllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = ((int)CfProjectType.Modpacks).ToString(),
            ParentCategoryId = ((int)CfProjectType.Modpacks).ToString(),
        };

        public PrototypeInstance ContentManager { get => new CurseforgeInstance(); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new CurseforgeInstallManager(localId, updateOnlyBase, updateCancelToken);
        }

        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IEnumerable<IProjectCategory> categories, string searchFilter, CfSortField sortField, string gameVersion)
        {
            IProjectCategory category = _modpacksAllCategory;

            if (categories == null)
            {
                // получаем первый элемент списка
                using (var iter = categories.GetEnumerator()) 
                {
                    iter.MoveNext();
                    category = (IProjectCategory)iter.Current;
                }
            }

            var curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, category.Id, sortField, searchFilter, gameVersion);
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
                        GameVersion = new MinecraftVersion(instance.latestFilesIndexes[0].gameVersion),
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
