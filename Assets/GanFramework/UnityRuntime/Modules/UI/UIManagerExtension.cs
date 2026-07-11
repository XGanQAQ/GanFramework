using System.Collections;
using GanFramework.Core;
using GanFramework.Core.UI;

namespace GanFramework.UnityRuntime.UI
{
    public static class IUIManagerExtension
    {
        public static T OpenUI<T>(this IUIManager uiManager, bool show = true) where T : class, IViewer
        {
            return uiManager.OpenUI(typeof(T).Name, show) as T;
        }

        public static void CloseUI<T>(this IUIManager uiManager) where T : class, IViewer
        {
            string uiName = typeof(T).Name;
            uiManager.CloseUI(uiName);
        }

        public static void SwitchUI<T>(this IUIManager uiManager) where T : class, IViewer
        {
            if (uiManager.IsUIActive<T>())
                uiManager.CloseUI<T>();
            else
                uiManager.OpenUI<T>();
        }

        public static void CloseLayerUI(this IUIManager uiManager, UILayer layer)
        {
            uiManager.CloseLayerUI(layer);
        }

        public static bool TryCloseLayerUIByEscape(this IUIManager uiManager, UILayer layer)
        {
            return uiManager.TryCloseLayerUIByEscape(layer);
        }
    }
}