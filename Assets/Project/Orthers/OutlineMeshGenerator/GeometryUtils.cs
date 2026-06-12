using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// T?o cung tròn bo góc t?i pc gi?a hai ?o?n p0–pc và pc–p1.
/// N?u radius quá l?n so v?i chi?u dài c?nh, s? t? gi?m ?? không v??t.
/// Tr? v? danh sách các ?i?m n?m trên cung tròn (bao g?m ?i?m ??u và cu?i c?a bo).
/// </summary>
public static class GeometryUtils
{
    public static List<Vector3> FilletCorner(Vector3 p0, Vector3 pc, Vector3 p1, float radius, int segments)
    {
        var points = new List<Vector3>();

        // Hai vector h??ng t? ??nh ra ngoài
        Vector3 v0 = (p0 - pc).normalized;
        Vector3 v1 = (p1 - pc).normalized;

        // Góc gi?a hai c?nh
        float angle = Mathf.Acos(Mathf.Clamp(Vector2.Dot(v0, v1), -1f, 1f));
        if (angle < 1e-4f)
        {
            // g?n th?ng -> không bo
            points.Add(pc);
            return points;
        }

        // Tính kho?ng cách t? pc ra ??n ?i?m ti?p xúc v?i cung (offset d?c theo m?i c?nh)
        float tanHalf = Mathf.Tan(angle / 2f);
        float dist = radius / tanHalf;

        // Gi?i h?n ?? không v??t c?nh th?t
        float len0 = (p0 - pc).magnitude;
        float len1 = (p1 - pc).magnitude;
        dist = Mathf.Min(dist, Mathf.Min(len0, len1) * 0.999f);

        // ?i?m ti?p xúc v?i cung trên m?i c?nh
        Vector3 p0a = pc + v0 * dist;
        Vector3 p1a = pc + v1 * dist;

        // Pháp tuy?n c?a m?i c?nh
        Vector3 n0 = new(-v0.y, v0.x);
        Vector3 n1 = new(-v1.y, v1.x);

        // Tâm cung tròn là giao c?a hai ???ng ?i qua p0a/p1a h??ng theo n0/n1
        if (!LineLine(p0a, n0, p1a, n1, out Vector2 center))
        {
            // n?u không giao thì b? bo
            points.Add(pc);
            return points;
        }

        // Tính góc b?t ??u và k?t thúc
        float startAng = Mathf.Atan2(p0a.y - center.y, p0a.x - center.x);
        float endAng = Mathf.Atan2(p1a.y - center.y, p1a.x - center.x);

        // Xác ??nh h??ng xoay (theo chi?u convex hay concave)
        float cross = v0.x * v1.y - v0.y * v1.x;
        bool convex = cross < 0;
        if (convex)
        {
            if (endAng < startAng) endAng += Mathf.PI * 2f;
        }
        else
        {
            if (endAng > startAng) endAng -= Mathf.PI * 2f;
        }

        // Sinh các ?i?m bo
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float ang = Mathf.Lerp(startAng, endAng, t);
            Vector2 p = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
            points.Add(p);
        }

        return points;
    }

    // --- Helper tìm giao tuy?n hai ???ng ---
    static bool LineLine(Vector2 p, Vector2 d, Vector2 q, Vector2 e, out Vector2 x)
    {
        float den = d.x * e.y - d.y * e.x;
        if (Mathf.Abs(den) < 1e-6f) { x = default; return false; }
        float t = ((q.x - p.x) * e.y - (q.y - p.y) * e.x) / den;
        x = p + d * t;
        return true;
    }
}
