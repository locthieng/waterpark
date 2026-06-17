using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockCellType
{
    Empty,
    BlockSpawner,// Have arrow, have pendingblockdata
    BlockSimple,// No arrow, no pendingblockdata
    BlockAccessible// flexible, no arrow, have pendingblockdata
}

[Serializable]
public class BlockCellData
{
    public BlockCellType CurBlockCellType;

    public Vector3 CellPos;

    public Quaternion CellRot;

    public Vector3 CellScale;

    public int BlockColor;

    public List<PendingBlockData> PendingBlockDataArr;

    public float PathDistForCollect;

    public float SpawnerDirectionAngleZ;
}
