using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System.Collections.Generic;

namespace Lexplosion.Logic.Management
{
    public static class CategoriesManager
    {
        public static IEnumerable<CategoryBase> GetModpackCategories(ProjectSource source)
        {
            switch (source)
            {
                case ProjectSource.Curseforge:
                    return CurseforgeApi.GetCategories(CfProjectType.Modpacks);
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

                        return result;
                    }
                default:
                    return null;
            }
        }

        public static IEnumerable<CategoryBase> GetAddonsCategories(ProjectSource source, AddonType addonType)
        {
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

                        return CurseforgeApi.GetCategories(projectType);
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

                        return result;
                    }
                default:
                    return null;
            }
        }
    }
}
