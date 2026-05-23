using System;

namespace GanFramework.Core.EventBus
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : IEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IEvent;
        void Publish<T>(T eventData) where T : IEvent;
    }
}
