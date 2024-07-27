using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories.Groups
{
    /// <summary>
    /// Группа для вывода списка CategoryWrapper в AddonRepostory 
    /// </summary>
    public class CategoryGroup : AddonSearchParamsGroupBase
    {
        public CategoryGroup(string header, IEnumerable<CategoryWrapper> categories, string iconData = null) : 
            base(header, categories.ToArray<object>(), iconData)
        {
        }
    }
}
