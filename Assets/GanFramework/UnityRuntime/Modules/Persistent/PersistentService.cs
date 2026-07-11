using System;
using System.Threading.Tasks;
using GanFramework.Core;
using GanFramework.Core.Data.Persistent;
using GanFramework.Odin.OdinSerializer;

namespace GanFramework.UnityRuntime.Persistent
{
    public enum SaveFormat
    {
        OdinBinary,
        OdinJson,
    }

    public class PersistentService : IPersistent, IModules
    {
        private static ISerializer GetSerializerForFormat(SaveFormat format)
        {
            return format switch
            {
                SaveFormat.OdinBinary => new OdinSerializer(DataFormat.Binary),
                SaveFormat.OdinJson => new OdinSerializer(DataFormat.JSON),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            };
        }

        public void OnInit() { }
        public void OnUpdate(float deltaTime) { }
        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }
        public void OnDestroy() { }

        // IPersistent implementation (ISerializer-based)

        public void Save<T>(T data, ISerializer serializer = null) =>
            SaveStore.Save(data, serializer);

        public T Load<T>(ISerializer serializer = null) =>
            SaveStore.Load<T>(serializer);

        public T LoadInto<T>(T instance, ISerializer serializer = null) =>
            SaveStore.LoadInto(instance, serializer);

        public bool Exists<T>(ISerializer serializer = null) =>
            SaveStore.Exists<T>(serializer);

        public void Delete<T>(ISerializer serializer = null) =>
            SaveStore.Delete<T>(serializer);

        public void SaveMembers<T>(T instance, ISerializer serializer = null) =>
            SaveStore.SaveMembers(instance, serializer);

        public T LoadMembers<T>(Func<T> factory = null, ISerializer serializer = null) where T : class =>
            SaveStore.LoadMembers(factory, serializer);

        public void LoadMembersInto<T>(T instance, ISerializer serializer = null) =>
            SaveStore.LoadMembersInto(instance, serializer);

        public Task SaveAsync<T>(T data, ISerializer serializer = null) =>
            AsyncSaveStore.SaveAsync(data, serializer);

        public Task<T> LoadAsync<T>(ISerializer serializer = null) =>
            AsyncSaveStore.LoadAsync<T>(serializer);

        public Task<T> LoadIntoAsync<T>(T instance, ISerializer serializer = null) =>
            AsyncSaveStore.LoadIntoAsync(instance, serializer);

        public Task SaveMembersAsync<T>(T instance, ISerializer serializer = null) =>
            AsyncSaveStore.SaveMembersAsync(instance, serializer);

        public Task<T> LoadMembersAsync<T>(Func<T> factory = null, ISerializer serializer = null) where T : class =>
            AsyncSaveStore.LoadMembersAsync(factory, serializer);

        public Task LoadMembersIntoAsync<T>(T instance, ISerializer serializer = null) =>
            AsyncSaveStore.LoadMembersIntoAsync(instance, serializer);

        // SaveFormat convenience overloads

        public void Save<T>(T data, SaveFormat format) =>
            SaveStore.Save(data, GetSerializerForFormat(format));

        public void Save<T>(T data, SaveFormat format, ISerializer serializer) =>
            SaveStore.Save(data, serializer ?? GetSerializerForFormat(format));

        public T Load<T>(SaveFormat format) =>
            SaveStore.Load<T>(GetSerializerForFormat(format));

        public T Load<T>(SaveFormat format, ISerializer serializer) =>
            SaveStore.Load<T>(serializer ?? GetSerializerForFormat(format));

        public bool Exists<T>(SaveFormat format = SaveFormat.OdinJson) =>
            SaveStore.Exists<T>(GetSerializerForFormat(format));

        public void Delete<T>(SaveFormat format = SaveFormat.OdinJson) =>
            SaveStore.Delete<T>(GetSerializerForFormat(format));

        public void SaveMembers<T>(T instance, SaveFormat format) =>
            SaveStore.SaveMembers(instance, GetSerializerForFormat(format));

        public void SaveMembers<T>(T instance, SaveFormat format, ISerializer serializer) =>
            SaveStore.SaveMembers(instance, serializer ?? GetSerializerForFormat(format));

        public T LoadMembers<T>(SaveFormat format, Func<T> factory = null) where T : class =>
            SaveStore.LoadMembers(factory, GetSerializerForFormat(format));

        public T LoadMembers<T>(Func<T> factory, SaveFormat format, ISerializer serializer) where T : class =>
            SaveStore.LoadMembers(factory, serializer ?? GetSerializerForFormat(format));

        public void LoadMembersInto<T>(T instance, SaveFormat format) =>
            SaveStore.LoadMembersInto(instance, GetSerializerForFormat(format));

        public void LoadMembersInto<T>(T instance, SaveFormat format, ISerializer serializer) =>
            SaveStore.LoadMembersInto(instance, serializer ?? GetSerializerForFormat(format));

        public Task SaveAsync<T>(T data, SaveFormat format) =>
            AsyncSaveStore.SaveAsync(data, GetSerializerForFormat(format));

        public Task SaveAsync<T>(T data, SaveFormat format, ISerializer serializer) =>
            AsyncSaveStore.SaveAsync(data, serializer ?? GetSerializerForFormat(format));

        public Task<T> LoadAsync<T>(SaveFormat format) =>
            AsyncSaveStore.LoadAsync<T>(GetSerializerForFormat(format));

        public Task<T> LoadAsync<T>(SaveFormat format, ISerializer serializer) =>
            AsyncSaveStore.LoadAsync<T>(serializer ?? GetSerializerForFormat(format));

        public Task SaveMembersAsync<T>(T instance, SaveFormat format) =>
            AsyncSaveStore.SaveMembersAsync(instance, GetSerializerForFormat(format));

        public Task SaveMembersAsync<T>(T instance, SaveFormat format, ISerializer serializer) =>
            AsyncSaveStore.SaveMembersAsync(instance, serializer ?? GetSerializerForFormat(format));

        public Task<T> LoadMembersAsync<T>(SaveFormat format, Func<T> factory = null) where T : class =>
            AsyncSaveStore.LoadMembersAsync(factory, GetSerializerForFormat(format));

        public Task<T> LoadMembersAsync<T>(Func<T> factory, SaveFormat format, ISerializer serializer) where T : class =>
            AsyncSaveStore.LoadMembersAsync(factory, serializer ?? GetSerializerForFormat(format));

        public Task LoadMembersIntoAsync<T>(T instance, SaveFormat format) =>
            AsyncSaveStore.LoadMembersIntoAsync(instance, GetSerializerForFormat(format));

        public Task LoadMembersIntoAsync<T>(T instance, SaveFormat format, ISerializer serializer) =>
            AsyncSaveStore.LoadMembersIntoAsync(instance, serializer ?? GetSerializerForFormat(format));
    }
}
