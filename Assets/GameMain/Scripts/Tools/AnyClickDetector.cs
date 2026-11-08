using System.Collections.Generic;
using BurnicleFramework.Generated;
using EndlessAbyss;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnyClickDetector : MonoBehaviour
{
    [Header("点击设置")]
    [Tooltip("是否只检测非UI区域的点击（忽略按钮、滚动条等）")]
    public bool ignoreUI = true;

    private PointerEventData _pointerData;
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    void Update()
    {
        if (!PointerInput.TryGetPointerDown(out Vector2 clickPosition))
            return;

        // 忽略 UI 元素的点击（防止点击按钮时重复触发）
        if (ignoreUI && EventSystem.current != null && IsPointerOverUI(clickPosition))
        {
            Debug.Log("点击了 UI 元素，忽略本次动作");
            return;
        }

        OnAnyClick(clickPosition);
    }

    private bool IsPointerOverUI(Vector2 pointerPos)
    {
        if (_pointerData == null)
            _pointerData = new PointerEventData(EventSystem.current);
        _pointerData.position = pointerPos;
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerData, _raycastResults);
        return _raycastResults.Count > 0;
    }

    void OnAnyClick(Vector3 screenPos)
    {
    }
}
