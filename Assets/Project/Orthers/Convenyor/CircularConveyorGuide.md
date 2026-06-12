# 🎡 Hướng Dẫn — Hệ Thống Băng Chuyền Vòng Tròn (Circular Conveyor System)

> **Phiên bản**: 2.0 — Kiến trúc Modular  
> **Cập nhật**: Tách thành 4 component độc lập, hỗ trợ real-time Inspector.

---

## Mục Lục
- [Tổng Quan Kiến Trúc](#tổng-quan-kiến-trúc)
- [Cấu Trúc File](#cấu-trúc-file)
- [Hướng Dẫn Setup](#hướng-dẫn-setup)
- [Giải Thích Từng Component](#giải-thích-từng-component)
  - [ConveyorPath — Hình Dạng](#1-conveyorpath--hình-dạng)
  - [ConveyorRotator — Xoay](#2-conveyorrotator--xoay)
  - [ConveyorSlotManager — Slot](#3-conveyorslotmanager--slot)
  - [CircularConveyor — Điều Phối](#4-circularconveyor--điều-phối)
  - [ConveyorItem — Object](#5-conveyoritem--object)
- [Các Chế Độ Hình Dạng](#các-chế-độ-hình-dạng-pathmode)
- [Flow Hoạt Động](#flow-hoạt-động)
- [Sử Dụng Bằng Code](#sử-dụng-bằng-code)
- [Events / Callbacks](#events--callbacks)
- [Gizmo Trong Scene View](#gizmo-trong-scene-view)
- [Real-Time Inspector Update](#real-time-inspector-update)
- [FAQ & Troubleshooting](#faq--troubleshooting)

---

## Tổng Quan Kiến Trúc

Hệ thống được **tách thành 4 component độc lập**, mỗi component chịu trách nhiệm 1 chức năng duy nhất:

```
┌──────────────────────────────────────────────────────┐
│              GameObject "ConveyorCenter"              │
│                                                      │
│  ┌────────────────┐  ┌────────────────┐              │
│  │  ConveyorPath   │  │ ConveyorRotator│              │
│  │  (Hình dạng)    │  │ (Xoay)         │              │
│  └───────┬────────┘  └───────┬────────┘              │
│          │                   │                       │
│  ┌───────▼───────────────────▼────────┐              │
│  │       ConveyorSlotManager          │              │
│  │       (Quản lý Slot)               │              │
│  └───────────────┬────────────────────┘              │
│                  │                                   │
│  ┌───────────────▼────────────────────┐              │
│  │       CircularConveyor             │              │
│  │       (Facade — API + Events)      │              │
│  └────────────────────────────────────┘              │
└──────────────────────────────────────────────────────┘

         ┌──────────────┐
         │ ConveyorItem  │  ← Gắn trên mỗi object
         │ (Snap + Lock) │     trên băng chuyền
         └──────────────┘
```

**Ưu điểm của kiến trúc modular:**
- ✅ Mỗi component có Inspector riêng biệt, dễ chỉnh sửa
- ✅ Thay đổi bất kỳ thông số nào → hệ thống phản hồi ngay lập tức
- ✅ Có thể enable/disable từng component độc lập
- ✅ Dễ mở rộng — thêm component mới không ảnh hưởng component cũ
- ✅ Code gọn gàng, dễ đọc, dễ debug

---

## Cấu Trúc File

```
Assets/LocProject/Orthers/Convenyor/
├── ConveyorPath.cs           ← Quản lý hình dạng quỹ đạo
├── ConveyorRotator.cs        ← Quản lý xoay (tốc độ, hướng)
├── ConveyorSlotManager.cs    ← Quản lý slot (thêm/gỡ/vị trí)
├── CircularConveyor.cs       ← Facade — API + Events + Snap
├── ConveyorItem.cs           ← Component trên mỗi object
├── CircularConveyorGuide.md  ← Tài liệu này
└── Scene/
    └── Demo.unity
```

---

## Hướng Dẫn Setup

### Bước 1 — Tạo Tâm Băng Chuyền
1. Tạo **Empty GameObject** trong Scene → đặt tên `ConveyorCenter`.
2. Đặt vị trí tại tâm mong muốn.

### Bước 2 — Gắn Script
1. Chọn `ConveyorCenter`.
2. **Add Component** → tìm `CircularConveyor`.
3. **Unity tự động thêm** 3 component phụ thuộc: `ConveyorPath`, `ConveyorRotator`, `ConveyorSlotManager`.

### Bước 3 — Cấu Hình
Chỉnh từng component trên Inspector:

| Component | Chỉnh gì |
|---|---|
| **ConveyorPath** | Hình dạng (Circle/Ellipse/Spline), bán kính, waypoints |
| **ConveyorRotator** | Tốc độ xoay, hướng xoay |
| **ConveyorSlotManager** | Số lượng slot, màu Gizmo |
| **CircularConveyor** | Thời gian snap, easing, Events |

### Bước 4 — Test
- Bấm **Play** → gọi `conveyor.AddObject(obj)` từ script khác.

---

## Giải Thích Từng Component

### 1. ConveyorPath — Hình Dạng

Quản lý hình dạng quỹ đạo. **Thay đổi bất kỳ giá trị nào → Gizmo cập nhật ngay.**

| Biến | Type | Mô Tả |
|---|---|---|
| `Path Mode` | Enum | **Circle** / **Ellipse** / **FreeformSpline** |
| `Radius` | float | Bán kính vòng tròn (Circle mode) |
| `Radius X` | float | Bán kính trục X (Ellipse mode) |
| `Radius Z` | float | Bán kính trục Z (Ellipse mode) |
| `Waypoints` | Transform[] | Điểm điều khiển spline (Freeform mode, ≥3 điểm) |
| `Spline Resolution` | int | Độ mịn spline (10–100) |
| `Path Color` | Color | Màu đường Gizmo |
| `Gizmo Segments` | int | Số đoạn vẽ Gizmo (32–128) |

**Tính năng đặc biệt:**
- Auto-detect khi waypoints di chuyển lúc runtime (qua position hashing)
- Tự động rebuild spline cache khi phát hiện thay đổi

---

### 2. ConveyorRotator — Xoay

Quản lý tốc độ và hướng xoay.

| Biến | Type | Mô Tả |
|---|---|---|
| `Rotation Speed` | float | Tốc độ (độ/giây). `30` = 12 giây/vòng. `360` = 1 vòng/giây |
| `Rotate Clockwise` | bool | Thuận chiều kim đồng hồ (nhìn từ trên) |

**API bổ sung:**
```csharp
rotator.Pause();              // Dừng xoay (speed = 0)
rotator.ResetOffset();        // Đưa về vị trí ban đầu
rotator.SetOffset(0.5f);      // Đặt offset thủ công (0..1)
rotator.RotationSpeed = 60f;  // Thay đổi tốc độ bằng code
rotator.IsClockwise = false;  // Đổi hướng bằng code
```

---

### 3. ConveyorSlotManager — Slot

Quản lý slot: biết slot nào trống, slot nào có object.

| Biến | Type | Mô Tả |
|---|---|---|
| `Slot Count` | int | Số lượng slot (n). **Thay đổi → auto rebuild** |
| `Gizmo Slot Size` | float | Kích thước sphere Gizmo |
| `Empty Slot Color` | Color | Màu slot trống |
| `Occupied Slot Color` | Color | Màu slot đã có object |

**Khi thay đổi `Slot Count` lúc Play:**
- Hệ thống tự reinitialize mảng slot
- Bảo toàn object ở các slot index còn hợp lệ
- Gizmo cập nhật ngay lập tức

---

### 4. CircularConveyor — Điều Phối (Facade)

Facade kết nối 3 component trên. Cung cấp API công khai duy nhất.

| Biến | Type | Mô Tả |
|---|---|---|
| `Snap Duration` | float | Thời gian bay vào slot (giây) |
| `Snap Ease Type` | LeanTweenType | Kiểu easing animation |
| `OnObjectSnapped` | UnityEvent | Khi object snap xong |
| `OnObjectRemoved` | UnityEvent | Khi object bị gỡ |
| `OnConveyorFull` | UnityEvent | Khi đầy slot |

---

### 5. ConveyorItem — Object

Gắn trên mỗi object trên băng chuyền. **Tự động được thêm** bởi `AddObject()`.

| Property | Type | Mô Tả |
|---|---|---|
| `IsSnapping` | bool | Đang bay vào slot |
| `IsLocked` | bool | Đã bám chặt, xoay đồng bộ |
| `ParentConveyor` | CircularConveyor | Băng chuyền chủ |
| `AssignedSlotIndex` | int | Index slot đang chiếm |

---

## Các Chế Độ Hình Dạng (PathMode)

### 1. Circle — Vòng Tròn
```
x = radius × cos(t × 2π)
z = radius × sin(t × 2π)
```
Đơn giản nhất — chỉ cần 1 biến `Radius`.

### 2. Ellipse — Hình Elip
```
x = radiusX × cos(t × 2π)
z = radiusZ × sin(t × 2π)
```
Nếu `radiusX == radiusZ` → trở thành hình tròn.

### 3. FreeformSpline — Đường Cong Tự Do
- **Catmull-Rom Spline** qua các waypoints (closed loop).
- **Arc-length parameterization** → slot phân bố đều theo chiều dài thực.
- Có thể tạo BẤT KỲ hình dạng: hình vuông bo góc, hình trái tim, số 8, v.v.
- **Tối thiểu 3 waypoints**. Thứ tự waypoints quyết định hình dạng.
- Auto-detect waypoint di chuyển lúc runtime.

---

## Flow Hoạt Động

```
         Object mới bay đến
                │
     ┌──────────▼───────────┐
     │  conveyor.AddObject() │
     └──────────┬───────────┘
                │
     ┌──────────▼───────────┐      ┌──────────────┐
     │  SlotManager:         │──NO─▶│ return false  │
     │  Có slot trống?       │      │ (đầy)         │
     └──────────┬───────────┘      └──────────────┘
               YES
                │
     ┌──────────▼───────────┐
     │  SlotManager:         │
     │  OccupySlot(i, item)  │
     └──────────┬───────────┘
                │
     ┌──────────▼──────────────────────┐
     │  ConveyorItem.SnapToSlot()      │
     │  LeanTween.value (0→1, easing)  │
     │  LateUpdate: Lerp → slot đích   │
     └──────────┬──────────────────────┘
                │
     ┌──────────▼───────────┐
     │  OnSnapComplete       │
     │  IsLocked = true      │
     │  → OnObjectSnapped    │
     └──────────┬───────────┘
                │
     ┌──────────▼────────────────────────────┐
     │  Mỗi frame (Update):                  │
     │  SlotManager.UpdateLockedOccupants()   │
     │  → position = Path.Evaluate(t+offset) │
     │  → Object xoay đồng bộ               │
     │  → Rotation KHÔNG bị thay đổi          │
     └────────────────────────────────────────┘
```

---

## Sử Dụng Bằng Code

### Thêm / Gỡ Object

```csharp
CircularConveyor conveyor = GetComponent<CircularConveyor>();

// Thêm vào slot trống đầu tiên
conveyor.AddObject(myObj);

// Thêm vào slot cụ thể
conveyor.AddObjectToSlot(myObj, 3);

// Gỡ object ở slot 2
GameObject removed = conveyor.RemoveObject(2);

// Gỡ tất cả
List<GameObject> all = conveyor.RemoveAllObjects();
```

### Truy Cập Sub-Components

```csharp
// Truy cập trực tiếp từng component
ConveyorPath path = conveyor.Path;
ConveyorRotator rotator = conveyor.Rotator;
ConveyorSlotManager slotMgr = conveyor.SlotManager;

// Thay đổi hình dạng lúc runtime (chỉnh trên Inspector hoặc code)
// → Gizmo và slot vị trí tự cập nhật

// Pause / Resume
rotator.Pause();
rotator.RotationSpeed = 60f; // Resume nhanh hơn

// Kiểm tra
bool full = conveyor.IsFull;
int empty = conveyor.GetFirstEmptySlotIndex();
Vector3 pos = conveyor.GetSlotWorldPosition(0);
```

### Ví Dụ: Spawner

```csharp
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private CircularConveyor conveyor;
    [SerializeField] private GameObject prefab;

    public void SpawnAndAdd()
    {
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        if (!conveyor.AddObject(obj))
        {
            Debug.Log("Băng chuyền đầy!");
            Destroy(obj);
        }
    }
}
```

---

## Events / Callbacks

| Event | Params | Khi Nào |
|---|---|---|
| `OnObjectSnapped` | `(GameObject, int)` | Object bay vào slot xong |
| `OnObjectRemoved` | `(GameObject, int)` | Object bị gỡ khỏi slot |
| `OnConveyorFull` | *(none)* | Tất cả slot đều có object |

```csharp
void Start()
{
    conveyor.OnObjectSnapped.AddListener((obj, idx) =>
        Debug.Log($"{obj.name} → slot [{idx}]"));

    conveyor.OnObjectRemoved.AddListener((obj, idx) =>
        Debug.Log($"{obj.name} rời slot [{idx}]"));

    conveyor.OnConveyorFull.AddListener(() =>
        Debug.Log("Đầy!"));
}
```

---

## Gizmo Trong Scene View

| Hình vẽ | Component | Ý nghĩa |
|---|---|---|
| Đường màu (cyan mặc định) | ConveyorPath | Quỹ đạo |
| Sphere magenta | ConveyorPath | Waypoints (Freeform mode) |
| Sphere vàng | ConveyorSlotManager | Slot trống |
| Sphere đỏ | ConveyorSlotManager | Slot đã có object |
| Số `[i]` | ConveyorSlotManager | Index slot |
| Sphere trắng ở tâm | ConveyorPath | Tâm băng chuyền |
| Tia đỏ / xanh | ConveyorPath | Hướng X / Z |

---

## Real-Time Inspector Update

Hệ thống được thiết kế để **phản hồi ngay lập tức** khi thay đổi thông số:

| Thay đổi gì | Phản hồi |
|---|---|
| `Radius`, `RadiusX`, `RadiusZ` | Gizmo resize ngay lập tức |
| `Path Mode` | Gizmo chuyển hình dạng ngay |
| `Slot Count` | Gizmo thêm/bớt slot markers ngay. Runtime: auto rebuild |
| `Rotation Speed` | Tốc độ thay đổi ngay frame tiếp |
| `Rotate Clockwise` | Đảo hướng ngay frame tiếp |
| `Waypoints` (di chuyển) | Auto-detect qua hash → rebuild spline |
| `Path Color` / `Slot Color` | Gizmo đổi màu ngay |
| `Gizmo Slot Size` | Slot markers resize ngay |

**Cơ chế hoạt động:**
- **Gizmo** đọc giá trị hiện tại mỗi frame → luôn đồng bộ
- **OnValidate()** trên mỗi component → clamp giá trị + trigger rebuild
- **ConveyorPath** hash waypoint positions mỗi frame → detect di chuyển

---

## FAQ & Troubleshooting

### Q: Sao có 4 component trên Inspector thay vì 1?
**A:** Kiến trúc modular — mỗi component 1 chức năng. Dễ chỉnh sửa, dễ debug, dễ mở rộng. `CircularConveyor` sẽ tự động thêm 3 component kia khi bạn gắn nó.

### Q: Có thể xóa bớt component không?
**A:** Không nên. `CircularConveyor` dùng `[RequireComponent]` → Unity sẽ cảnh báo nếu bạn xóa component phụ thuộc.

### Q: Muốn pause băng chuyền?
**A:** `conveyor.Rotator.Pause()` hoặc đặt `Rotation Speed = 0` trên Inspector.

### Q: Thay đổi slotCount lúc runtime?
**A:** Chỉnh trên Inspector khi đang Play → `ConveyorSlotManager.OnValidate()` tự rebuild. Object ở slot index vượt quá sẽ bị orphan.

### Q: Spline không hiện gì?
**A:** Kiểm tra `ConveyorPath` → `Waypoints` đã có ≥ 3 Transform không null.

### Q: Muốn thay đổi waypoints lúc runtime?
**A:** Chỉ cần di chuyển Transform → `ConveyorPath` auto-detect qua hash. Hoặc gọi `conveyor.Path.RebuildSplineCache()`.

### Q: Object xoay hướng mặt trên băng chuyền?
**A:** Hệ thống **KHÔNG** thay đổi rotation. Nếu cần LookAt, tự thêm logic trong script riêng.

---

*Circular Conveyor System v2.0 — Modular Architecture — Dự án LocThieng*
