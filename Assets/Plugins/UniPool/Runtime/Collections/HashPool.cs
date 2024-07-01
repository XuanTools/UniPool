using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace XuanTools.UniPool.Collections
{
    public interface IPool<T>
    {
        int Count { get; }

        void Add(T value);

        T Get();

        bool Contains(T value);
    }

    public class HashPool<T> : ICollection<T>, IPool<T>
    {
        internal struct Slot
        {
            internal int HashCode; // Lower 31 bits of hash code, -1 if unused
            internal int Next; // Index of next entry, -1 if last
            internal T Value; // Store value
        }

        // store lower 31 bits of hash code
        private const int Lower31BitMask = 0x7FFFFFFF;

        private int[] _buckets; // An array to store reference to slots
        private Slot[] _slots; // An array that value actually be stored
        private int _lastIndex; // Last index that has been used to store value
        private int _freeList; // A list for free slot cause by remove
        private readonly IEqualityComparer<T> _comparer; // Comparator used to determine equality

        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public HashPool() : this(EqualityComparer<T>.Default) { }

        public HashPool(int capacity) : this(capacity, EqualityComparer<T>.Default) { }

        public HashPool(int capacity, IEqualityComparer<T> comparer) : this(comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            Initialize(capacity);
        }

        public HashPool(IEqualityComparer<T> comparer)
        {
#if UNITY_2020_2_OR_NEWER
            comparer ??= EqualityComparer<T>.Default;
#else
            if (comparer == null) comparer = EqualityComparer<T>.Default;
#endif

            _comparer = comparer;
            _lastIndex = 0;
            Count = 0;
            _freeList = -1;
        }

        public void Add(T value)
        {
            AddIfNotPresent(value);
        }

        internal bool AddIfNotPresent(T value)
        {
            var hashCode = InternalGetHashCode(value);
            var bucket = hashCode % _buckets.Length;

            for (var i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _slots[i].Next)
            {
                if (_slots[i].HashCode == hashCode && _comparer.Equals(_slots[i].Value, value))
                {
                    return false;
                }
            }

            int index;
            if (_freeList >= 0)
            {
                index = _freeList;
                _freeList = _slots[index].Next;
            }
            else
            {
                if (_lastIndex == _slots.Length)
                {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % _buckets.Length;
                }
                index = _lastIndex;
                _lastIndex++;
            }
            _slots[index].HashCode = hashCode;
            _slots[index].Value = value;
            _slots[index].Next = _buckets[bucket];
            _buckets[bucket] = index;
            Count++;

            return true;
        }

        public T Get()
        {
            if (Count == 0) throw new InvalidOperationException("Count = 0 but try to release");

            var index = 0;
#if UNITY_2018_2_OR_NEWER
            T value = default;
#else
            T value = default(T);
#endif
            while (index < _lastIndex)
            {
                if (_slots[index].HashCode >= 0)
                {
                    value = _slots[index].Value;
                    break;
                }

                index++;
            }

            Debug.Assert(value != null, "Count != 0 but valid value not found");
            Remove(value);
            return value;
        }

        public int GetToList(List<T> list, int count, Action<T> actionAfterGet = null)
        {
            var index = 0;
            // Iterate buckets
            while (Count > 0 && count > 0 && index < _buckets.Length)
            {
                // Iterate slot list that buckets reference to
                while (Count > 0 && count > 0 && _buckets[index] >= 0)
                {
                    var value = _slots[_buckets[index]].Value;
                    actionAfterGet?.Invoke(value);
                    list.Add(value);
                    RemoveAt(index);

                    count--;
                }
                index++;
            }

            return count;
        }

        private void RemoveAt(int bucket)
        {
            // Remove first value
            var i = _buckets[bucket];
            _buckets[bucket] = _slots[i].Next;

            _slots[i].HashCode = -1;
#if UNITY_2018_2_OR_NEWER
            _slots[i].Value = default;
#else
            _slots[i].Value = default(T);
#endif
            _slots[i].Next = _freeList;

            Count--;
            if (Count == 0)
            {
                _lastIndex = 0;
                _freeList = -1;
            }
            else
            {
                _freeList = i;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (arrayIndex > array.Length || Count > array.Length - arrayIndex) throw new ArgumentException("Index too large or Array length too small");

            var numCopied = 0;
            for (var i = 0; i < _lastIndex && numCopied < Count; i++)
            {
                if (_slots[i].HashCode < 0) continue;

                array[arrayIndex + numCopied] = _slots[i].Value;
                numCopied++;
            }
        }

        public bool Remove(T item)
        {
            if (_buckets == null) return false;

            var hashCode = InternalGetHashCode(item);
            var bucket = hashCode % _buckets.Length;
            var last = -1;
            for (var i = _buckets[bucket]; i >= 0; last = i, i = _slots[i].Next)
            {
                if (_slots[i].HashCode != hashCode) continue;
                if (!_comparer.Equals(_slots[i].Value, item)) continue;

                if (last < 0)
                {
                    // First iteration; update buckets
                    _buckets[bucket] = _slots[i].Next;
                }
                else
                {
                    // Subsequent iterations; update 'next' pointers
                    _slots[last].Next = _slots[i].Next;
                }
                _slots[i].HashCode = -1;
#if UNITY_2018_2_OR_NEWER
                _slots[i].Value = default;
#else
                _slots[i].Value = default(T);
#endif
                _slots[i].Next = _freeList;

                Count--;
                if (Count == 0)
                {
                    _lastIndex = 0;
                    _freeList = -1;
                }
                else
                {
                    _freeList = i;
                }
                return true;
            }
            // Either _buckets is null or wasn't found
            return false;
        }

        public bool Contains(T item)
        {
            if (_buckets == null) return false;

            var hashCode = InternalGetHashCode(item);
            // see note at "HashSet" level describing why "- 1" appears in for loop

            for (var i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _slots[i].Next)
            {
                if (_slots[i].HashCode == hashCode && _comparer.Equals(_slots[i].Value, item))
                {
                    return true;
                }
            }
            // Either _buckets is null or wasn't found
            return false;
        }

        public void Clear()
        {
            if (_lastIndex <= 0) return;
            Debug.Assert(_buckets != null, "_buckets was null but _lastIndex > 0");

            // Clear the elements so that the gc can reclaim the references.
            // Clear only up to _lastIndex for _slots 
            for (var i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
            Array.Clear(_slots, 0, _lastIndex);

            _lastIndex = 0;
            Count = 0;
            _freeList = -1;
        }

        private void Initialize(int capacity)
        {
            Debug.Assert(_buckets == null, "Initialize was called but _buckets was non-null");

            var size = HashHelpers.GetPrime(capacity);

            _buckets = new int[size];
            for (var i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
            _slots = new Slot[size];
        }

        private void IncreaseCapacity()
        {
            Debug.Assert(_buckets != null, "IncreaseCapacity called on a _pool with no elements");

            var newSize = HashHelpers.ExpandPrime(Count);
            if (newSize <= Count)
            {
                throw new ArgumentException();
            }

            // Able to increase capacity; copy elements to larger array and rehash
            SetCapacity(newSize, false);
        }

        private void SetCapacity(int newSize, bool forceNewHashCodes)
        {
            Contract.Assert(HashHelpers.IsPrime(newSize), "New size is not prime!");

            Contract.Assert(_buckets != null, "SetCapacity called on a _pool with no elements");

            var newSlots = new Slot[newSize];
            if (_slots != null)
            {
                Array.Copy(_slots, 0, newSlots, 0, _lastIndex);
            }

            if (forceNewHashCodes)
            {
                for (var i = 0; i < _lastIndex; i++)
                {
                    if (newSlots[i].HashCode != -1)
                    {
                        newSlots[i].HashCode = InternalGetHashCode(newSlots[i].Value);
                    }
                }
            }

            var newBuckets = new int[newSize];
            for (var i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            // Recalculate next reference for slots
            for (var i = 0; i < _lastIndex; i++)
            {
                var bucket = newSlots[i].HashCode % newSize;
                newSlots[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
            _slots = newSlots;
            _buckets = newBuckets;
        }

        private int InternalGetHashCode(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return _comparer.GetHashCode(item) & Lower31BitMask;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly HashPool<T> _pool;
            private int _index;

            internal Enumerator(HashPool<T> pool)
            {
                this._pool = pool;
                _index = 0;
#if UNITY_2018_2_OR_NEWER
                Current = default;
#else
                Current = default(T);
#endif
            }

            public bool MoveNext()
            {
                while (_index < _pool._lastIndex)
                {
                    if (_pool._slots[_index].HashCode >= 0)
                    {
                        Current = _pool._slots[_index].Value;
                        _index++;
                        return true;
                    }
                    _index++;
                }
                _index = _pool._lastIndex + 1;
#if UNITY_2018_2_OR_NEWER
                Current = default;
#else
                Current = default(T);
#endif
                return false;
            }

            public T Current { get; private set; }

            void IEnumerator.Reset()
            {
                _index = 0;
#if UNITY_2018_2_OR_NEWER
                Current = default;
#else
                Current = default(T);
#endif
            }

#if UNITY_2020_2_OR_NEWER
            readonly object IEnumerator.Current => Current;

            public readonly void Dispose()
            {

            }
#else
            object IEnumerator.Current => Current;

            public void Dispose()
            {

            }
#endif
        }
    }

    internal static class HashHelpers
    {
        public static readonly int[] Primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0) return (candidate == 2);
            var limit = (int)Math.Sqrt(candidate);
            for (var divisor = 3; divisor <= limit; divisor += 2)
            {
                if ((candidate % divisor) == 0)
                    return false;
            }
            return true;
        }

        public static int GetPrime(int min)
        {
            if (min < 0)
                throw new ArgumentException();

            foreach (var prime in Primes)
            {
                if (prime >= min) return prime;
            }

            //outside our predefined table. 
            //compute the hard way. 
            for (var i = (min | 1); i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % 101 != 0))
                    return i;
            }
            return min;
        }

        public static int GetMinPrime()
        {
            return Primes[0];
        }

        // Returns size of hashtable to grow to.
        public static int ExpandPrime(int oldSize)
        {
            var newSize = 2 * oldSize;

            // Allow the hashtable to grow to maximum possible size (~2G elements) before encountering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize <= MaxPrimeArrayLength || MaxPrimeArrayLength <= oldSize) return GetPrime(newSize);
            Contract.Assert(MaxPrimeArrayLength == GetPrime(MaxPrimeArrayLength), "Invalid MaxPrimeArrayLength");
            return MaxPrimeArrayLength;

        }

        // This is the maximum prime smaller than Array.MaxArrayLength
        public const int MaxPrimeArrayLength = 0x7FEFFFFD;
    }
}
