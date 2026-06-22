using UnityEngine;
using System.Collections.Generic;

public class BlockCell : MonoBehaviour
{
    public BlockCellType CellType = BlockCellType.BlockSimple;

    public float PathDistForCollect;

    public float SpawnerDirectionAngleZ = 90f;

    public BlockCellAccessType AccessType { get; private set; } = BlockCellAccessType.NotAccessible;

    public List<Block> CurBlocks = new List<Block>();

    public List<PendingBlockData> PendingBlockDatas = new List<PendingBlockData>();

    public bool IsEmpty => CurBlocks.Count == 0;

    public float _spacingBlock = 1f;

    public int MaxInitialPendingDataLimit = 3; // Limit of how many PendingBlockDatas can be spawned/created initially

    private Block _cachedBlockPrefab;

    private List<int> _activeColOrder = new List<int>();

    private int _nextPendingDataIndex = 0;

    [SerializeField] private GameObject _Indicator;
    [SerializeField] private GameObject _ParentIndicator;

    private void Awake()
    {
        UpdateIndicatorRotation();
    }

    private void OnValidate()
    {
        UpdateIndicatorRotation();
    }

    public void UpdateIndicatorRotation()
    {
        if (_Indicator != null)
        {
            _Indicator.transform.localRotation = Quaternion.Euler(0f, 0f, SpawnerDirectionAngleZ);
        }
    }

    public float GetPathDistForCollect()
    {
        return PathDistForCollect;
    }

    public void SetAccessType(BlockCellAccessType newType)
    {
        AccessType = newType;
    }

    public Vector3 GetSpawnDirection()
    {
        Vector3 indicatorGlobalDir = -_Indicator.transform.up;

        return transform.InverseTransformDirection(indicatorGlobalDir);
    }

    public void RepositionBlocks(float blockSpacing)
    {
        Vector3 spawnDirection = GetSpawnDirection();
        int blockIndex = 0;
        if (PendingBlockDatas != null && PendingBlockDatas.Count > 0)
        {
            for (int c = 0; c < PendingBlockDatas.Count; c++)
            {
                if (c >= MaxInitialPendingDataLimit) break;
                var pending = PendingBlockDatas[c];
                if (pending == null) continue;
                for (int r = 0; r < pending.StackCt; r++)
                {
                    if (blockIndex < CurBlocks.Count)
                    {
                        Block block = CurBlocks[blockIndex];
                        if (block != null)
                        {
                            block.transform.localPosition = spawnDirection * (c * blockSpacing) + Vector3.back * (r * blockSpacing);
                            block.transform.localRotation = Quaternion.identity;
                        }
                        blockIndex++;
                    }
                }
            }
        }
        
        // Fallback for any extra blocks
        for (int i = blockIndex; i < CurBlocks.Count; i++)
        {
            if (CurBlocks[i] != null)
            {
                CurBlocks[i].transform.localPosition = spawnDirection * (i * blockSpacing);
                CurBlocks[i].transform.localRotation = Quaternion.identity;
            }
        }
    }

    public void InitializeStack(List<int> colors, Block blockPrefab, float blockSpacing, bool isInitializeStart = false, List<int> columns = null)
    {
        _cachedBlockPrefab = blockPrefab;
        if (colors == null || colors.Count == 0 || blockPrefab == null) return;

        for (int i = 0; i < colors.Count; i++)
        {
            Block newBlock = Instantiate(blockPrefab, this.transform);
            newBlock.Init(colors[i], this);
            if (columns != null && i < columns.Count)
            {
                newBlock.ColumnIndex = columns[i];
            }
            CurBlocks.Add(newBlock);
        }
        RepositionBlocks(blockSpacing);
        if (isInitializeStart) CurBlocks.Reverse();

        // Initialize queue tracking from columns
        _activeColOrder.Clear();
        if (columns != null)
        {
            foreach (var col in columns)
            {
                if (!_activeColOrder.Contains(col))
                {
                    _activeColOrder.Add(col);
                }
            }
            _activeColOrder.Sort((a, b) => b.CompareTo(a));
        }
        _nextPendingDataIndex = _activeColOrder.Count;
    }

    public void InitializeStackFromPending(Block blockPrefab, float blockSpacing, bool isInitializeStart = false)
    {
        List<int> colors = new List<int>();
        List<int> columns = new List<int>();
        if (PendingBlockDatas != null)
        {
            int pendingCount = 0;
            for (int c = 0; c < PendingBlockDatas.Count; c++)
            {
                var pending = PendingBlockDatas[c];
                if (pending != null)
                {
                    if (pendingCount >= MaxInitialPendingDataLimit)
                        break;

                    for (int i = 0; i < pending.StackCt; i++)
                    {
                        colors.Add(pending.BlockCol);
                        columns.Add(c);
                    }
                    pendingCount++;
                }
            }
        }
        InitializeStack(colors, blockPrefab, blockSpacing, isInitializeStart, columns);
    }

    public void ShiftBlocksForward()
    {
        // 1. Remove any columns from _activeColOrder that are no longer present in CurBlocks
        List<int> colsToRemove = new List<int>();
        foreach (var col in _activeColOrder)
        {
            bool hasBlocks = false;
            foreach (var block in CurBlocks)
            {
                if (block != null && block.ColumnIndex == col)
                {
                    hasBlocks = true;
                    break;
                }
            }
            if (!hasBlocks)
            {
                colsToRemove.Add(col);
            }
        }
        foreach (var col in colsToRemove)
        {
            _activeColOrder.Remove(col);
        }

        // 2. Spawn new columns from PendingBlockDatas if there is space
        if (PendingBlockDatas != null && _cachedBlockPrefab != null)
        {
            while (_activeColOrder.Count < MaxInitialPendingDataLimit && _nextPendingDataIndex < PendingBlockDatas.Count)
            {
                var pending = PendingBlockDatas[_nextPendingDataIndex];
                if (pending != null)
                {
                    for (int i = 0; i < pending.StackCt; i++)
                    {
                        Block newBlock = Instantiate(_cachedBlockPrefab, this.transform);
                        newBlock.Init(pending.BlockCol, this);
                        newBlock.ColumnIndex = _nextPendingDataIndex;
                        
                        // Set initial local position of the newly spawned block at the back of the queue
                        int newColTarget = MaxInitialPendingDataLimit - 1 - _activeColOrder.Count;
                        Vector3 startPos = GetSpawnDirection() * (newColTarget * _spacingBlock) + Vector3.back * (i * _spacingBlock);
                        newBlock.transform.localPosition = startPos;

                        CurBlocks.Add(newBlock);
                    }
                    _activeColOrder.Add(_nextPendingDataIndex);
                }
                _nextPendingDataIndex++;
            }
        }

        // 3. Position the blocks
        if (CurBlocks == null || CurBlocks.Count == 0) return;

        // Group remaining blocks by their original ColumnIndex
        Dictionary<int, List<Block>> blocksByCol = new Dictionary<int, List<Block>>();
        foreach (var block in CurBlocks)
        {
            if (block == null) continue;
            if (!blocksByCol.ContainsKey(block.ColumnIndex))
            {
                blocksByCol[block.ColumnIndex] = new List<Block>();
            }
            blocksByCol[block.ColumnIndex].Add(block);
        }

        // Find the maximum possible column index
        int maxColIndex = 0;
        if (PendingBlockDatas != null && PendingBlockDatas.Count > 0)
        {
            maxColIndex = Mathf.Min(PendingBlockDatas.Count, MaxInitialPendingDataLimit) - 1;
        }

        Vector3 spawnDirection = GetSpawnDirection();

        // Map active columns in _activeColOrder to target columns
        for (int i = 0; i < _activeColOrder.Count; i++)
        {
            int colIndex = _activeColOrder[i];
            int targetCol = maxColIndex - i;

            if (blocksByCol.ContainsKey(colIndex))
            {
                List<Block> colBlocks = blocksByCol[colIndex];
                // Sort blocks in this column so that the front-most block (smaller Z offset / smaller dot with Vector3.back) comes first.
                colBlocks.Sort((a, b) =>
                {
                    float dotA = Vector3.Dot(a.transform.localPosition, Vector3.back);
                    float dotB = Vector3.Dot(b.transform.localPosition, Vector3.back);
                    return dotA.CompareTo(dotB);
                });

                for (int r = 0; r < colBlocks.Count; r++)
                {
                    Block block = colBlocks[r];
                    Vector3 targetLocalPos = spawnDirection * (targetCol * _spacingBlock) + Vector3.back * (r * _spacingBlock);

                    if (Application.isPlaying)
                    {
                        LeanTween.cancel(block.gameObject);
                        LeanTween.moveLocal(block.gameObject, targetLocalPos, 0.25f)
                            .setEase(LeanTweenType.easeOutQuad);
                    }
                    else
                    {
                        block.transform.localPosition = targetLocalPos;
                    }
                }
            }
        }
    }

    public void ClearAllBlocks()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Block block = transform.GetChild(i).GetComponent<Block>();
            if (block != null)
            {
                if (Application.isPlaying)
                    Destroy(block.gameObject);
                else
                    DestroyImmediate(block.gameObject);
            }
        }

        CurBlocks.Clear();
        _activeColOrder.Clear();
        _nextPendingDataIndex = 0;
    }
}
