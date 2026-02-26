using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Add Newtonsoft JObject handling
using Newtonsoft.Json.Linq;

namespace GanFramework.Core.Data.Persistent
{
    internal static class SaveStoreUtils
    {
        public static readonly ISerializer DefaultSerializer = new JsonNetSerializer();

        // 根据类型获取保存路径，要求类型必须标记 [SaveClass]，并使用其 Key 作为文件名的一部分。支持传入序列化器以获取正确的文件扩展名。
        public static string GetPathForType<T>(ISerializer serializer)
        {
            var type = typeof(T);
            var attr = type.GetCustomAttribute<SaveClassAttribute>();
            if (attr == null)
                throw new InvalidOperationException($"类型 {type.Name} 未标记 [SaveClass]");

            string key = attr.Key;
            string ext = serializer?.FileExtension ?? DefaultSerializer.FileExtension;
            return Path.Combine(Application.persistentDataPath, $"{key}{ext}");
        }

        // 获取所有标记了 [SaveMember] 的成员（字段和属性）。对于属性，包含所有访问级别（public/private）以支持更灵活的设计。返回 MemberInfo 列表以便后续统一处理。
        public static IEnumerable<MemberInfo> GetSavedMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = type.GetFields(flags).Cast<MemberInfo>();
            var props = type.GetProperties(flags).Cast<MemberInfo>();
            return fields.Concat(props)
                .Where(m => m.GetCustomAttribute<SaveMemberAttribute>() != null);
        }

        // 获取成员值，支持字段和属性（仅当属性可读时）。对于属性，先检查 CanRead 以避免运行时异常。
        public static object GetMemberValue(MemberInfo member, object instance)
        {
            if (member is FieldInfo f) return f.GetValue(instance);
            if (member is PropertyInfo p && p.CanRead) return p.GetValue(instance);
            return null;
        }

        // 设置成员值，支持字段和属性（仅当属性可写时）。对于属性，先检查 CanWrite 以避免运行时异常。
        public static void SetMemberValue(MemberInfo member, object instance, object value)
        {
            if (member is FieldInfo f) f.SetValue(instance, value);
            else if (member is PropertyInfo p && p.CanWrite) p.SetValue(instance, value);
        }

        // 构建一个字典，包含所有标记了 [SaveMember] 的成员及其值。对于 UnityEngine.Object 类型的成员，存储特殊的元数据以便后续解析。
        public static Dictionary<string, object> BuildMemberDictionary(Type type, object instance)
        {
            var members = GetSavedMembers(type);
            var dict = new Dictionary<string, object>();
            foreach (var m in members)
            {
                var attr = m.GetCustomAttribute<SaveMemberAttribute>();
                string key = string.IsNullOrEmpty(attr?.Key) ? m.Name : attr.Key;
                object val = GetMemberValue(m, instance);
                dict[key] = val;
            }

            return dict;
        }

        // 将字典中的数据应用到实例的成员上
        public static void ApplyMemberDictionaryToInstance(Type type, object instance, Dictionary<string, object> dict,
            ISerializer serializer)
        {
            var members = GetSavedMembers(type);
            foreach (var m in members)
            {
                var attr = m.GetCustomAttribute<SaveMemberAttribute>();
                string key = string.IsNullOrEmpty(attr?.Key) ? m.Name : attr.Key;
                if (!dict.TryGetValue(key, out var boxed)) continue;

                Type memberType = m is FieldInfo ff ? ff.FieldType : ((PropertyInfo)m).PropertyType;

                // Handle simple casting for primitive types and when boxed is already of the right type
                if (boxed != null && memberType.IsInstanceOfType(boxed))
                {
                    SetMemberValue(m, instance, boxed);
                    continue;
                }

                var value = Convert.ChangeType(boxed, memberType);
                SetMemberValue(m, instance, value);
            }
        }
    }
}