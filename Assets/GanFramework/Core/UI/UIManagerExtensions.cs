using System.Linq;
using UnityEngine;

namespace GanFramework.Core.UI
{
    // UIManager的扩展方法类，提供一些便捷的方法来操作UI
    public static class UIManagerExtensions
    {
        // 来回切换UI状态，按同一个键实现UI的开关
        // 用来注册来回切换UI开关事件
        public static void SwitchUI<T>(this UIManager manager) where T : ViewerBase
        {
            if (manager == null) return;

            if (manager.IsUIActive<T>())
            {
                manager.CloseUI<T>();
            }
            else
            {
                manager.OpenUI<T>();
            }
        }

        // 关闭某个层级下的所有UI
         public static void CloseLayerUI(this UIManager manager ,UILayer layer)
        {
            var layerRoot = manager.layerRoots.FirstOrDefault(lr => lr.layer == layer);
            if (layerRoot == null) return;
            foreach (var uiBase in layerRoot.UIBasesDic.Values)
            {
                uiBase.Close();
            }

            manager.UpdateCursorState();
        }
    }
}