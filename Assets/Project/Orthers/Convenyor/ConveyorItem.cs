using UnityEngine;

/// <summary>
/// Component gắn lên mỗi object cần đặt lên băng chuyền.
/// Tự động được thêm bởi CircularConveyor.AddObject() nếu object chưa có.
///
/// KHÔNG cần tự gắn bằng tay — CircularConveyor sẽ tự AddComponent khi cần.
/// Nếu muốn pre-configure, có thể gắn sẵn lên Prefab.
///
/// HỖ TRỢ CLICK: Nếu object có Collider, OnMouseUp sẽ tự động bay vào băng chuyền.
/// Gán TargetConveyor trên Inspector, hoặc để trống → tự tìm bằng FindObjectOfType.
/// </summary>
public class ConveyorItem : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("══════ CLICK TO FLY ══════")]
    [Tooltip("Băng chuyền đích. Để trống → tự tìm CircularConveyor trong Scene.")]
    [SerializeField] private CircularConveyor targetConveyor;

    // ─────────────────────────────────────────────
    //  PUBLIC STATE (Read-Only)
    // ─────────────────────────────────────────────

    /// <summary>Object đang bay vào slot (animation đang chạy).</summary>
    public bool IsSnapping { get; private set; }

    /// <summary>Object đã bám chặt slot — đang xoay đồng bộ cùng băng chuyền.</summary>
    public bool IsLocked { get; private set; }

    /// <summary>Reference đến băng chuyền đang chứa object này.</summary>
    public CircularConveyor ParentConveyor { get; private set; }

    /// <summary>Index của slot đang chiếm (-1 nếu chưa gán).</summary>
    public int AssignedSlotIndex { get; private set; } = -1;

    // ─────────────────────────────────────────────
    //  INTERNAL
    // ─────────────────────────────────────────────

    private int tweenId = -1;
    private Vector3 snapStartPosition;
    private float snapProgress;

    // ═══════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Bắt đầu animation bay vào slot.
    /// Được gọi bởi CircularConveyor — không nên gọi trực tiếp.
    ///
    /// Sử dụng LeanTween.value để nội suy progress (0→1) với easing,
    /// kết hợp cập nhật vị trí thực trong LateUpdate để bám theo slot đang xoay.
    /// Hiệu ứng "đuổi theo đích" mượt mà, giống unit bay vào ô trong Auto Battler.
    /// </summary>
    public void SnapToSlot(CircularConveyor conveyor, int slotIndex, Vector3 initialTargetPos,
                           float duration, LeanTweenType easeType)
    {
        ParentConveyor = conveyor;
        AssignedSlotIndex = slotIndex;
        IsSnapping = true;
        IsLocked = false;

        snapStartPosition = transform.position;
        snapProgress = 0f;

        CancelTween();

        // LeanTween.value animate progress 0→1 với easing curve.
        // Vị trí thực tế được cập nhật trong LateUpdate để luôn bám theo slot đang xoay.
        tweenId = LeanTween.value(gameObject, OnSnapProgressUpdate, 0f, 1f, duration)
            .setEase(easeType)
            .setOnComplete(OnSnapComplete)
            .id;
    }

    /// <summary>
    /// Giải phóng object khỏi băng chuyền.
    /// Sau khi gọi, object tự do — không còn bị khóa vào slot.
    /// </summary>
    public void Release()
    {
        CancelTween();

        IsSnapping = false;
        IsLocked = false;
        ParentConveyor = null;
        AssignedSlotIndex = -1;
        snapProgress = 0f;
    }

    // ═══════════════════════════════════════════════
    //  CLICK HANDLER
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Khi user click chuột và thả trên object (cần Collider).
    /// Object sẽ tự động bay vào slot trống trên băng chuyền đích.
    /// </summary>
    private void OnMouseUp()
    {
        // Bỏ qua nếu đã trên băng chuyền hoặc đang bay vào
        if (IsSnapping || IsLocked) return;

        // Tìm conveyor đích
        CircularConveyor conveyor = targetConveyor;
        if (conveyor == null)
        {
            conveyor = FindObjectOfType<CircularConveyor>();
        }

        if (conveyor == null)
        {
            Debug.LogWarning($"[ConveyorItem] Không tìm thấy CircularConveyor trong Scene!");
            return;
        }

        if (conveyor.IsFull)
        {
            Debug.LogWarning($"[ConveyorItem] Băng chuyền đã đầy!");
            return;
        }

        // Bay vào slot trống
        conveyor.AddObject(gameObject);
    }

    // ═══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════

    private void LateUpdate()
    {
        // Khi đang snap: lerp giữa vị trí xuất phát và vị trí slot đích hiện tại.
        // Slot đích di chuyển theo rotation của băng chuyền → tạo hiệu ứng "đuổi theo".
        if (IsSnapping && ParentConveyor != null && AssignedSlotIndex >= 0)
        {
            Vector3 currentTargetPos = ParentConveyor.GetSlotWorldPosition(AssignedSlotIndex);
            transform.position = Vector3.Lerp(snapStartPosition, currentTargetPos, snapProgress);
        }
        // Lưu ý: khi IsLocked = true, vị trí được cập nhật bởi
        // ConveyorSlotManager.UpdateLockedOccupants() → không xử lý ở đây.
    }

    private void OnDestroy()
    {
        CancelTween();

        // Dọn dẹp slot nếu vẫn đang được gán
        if (ParentConveyor != null && AssignedSlotIndex >= 0)
        {
            ParentConveyor.RemoveObject(AssignedSlotIndex);
        }
    }

    // ═══════════════════════════════════════════════
    //  INTERNAL CALLBACKS
    // ═══════════════════════════════════════════════

    private void OnSnapProgressUpdate(float value)
    {
        snapProgress = value;
    }

    private void OnSnapComplete()
    {
        IsSnapping = false;
        IsLocked = true;
        tweenId = -1;
        snapProgress = 1f;

        // Snap chính xác vào vị trí slot
        if (ParentConveyor != null && AssignedSlotIndex >= 0)
        {
            transform.position = ParentConveyor.GetSlotWorldPosition(AssignedSlotIndex);
        }

        ParentConveyor?.NotifySnapComplete(gameObject, AssignedSlotIndex);
    }

    private void CancelTween()
    {
        if (tweenId >= 0)
        {
            LeanTween.cancel(tweenId);
            tweenId = -1;
        }
    }
}
