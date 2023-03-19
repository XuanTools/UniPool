using UnityEngine;
using XuanTools.UniPool;

public static class UniPoolExtentions
{
    /// <summary>
    /// Regist a pool of the prefab in UniPoolManager.
    /// </summary>
    /// <param name="prefab">An object to registed</param>
    /// <param name="defaultCapacity">Initial capacity in pool</param>
    /// <param name="maxCount">Max capacity to pool</param>
    /// <param name="worldPositionStays">Whether to stay world position in recycle</param>
    public static void RegistPool<T>(this T prefab, int defaultCapacity = 10, int maxCount = 10000, bool worldPositionStays = true) where T : Component
    {
        UniPoolManager.RegistPool(prefab, defaultCapacity, maxCount, worldPositionStays);
    }
    /// <summary>
    /// Regist a pool of the prefab in UniPoolManager.
    /// </summary>
    /// <param name="prefab">An object to registed</param>
    /// <param name="defaultCapacity">Initial capacity in pool</param>
    /// <param name="maxCount">Max capacity to pool</param>
    /// <param name="worldPositionStays">Whether to stay world position in recycle</param>
    public static void RegistPool(this GameObject prefab, int defaultCapacity = 10, int maxCount = 10000, bool worldPositionStays = true)
    {
        UniPoolManager.RegistPool(prefab, defaultCapacity, maxCount, worldPositionStays);
    }

    /// <summary>
    /// Spawn the prefab and returns the the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab) where T : Component
    {
        return UniPoolManager.Spawn(prefab);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Transform parent) where T : Component
    {
        return UniPoolManager.Spawn(prefab, parent);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <param name="instantiateInWorldSpace">When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the Object¡¯s position relative to its new parent.</param>
    /// <returns>Object spawned from pool.</returns>
    public static T Spawn<T>(this T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
    {
        return UniPoolManager.Spawn(prefab, parent, instantiateInWorldSpace);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
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
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
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
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab)
    {
        return UniPoolManager.Spawn(prefab);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Transform parent)
    {
        return UniPoolManager.Spawn(prefab, parent);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <param name="instantiateInWorldSpace">When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the Object¡¯s position relative to its new parent.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Transform parent, bool instantiateInWorldSpace)
    {
        return UniPoolManager.Spawn(prefab, parent, instantiateInWorldSpace);
    }
    /// <summary>
    /// Spawn the prefab and returns the object spawned.
    /// </summary>
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
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
    /// <param name="prefab">An registed object that you want to make spawn of.</param>
    /// <param name="position">Position for the new object.</param>
    /// <param name="rotation">Orientation of the new object.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <returns>Object spawned from pool.</returns>
    public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return UniPoolManager.Spawn(prefab, position, rotation, parent);
    }

    /// <summary>
    /// Recycle the gameobject to UniPoolManager.
    /// </summary>
    public static void Recycle<T>(this T obj) where T : Component
    {
        UniPoolManager.Recycle(obj);
    }
    /// <summary>
    /// Recycle the gameobject to UniPoolManager.
    /// </summary>
    public static void Recycle(this GameObject obj)
    {
        UniPoolManager.Recycle(obj);
    }

    /// <summary>
    /// Recycle the gameobject to UniPoolManager immediately. It is recommended to use Recycle.
    /// </summary>
    public static void RecycleImmediate<T>(this T obj) where T : Component
    {
        UniPoolManager.RecycleImmediate(obj);
    }
    /// <summary>
    /// Recycle the gameobject to UniPoolManager immediately. It is recommended to use Recycle.
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
    /// Get if the gameobject is spawn in UniPoolManager.
    /// </summary>
    public static bool ContainObject<T>(this T obj) where T : Component
    {
        return UniPoolManager.ContainObject(obj);
    }
    /// <summary>
    /// Get if the gameobject is spawn in UniPoolManager.
    /// </summary>
    public static bool ContainObject(this GameObject obj)
    {
        return UniPoolManager.ContainObject(obj);
    }

    /// <summary>
    /// Dislope pooled object of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposePooled<T>(this T prefab) where T : Component
    {
        UniPoolManager.DisposePooled(prefab);
    }
    /// <summary>
    /// Dislope pooled object of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposePooled(this GameObject prefab)
    {
        UniPoolManager.DisposePooled(prefab);
    }

    /// <summary>
    /// Dislope all object including spawned of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposeAll<T>(this T prefab) where T : Component
    {
        UniPoolManager.DisposeAll(prefab);
    }
    /// <summary>
    /// Dislope all object including spawned of the prefab in UniPoolManager.
    /// </summary>
    public static void DisposeAll(this GameObject prefab)
    {
        UniPoolManager.DisposeAll(prefab);
    }
}
