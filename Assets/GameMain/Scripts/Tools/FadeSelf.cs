using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeSelf : MonoBehaviour
{
    [SerializeField] private float target;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private LoopType loopType = LoopType.Yoyo;
    [SerializeField] private Ease ease = Ease.Flash;
    [SerializeField] private bool playInDisable = false;
    [SerializeField] private bool autoPlay = true;
    private Tweener _motionTweener;
    private Image _image;
    private float _originValue;
    private bool _isPlaying = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _originValue = _image.color.a;
    }

    private void OnEnable()
    {
        if (autoPlay)
        {
            StartMotion();
        }
    }

    private void OnDisable()
    {
        if (!playInDisable)
        {
            StopMotion();
        }
    }
    
    [ContextMenu("Replay")]
    public void Replay()
    {
        StopMotion();
        StartMotion();
    }

    private void StartMotion()
    {
        if (_isPlaying)
        {
            return;
        }
        _isPlaying = true;
        _motionTweener?.Kill();
        _image.SetColorAlpha(_originValue);
        _motionTweener = _image.DOFade(target,duration)
            .SetLoops(-1, loopType)
            .SetEase(ease);
    }
    
    private void StopMotion()
    {
        _isPlaying = false;
        _motionTweener?.Kill();
        _motionTweener = null;
    }
}