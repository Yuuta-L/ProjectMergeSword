using System;
using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{

    public class SafeAreaAdapter : MonoBehaviour
    {
        public AdaptOrientationModes adaptOrientationMode;
        public AdaptChangeModes adaptChangeMode;
        public bool adaptOnAwake = true;

        private RectTransform _rectTransform;
        private Vector2 _originAnchorPosition;
        private Vector2 _originOffsetMin;
        private Vector2 _originOffsetMax;
        private Vector2 _originSizeDelta;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _originAnchorPosition = _rectTransform.anchoredPosition;
            _originOffsetMin = _rectTransform.offsetMin;
            _originOffsetMax = _rectTransform.offsetMax;
            _originSizeDelta = _rectTransform.sizeDelta;

            if (adaptOnAwake)
            {
                Adapt();
            }
        }

        public void Adapt()
        {
            var safeAreaData = Burnicle.CrossPlatform.GetSafeArea();

            // 将不安全区域从数据来源坐标系（如微信CSS像素）换算到 Unity 屏幕坐标系
            float scaleY = safeAreaData.sourceHeight > 0
                ? ScreenUtility.ScreenHeight() / safeAreaData.sourceHeight
                : 1f;
            float actualTopUnsafe = safeAreaData.topUnsafe * scaleY;
            float actualBottomUnsafe = safeAreaData.bottomUnsafe * scaleY;

            switch (adaptOrientationMode)
            {
                case AdaptOrientationModes.Top:
                    Change(Vector2.down, actualTopUnsafe);
                    break;
                case AdaptOrientationModes.Down:
                    Change(Vector2.up, actualBottomUnsafe);
                    break;
            }
        }

        private void Change(Vector2 direction, float value)
        {
            bool isStretchY = Mathf.Approximately(_rectTransform.anchorMin.y, 0f) &&
                              Mathf.Approximately(_rectTransform.anchorMax.y, 1f);

            if (isStretchY)
            {
                // 拉伸模式：通过调整 offsetMin 和 offsetMax 来改变尺寸和位置
                ChangeStretch(direction, value);
            }
            else
            {
                // 固定尺寸模式：使用 SetSizeWithCurrentAnchors 和 anchoredPosition
                ChangeFixed(direction, value);
            }
        }

        private void ChangeFixed(Vector2 direction, float value)
        {
            var originSize = _originSizeDelta;
            var pivot = _rectTransform.pivot;

            switch (adaptChangeMode)
            {
                case AdaptChangeModes.Move:
                    _rectTransform.anchoredPosition = _originAnchorPosition + direction * value;
                    break;

                case AdaptChangeModes.Expand:
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originSize.y + value);
                    float expandOffset = value * (direction.y > 0 ? pivot.y : (1 - pivot.y));
                    _rectTransform.anchoredPosition = _originAnchorPosition + direction * expandOffset;
                    break;

                case AdaptChangeModes.Compress:
                    float newHeight = Mathf.Max(0, originSize.y - value);
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
                    float compressOffset = value * (direction.y > 0 ? (1 - pivot.y) : pivot.y);
                    _rectTransform.anchoredPosition = _originAnchorPosition + direction * compressOffset;
                    break;
            }
        }

        private void ChangeStretch(Vector2 direction, float value)
        {
            switch (adaptChangeMode)
            {
                case AdaptChangeModes.Move:
                    // 移动模式：上下同时移动，保持间距不变
                    _rectTransform.offsetMin = _originOffsetMin + Vector2.up * direction.y * value;
                    _rectTransform.offsetMax = _originOffsetMax + Vector2.up * direction.y * value;
                    break;

                case AdaptChangeModes.Expand:
                    if (direction == Vector2.down) // (0,-1) 向下延展，上边不动
                    {
                        // 上边保持不变，下边向下延伸
                        _rectTransform.offsetMax = _originOffsetMax;
                        _rectTransform.offsetMin = _originOffsetMin + Vector2.up * (-value);
                    }
                    else if (direction == Vector2.up) // (0,1) 向上延展，下边不动
                    {
                        // 下边保持不变，上边向上延伸
                        _rectTransform.offsetMin = _originOffsetMin;
                        _rectTransform.offsetMax = _originOffsetMax + Vector2.up * value;
                    }

                    break;

                case AdaptChangeModes.Compress:
                    if (direction == Vector2.down) // (0,-1) 向下压缩，下边不动
                    {
                        // 下边保持不变，上边向下移动（减少上方空间）
                        float newMaxY = Mathf.Min(_originOffsetMax.y, _originOffsetMax.y - value);
                        _rectTransform.offsetMin = _originOffsetMin; // 下边不变
                        _rectTransform.offsetMax = new Vector2(_originOffsetMax.x, newMaxY);
                    }
                    else if (direction == Vector2.up) // (0,1) 向上压缩，上边不动
                    {
                        // 上边保持不变，下边向上移动（减少下方空间）
                        float newMinY = Mathf.Max(_originOffsetMin.y, _originOffsetMin.y + value);
                        _rectTransform.offsetMax = _originOffsetMax; // 上边不变
                        _rectTransform.offsetMin = new Vector2(_originOffsetMin.x, newMinY);
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// 适配方向类型
    /// </summary>
    public enum AdaptOrientationModes
    {
        Top,
        Down
    }

    /// <summary>
    /// 适配变化类型
    /// </summary>
    public enum AdaptChangeModes
    {
        Move, //移动
        Expand, //延展
        Compress, //压缩
    }
}