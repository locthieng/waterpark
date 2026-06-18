using UnityEngine;
using System.Collections.Generic;

public class BlockCell : MonoBehaviour
{
    public BlockCellType CellType = BlockCellType.BlockSimple;

    public float PathDistForCollect;

    public float SpawnerDirectionAngleZ => _RotationAngleIndicator.z;

    public float SpawnWaitTimeAfterFill = 0.1f;

    public BlockCellAccessType AccessType { get; private set; } = BlockCellAccessType.NotAccessible;

    public List<Block> CurBlocks { get; private set; } = new List<Block>();

    public List<int> BlockColorList; // num block can appear in blockCell when start

    public List<BlockCell> ParentBlockCells = new List<BlockCell>();

    public List<BlockCell> ChildBlockCells = new List<BlockCell>();

    public Block TopBlock => CurBlocks.Count > 0 ? CurBlocks[CurBlocks.Count - 1] : null;

    public int TopColorID => TopBlock != null ? TopBlock.ColorID : -1;

    public int CurVisibleBlockCt => CurBlocks.Count;

    public bool IsEmpty => CurBlocks.Count == 0;

    public float _spacingBlock = 1f;

    [SerializeField] private GameObject _Indicator;
    [SerializeField] private GameObject _ParentIndicator;

    private Vector3 _RotationAngleIndicator => _Indicator.transform.rotation.eulerAngles;

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
        Debug.Log("spawnDirection = " + GetSpawnDirection());
        for (int i = 0; i < CurBlocks.Count; i++)
        {
            if (CurBlocks[i] == null) continue;
            CurBlocks[i].transform.localPosition = spawnDirection * (i * blockSpacing);
            CurBlocks[i].transform.localRotation = Quaternion.identity;
        }
    }

    public void InitializeStack(List<int> colors, Block blockPrefab, float blockSpacing)
    {
        //ClearAllBlocks();

        if (colors == null || colors.Count == 0 || blockPrefab == null) return;

        for (int i = 0; i < colors.Count; i++)
        {
            Block newBlock = Instantiate(blockPrefab, this.transform);
            newBlock.Init(colors[i], this);
            CurBlocks.Add(newBlock);
        }
        Debug.Log("InitializeStack  +2");
        RepositionBlocks(blockSpacing);
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
        BlockColorList.Clear();
    }

    public void AddBlockToBottom(Block block)
    {
        if (block == null) return;

        block.transform.SetParent(this.transform);
        CurBlocks.Insert(0, block);
        BlockColorList.Insert(0, block.ColorID);
    }

    public void AddBlockToTop(Block block)
    {
        if (block == null) return;

        block.transform.SetParent(this.transform);
        CurBlocks.Add(block);
        BlockColorList.Add(block.ColorID);
    }

    public Block RemoveTopBlock()
    {
        if (IsEmpty) return null;

        Block top = TopBlock;
        CurBlocks.RemoveAt(CurBlocks.Count - 1);
        
        if (BlockColorList.Count > 0)
        {
            BlockColorList.RemoveAt(BlockColorList.Count - 1);
        }

        return top;
    }
}
