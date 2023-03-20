using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int count;

    public Transform instance;
    public Transform objectPool;
    public Transform uniPool;

    public bool byInstance = false;
    public bool byObjectPool = false;
    public bool byUniPool = false;

    private ObjectPool<GameObject> pool;

    private void Start()
    {
        pool = new(() => Instantiate(prefab),
            obj => obj.SetActive(true),
            obj => obj.SetActive(false),
            obj => Destroy(obj));
    }

    private void Update()
    {
        foreach (Transform obj in instance.GetComponentsInChildren<Transform>())
        {
            if (obj != instance) Destroy(obj.gameObject);
        }
        foreach (Transform obj in objectPool.GetComponentsInChildren<Transform>())
        {
            if (obj.gameObject.activeSelf && obj != objectPool) pool.Release(obj.gameObject);
        }
        foreach (Transform obj in uniPool.GetComponentsInChildren<Transform>())
        {
            if (obj != uniPool) obj.Recycle();
        }

        if (byInstance)
        {
            for (int i = 0; i < count; i++)
            {
                Instantiate(prefab, Random.insideUnitCircle * 12f, Random.rotation, instance);
            }
        }
        else if (byObjectPool)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                obj.transform.SetParent(objectPool);
                obj.transform.SetLocalPositionAndRotation(Random.insideUnitCircle * 12f, Random.rotation);
            }
        }
        else if (byUniPool)
        {
            for (int i = 0; i < count; i++)
            {
                prefab.Spawn(Random.insideUnitCircle * 12f, Random.rotation, uniPool);
            }
        }
    }
}
