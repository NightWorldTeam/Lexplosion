using System;
using System.Collections.ObjectModel;

namespace Lexplosion.Core.Extensions
{
    // TODO: Вынести потом в общее ядро к ObservableObject
    public static class ObservableCollectionExtensions
    {
        public static int FindIndex<T>(this ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                    return i;
            }

            return -1;
        }

        // Перегрузка с начальным индексом
        public static int FindIndex<T>(this ObservableCollection<T> collection, int startIndex, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (startIndex < 0 || startIndex >= collection.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = startIndex; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                    return i;
            }

            return -1;
        }

        // Перегрузка с диапазоном (startIndex и count)
        public static int FindIndex<T>(this ObservableCollection<T> collection, int startIndex, int count, Func<T, bool> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (startIndex < 0 || startIndex >= collection.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > collection.Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (predicate(collection[i]))
                    return i;
            }

            return -1;
        }
    }
}
