using System;
using System.Text;
using GanFramework.Core.Data.Persistent;
using Newtonsoft.Json;

namespace GanFramework.UnityRuntime.Persistent
{
    public class JsonNetSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // Preserve references to avoid infinite recursion on circular graphs
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            // When preserving references, serialize reference loops instead of throwing
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            // Include type information for objects so deserialization back to object works better
            TypeNameHandling = TypeNameHandling.Auto,
            // Limit depth to avoid runaway recursion
            MaxDepth = 64
        };

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0) return default;
            string text = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(text, Settings);
        }
        
        public byte[] Serialize(object obj)
        {
            if (obj == null) return Array.Empty<byte>();
            string text = JsonConvert.SerializeObject(obj, Settings);
            return Encoding.UTF8.GetBytes(text);
        }

        public string FileExtension => ".json";
    }
}
