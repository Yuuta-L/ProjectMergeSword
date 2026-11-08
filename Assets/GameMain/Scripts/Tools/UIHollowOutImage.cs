using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 镂空遮罩
/// </summary>
public class UIHollowOutImage : Graphic, ICanvasRaycastFilter
{
    public float Radius = 10f;//内切圆半径 图片的一半差不多就是一个圆了 这里相当于图片十分之一的长度
    public int TriangleNum = 6;//每个扇形三角形个数 个数越大弧度越平滑
 
    [Header("镂空目标 (外部传入, 仅用于序列化调试)")]
    public RectTransform inner_trans;

    [Header("镂空目标额外内边距")]
    public Vector4 padding = Vector4.zero; // x:左 y:下 z:右 w:上

    [Header("是否实时刷新")]
    [Space(25)]
    public bool realtimeRefresh;
    [Header("是否显示镂空")]
    [Space(25)]
    public bool ShowHollowOut = true;
    public UIHollowOutImageOrnament Ornament;
    
    private RectTransform outer_trans;//背景区域
    private Vector2 inner_rt;//镂空区域的右上角坐标 (outer_trans 本地空间)
    private Vector2 inner_lb;//镂空区域的左下角坐标 (outer_trans 本地空间)
    private Vector2 outer_rt;//背景区域的右上角坐标
    private Vector2 outer_lb;//背景区域的左下角坐标

    // ---- 新的镂空区域定义 ----
    private Rect _hollowRect; // outer_trans 本地空间的镂空矩形
    private GameObject _trackTarget; // 需要实时跟踪的世界物体
    private bool _useRectMode; // 使用 Rect 模式而非 inner_trans

    private readonly List<Vector2> _commonPointOutside = new List<Vector2>(16);
    private readonly List<Vector2> _commonPoint = new List<Vector2>(16);
    private static readonly float[] DegreeDeltaList = { Mathf.PI, Mathf.PI / 2, 0, Mathf.PI * 1.5f };
    private readonly List<Vector2> _fanVerts = new List<Vector2>(16);
    private Vector3 _currentOffset; //扩大偏移

    private Tweener _tweener;

    protected override void Awake()
    {
        base.Awake();
        outer_trans = GetComponent<RectTransform>();
#if UNITY_EDITOR
        realtimeRefresh = true;
#endif
        CalcBounds();
    }

    /// <summary>
    /// 镂空目标 (RectTransform 方式, 向后兼容)
    /// </summary>
    public void SetHollowOut(RectTransform target)
    {
        inner_trans = target;
        _useRectMode = false;
        _trackTarget = null;
        CalcBounds();
        SetAllDirty();
    }

    /// <summary>
    /// 直接传入坐标+大小来设置镂空区域 (坐标在 UIHollowOutImage 的本地空间)。
    /// </summary>
    public void SetHollowOutRect(Vector2 center, Vector2 size)
    {
        _useRectMode = true;
        _trackTarget = null;
        inner_trans = null;
        _hollowRect = new Rect(center - size * 0.5f, size);
        CalcBounds();
        SetAllDirty();
    }

    /// <summary>
    /// 传入 GameObject 自动判断类型:
    /// - UI 物体: 取其 RectTransform 作为镂空目标
    /// - 世界物体: 计算该物体 (包括子物体整个) 在屏幕上的位置和大小作为镂空区域, 每帧跟踪
    /// </summary>
    public void SetHollowOutTarget(GameObject target)
    {
        if (target == null)
        {
            ShowHollowOut = false;
            _useRectMode = false;
            _trackTarget = null;
            inner_trans = null;
            CalcBounds();
            SetAllDirty();
            return;
        }

        ShowHollowOut = true;
        // 检测是否为 UI 物体 (有 RectTransform 且在 Canvas 下)
        var rt = target.transform as RectTransform;
        if (rt != null && target.GetComponentInParent<Canvas>() != null)
        {
            _useRectMode = false;
            _trackTarget = null;
            inner_trans = rt;
            CalcBounds();
            SetAllDirty();
            return;
        }

        // 世界物体
        _useRectMode = true;
        inner_trans = null;
        _trackTarget = target;
        UpdateWorldTargetBounds();
        SetAllDirty();
    }

    /// <summary>设置镂空缩放动画偏移 (可配合 SetHollowOutTarget 使用)。</summary>
    public Tweener SetAnimationOffset(float startOffset, float endOffset = 0f, float duration = 0.4f, Ease ease = Ease.OutQuad)
    {
        _tweener?.Kill();
        realtimeRefresh = true;
        _tweener = DOTween.To(() => startOffset, (x) =>
        {
            _currentOffset = Vector3.one * x;
        }, endOffset, duration).OnComplete(() =>
        {
            // 动画结束后关闭 realtimeRefresh 标记 (仅当未在跟踪世界物体时)
            if (_trackTarget == null)
                realtimeRefresh = false;
        }).SetUpdate(true).SetEase(ease);
        return _tweener;
    }

    public Tweener SetHollowOutAnimation(RectTransform _target, float startOffset = 800f, float endOffset = 0f, float duration = 0.4f, Ease ease = Ease.OutQuad)
    {
        inner_trans = _target;
        _useRectMode = false;
        _trackTarget = null;
        CalcBounds();
        SetAllDirty();
        _tweener?.Kill();
        realtimeRefresh = true;
        _tweener = DOTween.To(() => startOffset, (x) =>
        {
            _currentOffset = Vector3.one * x;
        }, endOffset, duration).OnComplete(() =>
        {
            if (_trackTarget == null)
                realtimeRefresh = false;
        }).SetUpdate(true).SetEase(ease);
        return _tweener;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (inner_trans == null && !_useRectMode)
        {
            base.OnPopulateMesh(vh);
            return;
        }

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vh.Clear();

        float w = Mathf.Abs(inner_lb.x - inner_rt.x);
        float h = Mathf.Abs(inner_lb.y - inner_rt.y);
        float halfW = w / 2;
        float halfH = h / 2;

        // 圆角半径计算
        float radius = Radius > 0f ? w / Radius : halfW;
        if (radius > halfW) radius = halfW;
        if (radius > halfH) radius = halfH;
        if (radius < 0f) radius = 0f;
        int triCount = Mathf.Max(1, TriangleNum);

        // ---- 外边框 4 个顶点 ----
        AddQuadVert(vh, ref vert, outer_lb.x, outer_lb.y);
        AddQuadVert(vh, ref vert, outer_lb.x, outer_rt.y);
        AddQuadVert(vh, ref vert, outer_rt.x, outer_rt.y);
        AddQuadVert(vh, ref vert, outer_rt.x, outer_lb.y);

        // ---- 内框 4 个顶点 ---- 
        AddQuadVert(vh, ref vert, inner_lb.x, inner_lb.y);
        AddQuadVert(vh, ref vert, inner_lb.x, inner_rt.y);
        AddQuadVert(vh, ref vert, inner_rt.x, inner_rt.y);
        AddQuadVert(vh, ref vert, inner_rt.x, inner_lb.y);

        if (!ShowHollowOut)
        {
            // 全屏填充
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
            return;
        }

        // ---- 8 个梯形三角 (外框切内框) ----
        vh.AddTriangle(0, 1, 4);
        vh.AddTriangle(1, 4, 5);
        vh.AddTriangle(1, 5, 2);
        vh.AddTriangle(2, 5, 6);
        vh.AddTriangle(2, 6, 3);
        vh.AddTriangle(6, 3, 7);
        vh.AddTriangle(4, 7, 3);
        vh.AddTriangle(0, 4, 3);

        // ---- 4 个圆角扇形 ----
        // 共点列表: [外部角点, 内部起始点] 按 左下/左上/右上/右下 顺序排列
        BuildCornerPoints(radius);

        for (int i = 0; i < 4; i++)
        {
            int idx = i * 2;
            Vector2 outerCorner = _commonPointOutside[idx];
            Vector2 center = _commonPoint[idx];
            Vector2 centerStart = _commonPoint[idx + 1];

            float startAngle = DegreeDeltaList[i];
            float delta = Mathf.PI / 2 / triCount;

            // 扇形的中心点 (外部角点)
            vert.position = outerCorner;
            vert.uv0 = ComputeUV(outerCorner, w, h);
            vh.AddVert(vert);
            int fanCenter = vh.currentVertCount - 1;
            int prev = vh.currentVertCount;

            _fanVerts.Clear();
            float angle = startAngle;
            for (int j = 0; j < triCount; j++)
            {
                angle += delta;
                float cos = radius * Mathf.Cos(angle);
                float sin = radius * Mathf.Sin(angle);

                Vector2 pos = new Vector2(center.x + cos, center.y + sin);
                if (j > 0)
                {
                    vert.position = _fanVerts[j - 1];
                    vert.uv0 = ComputeUV(_fanVerts[j - 1], w, h);
                }
                else
                {
                    vert.position = centerStart;
                    vert.uv0 = ComputeUV(centerStart, w, h);
                }
                vh.AddVert(vert);

                vert.position = pos;
                vert.uv0 = ComputeUV(pos, w, h);
                vh.AddVert(vert);

                vh.AddTriangle(fanCenter, prev, prev + 1);
                prev += 2;
                _fanVerts.Add(pos);
            }
        }
    }

    // ---- 辅助方法 ----

    /// <summary>添加一个顶点 (仅位置, UV 自动根据 w/h 计算)。</summary>
    private void AddQuadVert(VertexHelper vh, ref UIVertex vert, float x, float y)
    {
        vert.position = new Vector2(x, y);
        vert.uv0 = new Vector2(x / (inner_rt.x - inner_lb.x) + 0.5f, y / (inner_rt.y - inner_lb.y) + 0.5f);
        vh.AddVert(vert);
    }

    /// <summary>计算 UV (vh 坐标系原点在中心)。</summary>
    private static Vector2 ComputeUV(Vector2 pos, float w, float h)
    {
        return new Vector2(pos.x / w + 0.5f, pos.y / h + 0.5f);
    }

    /// <summary>
    /// 构建 4 个圆角的控制点。
    /// _commonPointOutside: [0]=左下外角, [1]=左下外角往上, [2]=左上外角, [3]=左上外角往右 ...
    /// _commonPoint:        [0]=左下圆心, [1]=左下圆心往上, [2]=左上圆心, [3]=左上圆心往右 ...
    /// </summary>
    private void BuildCornerPoints(float radius)
    {
        _commonPointOutside.Clear();
        _commonPoint.Clear();

        // 左下
        _commonPointOutside.Add(new Vector2(inner_lb.x, inner_lb.y));
        _commonPointOutside.Add(new Vector2(inner_lb.x, inner_lb.y + radius));
        _commonPoint.Add(new Vector2(inner_lb.x + radius, inner_lb.y + radius));
        _commonPoint.Add(new Vector2(inner_lb.x, inner_lb.y + radius));

        // 左上
        _commonPointOutside.Add(new Vector2(inner_lb.x, inner_rt.y));
        _commonPointOutside.Add(new Vector2(inner_lb.x + radius, inner_rt.y));
        _commonPoint.Add(new Vector2(inner_lb.x + radius, inner_rt.y - radius));
        _commonPoint.Add(new Vector2(inner_lb.x + radius, inner_rt.y));

        // 右上
        _commonPointOutside.Add(new Vector2(inner_rt.x, inner_rt.y));
        _commonPointOutside.Add(new Vector2(inner_rt.x, inner_rt.y - radius));
        _commonPoint.Add(new Vector2(inner_rt.x - radius, inner_rt.y - radius));
        _commonPoint.Add(new Vector2(inner_rt.x, inner_rt.y - radius));

        // 右下
        _commonPointOutside.Add(new Vector2(inner_rt.x, inner_lb.y));
        _commonPointOutside.Add(new Vector2(inner_rt.x - radius, inner_lb.y));
        _commonPoint.Add(new Vector2(inner_rt.x - radius, inner_lb.y + radius));
        _commonPoint.Add(new Vector2(inner_rt.x - radius, inner_lb.y));
    }

    /// <summary>
    /// 计算边界
    /// </summary>
    private void CalcBounds()
    {
        if (outer_trans == null)
            return;

        if (_useRectMode)
        {
            Vector2 offset = _currentOffset;
            inner_lb = _hollowRect.min - offset - new Vector2(padding.x, padding.y);
            inner_rt = _hollowRect.max + offset + new Vector2(padding.z, padding.w);
        }
        else if (inner_trans != null)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(outer_trans, inner_trans);
            // padding: x=左 y=下 z=右 w=上
            inner_rt = bounds.max + _currentOffset + new Vector3(padding.z, padding.w, 0);
            inner_lb = bounds.min - _currentOffset - new Vector3(padding.x, padding.y, 0);
        }
        else
        {
            inner_lb = Vector2.zero;
            inner_rt = Vector2.zero;
        }

        outer_rt = outer_trans.rect.max;
        outer_lb = outer_trans.rect.min;

        UpdateOrnament();
    }

    private void UpdateOrnament()
    {
        if (Ornament == null)
            return;

        Vector2 center = (inner_lb + inner_rt) * 0.5f;
        Vector2 size = new Vector2(inner_rt.x - inner_lb.x, inner_rt.y - inner_lb.y);
        Ornament.SetBounds(center, size);
    }

    private void Update()
    {
        if (_trackTarget != null)
        {
            UpdateWorldTargetBounds();
            SetAllDirty();
            return;
        }

        if (!realtimeRefresh)
            return;

        CalcBounds();
        SetAllDirty();
    }

    /// <summary>
    /// 跟踪世界物体: 合并所有 Renderer 的包围盒 → 屏幕矩形 → outer_trans 本地空间
    /// </summary>
    private void UpdateWorldTargetBounds()
    {
        if (_trackTarget == null || outer_trans == null)
            return;

        var renderers = _trackTarget.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        // 8 个角 → 屏幕坐标
        var c = combined;
        Vector3[] corners = new Vector3[8]
        {
            c.min,
            new Vector3(c.max.x, c.min.y, c.min.z),
            new Vector3(c.min.x, c.max.y, c.min.z),
            new Vector3(c.max.x, c.max.y, c.min.z),
            new Vector3(c.min.x, c.min.y, c.max.z),
            new Vector3(c.max.x, c.min.y, c.max.z),
            new Vector3(c.min.x, c.max.y, c.max.z),
            c.max,
        };

        var worldCam = Camera.main;
        if (worldCam == null) return;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 sp = worldCam.WorldToScreenPoint(corners[i]);
            if (sp.x < minX) minX = sp.x;
            if (sp.y < minY) minY = sp.y;
            if (sp.x > maxX) maxX = sp.x;
            if (sp.y > maxY) maxY = sp.y;
        }

        Vector2 screenCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        Vector2 screenHalf = new Vector2((maxX - minX) * 0.5f, (maxY - minY) * 0.5f);

        // UI 相机 (Overlay Canvas 时为 null, RectTransformUtility 可正确处理)
        var canvas = GetComponentInParent<Canvas>();
        Camera uiCam = canvas != null ? canvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(outer_trans, screenCenter, uiCam, out Vector2 localCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(outer_trans, screenCenter + screenHalf, uiCam, out Vector2 localMax);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(outer_trans, screenCenter - screenHalf, uiCam, out Vector2 localMin);

        Vector2 localSize = new Vector2(
            Mathf.Abs(localMax.x - localMin.x),
            Mathf.Abs(localMax.y - localMin.y)
        );

        _hollowRect = new Rect(localCenter - localSize * 0.5f, localSize);
        _useRectMode = true;

        // 更新 inner_lb/inner_rt (含 padding 和 offset)
        Vector2 offset = _currentOffset;
        inner_lb = _hollowRect.min - offset - new Vector2(padding.x, padding.y);
        inner_rt = _hollowRect.max + offset + new Vector2(padding.z, padding.w);

        // 同步更新外边界 (CalcBounds 的唯一调用点在 Awake, 此后不再执行)
        outer_lb = outer_trans.rect.min;
        outer_rt = outer_trans.rect.max;

        UpdateOrnament();
    }

    public bool IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
    {
        if (inner_trans == null && !_useRectMode)
            return true;

        if (_useRectMode)
        {
            // 将屏幕坐标转换到 outer_trans 本地空间, 检测是否在镂空区域内
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(outer_trans, screenPos, eventCamera, out Vector2 localPoint))
            {
                bool insideHollow = localPoint.x >= inner_lb.x && localPoint.x <= inner_rt.x
                                 && localPoint.y >= inner_lb.y && localPoint.y <= inner_rt.y;
                return !insideHollow;
            }
            return true;
        }

        // 传统模式: 检测 inner_trans
        return !RectTransformUtility.RectangleContainsScreenPoint(inner_trans, screenPos, eventCamera);
    }
}
