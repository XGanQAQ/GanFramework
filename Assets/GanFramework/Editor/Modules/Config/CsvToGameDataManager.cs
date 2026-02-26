// 仅在Unity编辑器环境下编译
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using GanFramework.Core.Data;
using System.IO;
using UnityEditor;

namespace GanFramework.Editor.Config
{
    public class CsvToGameDataManager
    {
        public static void Import<T>(string csvPath, string outputDir,T modelType) where T : ScriptableObject
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var lines = File.ReadAllLines(csvPath);

            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] values = lines[i].Split(',');

                string id = values[0];
                string assetPath = $"{outputDir}/{id}.asset";
                
                T newInstance = ScriptableObject.CreateInstance<T>();
                if (modelType != null)
                {
                    AssetDatabase.CreateAsset(newInstance, assetPath);
                }

                for (int j = 0; j < headers.Length; j++)
                {
                    SetField(modelType, headers[j], values[j]);
                }

                EditorUtility.SetDirty(modelType);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("CSV Import Finished");
        }
        
        // 使用反射在目标对象上设置字段值
        static void SetField(object target, string fieldName, string value)
        {
            var field = target.GetType().GetField(fieldName);
            if (field == null) return;

            if (field.FieldType == typeof(int))
                field.SetValue(target, int.Parse(value));
            else if (field.FieldType == typeof(float))
                field.SetValue(target, float.Parse(value));
            else
                field.SetValue(target, value);
        }
    }

    [Serializable]
    public class CsvToScriptableObjectList
    {
        public TextAsset csvFile;
        public ScriptableObject scriptableModel;
        public List<ScriptableObject> dataList;
    }
}
#endif
// #if UNITY_EDITOR
// #endif