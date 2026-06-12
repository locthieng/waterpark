using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Facade chính của hệ thống băng chuyền vòng tròn.
/// Kết nối 3 component: ConveyorPath + ConveyorRotator + ConveyorSlotManager.
/// 
/// Cung cấp API công khai duy nhất: AddObject, RemoveObject, Events.
/// Tự động thêm 3 component phụ thuộc khi gắn lên GameObject.
///
/// CÁCH DÙNG:
///   1. Gắn CircularConveyor lên Empty GameObject (các component khác tự thêm).
///   2. Chỉnh thông số trên từng component qua Inspector.
///   3. Gọi conveyor.AddObject(go) để thêm object.
/// </summary>
[RequireComponent(typeof(ConveyorPath))]
[RequireComponent(typeof(ConveyorRotator))]
[RequireComponent(typeof(ConveyorSlotManager))]
public class CircularConveyor : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("══════ SNAP ANIMATION ══════")]
    [Tooltip("Thời gian object bay vào slot (giây).")]
    [SerializeField] private float snapDuration = 0.6f;

    [Tooltip("Kiểu easing cho animation bay vào slot.")]
    [SerializeField] private LeanTweenType snapEaseType = LeanTweenType.easeOutBack;

    [Header("══════ EVENTS ══════")]
    [Tooltip("Khi object đã snap vào slot thành công. Params: (GameObject, slotIndex)")]
    public UnityEvent<GameObject, int> OnObjectSnapped;

    [Tooltip("Khi object bị gỡ khỏi slot. Params: (GameObject, slotIndex)")]
    public UnityEvent<GameObject, int> OnObjectRemoved;

    [Tooltip("Khi tất cả slot đều đã có object.")]
    public UnityEvent OnConveyorFull;

    // ─────────────────────────────────────────────
    //  COMPONENT REFERENCES (lazy-cached)
    // ─────────────────────────────────────────────

    private ConveyorPath _path;
    /// <summary>Component quản lý hình dạng quỹ đạo.</summary>
    public ConveyorPath Path => _path ? _path : (_path = GetComponent<ConveyorPath>());

    private ConveyorRotator _rotator;
    /// <summary>Component quản lý xoay.</summary>
    public ConveyorRotator Rotator => _rotator ? _rotator : (_rotator = GetComponent<ConveyorRotator>());

    private ConveyorSlotManager _slotManager;
    /// <summary>Component quản lý slot.</summary>
    public ConveyorSlotManager SlotManager => _slotManager ? _slotManager : (_slotManager = GetComponent<ConveyorSlotManager>());

    // ─────────────────────────────────────────────
    //  PROPERTIES (delegate to sub-components)
    // ─────────────────────────────────────────────

    /// <summary>Tổng số slot.</summary>
    public int SlotCount => SlotManager.SlotCount;

    /// <summary>Số slot đang có object.</summary>
    public int OccupiedCount => SlotManager.OccupiedCount;

    /// <summary>Băng chuyền đã đầy.</summary>
    public bool IsFull => SlotManager.IsFull;

    // ═══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════

    private void Update()
    {
        // Cập nhật vị trí cho tất cả object đã locked vào slot
        SlotManager.UpdateLockedOccupants();
    }

    // ═══════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Thêm object vào slot trống đầu tiên.
    /// Tự động gắn ConveyorItem nếu chưa có.
    /// </summary>
    /// <returns>true nếu thêm thành công.</returns>
    public bool AddObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[CircularConveyor] Object is null!");
            return false;
        }

        if (SlotManager.IsFull)
        {
            Debug.LogWarning($"[CircularConveyor] Băng chuyền đã đầy ({SlotCount}/{SlotCount})!");
            return false;
        }

        int emptyIndex = SlotManager.GetFirstEmptySlotIndex();
        if (emptyIndex < 0) return false;

        return Internal_AddToSlot(obj, emptyIndex);
    }

    /// <summary>
    /// Thêm object vào slot cụ thể.
    /// </summary>
    public bool AddObjectToSlot(GameObject obj, int slotIndex)
    {
        if (obj == null) return false;
        if (!SlotManager.IsSlotEmpty(slotIndex))
        {
            Debug.LogWarning($"[CircularConveyor] Slot [{slotIndex}] đã có object!");
            return false;
        }

        return Internal_AddToSlot(obj, slotIndex);
    }

    /// <summary>
    /// Gỡ object khỏi slot. Trả về GameObject (hoặc null).
    /// </summary>
    public GameObject RemoveObject(int slotIndex)
    {
        ConveyorItem item = SlotManager.FreeSlot(slotIndex);
        if (item == null) return null;

        GameObject obj = item.gameObject;
        item.Release();

        OnObjectRemoved?.Invoke(obj, slotIndex);
        return obj;
    }

    /// <summary>
    /// Gỡ tất cả object khỏi băng chuyền.
    /// </summary>
    public List<GameObject> RemoveAllObjects()
    {
        List<GameObject> removed = new List<GameObject>();
        for (int i = 0; i < SlotCount; i++)
        {
            GameObject obj = RemoveObject(i);
            if (obj != null) removed.Add(obj);
        }
        return removed;
    }

    /// <summary>
    /// Lấy vị trí world-space hiện tại của slot (delegate).
    /// </summary>
    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        return SlotManager.GetSlotWorldPosition(slotIndex);
    }

    /// <summary>
    /// Kiểm tra slot có trống không (delegate).
    /// </summary>
    public bool IsSlotEmpty(int slotIndex)
    {
        return SlotManager.IsSlotEmpty(slotIndex);
    }

    /// <summary>
    /// Lấy index slot trống đầu tiên (delegate).
    /// </summary>
    public int GetFirstEmptySlotIndex()
    {
        return SlotManager.GetFirstEmptySlotIndex();
    }

    /// <summary>
    /// Được gọi bởi ConveyorItem khi snap animation hoàn tất.
    /// </summary>
    public void NotifySnapComplete(GameObject obj, int slotIndex)
    {
        OnObjectSnapped?.Invoke(obj, slotIndex);
    }

    // ═══════════════════════════════════════════════
    //  INTERNAL
    // ═══════════════════════════════════════════════

    private bool Internal_AddToSlot(GameObject obj, int slotIndex)
    {
        // Đảm bảo có ConveyorItem
        ConveyorItem item = obj.GetComponent<ConveyorItem>();
        if (item == null)
        {
            item = obj.AddComponent<ConveyorItem>();
        }

        // Nếu item đang ở băng chuyền khác → gỡ ra trước
        if (item.ParentConveyor != null && item.ParentConveyor != this)
        {
            item.ParentConveyor.RemoveObject(item.AssignedSlotIndex);
        }

        // Gán vào slot
        if (!SlotManager.OccupySlot(slotIndex, item))
            return false;

        // Bắt đầu snap animation
        Vector3 targetPos = SlotManager.GetSlotWorldPosition(slotIndex);
        item.SnapToSlot(this, slotIndex, targetPos, snapDuration, snapEaseType);

        // Kiểm tra đầy
        if (SlotManager.IsFull)
        {
            OnConveyorFull?.Invoke();
        }

        return true;
    }
}
