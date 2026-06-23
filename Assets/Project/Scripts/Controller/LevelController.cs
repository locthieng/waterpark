using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using BezierSolution;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class LevelController : Singleton<LevelController>
{
    // Đã đồng nhất sử dụng levelAssetPath cho toàn bộ class
    private const string levelAssetPath = "StageLevel/";
    private const string levelPrefix = "Level_";

    [HideInInspector] public int CurrentLevel = 0;
    [HideInInspector] public int levelIndex;

    // public List<LevelAsset> ListLevelSpecials = new List<LevelAsset>();

    private LevelData loadedLevelData;

    protected override void Awake()
    {
        base.Awake();
    }

    public int GetNextLevelInOrder()
    {
        if (GlobalController.Instance.ForTesting) return GlobalController.CurrentLevelIndex + 1;
        DataController.Instance.Data.Levels.Sort(SortLevelsAccending);
        int level = 1;
        for (int i = 0; i < DataController.Instance.Data.Levels.Count; i++)
        {
            level = DataController.Instance.Data.Levels[i] + 1;
            if (!DataController.Instance.Data.Levels.Contains(level))
            {
                return level;
            }
        }
        return level;
    }

    private int SortLevelsAccending(int x, int y)
    {
        if (x < y) return -1;
        return 1;
    }

    public int GetCurrentLevel()
    {
        levelIndex = GlobalController.CurrentLevelIndex % StageController.Instance.LevelLimit;
        if (levelIndex == 0)
        {
            levelIndex = StageController.Instance.LevelLimit;
        }
        return levelIndex;
    }

    public int LoadLevel(int level, int levelLimit, bool forcedChange = false)
    {
        levelIndex = level % levelLimit;
        if (levelIndex == 0)
        {
            levelIndex = levelLimit;
        }

        loadedLevelData = Resources.Load<LevelData>(levelAssetPath + levelPrefix + levelIndex);

        CurrentLevel = levelIndex;

        LoadSplineAndMesh(loadedLevelData);

        return level;
    }

    public void LoadSplineAndMesh(LevelData levelData)
    {
        if (levelData == null) return;

        SplineMeshBuilder splineBuilder = UnityEngine.Object.FindFirstObjectByType<SplineMeshBuilder>();
        if (splineBuilder != null)
        {
            // 1. Replace points in BezierSpline
            if (splineBuilder.spline != null && levelData._bezierPoints != null && levelData._bezierPoints.Count >= 2)
            {
                BezierSpline spline = splineBuilder.spline;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(spline, "Load Spline Points");
#endif

                spline.loop = levelData._isBezierLoop;

                List<BezierPoint> currentPoints = new List<BezierPoint>();
                spline.GetComponentsInChildren(currentPoints);

                int targetCount = levelData._bezierPoints.Count;

                // Destroy extra points (detach parent first to prevent GetComponentsInChildren from finding them)
                for (int i = currentPoints.Count - 1; i >= targetCount; i--)
                {
                    currentPoints[i].transform.SetParent(null);
                    if (Application.isPlaying)
                    {
                        Destroy(currentPoints[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(currentPoints[i].gameObject);
                    }
                    currentPoints.RemoveAt(i);
                }

                // Add missing points
                while (currentPoints.Count < targetCount)
                {
                    BezierPoint point = spline.InsertNewPointAt(spline.Count);
                    currentPoints.Add(point);
                }

                // Apply saved positions and handle modes
                for (int i = 0; i < targetCount; i++)
                {
                    BezierPoint point = currentPoints[i];
                    BezierPointData data = levelData._bezierPoints[i];

#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(point, "Load Spline Point Data");
                    UnityEditor.Undo.RecordObject(point.transform, "Load Spline Point Transform");
#endif

                    point.localPosition = data.localPosition;
                    point.precedingControlPointLocalPosition = data.precedingControlPointLocalPosition;
                    point.followingControlPointLocalPosition = data.followingControlPointLocalPosition;
                    point.handleMode = data.handleMode;
                }

                spline.Refresh();
            }

            // 2. Replace mesh of SplineMeshBuilder
            MeshFilter meshFilter = splineBuilder.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh savedMesh = null;
#if UNITY_EDITOR
                string assetPath = "Assets/Project/Resources/MeshBezier/Level_" + levelData.levelIndex + "_Mesh.asset";
                savedMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
#else
                savedMesh = Resources.Load<Mesh>("MeshBezier/Level_" + levelData.levelIndex + "_Mesh");
#endif
                if (savedMesh != null)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(meshFilter, "Load Saved Mesh");
#endif
                    meshFilter.sharedMesh = savedMesh;
                }
                else if (levelData._meshBezierSpline != null)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(meshFilter, "Load Saved Mesh");
#endif
                    meshFilter.sharedMesh = levelData._meshBezierSpline;
                }
                else
                {
                    Debug.LogWarning("Could not find saved mesh for level " + levelData.levelIndex + " in resources/MeshBezier.");
                }
            }
        }
    }

#if UNITY_EDITOR
    public void EditorAddLevel()
    {
        Instance = this;
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { ("Assets/Project/Resources/" + levelAssetPath).TrimEnd('/') });
        CurrentLevel = guids.Length + 1;

        LevelData newLevelData = ScriptableObject.CreateInstance<LevelData>();
        newLevelData.levelIndex = CurrentLevel;

        string assetFolder = "Assets/Project/Resources/" + levelAssetPath;
        if (!System.IO.Directory.Exists(assetFolder))
        {
            System.IO.Directory.CreateDirectory(assetFolder);
        }

        string assetPath = assetFolder + levelPrefix + CurrentLevel + ".asset";

        AssetDatabase.CreateAsset(newLevelData, assetPath);
        EditorSaveLevel();

        Debug.Log("Tạo Level mới thành công tại: " + assetPath);
    }

    public void EditorLoadLevel(int level)
    {
        Instance = this;

        string assetPath = "Assets/Project/Resources/" + levelAssetPath + levelPrefix + level + ".asset";
        LevelData loadedData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
        if (loadedData == null)
        {
            Debug.LogWarning("No LevelData asset found at " + assetPath);

            if (level != 1)
            {
                CurrentLevel = 1;
                EditorLoadLevel(1);
            }
            return;
        }

        CurrentLevel = level;
        LoadSplineAndMesh(loadedData);
    }

    public void EditorLoadPrevLevel()
    {
        CurrentLevel--;
        if (CurrentLevel < 1) CurrentLevel = 1;
        EditorLoadLevel(CurrentLevel);
    }

    public void EditorLoadNextLevel()
    {
        CurrentLevel++;
        EditorLoadLevel(CurrentLevel);
    }

    public void EditorCloneLevel(int level)
    {
        Instance = this;
        EditorSaveLevel();
        // Sửa toàn bộ thành levelAssetPath để nhân bản đúng mục tiêu
        string sourcePath = "Assets/Project/Resources/" + levelAssetPath + levelPrefix + level + ".asset";
        LevelData sourceData = AssetDatabase.LoadAssetAtPath<LevelData>(sourcePath);
        if (sourceData == null)
        {
            Debug.LogError("No source level found to clone at: " + sourcePath);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { ("Assets/Project/Resources/" + levelAssetPath).TrimEnd('/') });
        CurrentLevel = guids.Length + 1;
        string newPath = "Assets/Project/Resources/" + levelAssetPath + levelPrefix + CurrentLevel + ".asset";

        AssetDatabase.CopyAsset(sourcePath, newPath);
        AssetDatabase.Refresh();
        LevelData newLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(newPath);
        newLevelData.levelIndex = CurrentLevel;
        EditorUtility.SetDirty(newLevelData);
        EditorSaveLevel();
    }

    public void EditorSaveLevel()
    {
        Instance = this;

        // Sửa thành levelAssetPath để tạo đúng thư mục lưu trữ StageLevel
        string assetFolder = "Assets/Project/Resources/" + levelAssetPath;
        if (!System.IO.Directory.Exists(assetFolder))
        {
            System.IO.Directory.CreateDirectory(assetFolder);
        }

        // Find SplineMeshBuilder and generate/save its mesh
        SplineMeshBuilder splineBuilder = UnityEngine.Object.FindFirstObjectByType<SplineMeshBuilder>();
        if (splineBuilder != null && splineBuilder.spline != null)
        {
            // Auto generate mesh
            splineBuilder.GenerateMesh();

            // Get active level data
            string activeAssetPath = "Assets/Project/Resources/" + levelAssetPath + levelPrefix + CurrentLevel + ".asset";
            LevelData activeData = AssetDatabase.LoadAssetAtPath<LevelData>(activeAssetPath);
            if (activeData != null)
            {
                // Save spline points to LevelData
                Undo.RecordObject(activeData, "Save Spline Points to LevelData");
                activeData._isBezierLoop = splineBuilder.spline.loop;
                activeData._bezierPoints.Clear();
                for (int i = 0; i < splineBuilder.spline.Count; i++)
                {
                    var pt = splineBuilder.spline[i];
                    if (pt != null)
                    {
                        var ptData = new BezierPointData
                        {
                            localPosition = pt.localPosition,
                            precedingControlPointLocalPosition = pt.precedingControlPointLocalPosition,
                            followingControlPointLocalPosition = pt.followingControlPointLocalPosition,
                            handleMode = pt.handleMode
                        };
                        activeData._bezierPoints.Add(ptData);
                    }
                }

                // Save mesh asset
                MeshFilter filter = splineBuilder.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    string folderPath = "Assets/Project/Resources/MeshBezier";
                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    string targetMeshName = $"Level_{CurrentLevel}_Mesh";
                    string meshPath = $"{folderPath}/{targetMeshName}.asset";
                    Mesh targetMesh = activeData._meshBezierSpline;

                    if (targetMesh == null)
                    {
                        targetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    }

                    if (targetMesh == null)
                    {
                        targetMesh = new Mesh();
                        targetMesh.name = targetMeshName;
                        CopyMeshPropertiesInController(filter.sharedMesh, targetMesh);
                        AssetDatabase.CreateAsset(targetMesh, meshPath);
                        Debug.Log($"Created new mesh asset at: {meshPath}");
                    }
                    else
                    {
                        Undo.RecordObject(targetMesh, "Update Spline Mesh");
                        CopyMeshPropertiesInController(filter.sharedMesh, targetMesh);
                        EditorUtility.SetDirty(targetMesh);
                        Debug.Log($"Updated mesh asset at: {meshPath}");
                    }

                    activeData._meshBezierSpline = targetMesh;
                    filter.sharedMesh = targetMesh;
                }
                EditorUtility.SetDirty(activeData);
            }
        }

        // Tự động sửa levelIndex của tất cả file LevelData dựa theo tên file
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { assetFolder.TrimEnd('/') });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data != null)
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                if (filename.StartsWith(levelPrefix))
                {
                    string numStr = filename.Substring(levelPrefix.Length);
                    if (int.TryParse(numStr, out int parsedLevel))
                    {
                        if (data.levelIndex != parsedLevel)
                        {
                            data.levelIndex = parsedLevel;
                            EditorUtility.SetDirty(data);
                        }
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CopyMeshPropertiesInController(Mesh source, Mesh destination)
    {
        if (source == null || destination == null) return;
        if (source == destination) return;

        destination.Clear();
        destination.vertices = source.vertices;
        destination.triangles = source.triangles;
        destination.uv = source.uv;
        destination.normals = source.normals;
        destination.tangents = source.tangents;
        destination.colors = source.colors;
        destination.bounds = source.bounds;
    }
}
#endif