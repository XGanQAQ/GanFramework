using System;
using System.Reflection;
using GanFramework.Core;
using GanFramework.Core.UI;
using UnityEngine;

namespace GanFramework.UnityRuntime.UI
{
    public abstract class ViewerBase : MonoBehaviour, IViewer
    {
        [SerializeField] private string viewerName;
        public virtual string UIName => string.IsNullOrEmpty(viewerName) ? GetType().Name : viewerName;
        public UILayer Layer { get; set; } = UILayer.Normal;

        private string _assetKey;
        public string AssetKey
        {
            get
            {
                if (_assetKey == null)
                    ResolveAttribute();
                return _assetKey;
            }
        }

        public bool IsActive => gameObject.activeSelf;
        public bool CloseableByEscape { get; set; } = true;

        public event Action OnOpen;
        public event Action OnClose;

        protected virtual void Awake()
        {
            ResolveAttribute();
        }

        /// <summary>
        /// 读取 [Viewer] 特性，填充 Layer 和 AssetKey
        /// </summary>
        private void ResolveAttribute()
        {
            var attr = GetType().GetCustomAttribute<ViewerAttribute>();
            if (attr != null)
            {
                Layer = attr.Layer;
                _assetKey = attr.AssetKey ?? "";
            }
            else
            {
                _assetKey = "";
            }
        }

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
            UIManager.Instance?.UpdateCursorState();
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
            UIManager.Instance?.RecordInteractiveUIClose(this);
            UIManager.Instance?.UpdateCursorState();
        }

        public virtual void Init()
        {
        }

        private void OnDestroy()
        {
        }
    }
}
