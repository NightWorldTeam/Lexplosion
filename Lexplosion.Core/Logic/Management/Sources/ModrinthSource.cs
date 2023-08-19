using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Sources
{
    class ModrinthSource : IInstanceSource
    {
        public PrototypeInstance ContentManager { get => new ModrinthInstance(); }

        public IInstallManager GetInstaller(string localId, bool updateOnlyBase, CancellationToken updateCancelToken)
        {
            return new ModrinthInstallManager(localId, updateOnlyBase, updateCancelToken);
        }

        private static List<ModrinthCategory> ParseCategories(List<string> data)
        {
            var categories = new List<ModrinthCategory>();
            if (data != null)
            {
                foreach (string category in data)
                {
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        categories.Add(new ModrinthCategory
                        {
                            Id = category
                        });
                    }
                }
            }

            return categories;
        }

        public List<InstanceInfo> GetCatalog(InstanceSource type, int pageSize, int pageIndex, IEnumerable<IProjectCategory> categories, string searchFilter, CfSortField sortField, string gameVersion)
        {
            string sortFiled;
            switch (sortField)
            {
                case CfSortField.TotalDownloads:
                    sortFiled = ModrinthApi.SearchFilters.Downloads;
                    break;
                case CfSortField.LastUpdated:
                    sortFiled = ModrinthApi.SearchFilters.Updated;
                    break;
                default:
                    sortFiled = ModrinthApi.SearchFilters.Relevance;
                    break;
            }

            List<ModrinthCtalogUnit> curseforgeInstances = ModrinthApi.GetInstances(pageSize, pageIndex, categories, sortFiled, searchFilter, gameVersion);
            var result = new List<InstanceInfo>(pageSize);

            foreach (var instance in curseforgeInstances)
            {
                var _categories = ParseCategories(instance.Categories);

                result.Add(new InstanceInfo()
                {
                    Name = instance.Title,
                    Author = instance.Author,
                    Categories = _categories,
                    Summary = instance.Summary,
                    Description = instance.Summary,
                    GameVersion = instance.GameVersions[instance.GameVersions.Count - 1],
                    WebsiteUrl = "https://modrinth.com/modpack/" + instance.Slug,
                    LogoUrl = instance.LogoUrl,
                    ExternalId = instance.ProjectId
                });
            }

            return result;
        }

        public InstanceSource SourceType { get => InstanceSource.Modrinth; }
    }
}
