using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using XuanTools.UniPool.Collections;

namespace XuanTools.UniPool
{
    public sealed class UniPool : IDisposable
    {
        private readonly Func<GameObject> _createFunc;
        private readonly Action<GameObject> _actionOnGet;
        private readonly Action<GameObject> _actionOnRelease;
        private readonly Action<GameObject> _actionOnDestroy;
        private readonly List<GameObject> _tempList = new();

        private readonly HashPool<GameObject> _pool;
        private readonly HashPool<GameObject> _cache;
        private readonly HashPool<GameObject> _active;

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
            _pool = new HashPool<GameObject>(maxSize);
            _cache = new HashPool<GameObject>(maxSize);
            _active = new HashPool<GameObject>(maxSize);
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
                obj = _cache.Get();
            }
            else if (_pool.Count > 0)
            {
                obj = _pool.Get();
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
            var list = new List<GameObject>(count);
            GetToList(list, count);
            return list;
        }
        public List<GameObject> GetList(int count, Action<GameObject> actionAfterGet)
        {
            var list = new List<GameObject>(count);
            GetToList(list, count, actionAfterGet);
            return list;
        }

        public void GetToList(List<GameObject> list, int count)
        {
            GetToList(list, count, _ => { });
        }
        public void GetToList(List<GameObject> list, int count, Action<GameObject> actionAfterGet)
        {
            if (count < 0) throw new ArgumentException("Count must not be less than 0", nameof(count));

            // Get game object from pool to list
            // Count will auto reduce by GetToList
            count = _cache.GetToList(list, count, Action);
            count = _pool.GetToList(list, count, Action);

            for (var i = 0; i < count; i++)
            {
                var obj = _createFunc();
                list.Add(obj);
                Action(obj);
            }

            return;

            void Action(GameObject obj)
            {
                _active.Add(obj);
                _actionOnGet(obj);
                actionAfterGet(obj);
            }
        }

        public void Release(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
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
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
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

        private void RemoveDestroyedObjectInCollection(HashPool<GameObject> pool)
        {
            _tempList.AddRange(pool);
            for (var i = _tempList.Count; i >= 0; i--)
            {
                if (!_tempList[i])
                {
                    pool.Remove(_tempList[i]);
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

        public List<T> GetList(int count)
        {
            return _uniPool.GetList(count).ConvertAll(obj => obj.GetComponent<T>());
        }

        public List<T> GetList(int count, Action<T> actionAfterGet)
        {
            return _uniPool.GetList(count, obj => actionAfterGet(obj.GetComponent<T>()))
                .ConvertAll(obj => obj.GetComponent<T>());
        }

        public void GetToList(List<T> list, int count)
        {
            list.AddRange(GetList(count));
        }

        public void GetToList(List<T> list, int count, Action<T> actionAfterGet)
        {
            list.AddRange(GetList(count, actionAfterGet));
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