using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


public class InGameController : Singleton<InGameController>
{
    public SpotsController SpotsController;

    public BlockCellController BlockCellController;

    public HoleController HoleController;

    public PathMoveController PathMoveController;

    public Block _blockPrefab;

    public GameDataBase _GameDataBase;

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        BlockCellController.SetUp();
        HoleController.SetUp();
    }    
}
