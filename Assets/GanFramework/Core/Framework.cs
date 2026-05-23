using System;
using System.Collections.Generic;

namespace GanFramework.Core
{
    public static class Framework
    {
        private static readonly List<IModules> modules = new();
        private static bool initialized = false;

        public static void Register(IModules module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            if (modules.Contains(module))
                return;

            modules.Add(module);

            if (initialized)
                module.OnInit();
        }

        public static void Unregister<T>() where T : IModules
        {
            for (int i = modules.Count - 1; i >= 0; i--)
            {
                if (modules[i] is T)
                {
                    modules[i].OnDestroy();
                    modules.RemoveAt(i);
                }
            }
        }

        public static void Unregister(IModules module)
        {
            if (modules.Remove(module))
                module.OnDestroy();
        }

        public static T GetModule<T>() where T : class
        {
            foreach (var m in modules)
                if (m is T t) return t;
            return null;
        }

        public static bool TryGetModule<T>(out T module) where T : class
        {
            foreach (var m in modules)
            {
                if (m is T t)
                {
                    module = t;
                    return true;
                }
            }
            module = null;
            return false;
        }

        public static T[] GetAllModules<T>() where T : class
        {
            var result = new List<T>();
            foreach (var m in modules)
                if (m is T t) result.Add(t);
            return result.ToArray();
        }

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            foreach (var m in modules)
                m.OnInit();
        }

        public static void Update(float deltaTime)
        {
            foreach (var m in modules)
                m.OnUpdate(deltaTime);
        }

        public static void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var m in modules)
                m.OnFixedUpdate(fixedDeltaTime);
        }

        public static void LateUpdate(float deltaTime)
        {
            foreach (var m in modules)
                m.OnLateUpdate(deltaTime);
        }

        public static void Shutdown()
        {
            for (int i = modules.Count - 1; i >= 0; i--)
                modules[i].OnDestroy();
            modules.Clear();
            initialized = false;
        }
    }
}
