using UnityEngine;
using BezierSolution;

public enum HoleState
{
    None,
    OnSpot,
    OnPath
}

public class Hole : MonoBehaviour
{
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
        // When mouse is released, start the movement from the beginning
        if (walker != null)
        {
            walker.NormalizedT = 0f; // Start from the beginning (point 0)
            walker.enabled = true;   // Enable the walker component to start movement
        }
        else
        {
            Debug.LogWarning("No BezierWalker component assigned to Hole script!", this);
        }
    }

    private void OnMovementCompleted()
    {
        if (walker != null)
        {
            walker.enabled = false;
        }
    }
}