# GanFramework 文档

框架基于 Unity 2022.3.62f1 (URP 14.0.12)，C# 9.0 / netstandard2.1。

## 程序集

| 程序集 | 路径 | 依赖 |
|---|---|---|
| `GanFramework.Core` | `Assets/GanFramework/Core/` | 无 |
| `GanFramework.Runtime` | `Assets/GanFramework/UnityRuntime/` | Core, Odin, Newtonsoft.Json, UniTask |
| `GanFramework.Editor` | `Assets/GanFramework/Editor/` | Core, Runtime, Odin |

所有运行时模块都注册到 `Framework`（静态类），通过 `FrameworkEntry` 驱动生命周期。

## 模块列表

| 模块 | 接口 | 实现 | 位置 |
|---|---|---|---|
| [Framework](Framework.md) | — | `FrameworkEntry` + `Framework` | Core/Runtime |
| [Patterns](Patterns.md) | — | `Singleton<T>`, `ServiceLocator<T>`, `ObjectPoolBase<T>` 等 | Core/Runtime |
| [StateMachine](StateMachine.md) | `IFSMState` | `FSM<T>` | Core |
| [EventBus](EventBus.md) | `IEventBus` | `EventBus` | Core |
| [UI](UI.md) | `IUIManager`, `IViewer`, `IPresenter` | `UIManager`, `ViewerBase`, `PresenterBase<T>` | Core + Runtime |
| [Persistent](Persistent.md) | `IPersistent`, `ISerializer` | `PersistentService`, `SaveStore`, `OdinSerializer` 等 | Core + Runtime |
| [ResManager](ResManager.md) | `IResManager` | `ResManager` | Core + Runtime |
| [Environment](Environment.md) | — | `Environment`, `EnvironmentState` | Core |
| [Log](Log.md) | `ILogger`, `ILogHandler` | `UnityLogger`, `UnityLogHandler` | Core + Runtime |
| [Scene](Modules/Scene/Scene.md) | `ISceneManager` | `SceneManagerModule` | Core + Runtime |
| [Editor Tools](EditorTools.md) | — | `SaveTestWindow`, `AutoColliderFitter` | Editor |

## 快速开始

```csharp
// 1. 模块在 FrameworkEntry.RegisterBuiltinModules() 中集中注册
// 2. 运行时通过接口获取模块实例
var bus = Framework.GetModule<IEventBus>();
bus.Subscribe<MyEvent>(OnMyEvent);

var save = Framework.GetModule<IPersistent>();
save.Save(playerData, new OdinSerializer(DataFormat.Binary));

var res = Framework.GetModule<IResManager>();
var prefab = res.Load<GameObject>("UI/MyPanel");
```
