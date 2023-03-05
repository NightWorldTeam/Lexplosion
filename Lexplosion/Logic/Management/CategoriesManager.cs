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
    static class CategoriesManager
    {
        public static IEnumerable<IProjectCategory> GetModpackCategories(ProjectSource source)
        {
            switch (source)
            {
                case ProjectSource.Curseforge:
                    return CurseforgeApi.GetCategories(CfProjectType.Modpacks);
                case ProjectSource.Modrinth:
                    {
                        var result = new List<ModrinthCategory>();
                        List<ModrinthCategory> categories = ModrinthApi.GetCategories();
                        foreach (ModrinthCategory category in categories)
                        {
                            if(category.ClassId == "modpack")
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
