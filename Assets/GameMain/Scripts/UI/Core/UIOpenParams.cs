using BurnicleFramework.Runtime;
using GameFramework;

namespace MergeSword
{
    public class UIOpenParams : RefParams
    {
        public bool? AllowEscapeClose { get; set; } = null;
        public GameFrameworkAction<UIFormLogic> OpenCallback { get; set; } = null;
        public GameFrameworkAction<UIFormLogic> CloseCallback { get; set; } = null;
        public GameFrameworkAction<object, string> ButtonClickCallback { get; set; } = null;

        public static UIOpenParams Create(bool? allowEscape = null, int depth = 0)
        {
            var uiParams = ReferencePool.Acquire<UIOpenParams>();
            uiParams.AllowEscapeClose = allowEscape;
            return uiParams;
        }


        public override void Clear()
        {
            base.Clear();
            AllowEscapeClose = null;
            OpenCallback = null;
            CloseCallback = null;
            ButtonClickCallback = null;
        }
    }
}