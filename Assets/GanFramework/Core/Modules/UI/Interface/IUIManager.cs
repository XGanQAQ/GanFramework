using System.Collections.Generic;

namespace GanFramework.Core.UI
{
    public interface IUIManager
    {
        bool IsLayerHasUIActive(UILayer layer);
        bool IsUIActive<T>() where T : class, IViewer;
        IViewer OpenUI(string viewerName, bool show = true);
        void CloseUI(string viewerName);
        IEnumerable<KeyValuePair<string, IViewer>> GetLayerViewers(UILayer layer);
    }
}
