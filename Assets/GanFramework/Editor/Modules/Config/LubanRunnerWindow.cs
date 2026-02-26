// 仅在Unity编辑器环境下编译
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace GanFramework.Editor.Config
{
    public class LubanRunnerWindow : EditorWindow
    {
        const string PrefKeyBatPath = "LubanEditor_BatPath";

        string batPath = string.Empty;
        bool showOutput = true;
        bool isRunning;
        StringBuilder outputBuilder = new StringBuilder();
        Process runningProcess;
        Vector2 scrollPos;

        [MenuItem("GanFramework/Luban Runner")]
        public static void ShowWindow()
        {
            var w = GetWindow<LubanRunnerWindow>(false, "Luban Runner");
            w.minSize = new Vector2(480, 220);
        }

        void OnEnable()
        {
            batPath = EditorPrefs.GetString(PrefKeyBatPath, string.Empty);
        }

        void OnDisable()
        {
            EditorPrefs.SetString(PrefKeyBatPath, batPath ?? string.Empty);
            TryKillProcess();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Luban .bat Runner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Batch file:", GUILayout.Width(70));
            batPath = EditorGUILayout.TextField(batPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string start = string.IsNullOrEmpty(batPath) ? Application.dataPath : Path.GetDirectoryName(batPath);
                string chosen = EditorUtility.OpenFilePanel("Select Luban .bat", start, "bat");
                if (!string.IsNullOrEmpty(chosen))
                {
                    batPath = chosen;
                    EditorPrefs.SetString(PrefKeyBatPath, batPath);
                }
            }
            if (GUILayout.Button("Auto-find", GUILayout.Width(80)))
            {
                string found = TryAutoFindBat();
                if (!string.IsNullOrEmpty(found))
                {
                    batPath = found;
                    EditorPrefs.SetString(PrefKeyBatPath, batPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Auto-find", "No .bat named 'luban' found under the project. You can browse manually.", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            showOutput = EditorGUILayout.ToggleLeft("Show output", showOutput, GUILayout.Width(120));
            GUILayout.FlexibleSpace();

            GUI.enabled = !isRunning && File.Exists(batPath);
            if (GUILayout.Button("Run", GUILayout.Width(80)))
            {
                RunBatAsync(batPath);
            }
            GUI.enabled = isRunning;
            if (GUILayout.Button("Stop", GUILayout.Width(80)))
            {
                TryKillProcess();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Open Folder", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(batPath) && File.Exists(batPath))
                {
                    EditorUtility.RevealInFinder(batPath);
                }
                else
                {
                    EditorUtility.RevealInFinder(Application.dataPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (showOutput)
            {
                EditorGUILayout.LabelField("Output:", EditorStyles.label);
                GUIStyle box = new GUIStyle(EditorStyles.helpBox);
                box.richText = true;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(outputBuilder.ToString(), box, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(isRunning ? "Process is running..." : "Idle.", MessageType.Info);
        }

        string TryAutoFindBat()
        {
            try
            {
                // Search common places: project root, Assets, under Plugins or Tools
                var parent = Directory.GetParent(Application.dataPath);
                if (parent == null) return string.Empty;
                string projectRoot = parent.FullName;
                var candidates = Directory.GetFiles(projectRoot, "luban*.bat", SearchOption.AllDirectories);
                if (candidates.Length > 0)
                    return candidates[0];

                // fallback: any .bat in project
                var any = Directory.GetFiles(projectRoot, "*.bat", SearchOption.AllDirectories);
                if (any.Length > 0)
                    return any[0];
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("Auto-find failed: " + ex.Message);
            }
            return string.Empty;
        }

        async void RunBatAsync(string path)
        {
            if (isRunning)
                return;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "Batch file not found. Please select a valid .bat file.", "OK");
                return;
            }

            isRunning = true;
            outputBuilder.Clear();
            Repaint();

            await Task.Run(() =>
            {
                try
                {
                    string workingDir = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;

                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c \"" + path + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDir
                    };

                    using (var proc = new Process())
                    {
                        runningProcess = proc;
                        proc.StartInfo = psi;

                        proc.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) AppendOutput(e.Data + "\n"); };
                        proc.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) AppendOutput("[ERR] " + e.Data + "\n"); };

                        proc.Start();
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        proc.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    AppendOutput("Exception: " + ex.Message + "\n");
                }
                finally
                {
                    runningProcess = null;
                    EditorApplication.delayCall += () => { isRunning = false; Repaint(); };
                }
            });
        }

        void AppendOutput(string text)
        {
            lock (outputBuilder)
            {
                outputBuilder.Append(text);
            }
            // Ensure UI updates on the main thread
            EditorApplication.delayCall += Repaint;
        }

        void TryKillProcess()
        {
            try
            {
                if (runningProcess != null && !runningProcess.HasExited)
                {
                    runningProcess.Kill();
                    AppendOutput("Process killed by user.\n");
                }
            }
            catch (Exception ex)
            {
                AppendOutput("Failed to kill process: " + ex.Message + "\n");
            }
            finally
            {
                runningProcess = null;
                isRunning = false;
                Repaint();
            }
        }
    }
}
#endif