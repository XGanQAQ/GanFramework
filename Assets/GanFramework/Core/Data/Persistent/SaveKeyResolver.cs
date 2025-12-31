using System;
using System.Reflection;

namespace GanFramework.Core.Data.Persistent
{
    // 利用反射获得SaveKey
    public static class SaveKeyResolver
    {
        public static string GetSaveKey(Type type)
        {
            var attr = type.GetCustomAttribute<SaveKeyAttribute>();
            if (attr == null)
            {
                throw new InvalidOperationException(
                    $"类型 {type.Name} 未标记 [SaveKey]"
                );
            }

            return attr.Key;
        }

        public static string GetSaveKey<T>()
        {
            return GetSaveKey(typeof(T));
        }

        public static string GetSaveKey(object obj)
        {
            return GetSaveKey(obj.GetType());
        }
    }
}