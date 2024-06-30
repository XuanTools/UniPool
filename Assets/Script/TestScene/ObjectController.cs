using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEngine.Debug;
using static UnityEngine.Random;


namespace XuanTools.UniPool.Benchmark
{
    public enum BenchmarkType
    {
        None = 0,
        Instantiate = 1,
        ObjectPool = 2,
        UniPoolSpawn = 3,
        UniPoolList = 4
    }

    public class ObjectController : MonoBehaviour
    {
        [Header("BenchByType Setting")] 
        public GameObject Prefab;
        public Transform ObjectParent;
        public int MillisecondsPerItem = 5000;
        public int Count = 1000;

        [Header("BenchByType Item")] 
        public bool BenchInstantiate = true;
        public bool BenchmarkObjectPool = true;
        public bool BenchmarkUniPoolSpawn = true;
        public bool BenchmarkUniPoolList = true;

        private ObjectPool<GameObject> _objectPool;
        private List<GameObject> _activeObject;
        private bool _isBenchmarking;

        private static readonly ProfilerMarker ProfilerMarker = new("Benchmark Code");

        private void Awake()
        {
            _objectPool = new ObjectPool<GameObject>(
                () => Instantiate(Prefab, ObjectParent),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                Destroy, true, Count, Count);

            _activeObject = new List<GameObject>(Count);
        }

        public async void BenchStart()
        {
            if (_isBenchmarking)
            {
                LogError("测试已开始，请不要重复点击测试按钮");
                return;
            }

            _isBenchmarking = true;
            if (BenchInstantiate) await BenchByType(BenchmarkType.Instantiate);
            if (BenchmarkObjectPool) await BenchByType(BenchmarkType.ObjectPool);
            if (BenchmarkUniPoolSpawn) await BenchByType(BenchmarkType.UniPoolSpawn);
            if (BenchmarkUniPoolList) await BenchByType(BenchmarkType.UniPoolList);
            _isBenchmarking = false;
        }

        private async Awaitable BenchByType(BenchmarkType benchmarkType)
        {
            LogWarning($"当前测试项目: {Enum.GetName(typeof(BenchmarkType), benchmarkType)}");

            var nanosecondPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
            var frameStopwatch = new Stopwatch();
            var codeStopwatch = new Stopwatch();
            var frameTimeList = new List<long>();
            var codeTimeList = new List<long>();
            long frameCount = 0;

            GC.Collect();
            var timeStopwatch = new Stopwatch();
            timeStopwatch.Start();
            while (timeStopwatch.ElapsedMilliseconds < MillisecondsPerItem)
            {
                frameCount++;
                frameStopwatch.Restart();
                codeStopwatch.Restart();

                ProfilerMarker.Begin();
                RecycleObjectByType(benchmarkType);
                SpawnObjectByType(benchmarkType);
                ProfilerMarker.End();

                codeTimeList.Add(codeStopwatch.ElapsedTicks * nanosecondPerTick);
                await Awaitable.NextFrameAsync();
                frameTimeList.Add(frameStopwatch.ElapsedTicks * nanosecondPerTick);
            }

            RecycleObjectByType(benchmarkType);
            timeStopwatch.Stop();
            frameStopwatch.Stop();
            codeStopwatch.Stop();

            Log($"平均帧率: {frameCount * 1000d / MillisecondsPerItem :N1}fps");
            Log($"平均帧时长: {frameTimeList.Average() / 1000 / 1000:N1}ms");
            Log($"平均代码时长: {codeTimeList.Average() / 1000 / 1000:N1}ms");
        }

        private void SpawnObjectByType(BenchmarkType benchmarkType)
        {
            switch (benchmarkType)
            {
                case BenchmarkType.Instantiate:
                    for (var i = 0; i < Count; i++)
                    {
                        _activeObject.Add(
                            Instantiate(Prefab, insideUnitCircle * 12f, rotation, ObjectParent));
                    }
                    break;
                case BenchmarkType.ObjectPool:
                    for (var i = 0; i < Count; i++)
                    {
                        var obj = _objectPool.Get();
                        _activeObject.Add(obj);
                        obj.transform.SetPositionAndRotation(insideUnitCircle * 12f, rotation);
                    }
                    break;
                case BenchmarkType.UniPoolSpawn:
                    for (var i = 0; i < Count; i++)
                    {
                        _activeObject.Add(Prefab.Spawn(insideUnitCircle * 12f, rotation, ObjectParent));
                    }
                    break;
                case BenchmarkType.UniPoolList:
                    Prefab.SpawnToList(_activeObject, Count, obj =>
                    {
                        obj.transform.SetPositionAndRotation(insideUnitCircle * 12f, rotation);
                        obj.transform.SetParent(ObjectParent);
                    });
                    break;
                case BenchmarkType.None:
                default:
                    break;
            }
        }

        private void RecycleObjectByType(BenchmarkType benchmarkType)
        {
            foreach (var obj in _activeObject)
            {
                switch (benchmarkType)
                {
                    case BenchmarkType.Instantiate:
                        Destroy(obj);
                        break;

                    case BenchmarkType.ObjectPool:
                        _objectPool.Release(obj);
                        break;

                    case BenchmarkType.UniPoolSpawn:
                    case BenchmarkType.UniPoolList:
                        obj.Recycle();
                        break;

                    case BenchmarkType.None:
                    default:
                        break;
                }
            }
            _activeObject.Clear();
        }
    }
}
