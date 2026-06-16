using System;

namespace GanFramework.Core.Modules.Log
{
    public static class LogManager
    {
        private static ILogger _instance;

        public static ILogger Instance => _instance;

        public static void Initialize(ILogger logger) => _instance = logger;

        public static LogLevel Level
        {
            get => _instance != null ? _instance.Level : LogLevel.Info;
            set { if (_instance != null) _instance.Level = value; }
        }

        public static long EnabledChannelsMask
        {
            get => _instance != null ? _instance.EnabledChannelsMask : 0L;
            set { if (_instance != null) _instance.EnabledChannelsMask = value; }
        }

        public static bool IsEnabled(LogLevel level, LogChannel channel) =>
            _instance != null && _instance.IsEnabled(level, channel);

        public static void Log(LogLevel level, LogChannel channel, object message) =>
            _instance?.Log(level, channel, message);

        public static void Log(LogLevel level, LogChannel channel, object message, object context) =>
            _instance?.Log(level, channel, message, context);

        public static void Log(LogLevel level, LogChannel channel, string format, params object[] args) =>
            _instance?.Log(level, channel, format, args);

        public static void Trace(LogChannel channel, object message) =>
            _instance?.Trace(channel, message);

        public static void Debug(LogChannel channel, object message) =>
            _instance?.Debug(channel, message);

        public static void Info(LogChannel channel, object message) =>
            _instance?.Info(channel, message);

        public static void Warn(LogChannel channel, object message) =>
            _instance?.Warn(channel, message);

        public static void Error(LogChannel channel, object message) =>
            _instance?.Error(channel, message);

        public static void Fatal(LogChannel channel, object message) =>
            _instance?.Fatal(channel, message);

        public static void EnableChannel(LogChannel channel) =>
            _instance?.EnableChannel(channel);

        public static void DisableChannel(LogChannel channel) =>
            _instance?.DisableChannel(channel);

        public static bool IsChannelEnabled(LogChannel channel) =>
            _instance != null && _instance.IsChannelEnabled(channel);

        public static void AddHandler(ILogHandler handler) =>
            _instance?.AddHandler(handler);

        public static void RemoveHandler(ILogHandler handler) =>
            _instance?.RemoveHandler(handler);
    }
}
