using UnityEngine;

/// <summary>
/// 镂空区域装饰：由 UIHollowOutImage 驱动，跟随镂空区域的位置和大小。
/// </summary>
public class UIHollowOutImageOrnament : MonoBehaviour
{
    /// <summary>在镂空区域基础上额外扩大的边距 (x:左 y:下 z:右 w:上)。</summary>
    public Vector4 padding = Vector4.zero;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 由 UIHollowOutImage 在镂空区域变化时调用。
    /// 当镂空区域宽高均 <= 0 时自动隐藏自身（不生成装饰）。
    /// </summary>
    public void SetBounds(Vector2 center, Vector2 size)
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        // 镂空区域不存在时隐藏装饰
        if (size.x <= 0f && size.y <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        size.x += padding.x + padding.z;
        size.y += padding.y + padding.w;

        _rectTransform.anchoredPosition = center;
        _rectTransform.sizeDelta = size;
    }
}
