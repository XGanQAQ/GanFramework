using System;
namespace GanFramework.Core.Data.Persistent
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