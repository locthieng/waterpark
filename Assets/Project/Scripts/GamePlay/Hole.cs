using UnityEngine;
using BezierSolution;
using System;

public class Hole : MonoBehaviour
{
    private float _curPathTravelDist;

    public int _ColorID;
    public HoldState _holdState;
    public HoldType _holdType;
    public int _holeCapacity;

    private Spot _CurSpot;
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
        }
    }

    private void Update()
    {
        //if (walker.enabled == true)
        //{
        //    Debug.Log("_CurPathTravelDist = " + GetCurPathTravelDist());
        //}    
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
            _curPathTravelDist = spline.evenlySpacedPoints.GetPercentageAtNormalizedT(walker.NormalizedT) * spline.evenlySpacedPoints.splineLength;
        }
        else
        {
            _curPathTravelDist = 0f;
        }
        return _curPathTravelDist;
    }    
}
