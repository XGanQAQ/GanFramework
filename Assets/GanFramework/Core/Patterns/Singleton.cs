using System;
using System.Reflection;

namespace GanFramework.Core.Patterns
{
    /// <summary>
    /// 不继承Mono的单例模式基类 继承该基类的类可实现单例模式 但要求有私有的无参构造函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class
    {
        private static T instance;
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly object lockObj = new();

        public static bool HasInstance => instance != null;

        public static T TryGetInstance()
        {
            return instance;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            Type type = typeof(T);
                            ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                                Type.EmptyTypes, null);
                            if (info != null)
                            {
                                instance = info.Invoke(null) as T;
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"类型 {typeof(T).Name} 没有显式实现私有无参构造函数");
                            }
                        }
                    }
                }
                return instance;
            }
        }
    }
}
