using UnityEngine;
using UnityEngine.Events;

namespace MergeSword
{
    public class ClickOutsideListener : MonoBehaviour
    {
        public RectTransform targetPanel;
        public UnityEvent onClickOutside;

        private void Update()
        {
            if (!targetPanel.gameObject.activeSelf)
                return;

            if (!PointerInput.TryGetPointerDown(out Vector2 pointerPos))
                return;

            // 手动 RaycastAll，不依赖 StandaloneInputModule.Process() 的执行顺序
            if (!IsPointerInside(targetPanel, pointerPos))
            {
                onClickOutside?.Invoke();
            }
        }

        private bool IsPointerInside(RectTransform rect, Vector2 pointerPos)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(
                rect,
                pointerPos,
                Burnicle.UI.UICamera
            );
        }
    }
}