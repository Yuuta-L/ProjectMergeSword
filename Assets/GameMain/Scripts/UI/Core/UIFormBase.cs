using UnityEngine;
using GameFramework;
using DG.Tweening;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;
using Cysharp.Threading.Tasks;
using GameFramework.ObjectPool;
using UnityEngine.Events;

namespace MergeSword
{
    /// <summary>
    /// UI基类, 所有UI界面需继承此类
    /// </summary>
    public class UIFormBase : UIFormLogic, ISerializeFieldTool
    {
        [HideInInspector][SerializeField] 
        private SerializeFieldData[] _fields;
        /// <summary>
        /// 动画类型
        /// </summary>
        [HideInInspector][SerializeField] 
        private UIFormAnimationType _uiAnimationType = UIFormAnimationType.None;
        /// <summary>
        /// DOTween打开动画
        /// </summary>
        [HideInInspector][SerializeField] 
        private DOTweenSequence _openAnimation = null;
        /// <summary>
        /// DOTween关闭动画
        /// </summary>
        [HideInInspector][SerializeField] 
        private DOTweenSequence _closeAnimation = null;
        /// <summary>
        /// animation打开动画
        /// </summary>
        [HideInInspector][SerializeField] 
        private string _openAnimationName = null;
        /// <summary>
        /// animation关闭动画
        /// </summary>
        [HideInInspector][SerializeField] 
        private string _closeAnimationName = null;

        /// <summary>
        /// 关闭动画沿用反向开启动画
        /// </summary>
        [HideInInspector][SerializeField] 
        private bool _reverseOpenAnimAsClose = true;


        public SerializeFieldData[] SerializeFieldArr
        {
            get => _fields;
            set => _fields = value;
        }

        //界面序列号
        public int SerialId => UIForm.SerialId;

        //界面可交互性
        public bool Interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup!.interactable = value;
        }

        protected UIOpenParams Params;
        private Animation _animationComponent;
        private CanvasGroup _canvasGroup = null;
        private UIItemPool _itemPool;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Array.Clear(_fields, 0, _fields.Length);
            _itemPool = new UIItemPool(gameObject.GetInstanceID().ToString());
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            RectTransform tf = GetComponent<RectTransform>();
            tf.anchorMin = Vector2.zero;
            tf.anchorMax = Vector2.one;
            tf.anchoredPosition = Vector2.zero;
            tf.sizeDelta = Vector2.zero;
            tf.localPosition = Vector3.zero;
            if (_uiAnimationType == UIFormAnimationType.Animation)
            {
                _animationComponent = GetComponent<Animation>();
            }

            InitLocalization();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Params = userData as UIOpenParams;
            Interactable = false;
            PlayOpenUIAnimation(OnOpenCompletely);
            Params.OpenCallback?.Invoke(this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            UIExtension.CheckScreenDim();
        }

        protected override void OnResume()
        {
            base.OnResume();
            UIExtension.CheckScreenDim();
        }

        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            transform.SetSiblingIndex(depthInUIGroup - 1);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (!isShutdown)
            {
                Params.CloseCallback?.Invoke(this);
                ReferencePool.Release(Params);
            }

            UnspawnAllItemObjects();
            base.OnClose(isShutdown, userData);
        }

        private void OnDestroy()
        {
            DestroyAllItemPool();
        }

        private void UnspawnAllItemObjects()
        {
            _itemPool.UnspawnAllItemObjects();
        }

        private void DestroyAllItemPool()
        {
            _itemPool.DestroyAllItemPool();
        }

        /// <summary>
        /// 从对象池获取一个Item (界面关闭时会自动Unspawn)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemTemple">Item实例化模板</param>
        /// <param name="instanceRoot">Item实例化到根节点</param>
        /// <param name="autoReleaseInterval"></param>
        /// <param name="capacity">对象池容量</param>
        /// <param name="expireTime">对象过期时间(过期后自动销毁)</param>
        /// <returns></returns>
        protected T SpawnItem<T>(GameObject itemTemple, Transform instanceRoot, float autoReleaseInterval = 5f,
            int capacity = 50, float expireTime = 50) where T : UIItemBase
        {
            return _itemPool.SpawnItem<T>(itemTemple, instanceRoot, autoReleaseInterval, capacity, expireTime);
        }

        /// <summary>
        /// 从对象池回收Item
        /// </summary>
        protected void UnspawnItem<T>(T item) where T : UIItemBase
        {
            _itemPool.UnspawnItem<T>(item);
        }

        /// <summary>
        /// 回收所有item
        /// </summary>
        protected void UnspawnAllItem(GameObject itemTemple)
        {
            _itemPool.UnspawnAllItem(itemTemple);
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
                else if (t.TryGetComponent<Text>(out var textCom))
                {
                    textCom.text = Burnicle.Localization.GetString(t.Key);
                }
            }
        }

        [Obfuz.ObfuzIgnore]
        public void Close(bool playAnimation = true, int delay = 0)
        {
            Interactable = false;
            if (delay > 0)
            {
                // 使用 UnscaledDeltaTime 确保在 Time.timeScale = 0（暂停）时延迟仍然生效
                UniTask.Delay(delay, DelayType.UnscaledDeltaTime).ContinueWith(() =>
                {
                    RealClose(playAnimation);
                });
            }
            else
            {
                RealClose(playAnimation);
            }
        }

        private void RealClose(bool playAnimation)
        {
            if (playAnimation)
            {
                PlayCloseUIAnimation(_Close);
                UIExtension.CloseScreenDim(UIForm.SerialId,true);
            }
            else
            {
                _Close();
                UIExtension.CloseScreenDim(UIForm.SerialId,false);
            }
        }
        
        protected void PlayCloseUIAnimation(UnityAction onAnimComplete)
        {
            if (_uiAnimationType == UIFormAnimationType.DoTween)
            {
                //如果关闭动画未配置, 默认将打开动画倒放作为关闭动画
                if (_closeAnimation != null)
                {
                    var anim = _closeAnimation.DOPlay();
                    if (onAnimComplete != null) anim.OnComplete(onAnimComplete.Invoke);
                    return;
                }
                else if (_reverseOpenAnimAsClose && _openAnimation != null)
                {
                    var anim = _openAnimation.DORewind();
                    if (onAnimComplete != null) anim.OnComplete(onAnimComplete.Invoke);
                    return;
                }
            }
            else if (_uiAnimationType == UIFormAnimationType.Animation)
            {
                if (_animationComponent != null && !string.IsNullOrWhiteSpace(_closeAnimationName) &&
                    _animationComponent.GetClip(_closeAnimationName))
                {
                    _animationComponent.PlayForward(_closeAnimationName, onAnimComplete.Invoke);
                    return;
                }
                else if (_reverseOpenAnimAsClose && _animationComponent != null &&
                         !string.IsNullOrEmpty(_openAnimationName) &&
                         _animationComponent.GetClip(_openAnimationName))
                {
                    _animationComponent.PlayBackward(_openAnimationName, onAnimComplete.Invoke);
                    return;
                }
            }

            onAnimComplete?.Invoke();
        }

        protected void PlayOpenUIAnimation(UnityAction onAnimComplete)
        {
            if (_uiAnimationType == UIFormAnimationType.DoTween && _openAnimation != null)
            {
                var anim = _openAnimation.DOPlay();
                if (onAnimComplete != null) anim.OnComplete(onAnimComplete.Invoke);
                return;
            }
            
            if (_uiAnimationType == UIFormAnimationType.Animation && _animationComponent != null && 
                     !string.IsNullOrWhiteSpace(_openAnimationName) && _animationComponent.GetClip(_openAnimationName))
            {
                if (onAnimComplete != null)
                {
                    _animationComponent.PlayForward(_openAnimationName, onAnimComplete.Invoke);
                    return;
                }
                else
                {
                    _animationComponent.PlayForward(_openAnimationName);
                    return;
                }
            }
            
            onAnimComplete?.Invoke();
        }

        private void _Close()
        {
            Burnicle.UI.CloseUIForm(this.UIForm);
            OnCloseCompletely();
        }
        
        [Obfuz.ObfuzIgnore]
        public virtual void OnClickClose()
        {
            Close();
        }

        [Obfuz.ObfuzIgnore]
        public void ClickUIButton(string bt_tag)
        {
            OnButtonClick(this, bt_tag);
        }

        [Obfuz.ObfuzIgnore]
        public void ClickUIButton(Button btSelf)
        {
            OnButtonClick(this, btSelf);
        }

        protected virtual void OnButtonClick(object sender, string btId)
        {
            Params.ButtonClickCallback?.Invoke(sender, btId);
        }

        protected virtual void OnButtonClick(object sender, UnityEngine.UI.Button btSelf)
        {
            btSelf.DoClickAnim();
        }

        /// <summary>
        /// UI打开动画完成时回调
        /// </summary>
        protected virtual void OnOpenCompletely()
        {
            Interactable = true;
        }
        
        /// <summary>
        /// UI关闭动画完成时回调
        /// </summary>
        protected virtual void OnCloseCompletely()
        {
        }
    }

}