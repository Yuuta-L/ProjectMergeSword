using DG.Tweening;
using UnityEngine;

public class MoveSelf : MonoBehaviour
{
    [SerializeField] private Vector3 target;
    [SerializeField] private bool isLocal;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private LoopType loopType = LoopType.Yoyo;
    [SerializeField] private Ease ease = Ease.Flash;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool runInDisable = true;
    private Tweener _moveTweener;
    private Vector3 _originPosition;
    private bool _moving = false;

    private void Awake()
    {
        _originPosition = isLocal ? transform.localPosition : transform.position;
    }

    private void OnEnable()
    {
        if (autoPlay)
        {
            StartMove();
        }
    }

    private void OnDisable()
    {
        if (!runInDisable)
        {
            StopMove();
        }
    }


    private void StartMove()
    {
        if (_moving)
        {
            return;
        }
        _moving = true;
        _moveTweener?.Kill();
        if (isLocal)
        {
            transform.localPosition = _originPosition;
            _moveTweener = transform.DOLocalMove(target,moveDuration)
                .SetLoops(-1, loopType)
                .SetEase(ease);
        }
        else
        {
            transform.position = _originPosition;
            _moveTweener = transform.DOMove(target,moveDuration)
                .SetLoops(-1, loopType)
                .SetEase(ease);
        }
        
    }
    
    private void StopMove()
    {
        _moving = false;
        _moveTweener?.Kill();
        _moveTweener = null;
    }
}