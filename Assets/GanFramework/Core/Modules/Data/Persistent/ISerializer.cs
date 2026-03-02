namespace GanFramework.Core.Data.Persistent
{
    /// <summary>
    /// 抽象序列化器，用于将对象序列化为字符串并从字符串反序列化回对象。
    /// 这样可以把保存格式（JSON、二进制、protobuf 等）与保存逻辑解耦。
    /// FileExtension 属性用于决定默认文件扩展名（例如 ".json"）。
    /// </summary>
    
    public interface ISerializer
    {
        byte[] Serialize(object obj);
        T Deserialize<T>(byte[] data);
        string FileExtension { get; }
    }

}
