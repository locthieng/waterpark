using UnityEngine;

/// <summary>
/// Script tiện ích hỗ trợ kiểm thử runtime các tính năng của Block và BlockCell trong Unity Editor.
/// Cho phép trigger các hàm thông qua chuột phải (ContextMenu) trong Play Mode.
/// </summary>
public class BlockCellTestHelper : MonoBehaviour
{
    [Header("Testing References")]
    [Tooltip("BlockCell cần test")]
    public BlockCell testCell;

    [Tooltip("Prefab của Block để spawn")]
    public Block blockPrefab;

    [Header("Test Configuration")]
    [Tooltip("Danh sách màu sắc ban đầu của các block trong stack (từ đáy lên đỉnh)")]
    public System.Collections.Generic.List<int> testColors = new System.Collections.Generic.List<int> { 1, 1, 1 };

    [Tooltip("Khoảng cách giữa các khối trong hàng")]
    public float blockSpacing = 1f;

    private void Start()
    {
        if (testCell == null || blockPrefab == null)
        {
            Debug.LogError("[BlockCellTestHelper] Sếp hãy gán đầy đủ Test Cell và Block Prefab trên Inspector!", this);
            return;
        }

        Debug.Log($"[BlockCellTestHelper] Khởi tạo sinh {testColors.Count} khối cho Cell: {testCell.name} theo hướng góc {testCell.SpawnerDirectionAngleZ} độ.");
        testCell.InitializeStack(testColors, blockPrefab, blockSpacing);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Các lệnh ContextMenu để sếp click chuột phải test ở Editor
    // ══════════════════════════════════════════════════════════════════════════

    [ContextMenu("Test: Collect Top Block")]
    public void TestCollectTopBlock()
    {
        if (testCell == null || testCell.IsEmpty)
        {
            Debug.LogWarning("[BlockCellTestHelper] Cell rỗng, không thể thu thập thêm khối!");
            return;
        }

    }


    [ContextMenu("Test: Add Block to Top")]
    public void TestAddBlockToTop()
    {
        if (testCell == null || blockPrefab == null) return;

        Block newBlock = Instantiate(blockPrefab, testCell.transform);
        // Sử dụng màu cuối cùng trong list hoặc màu test mặc định (ví dụ màu số 1)
        int colorID = testColors.Count > 0 ? testColors[testColors.Count - 1] : 1;
        newBlock.Init(colorID, testCell);

        testCell.AddBlockToTop(newBlock);
        testCell.RepositionBlocks(blockSpacing);
        
        Debug.Log($"[BlockCellTestHelper] Đã thêm khối mới màu {colorID} vào ĐỈNH stack.");
    }

    [ContextMenu("Test: Add Block to Bottom")]
    public void TestAddBlockToBottom()
    {
        if (testCell == null || blockPrefab == null) return;

        Block newBlock = Instantiate(blockPrefab, testCell.transform);
        int colorID = testColors.Count > 0 ? testColors[0] : 1;
        newBlock.Init(colorID, testCell);
        
        testCell.AddBlockToBottom(newBlock);
        testCell.RepositionBlocks(blockSpacing);

        Debug.Log($"[BlockCellTestHelper] Đã thêm khối mới màu {colorID} vào ĐÁY stack. Đã dịch chuyển các khối cũ nối đuôi tiếp theo.");
    }
}
