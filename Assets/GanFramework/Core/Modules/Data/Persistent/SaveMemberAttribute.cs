// ...new file...
using System;

namespace GanFramework.Core.Data.Persistent
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class SaveMemberAttribute : Attribute
    {
        /// <summary>
        /// Optional name to use in the saved JSON. If null, the member name is used.
        /// </summary>
        public string Key { get; }

        public SaveMemberAttribute(string key = null)
        {
            Key = key;
        }
    }
}

