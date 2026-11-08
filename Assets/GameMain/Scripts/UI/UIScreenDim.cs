using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using UnityEngine;
using BurnicleFramework.Runtime;
using UnityEngine.UI;

namespace MergeSword
{
    public class UIScreenDim : MonoBehaviour
    {
        private CanvasGroup m_CanvasGroup;
        private RectTransform m_Model;
        private readonly List<int> m_NeedShowUIForms = new List<int>();
        private bool m_Active;
        private Tweener m_AnimationTweener;

        private void Awake()
        {
            m_CanvasGroup = transform.GetComponent<CanvasGroup>();
            m_Model = (RectTransform)transform.Find("Model");
            m_Model.gameObject.SetActive(false);
            m_Active = false;
        }

        public void Start()
        {
            Burnicle.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId,OnOpenUIFormSuccess);
            Burnicle.Event.Subscribe(CloseUIFormCompleteEventArgs.EventId,OnCloseUIFormComplete);
        }
        
        //当界面完全打开时，再进行一次排序
        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            if (e is OpenUIFormSuccessEventArgs ne)
            {
                if (m_NeedShowUIForms.Contains(ne.UIForm.SerialId))
                {
                    CheckLayer();
                }
            }
        }
        
        //当界面以其他方式关闭时的保险
        private void OnCloseUIFormComplete(object sender, GameEventArgs e)
        {
            if (e is CloseUIFormCompleteEventArgs ne)
            {
                Hide(ne.SerialId, false);
            }
        }
        
        public void Show(int serialId)
        {
            if (!m_NeedShowUIForms.Contains(serialId))
            {
                m_NeedShowUIForms.Add(serialId);
            }
        }

        public void Hide(int serialId,bool fade)
        {
            if (m_NeedShowUIForms.Contains(serialId))
            {
                m_NeedShowUIForms.Remove(serialId);
                _Hide(fade);
                CheckLayer();
            }
        }

        public void CheckLayer()
        {
            if (m_NeedShowUIForms.Count <= 0)
            {
                return;
            }
            
            var targetGroupIndex = 0;
            var targetFormIndex = 0;
            UIForm targetUIForm = null;
            foreach (var serialId in m_NeedShowUIForms)
            {
                var uiForm = Burnicle.UI.GetUIForm(serialId);
                if (uiForm == null || !uiForm.Logic.Visible)
                {
                    continue;
                }

                var groupDepth = uiForm.UIGroup.Depth;
                var formIndex = uiForm.transform.GetSiblingIndex();
                if (targetUIForm == null || groupDepth > targetGroupIndex || formIndex > targetFormIndex)
                {
                    targetGroupIndex = groupDepth;
                    targetFormIndex = formIndex;
                    targetUIForm = uiForm;
                }
            }

            if (targetUIForm != null)
            {
                transform.SetParent(targetUIForm.transform,false);
                transform.SetAsFirstSibling();
                // m_Model.anchorMin = Vector2.zero;
                // m_Model.anchorMax = Vector2.one;
                // m_Model.anchoredPosition = Vector2.zero;
                _Show();
            }
        }
        
        private void _Show()
        {
            if (m_Active)
            {
                return;
            }

            m_Active = true;
            m_AnimationTweener?.Kill();
            m_CanvasGroup.alpha = 0f;
            m_Model.gameObject.SetActive(true);
            m_AnimationTweener = m_CanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }

        private void _Hide(bool fade)
        {
            if (!m_Active || m_NeedShowUIForms.Count > 0)
            {
                return;
            }
            
            m_Active = false;
            if (fade)
            {
                m_AnimationTweener?.Kill();
                m_AnimationTweener = m_CanvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
                {
                    m_Model.gameObject.SetActive(false);
                }).SetUpdate(true);
            }
            else
            {
                m_CanvasGroup.alpha = 0f;
                m_Model.gameObject.SetActive(false);
            }
            //m_Model.transform.SetParent(transform);
        }

        public static UIScreenDim CreateScreenDim(Transform tf)
        {
            var root = new GameObject("Screen Dim");
            var rootRect = root.AddComponent<RectTransform>();
            root.AddComponent<CanvasGroup>();
            rootRect.SetParent(tf,false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.localPosition = Vector3.zero;
            rootRect.localScale = Vector3.one;
            
            var model = new GameObject("Model");
            var modelRect = model.AddComponent<RectTransform>();
            modelRect.SetParent(root.transform,false);
            modelRect.anchorMin = Vector2.zero;
            modelRect.anchorMax = Vector2.one;
            modelRect.anchoredPosition = Vector2.zero;
            modelRect.sizeDelta = Vector2.zero;
            modelRect.localPosition = Vector3.zero;
            modelRect.localScale = Vector3.one;
            var dim = model.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.8f);
            
            return root.AddComponent<UIScreenDim>();
        }
    }
}