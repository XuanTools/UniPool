using UnityEngine;
using UnityEngine.UI;

namespace XuanTools.UniPool.Benchmark
{
    public class TestPanel : MonoBehaviour
    {
        public ObjectController Controller;

        public Toggle ByInstantiateToggle;
        public Toggle ByObjectPoolToggle;
        public Toggle ByUniPoolSpawnToggle;
        public Toggle ByUniPoolListToggle;

        public Button BenchmarkStartButton;

        private void Start()
        {
            ByInstantiateToggle.onValueChanged.AddListener(isOn => Controller.BenchInstantiate = isOn);
            ByObjectPoolToggle.onValueChanged.AddListener(isOn => Controller.BenchmarkObjectPool = isOn);
            ByUniPoolSpawnToggle.onValueChanged.AddListener(isOn => Controller.BenchmarkUniPoolSpawn = isOn);
            ByUniPoolListToggle.onValueChanged.AddListener(isOn => Controller.BenchmarkUniPoolList = isOn);

            BenchmarkStartButton.onClick.AddListener(() => Controller.BenchStart());
        }
    }
}
