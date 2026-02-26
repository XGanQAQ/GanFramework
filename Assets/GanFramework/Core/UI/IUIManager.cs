using UnityEngine;
namespace GanFramework.Core.UI
{
    public interface IUIManager
    {
        public bool IsShouldLockCursor();
        public bool IsLayerHasUIActive(UILayer layer);
        public void RegisterLayer(UILayerCanvas uiLayerCanvas);
        public void UnRegisterLayer(UILayerCanvas uiLayerCanvas);
        public T OpenUI<T>() where T : Component;
        public void CloseUI<T>() where T : Component;
    }
}