using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance { get; private set; }

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    public float tileSize = 1f;
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private GameObject visualTilePrefab;
    [SerializeField] private GameObject wallTilePrefab; // Thêm prefab "Wall Tile" ở đây

    private GameObject[,] shadowTiles;

#if UNITY_EDITOR
    [SerializeField] private bool enableSnapInEditor = false;
    [SerializeField] private List<MoveBlock> moveBlocks = new List<MoveBlock>();

    public bool EnableSnapInEditor => enableSnapInEditor;
    public List<MoveBlock> MoveBlocks => moveBlocks;

    public void RefreshMoveBlocks()
    {
        moveBlocks.Clear();
        moveBlocks = new List<MoveBlock>(GetComponentsInChildren<MoveBlock>());
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitBoard();
    }

    private void InitBoard()
    {
        shadowTiles = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Shadow tile
                GameObject shadow = Instantiate(shadowPrefab, transform);
                shadow.transform.position = GetWorldPos(x, y);
                shadow.SetActive(false);
                shadowTiles[x, y] = shadow;
            }
        }

        // Generate grid visual placeholders
        GenerateGridVisual();

    }

    public void GenerateGridVisual()
    {
#if UNITY_EDITOR
        ClearGeneratedVisuals();
#endif

        if (visualTilePrefab == null || wallTilePrefab == null) return;

        for (int x = -1; x <= width; x++)
        {
            for (int y = -1; y <= height; y++)
            {
                Vector3 pos = GetWorldPosExtended(x, y); // dùng hàm mới bên dưới

                bool isInsideGrid = x >= 0 && x < width && y >= 0 && y < height;

                if (isInsideGrid)
                {
                    // Tile nền
                    GameObject tile = Instantiate(visualTilePrefab, pos, Quaternion.identity, transform);
                    tile.name = visualTilePrefab.name + $"_{x}_{y}";
                }
                else
                {
                    // Tường bao
                    GameObject wall = Instantiate(wallTilePrefab, pos, Quaternion.identity, transform);
                    wall.name = wallTilePrefab.name + $"_{x}_{y}";
                }
            }
        }
    }

    public Vector3 GetWorldPosExtended(int x, int y)
    {
        float originX = -((width - 1) * tileSize) / 2f;
        float originZ = -((height - 1) * tileSize) / 2f;

        return transform.position + new Vector3(x * tileSize + originX, 0f, y * tileSize + originZ);
    }



    public Vector3 GetWorldPos(int x, int y)
    {
        float originX = -((width - 1) * tileSize) / 2f;
        float originZ = -((height - 1) * tileSize) / 2f;

        return transform.position + new Vector3(x * tileSize + originX, 0f, y * tileSize + originZ);
    }

    public Vector3 GetWorldPos(Vector2Int gridPos)
    {
        return GetWorldPos(gridPos.x, gridPos.y);
    }

    public void ShowShadows(Vector3[] worldPositions)
    {
        ClearShadows();
        foreach (Vector3 pos in worldPositions)
        {
            Vector2Int grid = WorldToGrid(GetSnappedPosition(pos));
            if (IsInBounds(grid))
            {
                shadowTiles[grid.x, grid.y].SetActive(true);
            }
        }
    }

    public void ClearShadows()
    {
        if (shadowTiles == null) return;

        foreach (var tile in shadowTiles)
        {
            tile.SetActive(false);
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float originX = -((width - 1) * tileSize) / 2f;
        float originZ = -((height - 1) * tileSize) / 2f;

        Vector3 localPos = worldPos - transform.position;
        int x = Mathf.FloorToInt((localPos.x - originX) / tileSize);
        int y = Mathf.FloorToInt((localPos.z - originZ) / tileSize);
        return new Vector2Int(x, y);
    }

    private bool IsInBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }

    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        float originX = -((width - 1) * tileSize) / 2f;
        float originZ = -((height - 1) * tileSize) / 2f;

        Vector3 local = worldPos - transform.position;
        int x = Mathf.RoundToInt((local.x - originX) / tileSize);
        int y = Mathf.RoundToInt((local.z - originZ) / tileSize);

        return GetWorldPos(x, y);
    }

    public void ClearGeneratedVisuals()
    {
#if UNITY_EDITOR
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = children.Length - 1; i >= 0; i--)
        {
            Transform child = children[i];
            if (child != transform)
            {
                if (child.name.Contains(visualTilePrefab.name))
                {
                    DestroyImmediate(child.gameObject);
                }
                else if (child.name.Contains(wallTilePrefab.name))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
#endif
    }
}
