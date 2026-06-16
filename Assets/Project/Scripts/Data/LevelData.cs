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

[CreateAssetMenu(fileName = "Level_Data", menuName = "Waterpark/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    public int levelIndex;
    public GameDifficult _gameDifficulty = GameDifficult.Easy;
    public Mesh _meshBezierSpline;

}
