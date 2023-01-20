using System.Collections;
using System.Collections.Generic;

namespace Lexplosion.Tools.Immutable
{
    public class ImmutableArray<T> : IEnumerable<T>
    {
        private readonly T[] _items;


#nullable enable
        public ImmutableArray(T[]? array)
        {
            _items = array;
        }

        public ImmutableArray(List<T> list)
        {
            _items = new T[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                _items[i] = list[i];
            }
        }

        public List<T> ToList()
        {
            var result = new List<T>();

            foreach (var item in _items)
            {
                result.Add(item);
            }

            return result;
        }

        public T[] ToArray() => (T[])_items.Clone();

        public T this[int index]
        {
            get => _items[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _items.Length;

        public bool IsReadOnly => true;

        public bool Contains(T item) 
        {
            if ((object)item == null)
            {
                for (var i = 0; i < _items.Length; i++)
                {
                    if (item as object == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            else 
            {
                EqualityComparer<T> equalityComparer= EqualityComparer<T>.Default;
                for (var i = 0; i < _items.Length; i++) 
                {
                    if (equalityComparer.Equals(_items[i], item)) return true;
                }
                return false;
            }
        }
    }
}
