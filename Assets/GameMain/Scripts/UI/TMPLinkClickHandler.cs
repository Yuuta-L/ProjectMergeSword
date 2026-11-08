using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MergeSword
{
    /// <summary>
    /// 仅处理 TMP 文本的 <link> 点击，不实现任何拖拽接口，
    /// 避免 EventTrigger 拦截拖动事件导致父级 ScrollRect 无法滚动。
    /// </summary>
    public class TMPLinkClickHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 链接被点击时触发，参数为链接 id 与点击位置。由项目层接管具体行为。
        /// </summary>
        public static event Action<string, Vector2> LinkClicked;

        [SerializeField] private TMP_Text _text;
        [SerializeField] private bool _forwardNonLinkClickToParent;

        public void Setup(TMP_Text text)
        {
            _text = text;
        }

        public void Setup(TMP_Text text, bool forwardNonLinkClickToParent)
        {
            _text = text;
            _forwardNonLinkClickToParent = forwardNonLinkClickToParent;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_text == null)
                return;

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, null);
            if (linkIndex >= 0)
            {
                var linkId = _text.textInfo.linkInfo[linkIndex].GetLinkID();
                eventData.Use();
                LinkClicked?.Invoke(linkId, eventData.position);
                return;
            }

            // 未命中 link：本组件已实现 IPointerClickHandler，会阻断 ExecuteHierarchy 向父级冒泡，
            // 需要手动把点击转发给父级，让父级（如技能选择项）正常响应。
            if (_forwardNonLinkClickToParent && _text.transform.parent != null)
            {
                ExecuteEvents.ExecuteHierarchy(_text.transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
