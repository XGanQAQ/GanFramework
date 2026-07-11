# UI 框架概述

## 框架定位

GanFramework UI 是一个基于 **MVP（Model-View-Presenter）** 模式的 UI 管理框架，配合 **EventBus 事件总线** 实现 Gameplay 与 UI 层的松耦合通信。框架运行于 Unity 6，使用 `UIManager` 单例集中管理所有 UI 的打开、关闭、层级渲染。

## 核心架构

```
┌─────────────────────────────────────────────────┐
│                    UIManager                     │
│  (单例，管理所有UI的打开/关闭/层级/鼠标状态)        │
└──────┬────────────────────────────────┬──────────┘
       │ 管理                            │ 管理
       ▼                                ▼
┌──────────────┐              ┌──────────────────┐
│  LayerInfo    │ ... 8 个     │   ViewerBase     │
│  (每层一个)   │◄─────────────┤   (UI 视图基类)   │
│  ┌──────────┐ │              │ - Open/Close     │
│  │Canvas    │ │              │ - Init           │
│  │UI实例列表│ │              │ - Layer 属性     │
│  └──────────┘ │              └────────┬─────────┘
└──────────────┘                        │ 挂载在同一物体
                                       ▼
                              ┌──────────────────┐
                              │  PresenterBase   │
                              │  (表现层/事件路由) │
                              │ - 订阅 EventBus  │
                              │ - 转发到 Viewer  │
                              └──────────────────┘
```

## 核心组件

### 1. UIManager（单例）

**职责**：统一管理所有 UI 的创建、显示、隐藏、切换和层级。

- 继承自 `GlobalMonoSingleton<UIManager>`，全局唯一
- 在 `Awake` 阶段按 `UILayer` 枚举顺序预先创建所有层级的 Canvas，确保渲染顺序正确
- 使用 `Dictionary<UILayer, LayerInfo>` 管理各层级，其中 `LayerInfo` 持有该层级的根 GameObject 和已打开的 UI 实例字典
- 提供泛型方法：`OpenUI<T>()`、`CloseUI<T>()`、`SwitchUI<T>()`、`CloseLayerUI()`
- 根据打开的 UI 自动管理鼠标锁定状态（Cursor Lock）

**层级渲染机制**：每个 `UILayer` 对应一个独立的 Canvas，`sortingOrder = (int)layer * 100`，数值越大渲染在越前面。Canvas 在 `Awake` 阶段全部创建就绪，避免运行时动态创建导致的渲染顺序问题。

### 2. UILayer（枚举）

```csharp
Background = 0  // 全屏背景 UI（主菜单背景、场景过渡背景等）
Scene      = 1  // 3D 场景 UI（场景内交互提示、怪物头顶血条等）
Normal     = 2  // 主界面 UI、HUD（背包、任务面板、角色信息等）
Info       = 3  // 浮动提示、信息显示（拾取提示、经验值提示等）
Popup      = 4  // 弹窗、对话框（确认框、设置面板等）
Toast      = 5  // 短时通知（Toast 提示、成就解锁等自动消失的通知）
Top        = 6  // 系统级 UI（加载界面、断线重连提示等全局覆盖 UI）
Debug      = 7  // 调试 UI（FPS 显示、开发调试面板，仅开发模式使用）
```

### 3. ViewerBase（抽象基类）

**职责**：所有 UI 视图的基类，继承自 `MonoBehaviour`，实现 `IInitializable`。

- `Open()` / `Close()` — 控制 UI 的显示与隐藏（激活/禁用 GameObject）
- `Init()` — UI 初始化逻辑，在 `UIManager.OpenUIInternal` 中被调用
- `Layer` — 该 UI 所属的层级，从预制体上读取，决定 UI 被放置在哪个 Canvas 下
- `UIName` — UI 标识名，默认使用类名，可通过 `viewerName` 字段在 Inspector 中覆写
- `OnOpen` / `OnClose` — 打开/关闭时触发的事件，供 Presenter 或其他系统监听
- 打开/关闭时会通过 `EventBus<OpenUIEvent>` / `EventBus<CloseUIEvent>` 广播

### 4. PresenterBase（抽象基类）

**职责**：表现层，负责事件路由。继承自 `MonoBehaviour`，实现 `IInitializable`。

- 泛型 `PresenterBase<T>`，`T` 为事件类型（需实现 `IEvent`）
- `Init()` — 注册 EventBus 订阅，将 `OnGetEvent` 绑定到事件
- `OnGetEvent(T)` — 收到事件时的回调，由子类实现具体的 UI 更新逻辑
- `OnDestroy()` — 自动取消 EventBus 订阅，防止内存泄漏
- Presenter 与 Viewer 挂载在同一 GameObject 上，由 UIManager 在实例化时按顺序初始化（先 ViewerBase.Init，再 PresenterBase.Init）

### 5. EventBus（事件总线）

**职责**：泛型事件系统，实现 Gameplay 与 UI 的松耦合通信。

- `EventBus<T>.Raise(T event)` — 触发事件，通知所有订阅者
- `EventBus<T>.Register(EventBinding<T>)` — 注册订阅
- `EventBus<T>.Deregister(EventBinding<T>)` — 取消订阅
- `EventBinding<T>` — 事件绑定包装类，支持带参数和无参数回调
- `IEvent` — 事件标记接口，所有事件类型需实现此接口

### 6. UICamera（单例）

**职责**：UI 相机，负责 UI 的渲染。

- 设置 `cullingMask` 为 `UI` 层，`depth` 为 100（在主相机之后渲染）
- 提供 `GetCamera()` 供其他组件获取引用

### 7. 辅助工具

- **OpenViewerButton** — 挂载到 Button 上的辅助组件，可在 Inspector 中指定点击后打开的 `ViewerBase`
- **ViewerAttribute** — 标记 `ViewerBase` 的元数据属性（层级、资源路径），预留用于后续自动注册

## UI 生命周期

```
UIManager.Awake()
  └─ 创建所有层级 Canvas（Background ~ Debug）

GameManager.Start() / 交互物触发事件
  └─ EventBus<T>.Raise(new T { ... })
       └─ Presenter.OnGetEvent(T)
            └─ Viewer.Open()

UIManager.OpenUI<T>()
  └─ GetViewer(uiName)  → 已存在则直接 Open()
  └─ Resources.Load("UI/{uiName}")
  └─ 获取预制体的 Layer 信息
  └─ 在对应 LayerInfo.Root 下 Instantiate
  └─ viewerBase.Init()
  └─ 其他 IInitializable（Presenter 等）Init()
  └─ viewerBase.Open()
  └─ UpdateCursorState()
```

## 场景层级结构

```
UIRoot ("UI" GameObject, 无 Canvas)
├── Background_Canvas (sortingOrder: 0)
├── Scene_Canvas      (sortingOrder: 100)
├── Normal_Canvas     (sortingOrder: 200)
├── Info_Canvas       (sortingOrder: 300)
├── Popup_Canvas      (sortingOrder: 400)
├── Toast_Canvas      (sortingOrder: 500)
├── Top_Canvas        (sortingOrder: 600)
├── Debug_Canvas      (sortingOrder: 700)
├── UIManager
├── UICamera
├── EventSystem
└── UIInputManager
```

## 命名空间

- `GanFramework.Runtime.UI` — 框架核心 UI 组件
- `GanFramework.Runtime.EventBus` — 事件总线系统
- `Gameplay.Events` — 游戏逻辑事件定义
- 游戏特定 Viewer/Presenter 放在对应模块的 `UI/` 目录下

## 依赖关系

- Unity 6 (6000.0.54f1)
- UniTask 用于异步操作（可选）
- 框架自身依赖：无外部 UI 库
