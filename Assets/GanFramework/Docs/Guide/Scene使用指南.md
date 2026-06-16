# Scene 使用指南

## 一、概述

Scene 模块提供基于 **SceneGroup** 的场景分组加载/卸载方案。  
适合需要同时加载多个场景（如 UI + Gameplay + HUD）的项目，替代 Unity 的 `SceneManager.LoadScene` 单场景模式。

### 核心流程

```
配置 SceneGroup (Inspector) → SceneLoader 或 代码调用
    → SceneManagerModule.LoadScenesAsync()
    → 并行加载所有场景 → 设置 ActiveScene → 完成回调
```

---

## 二、快速开始

### 2.1 准备场景

将要管理的场景添加到 **Build Settings** 中（Addressable 场景无需添加）。

### 2.2 创建 SceneGroup 配置

在任意 GameObject 上挂载 `SceneLoader` 或自定义组件，在 Inspector 中配置 `SceneGroup`：

```
SceneGroup
├── GroupName: "GameLevel1"
└── Scenes (List<SceneData>)
    ├── [0] Reference → 拖入 SceneAsset, SceneType: ActiveScene
    ├── [1] Reference → 拖入 SceneAsset, SceneType: UserInterface
    └── [2] Reference → 拖入 SceneAsset, SceneType: HUD
```

### 2.3 触发加载

```csharp
using GanFramework.Core;
using GanFramework.Runtime.Modules.Scene;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] SceneGroup[] groups;

    async void Start()
    {
        var scn = Framework.GetModule<ISceneManager>();
        await scn.LoadScenesAsync(groups[0].ToLoadTasks());
        Debug.Log("关卡场景加载完成");
    }
}
```

---

## 三、使用 SceneLoader 组件

框架提供 `SceneLoader` 组件，集成加载进度条 UI：

1. 创建空 GameObject，挂载 `SceneLoader`
2. 绑定 `Image`（进度条）、`Canvas`、`Camera`
3. 在 `SceneGroups` 数组中配置关卡分组
4. 运行时自动加载第 0 组

```csharp
// 手动切换分组
var loader = GetComponent<SceneLoader>();
await loader.LoadSceneGroup(1);  // 加载第 1 组，自动显示加载画面
```

---

## 四、场景类型（SceneType）

`SceneType` 决定场景在组中的角色，当前支持：

| 类型 | 用途 |
|---|---|
| `ActiveScene` | 自动设为 `SceneManager.activeScene`（每组合一个） |
| `MainMenu` | 主菜单 |
| `UserInterface` | UI Canvas 场景 |
| `HUD` | 游戏内 HUD |
| `Cinematic` | 过场动画 |
| `Environment` | 环境/地图场景 |
| `Tooling` | 编辑器辅助场景 |

只有标记为 `ActiveScene` 的场景会在加载完成后自动激活。

---

## 五、进度追踪

### 5.1 IProgress<float> 回调

```csharp
var progress = new Progress<float>(p =>
{
    loadingBar.fillAmount = p;
    Debug.Log($"场景加载进度: {p:P}");
});

await scm.LoadScenesAsync(tasks, progress);
```

### 5.2 事件监听

```csharp
scm.OnSceneLoaded    += name => Debug.Log($"加载: {name}");
scm.OnSceneUnloaded  += name => Debug.Log($"卸载: {name}");
scm.OnSceneGroupLoaded += () => Debug.Log("整组加载完成");
```

---

## 六、Addressable 场景

### 6.1 启用条件编译

在 **Player Settings → Scripting Define Symbols** 或对应 asmdef 的 `defineConstraints` 中添加：

```
USE_ADDRESSABLES
```

### 6.2 标记场景为 Addressable

配置 Addressable Groups 后，在 `SceneReference` Inspector 中勾选 **IsAddressable**。

### 6.3 运行时

```csharp
// API 完全一致，SceneManagerModule 内部自动判断
await scm.LoadScenesAsync(tasks);
```

未定义 `USE_ADDRESSABLES` 时，Addressable 场景会被跳过并输出警告，不影响编译。

---

## 七、卸载场景

```csharp
await scm.UnloadScenesAsync();
```

规则：
- 卸载当前**所有已加载的非激活、非 Bootstrap 场景**
- Addressable 场景自动释放 Addressable 句柄
- 最后调用 `Resources.UnloadUnusedAssets()` 清理内存

---

## 八、完整示例

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GanFramework.Core;
using GanFramework.Runtime.Modules.Scene;
using UnityEngine;
using UnityEngine.UI;

public class GameEntry : MonoBehaviour
{
    [SerializeField] SceneGroup[] levelGroups;
    [SerializeField] Image loadingBar;
    [SerializeField] Canvas loadingCanvas;

    ISceneManager scn;

    void Awake()
    {
        scn = Framework.GetModule<ISceneManager>();
        scn.OnSceneGroupLoaded += OnLevelLoaded;
    }

    public async Task LoadLevel(int index)
    {
        if (index < 0 || index >= levelGroups.Length) return;

        loadingCanvas.enabled = true;
        loadingBar.fillAmount = 0f;

        var progress = new Progress<float>(p => loadingBar.fillAmount = p);
        await scn.LoadScenesAsync(levelGroups[index].ToLoadTasks(), progress);

        loadingCanvas.enabled = false;
    }

    void OnLevelLoaded()
    {
        Debug.Log("关卡就绪");
    }

    void OnDestroy()
    {
        if (scn != null)
            scn.OnSceneGroupLoaded -= OnLevelLoaded;
    }
}
```

---

## 九、注意事项

1. **模块初始化** — `ISceneManager` 在 `FrameworkEntry.BeforeSceneLoad` 中注册，场景加载代码需在初始化后执行
2. **重复加载** — 默认跳过已加载的同名场景，设置 `reloadDupScenes = true` 可强制重载
3. **加载中阻塞** — 正在加载时再次调用会被忽略并输出警告
4. **Bootstrap 保护** — 名为 "Bootstrapper" 的场景不会被卸载
5. **ActiveScene** — 每组应且只应有一个 `SceneType.ActiveScene`，否则激活场景行为不确定
6. **场景路径** — 普通场景使用 Build Settings 中的路径（如 `Assets/Scenes/Game.unity`），Addressable 场景使用 Addressable 地址
