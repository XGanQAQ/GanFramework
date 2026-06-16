using System;
using System.Collections.Generic;
using GanFramework.Core;
using GanFramework.Core.Modules.Log;

namespace GanFramework.Runtime.Modules.Log
{
    public class UnityLogger : ILogger, IModules
    {
        private LogLevel _level = LogLevel.Debug;
        private long _enabledChannelsMask = LogChannel.GetAllMask();
        private readonly List<ILogHandler> _handlers = new();

        public LogLevel Level
        {
            get => _level;
            set => _level = value;
        }

        public long EnabledChannelsMask
        {
            get => _enabledChannelsMask;
            set => _enabledChannelsMask = value;
        }

        public bool IsEnabled(LogLevel level, LogChannel channel)
        {
            return level >= _level && (channel.BitMask & _enabledChannelsMask) != 0;
        }

        public void Log(LogLevel level, LogChannel channel, object message)
        {
            if (!IsEnabled(level, channel)) return;
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].Log(level, channel, message);
        }

        public void Log(LogLevel level, LogChannel channel, object message, object context)
        {
            if (!IsEnabled(level, channel)) return;
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].Log(level, channel, message, context);
        }

        public void Log(LogLevel level, LogChannel channel, string format, params object[] args)
        {
            if (!IsEnabled(level, channel)) return;
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].Log(level, channel, format, args);
        }

        public void Trace(LogChannel channel, object message) => Log(LogLevel.Trace, channel, message);
        public void Debug(LogChannel channel, object message) => Log(LogLevel.Debug, channel, message);
        public void Info(LogChannel channel, object message) => Log(LogLevel.Info, channel, message);
        public void Warn(LogChannel channel, object message) => Log(LogLevel.Warning, channel, message);
        public void Error(LogChannel channel, object message) => Log(LogLevel.Error, channel, message);
        public void Fatal(LogChannel channel, object message) => Log(LogLevel.Fatal, channel, message);

        public void EnableChannel(LogChannel channel) => _enabledChannelsMask |= channel.BitMask;
        public void DisableChannel(LogChannel channel) => _enabledChannelsMask &= ~channel.BitMask;
        public bool IsChannelEnabled(LogChannel channel) => (channel.BitMask & _enabledChannelsMask) != 0;

        public void AddHandler(ILogHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler))
                _handlers.Add(handler);
        }

        public void RemoveHandler(ILogHandler handler) => _handlers.Remove(handler);

        public void OnInit()
        {
            _handlers.Add(new UnityLogHandler());
            LogManager.Initialize(this);
        }

        public void OnUpdate(float deltaTime) { }
        public void OnFixedUpdate(float fixedDeltaTime) { }
        public void OnLateUpdate(float deltaTime) { }

        public void OnDestroy()
        {
            _handlers.Clear();
            if (LogManager.Instance == this)
                LogManager.Initialize(null);
        }
    }
}
