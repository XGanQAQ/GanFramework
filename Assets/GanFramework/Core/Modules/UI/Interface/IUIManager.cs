namespace GanFramework.Core.UI
{
    public interface IUIManager
    {
        bool IsLayerHasUIActive(UILayer layer);
        bool IsUIActive<T>() where T : class, IViewer;
        IViewer OpenUI(string viewerName, bool show = true);
        void CloseUI(string viewerName);
        void CloseLayerUI(UILayer layer);
        bool TryCloseLayerUIByEscape(UILayer layer);
    }
}
