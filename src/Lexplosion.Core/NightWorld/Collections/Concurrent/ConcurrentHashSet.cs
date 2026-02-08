//===========================================================
//
// Код в этом файле спизжен с:
// https://github.com/axion-utils/ConcurrentHashSet
// Пространство имен изменено для удобства
//
//===========================================================


//===========================================================
//
// This source code has been modified from the following following location:
// https://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/ConcurrentDictionary.cs,4d0f4ac22dbeaf08
//
//===========================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NightWorld.Collections.Concurrent
{
	[DebuggerDisplay("Count = {Count}")]
	public class ConcurrentHashSet<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
	{
		/// <summary>
		/// Tables that hold the internal state of the <see cref="ConcurrentHashSet{T}"/>.
		///
		/// Wrapping the three tables in a single object allows us to atomically
		/// replace all tables at once.
		/// </summary>
		private class Tables
		{
			internal readonly Node[] buckets; // A singly-linked list for each bucket.
			internal readonly object[] locks; // A set of locks, each guarding a section of the table.
			internal volatile int[] countPerLock; // The number of elements guarded by each lock.

			internal Tables(Node[] buckets, object[] locks, int[] countPerLock)
			{
				this.buckets = buckets;
				this.locks = locks;
				this.countPerLock = countPerLock;
			}
		}

		/// <summary>
		/// A node in a singly-linked list representing a particular hash table bucket.
		/// </summary>
		private class Node
		{
			internal T value;
			internal volatile Node next;
			internal int hashcode;

			internal Node(T value, int hashcode, Node next)
			{
				this.value = value;
				this.next = next;
				this.hashcode = hashcode;
			}
		}

		private const int MAX_ARRAY_LENGTH = 0X7FEFFFFF;

		private volatile Tables _tables; // Internal tables of the hashset

		protected readonly IEqualityComparer<T> comparer;

		protected readonly bool growLockArray; // Whether to dynamically increase the size of the striped lock

		private int lockBudget; // The maximum number of elements per lock before a resize operation is triggered

		// The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
		// a trade-off between the size of a very small hashset, and the number of resizes when
		// constructing a large hashset. Also, the capacity should not be divisible by a small prime.
		protected const int DEFAULT_CAPACITY = 31;

		// The maximum size of the striped lock that will not be exceeded when locks are automatically
		// added as the hashset grows. However, the user is allowed to exceed this limit by passing
		// a concurrency level larger than MAX_LOCK_NUMBER into the constructor.
		protected const int MAX_LOCK_NUMBER = 1024;

		// Whether TValue is a type that can be written atomically (i.e., with no danger of torn reads)
		protected static readonly bool isValueWriteAtomic = _IsValueWriteAtomic();

		/// <summary>
		/// Determines whether type TValue can be written atomically
		/// </summary>
		private static bool _IsValueWriteAtomic()
		{
			Type valueType = typeof(T);

			// Section 12.6.6 of ECMA CLI explains which types can be read and written atomically without
			// the risk of tearing.
			//
			// See http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-335.pdf

			if (valueType.IsClass)
			{
				return true;
			}
			switch (Type.GetTypeCode(valueType))
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
					return true;

				case TypeCode.Int64:
				case TypeCode.Double:
				case TypeCode.UInt64:
					return IntPtr.Size == 8;

				default:
					return false;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		public ConcurrentHashSet() : this(PlatformHelper.DefaultConcurrencyLevel, DEFAULT_CAPACITY, true, EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="capacity">The initial number of elements that the <see cref="ConcurrentHashSet{T}"> can contain.</param>
		public ConcurrentHashSet(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, false, EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="collection">The values to be copied.</param>
		public ConcurrentHashSet(IEnumerable<T> collection) : this(collection, EqualityComparer<T>.Default)
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
		public ConcurrentHashSet(IEqualityComparer<T> comparer) : this(PlatformHelper.DefaultConcurrencyLevel, DEFAULT_CAPACITY, true, comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="collection">The values to be copied.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			InitializeFromCollection(collection);
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="collection">The values to be copied.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
		public ConcurrentHashSet(
			int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(concurrencyLevel, DEFAULT_CAPACITY, false, comparer)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			InitializeFromCollection(collection);
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="capacity">The initial number of elements that the <see cref="ConcurrentHashSet{T}"> can contain.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
		public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
			: this(concurrencyLevel, capacity, false, comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="capacity">The initial number of elements that the <see cref="ConcurrentHashSet{T}"> can contain.</param>
		/// <param name="growLockArray">Determines if the number of locks can increase when tables are resized.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use when comparing values.</param>
		internal ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T> comparer)
		{
			if (concurrencyLevel < 1)
				throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));


			// The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
			// any buckets.
			if (capacity < concurrencyLevel)
			{
				capacity = concurrencyLevel;
			}

			object[] locks = new object[concurrencyLevel];
			for (int i = 0; i < locks.Length; i++)
			{
				locks[i] = new object();
			}

			int[] countPerLock = new int[locks.Length];
			Node[] buckets = new Node[capacity];
			_tables = new Tables(buckets, locks, countPerLock);

			this.growLockArray = growLockArray;
			lockBudget = buckets.Length / locks.Length;
			this.comparer = comparer;
		}

		private void InitializeFromCollection(IEnumerable<T> collection)
		{
			T dummy;
			foreach (T value in collection)
			{
				TryAddInternal(value, false, false, out dummy);
			}
			if (lockBudget == 0)
			{
				lockBudget = _tables.buckets.Length / _tables.locks.Length;
			}
		}

		/// <summary>
		/// Add a value if it doesn't exist.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>true if the value was added; otherwise false.</returns>
		public bool Add(T value)
		{
			return TryAddInternal(value, false, true, out T _);
		}

		/// <summary>
		/// Adds a value if it doesn't exist.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <param name="currentValue">The value added or the current value if it already exists.</param>
		/// <returns>true if the value was added; otherwise false.</returns>
		public bool TryAdd(T value, out T currentValue)
		{
			return TryAddInternal(value, false, true, out currentValue);
		}

		/// <summary>
		/// Removes a value if it exists.
		/// </summary>
		/// <param name="value">The value to remove.</param>
		/// <param name="oldValue">The removed value if it exists, otherwise a default value.</param>
		/// <returns>true if an object was removed; otherwise false.</returns>
		public bool TryRemove(T value, out T oldValue)
		{
			int hashcode = comparer.GetHashCode(value);
			while (true)
			{
				Tables tables = _tables;

				int bucketNo, lockNo;
				GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables.buckets.Length, tables.locks.Length);

				lock (tables.locks[lockNo])
				{
					// If the table just got resized, we may not be holding the right lock, and must retry.
					// This should be a rare occurence.
					if (tables != _tables)
					{
						continue;
					}

					Node prev = null;
					for (Node curr = tables.buckets[bucketNo]; curr != null; curr = curr.next)
					{
						Debug.Assert((prev == null && curr == tables.buckets[bucketNo]) || prev.next == curr);
						if (comparer.Equals(curr.value, value))
						{
							if (prev == null)
							{
								Volatile.Write<Node>(ref tables.buckets[bucketNo], curr.next);
							}
							else
							{
								prev.next = curr.next;
							}
							oldValue = curr.value;
							tables.countPerLock[lockNo]--;
							return true;
						}
						prev = curr;
					}
				}
				oldValue = default(T);
				return false;
			}
		}

		/// <summary>
		/// Gets a value if it exists.
		/// </summary>
		/// <param name="value">The value to get.</param>
		/// <param name="currentValue">The value if it exists, otherwise a default value.</param>
		/// <returns>true if the value was found; otherwise false.</returns>
		public bool TryGetValue(T value, out T currentValue)
		{
			int bucketNo, lockNoUnused;

			// We must capture the m_buckets field in a local variable. It is set to a new table on each table resize.
			Tables tables = _tables;
			//IEqualityComparer<T> comparer = tables.m_comparer;
			GetBucketAndLockNo(comparer.GetHashCode(value), out bucketNo, out lockNoUnused, tables.buckets.Length, tables.locks.Length);

			// We can get away w/out a lock here.
			// The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
			Node n = Volatile.Read<Node>(ref tables.buckets[bucketNo]);

			while (n != null)
			{
				if (comparer.Equals(n.value, value))
				{
					currentValue = n.value;
					return true;
				}
				n = n.next;
			}

			currentValue = default(T);
			return false;
		}

		/// <summary>
		/// Updates a value if it exists.
		/// </summary>
		/// <param name="value">The value to update.</param>
		/// <returns>true if the value was updated; otherwise false.</returns>
		public bool TryUpdate(T value)
		{
			int hashcode = comparer.GetHashCode(value);
			while (true)
			{
				int bucketNo;
				int lockNo;

				Tables tables = _tables;
				//IEqualityComparer<T> comparer = tables.m_comparer;
				GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables.buckets.Length, tables.locks.Length);

				lock (tables.locks[lockNo])
				{
					// If the table just got resized, we may not be holding the right lock, and must retry.
					// This should be a rare occurence.
					if (tables != _tables)
					{
						continue;
					}

					// Try to find this key in the bucket
					Node prev = null;
					for (Node node = tables.buckets[bucketNo]; node != null; node = node.next)
					{
						Debug.Assert((prev == null && node == tables.buckets[bucketNo]) || prev.next == node);
						if (comparer.Equals(node.value, value))
						{
							if (isValueWriteAtomic)
							{
								node.value = value;
							}
							else
							{
								Node newNode = new Node(value, hashcode, node.next);
								if (prev == null)
								{
									tables.buckets[bucketNo] = newNode;
								}
								else
								{
									prev.next = newNode;
								}
							}
							return true;
						}
						prev = node;
					}

					//didn't find the key
					return false;
				}
			}
		}

		/// <summary>
		/// Removes the value if it exists.
		/// </summary>
		/// <param name="value">The value to remove.</param>
		/// <returns>true if an object was removed; otherwise false.</returns>
		public bool Remove(T value)
		{
			return TryRemove(value, out T _);
		}

		/// <summary>
		/// Adds or updates a value.
		/// </summary>
		/// <param name="value">The value to add or update.</param>
		public void AddOrUpdate(T value)
		{
			TryAddInternal(value, true, true, out T _);
		}

		/// <summary>
		/// Adds a value if it does not exist.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>The current value if it already exists, otherwise the new value.</returns>
		public T GetOrAdd(T value)
		{
			TryAddInternal(value, false, true, out T currentValue);
			return currentValue;
		}

		/// <summary>
		/// Removes all values.
		/// </summary>
		public void Clear()
		{
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);

				Tables newTables = new Tables(new Node[DEFAULT_CAPACITY], _tables.locks, new int[_tables.countPerLock.Length]);
				_tables = newTables;
				lockBudget = Math.Max(1, newTables.buckets.Length / newTables.locks.Length);
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}

		protected bool TryAddInternal(T value, bool updateIfExists, bool acquireLock, out T resultingValue)
		{
			int hashcode = comparer.GetHashCode(value);
			while (true)
			{
				int bucketNo, lockNo;

				Tables tables = _tables;
				GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables.buckets.Length, tables.locks.Length);

				bool resizeDesired = false;
				bool lockTaken = false;

				try
				{
					if (acquireLock)
						Monitor.Enter(tables.locks[lockNo], ref lockTaken);

					// If the table just got resized, we may not be holding the right lock, and must retry.
					// This should be a rare occurence.
					if (tables != _tables)
					{
						continue;
					}

					// Try to find this key in the bucket
					Node prev = null;
					for (Node node = tables.buckets[bucketNo]; node != null; node = node.next)
					{
						Debug.Assert((prev == null && node == tables.buckets[bucketNo]) || prev.next == node);
						//if (m_comparer.Equals(node.m_value, value)) {
						if (hashcode == node.hashcode && comparer.Equals(node.value, value))
						{
							// The key was found in the hashset. If updates are allowed, update the value for that key.
							// We need to create a new node for the update, in order to support TValue types that cannot
							// be written atomically, since lock-free reads may be happening concurrently.
							if (updateIfExists)
							{
								if (isValueWriteAtomic)
								{
									node.value = value;
								}
								else
								{
									Node newNode = new Node(value, hashcode, node.next);
									if (prev == null)
									{
										tables.buckets[bucketNo] = newNode;
									}
									else
									{
										prev.next = newNode;
									}
								}
								resultingValue = value;
							}
							else
							{
								resultingValue = node.value;
							}
							return false;
						}
						prev = node;
					}

					// The key was not found in the bucket. Insert the key-value pair.
					Volatile.Write<Node>(ref tables.buckets[bucketNo], new Node(value, hashcode, tables.buckets[bucketNo]));
					checked
					{
						tables.countPerLock[lockNo]++;
					}

					//
					// If the number of elements guarded by this lock has exceeded the budget, resize the bucket table.
					// It is also possible that GrowTable will increase the budget but won't resize the bucket table.
					// That happens if the bucket table is found to be poorly utilized due to a bad hash function.
					//
					if (tables.countPerLock[lockNo] > lockBudget)
					{
						resizeDesired = true;
					}
				}
				finally
				{
					if (lockTaken)
						Monitor.Exit(tables.locks[lockNo]);
				}

				//
				// The fact that we got here means that we just performed an insertion. If necessary, we will grow the table.
				//
				// Concurrency notes:
				// - Notice that we are not holding any locks at when calling GrowTable. This is necessary to prevent deadlocks.
				// - As a result, it is possible that GrowTable will be called unnecessarily. But, GrowTable will obtain lock 0
				//   and then verify that the table we passed to it as the argument is still the current table.
				//
				if (resizeDesired)
				{
					GrowTable(tables);
				}

				resultingValue = value;
				return true;
			}
		}

		/// <summary>
		/// Gets the number of values contained in the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		public int Count
		{
			get
			{
				int acquiredLocks = 0;
				try
				{
					// Acquire all locks
					AcquireAllLocks(ref acquiredLocks);

					return GetCountInternal();
				}
				finally
				{
					// Release locks that have been acquired earlier
					ReleaseLocks(0, acquiredLocks);
				}
			}
		}

		private int GetCountInternal()
		{
			int count = 0;

			// Compute the count, we allow overflow
			for (int i = 0; i < _tables.countPerLock.Length; i++)
			{
				count += _tables.countPerLock[i];
			}

			return count;
		}

		/// <summary>
		/// Determines whether the <see cref="ConcurrentHashSet{T}"/> contains a value.
		/// </summary>
		/// <param name="key">The value to locate.</param>
		/// <returns>true if the value exists; otherwise false.</returns>
		public bool Contains(T item)
		{
			int bucketNo, lockNoUnused;
			// We must capture the m_buckets field in a local variable. It is set to a new table on each table resize.
			Tables tables = _tables;
			GetBucketAndLockNo(comparer.GetHashCode(item), out bucketNo, out lockNoUnused, tables.buckets.Length, tables.locks.Length);

			// We can get away w/out a lock here.
			// The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
			Node n = Volatile.Read<Node>(ref tables.buckets[bucketNo]);

			while (n != null)
			{
				if (comparer.Equals(n.value, item))
				{
					return true;
				}
				n = n.next;
			}
			return false;
		}

		/// <summary>
		/// Determines whether the <see cref="ConcurrentHashSet{T}"/> is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				int acquiredLocks = 0;
				try
				{
					// Acquire all locks
					AcquireAllLocks(ref acquiredLocks);

					for (int i = 0; i < _tables.countPerLock.Length; i++)
					{
						if (_tables.countPerLock[i] != 0)
						{
							return false;
						}
					}
				}
				finally
				{
					// Release locks that have been acquired earlier
					ReleaseLocks(0, acquiredLocks);
				}

				return true;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the values in the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <returns>An enumerator that iterates through the values in the <see cref="ConcurrentHashSet{T}"/></returns>
		public IEnumerator<T> GetEnumerator()
		{
			Node[] buckets = _tables.buckets;

			for (int i = 0; i < buckets.Length; i++)
			{
				// The Volatile.Read ensures that the load of the fields of 'current' doesn't move before the load from buckets[i].
				Node current = Volatile.Read<Node>(ref buckets[i]);

				while (current != null)
				{
					yield return current.value;
					current = current.next;
				}
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the values in the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <returns>An enumerator that iterates through the values in the <see cref="ConcurrentHashSet{T}"/></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Copies the values to an array starting at the specified index.
		/// </summary>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				Tables tables = _tables;

				int count = 0;
				for (int i = 0; i < tables.locks.Length && count >= 0; i++)
				{
					count += tables.countPerLock[i];
				}

				if (array.Length - count < index || count < 0) // "count" itself or "count + index" can overflow
				{
					throw new ArgumentException("Array is not large enough");
				}

				if (array is T[] arr)
				{
					CopyTo(arr, index);
				}
				else
				{
					Node[] buckets = _tables.buckets;
					for (int i = 0; i < buckets.Length; i++)
					{
						for (Node current = buckets[i]; current != null; current = current.next)
						{
							array.SetValue(current.value, index);
							index++;
						}
					}
				}
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is
		/// synchronized with the SyncRoot.
		/// </summary>
		bool ICollection.IsSynchronized => false;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
		/// This property is not supported.
		/// </summary>
		object ICollection.SyncRoot => throw new NotSupportedException();

		/// <summary>
		/// Add a value if it doesn't exist.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>true if the value was added; otherwise false.</returns>
		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		/// <summary>
		/// Replaces the bucket table with a larger one. Obtains a lock, and then checks the Tables 
		/// instance has been replaced in the meantime or not. The Tables instance is passed in to
		/// prevent To prevent multiple threads from resizing the table.
		/// </summary>
		/// <param name="tables"></param>
		private void GrowTable(Tables tables)
		{
			int locksAcquired = 0;
			try
			{
				// The thread that first obtains m_locks[0] will be the one doing the resize operation
				AcquireLocks(0, 1, ref locksAcquired);

				// If we don't require a regeneration of hash keys we want to make sure we don't do work when
				// we don't have to
				if (tables != _tables)
				{
					// We assume that since the table reference is different, it was already resized (or the budget
					// was adjusted). If we ever decide to do table shrinking, or replace the table for other reasons,
					// we will have to revisit this logic.
					return;
				}

				// Compute the (approx.) total size. Use an Int64 accumulation variable to avoid an overflow.
				long approxCount = 0;
				for (int i = 0; i < tables.countPerLock.Length; i++)
				{
					approxCount += tables.countPerLock[i];
				}

				//
				// If the bucket array is too empty, double the budget instead of resizing the table
				//
				if (approxCount < tables.buckets.Length / 4)
				{
					lockBudget = 2 * lockBudget;
					if (lockBudget < 0)
					{
						lockBudget = int.MaxValue;
					}

					return;
				}
				// Compute the new table size. We find the smallest integer larger than twice the previous table size, and not divisible by
				// 2,3,5 or 7. We can consider a different table-sizing policy in the future.
				int newLength = 0;
				bool maximizeTableSize = false;
				try
				{
					checked
					{
						// Double the size of the buckets table and add one, so that we have an odd integer.
						newLength = tables.buckets.Length * 2 + 1;

						// Now, we only need to check odd integers, and find the first that is not divisible
						// by 3, 5 or 7.
						while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0)
						{
							newLength += 2;
						}

						Debug.Assert(newLength % 2 != 0);

						if (newLength > MAX_ARRAY_LENGTH)
						{
							maximizeTableSize = true;
						}
					}
				}
				catch (OverflowException)
				{
					maximizeTableSize = true;
				}
				if (maximizeTableSize)
				{
					newLength = MAX_ARRAY_LENGTH;

					// We want to make sure that GrowTable will not be called again, since table is at the maximum size.
					// To achieve that, we set the budget to int.MaxValue.
					//
					// (There is one special case that would allow GrowTable() to be called in the future:
					// calling Clear() will shrink the table and lower the budget.)
					lockBudget = int.MaxValue;
				}

				// Now acquire all other locks for the table
				AcquireLocks(1, tables.locks.Length, ref locksAcquired);

				object[] newLocks = tables.locks;

				// Add more locks
				if (growLockArray && tables.locks.Length < MAX_LOCK_NUMBER)
				{
					newLocks = new object[tables.locks.Length * 2];
					Array.Copy(tables.locks, newLocks, tables.locks.Length);

					for (int i = tables.locks.Length; i < newLocks.Length; i++)
					{
						newLocks[i] = new object();
					}
				}

				Node[] newBuckets = new Node[newLength];
				int[] newCountPerLock = new int[newLocks.Length];

				// Copy all data into a new table, creating new nodes for all elements
				for (int i = 0; i < tables.buckets.Length; i++)
				{
					Node current = tables.buckets[i];
					while (current != null)
					{
						Node next = current.next;
						int newBucketNo, newLockNo;
						int nodeHashCode = current.hashcode;

						GetBucketAndLockNo(nodeHashCode, out newBucketNo, out newLockNo, newBuckets.Length, newLocks.Length);

						newBuckets[newBucketNo] = new Node(current.value, nodeHashCode, newBuckets[newBucketNo]);

						checked
						{
							newCountPerLock[newLockNo]++;
						}
						current = next;
					}
				}
				// Adjust the budget
				lockBudget = Math.Max(1, newBuckets.Length / newLocks.Length);

				// Replace tables with the new versions
				_tables = new Tables(newBuckets, newLocks, newCountPerLock);
			}
			finally
			{
				// Release all locks that we took earlier
				ReleaseLocks(0, locksAcquired);
			}
		}

		/// <summary>
		/// Computes the bucket and lock number for a particular key.
		/// </summary>
		private void GetBucketAndLockNo(
				int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
		{
			bucketNo = (hashcode & 0x7fffffff) % bucketCount;
			lockNo = bucketNo % lockCount;

			Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
			Debug.Assert(lockNo >= 0 && lockNo < lockCount);
		}

		public bool IsReadOnly => false;

		/// <summary>
		/// Acquires all locks for this hash table, and increments locksAcquired by the number
		/// of locks that were successfully acquired. The locks are acquired in an increasing
		/// order.
		/// </summary>
		private void AcquireAllLocks(ref int locksAcquired)
		{
			// First, acquire lock 0
			AcquireLocks(0, 1, ref locksAcquired);

			// Now that we have lock 0, the m_locks array will not change (i.e., grow),
			// and so we can safely read m_locks.Length.
			AcquireLocks(1, _tables.locks.Length, ref locksAcquired);
			Debug.Assert(locksAcquired == _tables.locks.Length);
		}

		/// <summary>
		/// Acquires a contiguous range of locks for this hash table, and increments locksAcquired
		/// by the number of locks that were successfully acquired. The locks are acquired in an
		/// increasing order.
		/// </summary>
		private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
		{
			Debug.Assert(fromInclusive <= toExclusive);
			object[] locks = _tables.locks;

			for (int i = fromInclusive; i < toExclusive; i++)
			{
				bool lockTaken = false;
				try
				{
					Monitor.Enter(locks[i], ref lockTaken);
				}
				finally
				{
					if (lockTaken)
					{
						locksAcquired++;
					}
				}
			}
		}

		/// <summary>
		/// Releases a contiguous range of locks.
		/// </summary>
		private void ReleaseLocks(int fromInclusive, int toExclusive)
		{
			Debug.Assert(fromInclusive <= toExclusive);

			for (int i = fromInclusive; i < toExclusive; i++)
			{
				Monitor.Exit(_tables.locks[i]);
			}
		}

		/// <summary>
		/// Copies the values to an array starting at the specified index.
		/// </summary>
		public void CopyTo(T[] array, int arrayIndex)
		{
			Node[] buckets = _tables.buckets;
			for (int i = 0; i < buckets.Length; i++)
			{
				for (Node current = buckets[i]; current != null; current = current.next)
				{
					array[arrayIndex] = current.value;
					arrayIndex++;
				}
			}
		}
	}

	/// <summary>
	/// A helper class to get the number of processors, it updates the numbers of processors every sampling interval.
	/// </summary>
	internal static class PlatformHelper
	{
		private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000; // How often to refresh the count, in milliseconds.
		private static volatile int s_processorCount; // The last count seen.
		private static volatile int s_lastProcessorCountRefreshTicks; // The last time we refreshed.

		/// <summary>
		/// The number of concurrent writes for which to optimize by default.
		/// </summary>
		public static int DefaultConcurrencyLevel => ProcessorCount;


		/// <summary>
		/// Gets the number of available processors
		/// </summary>
		private static int ProcessorCount
		{
			get
			{
				int now = Environment.TickCount;
				int procCount = s_processorCount;
				if (procCount == 0 || (now - s_lastProcessorCountRefreshTicks) >= PROCESSOR_COUNT_REFRESH_INTERVAL_MS)
				{
					s_processorCount = procCount = Environment.ProcessorCount;
					s_lastProcessorCountRefreshTicks = now;
				}

				Debug.Assert(procCount > 0 && procCount <= 64,
					"Processor count not within the expected range (1 - 64).");

				return procCount;
			}
		}
	}
}