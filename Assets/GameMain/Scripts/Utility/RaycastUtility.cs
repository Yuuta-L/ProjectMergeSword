using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MergeSword
{
    public static class RaycastUtility
    {
        private static RaycastHit2D[] _results;
        private static GraphicRaycaster _graphicRaycaster;
        private static PointerEventData _pointerData;
        private static List<RaycastResult> _raycastResults = new List<RaycastResult>();

        private static bool IsPointerOverUI(Vector2 position)
        {
            if (_pointerData == null)
                _pointerData = new PointerEventData(EventSystem.current);
            _pointerData.position = position;
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(_pointerData, _raycastResults);
            return _raycastResults.Count > 0;
        }

        /// <summary>
        /// 是否正在点击/触摸
        /// </summary>
        /// <returns></returns>
        public static bool IsMouseClick(int mouseButton = 0, bool ignoreUI = true)
        {
            if (mouseButton != 0)
                return Input.GetMouseButtonDown(mouseButton);

            if (!PointerInput.TryGetPointerDown(out Vector2 position))
                return false;

            if (!ignoreUI && IsPointerOverUI(position))
                return false;

            return true;
        }
        
        /// <summary>
        /// 获取当前鼠标/触摸屏幕坐标
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetCurrentMousePosition()
        {
            return PointerInput.Position;
        }
        
        /// <summary>
        /// 获取当前鼠标/触摸世界坐标
        /// </summary>
        public static Vector2 GetCurrentMouseWorldPosition()
        {
            var position = GetCurrentMousePosition();
            if (Camera.main == null)
            {
                return Vector2.zero;
            }

            return Camera.main.ScreenToWorldPoint(position);
        }

        /// <summary>
        /// 检查并尝试获取指定世界游戏物体
        /// </summary>
        public static T CheckCurrentMouseGameObject<T>(bool ignoreUI = false,Vector2? customPosition = null) where T : MonoBehaviour
        {
            if (!ignoreUI)
            {
                Vector2 pointerPos = customPosition ?? GetCurrentMousePosition();
                if (IsPointerOverUI(pointerPos))
                    return null;
            }

            if (_results == null)
            {
                _results = new RaycastHit2D[5];
            }
            
            var position = customPosition ?? GetCurrentMouseWorldPosition();
            var hitCount = Physics2D.RaycastNonAlloc(position, Vector2.zero, _results);
            if (hitCount > 0)
            {
                for (var i = 0; i < hitCount; i++)
                {
                    var hit = _results[i];
                    if (hit.transform == null)
                    {
                        continue;
                    }

                    var target = hit.transform.GetComponent<T>();
                    if (target != null)
                    {
                        return target;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查并尝试获取指定ui
        /// </summary>
        public static T CheckCurrentMouseUI<T>(Vector2? customPosition = null) where T : MonoBehaviour
        {
            var results = GetCurrentOverUI(customPosition);
            if (results != null)
            {
                foreach (var result in results)
                {
                    var target = result.gameObject.GetComponent<T>();
                    if (target != null)
                    {
                        return target;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取当前鼠标/触摸位置下所有ui元素
        /// </summary>
        public static List<RaycastResult> GetCurrentOverUI(Vector2? customPosition = null)
        {
            Vector2 position = customPosition ?? GetCurrentMousePosition();

            var pointerEventData = new PointerEventData(EventSystem.current) { position = position };
            var results = new List<RaycastResult>();
            if (_graphicRaycaster == null)
            {
                _graphicRaycaster = Burnicle.UI.transform.GetComponentInChildren<GraphicRaycaster>();
            }

            _graphicRaycaster.Raycast(pointerEventData, results);
            return results;
        }
    }
}