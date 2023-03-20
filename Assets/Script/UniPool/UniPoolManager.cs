using System.Collections.Generic;
using UnityEngine;

namespace XuanTools.UniPool
{
    public sealed class UniPoolManager : MonoSingelton<UniPoolManager>
    {
        [System.Serializable]
        public class InitialPool
        {
            public GameObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 1000;
            public bool worldPositionStays = true;
        }

        public InitialPool[] initialPools;

        private readonly Dictionary<GameObject, UniPool> m_UniPools = new();
        private readonly Dictionary<GameObject, GameObject> m_ObjectToPrefabDict = new();
        private readonly Dictionary<GameObject, Transform> m_PrefabToParentDict = new();

        public bool Initialized { get; private set; } = false;
        public int PoolCount { get => m_UniPools.Count; }

        protected override void OnAwake()
        {
            RegistInitialPools();
        }

        private void LateUpdate()
        {
            CacheRecycleAll();
        }

        public void RegistInitialPools()
        {
            if (!Initialized)
            {
                Initialized = true;
                if (initialPools != null)
                {
                    foreach (var pool in initialPools)
                    {
                        RegistPool(pool.prefab, pool.defaultCapacity, pool.maxSize, pool.worldPositionStays);
                    }
                }
            }
        }

        public static void RegistPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 10000, bool worldPositionStays = true) where T : Component
        {
            RegistPool(prefab.gameObject, defaultCapacity, maxSize, worldPositionStays);
        }

        public static void RegistPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000, bool worldPositionStays = true)
        {
            if (!Instance.m_UniPools.ContainsKey(prefab))
            {
                Transform parent = new GameObject(prefab.name).transform;
                parent.parent = Instance.transform;
                parent.gameObject.SetActive(false);
                Instance.m_PrefabToParentDict.Add(prefab, parent);

                Instance.m_UniPools.Add(prefab,
                    new UniPool(prefab, defaultCapacity, maxSize,
                        () =>
                        {
                            var obj = Instantiate(prefab);
                            Instance.m_ObjectToPrefabDict.Add(obj, prefab);
                            return obj;
                        },
                        obj => { },
                        obj => obj.transform.SetParent(parent, worldPositionStays),
                        obj =>
                        {
                            Destroy(obj);
                            Instance.m_ObjectToPrefabDict.Remove(obj);
                        }));
            }
        }

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            return Spawn(prefab.gameObject, parent).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
        {
            return Spawn(prefab.gameObject, parent, instantiateInWorldSpace).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            return Spawn(prefab.gameObject, position, rotation, parent).GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, null);
        }
        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Spawn(prefab, parent, false);
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, bool instantiateInWorldSpace)
        {
            GameObject obj;
            if (!Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                // Creat pool if not exist.
                RegistPool(prefab, 0);
                pool = Instance.m_UniPools[prefab];
            }
            obj = pool.Get();
            obj.transform.SetParent(parent, instantiateInWorldSpace);
            return obj;
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation, null);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject obj;
            if (!Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                // Creat pool if not exist.
                RegistPool(prefab, 0);
                pool = Instance.m_UniPools[prefab];
            }
            obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.SetParent(parent);
            return obj;
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }
        public static void Recycle(GameObject obj)
        {
            if (Instance.m_ObjectToPrefabDict.TryGetValue(obj, out var prefab))
            {
                Recycle(obj, prefab);
            }
            else
            {
                Destroy(obj);
            }
        }
        private static void Recycle(GameObject obj, GameObject prefab)
        {
            if (Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                // Check if contain to avoid cache same object again before cache recycle.
                if (!pool.m_Cache.Contains(obj))
                    pool.Cache(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        public static void RecycleImmediate<T>(T obj) where T : Component
        {
            RecycleImmediate(obj.gameObject);
        }
        public static void RecycleImmediate(GameObject obj)
        {
            if (Instance.m_ObjectToPrefabDict.TryGetValue(obj, out var prefab))
            {
                RecycleImmediate(obj, prefab);
            }
            else
            {
                Destroy(obj);
            }
        }
        private static void RecycleImmediate(GameObject obj, GameObject prefab)
        {
            if (Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        public static void RecycleAll<T>(T prefab) where T : Component
        {
            RecycleAll(prefab.gameObject);
        }
        public static void RecycleAll(GameObject prefab)
        {
            if (Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                pool.SpawnedCacheAll();
            }
        }

        public static void RecycleAllImmediate<T>(T prefab) where T : Component
        {
            RecycleAllImmediate(prefab.gameObject);
        }
        public static void RecycleAllImmediate(GameObject prefab)
        {
            if (Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                pool.SpawnedReleaseAll();
            }
        }

        public static void CacheRecycle<T>(T prefab) where T : Component
        {
            CacheRecycle(prefab.gameObject);
        }
        public static void CacheRecycle(GameObject prefab)
        {
            if (Instance.m_UniPools.TryGetValue(prefab, out var pool))
            {
                pool.CacheReleaseAll();
            }
        }

        public static void CacheRecycleAll()
        {
            foreach (var pool in Instance.m_UniPools.Values)
            {
                pool.CacheReleaseAll();
            }
        }

        public static bool ContainPool<T>(T prefab) where T : Component
        {
            return ContainPool(prefab.gameObject);
        }
        public static bool ContainPool(GameObject prefab)
        {
            return Instance.m_UniPools.ContainsKey(prefab);
        }

        public static T GetPrefab<T>(T obj) where T : Component
        {
            return Instance.m_ObjectToPrefabDict.GetValueOrDefault(obj.gameObject, null).GetComponent<T>();
        }
        public static GameObject GetPrefab(GameObject obj)
        {
            return Instance.m_ObjectToPrefabDict.GetValueOrDefault(obj, null);
        }

        public static bool TryGetPrefab<T>(T obj, out T prefab) where T : Component
        {
            bool flag = Instance.m_ObjectToPrefabDict.TryGetValue(obj.gameObject, out GameObject get);
            prefab = get.GetComponent<T>();
            return flag;
        }
        public static bool TryGetPrefab(GameObject obj, out GameObject prefab)
        {
            return Instance.m_ObjectToPrefabDict.TryGetValue(obj, out prefab);
        }

        public static List<GameObject> GetAllPooledPrefabs()
        {
            return new(Instance.m_PrefabToParentDict.Keys);
        }

        public static bool ContainObject<T>(T obj) where T : Component
        {
            return ContainObject(obj.gameObject);
        }
        public static bool ContainObject(GameObject obj)
        {
            return Instance.m_ObjectToPrefabDict.ContainsKey(obj);
        }


        public static void DisposePooled<T>(T prefab) where T : Component
        {
            DisposePooled(prefab.gameObject);
        }
        public static void DisposePooled(GameObject prefab)
        {
            Instance.m_UniPools[prefab].ClearPooled();
            Instance.m_UniPools.Remove(prefab);
            Destroy(Instance.m_PrefabToParentDict[prefab].gameObject);
            Instance.m_PrefabToParentDict.Remove(prefab);
        }

        public static void DisposeAll<T>(T prefab) where T : Component
        {
            DisposeAll(prefab.gameObject);
        }
        public static void DisposeAll(GameObject prefab)
        {
            Instance.m_UniPools[prefab].ClearAll();
            Instance.m_UniPools.Remove(prefab);
            Destroy(Instance.m_PrefabToParentDict[prefab].gameObject);
            Instance.m_PrefabToParentDict.Remove(prefab);
        }
    }

    public abstract class MonoSingelton<T> : MonoBehaviour where T : MonoBehaviour, new()
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_Instance != null)
                    return m_Instance;

                m_Instance = FindObjectOfType<T>();
                if (m_Instance != null)
                    return m_Instance;

                var obj = new GameObject(nameof(T));
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                obj.transform.localScale = Vector3.one;
                m_Instance = obj.AddComponent<T>();
                return m_Instance;
            }
        }

        private void Awake()
        {
            if (m_Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_Instance = this as T;
                OnAwake();
            }
        }

        protected virtual void OnAwake()
        {

        }
    }
}