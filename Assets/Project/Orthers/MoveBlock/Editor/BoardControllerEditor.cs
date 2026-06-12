#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardController))]
public class BoardControllerEditor : Editor
{
    private BoardController board;
    private SerializedProperty enableSnapProp;

    private void OnEnable()
    {
        board = (BoardController)target;
        enableSnapProp = serializedObject.FindProperty("enableSnapInEditor");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (board == null || !board.EnableSnapInEditor) return;

        foreach (MoveBlock t in board.MoveBlocks)
        {
            if (t == null) continue;

            // Snap only if it's selected and moved
            if (Selection.transforms.Contains(t.transform) && GUIUtility.hotControl == 0)
            {
                Vector3 oldPos = t.transform.position;
                Vector3 snappedPos = board.GetSnappedPosition(oldPos);

                if (Vector3.Distance(oldPos, snappedPos) > 0.01f)
                {
                    Undo.RecordObject(t.transform, "Snap MoveBlock to Grid");
                    t.transform.position = snappedPos;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        GUILayout.Space(10);

        EditorGUILayout.PropertyField(enableSnapProp, new GUIContent("Enable Snap in Editor"));

        if (GUILayout.Button("Refresh MoveBlocks Cache"))
        {
            board.RefreshMoveBlocks();
        }

        if (GUILayout.Button("Snap All MoveBlocks"))
        {
            foreach (MoveBlock t in board.MoveBlocks)
            {
                if (t == null) continue;
                Undo.RecordObject(t.transform, "Snap MoveBlock to Grid");
                t.transform.position = board.GetSnappedPosition(t.transform.position);
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Grid Visual and Walls"))
        {
            board.GenerateGridVisual(); // Tạo grid và tường
        }

        if (GUILayout.Button("Clear Grid Visual and Walls"))
        {
            board.ClearGeneratedVisuals(); // Xóa tất cả các tile visual và tường
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(board);
    }
}
#endif
