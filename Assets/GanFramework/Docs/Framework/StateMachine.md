# StateMachine

有限状态机，位于 Core（无 Unity 依赖）。

## 核心类型

### `IFSMState` (interface, Core)

```csharp
public interface IFSMState {
    void Enter();           // 进入状态
    void LogicalUpdate();   // 状态逻辑
    void Exit();            // 退出状态
}
```

### `FSM<T>` (class, Core)

泛型状态机，`T` 为实现 `IFSMState` 的类型。

```csharp
public class FSM<T> where T : IFSMState {
    public T CurrentState { get; }
    public void Add(T state);       // 注册状态
    public void ChangeState(T state); // 切换状态
    public void Update();           // 驱动当前状态 LogicalUpdate
}
```

## 用法

```csharp
public class IdleState : IFSMState {
    public void Enter() { /* 进入待机 */ }
    public void LogicalUpdate() { /* 待机逻辑 */ }
    public void Exit() { /* 退出待机 */ }
}

public class RunState : IFSMState {
    // ...
}

var fsm = new FSM<IFSMState>();
fsm.Add(new IdleState());
fsm.Add(new RunState());
fsm.ChangeState(idle);

// 每帧调用
fsm.Update();
```
