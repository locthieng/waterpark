using UnityEngine;

public class SpotEdit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Spacing along the Local X Axis (right)")]
    public float spacing = 1.0f;

    [Tooltip("Offset along the Local Z Axis (forward) applied incrementally (index * zOffset)")]
    public float _xOffset = 0.0f;
    public float _zOffset = 0.0f;

    [Tooltip("Automatically align child objects when the game starts at runtime")]
    public bool alignOnStart = false;

    private void Start()
    {
        if (alignOnStart)
        {
            DistributeChildren();
        }
    }

    [ContextMenu("Distribute Children")]
    public void DistributeChildren()
    {
        int childCount = transform.childCount;
        if (childCount <= 1)
        {
            Debug.LogWarning("SpotEdit: Need at least 2 child objects to distribute.");
            return;
        }

        // Gather all children
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObjects(children, "Distribute Children");
#endif

        System.Array.Sort(children, (a, b) =>
        {
            return a.localPosition.x.CompareTo(b.localPosition.x);
        });

        float startX = _xOffset;

        for (int i = 0; i < childCount; i++)
        {
            float targetX = startX + (i * spacing) + _xOffset;

            Vector3 currentPos = children[i].localPosition;

            children[i].localPosition = new Vector3(targetX, currentPos.y, _zOffset);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}