using DG.Tweening;
using UnityEngine;

public class ScaleSelf : MonoBehaviour
{
    [SerializeField] private Vector3 target;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private LoopType loopType = LoopType.Yoyo;
    [SerializeField] private Ease ease = Ease.Flash;
    [SerializeField] private bool playInDisable = false;
    [SerializeField] private bool autoPlay = true;
    private Tweener _motionTweener;
    private Vector3 _originScale;
    private bool _isPlaying = false;

    private void Awake()
    {
        _originScale = transform.localScale;
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
        transform.localScale = _originScale;
        _motionTweener = transform.DOScale(target,duration)
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