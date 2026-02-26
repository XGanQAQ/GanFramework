using GanFramework.Core.EventBus;

namespace GanFramework.Core.UI
{
    public class CloseUIEvent : IEvent
    {
        public ViewerBase Viewer { get; set; }

        public CloseUIEvent(ViewerBase viewer)
        {
            Viewer = viewer;
        }
    }
    
    public class OpenUIEvent : IEvent
    {
        public ViewerBase Viewer { get; set; }

        public OpenUIEvent(ViewerBase viewer)
        {
            Viewer = viewer;
        }
    }
}