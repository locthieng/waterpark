using UnityEngine;

/// <summary>
/// Điều khiển tốc độ và hướng xoay của băng chuyền.
/// Quản lý một giá trị offset (0..1) tăng dần theo thời gian.
/// 
/// Thay đổi rotationSpeed hoặc rotateClockwise trên Inspector
/// → hệ thống phản hồi ngay lập tức.
/// </summary>
public class ConveyorRotator : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("══════ ROTATION ══════")]
    [Tooltip("Tốc độ xoay (độ/giây). 360 = 1 vòng/giây. 30 = 1 vòng mỗi 12 giây.")]
    [SerializeField] private float rotationSpeed = 30f;

    [Tooltip("Xoay theo chiều kim đồng hồ (nhìn từ trên xuống — trục Y).")]
    [SerializeField] private bool rotateClockwise = true;

    // ─────────────────────────────────────────────
    //  INTERNAL
    // ─────────────────────────────────────────────

    private float currentOffset = 0f; // normalized 0..1

    // ─────────────────────────────────────────────
    //  PROPERTIES
    // ─────────────────────────────────────────────

    /// <summary>Offset xoay hiện tại (0..1). Một vòng đầy đủ = 1.0.</summary>
    public float CurrentOffset => currentOffset;

    /// <summary>Tốc độ xoay hiện tại (độ/giây).</summary>
    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = value;
    }

    /// <summary>Đang xoay thuận chiều kim đồng hồ.</summary>
    public bool IsClockwise
    {
        get => rotateClockwise;
        set => rotateClockwise = value;
    }

    // ═══════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Reset offset về 0 (đưa băng chuyền về vị trí ban đầu).
    /// </summary>
    public void ResetOffset()
    {
        currentOffset = 0f;
    }

    /// <summary>
    /// Đặt offset thủ công (0..1).
    /// </summary>
    public void SetOffset(float normalizedOffset)
    {
        currentOffset = Mathf.Repeat(normalizedOffset, 1f);
    }

    /// <summary>
    /// Pause/Resume xoay. Đặt speed = 0 để pause.
    /// </summary>
    public void Pause()
    {
        rotationSpeed = 0f;
    }

    // ═══════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════

    private void Update()
    {
        if (Mathf.Approximately(rotationSpeed, 0f)) return;

        float direction = rotateClockwise ? 1f : -1f;
        currentOffset += (rotationSpeed / 360f) * direction * Time.deltaTime;
        currentOffset = Mathf.Repeat(currentOffset, 1f);
    }

    private void OnValidate()
    {
        // Không clamp rotationSpeed — cho phép giá trị âm (đảo chiều bằng speed âm)
    }
}
