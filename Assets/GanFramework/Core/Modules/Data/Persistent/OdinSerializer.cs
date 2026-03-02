using System;
using System.IO;
using System.Linq;
using GanFramework.Odin.OdinSerializer;
using GanFramework.Core.Data.Persistent.ReferenceResolver;

namespace GanFramework.Core.Data.Persistent
{
    // OdinSerializer 实现了 ISerializer 接口，使用 Odin Serializer 进行对象的序列化和反序列化。
    // 构造函数接受一个 DataFormat 参数来指定使用 JSON 还是二进制格式。
    // 在序列化和反序列化过程中，OdinSerializer 会创建一个上下文对象，并从 ReferenceResolverManager 获取字符串、GUID 和索引引用解析器，以正确处理对象引用。
    public class OdinSerializer : ISerializer
    {
        private readonly DataFormat dataFormat;
        private readonly ReferenceResolverManager _referenceResolverManager;
        
        public OdinSerializer(DataFormat dataFormat = DataFormat.Binary)
        {
            this.dataFormat = dataFormat;
            _referenceResolverManager = new ReferenceResolverManager();
            // 默认使用内置的 ResManagerReferenceResolver 来处理字符串引用
            _referenceResolverManager.AddStringResolver(new ResManagerReferenceResolver());
        }

        byte[] ISerializer.Serialize(object obj)
        {
            var context = new SerializationContext()
            {
                // Use FirstOrDefault to avoid throwing when no resolver was registered.
                // SerializationContext handles null resolvers safely.
                StringReferenceResolver = _referenceResolverManager.StringResolvers.FirstOrDefault(),
                GuidReferenceResolver = _referenceResolverManager.GuidResolvers.FirstOrDefault(),
                IndexReferenceResolver = _referenceResolverManager.IndexResolver
            };
            var bytes = SerializationUtility.SerializeValue(obj, dataFormat, context);
            return bytes;
        }

        public T Deserialize<T>(byte[] data)
        {
            var context = new DeserializationContext()
            {
                // Use FirstOrDefault to avoid throwing when no resolver was registered.
                // DeserializationContext handles null resolvers safely.
                StringReferenceResolver = _referenceResolverManager.StringResolvers.FirstOrDefault(),
                GuidReferenceResolver = _referenceResolverManager.GuidResolvers.FirstOrDefault(),
                IndexReferenceResolver = _referenceResolverManager.IndexResolver
            };
            return SerializationUtility.DeserializeValue<T>(data, dataFormat, context);
        }

        public string FileExtension
        {
            get
            {
                if(dataFormat == DataFormat.Binary)
                    return ".bin";
                if(dataFormat == DataFormat.JSON)
                    return ".json";
                else
                    return ".dat";
            }
        }
    }
}