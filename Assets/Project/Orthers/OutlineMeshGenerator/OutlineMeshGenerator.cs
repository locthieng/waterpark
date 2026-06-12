using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sinh mesh 3D theo logic RoadMeshCreator nhưng nhận List<Vector3>.
/// Hỗ trợ bo góc (corner) + bo cạnh (edge bulge), hoạt động trên XY, XZ, YZ.
/// </summary>
public static class OutlineMeshGenerator
{
    public enum OutlinePlane { XY, XZ, YZ }

    public static Mesh Build(
    List<Vector3> pts,
    float thickness,
    float height,
    bool closed,
    float cornerRadius = 0f,
    int cornerSegments = 0,
    float edgeBluntRate = 0f,
    OutlinePlane plane = OutlinePlane.XY)
    {
        var mesh = new Mesh { name = "Outline" };
        if (pts == null || pts.Count < 2) return mesh;

        // (tuỳ chọn) bo góc
        if (cornerRadius > 0f)
            pts = OutlineSmooth(pts, cornerRadius, cornerSegments, closed, plane);

        // Project về XY để tính frame
        var ptsXY = new List<Vector3>(pts.Count);
        for (int i = 0; i < pts.Count; i++)
        {
            var v2 = ToXY(pts[i], plane);
            ptsXY.Add(new Vector3(v2.x, v2.y, 0));
        }

        var path = new VertexPathFromPoints(ptsXY, closed);
        int n = path.NumPoints;
        if (n < 2) return mesh;

        float half = Mathf.Max(0.0001f, thickness * 0.5f);
        float h = Mathf.Max(0.0001f, height);
        float a = Mathf.Clamp01(edgeBluntRate) * thickness; // độ mở vát

        // Mỗi khung có 12 đỉnh:
        // 0  A_L (top flat inner - left)
        // 1  A_R (top flat inner - right)
        // 2  B_L (top bevel - left)       (normal nghiêng cho top)
        // 3  B_R (top bevel - right)
        // 4  C_L (bottom bevel - left)    (normal nghiêng cho bottom)
        // 5  C_R (bottom bevel - right)
        // 6  D_L (bottom flat inner - left)
        // 7  D_R (bottom flat inner - right)
        // 8  Bsl_L (top bevel copy for side)  (normal ±right)
        // 9  Bsl_R
        // 10 Csl_L (bottom bevel copy for side)
        // 11 Csl_R
        int V = 12;
        int segCount = (n - 1) + (closed ? 1 : 0);

        var verts = new Vector3[n * V];
        var norms = new Vector3[n * V];
        var uvs = new Vector2[n * V];

        var topTris = new List<int>(segCount * 3 * 2 * 2);   // 3 strips * 2 tris/seg * 2 (an toàn)
        var sideTris = new List<int>(segCount * 2 * 2);       // 2 strips (left/right)
        var bottomTris = new List<int>(segCount * 3 * 2 * 2);   // 3 strips

        // helper nội bộ: thêm 1 strip (hai “cột” song song dọc theo path)
        static void AddStrip(List<int> dst, int baseI, int baseNext, int left, int right, int stride)
        {
            int a0 = baseI + left;
            int b0 = baseI + right;
            int a1 = baseNext + left;
            int b1 = baseNext + right;

            // Hai tam giác: (a0, b1, a1) & (a0, b0, b1)
            dst.Add(a0); dst.Add(b1); dst.Add(a1);
            dst.Add(a0); dst.Add(b0); dst.Add(b1);
        }

        for (int i = 0, vi = 0; i < n; i++, vi += V)
        {
            // Frame tại điểm i
            Vector2 pXY = new(path.points[i].x, path.points[i].y);
            Vector2 tXY = new(path.tangents[i].x, path.tangents[i].y);
            Vector2 rXY = new(path.normals[i].x, path.normals[i].y);

            float keepCoord = plane switch
            {
                OutlinePlane.XY => pts[i].z,
                OutlinePlane.XZ => pts[i].y,
                OutlinePlane.YZ => pts[i].x,
                _ => 0
            };

            Vector3 p = FromXY(pXY, keepCoord, plane);
            Vector3 tangent = DirFromXY(tXY.normalized, plane).normalized;
            Vector3 right = DirFromXY(rXY.normalized, plane).normalized;
            Vector3 up = Vector3.Cross(tangent, right).normalized;

            if (up.sqrMagnitude < 1e-6f)
            {
                up = plane switch
                {
                    OutlinePlane.XY => Vector3.forward,
                    OutlinePlane.XZ => Vector3.up,
                    OutlinePlane.YZ => Vector3.right,
                    _ => Vector3.forward
                };
            }

            // Lớp A (top phẳng)
            Vector3 A_L = p - right * half;
            Vector3 A_R = p + right * half;

            // Lớp D (bottom phẳng)
            Vector3 D_L = A_L - up * h;
            Vector3 D_R = A_R - up * h;

            // Lớp B (top vát): từ A dịch theo (-up ± right) * a
            Vector3 B_L = A_L + (-up - right).normalized * a;
            Vector3 B_R = A_R + (-up + right).normalized * a;

            // Lớp C (bottom vát): từ D dịch theo (+up ± right) * a
            Vector3 C_L = D_L + (up - right).normalized * a;
            Vector3 C_R = D_R + (up + right).normalized * a;

            // Gán vị trí
            verts[vi + 0] = A_L;
            verts[vi + 1] = A_R;
            verts[vi + 2] = B_L;
            verts[vi + 3] = B_R;
            verts[vi + 4] = C_L;
            verts[vi + 5] = C_R;
            verts[vi + 6] = D_L;
            verts[vi + 7] = D_R;
            verts[vi + 8] = B_L; // copy cho side
            verts[vi + 9] = B_R;
            verts[vi + 10] = C_L; // copy cho side
            verts[vi + 11] = C_R;

            // Normals:
            // - Top: A = up; B = nghiêng (nhìn mượt bevel)
            // - Bottom: D = -up; C = nghiêng xuống
            // - Side: ±right
            norms[vi + 0] = up;
            norms[vi + 1] = up;
            norms[vi + 2] = (-up - right).normalized;
            norms[vi + 3] = (-up + right).normalized;

            norms[vi + 4] = (-up - right).normalized;
            norms[vi + 5] = (-up + right).normalized;
            norms[vi + 6] = -up;
            norms[vi + 7] = -up;

            norms[vi + 8] = -right;
            norms[vi + 9] = right;
            norms[vi + 10] = -right;
            norms[vi + 11] = right;

            // UV: v dọc theo path.times; u: 0/1 theo cột trái/phải trong mỗi strip
            float v = path.times[i];
            // A, B-top, C-bottom, D
            uvs[vi + 0] = new Vector2(0, v);
            uvs[vi + 1] = new Vector2(1, v);
            uvs[vi + 2] = new Vector2(0, v);
            uvs[vi + 3] = new Vector2(1, v);
            uvs[vi + 4] = new Vector2(0, v);
            uvs[vi + 5] = new Vector2(1, v);
            uvs[vi + 6] = new Vector2(0, v);
            uvs[vi + 7] = new Vector2(1, v);
            // side copies
            uvs[vi + 8] = new Vector2(0, v);
            uvs[vi + 9] = new Vector2(1, v);
            uvs[vi + 10] = new Vector2(0, v);
            uvs[vi + 11] = new Vector2(1, v);
        }

        // Nối các khung liên tiếp
        for (int i = 0, vi = 0; i < n; i++, vi += V)
        {
            bool connect = (i < n - 1) || closed;
            if (!connect) break;

            int viNext = (vi + V) % (n * V);

            // TOP gồm 3 strips: B–A (bevel trái), A–A (phẳng giữa), A–B (bevel phải)
            AddStripFlipped(topTris, vi, viNext, 2, 0, V); // B_L ↔ A_L
            AddStripFlipped(topTris, vi, viNext, 0, 1, V); // A_L ↔ A_R
            AddStripFlipped(topTris, vi, viNext, 1, 3, V); // A_R ↔ B_R

            // SIDE gồm 2 strips đứng: B_side ↔ C_side (trái và phải)
            AddStrip(sideTris, vi, viNext, 8, 10, V);  // left outer wall
            AddStripFlipped(sideTris, vi, viNext, 9, 11, V);  // right outer wall

            // BOTTOM gồm 3 strips: C–D (bevel trái), D–D (phẳng giữa), D–C (bevel phải)
            // (giữ winding giống top để nhất quán; normal đã là -up)
            AddStrip(bottomTris, vi, viNext, 4, 6, V); // C_L ↔ D_L
            AddStrip(bottomTris, vi, viNext, 6, 7, V); // D_L ↔ D_R
            AddStrip(bottomTris, vi, viNext, 7, 5, V); // D_R ↔ C_R
        }

        mesh.subMeshCount = 3;
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.SetTriangles(topTris, 0);
        mesh.SetTriangles(sideTris, 1);
        mesh.SetTriangles(bottomTris, 2);
        var vertsArr = mesh.vertices;
        var normalsArr = mesh.normals;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Dictionary<Vector3, Vector3> smoothMap = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < vertsArr.Length; i++)
        {
            Vector3 pos = vertsArr[i];
            if (smoothMap.ContainsKey(pos))
                smoothMap[pos] += normalsArr[i];
            else
                smoothMap[pos] = normalsArr[i];
        }
        for (int i = 0; i < vertsArr.Length; i++)
        {
            normalsArr[i] = smoothMap[vertsArr[i]].normalized;
        }
        mesh.normals = normalsArr;
        void AddStripFlipped(List<int> dst, int baseI, int baseNext, int left, int right, int stride)
        {
            int a0 = baseI + left;
            int b0 = baseI + right;
            int a1 = baseNext + left;
            int b1 = baseNext + right;

            dst.Add(a0); dst.Add(a1); dst.Add(b1);
            dst.Add(a0); dst.Add(b1); dst.Add(b0);
        }
        return mesh;
    }


    // --- Bo góc & cạnh ---
    public static List<Vector3> OutlineSmooth(
        List<Vector3> pts, float cornerRadius, int cornerSegments,
        bool closed,
        OutlinePlane plane = OutlinePlane.XY)
    {
        if (pts == null || pts.Count < 3) return new List<Vector3>(pts);

        var outPts = new List<Vector3>();
        if (closed && (pts[0] - pts[^1]).sqrMagnitude < 1e-6f)
            pts.RemoveAt(pts.Count - 1);

        int n = pts.Count;

        for (int i = 0; i < n; i++)
        {
            int iPrev = (i - 1 + n) % n;
            int iNext = (i + 1) % n;

            Vector3 p0 = pts[iPrev];
            Vector3 pc = pts[i];
            Vector3 p1 = pts[iNext];
            Vector2 p0_2D = ToXY(p0, plane);
            Vector2 pc_2D = ToXY(pc, plane);
            Vector2 p1_2D = ToXY(p1, plane);

            // Gọi helper
            var arc2D = GeometryUtils.FilletCorner(p0_2D, pc_2D, p1_2D, cornerRadius, cornerSegments);

            // Unproject về 3D
            foreach (var v in arc2D)
                outPts.Add(Unproject2D(v, pc, plane));



            //// Bo góc
            //Vector2 p0a = pc_2D + v0 * cornerRadius;
            //Vector2 p1a = pc_2D + v1 * cornerRadius;

            //Vector2 n0 = new(-v0.y, v0.x);
            //Vector2 n1 = new(-v1.y, v1.x);

            //if (!LineLine(p0a, n0, p1a, n1, out Vector2 pivot))
            //{
            //    outPts.Add(pc);
            //    continue;
            //}

            //float cross = v0.x * v1.y - v0.y * v1.x;
            //bool convex = cross < 0;

            //float startAng = Mathf.Atan2(p0a.y - pivot.y, p0a.x - pivot.x);
            //float endAng = Mathf.Atan2(p1a.y - pivot.y, p1a.x - pivot.x);
            //if (convex)
            //{
            //    if (endAng < startAng) endAng += Mathf.PI * 2f;
            //}
            //else
            //{
            //    if (endAng > startAng) endAng -= Mathf.PI * 2f;
            //}

            //outPts.Add(Unproject2D(p0a, pc, plane));
            //for (int s = 1; s < cornerSegments; s++)
            //{
            //    float t = s / (float)cornerSegments;
            //    float ang = Mathf.Lerp(startAng, endAng, t);
            //    float x = pivot.x + Mathf.Cos(ang) * cornerRadius;
            //    float y = pivot.y + Mathf.Sin(ang) * cornerRadius;
            //    outPts.Add(Unproject2D(new Vector2(x, y), pc, plane));
            //}
            //outPts.Add(Unproject2D(p1a, pc, plane));
        }

        if (closed && outPts.Count > 1 && (outPts[0] - outPts[^1]).sqrMagnitude < 1e-6f)
            outPts.RemoveAt(outPts.Count - 1);

        return outPts;
    }

    // --- Coordinate helpers ---
    static Vector2 ToXY(Vector3 p, OutlinePlane plane) => plane switch
    {
        OutlinePlane.XY => new Vector2(p.x, p.y),
        OutlinePlane.XZ => new Vector2(p.x, p.z),
        OutlinePlane.YZ => new Vector2(p.y, p.z),
        _ => new Vector2(p.x, p.y)
    };

    static Vector3 FromXY(Vector2 v, float keepCoord, OutlinePlane plane) => plane switch
    {
        OutlinePlane.XY => new Vector3(v.x, v.y, keepCoord),
        OutlinePlane.XZ => new Vector3(v.x, keepCoord, v.y),
        OutlinePlane.YZ => new Vector3(keepCoord, v.x, v.y),
        _ => new Vector3(v.x, v.y, keepCoord)
    };

    static Vector3 DirFromXY(Vector2 v, OutlinePlane plane) => plane switch
    {
        OutlinePlane.XY => new Vector3(v.x, v.y, 0),
        OutlinePlane.XZ => new Vector3(v.x, 0, v.y),
        OutlinePlane.YZ => new Vector3(0, v.x, v.y),
        _ => new Vector3(v.x, v.y, 0)
    };

    static Vector3 Unproject2D(Vector2 v, Vector3 ref3D, OutlinePlane plane) => plane switch
    {
        OutlinePlane.XY => new Vector3(v.x, v.y, ref3D.z),
        OutlinePlane.XZ => new Vector3(v.x, ref3D.y, v.y),
        OutlinePlane.YZ => new Vector3(ref3D.x, v.x, v.y),
        _ => ref3D
    };

    static bool LineLine(Vector2 p, Vector2 d, Vector2 q, Vector2 e, out Vector2 x)
    {
        float den = d.x * e.y - d.y * e.x;
        if (Mathf.Abs(den) < 1e-6f) { x = default; return false; }
        float t = ((q.x - p.x) * e.y - (q.y - p.y) * e.x) / den;
        x = p + d * t;
        return true;
    }
}
