using UnityEngine;
namespace GanFramework.Core.Runtime
{
    public static class FrameworkEntry
    {
        private static bool initialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeInit()
        {
            if (initialized) return;
            initialized = true;

            RegisterModules();
            Framework.Init();
        }

        private static void RegisterModules()
        {
            // Framework.Register(new EventBusModule());
            // Framework.Register(new ResLoadModule());
            // Framework.Register(new ConfigModule());
            // Framework.Register(new DataModule());
        }
    }
}

