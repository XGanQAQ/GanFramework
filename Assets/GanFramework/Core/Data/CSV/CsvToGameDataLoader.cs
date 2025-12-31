using System;
using System.Collections.Generic;
using UnityEngine;

namespace GanFramework.Core.Data.CSV
{
    public class CsvToGameDataLoader<T>
    {
        private static List<T> LoadDataFromCsv(TextAsset csvFile)
        {
            if (csvFile == null)
            {
                Debug.LogError("CSV file is null.");
                return null;
            }
            
            // Parse CSV
            string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Debug.LogWarning("CSV file does not contain enough data.");
                return null;
            }
            
            List<T> gameDataList = new List<T>();

            // Assume first line is header
            string[] headers = lines[0].Split(',');

            // Read data lines
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                T dataInstance = Activator.CreateInstance<T>();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    var property = typeof(T).GetProperty(headers[j]);
                    if (property != null)
                    {
                        object convertedValue = Convert.ChangeType(values[j], property.PropertyType);
                        property.SetValue(dataInstance, convertedValue);
                    }
                }
                
                gameDataList.Add(dataInstance);
            }
            
            return gameDataList;
        }
    }
}