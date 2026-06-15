using UnityEngine;

public class Spot : MonoBehaviour
{
    private Hole CurHole;
    public SpotState _SpotState;

    public void SetHole(Hole hole)
    {
        CurHole = hole;
        _SpotState = hole != null ? SpotState.HoleOnSpot : SpotState.EmptySpot;
    }
}
