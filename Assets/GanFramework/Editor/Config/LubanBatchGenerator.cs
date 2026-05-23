// 仅在Unity编辑器环境下编译
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;

namespace GanFramework.Editor.Config
{
    /// <summary>
    /// Builds the content of a gen.bat for running Luban based on configurable paths.
    /// </summary>
    [Serializable]
    public class LubanBatchGenerator
    {
        public string workspace = ".."; // relative to project root
        public string lubanDllRelative = "..\\Luban\\Luban.dll"; // relative to workspace
        public string confRoot = ".";
        public string target = "all";
        public string config = "cs-simple-json";
        public string dataFormat = "json";
        public string confFile = "luban.conf";
        public string outputCodeDir = "Scripts\\Gameplay\\Auto\\ConfigsData\\";
        public string outputDataDir = "Configs\\Json\\";

        public string BuildBatchContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("setlocal enabledelayedexpansion");
            sb.AppendLine();

            sb.AppendFormat("set WORKSPACE={0}", workspace).AppendLine();
            sb.AppendFormat("set LUBAN_DLL=%WORKSPACE%\\{0}", lubanDllRelative).AppendLine();
            sb.AppendFormat("set CONF_ROOT={0}", confRoot).AppendLine();
            sb.AppendLine();

            // Build dotnet call lines
            sb.AppendLine("dotnet %LUBAN_DLL% ^");
            sb.AppendFormat("    -t {0} ^", target).AppendLine();
            sb.AppendFormat("    -c {0} ^", config).AppendLine();
            sb.AppendFormat("    -d {0} ^", dataFormat).AppendLine();
            sb.AppendFormat("    --conf %CONF_ROOT%\\{0} ^", confFile).AppendLine();
            sb.AppendFormat("    -x outputCodeDir=%WORKSPACE%\\{0} ^", outputCodeDir.TrimEnd('\\')).AppendLine();
            sb.AppendFormat("    -x outputDataDir={0}", outputDataDir).AppendLine();

            // Note: using AppendLine for final line ensures proper newlines across platforms
            sb.AppendLine();
            sb.AppendLine("pause");

            return sb.ToString();
        }

        public void SaveToFile(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir)) dir = ".";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, BuildBatchContent(), Encoding.Default);
        }
    }
}
#endif
// #if UNITY_EDITOR
// #endif
