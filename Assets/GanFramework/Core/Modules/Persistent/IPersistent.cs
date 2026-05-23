using System;
using System.Threading.Tasks;

namespace GanFramework.Core.Data.Persistent
{
    public interface IPersistent
    {
        void Save<T>(T data, ISerializer serializer = null);
        T Load<T>(ISerializer serializer = null);
        T LoadInto<T>(T instance, ISerializer serializer = null);
        bool Exists<T>(ISerializer serializer = null);
        void Delete<T>(ISerializer serializer = null);
        void SaveMembers<T>(T instance, ISerializer serializer = null);
        T LoadMembers<T>(Func<T> factory = null, ISerializer serializer = null) where T : class;
        void LoadMembersInto<T>(T instance, ISerializer serializer = null);

        Task SaveAsync<T>(T data, ISerializer serializer = null);
        Task<T> LoadAsync<T>(ISerializer serializer = null);
        Task<T> LoadIntoAsync<T>(T instance, ISerializer serializer = null);
        Task SaveMembersAsync<T>(T instance, ISerializer serializer = null);
        Task<T> LoadMembersAsync<T>(Func<T> factory = null, ISerializer serializer = null) where T : class;
        Task LoadMembersIntoAsync<T>(T instance, ISerializer serializer = null);
    }
}
