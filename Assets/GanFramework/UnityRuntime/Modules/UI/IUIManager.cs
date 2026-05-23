using GanFramework.Core.UI;

namespace GanFramework.Runtime.UI
{
    public interface IUIManager
    {
        public bool IsShouldLockCursor();
        public bool IsLayerHasUIActive(UILayer layer);
        public T OpenUI<T>(bool show = true) where T : ViewerBase;
        public ViewerBase OpenUI(string viewerName, bool show = true);
        public void CloseUI<T>() where T : ViewerBase;
    }
}
