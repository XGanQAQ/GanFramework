using System;
using GanFramework.Core.EventBus;
using UnityEngine;

namespace GanFramework.Core.UI
{
    public abstract class PresenterBase<T> : MonoBehaviour, IInitializable where T : IEvent 
    {
        private EventBinding<T> eventBinding;
        
        // 当此Presenter被UILayerCanvas加载时调用，用于初始化逻辑。
        public virtual void Init()
        {
            eventBinding = new EventBinding<T>(OnGetEvent);
            EventBus<T>.Register(eventBinding);
        }

        /// <summary>
        /// 当接收到T事件时调用此方法。
        /// </summary>
        /// <param name="passedEvent">传递过来的事件参数</param>
        protected virtual void OnGetEvent(T passedEvent)
        {
            // To be implemented by derived classes
            Debug.Log($"[UI] OnGetEvent: {passedEvent}");
        }

        protected virtual void OnDestroy()
        {
            if (eventBinding != null)
            {
                EventBus<T>.Deregister(eventBinding);
                eventBinding = null;
            }
        }
    }
}