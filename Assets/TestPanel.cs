using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using XuanTools.UniPool;

public class TestPanel : MonoBehaviour
{
    public TMP_Text titleText;

    public Button byInstantiate;
    public Button byObjectPool;
    public Button byUniPool;

    private void Start()
    {
        byInstantiate.onClick.AddListener(() =>
        {
            TestSettings.byInstantiate = !TestSettings.byInstantiate;
            TestSettings.byObjectPool = false;
            TestSettings.byUniPool = false;
        });

        byObjectPool.onClick.AddListener(() =>
        {
            TestSettings.byInstantiate = false;
            TestSettings.byObjectPool = !TestSettings.byObjectPool;
            TestSettings.byUniPool = false;
        });

        byUniPool.onClick.AddListener(() =>
        {
            TestSettings.byInstantiate = false;
            TestSettings.byObjectPool = false;
            TestSettings.byUniPool = !TestSettings.byUniPool;
        });
    }

    private void Update()
    {
        if (TestSettings.byInstantiate)
        {
            titleText.text = "Testing by Instantiate";
        }
        else if (TestSettings.byObjectPool)
        {
            titleText.text = "Testing by ObjectPool";
        }
        else if (TestSettings.byUniPool)
        {
            titleText.text = "Testing by UniPool";
        }
        else 
        {
            titleText.text = string.Empty;
        }
    }
}
