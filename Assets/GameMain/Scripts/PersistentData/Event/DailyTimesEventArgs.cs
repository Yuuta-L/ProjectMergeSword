using GameFramework;
using GameFramework.Event;

namespace MergeSword
{
    public class DailyTimesEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DailyTimesEventArgs).GetHashCode();

        public override int Id => EventId;

        public string Key;
        public int Times;

        public static DailyTimesEventArgs Create(string key,int times)
        {
            var e = ReferencePool.Acquire<DailyTimesEventArgs>();
            e.Key = key;
            e.Times = times;
            return e;
        }

        public override void Clear()
        {
            Key = string.Empty;
            Times = 0;
        }
    }
}