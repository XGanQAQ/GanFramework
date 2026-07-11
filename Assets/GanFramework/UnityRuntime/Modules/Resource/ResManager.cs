using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if ENABLE_ADDRESSABLES || ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using GanFramework.Core;

namespace GanFramework.UnityRuntime.Modules.Resource
{
    public class ResManager : IModules, IResManager
    {
        private static ResManager _instance;
        private readonly Dictionary<string, Object> _cache = new();

        public static ResManager Instance => _instance;

        public T Load<T>(string key, bool useResourcesOnly = false) where T : Object
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached as T;

            T asset = null;

            if (!useResourcesOnly)
            {
#if ENABLE_ADDRESSABLES || ADDRESSABLES
                var handle = Addressables.LoadAssetAsync<T>(key);
                asset = handle.WaitForCompletion();
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    asset = null;
                Addressables.Release(handle);
#endif
            }

            asset ??= Resources.Load<T>(key);

            if (asset != null)
                _cache[key] = asset;
            else
                Debug.LogWarning($"[ResManager] Asset not found: {key}");

            return asset;
        }

        public async UniTask<T> LoadAsync<T>(string key, bool useResourcesOnly = false) where T : Object
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached as T;

            T asset = null;

            if (!useResourcesOnly)
            {
#if ENABLE_ADDRESSABLES || ADDRESSABLES
                var handle = Addressables.LoadAssetAsync<T>(key);
                await handle.ToUniTask();
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    asset = handle.Result;
                Addressables.Release(handle);
#endif
            }

            asset ??= await Resources.LoadAsync<T>(key) as T;

            if (asset != null)
                _cache[key] = asset;
            else
                Debug.LogWarning($"[ResManager] Asset not found: {key}");

            return asset;
        }

        public T GetFromCache<T>(string key) where T : Object
        {
            if (_cache.TryGetValue(key, out var obj))
                return obj as T;
            return null;
        }

        public void OnInit()
        {
            _instance = this;
        }

        public void OnUpdate(float deltaTime) { }

        public void OnFixedUpdate(float fixedDeltaTime) { }

        public void OnLateUpdate(float deltaTime) { }

        public void OnDestroy()
        {
            _cache.Clear();
            _instance = null;
        }
    }
}