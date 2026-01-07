using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Network.Web.Models
{
    internal class DomainRouteData
    {
        public readonly string Domain;
        public DomainRouteMethod RouteMethod { get; }

        public IReadOnlyList<Proxy> Proxies { get => _proxies; }

        private ProxiesList _proxies = new();

        public void AddProxy(Proxy proxy, double delay)
        {
            _proxies.AddProxy(proxy, delay);
        }

        public void ProxyFailed(Proxy proxy)
        {
            _proxies.ProxyFailed(proxy);
        }

        public void RemoveProxy(Proxy proxy)
        {
            _proxies.RemoveProxy(proxy);
        }

        private class ProxiesList : IReadOnlyList<Proxy>
        {
            private readonly List<(double delay, Proxy proxy)> _list;
            private readonly object _locker = new();

            public Proxy this[int index]
            {
                get
                {
                    lock (_locker) return _list[index].proxy;
                }
            }

            public int Count
            {
                get
                {
                    lock (_locker) return _list.Count;
                }
            }

            public IEnumerator<Proxy> GetEnumerator()
            {
                lock (_locker)
                {
                    return new ProxyEnumerator(_list.ToArray());
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private void SortList()
            {
                _list.Sort(((double delay, Proxy proxy) x, (double delay, Proxy proxy) y) =>
                {
                    if (x.delay < y.delay) return -1;
                    if (x.delay > y.delay) return 1;
                    return 0;
                });
            }

            public void AddProxy(Proxy proxy, double delay)
            {
                lock (_locker)
                {
                    SortList();
                }
            }

            public void ProxyFailed(Proxy proxy)
            {
                lock (_locker)
                {
                    var index = _list.FindIndex(x => x.proxy.Equals(proxy));
                    if (index != -1)
                    {
                        var delay = _list[index].delay;
                        _list.RemoveAt(index);

                        if (delay < double.MaxValue) _list.Add((double.MaxValue, proxy));
                    }
                }
            }

            public void RemoveProxy(Proxy proxy)
            {
                lock (_locker)
                {
                    var index = _list.FindIndex(x => x.proxy.Equals(proxy));
                    if (index != -1)
                    {
                        _list.RemoveAt(index);
                    }
                }
            }
        }

        private class ProxyEnumerator : IEnumerator<Proxy>
        {
            private readonly IReadOnlyList<(double delay, Proxy proxy)> _list;
            private int _index = -1;
            private int _maxIndex = -1;

            public Proxy Current { get => _list[_index].proxy; }

            object IEnumerator.Current => Current;

            public ProxyEnumerator(IReadOnlyList<(double delay, Proxy proxy)> list)
            {
                _list = list;
                _maxIndex = _list.Count - 1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _index++;
                if (_index > _maxIndex) return false;

                return true;
            }

            public void Reset()
            {
                _index = -1;
                _maxIndex = _list.Count - 1;
            }
        }
    }
}
