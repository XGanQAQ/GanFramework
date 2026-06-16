# 如何编写 UI 相关代码

UI 层基于 MVP（Model-View-Presenter）模式，配合 EventBus 事件总线实现 Gameplay 与 UI 的松耦合通信。

- **Viewer**（视图层）：负责 UI 展示、数据绑定、用户交互事件触发。不直接写 Gameplay 逻辑。
- **Presenter**（表现层）：订阅 EventBus 事件，将数据转发给 Viewer，或将用户操作传回 Gameplay。
- **EventBus**（事件总线）：Gameplay 通过触发事件通知 UI 打开/更新，UI 通过事件将用户操作传回 Gameplay。

## 基本约定

| 分类 | 路径 | 命名 |
|------|------|------|
| 事件类 | `Assets/Scripts/Gameplay/Events/` | `OpenXxxUI.cs`, `UpdateXxxUI.cs` |
| Presenter | `Assets/Scripts/UI/{Module}/` | `XxxPresenter.cs` |
| Viewer | `Assets/Scripts/UI/{Module}/` | `XxxViewer.cs` |
| 交互物 | `Assets/Scripts/Gameplay/{Module}/` | 按功能命名如 `WeaponStation.cs` |

## 如何创建一个交互 UI（逐步）

### 1. 定义事件类

事件类需实现 `IEvent` 接口，只包含纯数据（DTO 风格），不要放逻辑。

```csharp
// Assets/Scripts/Gameplay/Events/OpenArmoryUI.cs
public class OpenArmoryUI : IEvent
{
    public GameObject Interactor;
    public Armory Armory;
    public Action<int> OnItemSelected; // 可选回调
}
```

### 2. 在 Gameplay 侧触发事件

交互物在 `Interact()` 中通过 EventBus 发送事件：

```csharp
// Assets/Scripts/Gameplay/Level/Facility/Armory.cs
public void Interact(GameObject interactor)
{
    EventBus<OpenArmoryUI>.Raise(new OpenArmoryUI
    {
        Interactor = interactor,
        Armory = this
    });
}
```

### 3. 创建 Viewer

继承 `ViewerBase`，只处理 UI 展示层逻辑：

```csharp
// Assets/Scripts/UI/Level/ArmoryViewer.cs
public class ArmoryViewer : ViewerBase
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button confirmButton;

    private OpenArmoryUI currentEvent;

    public void Open(OpenArmoryUI passedEvent)
    {
        currentEvent = passedEvent;
        titleText.text = "军械库";
        Open(); // 调用基类 Open，激活 GameObject
    }

    public void OnItemSelected(int itemIndex)
    {
        currentEvent?.OnItemSelected?.Invoke(itemIndex);
        Close();
    }
}
```

### 4. 创建 Presenter

继承 `PresenterBase<T>`，订阅事件并转发给 Viewer：

```csharp
// Assets/Scripts/UI/Level/ArmoryPresenter.cs
public class ArmoryPresenter : PresenterBase<OpenArmoryUI>
{
    [SerializeField] private ArmoryViewer armoryViewer;

    protected override void OnGetEvent(OpenArmoryUI passedEvent)
    {
        armoryViewer.Open(passedEvent);
    }
}
```

### 5. 场景设置

- 将 Viewer 和 Presenter 预设体放在 `Resources/UI/` 目录下（类名需与文件名一致）
- Viewer 和 Presenter 组件挂载在同一个 GameObject 上（或各自分开，通过引用关联）
- 在预制体上设置 Viewer 的 `Layer` 字段（决定 UI 所属层级）
- 打开 UI 时直接调用 `UIManager.Instance.OpenUI<ArmoryViewer>()` 或通过 EventBus 触发

## MVP 职责分工

### Presenter 负责

- 订阅 EventBus 事件，接收 Gameplay 的数据
- 将事件数据传给 Viewer 的公开方法
- 将 Viewer 的用户操作（按钮点击等）通过事件或回调传回 Gameplay
- 轻量级，不含复杂逻辑

```csharp
public class ArmoryPresenter : PresenterBase<OpenArmoryUI>
{
    [SerializeField] private ArmoryViewer armoryViewer;

    protected override void OnGetEvent(OpenArmoryUI passedEvent)
    {
        armoryViewer.Open(passedEvent);
        armoryViewer.OnItemApplied += index =>
        {
            // 将用户操作传回 Gameplay
            passedEvent.Armory.ApplyItem(index);
            armoryViewer.Close();
        };
    }
}
```

### Viewer 负责

- UI 的显示/隐藏、数据填充、动画播放
- 按钮点击等用户交互事件的通知（通过 C# 事件或回调）
- 不直接修改 Gameplay 数据

```csharp
public class ArmoryViewer : ViewerBase
{
    public event Action<int> OnItemApplied;

    [SerializeField] private Button[] itemButtons;

    private void Start()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            itemButtons[i].onClick.AddListener(() => OnItemApplied?.Invoke(index));
        }
    }

    public void Open(OpenArmoryUI passedEvent)
    {
        Open();
    }
}
```

## EventBus 用法

### 发送事件

```csharp
EventBus<OpenArmoryUI>.Raise(new OpenArmoryUI { ... });
```

### 订阅事件（在 PresenterBase 中自动完成）

```csharp
// PresenterBase.Init() 中自动注册：
eventBinding = new EventBinding<T>(OnGetEvent);
EventBus<T>.Register(eventBinding);

// PresenterBase.OnDestroy() 中自动取消订阅：
EventBus<T>.Deregister(eventBinding);
```

### 手动订阅（非 Presenter 场景）

```csharp
var binding = new EventBinding<SomeEvent>(OnSomeEvent);
EventBus<SomeEvent>.Register(binding);
// 不再需要时取消
EventBus<SomeEvent>.Deregister(binding);
```

## 打开 UI 的方式

### 方式一：通过 UIManager 直接打开（适合非交互物触发的 UI）

```csharp
UIManager.Instance.OpenUI<ArmoryViewer>();
```

### 方式二：通过 EventBus 触发（适合交互物触发的 UI）

```csharp
// Gameplay 侧
EventBus<OpenArmoryUI>.Raise(new OpenArmoryUI { ... });

// Presenter 侧（自动接收事件并打开对应 Viewer）
```

## 关闭 UI 的方式

```csharp
UIManager.Instance.CloseUI<ArmoryViewer>();
UIManager.Instance.CloseLayerUI(UILayer.Popup); // 关闭整个层级
```

## 切换 UI（开/关切换）

```csharp
UIManager.Instance.SwitchUI<ArmoryViewer>();
```

## 命名空间

```
GanFramework.Runtime.UI      → ViewerBase, PresenterBase, UIManager, UILayer
GanFramework.Runtime.EventBus → EventBus<T>, EventBinding<T>, IEvent
Gameplay.Events              → 业务事件定义（OpenXxxUI, UpdateXxxUI 等）
```

## UI 预制体规范

- 放在 `Assets/Resources/UI/` 目录下
- 文件名与类名一致（如 `ArmoryViewer.prefab`）
- 预制体根 GameObject 上必须有 `ViewerBase` 组件
- 如果有 Presenter，应挂载在同一个 GameObject 上或通过引用关联
- 在预制体上设置好 `Layer` 字段，决定 UI 归属的渲染层级
- 预制体上不需要 Canvas 组件（UIManager 会自动放在对应层级的 Canvas 下）

## 最佳实践

1. **Viewer 只做 UI 展示** — 不要在 Viewer 中写 Gameplay 逻辑，通过事件/回调解耦
2. **Presenter 保持轻量** — 只做事件路由，不做复杂业务处理
3. **事件类只放数据** — DTO 风格，不放方法逻辑
4. **Init 顺序** — `ViewerBase.Init()` 先于 `PresenterBase.Init()`，Presenter 可安全引用 Viewer
5. **幂等设计** — `Open()`/`Close()` 调用应安全，重复调用不产生副作用（UIManager 已做去重）
6. **Null 检查** — 对事件传入的数据做 null 检查
7. **引用校验** — Presenter 中引用的 Viewer 应在 Awake/Start 中校验非空

## 示例：完整的交互 UI 流程

```
玩家与军械库交互
  → Armory.Interact()
    → EventBus<OpenArmoryUI>.Raise(...)
      → ArmoryPresenter.OnGetEvent(openArmoryUI)
        → armoryViewer.Open(openArmoryUI)
          → 显示军械库面板
          → 玩家选择装备
            → ArmoryPresenter 将选择传回 Armory.ApplyItem()
              → Close()
```

## 注意事项

- **UI 资源加载**：当前使用 `Resources.Load` 加载 UI 预制体，资源放在 `Resources/UI/` 下
- **层级渲染**：Canvas 由 UIManager 在 `Awake` 时按 `UILayer` 顺序自动创建，不需手动设置
- **鼠标状态**：`Popup` 和 `Top` 层级的 UI 打开时自动解锁鼠标，其余层级保持锁定
- **不要手动销毁动态创建的 UI**：UIManager 通过字典管理 UI 实例的生命周期
