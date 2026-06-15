using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelController))]
public class LevelEditor : Editor
{
    private LevelController levelController;
    private LevelData currentLevelData;
    private Editor levelDataEditor;

    private void OnEnable()
    {
        levelController = (LevelController)target;
        LoadLevelDataEditor();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        if (levelDataEditor != null)
        {
            DestroyImmediate(levelDataEditor);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Application.isPlaying) return;
    }

    private void LoadLevelDataEditor()
    {
        if (levelController == null) return;

        string path = $"Assets/Project/Resources/StageLevel/Level_{levelController.CurrentLevel}.asset";
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (data != currentLevelData)
        {
            currentLevelData = data;
            if (levelDataEditor != null) DestroyImmediate(levelDataEditor);

            if (currentLevelData != null)
            {
                levelDataEditor = Editor.CreateEditor(currentLevelData);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        levelController = (LevelController)target;

        GUIStyle headerStyle = new GUIStyle();
        headerStyle.richText = true;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.cyan;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("LEVEL CONTROLS", headerStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        levelController.CurrentLevel = EditorGUILayout.IntField(levelController.CurrentLevel, GUILayout.Width(50), GUILayout.Height(25));
        if (EditorGUI.EndChangeCheck())
        {
            LoadLevelDataEditor();
        }

        if (GUILayout.Button("LOAD", GUILayout.Width(60), GUILayout.Height(25)))
        {
            levelController.EditorLoadLevel(levelController.CurrentLevel);
            LoadLevelDataEditor();
        }

        if (GUILayout.Button("<<", GUILayout.Width(40), GUILayout.Height(25)))
        {
            levelController.EditorLoadPrevLevel();
            LoadLevelDataEditor();
        }

        if (GUILayout.Button(">>", GUILayout.Width(40), GUILayout.Height(25)))
        {
            levelController.EditorLoadNextLevel();
            LoadLevelDataEditor();
        }

        if (GUILayout.Button("CLONE", GUILayout.MaxWidth(80), GUILayout.Height(25)))
        {
            levelController.EditorCloneLevel(levelController.CurrentLevel);
            LoadLevelDataEditor();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE LEVEL", GUILayout.Width(150), GUILayout.Height(40)))
        {
            levelController.EditorSaveLevel();
            LoadLevelDataEditor();
        }

        if (GUILayout.Button("GENERATE SPLINE MESHES", GUILayout.Width(180), GUILayout.Height(40)))
        {
            levelController.EditorGenerateAllSplineMeshes();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"LEVEL DATA (Level_{levelController.CurrentLevel})", headerStyle);
        EditorGUILayout.Space(5);

        if (currentLevelData != null && levelDataEditor != null)
        {
            EditorGUI.BeginChangeCheck();

            levelDataEditor.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentLevelData);
                AssetDatabase.SaveAssets(); 
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"Không tìm thấy file Asset nào cho Level {levelController.CurrentLevel} " +
                $"tại thư mục Assets/Project/Resources/StageLevel/. Sếp hãy bấm SAVE LEVEL để tạo mới.", MessageType.Warning);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelController);
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(levelController.gameObject.scene);
            }
        }
    }
}