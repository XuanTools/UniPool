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

#if UNITY_2021_2_OR_NEWER
        private readonly Dictionary<GameObject, UniPool> _prefabToUniPoolDict = new();
        private readonly Dictionary<GameObject, GameObject> _objectToPrefabDict = new();
        private readonly Dictionary<GameObject, Transform> _prefabToParentDict = new();
#else
        private readonly Dictionary<GameObject, UniPool> _prefabToUniPoolDict = new Dictionary<GameObject, UniPool>();
        private readonly Dictionary<GameObject, GameObject> _objectToPrefabDict = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, Transform> _prefabToParentDict = new Dictionary<GameObject, Transform>();
#endif

        public bool Initialized { get; private set; }
        public int PoolCount => _prefabToUniPoolDict.Count;

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
                GetOrRegisterPool(pool.Prefab, pool.DefaultCapacity, pool.MaxSize);
            }
        }

        public static void RegisterPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 10000) where T : Component
        {
            GetOrRegisterPool(prefab.gameObject, defaultCapacity, maxSize);
        }
        public static void RegisterPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000)
        {
            GetOrRegisterPool(prefab, defaultCapacity, maxSize);
        }
        private static UniPool GetOrRegisterPool(GameObject prefab, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (Instance._prefabToUniPoolDict.ContainsKey(prefab)) return Instance._prefabToUniPoolDict[prefab];

            // Create a new pool and return
            var parent = new GameObject(prefab.name).transform;
            parent.parent = Instance.transform;
            parent.gameObject.SetActive(false);
            Instance._prefabToParentDict.Add(prefab, parent);

            var pool = new UniPool(prefab, defaultCapacity, maxSize,
                () =>
                {
                    var obj = Instantiate(prefab);
                    Instance._objectToPrefabDict.Add(obj, prefab);
                    return obj;
                },
                _ => { },
#if UNITY_2021_2_OR_NEWER
                obj => obj.transform.SetParent(parent, obj.transform is not RectTransform),
#else
                obj => obj.transform.SetParent(parent, !(obj.transform is RectTransform)),
#endif
                obj =>
                {
                    Destroy(obj);
                    Instance._objectToPrefabDict.Remove(obj);
                });

            Instance._prefabToUniPoolDict.Add(prefab, pool);
            return pool;
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
            var pool = GetOrRegisterPool(prefab, 0);
            var obj = pool.Get();
#if UNITY_2021_2_OR_NEWER
            obj.transform.SetParent(parent, prefab.transform is not RectTransform);
#else
            obj.transform.SetParent(parent, !(prefab.transform is RectTransform));
#endif
            if (instantiateInWorldSpace)
            {
                obj.transform.SetPositionAndRotation(prefab.transform.position, prefab.transform.rotation);
                obj.transform.localScale = prefab.transform.localScale;
            }
            else
            {
#if UNITY_2021_3_OR_NEWER
                obj.transform.SetLocalPositionAndRotation(prefab.transform.position, prefab.transform.rotation);
#else
                obj.transform.localPosition = prefab.transform.position;
                obj.transform.localRotation = prefab.transform.rotation;
#endif
                obj.transform.localScale = prefab.transform.localScale;
            }

            return obj;
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation, null);
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            var pool = GetOrRegisterPool(prefab, 0);
            var obj = pool.Get();
#if UNITY_2021_2_OR_NEWER
            obj.transform.SetParent(parent, prefab.transform is not RectTransform);
#else
            obj.transform.SetParent(parent, !(obj.transform is RectTransform));
#endif
#if UNITY_2021_3_OR_NEWER
            obj.transform.SetLocalPositionAndRotation(position, rotation);
#else
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
#endif
            obj.transform.localScale = prefab.transform.localScale;
            return obj;
        }

        public static List<T> SpawnList<T>(T prefab, int count, Action<T> actionAfterGet = null)
            where T : Component
        {
            return SpawnList(prefab.gameObject, count, obj => actionAfterGet?.Invoke(obj.GetComponent<T>()))
                .ConvertAll(obj => obj.GetComponent<T>());
        }
        public static List<GameObject> SpawnList(GameObject prefab, int count, Action<GameObject> actionAfterGet = null)
        {
            return GetOrRegisterPool(prefab, count).GetList(count, actionAfterGet);
        }

        public static void SpawnToList<T>(T prefab, List<T> list, int count, Action<T> actionAfterGet = null)
            where T : Component
        {
            list.AddRange(SpawnList(prefab, count, actionAfterGet));
        }
        public static void SpawnToList(GameObject prefab, List<GameObject> list, int count, Action<GameObject> actionAfterGet = null)
        {
            GetOrRegisterPool(prefab, count).GetToList(list, count, actionAfterGet);
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }
        public static void Recycle(GameObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

#if UNITY_2018_2_OR_NEWER
            if (Instance._objectToPrefabDict.TryGetValue(obj, out var prefab))
            {
                Recycle(obj, prefab);
            }
#else
            GameObject prefab;
            if (Instance._objectToPrefabDict.TryGetValue(obj, out prefab))
            {
                Recycle(obj, prefab);
            }
#endif
            else
            {
                Destroy(obj);
            }
        }
        private static void Recycle(GameObject obj, GameObject prefab)
        {
            // Check if contain to avoid cache same object again before cache recycle.
#if UNITY_2018_2_OR_NEWER
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out var pool))
            {
                // if (!pool._cache.Contains(obj))
                pool.Cache(obj);
            }
#else
            UniPool pool;
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out pool))
            {
                // if (!pool._cache.Contains(obj))
                pool.Cache(obj);
            }
#endif
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
#if UNITY_2018_2_OR_NEWER
            if (Instance._objectToPrefabDict.TryGetValue(obj, out var prefab))
            {
                RecycleImmediate(obj, prefab);
            }
#else
            GameObject prefab;
            if (Instance._objectToPrefabDict.TryGetValue(obj, out prefab))
            {
                RecycleImmediate(obj, prefab);
            }
#endif
            else
            {
                Destroy(obj);
            }
        }
        private static void RecycleImmediate(GameObject obj, GameObject prefab)
        {
#if UNITY_2018_2_OR_NEWER
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out var pool))
            {
                pool.Release(obj);
            }
#else
            UniPool pool;
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out pool))
            {
                pool.Release(obj);
            }
#endif
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
#if UNITY_2018_2_OR_NEWER
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out var pool))
            {
                pool.SpawnedCacheAll();
            }
#else
            UniPool pool;
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out pool))
            {
                pool.SpawnedCacheAll();
            }
#endif
        }

        public static void RecycleAllImmediate<T>(T prefab) where T : Component
        {
            RecycleAllImmediate(prefab.gameObject);
        }
        public static void RecycleAllImmediate(GameObject prefab)
        {
#if UNITY_2018_2_OR_NEWER
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out var pool))
            {
                pool.SpawnedReleaseAll();
            }
#else
            UniPool pool;
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out pool))
            {
                pool.SpawnedReleaseAll();
            }
#endif
        }

        public static void CacheRecycle<T>(T prefab) where T : Component
        {
            CacheRecycle(prefab.gameObject);
        }
        public static void CacheRecycle(GameObject prefab)
        {
#if UNITY_2018_2_OR_NEWER
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out var pool))
            {
                pool.CacheReleaseAll();
            }
#else
            UniPool pool;
            if (Instance._prefabToUniPoolDict.TryGetValue(prefab, out pool))
            {
                pool.CacheReleaseAll();
            }
#endif
        }

        public static void CacheRecycleAll()
        {
            foreach (var pool in Instance._prefabToUniPoolDict.Values)
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
            return Instance._prefabToUniPoolDict.ContainsKey(prefab);
        }

        public static T GetPrefab<T>(T obj) where T : Component
        {
            return GetPrefab(obj.gameObject).GetComponent<T>();
        }
        public static GameObject GetPrefab(GameObject obj)
        {
#if UNITY_2018_2_OR_NEWER
            Instance._objectToPrefabDict.TryGetValue(obj, out var prefab);
#else
            GameObject prefab;
            Instance._objectToPrefabDict.TryGetValue(obj, out prefab);
#endif
            return prefab;
        }

        public static bool TryGetPrefab<T>(T obj, out T prefab) where T : Component
        {
#if UNITY_2018_2_OR_NEWER
            var flag = TryGetPrefab(obj.gameObject, out var get);
#else
            GameObject get;
            var flag = TryGetPrefab(obj.gameObject, out get);
#endif
            prefab = get.GetComponent<T>();
            return flag;
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
            Instance._prefabToUniPoolDict[prefab].ClearPooled();
        }

        public static void DisposeAll<T>(T prefab) where T : Component
        {
            DisposeAll(prefab.gameObject);
        }
        public static void DisposeAll(GameObject prefab)
        {
            Instance._prefabToUniPoolDict[prefab].ClearAll();
            Instance._prefabToUniPoolDict.Remove(prefab);
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

#if UNITY_2022_2_OR_NEWER || UNITY_2021_3 || UNITY_2020_3
                _instance = FindAnyObjectByType<T>();
#else
                _instance = FindObjectOfType<T>();
#endif
                if (_instance != null)
                    return _instance;

                var obj = new GameObject(typeof(T).Name);
#if UNITY_2021_3_OR_NEWER
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
#endif
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