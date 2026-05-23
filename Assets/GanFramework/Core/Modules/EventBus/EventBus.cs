using System;
using System.Collections.Generic;

namespace GanFramework.Core.EventBus
{
    public class EventBus : IModules
    {
        private static EventBus _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventBus();
                    Framework.Register(_instance);
                }
                return _instance;
            }
        }

        private readonly Dictionary<Type, Delegate> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var newDelegate = Delegate.Remove(existing, handler);
                if (newDelegate == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = newDelegate;
            }
        }

        public void Publish<T>(T eventData) where T : IEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
                (handler as Action<T>)?.Invoke(eventData);
        }

        public void OnInit()
        {
            _instance = this;
        }

        public void OnUpdate(float deltaTime) { }
        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }

        public void OnDestroy()
        {
            _handlers.Clear();
            _instance = null;
        }
    }
}
