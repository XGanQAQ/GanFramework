using System;
using System.Threading.Tasks;
using GanFramework.Odin.OdinSerializer;

namespace GanFramework.Core.Data.Persistent
{
    // 保存格式枚举，定义了支持的保存格式类型
    public enum SaveFormat
    {
        OdinBinary,
        OdinJson,
    }

    /// <summary>
    /// 保存入口：根据指定的保存格式（SaveFormat）选择对应的序列化器
    /// 并委托到底层的 SaveStore（其已被设计为支持可替换的序列化器）。
    /// 支持传入自定义的 ISaveSerializer 以覆盖默认实现。
    /// 同时包含异步 API（SaveAsync/LoadAsync/...）。
    /// </summary>
    public static class SaveManager
    {
        private static ISerializer GetSerializerForFormat(SaveFormat format)
        {
            switch (format)
            {
                case SaveFormat.OdinBinary:
                    return new OdinSerializer(DataFormat.Binary);
                case SaveFormat.OdinJson:
                    return new OdinSerializer(DataFormat.JSON);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        #region Sync APIs

        public static void Save<T>(T data, SaveFormat format)
        {
            Save(data, format, null);
        }

        public static void Save<T>(T data, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            SaveStore.Save(data, serializer);
        }

        public static T Load<T>(SaveFormat format)
        {
            return Load<T>(format, null);
        }

        public static T Load<T>(SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            return SaveStore.Load<T>(serializer);
        }

        public static bool Exists<T>(SaveFormat format = SaveFormat.OdinBinary)
        {
            var serializer = GetSerializerForFormat(format);
            return SaveStore.Exists<T>(serializer);
        }

        public static void Delete<T>(SaveFormat format = SaveFormat.OdinBinary)
        {
            var serializer = GetSerializerForFormat(format);
            SaveStore.Delete<T>(serializer);
        }

        // Members APIs
        public static void SaveMembers<T>(T instance, SaveFormat format)
        {
            SaveMembers(instance, format, null);
        }

        public static void SaveMembers<T>(T instance, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            SaveStore.SaveMembers(instance, serializer);
        }

        public static T LoadMembers<T>(SaveFormat format, Func<T> factory = null) where T : class
        {
            return LoadMembers(factory, format, null);
        }

        public static T LoadMembers<T>(Func<T> factory, SaveFormat format, ISerializer serializer) where T : class
        {
            serializer ??= GetSerializerForFormat(format);
            return SaveStore.LoadMembers(factory, serializer);
        }

        public static void LoadMembersInto<T>(T instance, SaveFormat format)
        {
            LoadMembersInto(instance, format, null);
        }

        /// <summary>
        /// 将保存的数据加载到现有实例的成员中，而不是创建新实例。
        /// </summary> 
        /// <param name="instance">要加载数据的现有实例</param> 
        /// <param name="format">保存格式</param> 
        /// <param name="serializer">可选的序列化器，如果为 null 则根据 format 获取默认序列化器</param>
        public static void LoadMembersInto<T>(T instance, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            SaveStore.LoadMembersInto(instance, serializer);
        }

        #endregion

        #region Async APIs
        public static Task SaveAsync<T>(T data, SaveFormat format)
        {
            return SaveAsync(data, format, null);
        }

        public static Task SaveAsync<T>(T data, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            return AsyncSaveStore.SaveAsync(data, serializer);
        }

        public static Task<T> LoadAsync<T>(SaveFormat format)
        {
            return LoadAsync<T>(format, null);
        }

        public static Task<T> LoadAsync<T>(SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            return AsyncSaveStore.LoadAsync<T>(serializer);
        }

        public static Task SaveMembersAsync<T>(T instance, SaveFormat format)
        {
            return SaveMembersAsync(instance, format, null);
        }

        public static Task SaveMembersAsync<T>(T instance, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            return AsyncSaveStore.SaveMembersAsync(instance, serializer);
        }

        public static Task<T> LoadMembersAsync<T>(SaveFormat format, Func<T> factory = null) where T : class
        {
            return LoadMembersAsync(factory, format, null);
        }

        public static Task<T> LoadMembersAsync<T>(Func<T> factory, SaveFormat format, ISerializer serializer)
            where T : class
        {
            serializer ??= GetSerializerForFormat(format);
            return AsyncSaveStore.LoadMembersAsync(factory, serializer);
        }

        public static Task LoadMembersIntoAsync<T>(T instance, SaveFormat format)
        {
            return LoadMembersIntoAsync(instance, format, null);
        }

        /// <summary>
        /// 将保存的数据加载到现有实例的成员中，而不是创建新实例。
        /// </summary>
        /// <param name="instance">要加载数据的现有实例</param>
        /// <param name="format">保存格式</param>
        /// <param name="serializer">可选的序列化器，如果为 null 则根据 format 获取默认序列化器</param>
        public static Task LoadMembersIntoAsync<T>(T instance, SaveFormat format, ISerializer serializer)
        {
            serializer ??= GetSerializerForFormat(format);
            return AsyncSaveStore.LoadMembersIntoAsync(instance, serializer);
        }

        #endregion 


    }
}