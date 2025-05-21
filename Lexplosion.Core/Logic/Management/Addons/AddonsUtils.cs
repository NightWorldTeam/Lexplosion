using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Addons
{
	public static class AddonsUtils
	{
		public static string GetFolderName(AddonType addonType)
		{
			return addonType switch
			{
				AddonType.Mods => "mods",
				AddonType.Maps => "saves",
				AddonType.Shaders => "shaderpacks",
				AddonType.Resourcepacks => "resourcepacks",
				_ => string.Empty
			};
		}
	}
}
