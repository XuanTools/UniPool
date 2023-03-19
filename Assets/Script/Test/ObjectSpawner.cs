using UnityEngine;

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

    private void Update()
    {
        foreach (Transform obj in instance.GetComponentsInChildren<Transform>())
        {
            if (obj != instance) Destroy(obj.gameObject);
        }
        foreach (Transform obj in objectPool.GetComponentsInChildren<Transform>())
        {
            if (obj.gameObject.activeSelf && obj != objectPool) ObjectPoolExtention.ObjectPoolExtensions.Recycle(obj);
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
                ObjectPoolExtention.ObjectPoolExtensions.Spawn(prefab, objectPool, Random.insideUnitCircle * 12f, Random.rotation);
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
