using UnityEngine;

public class Spot : MonoBehaviour
{
    public Hole CurHole { get; private set; }
    public SpotState _SpotState;

    public void SetHole(Hole hole)
    {
        CurHole = hole;
        _SpotState = hole != null ? SpotState.HoleOnSpot : SpotState.EmptySpot;
    }
}
