using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.Logic.Management
{
	public static class CategoriesManager
	{
		private static ConcurrentDictionary<ProjectSource, IEnumerable<CategoryBase>> _modpacksCategoriesChache = new();
		private static ConcurrentDictionary<ValueTuple<ProjectSource, AddonType>, IEnumerable<CategoryBase>> _addonsCategoriesChache = new();

		public static IEnumerable<CategoryBase> GetModpackCategories(ProjectSource source)
		{
			if (_modpacksCategoriesChache.ContainsKey(source)) return _modpacksCategoriesChache[source];

			switch (source)
			{
				case ProjectSource.Curseforge:
					{
						var result = CurseforgeApi.GetCategories(CfProjectType.Modpacks);
						_modpacksCategoriesChache[source] = result;

						return result;
					}
				case ProjectSource.Modrinth:
					{
						var result = new List<CategoryBase>();
						List<ModrinthCategory> categories = ModrinthApi.GetCategories();

						result.Add(new SimpleCategory
						{
							Id = "-1",
							Name = "All",
							ClassId = "",
							ParentCategoryId = ""
						});

						foreach (ModrinthCategory category in categories)
						{
							if (category.ClassId == "modpack")
							{
								result.Add(category);
							}
						}

						_modpacksCategoriesChache[source] = result;

						return result;
					}
				default:
					return null;
			}
		}

		public static IEnumerable<CategoryBase> GetAddonsCategories(ProjectSource source, AddonType addonType)
		{
			var key = ValueTuple.Create(source, addonType);
			if (_addonsCategoriesChache.ContainsKey(key)) return _addonsCategoriesChache[key];

			switch (source)
			{
				case ProjectSource.Curseforge:
					{
						CfProjectType projectType = CfProjectType.Resourcepacks;
						if (addonType == AddonType.Maps)
						{
							projectType = CfProjectType.Maps;
						}
						else if (addonType == AddonType.Mods)
						{
							projectType = CfProjectType.Mods;
						}

						var result = CurseforgeApi.GetCategories(projectType)
							.OrderBy(i => i.Name);
						_addonsCategoriesChache[key] = result;

						return result;
					}
				case ProjectSource.Modrinth:
					{
						string projectType = "resourcepack";
						if (addonType == AddonType.Mods)
						{
							projectType = "mod";
						}
						else if (addonType == AddonType.Shaders)
						{
							projectType = "shader";
						}

						var result = new List<CategoryBase>();
						List<ModrinthCategory> categories = ModrinthApi.GetCategories();
						result.Add(new SimpleCategory
						{
							Id = "-1",
							Name = "All",
							ClassId = "",
							ParentCategoryId = ""
						});

						foreach (ModrinthCategory category in categories)
						{
							if (category.ClassId == projectType)
							{
								result.Add(category);
							}
						}

						_addonsCategoriesChache[key] = result;
						return result;
					}
				default:
					return null;
			}
		}

		public static IEnumerable<CategoryBase> FindAddonsCategoriesById(ProjectSource source, AddonType addonType, IEnumerable<string> ids)
		{
			if (ids == null) return null;

			IEnumerable<CategoryBase> allCategories = GetAddonsCategories(source, addonType);

			var res = new List<CategoryBase>();
			foreach (string id in ids)
			{
				var category = allCategories.FirstOrDefault(x => x.Id == id);
				if (category == null) continue;
				res.Add(category);
			}

			return res;
		}
	}
}
