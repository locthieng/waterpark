using System;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

[Serializable]
public struct LevelObjectData
{
    public string prefabIdentifier; // Name or ID of the prefab (e.g. "Hole", "BoosterItem")
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public string customData; // JSON or string for object-specific settings
}

[Serializable]
public enum DifficultLevel
{
    VeryEasy,
    Easy,
    Medium,
    Hard,
    SuperHard
}

[Serializable]
public struct SplinePointData
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;

    public BezierPoint.HandleMode handleMode;

    public Vector3 precedingControlPointLocalPosition;
    public Vector3 followingControlPointLocalPosition;

    public Vector3 normal;
    public float autoCalculatedNormalAngleOffset;

    public BezierPoint.ExtraData extraData;
}

[Serializable]
public struct LevelSplineData
{
    public string splineName; // Name of the spline GameObject
    public bool loop;
    public SplineAutoConstructMode autoConstructMode;
    public bool autoCalculateNormals;
    public float autoCalculatedNormalsAngle;
    public int autoCalculatedIntermediateNormalsCount;
    public float evenlySpacedPointsResolution;
    public float evenlySpacedPointsAccuracy;
    public int pointCacheResolution;

    public List<SplinePointData> points;
}

[CreateAssetMenu(fileName = "Level_Data", menuName = "Waterpark/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Configurations")]
    public int levelIndex;
    public DifficultLevel difficulty = DifficultLevel.Easy;
    public bool isTutorial;

    [Header("Splines (Máng trượt)")]
    public List<LevelSplineData> splines = new List<LevelSplineData>();

    [Header("Level Objects (Hố, Chướng ngại vật, Item,...)")]
    public List<LevelObjectData> placementObjects = new List<LevelObjectData>();
}
