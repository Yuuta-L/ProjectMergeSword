using UnityEngine;

namespace EndlessAbyss
{
    /// <summary>
    /// 指针按下检测：按运行时能力在触摸与鼠标之间选择输入源。
    /// 不能用 UNITY_EDITOR 之类的编译期分支判断输入设备——WebGL 桌面端有鼠标但触摸数恒为 0，
    /// 手机会把触摸模拟成鼠标，两者必须靠 Input.touchCount 在运行时区分。
    /// </summary>
    public static class PointerInput
    {
        private static int _prevTouchCount;
        private static int _sampledFrame = -1;
        private static bool _touchCountIncreased;

        /// <summary>
        /// 本帧是否有新的按下，并给出对应的屏幕坐标。
        /// </summary>
        public static bool TryGetPointerDown(out Vector2 screenPos)
        {
            int touchCount = Input.touchCount;

            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began)
                    continue;

                screenPos = touch.position;
                return true;
            }

            // WXTouchInputOverride 会在 LateUpdate 清空触摸，Update 中可能观测不到 Began，
            // 因此再用“本帧触摸数增长”兜底。
            if (touchCount > 0 && IsTouchCountIncreased())
            {
                screenPos = Input.GetTouch(0).position;
                return true;
            }

            // 触摸存在时 Unity 会同时模拟鼠标事件，已在上面处理，这里只在无触摸时才看鼠标，
            // 避免一次点击被判定成两次。
            if (touchCount == 0 && Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                return true;
            }

            screenPos = Vector2.zero;
            return false;
        }

        /// <summary>
        /// 当前指针屏幕坐标（有触摸时取第一个触摸，否则取鼠标）。
        /// </summary>
        public static Vector2 Position =>
            Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        /// <summary>
        /// 按帧采样触摸数是否增长，避免同帧多个调用方互相吞掉检测结果。
        /// </summary>
        private static bool IsTouchCountIncreased()
        {
            if (_sampledFrame != Time.frameCount)
            {
                _sampledFrame = Time.frameCount;
                _touchCountIncreased = Input.touchCount > _prevTouchCount;
                _prevTouchCount = Input.touchCount;
            }

            return _touchCountIncreased;
        }
    }
}
