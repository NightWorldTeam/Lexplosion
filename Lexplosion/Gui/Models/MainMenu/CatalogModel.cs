using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.MainMenu
{
    public class CatalogModel
    {
        public static List<string> CfSortToString { get; } = new List<string>()
        {
            ResourceGetter.GetString("featuredSortBy"),
            ResourceGetter.GetString("popularitySortBy"),
            ResourceGetter.GetString("lastUpdatedSortBy"),
            ResourceGetter.GetString("nameSortBy"),
            ResourceGetter.GetString("authorSortBy"),
            ResourceGetter.GetString("totalDownloadsFlSortBy"),
            ResourceGetter.GetString("categorySortBy"),
            ResourceGetter.GetString("gameVersionSortBy"),
        };
    }
}
