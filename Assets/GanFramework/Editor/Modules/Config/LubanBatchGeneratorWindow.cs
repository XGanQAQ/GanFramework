// 仅在Unity编辑器环境下编译
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GanFramework.Editor.Config
{
    public class LubanBatchGeneratorWindow : EditorWindow
    {
        const string PrefKeyBatPath = "LubanEditor_BatPath";
        LubanBatchGenerator gen = new LubanBatchGenerator();
        string savePath = "gen.bat";

        [MenuItem("GanFramework/Luban Batch Generator")]
        public static void ShowWindow()
        {
            var w = GetWindow<LubanBatchGeneratorWindow>(false, "Luban Batch", true);
            w.minSize = new Vector2(520, 260);
        }

        void OnEnable()
        {
            // Load previously saved .bat path so Runner and Generator stay in sync
            savePath = EditorPrefs.GetString(PrefKeyBatPath, savePath);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Luban gen.bat generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            gen.workspace = EditorGUILayout.TextField("WORKSPACE", gen.workspace);
            gen.lubanDllRelative = EditorGUILayout.TextField("Luban DLL Relative", gen.lubanDllRelative);
            gen.confRoot = EditorGUILayout.TextField("CONF_ROOT", gen.confRoot);
            gen.target = EditorGUILayout.TextField("Target (-t)", gen.target);
            gen.config = EditorGUILayout.TextField("Config (-c)", gen.config);
            gen.dataFormat = EditorGUILayout.TextField("Data Format (-d)", gen.dataFormat);
            gen.confFile = EditorGUILayout.TextField("Conf File", gen.confFile);
            gen.outputCodeDir = EditorGUILayout.TextField("Output Code Dir", gen.outputCodeDir);
            gen.outputDataDir = EditorGUILayout.TextField("Output Data Dir", gen.outputDataDir);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(90)))
            {
                string start = Path.Combine(Application.dataPath, "../");
                string chosen = EditorUtility.SaveFilePanel("Save gen.bat", start, Path.GetFileName(savePath), "bat");
                if (!string.IsNullOrEmpty(chosen))
                {
                    savePath = chosen;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Preview", GUILayout.Width(140)))
            {
                var preview = gen.BuildBatchContent();
                EditorUtility.DisplayDialog("Preview", preview, "OK");
            }
            if (GUILayout.Button("Save .bat", GUILayout.Width(120)))
            {
                try
                {
                    gen.SaveToFile(savePath);
                    // Save chosen path so Runner can pick it up
                    EditorPrefs.SetString(PrefKeyBatPath, savePath ?? string.Empty);
                    EditorUtility.DisplayDialog("Saved", "Saved gen.bat to:\n" + savePath, "OK");
                    EditorUtility.RevealInFinder(savePath);
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Error", ex.Message, "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
// #if UNITY_EDITOR
// #endif
