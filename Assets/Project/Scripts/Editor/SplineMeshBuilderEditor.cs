using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineMeshBuilder))]
public class SplineMeshBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplineMeshBuilder builder = (SplineMeshBuilder)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Mesh", GUILayout.Height(30)))
        {
            builder.GenerateMesh();
            
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
            }
        }
    }
}
