using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Tools.Immutable
{
    public class ImmutableList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;

        public ImmutableList(List<T> list)
        {
            _list = list;
        }

        public List<T> ToList() => _list;

        public T this[int index]
        {
            get => _list[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
