using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using System;

namespace Lexplosion.Logic.Management.Addons.AddonsCatalogParams
{
	internal class CurseforgeAddonsCatalogParams : AddonsCatalogParamsBase<CurseforgeAddonInfo, CurseforgeSearchParams>
	{
		public CurseforgeAddonsCatalogParams(AddonType addonType, CurseforgeSearchParams searchParams, BaseInstanceData modpackInfo)
			: base(addonType, searchParams, modpackInfo)
		{

		}

		public override CatalogResult<CurseforgeAddonInfo> GetCatalog()
		{
			return CurseforgeApi.GetAddonsList(Type, SearchParams);
		}

		public override IPrototypeAddon CreateAddonPrototypeCreate(CurseforgeAddonInfo addonInfo)
		{
			return new CurseforgeAddon(ModpackInfo, addonInfo);
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
