using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories.Groups
{
    /// <summary>
    /// Группа для вывода списка Modloaders в AddonRepostory 
    /// </summary>
    public class ModloaderGroup : AddonSearchParamsGroupBase
    {
        public ModloaderGroup(string header, IEnumerable<Modloader> modloaders, string? iconData = null) :
            base(header, modloaders.Select(e => (object)e).ToArray(), iconData)
        {
        }
    }
}
