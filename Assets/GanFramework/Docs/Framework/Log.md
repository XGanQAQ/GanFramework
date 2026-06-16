# Log

多频道、可扩展的日志系统。接口在 Core，Unity Console 实现在 Runtime。

## 设计目标

- **频道分离** — 不同业务模块（UI / Network / Audio…）输出到不同频道，可独立开关
- **可扩展频道** — 内置 11 个常用频道，业务方通过 `LogChannel.Register()` 动态添加自定义频道
- **级别过滤** — Trace → Fatal 六级，低于设定级别的消息不输出
- **多输出端** — 通过 `ILogHandler` 接口可组合多个输出目标（Console、文件、网络等）
- **堆栈自动附加** — Error / Fatal 级别自动附带调用堆栈
- **空安全门面** — `LogManager` 静态门面在模块初始化前静默丢弃日志，不抛异常

## 核心类型

| 类型 | 位置 | 说明 |
|---|---|---|
| `LogLevel` | Core | `Trace / Debug / Info / Warning / Error / Fatal` |
| `LogChannel` | Core | 可扩展频道类，`Id` + `Name` + `BitMask`，支持 `Register()` 动态注册 |
| `ILogger` | Core | 主接口：级别/频道过滤、`Log` 重载、便捷方法、频道管理、Handler 管理 |
| `ILogHandler` | Core | 输出端接口：`Log` ×3 重载 + `LogException` |
| `LogManager` | Core | 静态门面，空安全代理到内部 `ILogger` 实例 |
| `UnityLogHandler` | Runtime | `ILogHandler` 实现，输出到 Unity Console，`[频道][级别] 消息` 格式 |
| `UnityLogger` | Runtime | `ILogger + IModules` 实现，持有 `List<ILogHandler>`，`OnInit` 注册到 `LogManager` |

## 可扩展频道机制

`LogChannel` 用 `long` 位掩码标识（最多 63 个频道）：

```
Bit  0: Default
Bit  1: System
Bit  2: UI
...
Bit 10: Event
Bit 16+: 动态注册 (LogChannel.Register)
```

内置频道在静态初始化时自动注册到 `_registered` 列表。动态注册线程安全（`_lock`）。`GetAllMask()` 遍历所有已注册频道生成全量掩码。

## ILogger 过滤规则

```
IsEnabled = (messageLevel >= _level) && (channel.BitMask & _enabledChannelsMask) != 0
```

- `Level` — 消息级别必须 ≥ 当前设置级别（例如 Level=Warning 时 Info 不输出）
- `EnabledChannelsMask` — 消息频道必须位于启用的频道掩码中

## 生命周期

```
FrameworkEntry.RegisterBuiltinModules()
  └─ Framework.Register(new UnityLogger())    ← 注册到模块列表

Framework.Init()
  └─ UnityLogger.OnInit()
       ├─ _handlers.Add(new UnityLogHandler()) ← 添加默认 Console 输出
       └─ LogManager.Initialize(this)          ← 设置静态门面实例

运行时
  └─ LogManager.Info(channel, msg)             ← 静态门面代理到 UnityLogger
       └─ UnityLogger.Log()
            ├─ IsEnabled 检查
            └─ _handlers[i].Log()              ← 遍历所有 Handler

Framework.Shutdown()
  └─ UnityLogger.OnDestroy()
       └─ LogManager.Initialize(null)          ← 清空静态门面
```

## 与 Unity Console 的集成

`UnityLogHandler` 按级别映射：

| LogLevel | Unity API | Console 颜色 |
|---|---|---|
| Trace / Debug / Info | `Debug.Log` | 白色 |
| Warning | `Debug.LogWarning` | 黄色 |
| Error / Fatal | `Debug.LogError` | 红色 |

带 `context` 参数时调用 `Debug.Log(msg, context)`，Unity Console 中可点击跳转。
