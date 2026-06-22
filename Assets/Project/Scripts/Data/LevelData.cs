using System;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

[Serializable]
public enum GameDifficult
{
    VeryEasy,
    Easy,
    Medium,
    Hard,
    SuperHard
}

[Serializable]
public class BezierPointData
{
    public Vector3 localPosition;
    public Vector3 precedingControlPointLocalPosition;
    public Vector3 followingControlPointLocalPosition;
    public BezierPoint.HandleMode handleMode;
}

[CreateAssetMenu(fileName = "Level_Data", menuName = "Waterpark/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    public int levelIndex;
    public GameDifficult _gameDifficulty = GameDifficult.Easy;
    public Mesh _meshBezierSpline;
    
    public bool _isBezierLoop;
    public List<BezierPointData> _bezierPoints = new List<BezierPointData>();
}
