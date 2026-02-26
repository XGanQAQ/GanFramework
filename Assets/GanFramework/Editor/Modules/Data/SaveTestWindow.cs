// 仅在Unity编辑器环境下编译
#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using GanFramework.Core.Data.Persistent;
using UnityEditor;
using UnityEngine;

namespace GanFramework.Editor.Data
{
    public class SaveTestWindow : EditorWindow
    {
        private TestSaveData data = new TestSaveData();
        private string log = "";
        // Selected save format for testing (shown in the dropdown)
        private SaveFormat selectedFormat = SaveFormat.NetJson;

        [MenuItem("GanFramework/Test/Save Test Window")]
        public static void ShowWindow()
        {
            GetWindow<SaveTestWindow>("Save Test");
        }

        private void OnGUI()
        {
            GUILayout.Label("Test Save/Load", EditorStyles.boldLabel);

            // Format selection dropdown
            selectedFormat = (SaveFormat)EditorGUILayout.EnumPopup("Save Format", selectedFormat);

            data.PlayerName = EditorGUILayout.TextField("PlayerName", data.PlayerName);
            data.Level = EditorGUILayout.IntField("Level", data.Level);
            data.Health = EditorGUILayout.FloatField("Health", data.Health);

            if (GUILayout.Button("Save (Sync)"))
            {
                try
                {
                    SaveManager.Save(data, selectedFormat);
                    log = "Save (sync) success";
                }
                catch (Exception ex)
                {
                    log = "Save (sync) failed: " + ex;
                }
            }

            if (GUILayout.Button("Load (Sync)"))
            {
                try
                {
                    var loaded = SaveManager.Load<TestSaveData>(selectedFormat);
                    if (loaded != null)
                    {
                        data = loaded;
                        log = "Load (sync) success: " + data;
                    }
                    else
                    {
                        log = "Load (sync) returned null";
                    }
                }
                catch (Exception ex)
                {
                    log = "Load (sync) failed: " + ex;
                }
            }

            if (GUILayout.Button("Save Members (Sync)"))
            {
                try
                {
                    SaveManager.SaveMembers(data, selectedFormat);
                    log = "SaveMembers (sync) success";
                }
                catch (Exception ex)
                {
                    log = "SaveMembers (sync) failed: " + ex;
                }
            }

            if (GUILayout.Button("Load Members (Sync)"))
            {
                try
                {
                    var loaded = SaveManager.LoadMembers<TestSaveData>(selectedFormat);
                    if (loaded != null)
                    {
                        data = loaded;
                        log = "LoadMembers (sync) success: " + data;
                    }
                    else
                    {
                        log = "LoadMembers (sync) returned null";
                    }
                }
                catch (Exception ex)
                {
                    log = "LoadMembers (sync) failed: " + ex;
                }
            }

            if (GUILayout.Button("Save (Async)"))
            {
                SaveAsync();
            }

            if (GUILayout.Button("Load (Async)"))
            {
                LoadAsync();
            }

            if (GUILayout.Button("Save Members (Async)"))
            {
                SaveMembersAsync();
            }

            if (GUILayout.Button("Load Members (Async)"))
            {
                LoadMembersAsync();
            }

            EditorGUILayout.SelectableLabel(log, GUILayout.Height(60));

            // Show file path and allow viewing/deleting
            string fileName = GetFileNameForType(typeof(TestSaveData), selectedFormat);
            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            EditorGUILayout.LabelField("Save File:", fullPath);

            if (File.Exists(fullPath))
            {
                if (GUILayout.Button("Show File Content"))
                {
                    var txt = File.ReadAllText(fullPath);
                    EditorUtility.DisplayDialog("Saved File Content", txt, "OK");
                }

                if (GUILayout.Button("Delete Save File"))
                {
                    File.Delete(fullPath);
                    AssetDatabase.Refresh();
                    Debug.Log("Deleted save file");
                }
            }
            else
            {
                EditorGUILayout.LabelField("No save file found.");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Fields marked with [SaveMember] will be written/read when using the members-only APIs. Full Save/Load will serialize the whole object.", MessageType.Info);
        }

        // Return filename with extension matching the selected save format
        private static string GetFileNameForType(Type t, SaveFormat format)
        {
            var attr = t.GetCustomAttribute<SaveClassAttribute>();
            string key = attr != null ? attr.Key : t.Name;
            string ext;
            switch (format)
            {
                case SaveFormat.NetJson:
                    ext = ".json";
                    break;
                case SaveFormat.OdinJson:
                    ext = ".json";
                    break;
                case SaveFormat.OdinBinary:
                    ext = ".bin";
                    break;
                default:
                    ext = ".dat";
                    break;
            }

            return key + ext;
        }

        private async void SaveAsync()
        {
            try
            {
                await SaveManager.SaveAsync(data, selectedFormat);
                log = "Save (async) success";
                Repaint();
            }
            catch (Exception ex)
            {
                log = "Save (async) failed: " + ex;
                Repaint();
            }
        }

        private async void LoadAsync()
        {
            try
            {
                var loaded = await SaveManager.LoadAsync<TestSaveData>(selectedFormat);
                if (loaded != null)
                {
                    data = loaded;
                    log = "Load (async) success: " + data;
                }
                else
                {
                    log = "Load (async) returned null";
                }
                Repaint();
            }
            catch (Exception ex)
            {
                log = "Load (async) failed: " + ex;
                Repaint();
            }
        }

        private async void SaveMembersAsync()
        {
            try
            {
                await SaveManager.SaveMembersAsync(data, selectedFormat);
                log = "SaveMembers (async) success";
                Repaint();
            }
            catch (Exception ex)
            {
                log = "SaveMembers (async) failed: " + ex;
                Repaint();
            }
        }

        private async void LoadMembersAsync()
        {
            try
            {
                var loaded = await SaveManager.LoadMembersAsync<TestSaveData>(selectedFormat);
                if (loaded != null)
                {
                    data = loaded;
                    log = "LoadMembers (async) success: " + data;
                }
                else
                {
                    log = "LoadMembers (async) returned null";
                }
                Repaint();
            }
            catch (Exception ex)
            {
                log = "LoadMembers (async) failed: " + ex;
                Repaint();
            }
        }
    }
}
#endif