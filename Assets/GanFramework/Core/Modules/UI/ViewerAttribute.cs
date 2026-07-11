using System;

namespace GanFramework.Modules.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ViewerAttribute : Attribute
    {
        public UILayer Layer { get; }
        public string AssetKey { get; }

        public ViewerAttribute(UILayer layer = UILayer.Normal, string assetKey = "")
        {
            Layer = layer;
            AssetKey = assetKey;
        }
    }
}
