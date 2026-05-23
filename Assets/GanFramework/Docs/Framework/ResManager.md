# ResManager

资源管理器，封装 `Resources.Load` 并增加缓存。

## 核心类型

| 类型 | 位置 | 说明 |
|---|---|---|
| `IResManager` | Core | 资源管理器接口 |
| `ResManager` | Runtime | 实现，`IModules` + `IResManager` |

## 接口

```csharp
public interface IResManager {
    T Load<T>(string path) where T : Object;
    T GetFromCache<T>(string path) where T : Object;
}
```

## 用法

```csharp
var res = Framework.GetModule<IResManager>();

// 加载（首次从 Resources 加载并缓存）
GameObject prefab = res.Load<GameObject>("UI/MyPanel");

// 仅从缓存获取（不调用 Resources.Load）
GameObject cached = res.GetFromCache<GameObject>("UI/MyPanel");
```

## 注意

- 等同于 `Resources.Load<T>(path)` + 内部 `Dictionary<string, Object>` 缓存
- `GetFromCache` 在未缓存时返回 `null`
- 不处理异步加载（单元测试场景可替换 `IResManager` 实现）
