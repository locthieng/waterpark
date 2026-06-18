# 📋 TODO — Các tính năng còn thiếu so với tài liệu thiết kế

> **Ngày tạo**: 2026-06-18  
> **Nguồn tham chiếu**: `block_cell_priority_rules.md`, `block_cell_types_rules.md`, `hole_vs_blockcell_by_type.md`  
> **Phương pháp**: So sánh mô tả trong 3 file tài liệu với source code thực tế trong `Assets/Project/Scripts/`

---

## ⚠️ NHẬN ĐỊNH TỔNG QUAN

**Phần lớn hệ thống gameplay mô tả trong tài liệu thiết kế CHƯA ĐƯỢC TRIỂN KHAI.**

Các file source code hiện tại hầu hết là **stub** (khung rỗng):

| File hiện có | Dòng code | Trạng thái |
|---|:---:|---|
| `BlockCell.cs` | **12 dòng** | Chỉ có field `PathDistForCollect` và getter |
| `Block.cs` | **7 dòng** | Chỉ có field `ParentHole` |
| `BlockCellData.cs` | 32 dòng | ✅ Đã có enum `BlockCellType` + data class |
| `BlockCellController.cs` | 51 dòng | Chỉ có `AllBlockCells` list + `InitializeCellDistances()` |
| `Hole.cs` | 209 dòng | Có Bezier movement + distance check cơ bản, **nhưng chưa có logic thu thập khối** |

**Đường dẫn file thực tế** khác với đường dẫn trong tài liệu:
- ✅ Thực tế: `Assets/Project/Scripts/GamePlay/`, `Assets/Project/Scripts/Data/`, `Assets/Project/Scripts/Controller/`
- ❌ Tài liệu ghi: `Assets/Scripts/Assembly-CSharp/` (không tồn tại)

---

## Chú thích trạng thái

| Ký hiệu | Ý nghĩa |
|:---:|---|
| ❌ | **Chưa triển khai** — Hoàn toàn chưa có trong code |
| ⚠️ | **Triển khai chưa đầy đủ** — Có một phần nhưng thiếu logic theo tài liệu |
| ✅ | **Đã triển khai đủ** — Phù hợp với mô tả tài liệu |

---

## PHẦN A — CẤU TRÚC DỮ LIỆU & ENUM CÒN THIẾU

### ❌ A.1 — Enum `BlockCellAccessType`

- **Tài liệu yêu cầu** (cả 3 file): Enum gồm `Accessible`, `ConnectedToAccessible`, `NotAccessible`
- **Hiện trạng**: Hoàn toàn **không tồn tại** trong codebase
- **Việc cần làm**: Tạo file `BlockCellAccessType.cs` trong `Assets/Project/Scripts/Data/`
  ```csharp
  public enum BlockCellAccessType
  {
      Accessible = 0,
      ConnectedToAccessible = 1,
      NotAccessible = 2,
  }
  ```

### ❌ A.2 — Enum `HoleType` (Màu sắc Hole)

- **Tài liệu yêu cầu**: `CurHoleType` trên Hole, `TopHoleType` trên BlockCell — dùng để so khớp màu
- **Hiện trạng**: `Hole.cs` chỉ có `_ColorID` (int), không có enum HoleType. Không có cơ chế so khớp màu
- **Việc cần làm**: Tạo hoặc hoàn thiện enum `HoleType` (nếu đã tồn tại ở nơi khác) để phân biệt các loại Hole theo màu

---

## PHẦN B — BLOCK.CS (Hiện tại: 7 dòng stub)

### ❌ B.1 — Dữ liệu cơ bản của Block

- **Tài liệu yêu cầu**: Mỗi Block có màu sắc (`BlockCol`/`HoleType`), trạng thái top/not-top
- **Hiện trạng**: Chỉ có `private Hole ParentHole`
- **Việc cần làm**: Thêm các field/property:
  - `BlockCol` hoặc `HoleType` — màu sắc của khối
  - `IsTop` — trạng thái khối đỉnh
  - Reference tới `BlockCell` chứa khối này

### ❌ B.2 — `SetAsTop()` / `SetAsNotTop()`

- **Tài liệu yêu cầu**: Khối đỉnh được highlight, các khối dưới bị ẩn/mờ
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Thêm method thay đổi visual state (scale, material, visibility)

### ❌ B.3 — `JumpToNewPos()`

- **Tài liệu yêu cầu**: Hoạt ảnh nhảy khi Spawner bắn khối mới
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Thêm method animation di chuyển khối từ Spawner đến ô đích (dùng LeanTween/DOTween)

### ❌ B.4 — `SetPathDistForCollect()`

- **Tài liệu yêu cầu**: Mỗi block có thể tự biết khoảng cách path
- **Hiện trạng**: Không tồn tại (chỉ có trên `BlockCell`)
- **Việc cần làm**: Đánh giá và thêm nếu cần

### ❌ B.5 — Visual State Management

- **Tài liệu yêu cầu**: `SetVisible()`, quản lý hiển thị khối
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Thêm các method quản lý hiển thị (show/hide, alpha, scale)

---

## PHẦN C — BLOCKCELL.CS (Hiện tại: 12 dòng stub)

> **Đây là file cần nhiều việc nhất** — gần như toàn bộ logic gameplay core nằm ở đây

### ❌ C.1 — Thuộc tính cơ bản của BlockCell

- **Tài liệu yêu cầu**: `CellType`, `AccessType`, `CurBlocks` (stack), `TopBlock`, `TopHoleType`, `CurVisibleBlockCt`
- **Hiện trạng**: Chỉ có `PathDistForCollect`
- **Việc cần làm**: Thêm tất cả các field/property:
  ```
  - BlockCellType CellType          → Loại ô (từ BlockCellData)
  - BlockCellAccessType AccessType  → Trạng thái truy cập  
  - List<Block> CurBlocks           → Stack các khối hiện tại
  - Block TopBlock                  → Property trả về khối đỉnh
  - HoleType TopHoleType            → Màu của khối đỉnh
  - int CurVisibleBlockCt           → Số khối hiển thị
  ```

### ❌ C.2 — Quan hệ Parent-Child giữa các BlockCell

- **Tài liệu yêu cầu**: `ParentBlockCells` (List) và `ChildBlockCells` (List) tạo cấu trúc chuỗi cây
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Thêm `List<BlockCell> ParentBlockCells` và `List<BlockCell> ChildBlockCells`, thiết lập khi khởi tạo màn chơi

### ❌ C.3 — `GetMatchingBlocksFromTop()`

- **Tài liệu yêu cầu**: Đếm số khối liên tiếp cùng màu từ đỉnh stack
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement logic duyệt `CurBlocks` từ trên xuống, đếm khối liên tiếp cùng `HoleType`

### ❌ C.4 — `CollectBlocks(count, collectPoint)`

- **Tài liệu yêu cầu**: Lấy khối ra khỏi stack, tạo animation bay theo Bezier curve đến Hole, stagger delay giữa các khối
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement:
  1. Xóa `count` khối từ đỉnh `CurBlocks`
  2. Animate từng khối bay đến `collectPoint` (Bezier curve)
  3. Stagger delay giữa các khối
  4. Callback khi hoàn tất

### ❌ C.5 — `UpdateTopBlock()`

- **Tài liệu yêu cầu**: Khi khối đỉnh bị hút, khối bên dưới trở thành top → có thể thay đổi màu yêu cầu
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement logic cập nhật khối đỉnh mới, gọi `SetAsTop()` trên block mới

### ❌ C.6 — `TryPullBlocksFromParent()`

- **Tài liệu yêu cầu**: Khi ô con trống, kéo khối từ ô cha trượt xuống lấp đầy
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement logic:
  1. Kiểm tra `ParentBlockCells` có khối không
  2. Lấy khối từ ô cha
  3. Animate khối trượt xuống
  4. Cập nhật cả ô cha và ô con

### ❌ C.7 — `TrySpawnBlocks()` (cho BlockSpawner)

- **Tài liệu yêu cầu**: Khi ô đích trống, Spawner sinh khối mới, bắn theo `SpawnerDirectionAngleZ`, animation `JumpToNewPos`
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement logic:
  1. Kiểm tra Strength còn lại
  2. Tạo khối mới với màu đã định sẵn
  3. Animate `JumpToNewPos()` theo hướng bắn
  4. Giảm Strength, cập nhật hiển thị

### ❌ C.8 — Spawner-specific fields

- **Tài liệu yêu cầu**: `SpawnerDirectionAngleZ`, `SpawnWaitTimeAfterFill`, `SpawnerIndicatorRenderer`
- **Hiện trạng**: `SpawnerDirectionAngleZ` chỉ có trong `BlockCellData`, không có runtime
- **Việc cần làm**: Thêm các field spawner-specific vào `BlockCell` hoặc tạo sub-class riêng

---

## PHẦN D — BLOCKCELLCONTROLLER.CS (Hiện tại: 51 dòng, thiếu phần quản lý)

> Tài liệu gọi đây là `BlockCellManager` — trong code hiện tại là `BlockCellController`

### ❌ D.1 — `CalcAccessibleBlocks()` / `GetAccessibleBlocksForHole()`

- **Tài liệu yêu cầu**: Quét tất cả cell, lọc ra các ô khả dụng cho Hole dựa trên khoảng cách + AccessType
- **Hiện trạng**: Chỉ có `GetCellByIndex()` — không lọc theo AccessType hay màu sắc
- **Việc cần làm**: Implement method lọc cell cho Hole theo:
  1. `PathDistForCollect` trong tầm
  2. `AccessType == Accessible`
  3. `TopHoleType` cùng màu với Hole

### ❌ D.2 — `RecalculateAccessibility()` (Thuật toán DFS)

- **Tài liệu yêu cầu**: Khi ô `BlockAccessible` trống, chạy DFS/BFS cập nhật trạng thái accessibility cho các ô liên kết phía sau (`ConnectedToAccessible` → `Accessible`)
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement thuật toán DFS cập nhật trạng thái truy cập trên toàn bộ chuỗi cell

### ❌ D.3 — Quản lý quan hệ Parent-Child cells

- **Tài liệu yêu cầu**: Controller biết cấu trúc cây của tất cả cells
- **Hiện trạng**: Chỉ có danh sách phẳng `AllBlockCells`
- **Việc cần làm**: Thêm logic xây dựng và quản lý cấu trúc cây khi khởi tạo màn chơi

---

## PHẦN E — HOLE.CS (Hiện tại: 209 dòng, có cơ bản nhưng thiếu nhiều)

### ⚠️ E.1 — Logic thu thập khối (Core Gameplay Loop)

- **Tài liệu yêu cầu**: Khi Hole đến gần BlockCell → kiểm tra 5 điều kiện → thu thập khối → cập nhật capacity
- **Hiện trạng**: `TryCheckDistanceRealTime()` chỉ kiểm tra khoảng cách và log, **KHÔNG thực sự thu thập khối**
  ```csharp
  // Hiện tại chỉ log:
  Debug.Log($"[Match Triggered] Hole reached Cell {NextAccessibleCellIdx}...");
  NextAccessibleCellIdx++;
  // → Không gọi CollectBlocks, không kiểm tra màu, không trừ capacity
  ```
- **Việc cần làm**: Thêm vào `TryCheckDistanceRealTime()`:
  1. Kiểm tra cùng màu (`TopHoleType == CurHoleType`)
  2. Kiểm tra `CurCapacity > 0`
  3. Kiểm tra `AccessType == Accessible`
  4. Kiểm tra không bị chướng ngại vật
  5. Gọi `BlockCell.CollectBlocks()`
  6. Trừ `CurCapacity`

### ❌ E.2 — `CurHoleType` / Quản lý màu sắc Hole

- **Tài liệu yêu cầu**: `CurHoleType` dùng để so khớp màu với khối
- **Hiện trạng**: Chỉ có `_ColorID` (int) — không có enum, không có logic so khớp
- **Việc cần làm**: Chuyển sang dùng `HoleType` enum hoặc kết nối `_ColorID` với hệ thống so khớp

### ❌ E.3 — `CurCapacity` & Quản lý sức chứa

- **Tài liệu yêu cầu**: `CurCapacity` giảm mỗi khi thu thập, khi = 0 → Hole đầy → chuỗi biến mất
- **Hiện trạng**: Có field `_holeCapacity` nhưng **không bao giờ thay đổi** trong runtime
- **Việc cần làm**: Implement logic trừ capacity mỗi lần collect + trigger vanish khi hết

### ❌ E.4 — Chuỗi biến mất khi Hole đầy (Vanish Sequence)

- **Tài liệu yêu cầu**: Hoạt ảnh biến mất (scale down + fade + particle burst)
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Implement animation sequence khi `CurCapacity == 0`

### ❌ E.5 — `HoleBlockCollectionEfx` (Hiệu ứng bounce/squish)

- **Tài liệu yêu cầu**: Hole co giãn bounce/squish khi thu thập mỗi khối
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Thêm `LeanTween.scale()` punch animation khi collect

### ❌ E.6 — Fill Indicator UI

- **Tài liệu yêu cầu**: Hiển thị mức đầy của Hole, cập nhật mỗi khi thu thập
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Tạo UI component hiển thị fill level

---

## PHẦN F — FILE CẦN TẠO MỚI

### ❌ F.1 — `ObstacleBlockSpawner.cs`

- **Tài liệu yêu cầu**: Component quản lý Strength của Spawner, hiển thị số, xử lý logic bắn
- **Hiện trạng**: **File không tồn tại**
- **Việc cần làm**: Tạo mới với:
  - `Strength` field (int)
  - `UpdateSpawnerStrength(int newStrength)` — cập nhật TextMeshPro hiển thị
  - `SpawnBlockSpawnerShootEfx()` — particle effect khi bắn
  - Logic deactivate khi Strength = 0

### ❌ F.2 — `ObstacleBlock.cs`

- **Tài liệu yêu cầu**: Chướng ngại vật khóa cell, ngăn Hole thu thập
- **Hiện trạng**: **File không tồn tại**
- **Việc cần làm**: Tạo component chướng ngại vật với logic khóa/mở BlockCell

### ❌ F.3 — `SpawnerIndicator.cs`

- **Tài liệu yêu cầu**: Hiển thị preview màu khối sắp sinh ra trên Spawner
- **Hiện trạng**: **File không tồn tại**
- **Việc cần làm**: Tạo component hiển thị indicator với cập nhật màu tự động

### ❌ F.4 — `BlockCellProxy.cs` (Editor Tool)

- **Tài liệu yêu cầu**: Data proxy trong editor, config `SpawnerDirectionAngleZ`, `ChildCellProxies`
- **Hiện trạng**: **File không tồn tại**
- **Việc cần làm**: Tạo editor tool để thiết kế màn chơi trực quan

---

## PHẦN G — HIỆU ỨNG & ÂM THANH

### ❌ G.1 — Particle VFX khi khối bay đến Hole

- **Tài liệu yêu cầu**: Phát particle effect mỗi khi khối bay đến Hole
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Tạo ParticleSystem prefab + trigger mỗi khi collect thành công

### ❌ G.2 — Sound Effect thu thập khối

- **Tài liệu yêu cầu**: Phát sound effect khi thu thập
- **Hiện trạng**: `SoundController.cs` tồn tại nhưng không có sound cho block collection
- **Việc cần làm**: Thêm AudioClip + play logic vào `SoundController`

### ❌ G.3 — `SpawnBlockSpawnerShootEfx()` (VFX bắn khối)

- **Tài liệu yêu cầu**: Hiệu ứng particle khi Spawner bắn khối
- **Hiện trạng**: Không tồn tại
- **Việc cần làm**: Tạo particle effect tại vị trí Spawner

### ❌ G.4 — Bezier Curve Animation cho block collection

- **Tài liệu yêu cầu**: Khối bay theo đường Bezier curve đến Hole
- **Hiện trạng**: Không có logic collection animation nào
- **Việc cần làm**: Implement đường cong Bezier cho animation khối bay

---

## 📊 BẢNG TỔNG HỢP THEO ĐỘ ƯU TIÊN

### 🔴 Ưu tiên CAO — Core Gameplay (Không có thì game không chơi được)

| # | Feature | File | Trạng thái |
|:---:|---|---|:---:|
| C.1 | Thuộc tính cơ bản BlockCell (CellType, CurBlocks, TopBlock...) | `BlockCell.cs` | ❌ |
| C.2 | Quan hệ Parent-Child BlockCells | `BlockCell.cs` | ❌ |
| C.4 | `CollectBlocks()` — Thu thập khối | `BlockCell.cs` | ❌ |
| C.5 | `UpdateTopBlock()` — Cập nhật khối đỉnh | `BlockCell.cs` | ❌ |
| C.3 | `GetMatchingBlocksFromTop()` — Đếm khối cùng màu | `BlockCell.cs` | ❌ |
| C.6 | `TryPullBlocksFromParent()` — Dòng chảy khối | `BlockCell.cs` | ❌ |
| E.1 | Logic thu thập khối trong Hole | `Hole.cs` | ⚠️ |
| E.3 | Quản lý CurCapacity | `Hole.cs` | ❌ |
| A.1 | Enum `BlockCellAccessType` | Tạo mới | ❌ |
| D.1 | `CalcAccessibleBlocks()` | `BlockCellController.cs` | ❌ |
| D.2 | `RecalculateAccessibility()` (DFS) | `BlockCellController.cs` | ❌ |
| B.1 | Dữ liệu cơ bản Block (màu, trạng thái) | `Block.cs` | ❌ |
| B.2 | `SetAsTop()` / `SetAsNotTop()` | `Block.cs` | ❌ |

### 🟡 Ưu tiên TRUNG BÌNH — Spawner System

| # | Feature | File | Trạng thái |
|:---:|---|---|:---:|
| C.7 | `TrySpawnBlocks()` | `BlockCell.cs` | ❌ |
| C.8 | Spawner fields (direction, delay) | `BlockCell.cs` | ❌ |
| F.1 | `ObstacleBlockSpawner.cs` | Tạo mới | ❌ |
| F.2 | `ObstacleBlock.cs` | Tạo mới | ❌ |
| B.3 | `JumpToNewPos()` | `Block.cs` | ❌ |
| D.3 | Quản lý cấu trúc cây cells | `BlockCellController.cs` | ❌ |

### 🟢 Ưu tiên THẤP — Polish & Effects

| # | Feature | File | Trạng thái |
|:---:|---|---|:---:|
| E.4 | Hole vanish animation | `Hole.cs` | ❌ |
| E.5 | `HoleBlockCollectionEfx` (bounce/squish) | `Hole.cs` | ❌ |
| E.6 | Fill Indicator UI | `Hole.cs` + UI | ❌ |
| G.1 | Particle VFX thu thập | VFX mới | ❌ |
| G.2 | Sound effect thu thập | `SoundController.cs` | ❌ |
| G.3 | Spawner shoot VFX | VFX mới | ❌ |
| G.4 | Bezier curve animation | `BlockCell.cs` | ❌ |
| F.3 | `SpawnerIndicator.cs` | Tạo mới | ❌ |
| F.4 | `BlockCellProxy.cs` (Editor) | Tạo mới | ❌ |
| E.2 | HoleType enum / màu sắc | `Hole.cs` | ❌ |
| A.2 | HoleType enum hoàn chỉnh | Tạo mới/cập nhật | ❌ |
| B.4 | `SetPathDistForCollect()` | `Block.cs` | ❌ |
| B.5 | Visual State Management | `Block.cs` | ❌ |

---

## ✅ NHỮNG GÌ ĐÃ CÓ (Không cần làm lại)

| Feature | File | Chi tiết |
|---|---|---|
| Enum `BlockCellType` (4 giá trị: Empty, BlockSpawner, BlockSimple, BlockAccessible) | `BlockCellData.cs` | ✅ Đủ, có thêm `Empty` so với tài liệu |
| `BlockCellData` class (dữ liệu config) | `BlockCellData.cs` | ✅ Có đủ: CellPos, CellRot, CellScale, BlockColor, PendingBlockDataArr, PathDistForCollect, SpawnerDirectionAngleZ |
| `PendingBlockData` (BlockCol, StackCt) | `PendingBlockData.cs` | ✅ Đủ |
| `HoleData` (_ColorID, _holdState, _holdType, _holeCapacity) | `HoleData.cs` | ✅ Đủ |
| `PathDistForCollect` field trên BlockCell | `BlockCell.cs` | ✅ Có |
| `InitializeCellDistances()` (tính PathDist từ Spline) | `BlockCellController.cs` | ✅ Có |
| `AllBlockCells` list + sort by distance | `BlockCellController.cs` | ✅ Có |
| Hole Bezier movement (BezierWalker) | `Hole.cs` | ✅ Có |
| Hole khoảng cách real-time check | `Hole.cs` | ⚠️ Có nhưng chưa trigger collect |
| Hole → Spot system (FlyToEmptySpot) | `Hole.cs` + `Spot.cs` | ✅ Có |
| `GetCurPathTravelDist()` | `Hole.cs` | ✅ Có |

---

## 📐 THỨ TỰ TRIỂN KHAI ĐỀ XUẤT

```
Giai đoạn 1 — Nền tảng dữ liệu
├── A.1  Tạo enum BlockCellAccessType
├── A.2  Hoàn thiện HoleType enum
├── B.1  Mở rộng Block.cs (màu sắc, trạng thái)
└── C.1  Mở rộng BlockCell.cs (thuộc tính cơ bản)

Giai đoạn 2 — Cấu trúc cây & Accessibility
├── C.2  Quan hệ Parent-Child
├── D.3  Xây dựng cấu trúc cây trong Controller
├── D.1  CalcAccessibleBlocks()
└── D.2  RecalculateAccessibility() (DFS)

Giai đoạn 3 — Core Collection Loop
├── B.2  SetAsTop() / SetAsNotTop()
├── C.3  GetMatchingBlocksFromTop()
├── C.4  CollectBlocks() + Animation
├── C.5  UpdateTopBlock()
├── E.1  Hoàn thiện logic thu thập trong Hole
└── E.3  CurCapacity management

Giai đoạn 4 — Dòng chảy & Spawner
├── C.6  TryPullBlocksFromParent()
├── C.7  TrySpawnBlocks()
├── B.3  JumpToNewPos()
├── F.1  ObstacleBlockSpawner.cs
└── F.2  ObstacleBlock.cs

Giai đoạn 5 — Polish & Effects
├── E.4  Hole vanish animation
├── E.5  HoleBlockCollectionEfx
├── G.1-G.4  VFX & Sound effects
├── E.6  Fill Indicator UI
├── F.3  SpawnerIndicator.cs
└── F.4  BlockCellProxy.cs (Editor)
```
