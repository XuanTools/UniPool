using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectController : MonoBehaviour
{
    public GameObject Prefab;
    public int Count;

    public Transform InstantiateParent;
    public Transform ObjectPoolParent;
    public Transform UniPoolParent;

    public bool ByInstance = false;
    public bool ByObjectPool = false;
    public bool ByUniPool = false;

    private List<GameObject> _activeByInstantiate;
    private List<GameObject> _activeByObjectPool;
    private List<GameObject> _activeByUniPool;

    private ObjectPool<GameObject> _objectPool;

    private void Start()
    {
        _objectPool = new ObjectPool<GameObject>(
            () => Instantiate(Prefab, InstantiateParent),
            obj => obj.SetActive(true),
            obj => obj.SetActive(false),
            Destroy);

        _activeByInstantiate = new List<GameObject>(Count);
        _activeByObjectPool = new List<GameObject>(Count);
        _activeByUniPool = new List<GameObject>(Count);
    }

    private void Update()
    {
        // Recycle or destroy generated objects.
        RecycleObjects();
        // Generate objects by selected way.
        GenerateObjects();
    }

    private void RecycleObjects()
    {
        foreach (var obj in _activeByInstantiate)
        {
            Destroy(obj);
        }

        _activeByInstantiate.Clear();

        foreach (var obj in _activeByObjectPool)
        {
            _objectPool.Release(obj);
        }

        _activeByObjectPool.Clear();

        foreach (var obj in _activeByUniPool)
        {
            obj.Recycle();
        }

        _activeByUniPool.Clear();
    }

    private void GenerateObjects()
    {
        if (ByInstance)
        {
            for (var i = 0; i < Count; i++)
            {
                _activeByInstantiate.Add(
                    Instantiate(Prefab, Random.insideUnitCircle * 12f, Random.rotation, InstantiateParent));
            }
        }
        else if (ByObjectPool)
        {
            for (var i = 0; i < Count; i++)
            {
                var obj = _objectPool.Get();
                _activeByObjectPool.Add(obj);
                obj.transform.SetPositionAndRotation(Random.insideUnitCircle * 12f, Random.rotation);
            }
        }
        else if (ByUniPool)
        {
            for (var i = 0; i < Count; i++)
            {
                _activeByUniPool.Add(Prefab.Spawn(Random.insideUnitCircle * 12f, Random.rotation, UniPoolParent));
            }
        }
    }
}
