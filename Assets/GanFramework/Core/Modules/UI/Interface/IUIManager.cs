namespace GanFramework.Core.UI
{
    public interface IUIManager
    {
        bool IsShouldLockCursor();
        bool IsLayerHasUIActive(UILayer layer);
        T OpenUI<T>(bool show = true) where T : class, IViewer;
        IViewer OpenUI(string viewerName, bool show = true);
        void CloseUI<T>() where T : class, IViewer;
        void UpdateCursorState();
        void RecordInteractiveUIClose(IViewer viewer);
    }
}
