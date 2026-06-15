using Codice.CM.Client.Differences;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LevelController))]
public class LevelEditor : Editor
{
    private LevelController levelController;
    //private SerializedProperty enableSnapProp;  
    private bool showGrid;
    private bool showDifficultLevel;
    private void OnSceneGUI(SceneView sceneView)
    {
        if (Application.isPlaying) return;
    }

    private void OnEnable()
    {
        levelController = (LevelController)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        levelController = (LevelController)target;
        //enableSnapProp = serializedObject.FindProperty("enableSnapInEditor");
        // SceneView.duringSceneGui += OnSceneGUI;

        GUIStyle headerStyle = new GUIStyle();
        headerStyle.richText = true;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.cyan;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("LEVEL", headerStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        levelController.CurrentLevel = EditorGUILayout.IntField(levelController.CurrentLevel, GUILayout.Width(50), GUILayout.Height(25));
        if (GUILayout.Button("LOAD", GUILayout.Width(60), GUILayout.Height(25)))
        {
            levelController.EditorLoadLevel(levelController.CurrentLevel);
        }

        if (GUILayout.Button("<<", GUILayout.Width(40), GUILayout.Height(25)))
        {
            levelController.EditorLoadPrevLevel();
        }

        if (GUILayout.Button(">>", GUILayout.Width(40), GUILayout.Height(25)))
        {
            levelController.EditorLoadNextLevel();
        }

        if (GUILayout.Button("CLONE", GUILayout.MaxWidth(80), GUILayout.Height(25)))
            levelController.EditorCloneLevel(levelController.CurrentLevel);


        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE LEVEL", GUILayout.Width(150), GUILayout.Height(40)))
        {
            levelController.EditorSaveLevel();
        }

        if (GUILayout.Button("GENERATE SPLINE MESHES", GUILayout.Width(180), GUILayout.Height(40)))
        {
            levelController.EditorGenerateAllSplineMeshes();
        }
        EditorGUILayout.EndHorizontal();

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