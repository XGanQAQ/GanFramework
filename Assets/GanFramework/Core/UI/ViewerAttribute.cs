using System;

namespace GanFramework.Core.UI
{
    // 用于标记Viewer的属性，方便UIManager自动注册和加载
    public class ViwerAttribute : Attribute
    {
        public UILayer Layer { get; }
        public string ResourcePath { get; }

        public ViwerAttribute(UILayer layer = UILayer.Normal, string resourcePath = "UI")
        {
            Layer = layer;
            ResourcePath = resourcePath;
        }
    }
}
