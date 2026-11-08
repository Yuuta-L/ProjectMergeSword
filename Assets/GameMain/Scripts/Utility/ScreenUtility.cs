using System.Runtime.CompilerServices;
using UnityEngine;

namespace MergeSword
{
    public static class ScreenUtility
    {
        // 静态缓存数组, 避免逐帧分配
        private static readonly Vector3[] _corners4 = new Vector3[4];
        private static readonly Vector3[] _corners8 = new Vector3[8];

        private static bool? _isMatchHeight;
        private static Vector2? canvasSize; 

        public static bool IsMatchHeight
        {
            get
            {
                if (_isMatchHeight == null)
                {
                    var designResolution = new Vector2(1080,1920);
                    _isMatchHeight = (float)Screen.width / Screen.height > (float)designResolution.x / designResolution.y;
                }

                return _isMatchHeight.Value;
            }
        }

        //经过缩放后的屏幕大小
        public static Vector2 CanvasSize
        {
            get => canvasSize ?? new Vector2(Screen.width, Screen.height);
            set => canvasSize = value;
        }

        public static Vector2 ScreenSize()
        {
            return new Vector2(Screen.width, Screen.height);
        }

        public static float ScreenWidth()
        {
            return Screen.width;
        }
        
        public static float ScreenHeight()
        {
            return Screen.height;
        }
        
        private static Camera MainCamera => Burnicle.Scene.MainCamera;
        private static Camera UICamera => Burnicle.UI.UICamera;

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        public static Vector2 WorldToScreenPos(Vector3 worldPosition)
        {
            return MainCamera.WorldToScreenPoint(worldPosition);
        }

        /// <summary>
        /// 屏幕坐标转世界坐标（需要指定距离相机的深度）
        /// </summary>
        public static Vector3 ScreenToWorldPos(Vector2 screenPosition, float distanceFromCamera)
        {
            var screenPos = new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera);
            return MainCamera.ScreenToWorldPoint(screenPos);
        }

        /// <summary>
        /// 屏幕坐标转UI锚点坐标（Canvas中心为原点）
        /// </summary>
        public static Vector2 ScreenToRectPos(Vector2 screenPosition, RectTransform targetRect = null)
        {
            if (targetRect == null)
            {
                return screenPosition - ScreenSize() * 0.5f;
            }
            
            // Camera模式：通过UI相机转换
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPosition, UICamera, out var localPoint);
            return localPoint;
        }

        /// <summary>
        /// Rect的屏幕坐标
        /// </summary>
        public static Vector2 RectInScreenPos(RectTransform sourceRect)
        {
            return RectTransformUtility.WorldToScreenPoint(UICamera, sourceRect.position);
        }

        /// <summary>
        /// 世界坐标转UI锚点坐标
        /// </summary>
        public static Vector2 WorldToRectPos(Vector3 worldPosition, RectTransform targetRect = null)
        {
            var screenPoint = WorldToScreenPos(worldPosition);
            return ScreenToRectPos(screenPoint, targetRect);
        }

        /// <summary>
        /// 世界坐标转视口坐标
        /// </summary>
        public static Vector2 WorldToViewportPoint(Vector3 worldPosition)
        {
            return MainCamera.WorldToViewportPoint(worldPosition);
        }

        /// <summary>
        /// 视口坐标转世界坐标
        /// </summary>
        public static Vector3 ViewportToWorldPoint(Vector2 viewportPosition, float distanceFromCamera)
        {
            var viewportPos = new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera);
            return MainCamera.ViewportToWorldPoint(viewportPos);
        }

        // ====== 教程系统相关坐标转换 ======

        /// <summary>
        /// 获取 RectTransform 在屏幕空间中的包围盒 (Overlay Canvas 模式, camera=null)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect GetRectScreenRect(RectTransform rt)
        {
            rt.GetWorldCorners(_corners4);
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 sp = RectTransformUtility.WorldToScreenPoint(null, _corners4[i]);
                if (sp.x < minX) minX = sp.x;
                if (sp.y < minY) minY = sp.y;
                if (sp.x > maxX) maxX = sp.x;
                if (sp.y > maxY) maxY = sp.y;
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 获取世界空间 Bounds 在屏幕空间中的包围盒 (使用 MainCamera)
        /// </summary>
        public static Rect GetBoundsScreenRect(Bounds bounds)
        {
            var cam = MainCamera;
            if (cam == null) return new Rect();

            var c = bounds;
            _corners8[0] = c.min;
            _corners8[1] = new Vector3(c.max.x, c.min.y, c.min.z);
            _corners8[2] = new Vector3(c.min.x, c.max.y, c.min.z);
            _corners8[3] = new Vector3(c.max.x, c.max.y, c.min.z);
            _corners8[4] = new Vector3(c.min.x, c.min.y, c.max.z);
            _corners8[5] = new Vector3(c.max.x, c.min.y, c.max.z);
            _corners8[6] = new Vector3(c.min.x, c.max.y, c.max.z);
            _corners8[7] = c.max;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(_corners8[i]);
                if (sp.x < minX) minX = sp.x;
                if (sp.y < minY) minY = sp.y;
                if (sp.x > maxX) maxX = sp.x;
                if (sp.y > maxY) maxY = sp.y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 判断屏幕坐标点是否在 RectTransform 内 (UI Camera 模式)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsScreenPoint(RectTransform rt, Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, UICamera);
        }

        /// <summary>
        /// 屏幕坐标转 UI 本地坐标 (UI Camera 模式, 带输出参数版本)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ScreenToLocalPoint(Vector2 screenPosition, RectTransform targetRect, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, screenPosition, UICamera, out localPoint);
        }
    }
}