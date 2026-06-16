using System;

namespace GanFramework.Core.Modules.Log
{
    public interface ILogHandler
    {
        void Log(LogLevel level, LogChannel channel, object message);
        void Log(LogLevel level, LogChannel channel, object message, object context);
        void Log(LogLevel level, LogChannel channel, string format, params object[] args);
        void LogException(Exception exception, object context);
    }
}
