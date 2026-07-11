namespace GanFramework.Core.UI
{
    public interface IUIManager
    {
        bool IsLayerHasUIActive(UILayer layer);

        T OpenUI<T>(bool show = true) where T : class, IViewer;
        IViewer OpenUI(string viewerName, bool show = true);
        void CloseUI<T>() where T : class, IViewer;
        
        void RecordInteractiveUIClose(IViewer viewer);
    }
}
