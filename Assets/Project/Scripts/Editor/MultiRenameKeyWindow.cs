using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MultiRenameKeyWindow : EditorWindow
{
    [System.Serializable]
    public class RenameRule
    {
        public KeyCode key = KeyCode.None;
        public string name = "";
        public bool waitingKey = false;
    }

    private List<RenameRule> rules = new List<RenameRule>();

    [MenuItem("Tools/Multi Rename By Key")]
    public static void Open()
    {
        GetWindow<MultiRenameKeyWindow>("Multi Rename");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;   // fallback
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        Event e = Event.current;

        GUILayout.Label("Rename Rules", EditorStyles.boldLabel);

        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Key:", GUILayout.Width(40));

            if (!rule.waitingKey)
            {
                GUILayout.Label(rule.key.ToString(), GUILayout.Width(80));

                if (GUILayout.Button("Set", GUILayout.Width(50)))
                {
                    rule.waitingKey = true;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.Label("Press a key...", GUILayout.Width(120));

                if (e.type == EventType.KeyDown)
                {
                    rule.key = e.keyCode;
                    rule.waitingKey = false;
                    e.Use();
                    Repaint();
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                rules.RemoveAt(i);
                return;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(40));
            rule.name = GUILayout.TextField(rule.name);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Rule"))
            rules.Add(new RenameRule());

        Repaint();

        // B?t key ngay trong chính EditorWindow
        CatchKey(Event.current);
    }

    private void OnSceneGUI(SceneView view)
    {
        if (Event.current.type == EventType.KeyDown)
        {
            CatchKey(Event.current);
        }
    }

    private void CatchKey(Event e)
    {
        if (e == null || e.type != EventType.KeyDown)
            return;

        foreach (var rule in rules)
        {
            if (rule.key == e.keyCode && !string.IsNullOrEmpty(rule.name))
            {
                ApplyRename(rule.name);
                e.Use();
                break;
            }
        }
    }

    private void ApplyRename(string newName)
    {
        if (Selection.activeObject == null)
            return;

        Undo.RecordObject(Selection.activeObject, "Rename Object");
        Selection.activeObject.name = newName;
    }
}