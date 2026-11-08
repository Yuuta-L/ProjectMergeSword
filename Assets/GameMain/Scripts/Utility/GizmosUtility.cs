#if UNITY_EDITOR
using UnityEngine;
public static class GizmosUtility
{
    public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f,
        float arrowHeadAngle = 25f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(origin, direction);

        DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void DrawArrowEnd(bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color,
        float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
    {

        if (direction == Vector3.zero)
        {
            return;
        }

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        if (drawGizmos)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(arrowEndPosition + direction, right * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, left * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, up * arrowHeadLength);
            Gizmos.DrawRay(arrowEndPosition + direction, down * arrowHeadLength);
        }
        else
        {
            Debug.DrawRay(arrowEndPosition + direction, right * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, left * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, up * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, down * arrowHeadLength, color);
        }
    }
}

#endif