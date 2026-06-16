# UI

基于层的 MVP 风格 UI 管理器。接口在 Core，实现和 Mono 基类在 Runtime。

## 层次结构

```
UIRoot (DontDestroyOnLoad)
  ├─ Background_Canvas (0)
  ├─ Scene_Canvas (100)
  ├─ Normal_Canvas (200)
  ├─ Info_Canvas (300)
  ├─ Popup_Canvas (400)
  ├─ Toast_Canvas (500)
  ├─ Top_Canvas (600)
  └─ Debug_Canvas (700)
```

每层排序值 = `(int)layer × 100`。

## UILayer 枚举

`Background`, `Scene`, `Normal`, `Info`, `Popup`, `Toast`, `Top`, `Debug`

## 核心类型

| 类型 | 位置 | 说明 |
|---|---|---|
| `IUIManager` | Core | UI 管理器接口 |
| `UIManager` | Runtime | 实现，`GlobalMonoSingleton<UIManager>` |
| `IViewer` | Core | 视图接口 |
| `ViewerBase` | Runtime | 视图基类（MonoBehaviour） |
| `IPresenter` | Core | 表现层接口 |
| `PresenterBase<T>` | Runtime | 表现层基类，自动订阅 `T : IEvent` |
| `UICamera` | Runtime | UI 相机组件 |
| `IInitializable` | Core | 初始化接口 `Init()` |
| `ViewerAttribute` | Core | 视图标记属性 |

## 用法

### UIManager 的打开/关闭

```csharp
UIManager.Instance.OpenUI<MyViewer>();
UIManager.Instance.CloseUI<MyViewer>();
UIManager.Instance.OpenUI("MyViewer", show: true);
```

### 创建新 UI

1. 在 `Resources/UI/` 下创建预制体，挂载 `ViewerBase` 子类
2. 定义 UI 事件（可选）：继承 `IEvent`，用于 Presenter 通信
3. 创建 Presenter（可选）：继承 `PresenterBase<T>`，`OnGetEvent` 处理事件

```csharp
// Viewer
public class MyViewer : ViewerBase {
    public override string UIName => "MyViewer";
    public override void Init() { /* 初始化视图 */ }
}

// Presenter
public class MyPresenter : PresenterBase<MyUIEvent> {
    protected override void OnGetEvent(MyUIEvent e) {
        // 处理 UI 事件
    }
}
```

### 预制体加载

按名称从 `Resources/UI/` 加载，预制体根组件必须为 `ViewerBase`。

### 游标锁定

`UIManager` 根据 `unLockedCursorLayers` 列表自动管理 `Cursor.lockState`（例如在 Popup/Top 层打开时不锁定光标）。
