#if UNITY_EDITOR
using UnityEditor;
using GanFramework.Core.Runtime.Environments;
namespace GanFramework.Editor
{
    [InitializeOnLoad]
    public static class EnvironmentMacroSync
    {
        static EnvironmentMacroSync()
        {
            SyncAddressablesMacro();
        }

        public static void SyncAddressablesMacro()
        {
            var envState = Environment.State;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            if (envState.EnableAddressables && !symbols.Contains("ENABLE_ADDRESSABLES"))
            {
                symbols += (symbols.Length > 0 ? ";" : "") + "ENABLE_ADDRESSABLES";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
            }
            else if (!envState.EnableAddressables && symbols.Contains("ENABLE_ADDRESSABLES"))
            {
                symbols = symbols.Replace("ENABLE_ADDRESSABLES;", "").Replace("ENABLE_ADDRESSABLES", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
            }
        }
    }
}
#endif