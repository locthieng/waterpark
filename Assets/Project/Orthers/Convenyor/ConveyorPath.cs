using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý hình dạng quỹ đạo của băng chuyền.
/// Hỗ trợ 3 chế độ: Circle, Ellipse, FreeformSpline (Catmull-Rom).
/// 
/// Gắn lên cùng GameObject với các component Conveyor khác.
/// Thay đổi bất kỳ thông số nào trên Inspector → Gizmo cập nhật ngay lập tức.
/// </summary>
public class ConveyorPath : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  ENUMS
    // ─────────────────────────────────────────────

    public enum PathMode
    {
        [Tooltip("Vòng tròn đều — chỉ cần 1 bán kính.")]
        Circle,

        [Tooltip("Hình elip — bán kính X và Z riêng biệt.")]
        Ellipse,

        [Tooltip("Đường cong tự do qua các waypoints (Catmull-Rom Spline).")]
        FreeformSpline
    }

    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("══════ CHẾ ĐỘ HÌNH DẠNG ══════")]
    [Tooltip("Chọn hình dạng quỹ đạo băng chuyền.")]
    [SerializeField] private PathMode pathMode = PathMode.Circle;

    [Header("── Circle ──")]
    [Tooltip("Bán kính vòng tròn. Đơn vị: Unity unit.")]
    [SerializeField] private float radius = 5f;

    [Header("── Ellipse ──")]
    [Tooltip("Bán kính trục X (chiều ngang).")]
    [SerializeField] private float radiusX = 5f;

    [Tooltip("Bán kính trục Z (chiều sâu).")]
    [SerializeField] private float radiusZ = 3f;

    [Header("── Freeform Spline ──")]
    [Tooltip("Các điểm điều khiển tạo đường cong. Tối thiểu 3 điểm. Thứ tự quyết định hình dạng.")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Số mẫu nội suy mỗi đoạn giữa 2 waypoints. Cao hơn = mượt hơn.")]
    [SerializeField, Range(10, 100)] private int splineResolution = 50;

    [Header("══════ GIZMO ══════")]
    [Tooltip("Màu đường quỹ đạo trong Scene View.")]
    [SerializeField] private Color pathColor = Color.cyan;

    [Tooltip("Số đoạn vẽ Gizmo (cao hơn = mượt hơn).")]
    [SerializeField, Range(32, 128)] private int gizmoSegments = 64;

    // ─────────────────────────────────────────────
    //  SPLINE CACHE
    // ─────────────────────────────────────────────

    private List<Vector3> cachedSplinePoints;
    private List<float> cachedCumulativeLengths;
    private float cachedTotalLength;
    private bool splineDirty = true;

    // Lưu hash của waypoint positions để detect thay đổi
    private int lastWaypointHash;

    // ─────────────────────────────────────────────
    //  PROPERTIES
    // ─────────────────────────────────────────────

    /// <summary>Chế độ hình dạng hiện tại.</summary>
    public PathMode CurrentMode => pathMode;

    /// <summary>Bán kính (Circle mode).</summary>
    public float Radius => radius;

    /// <summary>Bán kính X (Ellipse mode).</summary>
    public float RadiusX => radiusX;

    /// <summary>Bán kính Z (Ellipse mode).</summary>
    public float RadiusZ => radiusZ;

    /// <summary>Spline cần rebuild không.</summary>
    public bool IsSplineDirty => splineDirty;

    // ═══════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Tính vị trí world-space trên quỹ đạo tại normalized position t ∈ [0, 1).
    /// t = 0 và t = 1 là cùng một điểm (vòng kín).
    /// </summary>
    public Vector3 EvaluatePosition(float t)
    {
        t = Mathf.Repeat(t, 1f);

        switch (pathMode)
        {
            case PathMode.Circle:        return EvalCircle(t);
            case PathMode.Ellipse:       return EvalEllipse(t);
            case PathMode.FreeformSpline: return EvalSpline(t);
            default:                     return transform.position;
        }
    }

    /// <summary>
    /// Rebuild spline cache. Gọi khi waypoints thay đổi lúc runtime.
    /// Với Circle/Ellipse thì không cần gọi.
    /// </summary>
    public void RebuildSplineCache()
    {
        if (waypoints == null || waypoints.Length < 3)
        {
            Debug.LogWarning("[ConveyorPath] FreeformSpline cần tối thiểu 3 waypoints!");
            return;
        }

        cachedSplinePoints = new List<Vector3>();
        cachedCumulativeLengths = new List<float>();
        cachedTotalLength = 0f;

        int totalSamples = waypoints.Length * splineResolution;
        Vector3 prevPoint = EvalCatmullRom(0f);
        cachedSplinePoints.Add(prevPoint);
        cachedCumulativeLengths.Add(0f);

        for (int i = 1; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 point = EvalCatmullRom(t);
            float segLen = Vector3.Distance(prevPoint, point);
            cachedTotalLength += segLen;

            cachedSplinePoints.Add(point);
            cachedCumulativeLengths.Add(cachedTotalLength);
            prevPoint = point;
        }

        lastWaypointHash = ComputeWaypointHash();
        splineDirty = false;
    }

    /// <summary>
    /// Đánh dấu spline cần rebuild (gọi khi waypoints thay đổi).
    /// </summary>
    public void MarkSplineDirty()
    {
        splineDirty = true;
    }

    // ═══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════

    private void OnEnable()
    {
        if (pathMode == PathMode.FreeformSpline)
            RebuildSplineCache();
    }

    private void OnValidate()
    {
        // Clamp values
        radius = Mathf.Max(0.1f, radius);
        radiusX = Mathf.Max(0.1f, radiusX);
        radiusZ = Mathf.Max(0.1f, radiusZ);

        // Mark spline dirty khi bất kỳ thông số nào thay đổi
        splineDirty = true;
    }

    private void Update()
    {
        // Auto-detect waypoint di chuyển lúc runtime
        if (pathMode == PathMode.FreeformSpline && waypoints != null && waypoints.Length >= 3)
        {
            int hash = ComputeWaypointHash();
            if (hash != lastWaypointHash)
            {
                RebuildSplineCache();
            }
        }
    }

    // ═══════════════════════════════════════════════
    //  PATH EVALUATION (Internal)
    // ═══════════════════════════════════════════════

    private Vector3 EvalCircle(float t)
    {
        float angle = t * Mathf.PI * 2f;
        Vector3 local = new Vector3(
            radius * Mathf.Cos(angle),
            0f,
            radius * Mathf.Sin(angle)
        );
        return transform.TransformPoint(local);
    }

    private Vector3 EvalEllipse(float t)
    {
        float angle = t * Mathf.PI * 2f;
        Vector3 local = new Vector3(
            radiusX * Mathf.Cos(angle),
            0f,
            radiusZ * Mathf.Sin(angle)
        );
        return transform.TransformPoint(local);
    }

    private Vector3 EvalSpline(float t)
    {
        if (waypoints == null || waypoints.Length < 3)
            return transform.position;

        if (splineDirty || cachedSplinePoints == null || cachedSplinePoints.Count == 0)
            RebuildSplineCache();

        if (cachedTotalLength < 0.001f)
            return transform.position;

        // Arc-length parameterization — binary search
        float targetLength = t * cachedTotalLength;
        int lo = 0, hi = cachedCumulativeLengths.Count - 1;

        while (lo < hi - 1)
        {
            int mid = (lo + hi) / 2;
            if (cachedCumulativeLengths[mid] <= targetLength)
                lo = mid;
            else
                hi = mid;
        }

        float segLen = cachedCumulativeLengths[hi] - cachedCumulativeLengths[lo];
        float localT = (segLen > 0.001f)
            ? (targetLength - cachedCumulativeLengths[lo]) / segLen
            : 0f;

        return Vector3.Lerp(cachedSplinePoints[lo], cachedSplinePoints[hi], localT);
    }

    /// <summary>
    /// Catmull-Rom spline (closed loop). t ∈ [0, 1) → toàn bộ vòng.
    /// </summary>
    private Vector3 EvalCatmullRom(float t)
    {
        int n = waypoints.Length;
        float scaledT = t * n;
        int segment = Mathf.FloorToInt(scaledT) % n;
        float lt = scaledT - Mathf.FloorToInt(scaledT);

        Vector3 p0 = waypoints[((segment - 1) + n) % n].position;
        Vector3 p1 = waypoints[segment].position;
        Vector3 p2 = waypoints[(segment + 1) % n].position;
        Vector3 p3 = waypoints[(segment + 2) % n].position;

        float t2 = lt * lt;
        float t3 = t2 * lt;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * lt +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    /// <summary>
    /// Tạo hash từ vị trí waypoints để detect thay đổi.
    /// </summary>
    private int ComputeWaypointHash()
    {
        if (waypoints == null) return 0;

        int hash = 17;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Vector3 pos = waypoints[i].position;
                hash = hash * 31 + pos.GetHashCode();
            }
        }
        return hash;
    }

    // ═══════════════════════════════════════════════
    //  GIZMOS
    // ═══════════════════════════════════════════════

    private void OnDrawGizmos()
    {
        DrawPath(pathColor * 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        DrawPath(pathColor);
        DrawCenter();
    }

    private void DrawPath(Color color)
    {
        Gizmos.color = color;
        int segs = Mathf.Max(16, gizmoSegments);
        Vector3 prev = EvaluatePosition(0f);

        for (int i = 1; i <= segs; i++)
        {
            float t = (float)i / segs;
            Vector3 point = EvaluatePosition(t);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        // Vẽ waypoint markers cho FreeformSpline
        if (pathMode == PathMode.FreeformSpline && waypoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var wp in waypoints)
            {
                if (wp != null)
                    Gizmos.DrawWireSphere(wp.position, 0.2f);
            }
        }
    }

    private void DrawCenter()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}
