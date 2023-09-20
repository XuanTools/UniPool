using System;
using System.Collections.Generic;
using UnityEngine;

namespace XuanTools.UniPool
{
    public sealed class UniPoolManager : MonoSingleton<UniPoolManager>
    {
        [Serializable]
        public class InitialPool
        {
            public GameObject Prefab;
            public int DefaultCapacity = 10;
            public int MaxSize = 1000;
        }

        public InitialPool[] InitialPools;

        private readonly Dictionary<GameObject, UniPool> _uniPools = new();
        private readonly Dictionary<GameObject, GameObject> _objectToPrefabDict = new();
        private readonly Dictionary<GameObject, Transform> _prefabToParentDict = new();

        public bool Initialized { get; private set; }
        public int PoolCount => _uniPools.Count;

        protected override void OnAwake()
        {
            RegisterInitialPools();
        }

        private void LateUpdate()
        {
            CacheRecycleAll();
        }

        public void RegisterInitialPools()
        {
            if (Initialized) return;
            Initialized = true;

            if (InitialPools == null) return;
            foreach (var pool in InitialPools)
            {
                RegisterPool(pool.Prefab, pool.DefaultCapacity, pool.MaxSize);
            }
        }

        public static void RegisterPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 10000) where T : Component
        {
            RegisterPool(prefab.gameObject, defaultCapacity, maxSize);
        }

        public static void RegisterPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (Instance._uniPools.ContainsKey(prefab)) return;

            var parent = new GameObject(prefab.name).transform;
            parent.parent = Instance.transform;
            parent.gameObject.SetActive(false);
            Instance._prefabToParentDict.Add(prefab, parent);

            Instance._uniPools.Add(prefab,
                new UniPool(prefab, defaultCapacity, maxSize,
                    () =>
                    {
                        var obj = Instantiate(prefab);
                        Instance._objectToPrefabDict.Add(obj, prefab);
                        return obj;
                    },
                    _ => { },
                    obj => obj.transform.SetParent(parent, !obj.TryGetComponent<RectTransform>(out _)),
                    obj =>
                    {
                        Destroy(obj);
                        Instance._objectToPrefabDict.Remove(obj);
                    }));
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
            return Spawn(prefab, parent, !prefab.TryGetComponent<RectTransform>(out _));
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, bool instantiateInWorldSpace)
        {
            if (!Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                // Create pool if not exist.
                RegisterPool(prefab, 0);
                pool = Instance._uniPools[prefab];
            }
            var obj = pool.Get();
            obj.transform.SetParent(parent, instantiateInWorldSpace);
            return obj;
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation, null);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                // Create pool if not exist.
                RegisterPool(prefab, 0);
                pool = Instance._uniPools[prefab];
            }
            var obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.SetParent(parent, !prefab.TryGetComponent<RectTransform>(out _));
            return obj;
        }

        public static List<GameObject> SpawnList(GameObject prefab, int count)
        {
            if (Instance._uniPools.TryGetValue(prefab, out var pool)) return pool.GetList(count);

            // Create pool if not exist.
            RegisterPool(prefab, count);
            pool = Instance._uniPools[prefab];
            return pool.GetList(count);
        }
        public static List<GameObject> SpawnList(GameObject prefab, int count, Action<GameObject> actionAfterGet)
        {
            if (Instance._uniPools.TryGetValue(prefab, out var pool)) return pool.GetList(count, actionAfterGet);

            // Create pool if not exist.
            RegisterPool(prefab, count);
            pool = Instance._uniPools[prefab];
            return pool.GetList(count, actionAfterGet);
        }

        public static void SpawnToList(GameObject prefab, List<GameObject> list, int count)
        {
            if (!Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                // Create pool if not exist.
                RegisterPool(prefab, count);
                pool = Instance._uniPools[prefab];
            }

            pool.GetToList(list, count);
        }
        public static void SpawnToList(GameObject prefab, List<GameObject> list, int count, Action<GameObject> actionAfterGet)
        {
            if (!Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                // Create pool if not exist.
                RegisterPool(prefab, count);
                pool = Instance._uniPools[prefab];
            }

            pool.GetToList(list, count, actionAfterGet);
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }
        public static void Recycle(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (Instance._objectToPrefabDict.TryGetValue(obj, out var prefab))
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
            if (Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                // Check if contain to avoid cache same object again before cache recycle.
                // if (!pool._cache.Contains(obj))
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
            if (Instance._objectToPrefabDict.TryGetValue(obj, out var prefab))
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
            if (Instance._uniPools.TryGetValue(prefab, out var pool))
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
            if (Instance._uniPools.TryGetValue(prefab, out var pool))
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
            if (Instance._uniPools.TryGetValue(prefab, out var pool))
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
            if (Instance._uniPools.TryGetValue(prefab, out var pool))
            {
                pool.CacheReleaseAll();
            }
        }

        public static void CacheRecycleAll()
        {
            foreach (var pool in Instance._uniPools.Values)
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
            return Instance._uniPools.ContainsKey(prefab);
        }

        public static T GetPrefab<T>(T obj) where T : Component
        {
            return Instance._objectToPrefabDict.GetValueOrDefault(obj.gameObject, null).GetComponent<T>();
        }
        public static GameObject GetPrefab(GameObject obj)
        {
            return Instance._objectToPrefabDict.GetValueOrDefault(obj, null);
        }

        public static bool TryGetPrefab<T>(T obj, out T prefab) where T : Component
        {
            if (Instance._objectToPrefabDict.TryGetValue(obj.gameObject, out var get))
            {
                prefab = get.GetComponent<T>();
                return true;
            }

            prefab = null;
            return false;
        }

        public static bool TryGetPrefab(GameObject obj, out GameObject prefab)
        {
            return Instance._objectToPrefabDict.TryGetValue(obj, out prefab);
        }

        public static List<GameObject> GetAllPooledPrefabs()
        {
            return new List<GameObject>(Instance._prefabToParentDict.Keys);
        }

        public static bool ContainObject<T>(T obj) where T : Component
        {
            return ContainObject(obj.gameObject);
        }
        public static bool ContainObject(GameObject obj)
        {
            return Instance._objectToPrefabDict.ContainsKey(obj);
        }


        public static void DisposePooled<T>(T prefab) where T : Component
        {
            DisposePooled(prefab.gameObject);
        }
        public static void DisposePooled(GameObject prefab)
        {
            Instance._uniPools[prefab].ClearPooled();
        }

        public static void DisposeAll<T>(T prefab) where T : Component
        {
            DisposeAll(prefab.gameObject);
        }
        public static void DisposeAll(GameObject prefab)
        {
            Instance._uniPools[prefab].ClearAll();
            Instance._uniPools.Remove(prefab);
            Destroy(Instance._prefabToParentDict[prefab].gameObject);
            Instance._prefabToParentDict.Remove(prefab);
        }
    }

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null)
                    return _instance;

                var obj = new GameObject(typeof(T).Name);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                obj.transform.localScale = Vector3.one;
                _instance = obj.AddComponent<T>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this as T;
                OnAwake();
            }
        }

        protected virtual void OnAwake()
        {

        }
    }
}