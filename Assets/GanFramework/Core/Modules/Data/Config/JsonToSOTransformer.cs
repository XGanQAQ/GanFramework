// ...existing code...
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GanFramework.Core.Data.Config
{
    /// <summary>
    /// 通过任意类型的 JSON 生成 ScriptableObject 资源（不依赖具体类型）。
    /// JSON 格式示例:
    /// {
    ///   "type": "Namespace.MySO, AssemblyName",
    ///   "fields": {
    ///     "intField": 123,
    ///     "stringField": "hello",
    ///     "enumField": "EnumName",
    ///     "refAsset": { "assetPath": "Assets/Path/To/Asset.asset" },
    ///     "nestedSO": {
    ///       "type": "Namespace.NestedSO, AssemblyName",
    ///       "fields": { ... }
    ///     },
    ///     "listField": [1,2,3]
    ///   }
    /// }
    /// </summary>
    public static class JsonToSOTransformer
    {
        public static ScriptableObject CreateFromJson(string json, string assetPath = null)
        {
            var j = JObject.Parse(json);
            var typeToken = j["type"];
            if (typeToken == null) throw new ArgumentException("JSON must contain top-level 'type'.");

            var type = ResolveType((string)typeToken);
            if (type == null) throw new ArgumentException($"Type not found: {(string)typeToken}");
            if (!typeof(ScriptableObject).IsAssignableFrom(type)) throw new ArgumentException("Type must derive from ScriptableObject.");

            var so = ScriptableObject.CreateInstance(type);
            var fieldsToken = j["fields"] as JObject;
            if (fieldsToken != null) PopulateObject(so, fieldsToken, assetPath);

            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.CreateAsset(so, assetPath);
                // 如果有子资产已被创建，它们会被 AddObjectToAsset 在 PopulateObject 中处理
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return so;
        }

        static Type ResolveType(string name)
        {
            // First try assembly-qualified
            var t = Type.GetType(name);
            if (t != null) return t;
            // Otherwise search loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static void PopulateObject(object target, JObject fields, string parentAssetPath)
        {
            var type = target.GetType();
            foreach (var kv in fields)
            {
                var name = kv.Key;
                var token = kv.Value;
                var fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var propInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var memberType = (fieldInfo != null) ? fieldInfo.FieldType : propInfo?.PropertyType;

                if (memberType == null) continue;

                var value = ConvertTokenToValue(token, memberType, parentAssetPath, out var createdSubAssets);
                if (value == null && token.Type == JTokenType.Null) value = null;

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(target, value);
                }
                else if (propInfo != null && propInfo.CanWrite)
                {
                    propInfo.SetValue(target, value, null);
                }

                // 如果有新建的子资产需要添加到同一个 asset 文件
                if (createdSubAssets != null && !string.IsNullOrEmpty(parentAssetPath))
                {
                    foreach (var sub in createdSubAssets)
                    {
                        AssetDatabase.AddObjectToAsset(sub, parentAssetPath);
                    }
                }
            }
        }

        static object ConvertTokenToValue(JToken token, Type targetType, string parentAssetPath, out List<UnityEngine.Object> createdSubAssets)
        {
            createdSubAssets = null;

            if (token == null || token.Type == JTokenType.Null) return null;

            // UnityEngine.Object 引用 (用 assetPath 指定)
            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                if (token.Type == JTokenType.Object)
                {
                    var assetPath = (string)token["assetPath"];
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        return AssetDatabase.LoadAssetAtPath(assetPath, targetType);
                    }
                    // 内联生成 nested ScriptableObject 并作为子资产
                    var nestedTypeToken = token["type"];
                    if (nestedTypeToken != null)
                    {
                        var nestedType = ResolveType((string)nestedTypeToken);
                        if (nestedType != null && typeof(ScriptableObject).IsAssignableFrom(nestedType))
                        {
                            var nestedSo = ScriptableObject.CreateInstance(nestedType);
                            var fieldsObj = token["fields"] as JObject;
                            if (fieldsObj != null) PopulateObject(nestedSo, fieldsObj, parentAssetPath);
                            createdSubAssets = new List<UnityEngine.Object> { nestedSo };
                            return nestedSo;
                        }
                    }
                }

                // 尝试从 GUID 或 path 字符串加载
                if (token.Type == JTokenType.String)
                {
                    var path = (string)token;
                    var loaded = AssetDatabase.LoadAssetAtPath(path, targetType);
                    return loaded;
                }
            }

            // 可枚举类型（数组 / List<T>）
            if (token.Type == JTokenType.Array && (targetType.IsArray || IsGenericList(targetType)))
            {
                var elemType = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
                var arr = (JArray)token;
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
                foreach (var jt in arr)
                {
                    var createdSub = ConvertTokenToValue(jt, elemType, parentAssetPath, out var childCreated);
                    if (childCreated != null)
                    {
                        if (createdSubAssets == null) createdSubAssets = new List<UnityEngine.Object>();
                        createdSubAssets.AddRange(childCreated);
                    }
                    list.Add(createdSub);
                }

                if (targetType.IsArray)
                {
                    var a = Array.CreateInstance(elemType, list.Count);
                    list.CopyTo(a, 0);
                    return a;
                }
                else
                {
                    var concrete = Activator.CreateInstance(targetType);
                    var add = targetType.GetMethod("Add");
                    foreach (var item in list) add.Invoke(concrete, new object[] { item });
                    return concrete;
                }
            }

            // 基本类型与枚举
            if (targetType.IsEnum)
            {
                if (token.Type == JTokenType.String)
                    return Enum.Parse(targetType, (string)token);
                return Enum.ToObject(targetType, (int)token);
            }

            if (targetType == typeof(string)) return token.ToString();
            if (targetType == typeof(int)) return token.ToObject<int>();
            if (targetType == typeof(float)) return token.ToObject<float>();
            if (targetType == typeof(double)) return token.ToObject<double>();
            if (targetType == typeof(bool)) return token.ToObject<bool>();
            if (targetType == typeof(long)) return token.ToObject<long>();
            if (targetType == typeof(short)) return token.ToObject<short>();
            if (targetType == typeof(byte)) return token.ToObject<byte>();

            // 复杂对象：尝试构造并填充普通 POCO 或 ScriptableObject
            if (token.Type == JTokenType.Object)
            {
                var obj = token as JObject;

                if (typeof(ScriptableObject).IsAssignableFrom(targetType))
                {
                    var so = ScriptableObject.CreateInstance(targetType);
                    var fieldsToken = obj["fields"] as JObject ?? obj;
                    PopulateObject(so, fieldsToken, parentAssetPath);
                    createdSubAssets = new List<UnityEngine.Object> { so };
                    return so;
                }
                else
                {
                    var instance = Activator.CreateInstance(targetType);
                    foreach (var p in obj.Properties())
                    {
                        var f = targetType.GetField(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var pr = targetType.GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        Type memberType = f != null ? f.FieldType : pr?.PropertyType;
                        if (memberType == null) continue;
                        var child = ConvertTokenToValue(p.Value, memberType, parentAssetPath, out var childCreated);
                        if (childCreated != null)
                        {
                            if (createdSubAssets == null) createdSubAssets = new List<UnityEngine.Object>();
                            createdSubAssets.AddRange(childCreated);
                        }
                        if (f != null) f.SetValue(instance, child);
                        else if (pr != null && pr.CanWrite) pr.SetValue(instance, child, null);
                    }
                    return instance;
                }
            }

            // 作为最后手段，尝试直接转换
            try
            {
                return token.ToObject(targetType);
            }
            catch
            {
                return null;
            }
        }

        static bool IsGenericList(Type t)
        {
            return t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>)
                || t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
        }
    }
}