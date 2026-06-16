# Log 使用指南

## 一、概述

Log 模块提供多频道、可分级的日志系统。通过 **频道** 机制将不同业务模块的日志隔离，方便开发和调试时按需过滤。

### 核心命名空间

```csharp
using GanFramework.Core.Modules.Log;
```

---

## 二、快速使用

### 2.1 静态门面（最常用）

模块初始化后，可直接通过 `LogManager` 输出日志，无需获取实例：

```csharp
LogManager.Info(LogChannel.System, "游戏初始化完成");
LogManager.Warn(LogChannel.UI, "按钮 {0} 未找到", btnName);
LogManager.Error(LogChannel.Network, "连接超时", this);  // this 可点击跳转
LogManager.Fatal(LogChannel.Audio, "音频设备丢失");
```

### 2.2 通过模块接口获取

```csharp
var logger = Framework.GetModule<ILogger>();
logger.Info(LogChannel.System, "Hello");
```

### 2.3 按频道过滤

```csharp
// 关闭 Combat 和 Audio 频道的日志输出
LogManager.DisableChannel(LogChannel.Combat);
LogManager.DisableChannel(LogChannel.Audio);

// 只开启 Network
LogManager.EnableChannel(LogChannel.Network);
```

### 2.4 按级别过滤

```csharp
// 只输出 Warning 及以上级别的日志
LogManager.Level = LogLevel.Warning;
```

---

## 三、频道管理

### 3.1 内置频道

| 频道 | 适用场景 |
|---|---|
| `LogChannel.Default` | 通用日志 |
| `LogChannel.System` | 系统启动/关闭、生命周期 |
| `LogChannel.UI` | UI 打开/关闭、交互事件 |
| `LogChannel.Network` | 网络请求、连接状态 |
| `LogChannel.Audio` | 音频播放、资源加载 |
| `LogChannel.Combat` | 战斗数值、伤害计算 |
| `LogChannel.AI` | AI 状态切换、行为树 |
| `LogChannel.Input` | 输入事件、按键映射 |
| `LogChannel.Resource` | 资源加载/卸载 |
| `LogChannel.Animation` | 动画状态机、混合 |
| `LogChannel.Event` | 事件总线收发 |

### 3.2 自定义频道

```csharp
// 在业务模块中注册自定义频道（建议定义为静态只读字段）
public static readonly LogChannel MyFeature = LogChannel.Register("MyFeature");

// 使用自定义频道输出
LogManager.Info(MyFeature, "自定义频道测试");
LogManager.Error(MyFeature, "出错了", gameObject);
```

> `Register()` 线程安全，可在任意时机调用。最多支持 63 个自定义频道。

---

## 四、文件日志输出

### 4.1 实现自定义 ILogHandler

```csharp
using System.IO;
using GanFramework.Core.Modules.Log;

public class FileLogHandler : ILogHandler
{
    private readonly string _path;

    public FileLogHandler(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
    }

    public void Log(LogLevel level, LogChannel channel, object message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}][{channel.Name}][{level}] {message}";
        File.AppendAllText(_path, line + Environment.NewLine);
    }

    public void Log(LogLevel level, LogChannel channel, object message, object context)
    {
        Log(level, channel, message);
    }

    public void Log(LogLevel level, LogChannel channel, string format, params object[] args)
    {
        Log(level, channel, string.Format(format, args));
    }

    public void LogException(Exception exception, object context)
    {
        File.AppendAllText(_path, $"[EXCEPTION] {exception}{Environment.NewLine}");
    }
}
```

### 4.2 注册自定义 Handler

```csharp
// 在游戏启动时注册（例如某个 Manager 的 Awake 或 Start 中）
LogManager.AddHandler(new FileLogHandler(Application.persistentDataPath + "/logs/game.log"));
```

---

## 五、完整示例

```csharp
using GanFramework.Core.Modules.Log;

public class GameBooter : MonoBehaviour
{
    private static readonly LogChannel Boot = LogChannel.Register("Boot");

    private void Start()
    {
        LogManager.Info(LogChannel.System, "游戏启动");

        // 添加文件日志
        LogManager.AddHandler(new FileLogHandler(
            Application.persistentDataPath + "/logs/boot.log"));

        LogManager.Info(Boot, "开始加载配置");
        LoadConfig();

        // 调试期间只关注 Network 和 UI 频道
        LogManager.Level = LogLevel.Debug;
        LogManager.EnableChannel(LogChannel.Network);
        LogManager.EnableChannel(LogChannel.UI);
        LogManager.DisableChannel(LogChannel.Audio);
        LogManager.DisableChannel(LogChannel.Combat);
    }

    private void LoadConfig()
    {
        LogManager.Info(Boot, "配置加载完成");
    }
}
```

---

## 六、注意事项

1. **模块初始化前** — `LogManager` 的调用是空安全的，模块未初始化时静默丢弃，不会抛异常
2. **ECallback 中使用** — Error/Fatal 级别会自动附带堆栈信息，适合排查问题
3. **context 参数** — 传递 `UnityEngine.Object` 引用时，Unity Console 中的消息可点击跳转到对应对象
4. **频道数量** — 基于 `long` 位掩码，最多支持 63 个频道；前 16 位保留给内置频道
