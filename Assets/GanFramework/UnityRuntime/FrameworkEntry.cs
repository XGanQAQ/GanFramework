using UnityEngine;
using GanFramework.Core;

namespace GanFramework.Runtime
{
    public class FrameworkEntry : MonoBehaviour
    {
        private static bool initialized = false;
        private static FrameworkEntry instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (initialized) return;
            initialized = true;

            var go = new GameObject("[FrameworkEntry]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FrameworkEntry>();

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
            Framework.Shutdown();
            initialized = false;
            instance = null;
        }
    }
}
