# Editor Tools

Editor 工具代码，仅在 `#if UNITY_EDITOR` 下编译。
菜单位置：`GanFramework/` 子菜单。

## SaveTestWindow

持久化模块的测试工具。

```
菜单: GanFramework/Test/Save Test Window
```

- 支持选择 `SaveFormat`（OdinBinary / OdinJson）
- 同步/异步的 Save / Load / SaveMembers / LoadMembers 操作
- 显示文件路径，支持查看文件内容和删除

### TestSaveData

测试数据类，演示 `[SaveClass]` 和 `[SaveMember]` 的用法：

```csharp
[SaveClass("testdata")]
public class TestSaveData {
    [SaveMember] public int Level;
    [SaveMember] public string PlayerName;
    [SaveMember("hp")] public float Health;
    [SaveMember] public Vector3 position;
    [SaveMember] public Color color;
    [SaveMember] public Dictionary<string, int> DictStringInt;
    [SaveMember] public Dictionary<int, string> DictIntString;
}
```

## AutoColliderFitter

自动适配网格碰撞器的工具。

```
菜单: GanFramework/Tools/Auto Collider Fitter
```

在选定 GameObject 上自动添加/调整碰撞体以匹配网格形状。

| 选项 | 说明 |
|---|---|
| `ColliderType.Auto` | 自动选择碰撞体类型 |
| `ColliderType.Box` | 盒碰撞体 |
| `ColliderType.Sphere` | 球碰撞体 |
| `ColliderType.Capsule` | 胶囊碰撞体 |
