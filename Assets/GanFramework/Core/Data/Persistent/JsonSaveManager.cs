using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace GanFramework.Core.Data.Persistent
{
    /// <summary>
    /// 持久化数据保存管理器
    /// 以Json格式保存读取数据
    /// Json文件保存在 Application.persistentDataPath 当中
    /// 保存的数据类必须带有SaveKey标签
    /// Json文件的名称为SaveKey标签设置的名称
    /// </summary>
    public static class JsonSaveManager
    {
        private static readonly JsonSerializerSettings Settings =
            new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

        private static string GetPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static void Save<T>(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string fileName = GetPathByType<T>();

            string path = Path.Combine(Application.persistentDataPath, fileName);
            string json = JsonConvert.SerializeObject(data, Settings);
            File.WriteAllText(path, json);
        }

        public static T Load<T>()
        {
            string fileName = GetPathByType<T>();

            string path = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(path))
                return default;

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static bool Exists<T>()
        {
            string path = GetPathByType<T>();
            return File.Exists(path);
        }

        public static void Delete<T>()
        {
            string path = GetPathByType<T>();
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"存档已删除: {path}");
            }
        }
        
        private static string GetPathByType(Type type)
        {
            string key = SaveKeyResolver.GetSaveKey(type);
            return Path.Combine(Application.persistentDataPath, $"{key}.json");
        }

        private static string GetPathByType<T>()
        {
            return GetPathByType(typeof(T));
        }
    }
}