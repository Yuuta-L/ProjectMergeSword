using System;
using BurnicleFramework.Generated;
using GameFramework;
using GameFramework.Event;

namespace MergeSword
{
    public class PlayingUnlockedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayingUnlockedEventArgs).GetHashCode();

        public override int Id => EventId;

        // 添加你的字段
        public EnumUnlockable UnlockableItem;
        
        public static PlayingUnlockedEventArgs Create(EnumUnlockable unlockableItem)
        {
            var e = ReferencePool.Acquire<PlayingUnlockedEventArgs>();
            e.UnlockableItem = unlockableItem;
            return e;
        }

        public override void Clear()
        {
        }
    }
}