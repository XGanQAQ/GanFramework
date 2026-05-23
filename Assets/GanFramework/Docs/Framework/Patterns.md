# Patterns

## 单例模式

### `Singleton<T>` (Core)

非 MonoBehaviour 单例基类。线程安全双重检查锁，反射调用私有构造函数。

```csharp
public class GameConfig : Singleton<GameConfig> {
    private GameConfig() { }  // 私有构造
}

GameConfig.Instance  // 全局访问
```

### `GlobalMonoSingleton<T>` (Runtime)

跨场景持久化 MonoBehaviour 单例。在 `Awake` 中自动执行 `DontDestroyOnLoad`，自动从父级分离，销毁重复实例。

```csharp
public class AudioManager : GlobalMonoSingleton<AudioManager> { }
AudioManager.Instance
```

| 成员 | 说明 |
|---|---|
| `Instance` | 查找或自动创建（附带 `AutoCreated` 后缀） |
| `HasInstance` | 是否已创建 |
| `Current` | 同 `Instance` |
| `UnparentOnAwake` | `true` 时自动 `transform.SetParent(null)` |

### `SceneSingleton<T>` (Runtime)

场景级单例，场景切换时销毁。不会自动 `DontDestroyOnLoad`。

## 服务定位器

### `ServiceLocator<T>` (Core)

父链查找模式的泛型服务定位器。

```csharp
public class ManagerLocator : ServiceLocator<ManagerLocator> { }
```

### `ManagerLocator` (Core)

具体的服务定位器实现，继承 `ServiceLocator<ManagerLocator>`。

## 对象池

### `ObjectPoolBase<T>` (Core)

对象池抽象基类，子类需实现：

```csharp
public abstract class ObjectPoolBase<T> where T : class {
    protected abstract T Create();
    protected abstract void Reset(T obj);
    protected abstract void OnDestroy(T obj);

    public T Get();          // 取出
    public void Return(T);   // 归还
    public void Clear();     // 清空
}
```
