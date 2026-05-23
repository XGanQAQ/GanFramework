using UnityEngine;
using GanFramework.Core;
using GanFramework.Runtime.Patterns;

namespace GanFramework.Runtime
{
    public class FrameworkEntry : GlobalMonoSingleton<FrameworkEntry>
    {
        private static bool initialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (initialized) return;
            initialized = true;

            var go = new GameObject("[FrameworkEntry]");
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Framework.Shutdown();
            initialized = false;
        }
    }
}
