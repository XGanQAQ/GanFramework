// JsonToSOTransformer: 使用 OdinSerializer 优先反序列化 JSON 为对象，失败时回退到 JObject 解析并生成 ScriptableObject
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using GanFramework.Core.Data;
using System.Text;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GanFramework.Core.Data.Config
{
    /// <summary>
    /// 通过任意类型的 JSON 生成 ScriptableObject 资源（不依赖具体类型）。
    /// 支持优先使用 OdinSerializer(JSON) 反序列化为目标类型；若失败，则回退为逐字段解析并支持内联子 SO 与引用 assetPath 加载。
    /// JSON 格式示例:
    /// {
    ///   "type": "Namespace.MySO, AssemblyName",
    ///   "fields": { ... }
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

            // 首先尝试使用 OdinSerializer (JSON 格式) 进行反序列化
            try
            {
                ISerializer serializer = new OdinSerializer(Odin.OdinSerializer.DataFormat.JSON);
                var bytes = Encoding.UTF8.GetBytes(json);
                var deserializeMethod = typeof(OdinSerializer).GetMethod("Deserialize");
                if (deserializeMethod != null && deserializeMethod.IsGenericMethodDefinition)
                {
                    var generic = deserializeMethod.MakeGenericMethod(type);
                    var deserialized = generic.Invoke(serializer, new object[] { bytes });

                    if (deserialized is ScriptableObject desSo)
                    {
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            AssetDatabase.CreateAsset(desSo, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        return desSo;
                    }

                    // 如果 Odin 返回的是普通对象（非 Unity 引擎对象），创建 ScriptableObject 实例并拷贝字段
                    var so = (ScriptableObject)ScriptableObject.CreateInstance(type);
                    CopyValues(deserialized, so);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        AssetDatabase.CreateAsset(so, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    return so;
                }
            }
            catch
            {
                // 忽略 Odin 反序列化的错误，回退到原有的 JObject 解析逻辑
            }

            // 回退：使用逐字段解析逻辑
            var soFallback = ScriptableObject.CreateInstance(type);
            var fieldsToken = j["fields"] as JObject;
            if (fieldsToken != null) PopulateObject(soFallback, fieldsToken, assetPath);

            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.CreateAsset(soFallback, assetPath);
                // 如果有子资产已被创建，它们会被 AddObjectToAsset 在 PopulateObject 中处理
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return soFallback;
        }


        static Type ResolveType(string name)
        {
            var t = Type.GetType(name);
            if (t != null) return t;
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

            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                if (token.Type == JTokenType.Object)
                {
                    var assetPath = (string)token["assetPath"];
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        return AssetDatabase.LoadAssetAtPath(assetPath, targetType);
                    }
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

                if (token.Type == JTokenType.String)
                {
                    var path = (string)token;
                    var loaded = AssetDatabase.LoadAssetAtPath(path, targetType);
                    return loaded;
                }
            }

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

        static void CopyValues(object src, object dst)
        {
            if (src == null || dst == null) return;
            var srcType = src.GetType();
            var dstType = dst.GetType();

            foreach (var sf in srcType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var df = dstType.GetField(sf.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (df == null) continue;
                var sval = sf.GetValue(src);
                if (sval == null) { df.SetValue(dst, null); continue; }
                if (df.FieldType.IsAssignableFrom(sf.FieldType)) df.SetValue(dst, sval);
            }

            foreach (var sp in srcType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!sp.CanRead) continue;
                var dp = dstType.GetProperty(sp.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (dp == null || !dp.CanWrite) continue;
                var sval = sp.GetValue(src, null);
                if (sval == null) { dp.SetValue(dst, null, null); continue; }
                if (dp.PropertyType.IsAssignableFrom(sp.PropertyType)) dp.SetValue(dst, sval, null);
            }
        }

        public static string SerializeToOdinJson(object obj)
        {
            var serializer = new OdinSerializer(Odin.OdinSerializer.DataFormat.JSON);
            var bytes = ((ISerializer)serializer).Serialize(obj);
            return Encoding.UTF8.GetString(bytes);
        }

        // 自动扫描所有 ScriptableObject 类型
        // 如果提供 assemblyFolder，会尝试加载该文件夹下的 .dll 并一并扫描
        public static Dictionary<string, Type> GetAllSOTypes(string asmdefFolder = null)
        {
            var soTypes = new Dictionary<string, Type>();
            // 如果提供了 asmdef 文件夹，则读取 asmdef 并仅扫描这些 asmdef 对应的已编译程序集
            if (!string.IsNullOrEmpty(asmdefFolder) && Directory.Exists(asmdefFolder))
            {
                try
                {
                    var asmdefFiles = Directory.GetFiles(asmdefFolder, "*.asmdef", SearchOption.AllDirectories);
                    var asmNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var f in asmdefFiles)
                    {
                        try
                        {
                            var txt = File.ReadAllText(f);
                            var jo = JObject.Parse(txt);
                            var nameToken = jo["name"];
                            if (nameToken != null)
                            {
                                var asmName = (string)nameToken;
                                if (!string.IsNullOrEmpty(asmName)) asmNames.Add(asmName);
                            }
                        }
                        catch { }
                    }

                    // project Library/ScriptAssemblies path
                    string libDir = null;
                    try { libDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "ScriptAssemblies")); } catch { libDir = null; }

                    foreach (var asmName in asmNames)
                    {
                        // 先尝试在已加载 AppDomain 程序集中查找
                        var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, asmName, StringComparison.OrdinalIgnoreCase));
                        if (asm == null && !string.IsNullOrEmpty(libDir))
                        {
                            var dllPath = Path.Combine(libDir, asmName + ".dll");
                            if (File.Exists(dllPath))
                            {
                                try { asm = Assembly.LoadFrom(dllPath); } catch { asm = null; }
                            }
                        }

                        if (asm == null) continue;

                        try
                        {
                            var types = asm.GetTypes().Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract);
                            foreach (var type in types)
                            {
                                string key = $"{type.FullName}, {type.Assembly.GetName().Name}";
                                if (!soTypes.ContainsKey(key)) soTypes.Add(key, type);
                            }
                        }
                        catch (ReflectionTypeLoadException) { }
                        catch { }
                    }
                }
                catch { }

                return soTypes;
            }
            return soTypes;
        }
    }
}
#endif