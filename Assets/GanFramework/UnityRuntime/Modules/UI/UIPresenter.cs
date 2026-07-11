using GanFramework.Core.UI;
using GanFramework.Core;
using System;
using UnityEngine;

public class UIPresenter : MonoBehaviour, IPresenter
{
    protected UIViewer autoViewer;

    protected virtual void Awake()
    {
        autoViewer = GetComponent<UIViewer>();
    }

    public virtual void Init()
    {
    }

    protected virtual void OnDestroy()
    {
        if (Framework.TryGetModule<IEventBus>(out var eventBus))
        {
            eventBus.UnsubscribeAll(this);
            return;
        }

        Debug.LogWarning($"[UI][PresenterBase] IEventBus not found in OnDestroy: {GetType().Name}");
    }

    public void SubscribeToEvent<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        if (Framework.TryGetModule<IEventBus>(out var eventBus))
        {
            eventBus.Subscribe(callback);
            return;
        }

        Debug.LogWarning($"[UI][PresenterBase] IEventBus not found in SubscribeToEvent: {GetType().Name}");
    }

    public void UnsubscribeFromEvent<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        if (Framework.TryGetModule<IEventBus>(out var eventBus))
        {
            eventBus.Unsubscribe(callback);
            return;
        }

        Debug.LogWarning($"[UI][PresenterBase] IEventBus not found in UnsubscribeFromEvent: {GetType().Name}");
    }

    public virtual void OnGetEvent(IEvent passedEvent)
    {
        Debug.Log($"[UI] OnGetEvent: {passedEvent}");
    }
}