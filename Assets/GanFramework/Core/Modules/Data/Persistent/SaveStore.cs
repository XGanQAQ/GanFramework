using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GanFramework.Core.Data.Persistent
{
    /// <summary>
    /// 持久化数据保存管理器（已重构以支持可替换的序列化器）
    /// 现在命名为 SaveStore，表示它不再限定于 JSON 格式。
    /// 默认使用 JsonNetSerializer，但可通过可选参数传入任意实现了 ISaveSerializer 的序列化器。
    /// </summary>
    public static class SaveStore
    {
        // Use centralized default from utils
        private static readonly ISerializer DefaultSerializer = SaveStoreUtils.DefaultSerializer;

        // 保存类模式：直接保存整个实例，使用可选序列化器
        public static void Save<T>(T data, ISerializer serializer = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            serializer ??= DefaultSerializer;

            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            byte[] serializeData = serializer.Serialize(data);
            File.WriteAllBytes(path, serializeData);
        }

        public static T Load<T>(ISerializer serializer = null)
        {
            serializer ??= DefaultSerializer;

            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            
            if (!File.Exists(path)
                || new FileInfo(path).Length == 0)
                return default;

            byte[] data = File.ReadAllBytes(path);
            return serializer.Deserialize<T>(data);
        }

        // 直接加载到现有实例中，适用于单例等特殊情况
        public static T LoadInto<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            string path = SaveStoreUtils.GetPathForType<T>(serializer);

            if (!File.Exists(path)
                || new FileInfo(path).Length == 0)
                return instance;

            byte[] json = File.ReadAllBytes(path);
            T loaded = serializer.Deserialize<T>(json);

            // 将 loaded 的数据复制到 instance 中（覆盖原有数据）
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

        public static bool Exists<T>(ISerializer serializer = null)
        {
            serializer ??= DefaultSerializer;
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            return File.Exists(path);
        }

        public static void Delete<T>(ISerializer serializer = null)
        {
            serializer ??= DefaultSerializer;
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"存档已删除: {path}");
            }
        }

        // 保存实例中标记了 [SaveMember] 的成员到文件中，使用可选序列化器
        public static void SaveMembers<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            var dict = SaveStoreUtils.BuildMemberDictionary(type, instance);

            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            byte[] data = serializer.Serialize(dict);
            File.WriteAllBytes(path, data);
        }

        // 加载并创建新实例，适用于普通数据类
        public static T LoadMembers<T>(Func<T> factory = null, ISerializer serializer = null) where T : class
        {
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            if (!File.Exists(path)) return null;

            byte[] json = File.ReadAllBytes(path);
            var dict = serializer.Deserialize<Dictionary<string, object>>(json);
            T instance = factory != null ? factory() : Activator.CreateInstance<T>();

            SaveStoreUtils.ApplyMemberDictionaryToInstance(type, instance, dict, serializer);

            return instance;
        }

        // 直接加载到现有实例中，适用于单例等特殊情况
        public static void LoadMembersInto<T>(T instance, ISerializer serializer = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            serializer ??= DefaultSerializer;

            Type type = typeof(T);
            string path = SaveStoreUtils.GetPathForType<T>(serializer);
            if (!File.Exists(path)) return;

            byte[] json = File.ReadAllBytes(path);
            var dict = serializer.Deserialize<Dictionary<string, object>>(json);

            SaveStoreUtils.ApplyMemberDictionaryToInstance(type, instance, dict, serializer);
        }
    }
}