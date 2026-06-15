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
}
