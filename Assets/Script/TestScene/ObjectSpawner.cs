using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int count;

    public Transform instantiate;
    public Transform objectPool;
    public Transform uniPool;

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
        foreach (Transform obj in instantiate.GetComponentsInChildren<Transform>())
        {
            if (obj != instantiate) Destroy(obj.gameObject);
        }
        foreach (Transform obj in objectPool.GetComponentsInChildren<Transform>())
        {
            if (obj.gameObject.activeSelf && obj != objectPool) pool.Release(obj.gameObject);
        }
        foreach (Transform obj in uniPool.GetComponentsInChildren<Transform>())
        {
            if (obj != uniPool) obj.Recycle();
        }

        if (TestSettings.byInstantiate)
        {
            for (int i = 0; i < count; i++)
            {
                Instantiate(prefab, Random.insideUnitCircle * 12f, Random.rotation, instantiate);
            }
        }
        else if (TestSettings.byObjectPool)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                obj.transform.SetParent(objectPool);
                obj.transform.SetLocalPositionAndRotation(Random.insideUnitCircle * 12f, Random.rotation);
            }
        }
        else if (TestSettings.byUniPool)
        {
            for (int i = 0; i < count; i++)
            {
                prefab.Spawn(Random.insideUnitCircle * 12f, Random.rotation, uniPool);
            }
        }
    }
}
