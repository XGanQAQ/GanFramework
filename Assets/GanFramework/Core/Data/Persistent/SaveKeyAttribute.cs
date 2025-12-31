using System;
namespace GanFramework.Core.Data.Persistent
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SaveKeyAttribute : Attribute
    {
        public string Key { get; }

        public SaveKeyAttribute(string key)
        {
            Key = key;
        }
    }
}