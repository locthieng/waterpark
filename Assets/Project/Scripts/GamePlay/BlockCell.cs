using UnityEngine;
using System.Collections.Generic;

public class BlockCell : MonoBehaviour
{
    public BlockCellType CellType = BlockCellType.BlockSimple;

    public float PathDistForCollect;

    public float SpawnerDirectionAngleZ = 90f;

    public float SpawnWaitTimeAfterFill = 0.1f;

    public BlockCellAccessType AccessType { get; private set; } = BlockCellAccessType.NotAccessible;

    public List<Block> CurBlocks = new List<Block>();

    public List<PendingBlockData> PendingBlockDatas = new List<PendingBlockData>();

    public bool IsEmpty => CurBlocks.Count == 0;

    public float _spacingBlock = 1f;

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

    public void InitializeStack(List<int> colors, Block blockPrefab, float blockSpacing, bool isInitializeStart = false)
    {
        if (colors == null || colors.Count == 0 || blockPrefab == null) return;

        for (int i = 0; i < colors.Count; i++)
        {
            Block newBlock = Instantiate(blockPrefab, this.transform);
            newBlock.Init(colors[i], this);
            CurBlocks.Add(newBlock);
        }
        RepositionBlocks(blockSpacing);
        if (isInitializeStart) CurBlocks.Reverse();
    }

    public void InitializeStackFromPending(Block blockPrefab, float blockSpacing, bool isInitializeStart = false)
    {
        List<int> colors = new List<int>();
        if (PendingBlockDatas != null)
        {
            foreach (var pending in PendingBlockDatas)
            {
                if (pending != null)
                {
                    for (int i = 0; i < pending.StackCt; i++)
                    {
                        colors.Add(pending.BlockCol);
                    }
                }
            }
        }
        InitializeStack(colors, blockPrefab, blockSpacing, isInitializeStart);
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
    }
}
