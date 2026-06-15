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
        return level;
    }

#if UNITY_EDITOR
    public void EditorAddLevel()
    {
        Instance = this;
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Project/Resources/" + levelAssetPath });
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
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

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

        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Project/Resources/" + levelAssetPath });
        CurrentLevel = guids.Length + 1;
        string newPath = "Assets/Project/Resources/" + levelAssetPath + levelPrefix + CurrentLevel + ".asset";

        AssetDatabase.CopyAsset(sourcePath, newPath);
        AssetDatabase.Refresh();
        LevelData newLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(newPath);
        newLevelData.levelIndex = CurrentLevel;
        EditorUtility.SetDirty(newLevelData);
        AssetDatabase.SaveAssets();
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
        //string assetPath = assetFolder + Level.name + ".asset";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //Debug.Log("Level saved successfully as ScriptableObject: " + assetPath);
    }
}
#endif