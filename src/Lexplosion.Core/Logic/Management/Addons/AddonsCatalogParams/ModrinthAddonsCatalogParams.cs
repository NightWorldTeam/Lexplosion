using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management.Addons.AddonsCatalogParams
{
    internal class ModrinthAddonsCatalogParams : AddonsCatalogParamsBase<ModrinthProjectInfo, ModrinthSearchParams>
    {
        private readonly IModrinthFileServicesContainer _services;
        private readonly Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> _categoriesGetter;

        public ModrinthAddonsCatalogParams(AddonType type, ModrinthSearchParams sParams, IModrinthFileServicesContainer services, BaseInstanceData modpackInfo, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
            : base(type, sParams, modpackInfo)
        {
            _services = services;
            _categoriesGetter = categoriesGetter;
        }


        public override CatalogResult<ModrinthProjectInfo> GetCatalog()
        {
            return _services.MdApi.GetAddonsList(Type, SearchParams);
        }

        public override IPrototypeAddon CreateAddonPrototypeCreate(ModrinthProjectInfo addonInfo)
        {
            return new ModrinthAddon(ModpackInfo, addonInfo, _services, _categoriesGetter);
        }

        public override string GetAddonId(ModrinthProjectInfo addonInfo)
        {
            return addonInfo.ProjectId;
        }

        public override int GetDownloadCounts(ModrinthProjectInfo addonInfo)
        {
            return addonInfo.Downloads;
        }

        public override string GetLastUpdate(ModrinthProjectInfo addonInfo)
        {
            try
            {
                return DateTime.Parse(addonInfo.Updated).ToString("dd MMM yyyy");
            }
            catch
            {
                return string.Empty;
            }
        }

        public override string GetLogoUrl(ModrinthProjectInfo addonInfo)
        {
            return addonInfo.LogoUrl;
        }
    }
}
