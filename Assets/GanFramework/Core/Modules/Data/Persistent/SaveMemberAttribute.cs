// ...new file...
using System;

namespace GanFramework.Core.Data.Persistent
{
    // 标记一个成员（字段或属性）为可保存的。可以选择提供一个 Key 来指定在保存数据中的名称，如果不提供则使用成员名。
    // 这个属性用于标记需要被序列化的成员。
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

