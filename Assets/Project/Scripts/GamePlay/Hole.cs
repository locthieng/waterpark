using BezierSolution;
using Common.Helper;
using System;
using UnityEngine;
using System.Collections.Generic;

public class Hole : MonoBehaviour
{
    private float _curPathTravelDist;

    public int _ColorID;

    public HoldState _holdState;

    public HoldType _holdType;

    public int _holeCapacity;

    private Spot _CurSpot;

    private int NextAccessibleCellIdx = 0;

    private int _flyingBlocksCount = 0;
    private bool _shouldDeactivateAfterFly = false;

    public Renderer _rendererHole;

    [Header("Bezier Movement Settings")]
    [Tooltip("The walker component that controls the object's movement along the spline")]
    public BezierWalker walker;

    [Tooltip("Should the walker be disabled on Start so it doesn't move immediately?")]
    public bool disableWalkerOnStart = true;

    private void Start()
    {
        // If no walker is assigned, try to get it from this GameObject
        if (walker == null)
        {
            walker = GetComponent<BezierWalker>();
        }

        if (walker != null)
        {
            // Disable the walker on startup if requested
            if (disableWalkerOnStart)
            {
                walker.enabled = false;
            }

            // Hook into the path completed event to automatically disable the walker when it reaches the end
            if (walker is BezierWalkerWithSpeed walkerSpeed)
            {
                walkerSpeed.onPathCompleted.AddListener(OnMovementCompleted);
            }
            else if (walker is BezierWalkerWithTime walkerTime)
            {
                walkerTime.onPathCompleted.AddListener(OnMovementCompleted);
            }

            // Tự động tính khoảng cách các cell dựa trên Spline của Hole tại Start
            if (walker.Spline != null && BlockCellController.Instance != null)
            {
                BlockCellController.Instance.InitializeCellDistances(walker.Spline);
            }
        }
    }

    private void Update()
    {
        if (_holdState == HoldState.OnBezier)
        {
            TryCheckDistanceRealTime(GetCurPathTravelDist());
        }
    }

    public void SetUp()
    {
        ApplyColorFromDatabase();
    }    

    private void TryCheckDistanceRealTime(float curTravelDist)
    {
        if (BlockCellController.Instance == null)
            return;

        BlockCell targetCell = BlockCellController.Instance.GetCellByIndex(NextAccessibleCellIdx);
        if (targetCell == null) 
            return;

        if (curTravelDist >= targetCell.GetPathDistForCollect())
        {
            
            if (targetCell.CurBlocks != null && targetCell.CurBlocks.Count > 0)
            {
                Block topBlock = targetCell.CurBlocks[0];
                if (topBlock != null && this._ColorID == topBlock.ColorID)
                {
                    List<Block> blocksToRemove = new List<Block>();
                    for (int i = 0; i < targetCell.CurBlocks.Count; i++)
                    {
                        Block block = targetCell.CurBlocks[i];
                        if (block == null) continue;

                        if (block.ColorID == this._ColorID)
                        {
                            _flyingBlocksCount++;
                            block.MoveAlongCurve(this.transform, () =>
                            {
                                _flyingBlocksCount--;
                                if (_shouldDeactivateAfterFly && _flyingBlocksCount <= 0)
                                {
                                    this.gameObject.SetActive(false);
                                }
                            });

                            blocksToRemove.Add(block);
                            Debug.Log($"[Block Collected] Block {i} (ColorID: {block.ColorID}) flying to hole and marked for removal.");
                            
                            _holeCapacity--;
                            if (_holeCapacity <= 0)
                            {
                                Debug.Log("[Hole Capacity Empty] Hole capacity reached 0, deactivating hole after fly completes.");
                                _shouldDeactivateAfterFly = true;
                                break;
                            }
                        }
                        else
                        {
                            Debug.Log($"[Stop Collect] Block {i} has different ColorID ({block.ColorID}), stopping.");
                            break;
                        }
                    }

                    foreach (var block in blocksToRemove)
                    {
                        targetCell.CurBlocks.Remove(block);
                    }

                    targetCell.ShiftBlocksForward();
                }
            }
            
            NextAccessibleCellIdx++;
        }
    }

    private void OnMouseDown()
    {
    }

    private void OnMouseUp()
    {
        if (!CanMoveOnBezier()) return;
        MoveOnBezier();
    }

    public bool CanMoveOnBezier()
    {
        if (_holdState != HoldState.OnSpot && _holdState != HoldState.Unlock) return false;

        return true;
    }

    public void MoveOnBezier()
    {
        if (walker != null)
        {
            walker.NormalizedT = 0f;
            walker.enabled = true;
            _holdState = HoldState.OnBezier;
            NextAccessibleCellIdx = 0;
            _flyingBlocksCount = 0;
            _shouldDeactivateAfterFly = false;

            if (walker.Spline != null && BlockCellController.Instance != null)
            {
                BlockCellController.Instance.InitializeCellDistances(walker.Spline);
            }

            if (_CurSpot != null)
            {
                var spotsController = InGameController.Instance.SpotsController;
                int spotIndex = spotsController.AllSpots.IndexOf(_CurSpot);
                if (spotIndex != -1)
                {
                    _CurSpot._SpotState = SpotState.EmptySpot;
                    _CurSpot.SetHole(null);
                    spotsController.ShiftHolesForward(spotIndex);
                }
            }
        }
        else
        {
            Debug.LogWarning("No BezierWalker component assigned to Hole script!", this);
        }
    }    

    private Vector3 _PosEndBezier => InGameController.Instance.SpotsController._spotPoint.position;
    private void OnMovementCompleted()
    {
        if (walker != null)
        {
            walker.enabled = false;
            transform.position = _PosEndBezier;
            FlyToEmptySpot();
        }
    }

    public void FlyToEmptySpot()
    {
        Spot emptySpot = GetEmptySpot();
        if (emptySpot != null)
        {
            _holdState = HoldState.OnSpot;
            _CurSpot = emptySpot;
            emptySpot.SetHole(this);
            LeanTween.move(gameObject, emptySpot.transform.position, 0.5f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                });
        }
        else
        {
            Debug.LogWarning("No empty spot found for Hole!", this);
            //Check lose
        }
    }

    private Spot GetEmptySpot()
    {
        if (InGameController.Instance != null && InGameController.Instance.SpotsController != null)
        {
            var spots = InGameController.Instance.SpotsController.AllSpots;
            if (spots != null)
            {
                foreach (var spot in spots)
                {
                    if (spot._SpotState == SpotState.EmptySpot)
                    {
                        return spot;
                    }
                }
            }
        }
        return null;
    }

    public void MoveToSpot(Spot newSpot)
    {
        _CurSpot = newSpot;
        if (newSpot != null)
        {
            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, newSpot.transform.position, 0.3f)
                .setEase(LeanTweenType.easeOutQuad);
        }
    }

    public float GetCurPathTravelDist()
    {
        if (walker != null && walker.Spline != null)
        {
            var spline = walker.Spline;
            _curPathTravelDist = spline.evenlySpacedPoints.GetPercentageAtNormalizedT(walker.NormalizedT) 
                * spline.evenlySpacedPoints.splineLength;
        }
        else
        {
            _curPathTravelDist = 0f;
        }
        return _curPathTravelDist;
    }

    private void ApplyColorFromDatabase()
    {
        if (_rendererHole == null) return;

        if (InGameController.Instance != null && InGameController.Instance._GameDataBase != null)
        {
            var colorList = InGameController.Instance._GameDataBase.Colors;

            if (_ColorID >= 0 && _ColorID < colorList.Count)
            {
                Color targetColor = colorList[_ColorID];

                _rendererHole.SetBaseColor(targetColor);
            }
            else
            {
                Debug.LogWarning($"ColorID {_ColorID} không có trong GameDataBase kìa!");
            }
        }
        else
        {
            Debug.LogWarning("Chưa có InGameController hoặc GameDataBase chưa được gán!");
        }
    }
}
