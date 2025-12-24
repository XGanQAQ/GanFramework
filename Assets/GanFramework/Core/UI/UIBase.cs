using System;
using UnityEngine;


namespace GanFramework.Core.UI
{
    /// <summary>
    /// Base class for all UI components.
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        public bool IsOpenOnFirstLoad;

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            if (!IsOpenOnFirstLoad) Hide();
        }
        /// <summary>
        /// Called when the UI is shown.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when the UI is hidden.
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the UI is initialized.
        /// </summary>
        public virtual void Initialize()
        {
            // Override in derived classes for initialization logic.
        }
    }
}