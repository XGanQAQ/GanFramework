using GanFramework.Core.EventBus;

namespace GanFramework.Core.Modules.UI
{
    public class CloseUIEvent : IEvent
    {
        public IViewer Viewer { get; set; }

        public CloseUIEvent(IViewer viewer)
        {
            Viewer = viewer;
        }
    }

    public class OpenUIEvent : IEvent
    {
        public IViewer Viewer { get; set; }

        public OpenUIEvent(IViewer viewer)
        {
            Viewer = viewer;
        }
    }
}
