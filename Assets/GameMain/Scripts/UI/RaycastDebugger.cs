using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using MergeSword;

public class RaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (!PointerInput.TryGetPointerDown(out Vector2 pointerPos))
            return;

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = pointerPos;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        foreach (var result in results)
        {
            Debug.Log("检测到对象: " + result.gameObject.name);
        }
    }
}