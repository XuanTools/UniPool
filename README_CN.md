# UniPool

[![Readme_EN](https://img.shields.io/badge/UniPool-Readme_EN-red)](https://github.com/XuanTools/UniPool/blob/main/README.md) [![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/XuanTools/UniPool/blob/main/LICENSE)

一个为GameObject打造的，简单易用、性能优秀的对象池管理工具

> 在游戏中会出现大量重复的物体需要频繁的创建和销毁；比如子弹，敌人，成就列表的格子等；频繁的创建删除物体会造成很大的开销；

> UniPool能将需要频繁创建销毁的游戏对象缓存起来，将创建销毁行为替换成显示和隐藏，大大提高游戏运行效率。

* 允许您手动创建UniPool对象池，自定义委托，管理游戏物体的生成和回收
* 带有一个单例类UniPoolManager，用于管理每一个物体的对象池
* 延迟回收机制，优化同一帧内回收和获取大量物体时的性能
* 扩展GameObject的方法，能方便地生成、回收物体和管理对象池
* 通过使用泛型，Component拥有和物体一样的扩展方法，能方便地对组件所挂在的物体执行操作

## 目录

- [快速入门](#快速入门)
- [进阶](#进阶)
- [UniTask](#unitask)
- [UniTaskManager](#unitaskmanager)
- [安装](#安装)
- [工程](#工程)
- [License](#license)

## 快速入门

您可以通过以下简单的内容，快速了解UniPool的基础使用方法

```
// 您无需引入其它命名空间，即可方便地使用扩展方法
GameObject prefab; // 预制体
SpriteRenderer spri; // 挂载在预制体上的组件

// 您可以选择在场景中预先添加UniPoolManger单例，如果该单例不存在，在第一次调用UniPoolManager的方法时会自动创建
// 您可以场景中的UniPoolManger单例上挂载初始对象池，也可以使用prefab.RegistPool()为物体注册对象池
void Start()
{
    prefab.RegistPool(); // 为预制体创建对象池
    spri.RegistPool(); // 组件可以像预制体一样调用方法，这将对组件所挂载在的物体执行操作
}

void UniPoolDemo()
{
    // 判断物体是否存在对象池
    bool flag = prefab.ContaiPool();

    // 生成物体，取代Instanate()
    GameObject obj1 = prefab.Spawn();
    SpriteRenderer obj2 = spri.Spawn();

    // 回收物体，取代Destory()
    obj.Recycle();
}
```

## 进阶

（未完成）

## UniTask

#### 描述

一个为GameObject设计的对象池，带有一种缓存机制

（未完成）

## UniTaskManager

#### 描述

UniTaskManager是一个管理场景中对象池的单例，其内部使用UniTask来缓存对象

（未完成）

## 安装

下载资源，然后将对应文件夹导入您的Unity项目路径中即可

如果遇到兼容问题，请选择兼容性更高的版本导入

如果仍遇到问题，您也可以选择手动将错误修复（这将不会很难）

## 工程

项目工程包含了一个性能测试场景，对比Instantiate与ObjectPool和UniPool之间的性能差异

建议不要开启profiler，否则可能会导致UniPool测试帧率大幅降低

## License

[MIT](LICENSE) © Richard Littauer