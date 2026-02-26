#if UNITY_EDITOR
using UnityEditor;

namespace GanFramework.Editor
{
    public static class EnvironmentMacroSyncMenu
    {
        [MenuItem("GanFramework/刷新 Addressables 宏定义")]
        public static void RefreshMacro()
        {
            EnvironmentMacroSync.SyncAddressablesMacro();
            UnityEngine.Debug.Log("已刷新 ENABLE_ADDRESSABLES 宏定义");
        }
    }
}
#endif