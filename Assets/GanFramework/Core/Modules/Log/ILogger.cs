using System;

namespace GanFramework.Core.Modules.Log
{
    public interface ILogger
    {
        LogLevel Level { get; set; }
        long EnabledChannelsMask { get; set; }

        bool IsEnabled(LogLevel level, LogChannel channel);

        void Log(LogLevel level, LogChannel channel, object message);
        void Log(LogLevel level, LogChannel channel, object message, object context);
        void Log(LogLevel level, LogChannel channel, string format, params object[] args);

        void Trace(LogChannel channel, object message);
        void Debug(LogChannel channel, object message);
        void Info(LogChannel channel, object message);
        void Warn(LogChannel channel, object message);
        void Error(LogChannel channel, object message);
        void Fatal(LogChannel channel, object message);

        void EnableChannel(LogChannel channel);
        void DisableChannel(LogChannel channel);
        bool IsChannelEnabled(LogChannel channel);

        void AddHandler(ILogHandler handler);
        void RemoveHandler(ILogHandler handler);
    }
}
