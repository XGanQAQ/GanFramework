using System;
using System.Collections.Generic;

namespace GanFramework.Core
{
    public class EventBus : IEventBus
    {
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

        // Unsubscribe all handlers associated with a specific subscriber object.
        public void UnsubscribeAll(object subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            var eventTypes = new List<Type>(_handlers.Keys);
            foreach (var eventType in eventTypes)
            {
                var existing = _handlers[eventType];
                Delegate rebuilt = null;

                foreach (var invocation in existing.GetInvocationList())
                {
                    if (invocation.Target == subscriber)
                        continue;

                    rebuilt = rebuilt == null
                        ? invocation
                        : Delegate.Combine(rebuilt, invocation);
                }

                if (rebuilt == null)
                    _handlers.Remove(eventType);
                else
                    _handlers[eventType] = rebuilt;
            }
        }

    }
}
