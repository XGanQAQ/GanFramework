# EventBus 使用指南

## 一、概述

`EventBus<T>` 是一个 **泛型事件总线**，用于在 Unity 项目中实现 **解耦的事件驱动通信**。

### 设计原则

- **类型安全** — 每种事件类型对应独立的 `EventBus<T>`，无字符串事件名
- **零配置** — 实现 `IEvent` 接口的类/结构体自动被扫描注册
- **生命周期安全** — 退出 Play Mode 时自动清理所有绑定，防止泄漏
- **与 UI 系统深度整合** — `PresenterBase<T>` 自动订阅对应的事件类型

### 核心命名空间

```csharp
using GanFramework.Runtime.EventBus;
```

---

## 二、核心类说明

### 2.1 `IEvent` — 事件标记接口

所有事件类型必须实现此接口。建议定义为 **结构体** 以减少 GC 压力。

```csharp
// 文件: GanFramework/Runtime/EventBus/Events.cs
public interface IEvent { }
```

### 2.2 `EventBus<T>` — 事件总线

```csharp
public static class EventBus<T> where T : IEvent
```

- **静态类**，无需实例化
- 每种 `T` 对应一个独立的事件总线实例
- 提供三个核心方法：

| 方法 | 用途 |
|------|------|
| `Register(EventBinding<T>)` | 注册事件监听 |
| `Deregister(EventBinding<T>)` | 注销事件监听 |
| `Raise(T)` | 触发事件，通知所有监听者 |

### 2.3 `EventBinding<T>` — 事件绑定

```csharp
public class EventBinding<T> : IEventBinding<T> where T : IEvent
```

将事件与回调函数绑定。支持两种构造方式：

| 构造函数 | 回调签名 | 说明 |
|----------|---------|------|
| `new EventBinding<T>(Action<T>)` | 带事件参数 | 可读取事件中的数据 |
| `new EventBinding<T>(Action)` | 无参数 | 仅关心"触发了"，不关心数据 |

也支持 `Add`/`Remove` 方法叠加多个回调。

---

## 三、快速上手

### 3.1 定义事件类型

创建一个实现 `IEvent` 的结构体：

```csharp
using GanFramework.Runtime.EventBus;

public struct PlayerDiedEvent : IEvent
{
    public uint PlayerNetId;    // 死亡玩家的网络 ID
    public string KillerName;   // 击杀者名称
}
```

> **建议**：事件结构体中只存放数据，不包含方法逻辑。

### 3.2 触发事件（发布方）

在需要的时机调用 `EventBus<T>.Raise()`：

```csharp
EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent
{
    PlayerNetId = netId,
    KillerName = killerName,
});
```

### 3.3 监听事件（订阅方）

```csharp
public class ScoreManager : MonoBehaviour
{
    private EventBinding<PlayerDiedEvent> diedBinding;

    private void Start()
    {
        diedBinding = new EventBinding<PlayerDiedEvent>(OnPlayerDied);
        EventBus<PlayerDiedEvent>.Register(diedBinding);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        Debug.Log($"玩家 {evt.PlayerNetId} 被 {evt.KillerName} 击杀");
    }

    private void OnDestroy()
    {
        EventBus<PlayerDiedEvent>.Deregister(diedBinding);
    }
}
```

> **重要**：**必须在 `OnDestroy` 中 `Deregister`**，否则对象销毁后回调仍会被调用，导致 NullReferenceException。

### 3.4 使用无参数回调

如果只需要知道事件发生了，不关心具体数据：

```csharp
var binding = new EventBinding<PlayerDiedEvent>(() =>
{
    Debug.Log("有玩家死亡了");
});
EventBus<PlayerDiedEvent>.Register(binding);
```

### 3.5 叠加多个回调

同一个事件绑定可以挂多个回调：

```csharp
binding.Add(OnPlayerDied);
binding.Add(AnotherHandler);
// ...
binding.Remove(AnotherHandler);
```

---

## 四、UI 系统整合：PresenterBase

GanFramework 的 **MVP UI 系统** 已内置事件总线集成。

`PresenterBase<T>` 会在 `Init()` 时自动注册 `EventBus<T>`，并在 `OnDestroy()` 时自动注销：

```csharp
public class MyUIPresenter : PresenterBase<PlayerDiedEvent>
{
    [SerializeField] private MyUIViewer viewer;

    protected override void OnGetEvent(PlayerDiedEvent passedEvent)
    {
        // 当 EventBus<PlayerDiedEvent>.Raise() 被调用时自动触发
        viewer.ShowDeathInfo(passedEvent.PlayerNetId, passedEvent.KillerName);
    }
}
```

这样 UI 表现层就只需要实现 `OnGetEvent`，无需手动管理 `Register`/`Deregister`。

---

## 五、高级用法

### 5.1 将事件同时用于多个目的

事件结构体可以被不同系统监听。例如 `PlayerDiedEvent` 可以同时驱动：

- **UI 系统**：显示死亡面板
- **任务系统**：检查击杀任务目标
- **音频系统**：播放死亡音效
- **计分系统**：更新击杀分数

每个系统只需各自注册 `EventBus<PlayerDiedEvent>`，互不感知。

### 5.2 在非 MonoBehaviour 中使用

`EventBus<T>` 是静态类，可以在任何 C# 代码中使用：

```csharp
public class PureCSharpService
{
    private EventBinding<SomeEvent> binding;

    public void StartListening()
    {
        binding = new EventBinding<SomeEvent>(HandleEvent);
        EventBus<SomeEvent>.Register(binding);
    }

    public void StopListening()
    {
        EventBus<SomeEvent>.Deregister(binding);
    }

    private void HandleEvent(SomeEvent evt) { }
}
```

### 5.3 批量清理

退出 Play Mode 或关闭游戏时，`EventBusUtil` 会自动调用 `ClearAllBuses()` 清空所有总线。无需手动处理。

---

## 六、项目中的实际示例

### 6.1 事件定义 (`Assets/Scripts/Gameplay/Events/`)

```csharp
// 玩家事件
public struct PlayerBornEvent : IEvent { public uint PlayerNetId; }
public struct PlayerDieEvent : IEvent { public uint PlayerNetId; }
public struct PlayerReviveEvent : IEvent { public uint PlayerNetId; }

// 设施 UI 打开事件
public struct OpenArmoryEvent : IEvent
{
    public PlayerWeaponsController PlayerWeaponsController;
    public PlayerProfessionController PlayerProfessionController;
    public Armory Armory;
}
```

### 6.2 事件注册（任务系统）

```csharp
// Mission.cs
EventBus<PlayerCollectResourceEvent>.Register(itemCollectedBinding);
EventBus<MonsterKilledEvent>.Register(enemyKilledBinding);
EventBus<LocationReachedEvent>.Register(reachedLocationBinding);
EventBus<ObjectiveEvent>.Register(missionEventBinding);
```

### 6.3 事件触发（游戏管理器）

```csharp
// PlayerPropertyController.cs — 玩家出生时
EventBus<PlayerBornEvent>.Raise(new PlayerBornEvent { PlayerNetId = netId });

// MonsterProperty.cs — 怪物死亡时
EventBus<MonsterKilledEvent>.Raise(new MonsterKilledEvent { ... });

// Armory.cs — 打开军械库 UI
EventBus<OpenArmoryEvent>.Raise(new OpenArmoryEvent
{
    PlayerWeaponsController = playerWeaponController,
    PlayerProfessionController = playerProfessionController,
    Armory = this,
});
```

---

## 七、最佳实践

### ✅ 推荐做法

1. **事件定义集中管理** — 建议按领域分类放在 `Assets/Scripts/Gameplay/Events/`
2. **使用结构体** — 减少 GC 压力
3. **始终 Deregister** — 在 `OnDestroy()` 或 `StopListening()` 中务必注销
4. **事件粒度适中** — 一个事件类对应一种"发生了什么"，不要在一个事件里塞过多用途
5. **结合 PresenterBase 使用** — 当事件用于驱动 UI 时，继承 `PresenterBase<T>` 自动管理生命周期

### ❌ 避免做法

1. **不要在事件处理方法中执行耗时操作** — 所有监听者在 `Raise` 的线程中同步执行
2. **不要依赖事件触发顺序** — 同类型多监听者的执行顺序是不确定的
3. **不要用事件传递大量数据** — 事件适合传递引用或少量值类型数据
4. **避免循环触发** — 在事件处理中又触发同类型事件可能导致无限递归

---

## 八、生命周期说明

```
Unity 启动
  │
  ▼
EventBusUtil.Initialize()  [BeforeSceneLoad]
  │
  ├─ 扫描所有实现 IEvent 的类型
  └─ 为每个事件类型初始化 EventBus<T>
  │
  ▼
运行时: Register / Raise / Deregister
  │
  ▼
退出 Play Mode
  │
  ▼
EventBusUtil.ClearAllBuses()  [ExitingPlayMode]
  │
  └─ 清空所有 EventBus<T> 的绑定列表
```
