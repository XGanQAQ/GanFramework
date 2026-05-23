# EventBus

类型安全的事件发布/订阅系统。纯 Core（无 Unity 依赖）。

## 核心类型

| 类型 | 位置 | 说明 |
|---|---|---|
| `IEvent` | Core | 事件标记接口（空接口） |
| `IEventBus` | Core | 总线接口：`Subscribe<T>` / `Unsubscribe<T>` / `Publish<T>` |
| `EventBus` | Core | 实现 `IEventBus` + `IModules`，内部用 `Dictionary<Type, Delegate>` |

## 用法

### 1. 定义事件

```csharp
public struct PlayerDiedEvent : IEvent {
    public string PlayerId;
}
```

### 2. 订阅

```csharp
Framework.GetModule<IEventBus>().Subscribe<PlayerDiedEvent>(OnPlayerDied);

void OnPlayerDied(PlayerDiedEvent e) {
    Debug.Log($"玩家 {e.PlayerId} 死亡");
}
```

### 3. 取消订阅

```csharp
Framework.GetModule<IEventBus>().Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
```

### 4. 发布

```csharp
Framework.GetModule<IEventBus>().Publish(new PlayerDiedEvent { PlayerId = "p1" });
```

### 5. 内置 UI 事件

```csharp
public class OpenUIEvent : IEvent { IViewer Viewer; }
public class CloseUIEvent : IEvent { IViewer Viewer; }
```

`ViewerBase.Open()` / `Close()` 自动发布。`PresenterBase<T>` 自动订阅对应类型的 UI 事件。

## 注意

- 同一事件类型支持多个订阅者
- 取消订阅不存在的委托不会报错（Delegate.Remove 返回 null 时清理字典）
- EventBus 是一个 IModules 实例，由 FrameworkEntry 注册，Framework.GetModule<IEventBus>() 获取
