using System;
using System.Collections.Generic;

namespace GanFramework.Core.Modules.Log
{
    public sealed class LogChannel : IEquatable<LogChannel>
    {
        private static int _nextCustomId = 16;
        private static readonly List<LogChannel> _registered = new();
        private static readonly object _lock = new();

        public int Id { get; }
        public string Name { get; }
        public long BitMask { get; }

        public static readonly LogChannel Default = new(0, "Default");
        public static readonly LogChannel System = new(1, "System");
        public static readonly LogChannel UI = new(2, "UI");
        public static readonly LogChannel Network = new(3, "Network");
        public static readonly LogChannel Audio = new(4, "Audio");
        public static readonly LogChannel Combat = new(5, "Combat");
        public static readonly LogChannel AI = new(6, "AI");
        public static readonly LogChannel Input = new(7, "Input");
        public static readonly LogChannel Resource = new(8, "Resource");
        public static readonly LogChannel Animation = new(9, "Animation");
        public static readonly LogChannel Event = new(10, "Event");

        private LogChannel(int id, string name)
        {
            Id = id;
            Name = name;
            BitMask = 1L << id;
            _registered.Add(this);
        }

        public static LogChannel Register(string name)
        {
            lock (_lock)
            {
                return new LogChannel(_nextCustomId++, name);
            }
        }

        public static long GetAllMask()
        {
            long mask = 0;
            for (int i = 0; i < _registered.Count; i++)
                mask |= _registered[i].BitMask;
            return mask;
        }

        public bool Equals(LogChannel other) => other != null && Id == other.Id;
        public override bool Equals(object obj) => obj is LogChannel other && Equals(other);
        public override int GetHashCode() => Id;
        public override string ToString() => Name;
    }
}
