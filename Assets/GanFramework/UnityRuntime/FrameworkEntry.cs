using UnityEngine;
using GanFramework.Core;
using GanFramework.Core.UI;
using GanFramework.UnityRuntime.Persistent;
using GanFramework.UnityRuntime.UI;
using GanFramework.UnityRuntime.Resource;
using System.Collections.Generic;


namespace GanFramework.UnityRuntime
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
            Framework.Register(new PersistentService());
            Framework.Register(new ResManager());
            
            var uiManager = new UIManager(new HashSet<UILayer>() { UILayer.Popup, UILayer.Top });
            Framework.Register(uiManager);
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
