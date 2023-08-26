using UnityEngine;
using UnityEngine.UI;

public class TestPanel : MonoBehaviour
{
    public ObjectController Controller;

    public Toggle ByInstantiateButton;
    public Toggle ByObjectPoolButton;
    public Toggle ByUniPoolButton;

    private void Start()
    {
        ByInstantiateButton.onValueChanged.AddListener(isOn=> Controller.ByInstance = isOn);
        ByObjectPoolButton.onValueChanged.AddListener(isOn => Controller.ByObjectPool = isOn);
        ByUniPoolButton.onValueChanged.AddListener(isOn => Controller.ByUniPool = isOn);
    }
}
