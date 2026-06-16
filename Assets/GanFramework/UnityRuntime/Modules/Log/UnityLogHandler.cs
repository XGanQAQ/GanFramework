using System;
using UnityEngine;
using GanFramework.Core.Modules.Log;
using ILogHandler = GanFramework.Core.Modules.Log.ILogHandler;

namespace GanFramework.Runtime.Modules.Log
{
    public class UnityLogHandler : ILogHandler
    {
        public void Log(LogLevel level, LogChannel channel, object message)
        {
            var msg = FormatMessage(level, channel, message != null ? message.ToString() : "null");
            DispatchToConsole(level, msg, null);
        }

        public void Log(LogLevel level, LogChannel channel, object message, object context)
        {
            var msg = FormatMessage(level, channel, message != null ? message.ToString() : "null");
            DispatchToConsole(level, msg, context as UnityEngine.Object);
        }

        public void Log(LogLevel level, LogChannel channel, string format, params object[] args)
        {
            var msg = FormatMessage(level, channel, args != null ? string.Format(format, args) : format);
            DispatchToConsole(level, msg, null);
        }

        public void LogException(Exception exception, object context)
        {
            Debug.LogException(exception, context as UnityEngine.Object);
        }

        private static void DispatchToConsole(LogLevel level, string message, UnityEngine.Object context)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    if (context != null)
                        Debug.Log(message, context);
                    else
                        Debug.Log(message);
                    break;

                case LogLevel.Warning:
                    if (context != null)
                        Debug.LogWarning(message, context);
                    else
                        Debug.LogWarning(message);
                    break;

                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (context != null)
                        Debug.LogError(message, context);
                    else
                        Debug.LogError(message);
                    break;
            }
        }

        private static string FormatMessage(LogLevel level, LogChannel channel, string message)
        {
            var result = $"[{channel.Name}][{level}] {message}";
            if (level >= LogLevel.Error)
            {
                var stack = new System.Diagnostics.StackTrace(true).ToString();
                result += "\n" + stack;
            }
            return result;
        }
    }
}
