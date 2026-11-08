using UnityEngine;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    /// <summary>
    /// uGUI 界面组辅助器。
    /// </summary>
    public class UGuiGroupHelper : UIGroupHelperBase
    {
        public const int DepthFactor = 10000;

        private int m_Depth = 0;

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth(int depth)
        {
            m_Depth = depth;
        }

        private void Awake()
        {
            gameObject.GetOrAddComponent<RectTransform>();
        }

        private void Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.SetLocalPositionZ(0);
        }
    }
}
