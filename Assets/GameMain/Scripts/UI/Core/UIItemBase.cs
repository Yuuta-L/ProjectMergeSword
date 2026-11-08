using System;
using BurnicleFramework.Runtime;
using UnityEngine;

namespace MergeSword
{
    public abstract class UIItemBase : MonoBehaviour
    {
        private UIItemObject _container;

        public UIItemObject Container => _container;
        protected virtual void Awake()
        {
            InitLocalization();
        }

        public void SetContainer(UIItemObject container)
        {
            _container = container;
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
            transform.localScale = Vector3.one;
        }

        public virtual void OnSpawn()
        {
            gameObject.SetActive(true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        public virtual void OnUnspawn()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 更新界面中静态文本的多语言文字
        /// </summary>
        public virtual void InitLocalization()
        {
            UIStringKey[] texts = GetComponentsInChildren<UIStringKey>(true);
            foreach (var t in texts)
            {
                if (t.TryGetComponent<TMPro.TextMeshProUGUI>(out var textMeshCom))
                {
                    textMeshCom.text = Burnicle.Localization.GetString(t.Key);
                }
                else if (t.TryGetComponent<UnityEngine.UI.Text>(out var textCom))
                {
                    textCom.text = Burnicle.Localization.GetString(t.Key);
                }
            }
        }
    }
}