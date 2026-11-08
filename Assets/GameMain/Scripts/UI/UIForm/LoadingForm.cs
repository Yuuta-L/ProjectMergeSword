using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MergeSword
{
    /// <summary>
    /// 加载界面：只负责进度显示，具体加载流程由调用方驱动。
    /// </summary>
    public class LoadingForm : UIFormBase
    {
        [SerializeField] private TextMeshProUGUI varProgressText = null;

        private float _currentProgress;
        private Tweener _progressTweener;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            SetProgress(0f, true);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            _progressTweener?.Kill();
        }

        public void SetProgress(float value, bool immediately = false)
        {
            _progressTweener?.Kill();

            if (immediately)
            {
                _currentProgress = value;
                if (varProgressText != null)
                {
                    varProgressText.text = $"{value * 100:F1}%";
                }
                return;
            }

            var start = _currentProgress;
            _progressTweener = DOTween.To(() => start, x =>
            {
                _currentProgress = x;
                if (varProgressText != null)
                {
                    varProgressText.text = $"{x * 100:F1}%";
                }
            }, value, 0.5f).SetEase(Ease.Linear).SetUpdate(true);
        }
    }
}
