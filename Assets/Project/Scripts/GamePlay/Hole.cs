using UnityEngine;
using BezierSolution;

public class Hole : MonoBehaviour
{
    public HoldState _holdState;
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

    private void OnMouseDown()
    {
    }

    private void OnMouseUp()
    {
        if (walker != null)
        {
            walker.NormalizedT = 0f; 
            walker.enabled = true;
            _holdState = HoldState.OnBezier;
            if (_CurSpot != null)
            {
                _CurSpot._SpotState = SpotState.EmptySpot;
                _CurSpot = null;
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
            emptySpot.SetHole(this);
            LeanTween.move(gameObject, emptySpot.transform.position, 0.5f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    _holdState = HoldState.OnSpot;
                    _CurSpot = emptySpot;
                    emptySpot._SpotState = SpotState.HoleOnSpot;
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
}
