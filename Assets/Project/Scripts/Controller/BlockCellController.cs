using UnityEngine;
using System.Collections.Generic;
using BezierSolution;

public class BlockCellController : MonoBehaviour
{
    public static BlockCellController Instance { get; private set; }

    public Block _blockPrefab;

    public List<BlockCell> AllBlockCells = new List<BlockCell>();

    // Debug visualization data
    private struct CellDebugData
    {
        public Vector3 cellPos;
        public Vector3 splineHitPoint;
        public float pathDist;
        public string cellName;
    }
    private List<CellDebugData> _debugDataList = new List<CellDebugData>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetUp()
    {
        for (int i = 0; i < AllBlockCells.Count; i++)
        {
            BlockCell _BlockCell = AllBlockCells[i];
            _BlockCell.InitializeStackFromPending(_blockPrefab, _BlockCell._spacingBlock, true);
        }
    }    

    public BlockCell GetCellByIndex(int index)
    {
        if (index >= 0 && index < AllBlockCells.Count)
        {
            return AllBlockCells[index];
        }
        return null;
    }

    public void InitializeCellDistances(BezierSpline spline)
    {
        if (spline == null) return;

        // Độ dài ray đủ lớn để chắc chắn vươn tới spline
        const float rayLength = 100f;

        _debugDataList.Clear();

        foreach (var cell in AllBlockCells)
        {
            if (cell == null) continue;

            // Lấy hướng bắn từ spawn direction (local space) của cell và chuyển sang world space
            Vector3 worldDir = cell.transform.TransformDirection(cell.GetSpawnDirection()).normalized;

            // Tạo đoạn line từ vị trí cell theo hướng bắn
            Vector3 lineStart = cell.transform.position;
            Vector3 lineEnd = lineStart + worldDir * rayLength;

            // Tìm điểm trên Spline gần nhất với đoạn line (ray) đó
            spline.FindNearestPointToLine(lineStart, lineEnd, out Vector3 pointOnLine, out float normalizedT);

            // Lấy điểm thực tế trên spline tại normalizedT
            Vector3 splineHitPoint = spline.GetPoint(normalizedT);

            // Quy đổi normalizedT sang khoảng cách thực tế trên spline
            float pathDist = spline.evenlySpacedPoints.GetPercentageAtNormalizedT(normalizedT) 
                             * spline.evenlySpacedPoints.splineLength;

            cell.PathDistForCollect = pathDist;

            // Vẽ debug lên Scene (hiển thị 30 giây)
            Debug.DrawLine(lineStart, splineHitPoint, Color.green, 30f);        // Cell → điểm trên spline (xanh lá)

            // Lưu data để OnDrawGizmos vẽ sphere
            _debugDataList.Add(new CellDebugData
            {
                cellPos = lineStart,
                splineHitPoint = splineHitPoint,
                pathDist = pathDist,
                cellName = cell.name
            });

            float angleZ = cell.SpawnerDirectionAngleZ;

            Debug.Log($"[Init Cell Dist] Cell: {cell.name} " +
                $"| Pos: {cell.transform.position} | AngleZ: {angleZ:F1} | Dir: {worldDir} " +
                $"| NormalizedT: {normalizedT:F3} | PathDist: {pathDist:F2}");
        }

        // Sắp xếp các cell theo thứ tự tăng dần của khoảng cách để Hole duyệt qua đúng trình tự di chuyển
        AllBlockCells.Sort((a, b) => a.PathDistForCollect.CompareTo(b.PathDistForCollect));
    }

    private void OnDrawGizmos()
    {
        if (_debugDataList == null || _debugDataList.Count == 0) return;

        for (int i = 0; i < _debugDataList.Count; i++)
        {
            var data = _debugDataList[i];

            // Sphere đỏ tại điểm hit trên spline
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(data.splineHitPoint, 0.15f);

            // Sphere cyan tại vị trí cell
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(data.cellPos, 0.1f);

            // Đường cell → spline hit (xanh lá)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(data.cellPos, data.splineHitPoint);
        }
    }
}
