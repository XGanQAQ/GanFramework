using GanFramework.Core.EventBus;
using GanFramework.Core.Modules.UI;
using UnityEngine;

namespace GanFramework.Runtime.Modules.UI
{
    public abstract class PresenterBase<T> : MonoBehaviour, IPresenter where T : IEvent
    {
        public virtual void Init()
        {
            EventBus.Instance.Subscribe<T>(OnGetEvent);
        }

        protected virtual void OnGetEvent(T passedEvent)
        {
            Debug.Log($"[UI] OnGetEvent: {passedEvent}");
        }

        protected virtual void OnDestroy()
        {
            EventBus.Instance.Unsubscribe<T>(OnGetEvent);
        }
    }
}
