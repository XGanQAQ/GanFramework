using System;
using GanFramework.Core.EventBus;
using GanFramework.Core.UI;
using UnityEngine;

namespace GanFramework.Runtime.UI
{
    /// <summary>
    /// Base class for all UI components.
    /// </summary>
    public abstract class ViewerBase : MonoBehaviour, IInitializable
    {
        [SerializeField] private string viewerName; // 可在 Inspector 覆写
        public virtual string UIName => string.IsNullOrEmpty(viewerName) ? GetType().Name : viewerName; // 暂时保留，后续改为使用ViewerAttribute指定
        public UILayer Layer = UILayer.Normal; // 暂时保留，后续改为使用ViewerAttribute指定
        public bool IsActive => gameObject.activeSelf;
        public bool CloseableByEscape { get; set; } = true;

        public event Action OnOpen;
        public event Action OnClose;

        /// <summary>
        /// Called when the UI is shown.
        /// </summary>
        public virtual void Open()
        {
            gameObject.SetActive(true);
            OnOpen?.Invoke();
            EventBus.Instance.Publish(new OpenUIEvent(this));
            UIManager.Current?.UpdateCursorState();
        }

        /// <summary>
        /// Called when the UI is hidden.
        /// </summary>
        public virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
            EventBus.Instance.Publish(new CloseUIEvent(this));
            UIManager.Current?.RecordInteractiveUIClose(this);
            UIManager.Current?.UpdateCursorState();
        }

        /// <summary>
        /// 当此Viewer被UILayerCanvas加载时调用，用于初始化逻辑。
        /// 常用于跨过Awake和Start，确保UIManager已经初始化。
        /// </summary>
        public virtual void Init()
        {

        }

        private void OnDestroy()
        {
            // Cleanup if necessary
        }
    }
}
