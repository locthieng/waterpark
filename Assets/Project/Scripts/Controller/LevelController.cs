using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    private const string levelPrefabPath = "StageLevel/";
    private const string levelAssetFolder = "Assets/Project/Resources/StageLevel/";
    private const string levelPrefabPathSpecial = "SpecialLevel/";
    private const string levelPrefabBasePath = "Base/";

    private const string levelPrefix = "Level_";
    [HideInInspector] public int CurrentLevel = 0;
    [HideInInspector] public SingleLevelController LoadedLevel;
    public SingleLevelController Level;
    [HideInInspector] public int levelIndex;
    [HideInInspector] public List<LevelAsset> ListLevelSpecials = new List<LevelAsset>();

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
        if (x < y)
        {
            return -1;
        }
        else
        {
            return 1;
        }
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

    /// <summary>
    /// Load a level map & return the map index
    /// </summary>
    /// <param name="level"></param>
    /// <param name="levelLimit"></param>
    /// <param name="forcedChange"></param>
    /// <returns></returns>
    public int LoadLevel(int level, int levelLimit, bool forcedChange = false)
    {
        Level = transform.GetComponentInChildren<SingleLevelController>();
        if (Level != null)
        {
            if (forcedChange)
            {
                Destroy(Level.gameObject);
            }
            else
            {
                //CurrentLevel = int.Parse(Level.name.Split('_')[1]);
                //LoadedLevel = Level;
                //return CurrentLevel; // For GD Test
            }
        }
        levelIndex = level % levelLimit;
        if (levelIndex == 0)
        {
            levelIndex = levelLimit;
        }
        switch (GlobalController.CurrentLevelType)
        {
            case LevelType.Main:
                LoadedLevel = Resources.Load<SingleLevelController>(levelPrefabPath + levelPrefix + levelIndex);
                break;
            case LevelType.Special:
                LoadedLevel = ListLevelSpecials[GlobalController.CurrentLevelSpecialIndex].Prefab;
                break;
            default:
                break;
        }

        CurrentLevel = levelIndex;
        return level;
    }

    public void SetUpLevel()
    {
        if (LoadedLevel != null && Level == null)
        {
            Level = Instantiate(LoadedLevel, transform);
        }
    }

#if UNITY_EDITOR
    public void EditorAddLevel()
    {
        Instance = this;
        // Count existing levels to determine new index
        SingleLevelController[] allLevels = Resources.LoadAll<SingleLevelController>(levelPrefabPath);
        CurrentLevel = allLevels.Length + 1;

        // Clone from current level or Level_1 as template
        Object sourcePrefab = null;
        if (Level != null)
        {
            Object prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(Level.gameObject);
            if (prefabParent != null)
            {
                PrefabUtility.SaveAsPrefabAsset(Level.gameObject, AssetDatabase.GetAssetPath(prefabParent));
                sourcePrefab = prefabParent;
            }
            DestroyImmediate(Level.gameObject);
        }

        if (sourcePrefab == null)
            sourcePrefab = Resources.Load(levelPrefabPath + levelPrefix + "1");

        if (sourcePrefab == null)
        {
            Debug.LogError("No source level prefab found to clone.");
            return;
        }

        // Copy and load new level
        string sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
        string newPath = levelAssetFolder + levelPrefix + CurrentLevel + ".prefab";
        AssetDatabase.CopyAsset(sourcePath, newPath);
        AssetDatabase.Refresh();

        GameObject g = PrefabUtility.InstantiatePrefab(Resources.Load(levelPrefabPath + levelPrefix + CurrentLevel), transform) as GameObject;
        Level = g.GetComponent<SingleLevelController>();
        Level.name = levelPrefix + CurrentLevel;
    }

    public void EditorLoadLevel(int level)
    {
        Instance = this;
        Level = transform.GetComponentInChildren<SingleLevelController>();
        if (Level != null)
        {
            Object prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(Level.gameObject);
            if (prefabParent != null)
            {
                PrefabUtility.SaveAsPrefabAsset(
                    Level.gameObject,
                    AssetDatabase.GetAssetPath(prefabParent));
            }
            DestroyImmediate(Level.gameObject);
            AssetDatabase.Refresh();
        }

        Object loadedPrefab = Resources.Load(levelPrefabPath + levelPrefix + level);
        if (loadedPrefab == null)
        {
            Debug.LogError("No prefab found at Resources/" + levelPrefabPath + levelPrefix + level);
            if (level != 1)
            {
                CurrentLevel = 1;
                EditorLoadLevel(1);
            }
            return;
        }

        GameObject g = PrefabUtility.InstantiatePrefab(loadedPrefab, transform) as GameObject;
        Level = g.GetComponent<SingleLevelController>();
        Level.name = levelPrefix + level;
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
        Level = transform.GetComponentInChildren<SingleLevelController>();
        if (Level != null)
        {
            Object prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(Level.gameObject);
            string basePath = AssetDatabase.GetAssetPath(prefabParent);
            basePath = basePath.Substring(0, basePath.Length - (levelPrefix + level).Length - ".prefab".Length);
            PrefabUtility.SaveAsPrefabAsset(
                Level.gameObject,
                AssetDatabase.GetAssetPath(prefabParent));
            CurrentLevel = Resources.LoadAll<SingleLevelController>(levelPrefabPath).Length + 1;
            string newPath = levelAssetFolder + levelPrefix + CurrentLevel + ".prefab";
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(prefabParent), newPath);
            DestroyImmediate(Level.gameObject);
            AssetDatabase.Refresh();
            GameObject g = PrefabUtility.InstantiatePrefab(Resources.Load(levelPrefabPath + levelPrefix + CurrentLevel), transform) as GameObject;
            Level = g.GetComponent<SingleLevelController>();
            Level.name = levelPrefix + CurrentLevel;
        }
    }

    public void EditorSaveLevel()
    {
        Instance = this;
        if (Level == null)
        {
            Level = transform.GetComponentInChildren<SingleLevelController>();
        }
        if (Level != null)
        {
            GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(Level.gameObject);
            if (prefabRoot != null)
            {
                PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            // Recheck all of level's objects
            //SaveGridDataToAsset();
            DoSaveAssetDatabase();
        }
    }

    private void DoSaveAssetDatabase()
    {
        Object instanceRoot = PrefabUtility.GetCorrespondingObjectFromSource(Level.gameObject);
        GameObject prefabParent = PrefabUtility.GetOutermostPrefabInstanceRoot(Level.gameObject);
        if (instanceRoot == null)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                Level.gameObject,
                levelAssetFolder + Level.name + ".prefab", InteractionMode.AutomatedAction);
        }
        else
        {
            PrefabUtility.SaveAsPrefabAsset(
                Level.gameObject,
                AssetDatabase.GetAssetPath(instanceRoot));
            PrefabUtility.RevertPrefabInstance(prefabParent, InteractionMode.AutomatedAction);
        }
        AssetDatabase.Refresh();
        EditorApplication.update -= DoSaveAssetDatabase;
    }

    public void EditorUpdateNumObstacle(int type)
    {
        Instance = this;

    }

    public void EditorUpdateNumFloor()
    {
        Instance = this;

    }

    public void EditorUpdateNumLoot(int type)
    {
        Instance = this;

    }

    public void EditorUpdateNumTrap(int type)
    {
        Instance = this;

    }

    public void EditorRelinkAllObstacles()
    {
        Instance = this;
        EditorUnpackPrefab(Level.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

    }

    public void EditorSaveAll()
    {
        Instance = this;

        int totalLevels = Resources.LoadAll<SingleLevelController>(levelPrefabPath).Length;
        for (int i = 1; i < totalLevels + 1; i++)
        {
            EditorLoadLevel(i);
            // DO SOMETHING WITH THE CURRENT LEVEL
            // -----------------------------------
            EditorSaveLevel();
        }
    }

    public void EditorUnpackPrefab(GameObject currentObject, PrefabUnpackMode unpackMode, InteractionMode interactionMode)
    {
        EditorSaveLevel();
        GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(currentObject);
        if (prefabRoot != null)
        {
            PrefabUtility.UnpackPrefabInstance(prefabRoot, unpackMode, interactionMode);
        }
    }

    public void RemoveSpecialLevel(int i)
    {
        ListLevelSpecials.RemoveAt(i);
    }

    public void AddSpecialLevel()
    {
        ListLevelSpecials.Add(new LevelAsset()
        {
            ID = ListLevelSpecials.Count
        });
    }

    internal void ShowHint()
    {
    }
#endif

    void OnEnable()
    {
        if (!Application.isPlaying)
        {
        }
    }
}

