# GanFramework

Unity 2022.3.62f3c1 (URP 14.0.12) 个人游戏框架。  
C# 9.0, netstandard2.1。IDE: VSCode + Unity 扩展。

## 程序集布局

所有代码位于 `Assets/GanFramework/`。三个 asmdef 程序集：

| 程序集 | 路径 | 依赖 |
|---|---|---|
| `GanFramework.Core` | `Core/` | (无) |
| `GanFramework.Runtime` | `UnityRuntime/` | Core, Odin, Newtonsoft.Json, UniTask |
| `GanFramework.Editor` | `Editor/` | Core, Runtime |

- **Core** — 纯 C# 接口和基类（asmdef 中没有 Unity 类型依赖，但 Unity 自动生成的 .csproj 会添加 UnityEngine 引用）。  
- **Runtime** — Unity 特定实现（MonoBehaviour、`Resources.Load`、`PlayerPrefs` 等）。  
- **Editor** — 仅 `#if UNITY_EDITOR` 工具代码；运行时代码不得引用。

## 入口点

`FrameworkEntry`（`UnityRuntime/FrameworkEntry.cs:7`）是一个 `GlobalMonoSingleton`，通过 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` 自动创建。它驱动 `IModules` 生命周期：`Init → Update/LateUpdate/FixedUpdate → Shutdown`。

## 模块系统

`GanFramework.Core.Framework`（静态类）管理 `IModules` 列表。所有模块必须先调用 `Framework.Register<T>()` 注册，然后通过 `Framework.Init()` 或 `FrameworkEntry` 自动完成初始化。模块需实现：

```csharp
interface IModules {
    void OnInit();
    void OnUpdate(float deltaTime);
    void OnFixedUpdate(float fixedDeltaTime);
    void OnLateUpdate(float deltaTime);
    void OnDestroy();
}
```

**不要**在模块内部通过 `Instance` 属性惰性注册——模块应当由外部统一注册到 Framework 中，通过 `Framework.GetModule<T>()` 获取。如需单例，继承 `Singleton<T>`（非Mono）或 `GlobalMonoSingleton<T>` / `SceneSingleton<T>`（Mono）。

## 核心模式

| 模式 | 位置 | 说明 |
|---|---|---|
| `Singleton<T>` | `Core/Patterns/Singleton.cs` | 非Mono单例，反射调用私有构造，线程安全双重检查锁 |
| `ServiceLocatorBase` / `ServiceLocator<T>` | `Core/Patterns/ServiceLocator.cs` | 父链查找，`TryGetWait` 支持异步注册等待 |
| `ManagerLocator` | `Core/Patterns/ManagerLocator.cs` | 具体的 `ServiceLocator<ManagerLocator>` |
| `ObjectPoolBase<T>` | `Core/Patterns/ObjectPoolBase.cs` | 抽象基类，子类需实现 Create/Reset/OnDestroy |
| `GlobalMonoSingleton<T>` | `UnityRuntime/Patterns/GlobalMonoSingleton.cs` | `DontDestroyOnLoad`，自动创建，Awake 时取消父级 |
| `SceneSingleton<T>` | `UnityRuntime/Patterns/SceneSingleton.cs` | 仅当前场景，场景切换时销毁 |
| `FSM<T>` | `Core/StateMachine/FSM.cs` | 泛型有限状态机，配合 `IFSMState`（Enter/LogicalUpdate/Exit） |

## 内置模块

- **EventBus**（`Core/Modules/EventBus/`）：`EventBus`（实现 `IModules`）。发布/订阅类型化事件（`IEvent` 接口）。
- **UI**（`Core/Modules/UI/` 接口，`UnityRuntime/Modules/UI/` 实现）：
  - `IUIManager` / `UIManager` — 基于层的 UI 管理器（类似 MVP：Viewer/Presenter）。
  - `ViewerBase` / `PresenterBase` — UI 视图和表现层的 MonoBehaviour 基类。
  - `UILayer` 枚举控制排序顺序（层 × 100）。
  - UI 预制体放在 `Resources/UI/` 文件夹，按名称加载。
- **存档系统**（`Core/Modules/Persistent/` 接口，`UnityRuntime/Modules/Persistent/` 实现）：
  - `ISerializer` 接口（Serialize/Deserialize + FileExtension）。
  - `SaveManager` 静态门面，提供同步 + 异步 API。
  - `OdinSerializer` 包装内置 OdinSerializer。`JsonNetSerializer` 使用内置 Newtonsoft.Json。
  - `SaveMembers` / `LoadMembers` — 通过特性实现字段级存档。
  - 引用解析器：`UnityObjIndexReferenceResolver`、`ResManagerReferenceResolver`。
- **资源管理**（`Core/Modules/IResManager.cs`，`UnityRuntime/Modules/Resource/`）：
  - `ResManager`（实现 `IResManager` + `IModules`）。缓存 `Resources.Load` 结果。

## 插件（在 `Plugins/` 中内置）

- **OdinSerializer**（完整源码，`GanFramework.Odin.OdinSerializer.csproj`）
- **Newtonsoft.Json**（预编译 DLL）
- **UniTask**（预编译 DLL — addressables/DOTween/TextMeshPro 变体以单独的 csproj 存根存在）

**不要**升级或替换 Odin/Newtonsoft，除非已验证所有 SaveManager 序列化器仍能编译。OdinSerializer 是经过修补/内置的版本，不是 Asset Store 版本。

## 编辑器工具（`Editor/`）

- `SaveTestWindow` / `TestSaveData` — 存档系统测试工具。
- `AutoColliderFitter` — 编辑器网格碰撞器拟合工具。

菜单位置：`GanFramework/` 子菜单。

## 项目配置

- `Packages/manifest.json` 列出所有 UPM 依赖。URP 14.0.12。
- `.gitignore` 为标准 Unity 配置 — 忽略 `Library/`、`Temp/`、`Obj/`、`Logs/`、`UserSettings/`、`.vs/`。`*.csproj`、`*.sln` 同样被忽略（由 Unity 重新生成）。
- 仓库根目录的 `*.csproj` 文件是**自动生成的**——不要手动编辑。修改应通过 `AssetPostprocessor.OnGeneratedCSProject`。

## 没有测试

`com.unity.test-framework@1.1.33` 存在于 manifest 中，但没有测试程序集或测试文件。如需添加测试，在 `Tests/` 文件夹下创建独立的 asmdef，标记 `"includePlatforms": ["Editor"]`。

## 提交

`.csproj`、`.sln`、`.vs/`、`Library/`、`Temp/`、`Logs/` 和 `UserSettings/` 都在 gitignore 中。只提交 `Assets/GanFramework/` 下的源文件以及 `Packages/` 和 `ProjectSettings/` 中的项目配置文件。
