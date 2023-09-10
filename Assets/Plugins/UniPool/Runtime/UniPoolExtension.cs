using System;
using System.Collections.Generic;
using UnityEngine;
using XuanTools.UniPool;

public static class UniPoolExtension
{
    /// <summary>
    /// Register a pool of the prefab in UniPoolManager.
    /// </summary>
    /// <param name="prefab">An object to registered</param>
    /// <param name="defaultCapacity">Initial capacity in pool</param>
    /// <param name="maxCount">Max capacity to pool</param>
    public static void RegisterPool<T>(this T prefab, int defaultCapacity = 10, int maxCount = 10000) where T : Component
    {
        UniPoolManager.RegisterPool(prefab, defaultCapacity, maxCount);
    }
    /// <summary>
    /// Register a pool of the prefab in UniPoolManager.
    /// </summary>
    /// <param name="prefab">An object to registered</param>
    /// <param name="defaultCapacity">Initial capacity in pool</param>
    /// <param name="maxCount">Max capacity to pool</param>
    public static void RegisterPool(this GameObject prefab, int defaultCapacity = 10, int maxCount = 10000)
    {
        UniPoolManager.RegisterPool(prefab, defaultCapacity, maxCount);
    }

    /// <summary>
    /// Spawn the prefab and returns the the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab) where T : Component
    {
        return UniPoolManager.Spawn(prefab);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Transform parent) where T : Component
    {
        return UniPoolManager.Spawn(prefab, parent);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <param name="instantiateInWorldSpace">When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the Object’s position relative to its new parent.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
    {
        return UniPoolManager.Spawn(prefab, parent, instantiateInWorldSpace);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="position">Position for the new object.</param>
    /// <param name="rotation">Orientation of the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return UniPoolManager.Spawn(prefab, position, rotation);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="position">Position for the new object.</param>
    /// <param name="rotation">Orientation of the new object.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
    {
        return UniPoolManager.Spawn(prefab, position, rotation, parent);
    }
    /// <summary>
    /// Spawn the prefab and returns the the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab)
    {
        return UniPoolManager.Spawn(prefab);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Transform parent)
    {
        return UniPoolManager.Spawn(prefab, parent);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <param name="instantiateInWorldSpace">When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the Object’s position relative to its new parent.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Transform parent, bool instantiateInWorldSpace)
    {
        return UniPoolManager.Spawn(prefab, parent, instantiateInWorldSpace);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="position">Position for the new object.</param>
    /// <param name="rotation">Orientation of the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return UniPoolManager.Spawn(prefab, position, rotation);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="position">Position for the new object.</param>
    /// <param name="rotation">Orientation of the new object.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return UniPoolManager.Spawn(prefab, position, rotation, parent);
    }

    /// <summary>
    /// Spawn a specified number of prefab and returns the the list spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="count">Number you need to spawned.</param>
    /// <returns>Object spawned from pool.</returns>
    public static List<GameObject> SpawnList(this GameObject prefab, int count)
    {
        return UniPoolManager.SpawnList(prefab, count);
    }

    /// <summary>
    /// Spawn a specified number of prefab and returns the the list spawned.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="count">Number you need to spawned.</param>
    /// <param name="actionAfterGet">Action after object spawned.</param>
    /// <returns>Object spawned from pool.</returns>
    public static List<GameObject> SpawnList(this GameObject prefab, int count, Action<GameObject> actionAfterGet)
    {
        return UniPoolManager.SpawnList(prefab, count, actionAfterGet);
    }

    /// <summary>
    /// Spawn a specified number of prefab to existing list.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="list">An existing list to spawn object</param>
    /// <param name="count">Number you need to spawned.</param>
    public static void SpawnToList(this GameObject prefab, List<GameObject> list, int count)
    { 
        UniPoolManager.SpawnToList(prefab, list, count);
    }

    /// <summary>
    /// Spawn a specified number of prefab to existing list.
    /// </summary>
    /// <param name="prefab">An registered object that you want to make spawn of.</param>
    /// <param name="list">An existing list to spawn object</param>
    /// <param name="count">Number you need to spawned.</param>
    /// <param name="actionAfterGet">Action after object spawned.</param>
    /// <returns>Object spawned from pool.</returns>
    public static void SpawnToList(this GameObject prefab, List<GameObject> list, int count, Action<GameObject> actionAfterGet)
    {
        UniPoolManager.SpawnToList(prefab, list, count, actionAfterGet);
    }

    /// <summary>
    /// Recycle the game object to UniPoolManager.
    /// </summary>
    public static void Recycle<T>(this T obj) where T : Component
    {
        UniPoolManager.Recycle(obj);
    }
    /// <summary>
    /// Recycle the game object to UniPoolManager.
    /// </summary>
    public static void Recycle(this GameObject obj)
    {
        UniPoolManager.Recycle(obj);
    }

    /// <summary>
    /// Recycle the game object to UniPoolManager immediately. It is recommended to use Recycle.
    /// </summary>
    public static void RecycleImmediate<T>(this T obj) where T : Component
    {
        UniPoolManager.RecycleImmediate(obj);
    }
    /// <summary>
    /// Recycle the game object to UniPoolManager immediately. It is recommended to use Recycle.
    /// </summary>
    public static void RecycleImmediate(this GameObject obj)
    {
        UniPoolManager.RecycleImmediate(obj);
    }

    /// <summary>
    /// Recycle all spawned object of the prefab to UniPoolManager.
    /// </summary>
    public static void RecycleAll<T>(this T prefab) where T : Component
    {
        UniPoolManager.RecycleAll(prefab.gameObject);
    }
    /// <summary>
    /// Recycle all spawned object of the prefab to UniPoolManager.
    /// </summary>
    public static void RecycleAll(this GameObject prefab)
    {
        UniPoolManager.RecycleAll(prefab);
    }

    /// <summary>
    /// Recycle all spawned object of the prefab to UniPoolManager immediately. It is recommended to use RecycleAll.
    /// </summary>
    public static void RecycleAllImmediate<T>(this T prefab) where T : Component
    {
        UniPoolManager.RecycleAllImmediate(prefab.gameObject);
    }
    /// <summary>
    /// Recycle all spawned object of the prefab to UniPoolManager immediately. It is recommended to use RecycleAll.
    /// </summary>
    public static void RecycleAllImmediate(this GameObject prefab)
    {
        UniPoolManager.RecycleAllImmediate(prefab);
    }

    /// <summary>
    /// Get if UniPoolManager contain pool of the prefab.
    /// </summary>
    public static bool ContainPool<T>(this T prefab) where T : Component
    {
        return UniPoolManager.ContainPool(prefab);
    }
    /// <summary>
    /// Get if UniPoolManager contain pool of the prefab.
    /// </summary>
    public static bool ContainPool(this GameObject prefab)
    {
        return UniPoolManager.ContainPool(prefab);
    }

    /// <summary>
    /// Get if the game object is spawn in UniPoolManager.
    /// </summary>
    public static bool ContainObject<T>(this T obj) where T : Component
    {
        return UniPoolManager.ContainObject(obj);
    }
    /// <summary>
    /// Get if the game object is spawn in UniPoolManager.
    /// </summary>
    public static bool ContainObject(this GameObject obj)
    {
        return UniPoolManager.ContainObject(obj);
    }

    /// <summary>
    /// Dispose pooled object of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposePooled<T>(this T prefab) where T : Component
    {
        UniPoolManager.DisposePooled(prefab);
    }
    /// <summary>
    /// Dispose pooled object of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposePooled(this GameObject prefab)
    {
        UniPoolManager.DisposePooled(prefab);
    }

    /// <summary>
    /// Dispose all object including spawned of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposeAll<T>(this T prefab) where T : Component
    {
        UniPoolManager.DisposeAll(prefab);
    }
    /// <summary>
    /// Dispose all object including spawned of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposeAll(this GameObject prefab)
    {
        UniPoolManager.DisposeAll(prefab);
    }
}
