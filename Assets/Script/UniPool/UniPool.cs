using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XuanTools.UniPool
{
    public sealed class UniPool : IDisposable
    {
        private readonly Func<GameObject> m_CreateFunc;
        private readonly Action<GameObject> m_ActionOnGet;
        private readonly Action<GameObject> m_ActionOnRelease;
        private readonly Action<GameObject> m_ActionOnDestroy;
        private readonly List<GameObject> tempList = new();

        internal readonly GameObject m_Prefab;
		internal readonly HashSet<GameObject> m_Pool;
		internal readonly HashSet<GameObject> m_Cache;
		internal readonly HashSet<GameObject> m_Active;
		internal readonly int m_MaxSize;

        public GameObject Prefab { get => m_Prefab; }
        public int CountAll { get => CountActive + CountInactive; }
        public int CountActive { get => m_Active.Count; }
        public int CountInactive { get => m_Pool.Count + m_Cache.Count; }
        public int MaxSize { get => m_MaxSize; }

        public UniPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000, Func<GameObject> createFunc = null, Action<GameObject> actionOnGet = null, Action<GameObject> actionOnRelease = null, Action<GameObject> actionOnDestroy = null)
        {
            m_Prefab = prefab != null ? prefab : throw new ArgumentNullException("prefab");
            m_Pool = new HashSet<GameObject>(maxSize);
            m_Cache = new HashSet<GameObject>(maxSize);
            m_Active = new HashSet<GameObject>(maxSize);
			m_MaxSize = maxSize > 0 ? maxSize : throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            m_CreateFunc = createFunc ?? DefaultCreateFunc;
            m_ActionOnGet = actionOnGet ?? DefaultActionOnGet;
            m_ActionOnRelease = actionOnRelease ?? DefaultActionOnRelease;
            m_ActionOnDestroy = actionOnDestroy ?? DefaultActionOnDestory;
            InitDefaultCapicity(defaultCapacity);
        }

        public GameObject Get()
        {
            GameObject obj;
            if (m_Cache.Count > 0)
            {
                obj = m_Cache.First();
                m_Cache.Remove(obj);
            }
            else if (m_Pool.Count > 0)
            {
                obj = m_Pool.First();
                m_Pool.Remove(obj);
            }
            else
            {
                obj = m_CreateFunc();
            }
            m_Active.Add(obj);
            m_ActionOnGet(obj);
            return obj;
        }

        public void Release(GameObject obj)
        {
            if (m_Pool.Contains(obj))
            {
                throw new InvalidOperationException($"Trying to release an object that has already been released in the pool: {obj}");
            }

            m_ActionOnRelease(obj);
            m_Active.Remove(obj);
            if (CountInactive < m_MaxSize)
            {
                m_Pool.Add(obj);
            }
            else
            {
                m_ActionOnDestroy(obj);
            }
        }

        public void Cache(GameObject obj)
        {
            if (m_Active.Contains(obj))
            {
                m_Active.Remove(obj);
                m_Cache.Add(obj);
            }
            else if (m_Cache.Contains(obj) || m_Pool.Contains(obj))
            {
                throw new InvalidOperationException($"Trying to cache an object that has already been in the pool: {obj}");
            }
            else 
            {
                throw new InvalidOperationException($"Trying to cache an object that does not belong to the pool: {obj}");
            }
        }

        public void CacheReleaseAll()
        {
            foreach (GameObject obj in m_Cache)
            {
                Release(obj);
            }
            m_Cache.Clear();
        }

        public void SpawnedReleaseAll()
        {
            tempList.AddRange(m_Active);
            for (int i = tempList.Count; i >= 0; i--)
            {
                Release(tempList[i]);
            }
            tempList.Clear();
        }

        public void SpawnedCacheAll()
        {
            tempList.AddRange(m_Active);
            for (int i = tempList.Count; i >= 0; i--)
            {
                Cache(tempList[i]);
            }
            tempList.Clear();
        }

        public void RemoveDestoriedObject() 
        {
            RemoveDestoriedObjectInSet(m_Pool);
            RemoveDestoriedObjectInSet(m_Cache);
            RemoveDestoriedObjectInSet(m_Active);
        }

        private void RemoveDestoriedObjectInSet(ISet<GameObject> set) 
        {
            tempList.AddRange(set);
            for (int i = tempList.Count; i >= 0; i--)
            {
                if (!tempList[i])
                {
                    set.Remove(tempList[i]);
                }
            }
            tempList.Clear();
        }

        public bool Contain(GameObject obj) 
        {
            if (m_Pool.Contains(obj) || m_Cache.Contains(obj) || m_Active.Contains(obj)) 
            {
                return true;
            }
            else 
            { 
                return false; 
            }
        }

        public void ClearPooled()
        {
            foreach (GameObject obj in m_Pool)
            {
                m_ActionOnDestroy(obj);
            }
            foreach (GameObject obj in m_Cache)
            {
                m_ActionOnDestroy(obj);
            }
            m_Pool.Clear();
            m_Cache.Clear();
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

        private void InitDefaultCapicity(int defaultCapicity)
        {
            for (int i = 0; i < defaultCapicity; i++)
            {
                var obj = m_CreateFunc();
                m_ActionOnRelease(obj);
                m_Pool.Add(obj);
            }
        }

        private GameObject DefaultCreateFunc()
        {
            return UnityEngine.Object.Instantiate(m_Prefab);
        }

        private void DefaultActionOnGet(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void DefaultActionOnRelease(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void DefaultActionOnDestory(GameObject obj)
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
        private readonly UniPool m_UniPool;

        public T Prefab { get => m_UniPool.Prefab.GetComponent<T>(); }
        public int CountAll { get => m_UniPool.CountAll; }
        public int CountActive { get => m_UniPool.CountActive; }
        public int CountInactive { get => m_UniPool.CountInactive; }
        public int MaxSize { get => m_UniPool.MaxSize; }

        public UniPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000, Func<T> createFunc = null, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null)
        {
            m_UniPool = new UniPool(prefab, defaultCapacity, maxSize, 
                () => createFunc().gameObject,
                obj => actionOnGet(obj.GetComponent<T>()),
                obj => actionOnRelease(obj.GetComponent<T>()),
                obj => actionOnDestroy(obj.GetComponent<T>()));
        }

        public T Get()
        {
            return m_UniPool.Get().GetComponent<T>();
        }

        public void Release(T obj)
        {
            m_UniPool.Release(obj.gameObject);
        }

        public void Cache(T obj)
        {
            m_UniPool.Cache(obj.gameObject);
        }

        public void CacheReleaseAll()
        {
            m_UniPool.CacheReleaseAll();
        }

        public void SpawnedReleaseAll() 
        {
            m_UniPool.SpawnedReleaseAll();
        }

        public void SpawnedCacheAll()
        {
            m_UniPool.SpawnedCacheAll();
        }

        public void RemoveDestoriedObject() 
        {
            m_UniPool.RemoveDestoriedObject();
        }

        public bool Contain(GameObject obj) 
        {
            return m_UniPool.Contain(obj);
        }

        public void ClearPooled()
        {
            m_UniPool.ClearPooled();
        }

        public void ClearAll() 
        {
            m_UniPool.ClearAll();
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