using UnityEngine;
using GanFramework.Core;
using GanFramework.Core.UI;
using GanFramework.UnityRuntime.Persistent;
using GanFramework.UnityRuntime.UI;
using System.Collections.Generic;


namespace GanFramework.UnityRuntime
{
    public class FrameworkUnityEntry : MonoBehaviour
    {
        private static FrameworkUnityEntry instance;
        public static FrameworkUnityEntry Instance => instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (instance != null) return;

            var go = new GameObject("[FrameworkEntry]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FrameworkUnityEntry>();

            // Register modules
            Framework.Register(new PersistentService());
            var uiManager = new UnityUIManager(new HashSet<UILayer>() { UILayer.Popup, UILayer.Top });
            Framework.Register(uiManager);

            Framework.Init();
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
