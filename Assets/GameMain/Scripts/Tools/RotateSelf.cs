using DG.Tweening;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    [SerializeField] private float aroundDuration = 1.5f;
    [SerializeField] private Axis axis = Axis.Forward;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool playInDisable = false;
    [SerializeField] private bool autoPlay = true;
    private Tweener _rotateTweener;
    private bool _isPlaying = false;
    private void OnEnable()
    {
        if (autoPlay)
        {
            StartRotate();
        }
    }

    private void OnDisable()
    {
        if (!playInDisable)
        {
            StopRotate();
        }
    }

    [ContextMenu("Replay")]
    public void Replay()
    {
        StopRotate();
        StartRotate();
    }

    private void StartRotate()
    {
        if (_isPlaying)
        {
            return;
        }
        
        _isPlaying = true;
        _rotateTweener?.Kill();
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        _rotateTweener = transform.DOLocalRotate(GetAxisValue(axis) * 360, aroundDuration,RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(ease);
    }
    
    private void StopRotate()
    {
        _isPlaying = false;
        _rotateTweener?.Kill();
        _rotateTweener = null;
    }

    private Vector3 GetAxisValue(Axis targetAxis)
    {
        var result = Vector3.zero;
        switch (targetAxis)
        {
            case Axis.Left:
                result = Vector3.left;
                break;
            case Axis.Right:
                result = Vector3.right;
                break;
            case Axis.Forward:
                result = Vector3.forward;
                break;
            case Axis.Back:
                result = Vector3.back;
                break;
            case Axis.Up:
                result = Vector3.up;
                break;
            case Axis.Down:
                result = Vector3.down;
                break;
        }

        return result;
    }
}

public enum Axis
{
    Left,
    Right,
    Forward,
    Back,
    Up,
    Down
}
