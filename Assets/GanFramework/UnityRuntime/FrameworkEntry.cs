using UnityEngine;
using GanFramework.Core;
using GanFramework.Core.EventBus;
using GanFramework.Runtime.Data.Persistent;
using GanFramework.UnityRuntime.Modules.Resource;


namespace GanFramework.Runtime
{
    public class FrameworkEntry : MonoBehaviour
    {
        private static FrameworkEntry instance;

        public static FrameworkEntry Instance => instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (instance != null) return;

            var go = new GameObject("[FrameworkEntry]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FrameworkEntry>();

            RegisterBuiltinModules();
            Framework.Init();
        }

        private static void RegisterBuiltinModules()
        {
            Framework.Register(new EventBus());
            Framework.Register(new PersistentService());
            Framework.Register(new ResManager());
        }

        private void Update()
        {
            Framework.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Framework.FixedUpdate(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            Framework.LateUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            Framework.Shutdown();
            instance = null;
        }
    }
}
