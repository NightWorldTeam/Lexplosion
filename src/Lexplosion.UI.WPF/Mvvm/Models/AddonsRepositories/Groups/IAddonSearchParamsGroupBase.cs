using Lexplosion.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories.Groups
{
    /// <summary>
    /// Класс созданный для вывода SearchParams по группам в AddonRepostory.
    /// WPF не поддерживает использование generic для DataType.
    /// </summary>
    public abstract class AddonSearchParamsGroupBase
    {
        public string Header { get; }
        /// <summary>
        /// Иконка группы
        /// </summary>
        public string? IconData { get; }
        /// <summary>
        /// Содержимое группы.
        /// </summary>
        public IReadOnlyCollection<object> Objects { get; }

        public AddonSearchParamsGroupBase(string header, IEnumerable<object> objects, string? iconData = null)
        {
            Header = header.FirstCharToUpper().Replace(" ", "");
            Objects = objects.ToArray();
            IconData = iconData;
        }
    }
}
