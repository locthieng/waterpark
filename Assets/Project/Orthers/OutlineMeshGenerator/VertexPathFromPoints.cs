using System.Collections.Generic;
using UnityEngine;

/// Phiên bản rút gọn của VertexPath: tạo từ list điểm phẳng XY, không Bezier.
public class VertexPathFromPoints
{
    public readonly Vector3[] points;
    public readonly Vector3[] tangents;
    public readonly Vector3[] normals;     // "right" theo RoadMeshCreator
    public readonly float[] times;         // 0..1 dọc path
    public readonly bool isClosed;
    public readonly float length;

    public int NumPoints => points.Length;

    public VertexPathFromPoints(List<Vector3> input, bool closed)
    {
        isClosed = closed;
        int n = input.Count;
        points = input.ToArray();
        tangents = new Vector3[n];
        normals  = new Vector3[n];
        times    = new float[n];

        // Tangent
        for (int i = 0; i < n; i++)
        {
            int ip = (i - 1 + n) % n;
            int inx = (i + 1) % n;
            if (!closed && (i == 0 || i == n - 1))
                tangents[i] = ((i == 0) ? (points[inx] - points[i]) : (points[i] - points[ip])).normalized;
            else
                tangents[i] = ((points[inx] - points[ip]) * 0.5f).normalized;
        }

        // Parallel transport (y hệt VertexPath, up chọn -Z cho path XY)
        Vector3 up = Vector3.back;
        Vector3 lastAxis = up;
        for (int i = 0; i < n; i++)
        {
            if (i == 0)
            {
                normals[0] = Vector3.Cross(lastAxis, tangents[0]).normalized; // “right”
            }
            else
            {
                Vector3 off = points[i] - points[i - 1];
                float sq = Mathf.Max(1e-12f, off.sqrMagnitude);

                Vector3 r = lastAxis - off * 2f / sq * Vector3.Dot(off, lastAxis);
                Vector3 t = tangents[i - 1] - off * 2f / sq * Vector3.Dot(off, tangents[i - 1]);

                Vector3 v2 = tangents[i] - t;
                float c2 = Mathf.Max(1e-12f, Vector3.Dot(v2, v2));
                Vector3 finalRot = r - v2 * 2f / c2 * Vector3.Dot(v2, r);

                normals[i] = Vector3.Cross(finalRot, tangents[i]).normalized;
                lastAxis = finalRot;
            }
        }

        // length + times
        float cum = 0f;
        times[0] = 0f;
        for (int i = 1; i < n; i++)
        {
            cum += Vector3.Distance(points[i - 1], points[i]);
            times[i] = cum;
        }
        length = Mathf.Max(1e-6f, cum);
        for (int i = 0; i < n; i++) times[i] /= length;
    }
}
