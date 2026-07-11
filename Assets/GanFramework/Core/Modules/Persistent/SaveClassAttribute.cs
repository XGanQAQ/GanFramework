using System;
namespace GanFramework.Core.Persistent
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SaveClassAttribute : Attribute
    {
        public string Key { get; }

        public SaveClassAttribute(string key)
        {
            Key = key;
        }
    }
}
