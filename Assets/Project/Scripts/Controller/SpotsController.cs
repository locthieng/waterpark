using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum SpotState
{
    EmptySpot,
    HoleOnSpot
}

public class SpotsController : MonoBehaviour
{
    public Transform _spotPoint;

    public List<Spot> AllSpots;

    public Spot _spotPrefab;

    public void ShiftHolesForward(int startIndex)
    {
        for (int j = startIndex + 1; j < AllSpots.Count; j++)
        {
            Spot currentSpot = AllSpots[j];
            Spot targetSpot = AllSpots[j - 1];

            Hole holeToMove = currentSpot.CurHole;
            if (holeToMove != null)
            {
                currentSpot.SetHole(null);
                targetSpot.SetHole(holeToMove);
                holeToMove.MoveToSpot(targetSpot);
            }
        }
    }

}
