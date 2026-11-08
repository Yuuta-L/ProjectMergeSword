using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 贝塞尔曲线投射动画：物体沿三次贝塞尔曲线从近处（大）飞向远处（小），
/// 通过缩放模拟近大远小的透视纵深感，全程在2D平面运动。
/// </summary>
public class BezierProjectile : MonoBehaviour
{
    [Header("动态目标 (优先级高于固定坐标)")]
    [SerializeField] Transform _startTransform;
    [SerializeField] Transform _endWorldTransform;
    [SerializeField] Vector3 _endWorldOffset;

    [Header("路径 (屏幕坐标)")]
    [SerializeField] Vector2 _startPoint;
    [SerializeField] Vector2 _endPoint;
    [SerializeField] float _autoArcHeight = 150f;

    [Header("随机弧度")]
    [SerializeField] bool _randomArc = true;
    [SerializeField] float _randomArcMin = 80f;
    [SerializeField] float _randomArcMax = 300f;

    [Header("透视缩放")]
    [SerializeField] float _startScale = 2.5f;
    [SerializeField] float _endScale = 0.4f;

    [Header("自转")]
    [SerializeField] float _spinAngle = 720f;

    [Header("运动")]
    [SerializeField] bool _useDuration = false;
    [SerializeField] float _duration = 0.5f;
    [SerializeField] float _speed = 800f;
    [SerializeField] Ease _ease = Ease.InSine;

    [Header("回调")]
    [SerializeField] UnityEvent _onComplete;

    private Tween _tween;

    [ContextMenu("Launch")]
    public void Launch()
    {
        Vector2 start, end;
        if (_startTransform != null && _endWorldTransform != null)
        {
            start = GetScreenPoint(_startTransform);
            Vector3 worldEnd = _endWorldTransform.position + _endWorldOffset;
            end = WorldToCanvasPoint(worldEnd);
        }
        else
        {
            start = _startPoint;
            end = _endPoint;
        }

        Vector2 cp1, cp2;
        AutoComputeControlPoints(start, end, out cp1, out cp2);
        float dur = _useDuration ? _duration : BezierUtility.EstimateLength(start, cp1, cp2, end, 32) / Mathf.Max(_speed, 0.01f);
        Launch(start, end, cp1, cp2, dur, _startScale, _endScale);
    }

    public void Launch(Vector2 start, Vector2 end, Vector2 ctrl1, Vector2 ctrl2,
        float duration, float startScale, float endScale)
    {
        _tween?.Kill();

        float depthRatio = startScale / Mathf.Max(endScale, 0.001f);

        transform.position = start;
        transform.localScale = Vector3.one * startScale;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);

        _tween = DOTween.To(
            () => 0f,
            t =>
            {
                transform.position = BezierUtility.CalculatePoint(start, ctrl1, ctrl2, end, t);
                // 透视缩放: scale ∝ 1/distance
                float s = startScale / (1f + (depthRatio - 1f) * t);
                transform.localScale = new Vector3(s, s, 1f);
                // 自转
                if (_spinAngle != 0f)
                    transform.rotation = Quaternion.Euler(0f, 0f, _spinAngle * t);
            },
            1f,
            duration
        ).SetEase(_ease).OnComplete(() =>
        {
            _onComplete?.Invoke();
        });
    }

    public void Kill()
    {
        _tween?.Kill();
        _tween = null;
    }

    private void OnDestroy()
    {
        Kill();
    }

    private void AutoComputeControlPoints(Vector2 start, Vector2 end, out Vector2 cp1, out Vector2 cp2)
    {
        float arc = _randomArc ? Random.Range(_randomArcMin, _randomArcMax) : _autoArcHeight;
        BezierUtility.ComputeArcControlPoints(start, end, arc, out Vector3 c1, out Vector3 c2);
        cp1 = c1;
        cp2 = c2;
    }

    private static Vector2 GetScreenPoint(Transform t)
    {
        var rt = t as RectTransform;
        if (rt != null)
        {
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return rt.position;
        }
        return Camera.main != null ? (Vector2)Camera.main.WorldToScreenPoint(t.position) : t.position;
    }

    private Vector2 WorldToCanvasPoint(Vector3 worldPos)
    {
        if (Camera.main == null) return worldPos;
        Vector2 screenPt = Camera.main.WorldToScreenPoint(worldPos);
        // Convert screen pixels to canvas-space coordinates
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return screenPt;
        var canvasRt = canvas.transform as RectTransform;
        if (canvasRt == null) return screenPt;
        return new Vector2(
            screenPt.x * canvasRt.rect.width / Screen.width,
            screenPt.y * canvasRt.rect.height / Screen.height
        );
    }
}
