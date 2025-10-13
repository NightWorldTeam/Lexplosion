using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using System;

namespace Lexplosion.Logic.Management.Addons.AddonsCatalogParams
{
    internal class CurseforgeAddonsCatalogParams : AddonsCatalogParamsBase<CurseforgeAddonInfo, CurseforgeSearchParams>
    {
        private readonly ICurseforgeFileServicesContainer _services;

        public CurseforgeAddonsCatalogParams(AddonType addonType, CurseforgeSearchParams searchParams, ICurseforgeFileServicesContainer services, BaseInstanceData modpackInfo)
            : base(addonType, searchParams, modpackInfo)
        {
            _services = services;
        }

        public override CatalogResult<CurseforgeAddonInfo> GetCatalog()
        {
            return _services.CfApi.GetAddonsList(Type, SearchParams);
        }

        public override IPrototypeAddon CreateAddonPrototypeCreate(CurseforgeAddonInfo addonInfo)
        {
            return new CurseforgeAddon(ModpackInfo, addonInfo, _services);
        }

        public override string GetAddonId(CurseforgeAddonInfo addonInfo)
        {
            return addonInfo.id;
        }

        public override int GetDownloadCounts(CurseforgeAddonInfo addonInfo)
        {
            return (int)addonInfo.downloadCount;
        }

        public override string GetLastUpdate(CurseforgeAddonInfo addonInfo)
        {
            try
            {
                return DateTime.Parse(addonInfo.dateModified).ToString("dd MMM yyyy");
            }
            catch
            {
                return string.Empty;
            }
        }

        public override string GetLogoUrl(CurseforgeAddonInfo addonInfo)
        {
            return addonInfo.logo?.url;
        }
    }
}
