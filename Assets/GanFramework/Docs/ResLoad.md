# ResLoad 模块说明

本文档概述 `ResLoad` 资源加载模块的设计、工作流程与使用示例，便于快速上手与排查问题。

**模块定位**
- 负责统一的资源加载/缓存/卸载管理；支持多后端（Resources、File、Web、Addressables、Editor）。
- 核心入口：`ResManager`（见代码：[Assets/GanFramework/Core/Modules/ResLoad/ResManager.cs](Assets/GanFramework/Core/Modules/ResLoad/ResManager.cs)）。

**主要概念**
- Provider：后端实现（实现 `IResProvider`），通过前缀路由（如 `res://`、`file://`、`http://`、`addr://` 等）。
  - Providers 实现位于：[Assets/GanFramework/Core/Modules/ResLoad/Providers](Assets/GanFramework/Core/Modules/ResLoad/Providers)
- 缓存字典 `resDic`：key = `fullPath + "_" + typeof(T).Name`，值为 `BaseResInfo`（运行时 cast 为 `ResInfo<T>`）。
- 引用计数：每次加载会 `AddRefCount()`，卸载会 `SubRefCount()`；计数为 0 且被标记删除 (`isDel`) 时可卸载。
- 并发共享：并发请求同一资源时，首次创建 `ResInfo` 并填充 `task`，后续请求共享该 `task`，避免重复加载。

**高层流程**
1. 路径解析：`ParsePath(fullPath)` 分析 `prefix://realPath`，无前缀视为 `Resources`。
2. 选择 Provider：`GetProvider(fullPath)` 根据前缀返回对应 `IResProvider`。
3. 加载（同步/异步）
   - 同步 `Load<T>(fullPath)`：若缓存无记录，调用 provider.Load，同步写入缓存并 AddRef；若已有 task 则阻塞等待 task 完成并返回结果。
   - 异步 `LoadAsync<T>(fullPath)`：首次请求创建 `ResInfo` 并 `AddRefCount()`，将 `provider.LoadAsync` 的 `UniTask` 写入 `info.task` 并 await；后续并发请求仅 AddRef 并 await 同一 `task`。
   - 托底机制：若缓存异常（asset==null && task==null），会触发同步/异步托底重新加载以自愈。
4. 卸载
   - 单资源 `UnloadAsset<T>(fullPath, isDel, isSub)`：默认减少引用计数并标记 `isDel`，当 `asset` 已加载且 `refCount==0 && isDel==true` 时实际调用 provider.Unload 或 `Resources.UnloadAsset` 并移除缓存。
   - 批量 `UnloadUnusedAssets()`：查找所有 `refCount==0 && isDel==true` 条目，统一卸载并调用 `Resources.UnloadUnusedAssets()`。
5. 清理：`ClearDic()` / `ClearDicAsync()` 会调用各 provider.Clear() 并清空缓存（对 Addressables/AB 必需）。

**Provider 要点**
- `ResourcesProvider`：使用 `Resources.Load`/`LoadAsync`，不支持真实进度，卸载由 `ResManager` 调用 `Resources.UnloadAsset`。
- `FileProvider`：从文件系统读取 bytes，支持 Texture2D、TextAsset、AudioClip（异步支持 mp3/ogg/wav）、AssetBundle、byte[] 等。
- `WebProvider`：基于 `UnityWebRequest`，支持真实进度（内部维护 ops），解析不同类型（Texture/Audio/AssetBundle/Text/bytes）。
- `AddressablesProvider`（若启用）：封装 Addressables 句柄，维护自身 refCount，支持真实进度与 Release。
- `EditorProvider`（仅编辑器）：使用 `AssetDatabase` 加载，供开发期使用。

**使用示例**
- 同步加载：
```csharp
var tex = ResManager.Instance.Load<Texture2D>("res://Textures/MyTex");
```
- 异步加载（await）：
```csharp
Texture2D tex = await ResManager.Instance.LoadAsync<Texture2D>("http://.../tex.png");
```
- 异步加载（回调）：
```csharp
ResManager.Instance.LoadAsyncCallback<Texture2D>("file://C:/path/tex.png", tex => { /* use tex */ });
```
- 异步加载（句柄/进度）：
```csharp
var op = ResManager.Instance.LoadAsyncHandle<AudioClip>("http://.../bgm.mp3");
op.Completed += o => { var clip = o.Result; };
StartCoroutine(UIBinder(op));
```
- 批量加载（场景 Loading）：
```csharp
var batch = ResManager.Instance.BatchLoadAsync(new List<string>{"res://A","res://B"});
batch.Completed += b => { /* all done */ };
```
- 卸载资源：
```csharp
ResManager.Instance.UnloadAsset<Texture2D>("res://Textures/MyTex", isDel:true);
```

**注意事项与建议**
- 必须配对调用加载/卸载以保持引用计数正确；若计数变负会产生日志错误提示。
- 不同类型需分开缓存 key（路径相同但类型不同会被当作不同资源）。
- 对于 `Resources`，真正卸载需要对象实例，`ResManager` 在卸载时会调用 `Resources.UnloadAsset`。
- 若使用 Addressables，请确保工程启用了 `ENABLE_ADDRESSABLES` 并在 provider 中正确注册前缀（ResManager 构造已处理）。

**参考源码**
- 核心：[Assets/GanFramework/Core/Modules/ResLoad/ResManager.cs](Assets/GanFramework/Core/Modules/ResLoad/ResManager.cs)
- 基类与操作句柄：[Assets/GanFramework/Core/Modules/ResLoad/Base](Assets/GanFramework/Core/Modules/ResLoad/Base)
- 后端实现：[Assets/GanFramework/Core/Modules/ResLoad/Providers](Assets/GanFramework/Core/Modules/ResLoad/Providers)