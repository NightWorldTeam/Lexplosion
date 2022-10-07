using System.Collections;
using System.Collections.Generic;

namespace Lexplosion.Tools.Immutable
{
    public class ImmutableArray<T> : IEnumerable<T>
    {
        private readonly T[] _array;

#nullable enable
        public ImmutableArray(T[]? array)
        {
            _array = array;
        }

        public ImmutableArray(List<T> list)
        {
            _array = new T[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                _array[i] = list[i];
            }
        }

        public List<T> ToList()
        {
            var result = new List<T>();

            foreach (var item in _array)
            {
                result.Add(item);
            }

            return result;
        }

        public T[] ToArray() => (T[])_array.Clone();

        public T this[int index]
        {
            get => _array[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
