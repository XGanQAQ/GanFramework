using System;

namespace GanFramework.Core.Persistent
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class SaveMemberAttribute : Attribute
    {
        public string Key { get; }

        public SaveMemberAttribute(string key = null)
        {
            Key = key;
        }
    }
}

