using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
