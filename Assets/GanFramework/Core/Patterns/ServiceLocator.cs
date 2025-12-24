using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Locator 基类，支持父级查找机制。
    /// </summary>
    public abstract class ServiceLocatorBase
    {
        protected readonly Dictionary<Type, object> _services = new();

        /// <summary>父 Locator，用于层级查找。</summary>
        public ServiceLocatorBase Parent { get; set; }

        /// <summary>
        /// 注册一个服务实例。
        /// </summary>
        public virtual void Register<T>(T service)
        {
            _services[typeof(T)] = service;
            OnServiceRegistered?.Invoke(typeof(T), service);
        }

        /// <summary>
        /// 注销一个服务。
        /// </summary>
        public virtual void UnRegister<T>(T service)
        {
            if (service == null)
            {
                _services.Remove(typeof(T));
                return;
            }
            
            if (_services.TryGetValue(typeof(T), out var instance) && instance.Equals(service))
                _services.Remove(typeof(T));
        }

        /// <summary>
        /// 解析服务（支持向父级递归查找）。
        /// </summary>
        public virtual T Resolve<T>()
        {
            if (_services.TryGetValue(typeof(T), out var instance))
                return (T)instance;

            return Parent != null ? Parent.Resolve<T>() : default;
        }

        /// <summary>清空所有服务。</summary>
        public virtual void Clear() => _services.Clear();

        /// <summary>服务注册事件：当某个服务注册时触发。</summary>
        public event Action<Type, object> OnServiceRegistered;

        /// <summary>
        /// 尝试立即获取服务。
        /// </summary>
        public virtual bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var instance))
            {
                service = (T)instance;
                return true;
            }
            else if (Parent != null)
            {
                return Parent.TryGet(out service);
            }

            service = default;
            return false;
        }

        /// <summary>
        /// 尝试获取服务；若不存在，则等待其注册。
        /// </summary>
        public virtual bool TryGetWait<T>(Action<T> onGet)
        {
            if (TryGet<T>(out var service))
            {
                onGet?.Invoke(service);
                return true;
            }

            void Handler(Type type, object instance)
            {
                if (type == typeof(T))
                {
                    onGet?.Invoke((T)instance);
                    OnServiceRegistered -= Handler;
                }
            }

            OnServiceRegistered += Handler;
            return false;
        }
    }

    /// <summary>
    /// 泛型单例版本的 Locator。
    /// </summary>
    public abstract class ServiceLocator<T> : ServiceLocatorBase where T : ServiceLocator<T>, new()
    {
        private static readonly Lazy<T> _instance = new(() => new T());

        /// <summary>全局访问入口。</summary>
        public static T Instance => _instance.Value;
    }
}