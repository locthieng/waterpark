using UnityEngine;
using System.Collections.Generic;
using BezierSolution;

public class BlockCellController : MonoBehaviour
{
    public static BlockCellController Instance { get; private set; }

    public Block _blockPrefab;
    public List<BlockCell> AllBlockCells = new List<BlockCell>();

    private void Awake()
    {
        Instance = this;
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

        foreach (var cell in AllBlockCells)
        {
            if (cell == null) continue;

            // Tìm điểm gần nhất trên Spline với vị trí của BlockCell
            spline.FindNearestPointTo(cell.transform.position, out float normalizedT);

            // Quy đổi normalizedT sang khoảng cách thực tế trên spline
            float pathDist = spline.evenlySpacedPoints.GetPercentageAtNormalizedT(normalizedT) 
                             * spline.evenlySpacedPoints.splineLength;

            cell.PathDistForCollect = pathDist;

            Debug.Log($"[Init Cell Dist] Cell: {cell.name} " +
                $"| Pos: {cell.transform.position} | NormalizedT: {normalizedT:F3} | PathDist: {pathDist:F2}");
        }

        // Sắp xếp các cell theo thứ tự tăng dần của khoảng cách để Hole duyệt qua đúng trình tự di chuyển
        AllBlockCells.Sort((a, b) => a.PathDistForCollect.CompareTo(b.PathDistForCollect));
    }
}
