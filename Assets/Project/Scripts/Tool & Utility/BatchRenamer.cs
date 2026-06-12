#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BatchRenamer : EditorWindow
{
    [MenuItem("AMZG/Batch Rename Levels")]
    private static void Init()
    {
        EditorWindow window = GetWindow(typeof(BatchRenamer));
        window.titleContent.text = "Batch Change Level";
        //window.position = new Rect(Screen.width / 2f, Screen.height / 5f, 250, 150);
        window.ShowPopup();
    }

    static int change;

    void OnGUI()
    {
        change = EditorGUILayout.IntField("Change value:", change);
        if (GUILayout.Button("Change Level Names"))
        {
            BatchIncreaseLevels(change);
        }

        if (GUILayout.Button("Cancel"))
            Close();
    }

    private static void BatchIncreaseLevels(int changeValue)
    {
        if (Application.isPlaying) return;
        if (Selection.objects.Length == 0)
        {
            Debug.LogError("No level objects selected.");
            return;
        }
        int level = 0;
        foreach (Object o in Selection.objects)
        {

            if (o.GetType() != typeof(GameObject))
            {
                Debug.LogWarning("Skipping " + o + " since it isn't a GameObject instance.");
                continue;
            }

            GameObject levelObject = o as GameObject;
            string[] levelNameParts = levelObject.name.Split('_');
            if (levelNameParts.Length > 1)
            {
                AssetDatabase.Refresh();
                int.TryParse(levelNameParts[1], out level);
                string newAssetName = levelNameParts[0] + "_" + level + "_.prefab";
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelObject), newAssetName);
                AssetDatabase.SaveAssets();
            }
            else
            {
                continue;
            }

        }
        foreach (Object o in Selection.objects)
        {

            if (o.GetType() != typeof(GameObject))
            {
                Debug.LogWarning("Skipping " + o + " since it isn't a GameObject instance.");
                continue;
            }

            GameObject levelObject = o as GameObject;
            string[] levelNameParts = levelObject.name.Split('_');
            if (levelNameParts.Length > 1)
            {
                AssetDatabase.Refresh();
                int.TryParse(levelNameParts[1], out level);
                string newAssetName = levelNameParts[0] + "_" + (level + changeValue) + ".prefab";
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelObject), newAssetName);
                AssetDatabase.SaveAssets();
            }
            else
            {
                continue;
            }

        }
    }
}
#endif