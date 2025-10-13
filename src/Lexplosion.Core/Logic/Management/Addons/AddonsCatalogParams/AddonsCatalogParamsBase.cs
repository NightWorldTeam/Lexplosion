using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Addons
{
    internal abstract class AddonsCatalogParamsBase<TAddonInfo, TSearchParams>
        where TAddonInfo : IAddonProjectInfo
        where TSearchParams : ISearchParams
    {

        public AddonType Type { get; }
        public TSearchParams SearchParams { get; }
        public BaseInstanceData ModpackInfo { get; }


        protected AddonsCatalogParamsBase(AddonType addonType, TSearchParams searchParams, BaseInstanceData modpackInfo)
        {
            Type = addonType;
            SearchParams = searchParams;
            ModpackInfo = modpackInfo;
        }


        /// <summary>
        /// Возвращает CatalogResult TAddonInfo
        /// </summary>
        public abstract CatalogResult<TAddonInfo> GetCatalog();

        public abstract IPrototypeAddon CreateAddonPrototypeCreate(TAddonInfo addonInfo);

        public abstract string GetAddonId(TAddonInfo addonInfo);

        public abstract int GetDownloadCounts(TAddonInfo addonInfo);

        public abstract string GetLastUpdate(TAddonInfo addonInfo);

        public abstract string GetLogoUrl(TAddonInfo addonInfo);
    }
}
