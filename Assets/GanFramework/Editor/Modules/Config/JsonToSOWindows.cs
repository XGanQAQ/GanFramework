#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GanFramework.Core.Data.Config
{
    public class JsonToSOWindows : EditorWindow
    {
        UnityEngine.Object sourceFolder;
        TextAsset singleJsonFile;
        UnityEngine.Object outputFolder;

        Dictionary<string, Type> soTypes = new Dictionary<string, Type>();
        string[] soTypeKeys = new string[0];
        int selectedTypeIndex = 0;
        string assemblyFolderPath = string.Empty;

        bool useFolder = true;

        [MenuItem("GanFramework/Config/Config ->SO Converter")]
        static void OpenWindow()
        {
            var w = GetWindow<JsonToSOWindows>("JSON -> SO");
            w.minSize = new Vector2(480, 200);
            w.RefreshTypes();
        }

        void OnEnable()
        {
            RefreshTypes();
        }

        void RefreshTypes()
        {
            soTypes = JsonToSOTransformer.GetAllSOTypes(string.IsNullOrWhiteSpace(assemblyFolderPath) ? null : assemblyFolderPath);
            soTypeKeys = soTypes.Keys.OrderBy(k => k).ToArray();
            if (soTypeKeys.Length == 0)
            {
                soTypeKeys = new[] { "<No SO types found>" };
                selectedTypeIndex = 0;
            }
            else
            {
                selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, soTypeKeys.Length - 1);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Json -> ScriptableObject 批量转换", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            useFolder = EditorGUILayout.ToggleLeft("Use folder (batch)", useFolder);

            EditorGUI.indentLevel++;
            if (useFolder)
            {
                sourceFolder = EditorGUILayout.ObjectField(new GUIContent("Source Folder (Project)"), sourceFolder, typeof(DefaultAsset), false);
            }
            else
            {
                singleJsonFile = (TextAsset)EditorGUILayout.ObjectField(new GUIContent("Single JSON File"), singleJsonFile, typeof(TextAsset), false);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("SO Type", GUILayout.Width(60));
            if (soTypeKeys.Length > 0)
            {
                selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, soTypeKeys);
            }
            if (GUILayout.Button("Refresh Types", GUILayout.Width(110))) RefreshTypes();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Assembly Folder", GUILayout.Width(100));
            assemblyFolderPath = EditorGUILayout.TextField(assemblyFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                var picked = EditorUtility.OpenFolderPanel("Select assembly folder", assemblyFolderPath, "");
                if (!string.IsNullOrEmpty(picked)) assemblyFolderPath = picked;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            outputFolder = EditorGUILayout.ObjectField(new GUIContent("Output Folder (Project)"), outputFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Convert"))
            {
                ConvertOnce();
            }

            if (GUILayout.Button("Convert All JSON in Folder"))
            {
                ConvertAllInFolder();
            }
            EditorGUILayout.EndHorizontal();
        }

        string GetProjectPath(UnityEngine.Object obj)
        {
            if (obj == null) return null;
            var p = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(p)) return null;
            return p;
        }

        void ConvertOnce()
        {
            if (soTypeKeys.Length == 0 || soTypeKeys[0].StartsWith("<No SO"))
            {
                EditorUtility.DisplayDialog("Error", "No ScriptableObject types found. Compile assemblies and try Refresh.", "OK");
                return;
            }

            var typeKey = soTypeKeys[selectedTypeIndex];
            if (!soTypes.TryGetValue(typeKey, out var targetType))
            {
                EditorUtility.DisplayDialog("Error", "Selected type not available.", "OK");
                return;
            }

            string outFolderPath = GetProjectPath(outputFolder);
            if (string.IsNullOrEmpty(outFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select an output folder in the Project.", "OK");
                return;
            }

            if (useFolder)
            {
                string src = GetProjectPath(sourceFolder);
                if (string.IsNullOrEmpty(src)) { EditorUtility.DisplayDialog("Error", "Please select a source folder.", "OK"); return; }
                ConvertFolder(src, outFolderPath, targetType);
            }
            else
            {
                if (singleJsonFile == null) { EditorUtility.DisplayDialog("Error", "Please select a JSON file.", "OK"); return; }
                var path = AssetDatabase.GetAssetPath(singleJsonFile);
                var json = File.ReadAllText(path);
                var name = Path.GetFileNameWithoutExtension(path);
                var created = JsonToSOTransformer.CreateFromJsonMany(json, targetType, outFolderPath, name);
                if (created != null && created.Count > 0)
                {
                    Debug.Log($"Created {created.Count} assets from {path}");
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = created.First();
                }
                else
                {
                    Debug.LogError($"Failed to create SO from {path}");
                }
            }
        }

        void ConvertAllInFolder()
        {
            if (soTypeKeys.Length == 0 || soTypeKeys[0].StartsWith("<No SO"))
            {
                EditorUtility.DisplayDialog("Error", "No ScriptableObject types found. Compile assemblies and try Refresh.", "OK");
                return;
            }

            var typeKey = soTypeKeys[selectedTypeIndex];
            if (!soTypes.TryGetValue(typeKey, out var targetType))
            {
                EditorUtility.DisplayDialog("Error", "Selected type not available.", "OK");
                return;
            }

            string outFolderPath = GetProjectPath(outputFolder);
            if (string.IsNullOrEmpty(outFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select an output folder in the Project.", "OK");
                return;
            }

            string src = GetProjectPath(sourceFolder);
            if (string.IsNullOrEmpty(src)) { EditorUtility.DisplayDialog("Error", "Please select a source folder.", "OK"); return; }

            // find all TextAsset under src that end with .json
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { src });
            var jsonPaths = new List<string>();
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) jsonPaths.Add(p);
            }

            if (jsonPaths.Count == 0) { EditorUtility.DisplayDialog("Info", "No JSON files found in the selected folder.", "OK"); return; }

            int count = 0;
            try
            {
                for (int i = 0; i < jsonPaths.Count; i++)
                {
                    var p = jsonPaths[i];
                    EditorUtility.DisplayProgressBar("Converting JSON", p, (float)i / jsonPaths.Count);
                    var json = File.ReadAllText(p);
                    var name = Path.GetFileNameWithoutExtension(p);
                    var created = JsonToSOTransformer.CreateFromJsonMany(json, targetType, outFolderPath, name);
                    if (created != null) count += created.Count;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Done", $"Converted {count} / {jsonPaths.Count} JSON files.", "OK");
        }

        void ConvertFolder(string srcFolder, string outFolder, Type targetType)
        {
            // find json files
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { srcFolder });
            var jsonPaths = new List<string>();
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) jsonPaths.Add(p);
            }

            if (jsonPaths.Count == 0) { EditorUtility.DisplayDialog("Info", "No JSON files found in the selected folder.", "OK"); return; }

            int count = 0;
            try
            {
                for (int i = 0; i < jsonPaths.Count; i++)
                {
                    var p = jsonPaths[i];
                    EditorUtility.DisplayProgressBar("Converting JSON", p, (float)i / jsonPaths.Count);
                    var json = File.ReadAllText(p);
                    var name = Path.GetFileNameWithoutExtension(p);
                    var created = JsonToSOTransformer.CreateFromJsonMany(json, targetType, outFolder, name);
                    if (created != null) count += created.Count;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Done", $"Converted {count} / {jsonPaths.Count} JSON files.", "OK");
        }
    }
}
#endif
