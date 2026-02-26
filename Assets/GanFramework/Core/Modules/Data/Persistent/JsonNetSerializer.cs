using System;
using System.Text;
using Newtonsoft.Json;

namespace GanFramework.Core.Data.Persistent
{
    public class JsonNetSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
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
