using System;
using GanFramework.Core.EventBus;
using UnityEngine;

namespace GanFramework.Core.UI
{
    /// <summary>
    /// Base class for all UI components.
    /// </summary>
    public abstract class ViewerBase : MonoBehaviour, IInitializable
    {
        [SerializeField] private string viewerName; // 可在 Inspector 覆写
        public virtual string UIName => string.IsNullOrEmpty(viewerName) ? GetType().Name : viewerName;
        public bool IsOpenOnFirstLoad;
        public UILayer Layer = UILayer.Normal; // Default layer
        public bool IsActive => gameObject.activeSelf;

        /// <summary>
        /// Called when the UI is shown.
        /// </summary>
        public virtual void Open()
        {
            gameObject.SetActive(true);
            EventBus<OpenUIEvent>.Raise(new OpenUIEvent(this));
        }

        /// <summary>
        /// Called when the UI is hidden.
        /// </summary>
        public virtual void Close()
        {
            gameObject.SetActive(false);
            EventBus<CloseUIEvent>.Raise(new CloseUIEvent(this));
        }

        /// <summary>
        /// 当此Viewer被UILayerCanvas加载时调用，用于初始化逻辑。
        /// 常用于跨过Awake和Start，确保UIManager已经初始化。
        /// </summary>
        public virtual void Init()
        {
            if (!IsOpenOnFirstLoad) Close();
        }
        
        private void OnDestroy()
        {
            // Cleanup if necessary
        }
    }
}