using System;
using UnityEngine;

/// <summary>
/// Quản lý các slot trên băng chuyền.
/// Biết slot nào trống, slot nào đang chứa object.
/// Tính toán vị trí world-space của từng slot dựa trên ConveyorPath + ConveyorRotator.
///
/// Thay đổi slotCount trên Inspector → hệ thống tự reinitialize slots ngay lập tức.
/// </summary>
[RequireComponent(typeof(ConveyorPath))]
[RequireComponent(typeof(ConveyorRotator))]
public class ConveyorSlotManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  SLOT DATA
    // ─────────────────────────────────────────────

    [Serializable]
    public class SlotData
    {
        /// <summary>Vị trí gốc trên quỹ đạo (0..1), chưa tính rotation offset.</summary>
        [HideInInspector] public float BaseNormalizedPos;

        /// <summary>Slot đang có object không.</summary>
        public bool IsOccupied;

        /// <summary>ConveyorItem đang chiếm slot (null nếu trống).</summary>
        public ConveyorItem Occupant;

        public SlotData(float normalizedPos)
        {
            BaseNormalizedPos = normalizedPos;
            IsOccupied = false;
            Occupant = null;
        }
    }

    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("══════ SLOTS ══════")]
    [Tooltip("Số lượng slot (n) chia đều trên quỹ đạo.")]
    [SerializeField, Min(1)] private int slotCount = 8;

    [Header("══════ GIZMO ══════")]
    [Tooltip("Kích thước sphere hiển thị slot trong Scene View.")]
    [SerializeField] private float gizmoSlotSize = 0.3f;

    [Tooltip("Màu slot trống.")]
    [SerializeField] private Color emptySlotColor = Color.yellow;

    [Tooltip("Màu slot đã có object.")]
    [SerializeField] private Color occupiedSlotColor = Color.red;

    // ─────────────────────────────────────────────
    //  INTERNAL — lazy-cached references
    // ─────────────────────────────────────────────

    private ConveyorPath _path;
    private ConveyorPath Path => _path ? _path : (_path = GetComponent<ConveyorPath>());

    private ConveyorRotator _rotator;
    private ConveyorRotator Rotator => _rotator ? _rotator : (_rotator = GetComponent<ConveyorRotator>());

    private SlotData[] slots;
    private int lastSlotCount = -1; // detect thay đổi slotCount

    // ─────────────────────────────────────────────
    //  PROPERTIES
    // ─────────────────────────────────────────────

    /// <summary>Tổng số slot.</summary>
    public int SlotCount => slotCount;

    /// <summary>Số slot đang chứa object.</summary>
    public int OccupiedCount { get; private set; }

    /// <summary>Băng chuyền đã đầy.</summary>
    public bool IsFull => OccupiedCount >= slotCount;

    /// <summary>Mảng slot (read-only reference).</summary>
    public SlotData[] Slots => slots;

    // ═══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════

    private void OnEnable()
    {
        RebuildSlots();
    }

    private void OnValidate()
    {
        slotCount = Mathf.Max(1, slotCount);

        // Detect thay đổi slotCount → rebuild slots khi Play
        if (Application.isPlaying && slotCount != lastSlotCount && slots != null)
        {
            RebuildSlots();
        }
    }

    // ═══════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Tính vị trí world-space hiện tại của slot (đã tính rotation offset).
    /// </summary>
    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slotCount)
            return transform.position;

        float offset = Rotator != null ? Rotator.CurrentOffset : 0f;
        float normalizedPos = Mathf.Repeat(slots[slotIndex].BaseNormalizedPos + offset, 1f);
        return Path != null ? Path.EvaluatePosition(normalizedPos) : transform.position;
    }

    /// <summary>
    /// Lấy index slot trống đầu tiên. Trả về -1 nếu đầy.
    /// </summary>
    public int GetFirstEmptySlotIndex()
    {
        if (slots == null) return -1;
        for (int i = 0; i < slotCount; i++)
        {
            if (!slots[i].IsOccupied) return i;
        }
        return -1;
    }

    /// <summary>
    /// Kiểm tra slot có trống không.
    /// </summary>
    public bool IsSlotEmpty(int slotIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slotCount) return false;
        return !slots[slotIndex].IsOccupied;
    }

    /// <summary>
    /// Gán object vào slot. Trả về true nếu thành công.
    /// </summary>
    public bool OccupySlot(int slotIndex, ConveyorItem item)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slotCount) return false;
        if (slots[slotIndex].IsOccupied) return false;

        slots[slotIndex].IsOccupied = true;
        slots[slotIndex].Occupant = item;
        OccupiedCount++;
        return true;
    }

    /// <summary>
    /// Giải phóng slot. Trả về ConveyorItem đã gỡ (hoặc null).
    /// </summary>
    public ConveyorItem FreeSlot(int slotIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slotCount) return null;
        if (!slots[slotIndex].IsOccupied) return null;

        ConveyorItem item = slots[slotIndex].Occupant;
        slots[slotIndex].IsOccupied = false;
        slots[slotIndex].Occupant = null;
        OccupiedCount = Mathf.Max(0, OccupiedCount - 1);
        return item;
    }

    /// <summary>
    /// Cập nhật vị trí cho tất cả object đã locked.
    /// Được gọi mỗi frame bởi CircularConveyor.
    /// </summary>
    public void UpdateLockedOccupants()
    {
        if (slots == null) return;

        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].IsOccupied && slots[i].Occupant != null && slots[i].Occupant.IsLocked)
            {
                slots[i].Occupant.transform.position = GetSlotWorldPosition(i);
            }
        }
    }

    /// <summary>
    /// Rebuild mảng slot (khi slotCount thay đổi).
    /// Bảo toàn occupants cũ ở các index hợp lệ.
    /// </summary>
    public void RebuildSlots()
    {
        // Lưu tạm occupants cũ
        ConveyorItem[] existingOccupants = null;
        if (slots != null)
        {
            existingOccupants = new ConveyorItem[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].IsOccupied)
                    existingOccupants[i] = slots[i].Occupant;
            }
        }

        // Tạo mảng mới
        slots = new SlotData[slotCount];
        OccupiedCount = 0;

        for (int i = 0; i < slotCount; i++)
        {
            float normalizedPos = (float)i / slotCount;
            slots[i] = new SlotData(normalizedPos);

            // Khôi phục occupant nếu index vẫn hợp lệ
            if (existingOccupants != null && i < existingOccupants.Length && existingOccupants[i] != null)
            {
                slots[i].IsOccupied = true;
                slots[i].Occupant = existingOccupants[i];
                OccupiedCount++;
            }
        }

        lastSlotCount = slotCount;
    }

    // ═══════════════════════════════════════════════
    //  GIZMOS
    // ═══════════════════════════════════════════════

    private void OnDrawGizmos()
    {
        DrawSlotGizmos(0.7f);
    }

    private void OnDrawGizmosSelected()
    {
        DrawSlotGizmos(1f);
    }

    private void DrawSlotGizmos(float alpha)
    {
        if (Path == null) return;

        int count = Mathf.Max(1, slotCount);
        float offset = (Application.isPlaying && Rotator != null) ? Rotator.CurrentOffset : 0f;

        for (int i = 0; i < count; i++)
        {
            float t = Mathf.Repeat((float)i / count + offset, 1f);
            Vector3 pos = Path.EvaluatePosition(t);

            bool occupied = Application.isPlaying
                && slots != null
                && i < slots.Length
                && slots[i].IsOccupied;

            Color color = occupied ? occupiedSlotColor : emptySlotColor;
            color.a = alpha;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(pos, gizmoSlotSize);

#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = occupied ? occupiedSlotColor : Color.white;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            UnityEditor.Handles.Label(pos + Vector3.up * (gizmoSlotSize + 0.2f), $"[{i}]", style);
#endif
        }
    }
}
