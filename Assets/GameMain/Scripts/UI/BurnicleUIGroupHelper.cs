using BurnicleFramework.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace MergeSword
{
    /// <summary>
    /// ui组位于不同的canvas中
    /// 1.ui组一般是少量有限的，方便动静分离
    /// 2.可以通过设置sortingorder对ui组排序
    /// 3.uiform不带canvas，而是依托于ui组
    /// </summary>
    [Obfuz.ObfuzIgnore]
    public class BurnicleUIGroupHelper : UIGroupHelperBase
    {
        private Canvas _cachedCanvas;
        
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth(int depth)
        {
            _cachedCanvas.overrideSorting = true;
            _cachedCanvas.sortingOrder = depth;
        }
        
        private void Awake()
        {
            _cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            
            // 根据UICamera是否存在选择不同的渲染模式
            if (Burnicle.UI.UICamera != null)
            {
                // Camera模式：UI有独立相机
                _cachedCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                _cachedCanvas.worldCamera = Burnicle.UI.UICamera;
            }
            else
            {
                // Overlay模式：UI直接渲染到屏幕
                _cachedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            
            var scaler = gameObject.GetOrAddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.matchWidthOrHeight = ScreenUtility.IsMatchHeight ? 1 : 0;
            scaler.referenceResolution = new Vector2(1080, 1920);
            ScreenUtility.CanvasSize = scaler.scaleFactor * scaler.referenceResolution;
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            var rectTransform = gameObject.GetOrAddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
    }
}