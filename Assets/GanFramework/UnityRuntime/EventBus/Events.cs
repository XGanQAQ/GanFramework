using GanFramework.Core.EventBus;

namespace GanFramework.Runtime.EventBus
{
    public struct TestEvent : IEvent
    {
    }

    // 占位事件，后续可以删除
    public struct PlaceholderEvent : IEvent
    {

    }

    public struct PlayerEvent : IEvent
    {
        public int health;
        public int mana;
    }
}
