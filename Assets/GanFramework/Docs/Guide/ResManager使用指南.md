# ResManager 使用指南（精简版）

本指南用于快速理解 **ResManager 的实际使用方式与调用模式**，适合作为项目开发时的查阅文档或团队规范说明。

---

# 一、基础概念

ResManager 是整个资源系统唯一入口：

```
任何资源加载 → 必须通过 ResManager
```

它负责：

* 自动选择加载方式（Provider）
* 缓存资源
* 管理引用计数
* 防止重复加载
* 自动卸载无用资源

---

# 二、路径规范（必须掌握）

资源路径统一使用：

```
协议前缀://真实路径
```

示例：

```
res://UI/LoginPanel
file:///C:/icon.png
https://server/a.png
addr://Weapon/Sword
```

若不写前缀：

```
UI/LoginPanel
```

默认等价于：

```
res://UI/LoginPanel
```

---

# 三、同步加载

```csharp
var prefab = ResManager.Instance.Load<GameObject>("res://UI/LoginPanel");
```

特点：

* 自动缓存
* 自动引用计数 +1
* 若正在异步加载 → 自动等待完成

适用于：

* 初始化
* 必须立即拿到资源
* 编辑器工具

---

# 四、异步加载（推荐）

### 1️⃣ await 写法（最推荐）

```csharp
var prefab = await ResManager.Instance.LoadAsync<GameObject>("res://UI/LoginPanel");
```

优点：

* 不阻塞主线程
* 自动合并重复加载请求

---

### 2️⃣ 回调写法（兼容旧代码）

```csharp
ResManager.Instance.LoadAsyncCallback<GameObject>(
    "res://UI/LoginPanel",
    prefab => { }
);
```

---

### 3️⃣ 句柄写法（进度条）

```csharp
var op = ResManager.Instance.LoadAsyncHandle<GameObject>("res://UI/LoginPanel");

op.Completed += obj => { };
float p = op.Progress;
```

适用于：

* Loading界面
* 进度UI
* 资源下载显示

---

# 五、批量加载

```csharp
var op = ResManager.Instance.BatchLoadAsync(list);
```

用途：

* 场景切换
* 预加载
* UI资源包

---

# 六、卸载资源

### 普通卸载（引用-1）

```csharp
ResManager.Instance.UnloadAsset<GameObject>("res://UI/LoginPanel");
```

---

### 强制立即卸载

```csharp
ResManager.Instance.UnloadAsset<GameObject>(
    "res://UI/LoginPanel",
    true
);
```

资源只有在满足条件才会释放：

```
refCount == 0 && isDel == true
```

---

# 七、清理未使用资源

```csharp
await ResManager.Instance.UnloadUnusedAssets();
```

用途：

* 切场景后
* 大量资源释放后
* 内存优化阶段

---

# 八、清空所有缓存（场景切换推荐）

### 异步版

```csharp
await ResManager.Instance.ClearDicAsync();
```

### 同步版

```csharp
ResManager.Instance.ClearDic();
```

---

# 九、调试接口

获取某资源引用数：

```csharp
int count = ResManager.Instance.GetRefCount<GameObject>(path);
```

用于：

* 内存排查
* 泄漏检测
* 调试工具窗口

---

# 十、最佳实践规范（团队强制建议）

### ✔ 必须遵守

* 禁止直接使用 Resources.Load
* 禁止直接 Addressables.Load
* 禁止直接 UnityWebRequest 加载资源

全部必须走：

```
ResManager
```

---

### ✔ 推荐路径规范

```
UI → res://UI/xxx
配置 → addr://Config/xxx
远程资源 → https://
本地缓存 → file://
```

---

### ✔ 推荐释放策略

| 资源类型      | 建议     |
| --------- | ------ |
| UI Prefab | 用完立即卸载 |
| 角色模型      | 场景结束卸载 |
| 图集        | 场景卸载   |
| 配置        | 常驻     |

---

# 十一、常见错误用法

❌ 错误

```
Load → 不Unload
```

✔ 正确

```
Load → 使用 → Unload
```

---

❌ 错误

```
直接 new WWW()
```

✔ 正确

```
ResManager.LoadAsync
```

---

# 十二、一句话核心总结

> ResManager = 项目唯一资源入口 + 生命周期控制器 + 加载调度中心
