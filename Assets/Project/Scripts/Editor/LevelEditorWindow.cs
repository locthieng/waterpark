using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
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
    private SerializedObject _serializedSplineBuilder;

    private BrushType _currentBrush = BrushType.Select;
    private int _selectedColorId = 0;

    // Quản lý Tabs danh mục bên trái
    private enum TabType { Block, Map, Pipe, Hole, Spline }
    private TabType _currentTab = TabType.Block;

    private VisualElement _root;
    private VisualElement _warningContainer;
    private VisualElement _mainEditorContainer;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(350, 150);
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        SceneView.duringSceneGui += OnSceneGUI;

        levelController = Object.FindFirstObjectByType<LevelController>();
        if (levelController != null)
            inputLevel = levelController.CurrentLevel < 1 ? 1 : levelController.CurrentLevel;
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
        // Cập nhật danh sách điểm tọa độ spline liên tục nếu đang mở Tab Spline
        if (_currentTab == TabType.Spline && _activeSplineBuilder != null)
        {
            RefreshSplinePointsList();
        }
    }

    public void CreateGUI()
    {
        _root = rootVisualElement;

        // Tải các tài nguyên UXML và USS
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Project/Scripts/Editor/LevelEditorWindow.uxml");
        var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Project/Scripts/Editor/LevelEditorWindow.uss");

        if (uxml != null)
        {
            var clone = uxml.CloneTree();
            clone.style.flexGrow = 1;
            _root.Add(clone);
        }
        if (uss != null) _root.styleSheets.Add(uss);

        _warningContainer = _root.Q<VisualElement>("warning-container");
        _mainEditorContainer = _root.Q<VisualElement>("main-editor-container");

        // Thiết lập nút cảnh báo
        var btnOpenScene = _root.Q<Button>("btn-open-scene");
        if (btnOpenScene != null)
        {
            btnOpenScene.clicked += OpenGameScene;
        }

        // Thiết lập các bộ điều khiển Toolbar
        SetupToolbar();

        // Thiết lập bộ chọn cọ vẽ
        SetupBrushSelector();

        // Thiết lập bảng màu động từ GameDataBase
        SetupColorPalette();

        // Thiết lập bảng điều khiển Bezier Spline
        SetupSplinePanel();

        // Thiết lập thanh Tab chuyển đổi danh mục
        SetupTabs();

        // Đồng bộ trạng thái ban đầu của UI
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        if (levelController == null)
        {
            if (_warningContainer != null) _warningContainer.style.display = DisplayStyle.Flex;
            if (_mainEditorContainer != null) _mainEditorContainer.style.display = DisplayStyle.None;
        }
        else
        {
            if (_warningContainer != null) _warningContainer.style.display = DisplayStyle.None;
            if (_mainEditorContainer != null) _mainEditorContainer.style.display = DisplayStyle.Flex;

            var levelInput = _root.Q<IntegerField>("level-input");
            if (levelInput != null && levelInput.value != levelController.CurrentLevel)
            {
                levelInput.value = levelController.CurrentLevel;
            }

            RefreshActiveLevelData();
            
            // Tự động tìm Spline Builder trong Scene nếu chưa được gán
            if (_activeSplineBuilder == null)
            {
                AutoFindSplineBuilder();
            }
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

    private void SetupToolbar()
    {
        var levelInput = _root.Q<IntegerField>("level-input");
        if (levelInput != null)
        {
            levelInput.value = inputLevel;
            levelInput.RegisterValueChangedCallback(evt =>
            {
                inputLevel = evt.newValue;
                if (inputLevel < 1) inputLevel = 1;
            });
        }

        var btnLoad = _root.Q<Button>("btn-load");
        if (btnLoad != null)
        {
            btnLoad.clicked += () =>
            {
                if (levelController != null)
                {
                    levelController.CurrentLevel = inputLevel;
                    levelController.EditorLoadLevel(inputLevel);
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }

        var btnPrev = _root.Q<Button>("btn-prev");
        if (btnPrev != null)
        {
            btnPrev.clicked += () =>
            {
                if (levelController != null)
                {
                    levelController.CurrentLevel = inputLevel;
                    levelController.EditorLoadPrevLevel();
                    inputLevel = levelController.CurrentLevel;
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }

        var btnNext = _root.Q<Button>("btn-next");
        if (btnNext != null)
        {
            btnNext.clicked += () =>
            {
                if (levelController != null)
                {
                    levelController.CurrentLevel = inputLevel;
                    levelController.EditorLoadNextLevel();
                    inputLevel = levelController.CurrentLevel;
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }

        var btnClone = _root.Q<Button>("btn-clone");
        if (btnClone != null)
        {
            btnClone.clicked += () =>
            {
                if (levelController != null)
                {
                    levelController.EditorCloneLevel(levelController.CurrentLevel);
                    inputLevel = levelController.CurrentLevel;
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }

        var btnSave = _root.Q<Button>("btn-save");
        if (btnSave != null)
        {
            btnSave.clicked += () =>
            {
                if (levelController != null)
                {
                    if (_activeLevelData != null)
                    {
                        _activeLevelData.levelIndex = levelController.CurrentLevel;
                        
                        // Lưu tọa độ spline points vào LevelData
                        SaveSplinePointsToLevelData();

                        EditorUtility.SetDirty(_activeLevelData);
                    }
                    levelController.EditorSaveLevel();
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }

        var btnAdd = _root.Q<Button>("btn-add");
        if (btnAdd != null)
        {
            btnAdd.clicked += () =>
            {
                if (levelController != null)
                {
                    levelController.EditorAddLevel();
                    inputLevel = levelController.CurrentLevel;
                    UpdateUIState();
                    MarkDirty();
                }
            };
        }
    }

    private void SetupBrushSelector()
    {
        var btnSelect = _root.Q<Button>("brush-select");
        var btnPaintBlock = _root.Q<Button>("brush-paint-block");
        var btnPaintObstacle = _root.Q<Button>("brush-paint-obstacle");
        var btnPaintHole = _root.Q<Button>("brush-paint-hole");

        if (btnSelect != null) btnSelect.clicked += () => SelectBrush(btnSelect, BrushType.Select);
        if (btnPaintBlock != null) btnPaintBlock.clicked += () => SelectBrush(btnPaintBlock, BrushType.PaintBlock);
        if (btnPaintObstacle != null) btnPaintObstacle.clicked += () => SelectBrush(btnPaintObstacle, BrushType.PaintObstacle);
        if (btnPaintHole != null) btnPaintHole.clicked += () => SelectBrush(btnPaintHole, BrushType.PaintHole);
    }

    private void SelectBrush(Button selectedButton, BrushType brushType)
    {
        _currentBrush = brushType;

        var brushContainer = _root.Q<VisualElement>("brush-container");
        if (brushContainer != null)
        {
            brushContainer.Query<Button>(className: "btn-brush").ForEach(btn =>
            {
                btn.RemoveFromClassList("btn-brush-active");
            });
        }

        if (selectedButton != null)
        {
            selectedButton.AddToClassList("btn-brush-active");
        }

    }

    private void SetupColorPalette()
    {
        var paletteContainer = _root.Q<VisualElement>("color-palette-container");
        if (paletteContainer == null) return;

        paletteContainer.Clear();

        GameDataBase db = Resources.Load<GameDataBase>("Database/GameDataBase");
        if (db == null || db.Colors == null || db.Colors.Count == 0)
        {
            var fallbackLabel = new Label("Không tìm thấy GameDataBase hoặc mảng màu trống.") { style = { fontSize = 10, color = Color.gray } };
            paletteContainer.Add(fallbackLabel);
            return;
        }

        for (int i = 0; i < db.Colors.Count; i++)
        {
            int colorId = i;
            var colorBtn = new VisualElement();
            colorBtn.AddToClassList("color-palette-item");
            colorBtn.style.backgroundColor = db.Colors[colorId];

            if (colorId == _selectedColorId)
            {
                colorBtn.AddToClassList("color-palette-item-selected");
            }

            colorBtn.RegisterCallback<ClickEvent>(evt =>
            {
                _selectedColorId = colorId;

                paletteContainer.Query<VisualElement>(className: "color-palette-item").ForEach(item =>
                {
                    item.RemoveFromClassList("color-palette-item-selected");
                });

                colorBtn.AddToClassList("color-palette-item-selected");
            });

            paletteContainer.Add(colorBtn);
        }
    }

    // ==========================================
    // LOGIC CHUYỂN TABS BÊN TRÁI
    // ==========================================

    private void SetupTabs()
    {
        var tabBlock = _root.Q<Button>("tab-block");
        var tabMap = _root.Q<Button>("tab-map");
        var tabPipe = _root.Q<Button>("tab-pipe");
        var tabHole = _root.Q<Button>("tab-hole");
        var tabSpline = _root.Q<Button>("tab-spline");

        if (tabBlock != null) tabBlock.clicked += () => SwitchTab(TabType.Block, tabBlock);
        if (tabMap != null) tabMap.clicked += () => SwitchTab(TabType.Map, tabMap);
        if (tabPipe != null) tabPipe.clicked += () => SwitchTab(TabType.Pipe, tabPipe);
        if (tabHole != null) tabHole.clicked += () => SwitchTab(TabType.Hole, tabHole);
        if (tabSpline != null) tabSpline.clicked += () => SwitchTab(TabType.Spline, tabSpline);

        // Khởi tạo tab mặc định
        SwitchTab(TabType.Block, tabBlock);
    }

    private void SwitchTab(TabType tab, Button clickedButton)
    {
        _currentTab = tab;

        // Clear active class from all tabs
        var tabContainer = _root.Q<VisualElement>("category-tabs");
        if (tabContainer != null)
        {
            tabContainer.Query<Button>(className: "tab-button").ForEach(btn =>
            {
                btn.RemoveFromClassList("tab-button-active");
            });
        }

        if (clickedButton != null)
        {
            clickedButton.AddToClassList("tab-button-active");
        }

        // Ẩn/Hiện các panel nội dung tương ứng
        var panelBlock = _root.Q<VisualElement>("panel-tab-block");
        var panelMap = _root.Q<VisualElement>("panel-tab-map");
        var panelPipe = _root.Q<VisualElement>("panel-tab-pipe");
        var panelHole = _root.Q<VisualElement>("panel-tab-hole");
        var panelSpline = _root.Q<VisualElement>("panel-tab-spline");

        if (panelBlock != null) panelBlock.style.display = tab == TabType.Block ? DisplayStyle.Flex : DisplayStyle.None;
        if (panelMap != null) panelMap.style.display = tab == TabType.Map ? DisplayStyle.Flex : DisplayStyle.None;
        if (panelPipe != null) panelPipe.style.display = tab == TabType.Pipe ? DisplayStyle.Flex : DisplayStyle.None;
        if (panelHole != null) panelHole.style.display = tab == TabType.Hole ? DisplayStyle.Flex : DisplayStyle.None;
        if (panelSpline != null) panelSpline.style.display = tab == TabType.Spline ? DisplayStyle.Flex : DisplayStyle.None;

    }

    // ==========================================
    // LOGIC ĐIỀU KHIỂN BEZIER SPLINE & MESH
    // ==========================================

    private void SetupSplinePanel()
    {
        var btnFindSpline = _root.Q<Button>("btn-find-spline");
        if (btnFindSpline != null)
        {
            btnFindSpline.clicked += AutoFindSplineBuilder;
        }

        var splineBuilderField = _root.Q<ObjectField>("spline-builder-field");
        if (splineBuilderField != null)
        {
            splineBuilderField.RegisterValueChangedCallback(evt =>
            {
                SelectSplineBuilder((SplineMeshBuilder)evt.newValue);
            });
        }

        var btnAddPoint = _root.Q<Button>("btn-spline-add-point");
        if (btnAddPoint != null)
        {
            btnAddPoint.clicked += () =>
            {
                if (_activeSplineBuilder != null && _activeSplineBuilder.spline != null)
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
                    RefreshSplinePointsList();
                    SaveSplinePointsToLevelData();
                    Debug.Log("Đã thêm điểm spline mới (Duplicate điểm cuối).");
                }
            };
        }

        var btnRemovePoint = _root.Q<Button>("btn-spline-remove-point");
        if (btnRemovePoint != null)
        {
            btnRemovePoint.clicked += () =>
            {
                if (_activeSplineBuilder != null && _activeSplineBuilder.spline != null)
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
                        RefreshSplinePointsList();
                        SaveSplinePointsToLevelData();
                        Debug.Log("Đã xóa điểm cuối cùng của spline.");
                    }
                    else
                    {
                        Debug.LogWarning("Không thể xóa: Spline phải chứa ít nhất 2 điểm!");
                    }
                }
            };
        }

        var btnGenerate = _root.Q<Button>("btn-generate-mesh");
        if (btnGenerate != null)
        {
            btnGenerate.clicked += () =>
            {
                if (_activeSplineBuilder != null)
                {
                    Undo.RecordObject(_activeSplineBuilder, "Generate Spline Mesh");
                    _activeSplineBuilder.GenerateMesh();

                    MeshFilter filter = _activeSplineBuilder.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        if (_activeLevelData != null)
                        {
                            Undo.RecordObject(_activeLevelData, "Save Spline Mesh to LevelData");
                            _activeLevelData._meshBezierSpline = filter.sharedMesh;
                            
                            // Lưu đồng thời thông số spline points và loop
                            SaveSplinePointsToLevelData();

                            EditorUtility.SetDirty(_activeLevelData);
                            AssetDatabase.SaveAssets();
                            Debug.Log($"Đã tạo và gán mesh spline thành công vào {_activeLevelData.name}!");
                        }
                        else
                        {
                            Debug.LogWarning("Không tìm thấy file LevelData để gán mesh!");
                        }
                    }
                }
            };
        }
    }

    private void AutoFindSplineBuilder()
    {
        var builder = Object.FindFirstObjectByType<SplineMeshBuilder>();
        var splineBuilderField = _root.Q<ObjectField>("spline-builder-field");
        if (splineBuilderField != null)
        {
            splineBuilderField.value = builder;
        }
    }

    private void SelectSplineBuilder(SplineMeshBuilder builder)
    {
        _activeSplineBuilder = builder;
        var settingsContainer = _root.Q<VisualElement>("spline-settings-container");
        if (settingsContainer == null) return;

        if (_activeSplineBuilder != null)
        {
            settingsContainer.style.display = DisplayStyle.Flex;
            _serializedSplineBuilder = new SerializedObject(_activeSplineBuilder);

            var widthField = _root.Q<FloatField>("field-mesh-width");
            var resField = _root.Q<IntegerField>("field-mesh-res");
            var tilingField = _root.Q<FloatField>("field-mesh-tiling");
            var normalToggle = _root.Q<Toggle>("toggle-mesh-normal");
            var loopToggle = _root.Q<Toggle>("toggle-spline-loop");

            if (widthField != null)
            {
                widthField.BindProperty(_serializedSplineBuilder.FindProperty("width"));
                widthField.RegisterValueChangedCallback(evt =>
                {
                    if (_activeSplineBuilder != null)
                    {
                        _serializedSplineBuilder.ApplyModifiedProperties();
                        _activeSplineBuilder.GenerateMesh();
                    }
                });
            }

            if (resField != null)
            {
                resField.BindProperty(_serializedSplineBuilder.FindProperty("resolution"));
                resField.RegisterValueChangedCallback(evt =>
                {
                    if (_activeSplineBuilder != null)
                    {
                        _serializedSplineBuilder.ApplyModifiedProperties();
                        _activeSplineBuilder.GenerateMesh();
                    }
                });
            }

            if (tilingField != null)
            {
                tilingField.BindProperty(_serializedSplineBuilder.FindProperty("uvTiling"));
                tilingField.RegisterValueChangedCallback(evt =>
                {
                    if (_activeSplineBuilder != null)
                    {
                        _serializedSplineBuilder.ApplyModifiedProperties();
                        _activeSplineBuilder.GenerateMesh();
                    }
                });
            }

            if (normalToggle != null)
            {
                normalToggle.BindProperty(_serializedSplineBuilder.FindProperty("useSplineNormal"));
                normalToggle.RegisterValueChangedCallback(evt =>
                {
                    if (_activeSplineBuilder != null)
                    {
                        _serializedSplineBuilder.ApplyModifiedProperties();
                        _activeSplineBuilder.GenerateMesh();
                    }
                });
            }

            if (loopToggle != null && _activeSplineBuilder.spline != null)
            {
                var serializedSpline = new SerializedObject(_activeSplineBuilder.spline);
                loopToggle.BindProperty(serializedSpline.FindProperty("m_loop"));
                loopToggle.RegisterValueChangedCallback(evt =>
                {
                    if (_activeSplineBuilder != null && _activeSplineBuilder.spline != null)
                    {
                        serializedSpline.FindProperty("m_loop").boolValue = evt.newValue;
                        serializedSpline.ApplyModifiedProperties();
                        _activeSplineBuilder.GenerateMesh();
                        
                        // Cập nhật lưu lại loop vào LevelData
                        if (_activeLevelData != null)
                        {
                            Undo.RecordObject(_activeLevelData, "Update Spline Loop in LevelData");
                            _activeLevelData._isBezierLoop = evt.newValue;
                            EditorUtility.SetDirty(_activeLevelData);
                        }
                    }
                });
            }

            RefreshSplinePointsList();
        }
        else
        {
            settingsContainer.style.display = DisplayStyle.None;
            _serializedSplineBuilder = null;
        }
    }

    private void RefreshSplinePointsList()
    {
        var pointsList = _root.Q<ScrollView>("spline-points-list");
        if (pointsList == null) return;

        pointsList.Clear();

        if (_activeSplineBuilder == null || _activeSplineBuilder.spline == null)
        {
            var placeholder = new Label("Không tìm thấy Spline.") { style = { fontSize = 10, color = Color.gray } };
            pointsList.Add(placeholder);
            return;
        }

        var spline = _activeSplineBuilder.spline;
        for (int i = 0; i < spline.Count; i++)
        {
            int index = i;
            var pt = spline[index];
            if (pt == null) continue;

            var item = new VisualElement();
            item.AddToClassList("spline-point-item");

            // Hiển thị vị trí tọa độ
            var label = new Label($"P{index}: ({pt.localPosition.x:F2}, {pt.localPosition.y:F2}, {pt.localPosition.z:F2})");
            label.AddToClassList("spline-point-label");
            item.Add(label);

            // Nút focus camera vào point
            var btnFocus = new Button { text = "🔍" };
            btnFocus.AddToClassList("btn-spline-action");
            btnFocus.clicked += () =>
            {
                Selection.activeGameObject = pt.gameObject;
                SceneView.FrameLastActiveSceneView();
            };
            item.Add(btnFocus);

            // Nút xóa point này
            var btnDel = new Button { text = "❌" };
            btnDel.AddToClassList("btn-spline-action");
            btnDel.clicked += () =>
            {
                if (spline.Count > 2)
                {
                    Undo.RegisterCompleteObjectUndo(spline, "Remove Spline Point");
                    Undo.DestroyObjectImmediate(pt.gameObject);
                    
                    // Delay để Unity xử lý xóa Object rồi làm sạch UI
                    EditorApplication.delayCall += () =>
                    {
                        if (_activeSplineBuilder != null)
                        {
                            _activeSplineBuilder.GenerateMesh();
                            RefreshSplinePointsList();
                            SaveSplinePointsToLevelData();
                        }
                    };
                }
                else
                {
                    Debug.LogWarning("Không thể xóa: Spline phải chứa ít nhất 2 điểm!");
                }
            };
            item.Add(btnDel);

            pointsList.Add(item);
        }
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

    // ==========================================

    private void RefreshActiveLevelData()
    {
        if (levelController == null)
        {
            _activeLevelData = null;
            _serializedLevelData = null;
            var scrollRight = _root.Q<ScrollView>(className: "sidebar-right");
            if (scrollRight != null) scrollRight.Unbind();
            return;
        }

        string path = $"Assets/Project/Resources/StageLevel/Level_{levelController.CurrentLevel}.asset";
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        var lblCurrentLevel = _root.Q<Label>("lbl-current-level");
        if (lblCurrentLevel != null)
        {
            lblCurrentLevel.text = data != null ? $"Active Level: Level_{levelController.CurrentLevel}" : $"Active Level: Level_{levelController.CurrentLevel} (Chưa tạo asset)";
        }

        if (data != _activeLevelData)
        {
            _activeLevelData = data;
            var scrollRight = _root.Q<ScrollView>(className: "sidebar-right");
            if (scrollRight != null)
            {
                if (_activeLevelData != null)
                {
                    _serializedLevelData = new SerializedObject(_activeLevelData);
                    scrollRight.Bind(_serializedLevelData);
                }
                else
                {
                    _serializedLevelData = null;
                    scrollRight.Unbind();
                }
            }
        }
    }

    private void MarkDirty()
    {
        if (levelController == null) return;
        EditorUtility.SetDirty(levelController);
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(levelController.gameObject.scene);
    }
}
