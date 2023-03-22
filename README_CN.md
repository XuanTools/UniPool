# UniPool

[![Readme_EN](https://img.shields.io/badge/UniPool-Document-red)](https://github.com/XuanTools/UniPool/blob/main/README.md) [![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/XuanTools/UniPool/blob/main/LICENSE)

一个为GameObject打造的，简单易用、性能优秀的对象池管理工具

> 在游戏中会出现大量重复的物体需要频繁的创建和销毁；比如子弹，敌人，成就列表的格子等；频繁的创建删除物体会造成很大的开销；

> UniPool能将需要频繁创建销毁的游戏对象缓存起来，将创建销毁行为替换成显示和隐藏，大大提高游戏运行效率。

* UniTask对象池系统，缓存游戏对象，提升运行效率
* 单例类UniPoolManager，统一管理场景内物体的对象池
* 包含大量方便易用的扩展方法，能够便捷地对物体进行对象池操作
* 延迟回收机制，优化同一帧内回收和获取大量物体时的性能
* 允许自定义创建UniPool对象池，自定义委托，管理游戏物体的生成和回收

## 目录

- [快速入门](#快速入门)
- [UniPool基础](#unipool基础)
- [UniPoolManager](#unipoolmanager)
- [进阶](#进阶)
- [安装](#安装)
- [工程](#工程)
- [License](#license)

## 快速入门

您可以通过以下简单的内容，快速了解UniPool的基础使用方法

```
// 您无需引入其它命名空间，即可方便地使用扩展方法
GameObject prefab; // 预制体
SpriteRenderer spri; // 挂载在预制体上的组件

// 扩展方法依赖场景中的UniPoolManager单例，如果该单例不存在，在第一次调用扩展方法时会自动创建
void Start()
{
    // 预先创建对象池，可以提前加载物体和设置对象池最大数量，避免使用时大量加载造成的性能问题
    // 您可以在场景中的UniPoolManger单例上挂载初始对象池，也可以使用调用方法为物体创建对象池
    prefab.RegistPool(100, 1000); // 为预制体创建对象池，指定初始大小和最大容量
}

void UniPoolDemo()
{
    // 生成物体，取代Instanate()
    // 如果生成物体时不存在对象池，则会自动创建一个新的对象池
    GameObject obj1 = prefab.Spawn();
    SpriteRenderer obj2 = spri.Spawn(); // 组件与预制体拥有相同的扩展方法

    // 回收物体，取代Destory()
    obj.Recycle();
}
```

## UniPool基础

UniPool是一个为GameObject设计的简单易用的对象池

为什么要使用UniPool？因为Instanate()和Destory()会造成较大的性能开销，UniPool能缓存游戏对象，提升运行效率

使用下面的构造方法为预制体创建UniPool对象池

```
// 为prefab物体创建一个对象池，初始容量10，最大容量1000
var pool1 = new UniPool(prefab, 100, 1000);
// UniPool各阶段的委托可以自定义，若传入null则将使用UniPool默认的委托
var pool2 = new UniPool(prefab, 100, 1000, CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy);
```

UniPool内部使用HashSet\<GameObject>缓存生成的物体，使用下面的方法获取和回收物体

注意只能回收处于活动状态的物体（既已取出的物体），试图回收已回收的物体会引发一个错误

```
// 从UniPool中取出一个物体并调用ActionOnGet委托，如果对象池中没有物体，则将调用CreateFunc委托创建一个新物体
var obj = pool.Get();

// 将物体放回UniPool中并调用ActionOnRelease委托，如果对象池已满，则再调用ActionOnDestroy委托销毁该物体
pool.Release(obj);
pool.Release(obj); // 寄了，抛出异常
```

释放物体的委托（如常用的SetActive方法）可能造成一些开销，可以使用缓存方法，将物体暂时缓存在池中

取出物体时将优先取出缓存状态的物体，这能避免释放时的开销（此时物体没有调用ActionOnRelease委托）

```
// 缓存物体进入对象池，不调用ActionOnRelease委托
pool.Cache(obj);
```

可以待到合适的时机再使用下面的方法将缓存的物体释放入池中

```
// 释放所有已缓存的物体，并调用ActionOnRelease委托
pool.CacheReleaseAll();
```

UniPool还另外内置一个HashSet\<GameObject>追踪生成的物体，您可以快速的对所有已取出的物体执行回收操作

调用下列方法将释放或缓存所有取出的物体，但是您的代码中可能仍存在这些物体的引用，这可能导致一些问题，请谨慎使用这些方法

```
// 释放所有已取出的物体
pool.SpawnedReleaseAll();

// 缓存所有已取出的物体
pool.SpawnedCacheAll();
```

UniPool只能管理由UniPool本身生成的物体，试图回收不属于该UniPool物体是不允许的，这将导致报错

使用以下方法可以判断一个物体是否属于某个UniPool（包括回收、缓存和活动状态的物体）

```
// 判断物体是否属于此对象池
bool flag = pool.Contain(obj);
```

处在UniPool中的物体应由UniPool管理销毁，不应该对属于UniPool的物体调用Destory方法，UniPool会继续管理被销毁的物体，这将导致错误

使用以下方法可以清除UniPool中已销毁的物体（正常使用UniPool无需调用此方法）

```
// 清除UniPool中已销毁的物体
pool.RemoveDestoriedObject();
```

UniPool使用完成后，可以使用以下方法销毁对象池

```
// 清除所有非活动状态的物体（包括已回收和已缓存的物体）
pool.ClearPooled();

// 回收所有生成物体再释放UniPool，将导致所有物体被销毁
pool.ClearAll();

// 实现IDisposable接口，与ClearAll方法相同
pool.Dispose();
```

UniPool还包含了一个泛型版本UniPool\<T> where T : Component，可以对组件所属的物体进行上述相同的操作

## UniPoolManager

UniPoolManager是一个管理场景中对象池的单例，其内部使用UniPool来缓存对象

（未完成）

## 进阶

以下代码展示了所有扩展方法的使用方法

GameObject类型和继承自Component的类型可以使用下列扩展方法

在下列示例中，prefab为预制体（对象池使用期间不能销毁），obj为对象池生成的物体

```
// 注册对象池
// defaultCapacity 初始化的物体数量
// maxCount 该对象池所能容纳的最大容量
// worldPositionStays设置回收设置父物体时是否保持世界坐标，UGUI物体应将此项设为false
prefab.RegistPool(int defaultCapacity, int maxCount, bool worldPositionStays);

// 生成物体，并设置父物体或初始位置
// instantiateInWorldSpace, 取出并分配父对象时，传递true可直接在世界空间中定位新对象，传递false可相对于其新父项来设置对象的位置
obj = prefab.Spawn();
obj = prefab.Spawn(Transform parent);
obj = prefab.Spawn(Transform parent, bool instantiateInWorldSpace);
obj = prefab.Spawn(Vector3 position, Quaternion rotation);
obj = prefab.Spawn(Vector3 position, Quaternion rotation, Transform parent);

// 未完成

```

## 安装

下载资源，然后将对应文件夹导入您的Unity项目路径中即可

如果遇到兼容问题，请选择兼容性更高的版本导入

如果仍遇到问题，您也可以选择手动将错误修复（这将不会很难）

## 工程

项目工程包含了一个性能测试场景，对比Instantiate与ObjectPool和UniPool之间的性能差异

建议不要开启profiler，否则可能会导致UniPool测试帧率大幅降低

## License

[MIT](LICENSE) © Richard Littauer