using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 贝塞尔曲线工具类
/// 提供创建、计算和约束贝塞尔曲线的方法
/// </summary>
public static class BezierUtility
{
    #region 数据结构
    
    /// <summary>
    /// 贝塞尔曲线数据
    /// </summary>
    [System.Serializable]
    public class CurveData
    {
        public Vector3 start;
        public Vector3 end;
        public Vector3 cp1;
        public Vector3 cp2;
        public float length;
        
        public Vector3 GetPoint(float t)
        {
            return CalculatePoint(start, cp1, cp2, end, t);
        }
        
        public Vector3 GetTangent(float t)
        {
            return CalculateTangent(start, cp1, cp2, end, t).normalized;
        }
    }
    
    /// <summary>
    /// 曲线配置
    /// </summary>
    [System.Serializable]
    public class CurveConfig
    {
        [Header("基本参数")]
        [Range(0.1f, 10f)] public float minDistance = 3f;        // 控制点最小距离
        [Range(0.1f, 20f)] public float maxDistance = 8f;        // 控制点最大距离
        [Range(0f, 5f)] public float heightFactor = 2f;          // 高度系数
        [Range(0f, 5f)] public float randomDeviation = 1.5f;     // 随机偏移量
    }
    
    #endregion
    
    #region 基础计算方法
    
    /// <summary>
    /// 计算三次贝塞尔曲线上的点
    /// </summary>
    public static Vector3 CalculatePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;
        
        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }
    
    /// <summary>
    /// 计算贝塞尔曲线的切线（导数）
    /// </summary>
    public static Vector3 CalculateTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        return 3 * u * u * (p1 - p0) +
               6 * u * t * (p2 - p1) +
               3 * t * t * (p3 - p2);
    }
    
    /// <summary>
    /// 估算贝塞尔曲线长度
    /// </summary>
    public static float EstimateLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments = 20)
    {
        float length = 0f;
        Vector3 prev = p0;
        
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = CalculatePoint(p0, p1, p2, p3, t);
            length += Vector3.Distance(prev, point);
            prev = point;
        }
        
        return length;
    }
    
    #endregion
    
    #region 曲线创建方法
    
    /// <summary>
    /// 创建导弹轨迹曲线
    /// </summary>
    public static CurveData CreateMissileCurve(
        Vector3 launchPos,
        Vector3 targetPos,
        Vector3 startDirection,
        CurveConfig config)
    {
        Vector3 toTarget = targetPos - launchPos;
        
        // 生成第一个控制点
        Vector3 cp1 = launchPos + startDirection * Random.Range(config.minDistance, config.maxDistance);
        
        // 生成第二个控制点
        Vector3 midPoint = (launchPos + targetPos) * 0.5f;
        float height = toTarget.magnitude * 0.3f * config.heightFactor;
        
        Vector3 cp2 = midPoint + 
                     Vector3.up * height + 
                     Random.onUnitSphere * config.randomDeviation;
        
        // 计算曲线长度
        float length = EstimateLength(launchPos, cp1, cp2, targetPos);
        
        return new CurveData
        {
            start = launchPos,
            end = targetPos,
            cp1 = cp1,
            cp2 = cp2,
            length = length
        };
    }
    
    /// <summary>
    /// 基于起点终点方向创建曲线
    /// </summary>
    public static CurveData CreateSimpleCurve(Vector3 start, Vector3 end, CurveConfig config)
    {
        Vector3 direction = (end - start).normalized;
        return CreateMissileCurve(start, end, direction, config);
    }
    
    /// <summary>
    /// 计算弧形贝塞尔控制点，弧只向左或上方偏移
    /// </summary>
    public static void ComputeArcControlPoints(Vector3 start, Vector3 end, float arcHeight, out Vector3 cp1, out Vector3 cp2)
    {
        Vector3 dir = end - start;
        Vector3 perp1 = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 perp2 = -perp1;
        Vector3 bias = new Vector3(-1f, 1f, 0f);
        Vector3 perp = Vector3.Dot(perp1, bias) > Vector3.Dot(perp2, bias) ? perp1 : perp2;
        cp1 = start + dir * 0.33f + perp * arcHeight;
        cp2 = start + dir * 0.66f + perp * arcHeight;
    }
    
    #endregion
    
    #region 散射曲线方法
    
    /// <summary>
    /// 计算散布方向
    /// </summary>
    public static Vector3 CalculateSpreadDirection(
        Vector3 baseDirection, 
        float angle, 
        Vector3 spreadAxis,
        bool useCircularSpread = true,
        bool useRandomSpread = false)
    {
        if (useCircularSpread && !useRandomSpread)
        {
            // 圆形散布（均匀但有一定随机性）
            float randomRadius = Random.Range(0.8f, 1.2f);
            float randomPhase = Random.Range(0f, 360f);
            
            // 创建垂直于基础方向的向量
            Vector3 perpendicular = Vector3.Cross(baseDirection, spreadAxis).normalized;
            if (perpendicular.magnitude < 0.1f)
                perpendicular = Vector3.Cross(baseDirection, Vector3.up).normalized;
            
            // 旋转矩阵
            Quaternion rotation = Quaternion.AngleAxis(angle + randomPhase, spreadAxis);
            
            // 应用旋转并添加随机性
            Vector3 direction = rotation * baseDirection;
            direction += Random.onUnitSphere * 0.2f;
            direction = direction.normalized;
            
            return direction * randomRadius;
        }
        else
        {
            // 简单角度散布
            Quaternion rotation = Quaternion.AngleAxis(angle, spreadAxis);
            Vector3 direction = rotation * baseDirection;
            
            if (useRandomSpread)
            {
                direction += Random.onUnitSphere * 0.3f;
                direction = direction.normalized;
            }
            
            return direction;
        }
    }
    
    /// <summary>
    /// 创建多条散射曲线
    /// </summary>
    public static List<CurveData> CreateScatterCurves(
        Vector3 start,
        Vector3 end,
        int count,
        CurveConfig config,
        float spreadAngle = 60f,
        Vector3 spreadAxis = default,
        bool useCircularSpread = true)
    {
        List<CurveData> curves = new List<CurveData>();
        
        if (spreadAxis == Vector3.zero)
            spreadAxis = Vector3.up;
        
        Vector3 toTarget = end - start;
        Vector3 baseDir = toTarget.normalized;
        
        // 计算散布角度
        float angleStep = spreadAngle / Mathf.Max(1, count - 1);
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 spreadDir = CalculateSpreadDirection(baseDir, angle, spreadAxis, useCircularSpread, false);
            
            var curve = CreateMissileCurve(start, end, spreadDir, config);
            curves.Add(curve);
        }
        
        return curves;
    }
    
    /// <summary>
    /// 创建随机散射曲线
    /// </summary>
    public static List<CurveData> CreateRandomScatterCurves(
        Vector3 start,
        Vector3 end,
        int count,
        CurveConfig config,
        float spreadAngle = 60f,
        Vector3 spreadAxis = default)
    {
        List<CurveData> curves = new List<CurveData>();
        
        if (spreadAxis == Vector3.zero)
            spreadAxis = Vector3.up;
        
        Vector3 toTarget = end - start;
        Vector3 baseDir = toTarget.normalized;
        
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            Vector3 randomDir = CalculateSpreadDirection(baseDir, randomAngle, spreadAxis, false, true);
            
            var curve = CreateMissileCurve(start, end, randomDir, config);
            curves.Add(curve);
        }
        
        return curves;
    }
    
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 使用DOTween缓动函数库对t求值：t∈[0,1]
    /// </summary>
    public static float EvaluateEase(DG.Tweening.Ease ease, float t)
    {
        if (ease == DG.Tweening.Ease.Unset || ease == DG.Tweening.Ease.INTERNAL_Zero)
            return t;
        
        return DG.Tweening.Core.Easing.EaseManager.Evaluate(ease, null, t, 1f, 1f, 0.3f);
    }
    
    /// <summary>
    /// 绘制贝塞尔曲线（Gizmos）
    /// </summary>
    public static void DrawCurveGizmos(
        CurveData curveData, 
        Color curveColor = default, 
        Color controlPointColor = default,
        int segments = 20)
    {
        if (curveColor == default) curveColor = Color.yellow;
        if (controlPointColor == default) controlPointColor = Color.cyan;
        
        Color originalColor = Gizmos.color;
        
        // 绘制曲线
        Gizmos.color = curveColor;
        Vector3 prevPoint = curveData.start;
        
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = curveData.GetPoint(t);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
        
        // 绘制控制点
        Gizmos.color = controlPointColor;
        Gizmos.DrawSphere(curveData.cp1, 0.2f);
        Gizmos.DrawSphere(curveData.cp2, 0.2f);
        
        // 绘制连接线
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(curveData.start, curveData.cp1);
        Gizmos.DrawLine(curveData.cp1, curveData.cp2);
        Gizmos.DrawLine(curveData.cp2, curveData.end);
        
        Gizmos.color = originalColor;
    }
    
    #endregion
}