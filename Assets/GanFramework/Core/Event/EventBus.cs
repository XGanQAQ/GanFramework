using System;
using System.Collections.Generic;

namespace GanFramework.Core.Event
{
    /// <summary>
    /// 事件总线：用于解耦组件间的通信
    /// </summary>
    /// <remarks>
    /// 1. 支持多种事件类型，使用泛型 Action<T> 作为回调。
    /// 2. 支持订阅、取消订阅和触发事件。
    /// 3. 使用 Dictionary 存储事件类型和对应的多播委托。
    /// </remarks>{
    public static class EventBus
    {
        // 1. 存放所有事件：Key 是事件类型的 System.Type，Value 是合并后的多播委托
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        // 2. 订阅：把新回调拼到旧委托后面
        public static void Subscribe<T>(Action<T> callback)
        {
            var key = typeof(T);
            _handlers[key] = Delegate.Combine(_handlers.GetValueOrDefault(key), callback);
        }

        // 3. 取消订阅：从委托链里摘掉指定回调
        public static void Unsubscribe<T>(Action<T> callback)
        {
            var key = typeof(T);
            if (_handlers.TryGetValue(key, out var d))
            {
                var newD = Delegate.Remove(d, callback);
                if (newD == null) _handlers.Remove(key);
                else _handlers[key] = newD;
            }
        }

        // 4. 触发：按事件类型找到整条委托链并执行
        public static void Raise<T>(T evt)
        {
            if (_handlers.TryGetValue(typeof(T), out var d))
                (d as Action<T>)?.Invoke(evt);
        }

        // 5. 调试/重置用：清空全部
        public static void Clear() => _handlers.Clear();
    }
}

