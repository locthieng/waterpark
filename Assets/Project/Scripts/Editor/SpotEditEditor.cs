using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpotEdit))]
public class SpotEditEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpotEdit spotEdit = (SpotEdit)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Distribute Children", GUILayout.Height(30)))
        {
            spotEdit.DistributeChildren();
        }
    }
}
