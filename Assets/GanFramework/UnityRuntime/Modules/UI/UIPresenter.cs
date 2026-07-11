using GanFramework.Core.UI;
using GanFramework.Core;
using System;
using UnityEngine;

public class UIPresenter : MonoBehaviour, IPresenter
{
    // This is a reference to the ViewerBase component attached to the same GameObject as this PresenterBase. 
    // It is used to automatically link the presenter with its corresponding viewer.
    // It may be **null** if the ViewerBase component is not attached to the same GameObject, so it is important to check for null before using it.
    protected UIViewer autoViewer;

    protected virtual void Awake()
    {
        autoViewer = GetComponent<UIViewer>();
    }

    // Init是在被框架调用时执行的初始化方法，通常用于设置Presenter的初始状态或绑定事件。
    public virtual void Init()
    {
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe from all events when the presenter is destroyed to prevent memory leaks.
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