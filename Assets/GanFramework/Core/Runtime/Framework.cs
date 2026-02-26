using System.Collections.Generic;
using UnityEngine;
namespace GanFramework.Core.Runtime
{
    public static class Framework
    {
        private static readonly List<IFrameworkModule> modules = new();

        public static void Register(IFrameworkModule module)
        {
            modules.Add(module);
        }

        public static void Init()
        {
            foreach (var m in modules)
                m.Init();
        }

        public static void Shutdown()
        {
            for (int i = modules.Count - 1; i >= 0; i--)
                modules[i].Shutdown();
        }
    }
}