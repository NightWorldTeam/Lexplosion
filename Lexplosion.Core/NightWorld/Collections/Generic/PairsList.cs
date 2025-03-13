using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightWorld.Collections.Generic
{
	public class PairsList<T, U>
	{
		private struct ValuePair
		{
			public T Value;
			public U Pair;

			public override bool Equals(object obj)
			{
				return Value.Equals(obj);
			}

			public override int GetHashCode()
			{
				if (Value == null) return 0;
				return Value.GetHashCode();
			}
		}

		public struct PairHandler<TValue, TKey> : IDictionary<TValue, TKey>
		{
			public TKey this[TValue key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ICollection<TValue> Keys => throw new NotImplementedException();

			public ICollection<TKey> Values => throw new NotImplementedException();

			public int Count => throw new NotImplementedException();

			public bool IsReadOnly => throw new NotImplementedException();

			public void Add(TValue key, TKey value)
			{
				throw new NotImplementedException();
			}

			public void Add(KeyValuePair<TValue, TKey> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<TValue, TKey> item)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(TValue key)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<TValue, TKey>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<KeyValuePair<TValue, TKey>> GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public bool Remove(TValue key)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<TValue, TKey> item)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(TValue key, out TKey value)
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		private HashSet<ValuePair> _storage = new();

		public PairHandler<T, U> LeftColumn { get; private set; }
		public PairHandler<U, T> RightColumn { get; private set; }

		public PairsList()
		{
			
		}
	}
}
