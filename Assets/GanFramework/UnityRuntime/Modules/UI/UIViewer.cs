using System;
using System.Reflection;
using UnityEngine;

namespace GanFramework.Core.UI
{
    public abstract class UIViewer : MonoBehaviour, IViewer
    {
        [SerializeField] private string viewerName;
        public virtual string UIName => string.IsNullOrEmpty(viewerName) ? GetType().Name : viewerName;
        public UILayer Layer { get; set; } = UILayer.Normal;

        public bool IsActive => gameObject.activeSelf;
        public bool CloseableByEscape { get; set; } = true;

        public event Action OnOpen;
        public event Action OnClose;

        public virtual void Open()
        {
            gameObject.SetActive(true);
            OnOpen?.Invoke();
            if (Framework.TryGetModule<IEventBus>(out var eventBus))
            {
                eventBus.Publish(new OpenUIEvent(this));
            }
            else
            {
                Debug.LogWarning($"[UI][ViewerBase] IEventBus not found in Open: {GetType().Name}");
            }
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
            if (Framework.TryGetModule<IEventBus>(out var eventBus))
            {
                eventBus.Publish(new CloseUIEvent(this));
            }
            else
            {
                Debug.LogWarning($"[UI][ViewerBase] IEventBus not found in Close: {GetType().Name}");
            }
        }

        public virtual void Init()
        {
        }
    }
}
