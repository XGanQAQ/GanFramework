namespace GanFramework.Core.Data.Persistent.ReferenceResolver
{
    // 资源 GUID 接口，供 ResManagerReferenceResolver 使用
    // 推荐项目统一约定：所有可存档资源都必须实现这个接口，或者挂一个组件提供 GUID
    // 这样 ResManagerReferenceResolver 就能通过 GUID 从 ResManager 加载资源了
    // 如果不想强制资源实现这个接口，也可以在 ResManagerReferenceResolver 中添加其他方式获取 GUID 的逻辑（例如通过组件）
    public interface IResGuid
    {
        string Guid { get; }
    }
}