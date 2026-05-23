# Framework

## 架构

```
[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]
  └─ FrameworkEntry.BeforeSceneLoad()
       ├─ 创建 GameObject "[FrameworkEntry]"
       ├─ 注册内置模块（RegisterBuiltinModules）
       │    ├─ new EventBus()
       │    └─ new PersistentService()
       └─ Framework.Init()

Unity 循环
  ├─ Update()     → Framework.Update()     → 各模块 OnUpdate()
  ├─ FixedUpdate() → Framework.FixedUpdate() → 各模块 OnFixedUpdate()
  └─ LateUpdate()  → Framework.LateUpdate()  → 各模块 OnLateUpdate()

退出/销毁
  └─ OnDestroy() → Framework.Shutdown() → 各模块 OnDestroy()
```

## 核心类型

### `Framework` (static class, Core)

中央注册表 + 生命周期驱动器。

| 方法 | 说明 |
|---|---|
| `Register(IModules)` | 注册模块；若已初始化则立即调用 `OnInit()` |
| `Unregister<T>()` | 按类型移除模块并调用 `OnDestroy()` |
| `GetModule<T>()` | 按接口/类型查找模块 |
| `TryGetModule<T>(out T)` | 安全的查找方式 |
| `GetAllModules<T>()` | 获取所有匹配的模块 |
| `Init()` | 初始化所有已注册模块 |
| `Shutdown()` | 逆序销毁所有模块 |

### `IModules` (interface, Core)

所有模块必须实现的声明周期接口：

```csharp
public interface IModules {
    void OnInit();
    void OnUpdate(float deltaTime);
    void OnFixedUpdate(float fixedDeltaTime);
    void OnLateUpdate(float deltaTime);
    void OnDestroy();
}
```

### `FrameworkEntry` (MonoBehaviour, Runtime)

- 继承 `MonoBehaviour`（非 `GlobalMonoSingleton`）
- 通过 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 自动创建
- 公共属性 `Instance` 可获取单例

### 内置模块注册点

`FrameworkEntry.RegisterBuiltinModules()` 中添加新模块：

```csharp
private static void RegisterBuiltinModules() {
    Framework.Register(new EventBus());
    Framework.Register(new PersistentService());
}
```

### 添加自定义模块

```csharp
public class MyModule : IModules {
    public void OnInit() { /* 初始化 */ }
    public void OnUpdate(float dt) { /* 每帧更新 */ }
    public void OnDestroy() { /* 清理 */ }
    // ...
}

// 在 BeforeSceneLoad 之前注册
Framework.Register(new MyModule());
```
