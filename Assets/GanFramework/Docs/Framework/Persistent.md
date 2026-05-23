# Persistent

持久化存储模块。Core 定义接口和特性，Runtime 提供实现。

## 架构

```
IPersistent (Core 接口)
  └─ PersistentService (Runtime, 实现 IPersistent + IModules)
       ├─ SaveStore (同步文件 IO)
       └─ AsyncSaveStore (异步文件 IO)
            └─ SaveStoreUtils (路径、反射工具)

ISerializer (Core 接口)
  ├─ JsonNetSerializer (Runtime, Newtonsoft.Json)
  └─ OdinSerializer (Runtime, OdinSerializer 二进制/JSON)
       └─ ReferenceResolverManager
            ├─ ResManagerReferenceResolver (资源 GUID 解析)
            └─ UnityObjIndexReferenceResolver (Unity Object 索引)
```

## 核心接口

### `IPersistent` (Core)

```csharp
public interface IPersistent {
    // 同步
    void Save<T>(T data, ISerializer serializer = null);
    T Load<T>(ISerializer serializer = null);
    T LoadInto<T>(T instance, ISerializer serializer = null);
    bool Exists<T>(ISerializer serializer = null);
    void Delete<T>(ISerializer serializer = null);
    void SaveMembers<T>(T instance, ISerializer serializer = null);
    T LoadMembers<T>(Func<T> factory = null, ISerializer serializer = null) where T : class;
    void LoadMembersInto<T>(T instance, ISerializer serializer = null);
    // 异步（同上，Async 后缀）
    Task SaveAsync<T>(T data, ISerializer serializer = null);
    // ...
}
```

### `ISerializer` (Core)

```csharp
public interface ISerializer {
    byte[] Serialize(object obj);
    T Deserialize<T>(byte[] data);
    string FileExtension { get; }
}
```

## 用法

### 基础保存/读取

```csharp
// 通过接口注入（推荐）
var save = Framework.GetModule<IPersistent>();

save.Save(playerData, new OdinSerializer(DataFormat.Binary));
var loaded = save.Load<PlayerData>(new OdinSerializer(DataFormat.Binary));
```

### SaveFormat 快捷重载（需获取具体类型）

```csharp
// PersistentService 额外提供 SaveFormat 重载
var service = (PersistentService)Framework.GetModule<IPersistent>();
service.Save(playerData, SaveFormat.OdinBinary);
```

### 字段级存档

```csharp
[SaveClass("player")]
public class PlayerData {
    [SaveMember] public string Name;
    [SaveMember("hp")] public float Health;  // 自定义键名
    [SaveMember] public int Level;
    public float TempNotSaved;  // 不保存
}

// 仅保存标记了 [SaveMember] 的字段
save.SaveMembers(data, serializer);
save.LoadMembersInto(data, serializer);
```

### 路径规则

文件保存到 `Application.persistentDataPath`，文件名 = `[SaveClass].Key` + `serializer.FileExtension`。

### 异步

```csharp
await save.SaveAsync(data, serializer);
var data = await save.LoadAsync<PlayerData>(serializer);
```

## 序列化器

| 实现 | 格式 | 特性 |
|---|---|---|
| `JsonNetSerializer` | `.json` | Newtonsoft.Json，PreserveReferences |
| `OdinSerializer(DataFormat.Binary)` | `.bin` | Odin 二进制，支持 Unity Object 引用 |
| `OdinSerializer(DataFormat.JSON)` | `.json` | Odin JSON，支持 Unity Object 引用 |

## 引用解析器

仅在 `OdinSerializer` 中生效：

| 解析器 | 用途 |
|---|---|
| `ResManagerReferenceResolver` | 通过 `IResGuid` 接口 + `IResManager` 按 GUID 加载资源 |
| `UnityObjIndexReferenceResolver` | 按索引表存储 Unity Object 引用（当前 `CanReference` 始终返回 `false`，疑似未完成） |

## 注意

- `LoadInto<T>` 通过反射复制顶层公共属性，不同于 `LoadMembersInto`（继承 `[SaveMember]`）
- `OdinSerializer` 依赖 `GanFramework.Odin.OdinSerializer` 内置库
- `SaveFormat` 枚举（`OdinBinary`, `OdinJson`）在 Runtime 定义，Core 不可见
