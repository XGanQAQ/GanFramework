using System;

namespace GanFramework.Core.UI
{
    public interface IViewer: IInitializable
    {
        string UIName { get; }
        UILayer Layer { get; }
        string AssetKey { get; }
        bool IsActive { get; }
        bool CloseableByEscape { get; set; }

        event Action OnOpen;
        event Action OnClose;

        void Open();
        void Close();
    }
}
