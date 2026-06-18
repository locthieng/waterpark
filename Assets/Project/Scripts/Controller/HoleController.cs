using UnityEngine;
using System.Collections.Generic;

public enum HoldState
{
    Lock,
    Unlock,
    OnSpot,
    OnBezier,
    CoupleHold,
    Completed
}

public enum HoldType
{
    Normal,
    Mystery,
    Link
}

public class HoleController : MonoBehaviour
{
    public List<Hole> AllHoles = new List<Hole>();

    public void SetUp()
    {
        for (int i = 0; i < AllHoles.Count; i++) { AllHoles[i].SetUp(); }
    }    

}
