using System;
namespace GanFramework.Core.Data.Persistent
{
    // 标记一个类为可保存的，要求提供一个唯一的 Key 作为文件名的一部分。这个 Key 用于生成保存的文件名。
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