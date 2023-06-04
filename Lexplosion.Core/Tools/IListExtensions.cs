using System.Collections.Generic;

namespace Lexplosion.Core.Tools
{
    public static class IListExtensions
    {
        /// <summary>
        /// Возвращает последний элемент списка. Если список пуст или равен null, то верне значение по умолчанию
        /// </summary>
        public static T GetLastElement<T>(this IList<T> collection)
        {
            if (collection == null || collection.Count == 0) return default(T);
            return collection[collection.Count - 1];
        }
    }
}
