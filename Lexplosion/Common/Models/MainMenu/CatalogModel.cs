using Lexplosion.Tools;
using System.Collections.Generic;

namespace Lexplosion.Common.Models.MainMenu
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
