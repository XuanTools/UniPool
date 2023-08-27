using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace XuanTools.UniPool
{
    public sealed class UniPool : IDisposable
    {
        private readonly Func<GameObject> _createFunc;
        private readonly Action<GameObject> _actionOnGet;
        private readonly Action<GameObject> _actionOnRelease;
        private readonly Action<GameObject> _actionOnDestroy;
        private readonly List<GameObject> _tempList = new();

        private readonly HashSet<GameObject> _pool;
        private readonly HashSet<GameObject> _cache;
        private readonly HashSet<GameObject> _active;

        public GameObject Prefab { get; }
        public int MaxSize { get; }
        public int CountAll => CountActive + CountInactive;
        public int CountActive => _active.Count;
        public int CountInactive => _pool.Count + _cache.Count;

        public UniPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000,
            [CanBeNull] Func<GameObject> createFunc = null, [CanBeNull] Action<GameObject> actionOnGet = null,
            [CanBeNull] Action<GameObject> actionOnRelease = null,
            [CanBeNull] Action<GameObject> actionOnDestroy = null)
        {
            Prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            MaxSize = maxSize > 0
                ? maxSize
                : throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            _pool = new HashSet<GameObject>(maxSize);
            _cache = new HashSet<GameObject>(maxSize);
            _active = new HashSet<GameObject>(maxSize);
            _createFunc = createFunc ?? DefaultCreateFunc;
            _actionOnGet = actionOnGet ?? DefaultActionOnGet;
            _actionOnRelease = actionOnRelease ?? DefaultActionOnRelease;
            _actionOnDestroy = actionOnDestroy ?? DefaultActionOnDestroy;
            InitDefaultCapacity(defaultCapacity);
        }

        public GameObject Get()
        {
            GameObject obj;
            if (_cache.Count > 0)
            {
                obj = _cache.First();
                _cache.Remove(obj);
            }
            else if (_pool.Count > 0)
            {
                obj = _pool.First();
                _pool.Remove(obj);
            }
            else
            {
                obj = _createFunc();
            }

            _active.Add(obj);
            _actionOnGet(obj);
            return obj;
        }

        public List<GameObject> GetList(int count)
        {
            if (count < 0) throw new ArgumentException("Max Size must not be less than 0", nameof(count));

            var list = new List<GameObject>(count);

            var remain = count;
            while (remain > 0 && _cache.Any())
            {
                var obj = _cache.First();
                list.Add(obj);
                _cache.Remove(obj);
                remain--;
            }
            while (remain > 0 && _pool.Any())
            {
                var obj = _pool.First();
                list.Add(obj);
                _pool.Remove(obj);
                remain--;
            }
            for (var i = 0; i < remain; i++)
            {
                list.Add(_createFunc());
            }

            list.ForEach(obj =>
            {
                _active.Add(obj);
                _actionOnGet(obj);
            });
            return list;
        }

        public void Release(GameObject obj)
        {
            if (obj == Prefab)
                throw new InvalidOperationException($"Trying to release prefab which is not allowed: {obj}");
            if (_pool.Contains(obj))
                throw new InvalidOperationException(
                    $"Trying to release an object that has already been released in the pool: {obj}");

            _actionOnRelease(obj);
            _active.Remove(obj);
            if (CountInactive < MaxSize)
            {
                _pool.Add(obj);
            }
            else
            {
                _actionOnDestroy(obj);
            }
        }

        public void Cache(GameObject obj)
        {
            if (obj == Prefab)
                throw new InvalidOperationException($"Trying to cache prefab which is not allowed: {obj}");
            if (_cache.Contains(obj) || _pool.Contains(obj))
                throw new InvalidOperationException(
                    $"Trying to cache an object that has already been in the pool: {obj}");
            if (!_active.Contains(obj))
                throw new InvalidOperationException(
                    $"Trying to cache an object that does not belong to the pool: {obj}");

            _active.Remove(obj);
            _cache.Add(obj);
        }

        public void CacheReleaseAll()
        {
            foreach (var obj in _cache)
            {
                Release(obj);
            }

            _cache.Clear();
        }

        public void SpawnedReleaseAll()
        {
            _tempList.AddRange(_active);
            for (var i = _tempList.Count; i >= 0; i--)
            {
                Release(_tempList[i]);
            }

            _tempList.Clear();
        }

        public void SpawnedCacheAll()
        {
            _tempList.AddRange(_active);
            for (var i = _tempList.Count; i >= 0; i--)
            {
                Cache(_tempList[i]);
            }

            _tempList.Clear();
        }

        public void RemoveDestroyedObject()
        {
            RemoveDestroyedObjectInCollection(_pool);
            RemoveDestroyedObjectInCollection(_cache);
            RemoveDestroyedObjectInCollection(_active);
        }

        private void RemoveDestroyedObjectInCollection(ICollection<GameObject> collection)
        {
            _tempList.AddRange(collection);
            for (var i = _tempList.Count; i >= 0; i--)
            {
                if (!_tempList[i])
                {
                    collection.Remove(_tempList[i]);
                }
            }

            _tempList.Clear();
        }

        public bool Contain(GameObject obj)
        {
            return _pool.Contains(obj) || _cache.Contains(obj) || _active.Contains(obj);
        }

        public void ClearPooled()
        {
            foreach (var obj in _pool)
            {
                _actionOnDestroy(obj);
            }

            foreach (var obj in _cache)
            {
                _actionOnDestroy(obj);
            }

            _pool.Clear();
            _cache.Clear();
        }

        public void ClearAll()
        {
            SpawnedCacheAll();
            ClearPooled();
        }

        public void Dispose()
        {
            ClearAll();
        }

        private void InitDefaultCapacity(int defaultCapacity)
        {
            for (var i = 0; i < defaultCapacity; i++)
            {
                var obj = _createFunc();
                _actionOnRelease(obj);
                _pool.Add(obj);
            }
        }

        private GameObject DefaultCreateFunc()
        {
            return UnityEngine.Object.Instantiate(Prefab);
        }

        private static void DefaultActionOnGet(GameObject obj)
        {
            obj.SetActive(true);
        }

        private static void DefaultActionOnRelease(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void DefaultActionOnDestroy(GameObject obj)
        {
            UnityEngine.Object.Destroy(obj);
        }

        public override string ToString()
        {
            return $"UniPool: {Prefab.name}({CountInactive}/{CountAll})";
        }
    }

    public sealed class UniPool<T> : IDisposable where T : Component
    {
        private readonly UniPool _uniPool;

        public T Prefab => _uniPool.Prefab.GetComponent<T>();
        public int CountAll => _uniPool.CountAll;
        public int CountActive => _uniPool.CountActive;
        public int CountInactive => _uniPool.CountInactive;
        public int MaxSize => _uniPool.MaxSize;

        public UniPool(T prefab, int defaultCapacity = 10, int maxSize = 10000, Func<T> createFunc = null,
            Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null)
        {
            _uniPool = new UniPool(prefab.gameObject, defaultCapacity, maxSize,
                () => createFunc!().gameObject,
                obj => actionOnGet!(obj.GetComponent<T>()),
                obj => actionOnRelease!(obj.GetComponent<T>()),
                obj => actionOnDestroy!(obj.GetComponent<T>()));
        }

        public T Get()
        {
            return _uniPool.Get().GetComponent<T>();
        }

        public void Release(T obj)
        {
            _uniPool.Release(obj.gameObject);
        }

        public void Cache(T obj)
        {
            _uniPool.Cache(obj.gameObject);
        }

        public void CacheReleaseAll()
        {
            _uniPool.CacheReleaseAll();
        }

        public void SpawnedReleaseAll()
        {
            _uniPool.SpawnedReleaseAll();
        }

        public void SpawnedCacheAll()
        {
            _uniPool.SpawnedCacheAll();
        }

        public void RemoveDestroyedObject()
        {
            _uniPool.RemoveDestroyedObject();
        }

        public bool Contain(T obj)
        {
            return _uniPool.Contain(obj.gameObject);
        }

        public void ClearPooled()
        {
            _uniPool.ClearPooled();
        }

        public void ClearAll()
        {
            _uniPool.ClearAll();
        }

        public void Dispose()
        {
            ClearAll();
        }

        public override string ToString()
        {
            return $"UniPool: {Prefab.name}({CountInactive}/{CountAll})";
        }
    }
}