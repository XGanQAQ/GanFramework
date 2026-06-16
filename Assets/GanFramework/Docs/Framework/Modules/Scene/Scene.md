# Scene 场景管理模块

基于分组的场景加载/卸载系统，支持普通场景与 Addressable 场景。  
通过 `SceneGroup` 将多个场景组织为逻辑组，一次加载/卸载整组。

---

## 核心类型

| 类型 | 位置 | 说明 |
|---|---|---|
| `ISceneManager` | `Core/Modules/SceneManagement/` | 场景管理器接口 |
| `SceneManagerModule` | `UnityRuntime/Modules/Scene/` | 实现，`IModules` + `ISceneManager` |
| `SceneGroup` | `UnityRuntime/Modules/Scene/` | 可序列化场景组，Inspector 中配置 |
| `SceneData` | `UnityRuntime/Modules/Scene/` | 单个场景数据（引用 + 类型） |
| `SceneReference` | `UnityRuntime/Modules/Scene/` | 场景引用，替代 `Eflatun.SceneReference` |
| `SceneType` | `Core/Modules/SceneManagement/` | 场景类型枚举 |
| `SceneLoadTask` | `Core/Modules/SceneManagement/` | 内部传输用的场景加载任务 |
| `SceneLoader` | `UnityRuntime/Modules/Scene/` | 加载 UI 辅助组件（可选） |

---

## 接口

```csharp
public interface ISceneManager
{
    event Action<string> OnSceneLoaded;      // sceneName
    event Action<string> OnSceneUnloaded;    // sceneName
    event Action OnSceneGroupLoaded;

    bool IsLoading { get; }
    float Progress { get; }

    Task LoadScenesAsync(IReadOnlyList<SceneLoadTask> scenes,
        IProgress<float> progress = null, bool reloadDupScenes = false);
    Task UnloadScenesAsync();
}
```

---

## 数据模型

### SceneType

```csharp
public enum SceneType { ActiveScene, MainMenu, UserInterface, HUD, Cinematic, Environment, Tooling }
```

### SceneReference（替代 Eflatun.SceneReference）

与 `Eflatun.SceneReference` 功能等价的自定义类型：
- 编辑器中拖拽 `SceneAsset` 自动获取路径
- 运行时通过 `Path` / `Name` 属性访问
- Addressable 开关由 `#if USE_ADDRESSABLES` 条件编译控制

```csharp
[Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
    // 编辑器: SceneAsset 拖拽 → 自动填入 scenePath
    // 运行时: scenePath → sceneName（自动推导）
    public string Path { get; }
    public string Name { get; }
}
```

### SceneGroup

```csharp
[Serializable]
public class SceneGroup
{
    public string GroupName;
    public List<SceneData> Scenes;

    public string FindSceneNameByType(SceneType type);
    public List<SceneLoadTask> ToLoadTasks();  // 转换为传输对象
}
```

在 Inspector 中将 `SceneGroup[]` 挂到 `SceneLoader` 或自行持有，在需要时调用 `ToLoadTasks()`。

---

## 模块注册

`SceneManagerModule` 在 `FrameworkEntry.RegisterBuiltinModules()` 中自动注册：

```csharp
private static void RegisterBuiltinModules()
{
    // ...
    Framework.Register(new SceneManagerModule());
}
```

通过 `Framework.GetModule<ISceneManager>()` 获取。

---

## 用法

### 基础用法（代码驱动）

```csharp
var sceneMgr = Framework.GetModule<ISceneManager>();

var tasks = new List<SceneLoadTask>
{
    new SceneLoadTask("Assets/Scenes/MainMenu.unity", "MainMenu", SceneType.MainMenu),
    new SceneLoadTask("Assets/Scenes/UI.unity", "UI", SceneType.UserInterface),
};

sceneMgr.OnSceneGroupLoaded += () => Debug.Log("加载完成");

await sceneMgr.LoadScenesAsync(tasks);
```

### 通过 SceneLoader 组件

在场景中放置 `SceneLoader`（需挂 `Image` 进度条、`Canvas`、`Camera`），在 Inspector 中配置 `SceneGroup[]`：

```csharp
// 默认 Start 时加载第 0 组，也可手动调用
GetComponent<SceneLoader>().LoadSceneGroup(1);
```

### 使用 SceneGroup 配置

```csharp
public class MyBoot : MonoBehaviour
{
    [SerializeField] SceneGroup[] groups;

    async void Start()
    {
        var sceneMgr = Framework.GetModule<ISceneManager>();
        var progress = new Progress<float>(p => Debug.Log($"进度: {p:P}"));
        await sceneMgr.LoadScenesAsync(groups[0].ToLoadTasks(), progress);
    }
}
```

### 卸载场景

```csharp
await sceneMgr.UnloadScenesAsync();
```

自动卸载当前已加载的**非 Bootstrap、非激活**场景。对于 Addressable 场景会同时释放 Addressable 句柄。

---

## Addressable 支持

`Addressables.LoadSceneAsync` / `Addressables.UnloadSceneAsync` 调用由 `#if USE_ADDRESSABLES` 条件编译包裹：

- 启用 Addressable：在 Player Settings / asmdef 中定义 `USE_ADDRESSABLES`，并将场景标记为 Addressable
- 未启用：标记为 Addressable 的场景会被跳过并输出警告，不会报编译错误

```csharp
// SceneReference 中：
#if USE_ADDRESSABLES
public bool IsAddressable;
#endif
```

---

## 生命周期

| 阶段 | 行为 |
|---|---|
| `OnInit()` | 无操作 |
| `OnUpdate()` | 无操作 |
| `OnDestroy()` | 清空操作队列和事件委托 |

所有异步加载通过 `Task` + 轮询 `AsyncOperation.isDone` 实现，无需 MonoBehaviour 驱动。

---

## 注意

- `LoadScenesAsync` 内部先调用 `UnloadScenesAsync`，避免重复叠加
- 加载中的请求会被阻塞（`IsLoading = true`），后续调用直接返回
- 跳过加载时自动跳过已存在的同名场景（`reloadDupScenes = false` 时）
- 加载完成后自动设置 `SceneType.ActiveScene` 为激活场景
- `UnloadScenesAsync` 末尾调用 `Resources.UnloadUnusedAssets()` 释放未使用资源
- `Bootstrapper` 场景（名称含 "Bootstrapper"）不会被卸载
