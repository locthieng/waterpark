using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using BezierSolution;

public enum BrushType
{
    Select,
    PaintBlock,
    PaintObstacle,
    PaintHole,
    Linker
}

public class LevelEditorWindow : EditorWindow
{
    private LevelController levelController;
    private int inputLevel = 1;
    private LevelData _activeLevelData;
    private SerializedObject _serializedLevelData;

    // Quản lý Bezier Spline
    private SplineMeshBuilder _activeSplineBuilder;

    private BrushType _currentBrush = BrushType.Select;
    private int _selectedColorId = 0;

    // Quản lý Tabs danh mục bên trái
    private enum TabType { Block, Map, Pipe, Hole, Spline }
    private TabType _currentTab = TabType.Block;

    // Scroll positions for IMGUI
    private Vector2 leftScrollPosition = Vector2.zero;
    private Vector2 rightScrollPosition = Vector2.zero;
    private Vector2 splinePointsScrollPos = Vector2.zero;

    // Map/Grid temporary settings
    private int mapWidth = 7;
    private int mapHeight = 9;
    private float cellSize = 1f;

    // Pipe/Hole settings
    private int pipeStrength = 1;
    private int holeCapacity = 3;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(650, 400);
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        SceneView.duringSceneGui += OnSceneGUI;

        levelController = Object.FindFirstObjectByType<LevelController>();
        if (levelController != null)
            inputLevel = levelController.CurrentLevel < 1 ? 1 : levelController.CurrentLevel;

        UpdateUIState();
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnHierarchyChanged()
    {
        if (levelController == null)
            levelController = Object.FindFirstObjectByType<LevelController>();
        
        UpdateUIState();
    }

    private void OnInspectorUpdate()
    {
        // Force repaint to make UI responsive
        Repaint();
    }

    private void UpdateUIState()
    {
        if (levelController != null)
        {
            RefreshActiveLevelData();
            
            // Tự động tìm Spline Builder trong Scene nếu chưa được gán
            if (_activeSplineBuilder == null)
            {
                AutoFindSplineBuilder();
            }
        }
        else
        {
            _activeLevelData = null;
            _serializedLevelData = null;
        }
    }

    private void OpenGameScene()
    {
        string scenePath = "Assets/Project/Scenes/Game.unity";
        if (File.Exists(scenePath))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy scene Game tại: " + scenePath);
        }
    }

    private void OnGUI()
    {
        // Custom style definitions
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft
        };
        titleStyle.normal.textColor = new Color(0.35f, 0.65f, 0.9f);

        GUIStyle sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            margin = new RectOffset(0, 0, 10, 5)
        };
        sectionTitleStyle.normal.textColor = new Color(0.35f, 0.65f, 0.9f);

        if (levelController == null)
        {
            // WARNING CONTAINER
            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(30, 30, 30, 30) });
            GUILayout.FlexibleSpace();
            
            var warningStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter
            };
            warningStyle.normal.textColor = new Color(1f, 0.7f, 0.3f);
            
            GUILayout.Label("⚠️ LevelController not found in the current Scene.", warningStyle);
            GUILayout.Space(15);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Game Scene", GUILayout.Height(34), GUILayout.Width(200)))
            {
                OpenGameScene();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            return;
        }

        // TOP TOOLBAR
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(30));
        
        GUILayout.Label("Level:", EditorStyles.miniLabel, GUILayout.Width(40));
        inputLevel = EditorGUILayout.IntField(inputLevel, GUILayout.Width(45));
        if (inputLevel < 1) inputLevel = 1;

        if (GUILayout.Button("LOAD", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            levelController.CurrentLevel = inputLevel;
            levelController.EditorLoadLevel(inputLevel);
            UpdateUIState();
            MarkDirty();
        }

        if (GUILayout.Button("◀◀ Prev", EditorStyles.toolbarButton, GUILayout.Width(65)))
        {
            levelController.EditorLoadPrevLevel();
            inputLevel = levelController.CurrentLevel;
            UpdateUIState();
            MarkDirty();
        }

        if (GUILayout.Button("Next ▶▶", EditorStyles.toolbarButton, GUILayout.Width(65)))
        {
            levelController.EditorLoadNextLevel();
            inputLevel = levelController.CurrentLevel;
            UpdateUIState();
            MarkDirty();
        }

        if (GUILayout.Button("CLONE", EditorStyles.toolbarButton, GUILayout.Width(55)))
        {
            levelController.EditorCloneLevel(levelController.CurrentLevel);
            inputLevel = levelController.CurrentLevel;
            UpdateUIState();
            MarkDirty();
        }

        GUILayout.Space(10);

        Color originalBg = GUI.backgroundColor;
        
        // Save Button (Green)
        GUI.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
        if (GUILayout.Button("💾 SAVE LEVEL", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            if (_activeLevelData != null)
            {
                _activeLevelData.levelIndex = levelController.CurrentLevel;
                SaveSplinePointsToLevelData();
                EditorUtility.SetDirty(_activeLevelData);
            }
            levelController.EditorSaveLevel();
            UpdateUIState();
            MarkDirty();
        }

        // Add Button (Blue)
        GUI.backgroundColor = new Color(0.1f, 0.4f, 0.8f);
        if (GUILayout.Button("+ ADD NEW", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            levelController.EditorAddLevel();
            inputLevel = levelController.CurrentLevel;
            UpdateUIState();
            MarkDirty();
        }
        
        GUI.backgroundColor = originalBg;
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // MAIN LAYOUT (Two-column layout using EditorGUILayout)
        EditorGUILayout.BeginHorizontal();

        // PANE 1: LEFT SIDEBAR (Width 325)
        EditorGUILayout.BeginVertical("box", GUILayout.Width(325), GUILayout.ExpandHeight(true));
        leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);

        // BRUSH SELECTOR
        GUILayout.Label("CỌ VẼ (BRUSHES)", sectionTitleStyle);
        
        string[] brushNames = { "Select", "Paint Block", "Paint Obstacle", "Paint Hole" };
        BrushType[] brushTypes = { BrushType.Select, BrushType.PaintBlock, BrushType.PaintObstacle, BrushType.PaintHole };
        int selectedBrushIdx = System.Array.IndexOf(brushTypes, _currentBrush);
        if (selectedBrushIdx < 0) selectedBrushIdx = 0;
        
        int newBrushIdx = GUILayout.Toolbar(selectedBrushIdx, brushNames);
        if (newBrushIdx != selectedBrushIdx)
        {
            _currentBrush = brushTypes[newBrushIdx];
        }

        GUILayout.Space(10);
        
        // CATEGORY TABS
        string[] tabNames = { "Block", "Map", "Pipe", "Hole", "Spline" };
        _currentTab = (TabType)GUILayout.Toolbar((int)_currentTab, tabNames);
        
        GUILayout.Space(10);
        GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Horizontal line separator
        GUILayout.Space(5);

        // TAB PANELS
        switch (_currentTab)
        {
            case TabType.Block:
                GUILayout.Label("BẢNG MÀU BLOCK", sectionTitleStyle);
                DrawColorPalette();
                break;

            case TabType.Map:
                GUILayout.Label("BÀN CỜ (MAP)", sectionTitleStyle);
                mapWidth = EditorGUILayout.IntField("Cột (Width)", mapWidth);
                mapHeight = EditorGUILayout.IntField("Dòng (Height)", mapHeight);
                cellSize = EditorGUILayout.FloatField("Kích thước ô", cellSize);
                GUILayout.Space(10);
                if (GUILayout.Button("⚡ Khởi Tạo Lưới Trống", GUILayout.Height(30)))
                {
                    Debug.Log($"Khởi tạo lưới trống {mapWidth}x{mapHeight} với kích thước {cellSize} (Tính năng đang phát triển)");
                }
                break;

            case TabType.Pipe:
                GUILayout.Label("ỐNG THẢ (PIPES)", sectionTitleStyle);
                if (GUILayout.Button("Paint Pipe Brush", _currentBrush == BrushType.Linker ? GetActiveButtonStyle() : GUI.skin.button))
                {
                    _currentBrush = BrushType.Linker;
                }
                GUILayout.Space(5);
                pipeStrength = EditorGUILayout.IntField("Độ Bền (Pipe Strength)", pipeStrength);
                break;

            case TabType.Hole:
                GUILayout.Label("HỐ THU THẬP (HOLES)", sectionTitleStyle);
                if (GUILayout.Button("Paint Hole Brush", _currentBrush == BrushType.PaintHole ? GetActiveButtonStyle() : GUI.skin.button))
                {
                    _currentBrush = BrushType.PaintHole;
                }
                GUILayout.Space(5);
                holeCapacity = EditorGUILayout.IntField("Sức Chứa Hố", holeCapacity);
                break;

            case TabType.Spline:
                GUILayout.Label("ĐƯỜNG ĐI (BEZIER SPLINE)", sectionTitleStyle);
                
                if (GUILayout.Button("🔍 Auto Find Spline", GUILayout.Height(26)))
                {
                    AutoFindSplineBuilder();
                }
                GUILayout.Space(5);

                _activeSplineBuilder = (SplineMeshBuilder)EditorGUILayout.ObjectField("Spline Builder", _activeSplineBuilder, typeof(SplineMeshBuilder), true);
                
                if (_activeSplineBuilder != null)
                {
                    GUILayout.Space(8);
                    GUILayout.Label("Cấu Hình Điểm Spline:", EditorStyles.boldLabel);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("➕ Add Point", GUILayout.Height(24)))
                    {
                        if (_activeSplineBuilder.spline != null)
                        {
                            var spline = _activeSplineBuilder.spline;
                            Undo.RegisterCompleteObjectUndo(spline, "Add Spline Point");
                            
                            int count = spline.Count;
                            if (count > 0)
                            {
                                spline.DuplicatePointAt(count - 1);
                            }
                            else
                            {
                                spline.InsertNewPointAt(0);
                            }
                            
                            _activeSplineBuilder.GenerateMesh();
                            SaveSplinePointsToLevelData();
                            Debug.Log("Đã thêm điểm spline mới (Duplicate điểm cuối).");
                        }
                    }
                    if (GUILayout.Button("➖ Remove Last", GUILayout.Height(24)))
                    {
                        if (_activeSplineBuilder.spline != null)
                        {
                            var spline = _activeSplineBuilder.spline;
                            if (spline.Count > 2)
                            {
                                Undo.RegisterCompleteObjectUndo(spline, "Remove Spline Point");
                                
                                var lastPoint = spline[spline.Count - 1];
                                if (lastPoint != null)
                                {
                                    Undo.DestroyObjectImmediate(lastPoint.gameObject);
                                }
                                
                                _activeSplineBuilder.GenerateMesh();
                                SaveSplinePointsToLevelData();
                                Debug.Log("Đã xóa điểm cuối cùng của spline.");
                            }
                            else
                            {
                                Debug.LogWarning("Không thể xóa: Spline phải chứa ít nhất 2 điểm!");
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // Loop Toggle
                    if (_activeSplineBuilder.spline != null)
                    {
                        var spline = _activeSplineBuilder.spline;
                        bool prevLoop = spline.loop;
                        bool newLoop = EditorGUILayout.Toggle("Đóng Vòng (Loop)", prevLoop);
                        if (newLoop != prevLoop)
                        {
                            Undo.RecordObject(spline, "Toggle Spline Loop");
                            spline.loop = newLoop;
                            _activeSplineBuilder.GenerateMesh();
                            
                            if (_activeLevelData != null)
                            {
                                Undo.RecordObject(_activeLevelData, "Update Spline Loop in LevelData");
                                _activeLevelData._isBezierLoop = newLoop;
                                EditorUtility.SetDirty(_activeLevelData);
                            }
                        }
                    }

                    // Points list
                    GUILayout.Space(8);
                    GUILayout.Label("Danh Sách Các Điểm (Points):", EditorStyles.boldLabel);
                    DrawSplinePointsList();

                    // Mesh settings
                    GUILayout.Space(8);
                    GUILayout.Label("Cấu Hình Mesh:", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    float newWidth = EditorGUILayout.FloatField("Độ Rộng Mesh", _activeSplineBuilder.width);
                    int newRes = EditorGUILayout.IntField("Độ Phân Giải (Res)", _activeSplineBuilder.resolution);
                    float newTiling = EditorGUILayout.FloatField("UV Tiling", _activeSplineBuilder.uvTiling);
                    bool newNormal = EditorGUILayout.Toggle("Use Spline Normal", _activeSplineBuilder.useSplineNormal);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_activeSplineBuilder, "Modify Spline Mesh Properties");
                        _activeSplineBuilder.width = newWidth;
                        _activeSplineBuilder.resolution = newRes;
                        _activeSplineBuilder.uvTiling = newTiling;
                        _activeSplineBuilder.useSplineNormal = newNormal;
                        _activeSplineBuilder.GenerateMesh();
                        EditorUtility.SetDirty(_activeSplineBuilder);
                    }

                    GUILayout.Space(12);
                    
                    GUI.backgroundColor = new Color(0.1f, 0.4f, 0.8f);
                    if (GUILayout.Button("🔨 Generate & Save Mesh", GUILayout.Height(32)))
                    {
                        GenerateAndSaveMesh();
                    }
                    GUI.backgroundColor = originalBg;
                }
                break;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Divider
        GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));

        // PANE 2: RIGHT SIDEBAR (Expands to fill remaining window)
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition);

        GUILayout.Label("THÔNG TIN LEVEL", sectionTitleStyle);
        
        string activeLevelLabel = (_activeLevelData != null) 
            ? $"Active Level: Level_{levelController.CurrentLevel}" 
            : $"Active Level: Level_{levelController.CurrentLevel} (Chưa tạo asset)";
        
        GUILayout.Label(activeLevelLabel, EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (_activeLevelData != null && _serializedLevelData != null)
        {
            _serializedLevelData.Update();
            
            SerializedProperty prop = _serializedLevelData.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name == "m_Script") continue;
                    EditorGUILayout.PropertyField(prop, true);
                }
                while (prop.NextVisible(false));
            }

            if (_serializedLevelData.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_activeLevelData);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Không tìm thấy dữ liệu LevelData. Hãy nhấp LOAD hoặc SAVE LEVEL để khởi tạo.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private GUIStyle GetActiveButtonStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.normal.background = style.active.background;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    private void DrawColorPalette()
    {
        GameDataBase db = Resources.Load<GameDataBase>("Database/GameDataBase");
        if (db == null || db.Colors == null || db.Colors.Count == 0)
        {
            GUILayout.Label("Không tìm thấy GameDataBase hoặc mảng màu trống.", EditorStyles.miniLabel);
            return;
        }

        int cols = 6;
        int rows = Mathf.CeilToInt((float)db.Colors.Count / cols);
        
        EditorGUILayout.BeginVertical();
        for (int r = 0; r < rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < cols; c++)
            {
                int index = r * cols + c;
                if (index < db.Colors.Count)
                {
                    Color color = db.Colors[index];
                    
                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = color;
                    
                    string btnTxt = (index == _selectedColorId) ? "●" : "";
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                    btnStyle.normal.textColor = Color.white;
                    btnStyle.fontStyle = FontStyle.Bold;

                    if (GUILayout.Button(btnTxt, btnStyle, GUILayout.Width(35), GUILayout.Height(35)))
                    {
                        _selectedColorId = index;
                    }
                    
                    GUI.backgroundColor = oldBg;
                }
                else
                {
                    GUILayout.Space(35);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawSplinePointsList()
    {
        if (_activeSplineBuilder == null || _activeSplineBuilder.spline == null)
        {
            GUILayout.Label("Không tìm thấy Spline.", EditorStyles.miniLabel);
            return;
        }

        var spline = _activeSplineBuilder.spline;
        
        splinePointsScrollPos = EditorGUILayout.BeginScrollView(splinePointsScrollPos, "box", GUILayout.Height(180));
        for (int i = 0; i < spline.Count; i++)
        {
            int index = i;
            var pt = spline[index];
            if (pt == null) continue;

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label($"P{index}: ({pt.localPosition.x:F2}, {pt.localPosition.y:F2}, {pt.localPosition.z:F2})", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("🔍", GUILayout.Width(24), GUILayout.Height(18)))
            {
                Selection.activeGameObject = pt.gameObject;
                SceneView.FrameLastActiveSceneView();
            }

            if (GUILayout.Button("❌", GUILayout.Width(24), GUILayout.Height(18)))
            {
                if (spline.Count > 2)
                {
                    Undo.RegisterCompleteObjectUndo(spline, "Remove Spline Point");
                    Undo.DestroyObjectImmediate(pt.gameObject);
                    
                    EditorApplication.delayCall += () =>
                    {
                        if (_activeSplineBuilder != null)
                        {
                            _activeSplineBuilder.GenerateMesh();
                            SaveSplinePointsToLevelData();
                        }
                    };
                }
                else
                {
                    Debug.LogWarning("Không thể xóa: Spline phải chứa ít nhất 2 điểm!");
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void GenerateAndSaveMesh()
    {
        if (_activeSplineBuilder == null) return;

        Undo.RecordObject(_activeSplineBuilder, "Generate Spline Mesh");
        _activeSplineBuilder.GenerateMesh();

        MeshFilter filter = _activeSplineBuilder.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            if (_activeLevelData != null)
            {
                Undo.RecordObject(_activeLevelData, "Save Spline Mesh to LevelData");

                string folderPath = "Assets/Project/Resources/MeshBezier";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.Refresh();
                }

                string targetMeshName = $"Level_{_activeLevelData.levelIndex}_Mesh";
                string targetPath = $"{folderPath}/{targetMeshName}.asset";
                Mesh targetMesh = null;

                // 1. Tìm mesh theo LevelData đang trỏ tới (nếu nằm trong MeshBezier)
                if (_activeLevelData._meshBezierSpline != null)
                {
                    string existingPath = AssetDatabase.GetAssetPath(_activeLevelData._meshBezierSpline);
                    if (!string.IsNullOrEmpty(existingPath) && existingPath.StartsWith(folderPath))
                    {
                        targetMesh = _activeLevelData._meshBezierSpline;
                        targetPath = existingPath;
                    }
                }

                // 2. Nếu chưa tìm thấy, thử load từ targetPath (Level_LevelIndex_Mesh.asset)
                if (targetMesh == null)
                {
                    targetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(targetPath);
                }

                // 3. Nếu vẫn chưa có thì tạo mới, nếu có rồi thì ghi đè dữ liệu
                if (targetMesh == null)
                {
                    targetMesh = new Mesh();
                    targetMesh.name = targetMeshName;
                    CopyMeshProperties(filter.sharedMesh, targetMesh);
                    AssetDatabase.CreateAsset(targetMesh, targetPath);
                    Debug.Log($"Đã tạo mesh asset mới tại: {targetPath}");
                }
                else
                {
                    Undo.RecordObject(targetMesh, "Update Spline Mesh");
                    CopyMeshProperties(filter.sharedMesh, targetMesh);
                    EditorUtility.SetDirty(targetMesh);
                    Debug.Log($"Đã cập nhật mesh asset hiện tại tại: {targetPath}");
                }

                // Gán mesh asset đã lưu/cập nhật vào LevelData và MeshFilter
                _activeLevelData._meshBezierSpline = targetMesh;
                filter.sharedMesh = targetMesh;

                // Lưu đồng thời thông số spline points và loop
                SaveSplinePointsToLevelData();

                EditorUtility.SetDirty(_activeLevelData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Đã tạo/ghi đè và gán mesh spline thành công vào {_activeLevelData.name}!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy file LevelData để gán mesh!");
            }
        }
    }

    private void RefreshActiveLevelData()
    {
        if (levelController == null)
        {
            _activeLevelData = null;
            _serializedLevelData = null;
            return;
        }

        string path = $"Assets/Project/Resources/StageLevel/Level_{levelController.CurrentLevel}.asset";
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (data != _activeLevelData)
        {
            _activeLevelData = data;
            if (_activeLevelData != null)
            {
                _serializedLevelData = new SerializedObject(_activeLevelData);
            }
            else
            {
                _serializedLevelData = null;
            }
        }
    }

    private void AutoFindSplineBuilder()
    {
        _activeSplineBuilder = Object.FindFirstObjectByType<SplineMeshBuilder>();
    }

    private void SaveSplinePointsToLevelData()
    {
        if (_activeLevelData == null || _activeSplineBuilder == null || _activeSplineBuilder.spline == null) return;

        Undo.RecordObject(_activeLevelData, "Save Spline Points to LevelData");
        _activeLevelData._isBezierLoop = _activeSplineBuilder.spline.loop;
        _activeLevelData._bezierPoints.Clear();

        var spline = _activeSplineBuilder.spline;
        for (int i = 0; i < spline.Count; i++)
        {
            var pt = spline[i];
            if (pt == null) continue;

            var ptData = new BezierPointData
            {
                localPosition = pt.localPosition,
                precedingControlPointLocalPosition = pt.precedingControlPointLocalPosition,
                followingControlPointLocalPosition = pt.followingControlPointLocalPosition,
                handleMode = pt.handleMode
            };
            _activeLevelData._bezierPoints.Add(ptData);
        }
        EditorUtility.SetDirty(_activeLevelData);
    }

    // ==========================================
    // LOGIC VẼ TRÊN SCENE VIEW (INTERCEPTION)
    // ==========================================

    private void OnSceneGUI(SceneView sceneView)
    {
        if (levelController == null) return;
        
        // Cọ Select thì để Unity xử lý click chuột mặc định
        if (_currentBrush == BrushType.Select) return;

        // Vô hiệu hóa việc Unity tự click chọn GameObject khác khi ta đang dùng cọ vẽ
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event currentEvent = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        Plane gridPlane = new Plane(Vector3.up, Vector3.zero);

        if (gridPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 hitPoint = ray.GetPoint(enterDistance);
            Vector3Int gridCoords = SnapToGrid(hitPoint);

            // 1. Hiển thị khung xem trước (Preview Indicator) tại ô đang chỉ vào
            DrawBrushPreview(gridCoords);

            // 2. Nhấn giữ chuột trái (MouseDown hoặc MouseDrag) để thực hiện hành động cọ vẽ
            if ((currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) && currentEvent.button == 0)
            {
                ApplyBrushAction(gridCoords);
                currentEvent.Use(); // Nuốt sự kiện chuột
            }
            
            // Nhấn chuột phải để xóa BlockCell
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                EraseBlockCell(gridCoords);
                currentEvent.Use();
            }
        }
        
        // Ép SceneView vẽ lại liên tục để preview di chuyển mượt mà
        sceneView.Repaint();
    }

    private Vector3Int SnapToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x),
            0,
            Mathf.RoundToInt(worldPos.z)
        );
    }

    private void DrawBrushPreview(Vector3Int gridCoords)
    {
        Vector3 center = new Vector3(gridCoords.x, 0f, gridCoords.z);
        
        // Vẽ khung ô lưới viền
        Handles.color = GetBrushColor();
        Handles.DrawWireCube(center + new Vector3(0f, 0.1f, 0f), new Vector3(1f, 0.2f, 1f));
        
        // Nếu là Paint Block, vẽ thêm khối vuông mờ theo màu đã chọn
        if (_currentBrush == BrushType.PaintBlock)
        {
            Color col = GetSelectedColor();
            col.a = 0.2f; // độ trong suốt của preview box
            Handles.color = col;
            
            // Sử dụng CubeHandleCap để vẽ khối solid cube chuẩn Unity
            Handles.CubeHandleCap(0, center + new Vector3(0f, 0.5f, 0f), Quaternion.identity, 1f, EventType.Repaint);
        }
    }

    private Color GetBrushColor()
    {
        switch (_currentBrush)
        {
            case BrushType.PaintBlock: return Color.green;
            case BrushType.PaintObstacle: return Color.yellow;
            case BrushType.PaintHole: return Color.red;
            default: return Color.white;
        }
    }

    private Color GetSelectedColor()
    {
        GameDataBase db = Resources.Load<GameDataBase>("Database/GameDataBase");
        if (db != null && db.Colors != null && _selectedColorId >= 0 && _selectedColorId < db.Colors.Count)
        {
            return db.Colors[_selectedColorId];
        }
        return Color.green;
    }

    private void ApplyBrushAction(Vector3Int gridCoords)
    {
        if (_currentBrush == BrushType.PaintBlock)
        {
            BlockCell existingCell = FindCellAt(gridCoords);
            if (existingCell == null)
            {
                // Tải prefab BlockCell
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project/Prefabs/BlockCellPrefab.prefab");
                if (prefab != null)
                {
                    GameObject newCellGO = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    newCellGO.transform.position = new Vector3(gridCoords.x, 0f, gridCoords.z);
                    newCellGO.name = $"BlockCell_{gridCoords.x}_{gridCoords.z}";
                    
                    Undo.RegisterCreatedObjectUndo(newCellGO, "Paint BlockCell");

                    BlockCell cellComponent = newCellGO.GetComponent<BlockCell>();
                    if (cellComponent != null)
                    {
                        var bcc = Object.FindFirstObjectByType<BlockCellController>();
                        if (bcc != null)
                        {
                            Undo.RecordObject(bcc, "Add BlockCell to Controller");
                            bcc.AllBlockCells.Add(cellComponent);
                        }
                        
                        cellComponent.CellType = BlockCellType.BlockSimple;
                        EditorUtility.SetDirty(newCellGO);
                    }
                }
            }
        }
    }

    private void EraseBlockCell(Vector3Int gridCoords)
    {
        BlockCell existingCell = FindCellAt(gridCoords);
        if (existingCell != null)
        {
            var bcc = Object.FindFirstObjectByType<BlockCellController>();
            if (bcc != null && bcc.AllBlockCells.Contains(existingCell))
            {
                Undo.RecordObject(bcc, "Remove BlockCell from Controller");
                bcc.AllBlockCells.Remove(existingCell);
            }

            Undo.DestroyObjectImmediate(existingCell.gameObject);
            Debug.Log($"Đã xóa BlockCell tại {gridCoords}");
        }
    }

    private BlockCell FindCellAt(Vector3Int coords)
    {
        var cells = Object.FindObjectsByType<BlockCell>(FindObjectsSortMode.None);
        foreach (var cell in cells)
        {
            if (cell != null)
            {
                Vector3Int cellCoords = SnapToGrid(cell.transform.position);
                if (cellCoords.x == coords.x && cellCoords.z == coords.z)
                {
                    return cell;
                }
            }
        }
        return null;
    }

    private void CopyMeshProperties(Mesh source, Mesh destination)
    {
        if (source == null || destination == null) return;
        if (source == destination) return; // Prevent clearing itself when saving without changes

        destination.Clear();
        destination.vertices = source.vertices;
        destination.triangles = source.triangles;
        destination.uv = source.uv;
        destination.normals = source.normals;
        destination.tangents = source.tangents;
        destination.colors = source.colors;
        destination.bounds = source.bounds;
    }

    private void MarkDirty()
    {
        if (levelController == null) return;
        EditorUtility.SetDirty(levelController);
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(levelController.gameObject.scene);
    }
}
