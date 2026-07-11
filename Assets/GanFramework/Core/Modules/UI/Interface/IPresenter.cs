using System;
namespace GanFramework.Core.UI
{
    public interface IPresenter : IInitializable
    {
        void SubscribeToEvent<TEvent>(Action<TEvent> callback) where TEvent : IEvent;
        void UnsubscribeFromEvent<TEvent>(Action<TEvent> callback) where TEvent : IEvent;
        void OnGetEvent(IEvent passedEvent);
    }
}
