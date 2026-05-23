using System;
using GanFramework.Core;
using GanFramework.Core.EventBus;
using GanFramework.Core.Modules.UI;
using UnityEngine;

namespace GanFramework.Runtime.Modules.UI
{
    public abstract class ViewerBase : MonoBehaviour, IViewer
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
            Framework.GetModule<IEventBus>().Publish(new OpenUIEvent(this));
            UIManager.Current?.UpdateCursorState();
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
            Framework.GetModule<IEventBus>().Publish(new CloseUIEvent(this));
            UIManager.Current?.RecordInteractiveUIClose(this);
            UIManager.Current?.UpdateCursorState();
        }

        public virtual void Init()
        {
        }

        private void OnDestroy()
        {
        }
    }
}
