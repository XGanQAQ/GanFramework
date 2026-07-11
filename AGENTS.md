# GanFramework

Unity 2022.3.62f3c1 (URP 14.0.12) 个人游戏框架。  
C# 9.0, netstandard2.1。
IDE: VSCode + Unity 扩展。

## 设计理念

我希望在此项目中实现一个轻量级、可扩展的游戏框架，提供基础设施和工具，帮助开发者快速构建游戏。
此项目的代码需要保持简洁、可读性高，并且易于维护。

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

`FrameworkEntry`（`UnityRuntime/FrameworkEntry.cs:7`）通过 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` 自动创建。它驱动 `IModules` 生命周期：`Init → Update/LateUpdate/FixedUpdate → Shutdown`。

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

模块与模块之间不应直接依赖。模块间通信应通过事件或 `Framework.GetModule<T>()` 获取接口。

## 插件（在 `Plugins/` 中内置）

- **OdinSerializer**（完整源码，`GanFramework.Odin.OdinSerializer.csproj`）
- **Newtonsoft.Json**（预编译 DLL）
- **UniTask**（预编译 DLL — addressables/DOTween/TextMeshPro 变体以单独的 csproj 存根存在）

**不要**升级或替换 Odin/Newtonsoft，除非已验证所有 SaveManager 序列化器仍能编译。OdinSerializer 是经过修补/内置的版本，不是 Asset Store 版本。

## 项目配置

- `Packages/manifest.json` 列出所有 UPM 依赖。URP 14.0.12。
- `.gitignore` 为标准 Unity 配置 — 忽略 `Library/`、`Temp/`、`Obj/`、`Logs/`、`UserSettings/`、`.vs/`。`*.csproj`、`*.sln` 同样被忽略（由 Unity 重新生成）。
- 仓库根目录的 `*.csproj` 文件是**自动生成的**——不要手动编辑。修改应通过 `AssetPostprocessor.OnGeneratedCSProject`。

## 提交

`.csproj`、`.sln`、`.vs/`、`Library/`、`Temp/`、`Logs/` 和 `UserSettings/` 都在 gitignore 中。只提交 `Assets/GanFramework/` 下的源文件以及 `Packages/` 和 `ProjectSettings/` 中的项目配置文件。
