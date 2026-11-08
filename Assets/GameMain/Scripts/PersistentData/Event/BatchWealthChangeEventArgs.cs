using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;

namespace MergeSword
{
    public struct WealthChangeInfo
    {
        public WealthTypes Type;
        public int Id;
        public int Delta;

        public WealthChangeInfo(WealthTypes type, int id, int delta)
        {
            Type = type;
            Id = id;
            Delta = delta;
        }
    }

    public class BatchWealthChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(BatchWealthChangeEventArgs).GetHashCode();

        public override int Id => EventId;

        public List<WealthChangeInfo> Changes { get; private set; }

        public static BatchWealthChangeEventArgs Create()
        {
            var e = ReferencePool.Acquire<BatchWealthChangeEventArgs>();
            if (e.Changes == null)
            {
                e.Changes = new List<WealthChangeInfo>();
            }
            return e;
        }

        public static BatchWealthChangeEventArgs Create(WealthTypes wealthType, int wealthId, int delta)
        {
            var e = Create();
            e.Changes.Add(new WealthChangeInfo(wealthType, wealthId, delta));
            return e;
        }

        public override void Clear()
        {
            Changes?.Clear();
        }
    }
}
