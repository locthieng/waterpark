using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelController levelController;
    private int inputLevel = 1;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(200, 120);
        window.maxSize = new Vector2(400, 120);
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        levelController = FindObjectOfType<LevelController>();
        if (levelController != null)
            inputLevel = levelController.CurrentLevel < 1 ? 1 : levelController.CurrentLevel;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnHierarchyChanged()
    {
        if (levelController == null)
            levelController = FindObjectOfType<LevelController>();
        Repaint();
    }

    private void OnGUI()
    {
        if (levelController == null)
            levelController = FindObjectOfType<LevelController>();

        if (levelController == null)
        {
            EditorGUILayout.HelpBox("LevelController not found in Scene.", MessageType.Warning);
            return;
        }

        // Row 1: Level input + Load
        GUIStyle centeredField = new GUIStyle(EditorStyles.numberField)
        {
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Level:", GUILayout.Width(38));
        inputLevel = EditorGUILayout.IntField(inputLevel, centeredField, GUILayout.Width(40));
        if (inputLevel < 1) inputLevel = 1;
        if (GUILayout.Button("LOAD"))
        {
            levelController.CurrentLevel = inputLevel;
            levelController.EditorLoadLevel(inputLevel);
            MarkDirty();
        }
        EditorGUILayout.EndHorizontal();

        // Row 2: Prev / Next / Clone
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("◀◀ Prev"))
        {
            levelController.CurrentLevel = inputLevel;
            levelController.EditorLoadPrevLevel();
            inputLevel = levelController.CurrentLevel;
            MarkDirty();
        }
        if (GUILayout.Button("Next ▶▶"))
        {
            levelController.CurrentLevel = inputLevel;
            levelController.EditorLoadNextLevel();
            inputLevel = levelController.CurrentLevel;
            MarkDirty();
        }
        if (GUILayout.Button("CLONE"))
        {
            levelController.EditorCloneLevel(levelController.CurrentLevel);
            inputLevel = levelController.CurrentLevel;
            MarkDirty();
        }
        EditorGUILayout.EndHorizontal();

        // Row 3: Save + Add New
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
        if (GUILayout.Button("💾 SAVE LEVEL", GUILayout.Height(28)))
        {
            levelController.EditorSaveLevel();
            MarkDirty();
        }
        GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
        if (GUILayout.Button("+ ADD NEW", GUILayout.Height(28)))
        {
            levelController.EditorAddLevel();
            inputLevel = levelController.CurrentLevel;
            MarkDirty();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    private void MarkDirty()
    {
        if (levelController == null) return;
        EditorUtility.SetDirty(levelController);
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(levelController.gameObject.scene);
        Repaint();
    }
}
