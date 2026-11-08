using GameFramework;

namespace MergeSword
{
    public partial class UISequence
    {
        private class Cell: IReference
        {
            public Cell()
            {
                UIFormId = -1;
                OpenParams = null;
                Priority = 0;
            }

            public int UIFormId
            {
                get;
                private set;
            }

            public UIOpenParams OpenParams
            {
                get;
                private set;
            }

            public int Priority
            {
                get;
                private set;
            }
            
            public static Cell Create(int uiFormId,UIOpenParams openParams, int priority)
            {
                var item = ReferencePool.Acquire<Cell>();
                item.UIFormId = uiFormId;
                item.OpenParams = openParams;
                item.Priority = priority;
                return item;
            }

            public void Clear()
            {
                UIFormId = -1;
                OpenParams = null;
                Priority = 0;
            }
        }
    }
}