namespace GanFramework.Core.Data
{
    // 简单的CSV读取器
    // lineIndex = 0表示第一行（头部行）
    // 1表示第二行（第一条值行），以此类推
    public static class CsvReader
    {
        // 读取CSV内容的头部（第一行）
        public static string[] GetHeaders(string csvContent)
        {
            var lines = csvContent.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return null;

            return lines[0].Split(',');
        }
        
        // 读取CSV内容的指定行的值
        // 1表示值行的第一行（0表示头部行）
        public static string[] GetValues(string csvContent, int lineIndex)
        {
            var lines = csvContent.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= lineIndex)
                return null;

            return lines[lineIndex].Split(',');
        }
    }
}