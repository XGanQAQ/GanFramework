using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GanFramework.Core.Data.Persistent
{
    /// <summary>
    /// 异步版的 SaveStore，提供与同步版一致的功能但使用异步文件 I/O。
    /// 该类现在复用 SaveStoreUtils 中的通用逻辑以避免重复代码。
    /// </summary>
    public static class AsyncSaveStore
    {
        private static readonly ISerializer DefaultSerializer = SaveStoreUtils.DefaultSerializer;

        public static async Task SaveAsync<T>(T data, ISerializer serializer = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            serializer ??= DefaultSerializer;

            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            byte[] bytes = serializer.Serialize(data);
            await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
        }

        public static async Task<T> LoadAsync<T>(ISerializer serializer = null)
        {
            serializer ??= DefaultSerializer;
            string path = SaveStoreUtils.GetPathForType<T>(serializer);

            if (!File.Exists(path))
                return default;

            byte[] bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            return serializer.Deserialize<T>(bytes);
        }

        public static async Task<T> LoadIntoAsync<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            string path = SaveStoreUtils.GetPathForType<T>(serializer);

            if (!File.Exists(path))
                return instance;

            byte[] bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            T loaded = serializer.Deserialize<T>(bytes);

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(loaded);
                    prop.SetValue(instance, value);
                }
            }

            return instance;
        }

        public static async Task SaveMembersAsync<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            var dict = SaveStoreUtils.BuildMemberDictionary(type, instance);

            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            byte[] data = serializer.Serialize(dict);
            await File.WriteAllBytesAsync(path, data).ConfigureAwait(false);
        }

        public static async Task<T> LoadMembersAsync<T>(Func<T> factory = null, ISerializer serializer = null) where T : class
        {
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            if (!File.Exists(path)) return null;

            byte[] bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            var dict = serializer.Deserialize<Dictionary<string, object>>(bytes);
            T instance = factory != null ? factory() : Activator.CreateInstance<T>();

            SaveStoreUtils.ApplyMemberDictionaryToInstance(type, instance, dict, serializer);

            return instance;
        }

        public static async Task LoadMembersIntoAsync<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            if (!File.Exists(path)) return;

            byte[] bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            var dict = serializer.Deserialize<Dictionary<string, object>>(bytes);

            SaveStoreUtils.ApplyMemberDictionaryToInstance(type, instance, dict, serializer);
        }
    }
}
