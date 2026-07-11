using System;
using System.Threading.Tasks;

namespace GanFramework.Core.Persistent
{
    public interface IPersistentManager
    {
        void Save<T>(T data, ISerializer serializer = null);
        T Load<T>(ISerializer serializer = null);

        Task SaveAsync<T>(T data, ISerializer serializer = null);
        Task<T> LoadAsync<T>(ISerializer serializer = null);
    }
}
