using System;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

[Serializable]
public enum DifficultLevel
{
    VeryEasy,
    Easy,
    Medium,
    Hard,
    SuperHard
}

[CreateAssetMenu(fileName = "Level_Data", menuName = "Waterpark/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Configurations")]
    public int levelIndex;
    public DifficultLevel difficulty = DifficultLevel.Easy;
    public Mesh _meshBezierSpline;
}
