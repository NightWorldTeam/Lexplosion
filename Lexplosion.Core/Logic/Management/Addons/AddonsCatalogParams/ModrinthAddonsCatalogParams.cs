using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System;

namespace Lexplosion.Logic.Management.Addons.AddonsCatalogParams
{
	internal class ModrinthAddonsCatalogParams : AddonsCatalogParamsBase<ModrinthProjectInfo, ModrinthSearchParams>
	{
		public ModrinthAddonsCatalogParams(AddonType type, ModrinthSearchParams sParams, BaseInstanceData modpackInfo)
			: base(type, sParams, modpackInfo)
		{

		}


		public override CatalogResult<ModrinthProjectInfo> GetCatalog()
		{
			return ModrinthApi.GetAddonsList(Type, SearchParams);
		}

		public override IPrototypeAddon CreateAddonPrototypeCreate(ModrinthProjectInfo addonInfo)
		{
			return new ModrinthAddon(ModpackInfo, addonInfo);
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
