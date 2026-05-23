using System;

namespace GanFramework.Core.Modules.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ViewerAttribute : Attribute
    {
        public UILayer Layer { get; }
        public string ResourcePath { get; }

        public ViewerAttribute(UILayer layer = UILayer.Normal, string resourcePath = "UI")
        {
            Layer = layer;
            ResourcePath = resourcePath;
        }
    }
}
