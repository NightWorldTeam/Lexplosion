using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
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
            return new ModrinthInstallManager(localId, updateOnlyBase, NetworkServicesManager.MinecraftInfo, updateCancelToken);
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

        public CatalogResult<InstanceInfo> GetCatalog(InstanceSource type, ISearchParams searchParams)
        {
            ModrinthSearchParams sParams;
            if (searchParams is ModrinthSearchParams)
            {
                sParams = (ModrinthSearchParams)searchParams;
            }
            else
            {
                sParams = new ModrinthSearchParams();
            }

            CatalogResult<ModrinthCtalogUnit> catalogResult = ModrinthApi.GetInstances(sParams.PageSize, sParams.PageIndex, sParams.Categories, sParams.SortFieldString, sParams.SearchFilter, sParams.GameVersion);
            var result = new List<InstanceInfo>(sParams.PageSize);

            foreach (var instance in catalogResult.Collection)
            {
                var _categories = ParseCategories(instance.Categories);

                result.Add(new InstanceInfo()
                {
                    Name = instance.Title,
                    Author = instance.Author,
                    Categories = _categories,
                    Summary = instance.Summary,
                    Description = instance.Summary,
                    GameVersion = new MinecraftVersion(instance.GameVersions[instance.GameVersions.Count - 1]),
                    WebsiteUrl = "https://modrinth.com/modpack/" + instance.Slug,
                    LogoUrl = instance.LogoUrl,
                    ExternalId = instance.ProjectId
                });
            }

            return new(result, catalogResult.TotalCount);
        }

        public InstancePlatformData CreateInstancePlatformData(string externalId, string localId, string instanceVersion)
        {
            return new InstancePlatformData
            {
                id = externalId,
                instanceVersion = instanceVersion,
            };
        }

        public InstanceSource SourceType { get => InstanceSource.Modrinth; }
    }
}
