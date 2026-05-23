using System.Collections.Generic;
using UnityEngine;
using GanFramework.Core;

namespace GanFramework.UnityRuntime.Modules.Resource
{
    public class ResManager : IModules, IResManager
    {
        private static ResManager _instance;
        private readonly Dictionary<string, Object> _cache = new();

        public T Load<T>(string path) where T : Object
        {
            if (_cache.TryGetValue(path, out var cached))
                return cached as T;

            var asset = Resources.Load<T>(path);
            if (asset != null)
                _cache[path] = asset;
            else
                Debug.LogWarning($"Resource not found: {path}");
            return asset;
        }

        public T GetFromCache<T>(string path) where T : Object
        {
            if (_cache.TryGetValue(path, out var obj))
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