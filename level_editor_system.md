# Hệ Thống Level Editor - Hướng Dẫn Chi Tiết Tạo và Lưu Level

Tài liệu này tóm tắt toàn bộ cấu trúc, luồng hoạt động, các lớp (classes) và cấu trúc dữ liệu liên quan đến hệ thống **Level Editor** trong dự án game.

---

## 1. Tổng Quan Kiến Trúc (Architecture Overview)

Hệ thống Level Editor trong game cho phép thiết kế các màn chơi trực quan trong Unity Editor, hỗ trợ hai phương thức:
1. **Thiết kế thủ công (Manual Design):** Sử dụng các cọ vẽ (brushes) và thanh công cụ trong Unity Scene View thông qua lớp [LevelLayout](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayout.cs).
2. **Tạo từ hình ảnh (Image-based Generation):** Sử dụng hình ảnh 2D (`Texture2D`), ánh xạ màu sắc điểm ảnh (pixels) thành các loại BlockCell tương ứng qua lớp [LevelLayoutGen](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutGen.cs).

Sau khi thiết kế xong, dữ liệu được tuần tự hóa (serialization) và lưu trữ dưới dạng một **ScriptableObject** có tên [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs). Khi vào màn chơi, [InGameManager](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/InGameManager.cs) kết hợp với các Manager con sẽ đọc [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs) để tái cấu trúc màn chơi trên bàn cờ.

---

## 2. Các Thành Phần Chính & Lớp Code (Main Components & Classes)

### A. Công cụ Thiết kế trong Editor
*   **[LevelLayout](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayout.cs) (MonoBehaviour):** Bộ điều khiển chính của editor trong Scene view. Chứa các trạng thái cọ vẽ đang hoạt động, cấu hình màu sắc, số lượng xếp chồng (stack count), các thuộc tính thiết lập Hole/Pipe, và tham chiếu đến mảng dữ liệu Hole/Pipe.
*   **[LevelLayoutGen](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutGen.cs) (MonoBehaviour):** Trình tạo level từ ảnh 2D. Đọc pixel từ `SourceImage` và ánh xạ dựa trên mảng `BlockTypeColors` (có độ lệch màu `ColorTolerance`) để sinh ra các BlockCell.
*   **[LevelLayoutSpawner](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutSpawner.cs) (MonoBehaviour):** Hỗ trợ sinh nhanh một lưới (Grid) ô BlockCell ban đầu với kích thước cột/dòng (`Width` x `Height`), kích thước ô (`CellSize`) và số lượng xếp chồng mặc định (`StackCt`).
*   **[LevelLayoutColUtils](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutColUtils.cs) (Static Class):** Tiện ích ánh xạ loại block (block type ID) sang màu hiển thị (`Color`) tương ứng trong editor.
*   **[LevelLayoutConstants](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutConstants.cs) (Static Class):** Định nghĩa các hằng số giao diện cho Custom Inspector (kích thước nút cọ vẽ, khoảng cách, padding).
*   **[LevelLayoutGizmoUtils](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutGizmoUtils.cs) (Static Class):** Vẽ các hình đồ họa phụ trợ (Gizmos) trong Scene View như: khung giới hạn ô block (`DrawBounds`), mũi tên liên kết giữa các ô (`DrawArrow`, `DrawArrowsToCellProxies`) và chỉ hướng của Spawner.

### B. Cấu Trúc Tuần Tự Hóa (Serialization / Data Structures)
*   **[LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs) (ScriptableObject):** File lưu trữ dữ liệu của mỗi level dưới dạng asset (`Level_000.asset`). Chứa tất cả mảng dữ liệu về BlockCells, Props, Obstacles, Path, Holes, Pipes và thông tin phân tích/độ khó.
*   **[BlockCellProxy](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellProxy.cs) (MonoBehaviour):** Ô đại diện trong Scene hierarchy lúc thiết kế. Chứa thông tin vị trí, góc xoay, loại block (`BlockCellType`), màu sắc, các block đang chờ (`PendingBlockDataArr`), và danh sách các ô con liên kết (`ChildCellProxies`).
*   **[BlockCellData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellData.cs) (Serializable Class):** Phiên bản đã được chuyển đổi của [BlockCellProxy](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellProxy.cs) để lưu vào file `.asset`. Thay thế tham chiếu trực tiếp `BlockCellProxy` bằng mảng chỉ số (`ChildCellIndexes`).

---

## 3. Chi Tiết File Cấu Trúc Dữ Liệu `LevelData`

Mỗi file màn chơi [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs) lưu trữ các dữ liệu chi tiết sau:

### Thông tin cơ bản & Phân tích (Analytics & Meta)
*   `CurGameDifficulty` (`GameDifficulty`): Độ khó của màn chơi (Easy, Medium, Hard, VeryHard, Insane).
*   `SkipLevelLoop` (`bool`): Nếu `true`, màn chơi chỉ chơi 1 lần duy nhất (không lặp lại sau khi hết danh sách level).
*   `HoleCapacity` (`int`): Sức chứa cơ bản của Hole trong level này.
*   Các biến số thống kê tỷ lệ thắng/thua, số lượt tap trung bình (`WinRate`, `TapsPerHole`, `TotalPlayCount`, v.v.).

### Dữ liệu bàn cờ & Ô Block
*   `AllBlockCellDataArr` ([BlockCellData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellData.cs)): Lưu thông tin của mọi ô BlockCell (loại ô, màu sắc, số tầng stack, liên kết ô con).
*   `AllGameBoardPropDataArr` ([GameBoardPropData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/GameBoardPropData.cs)): Lưu vị trí và prefab các vật thể trang trí trên bàn cờ.

### Dữ liệu Chướng ngại vật (Obstacles)
*   `AllBlockObstacleDataArr` ([BlockObstacleData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockObstacleData.cs)): Đá viên (Ice), rào chắn (Barricade), Rắn (Snake), Hộp xích khóa (Chain Lock), Thùng gỗ (Crate).
*   `AllBlockObstacleSpawnerDataArr` ([BlockObstacleSpawnerData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockObstacleSpawnerData.cs)): Các chướng ngại vật sinh thêm block.
*   `AllBlockObstacleMysteryDataArr` ([BlockObstacleMysteryData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockObstacleMysteryData.cs)): Ô bí ẩn.
*   `AllBlockObstacleVoxelTrayDataArr` ([BlockObstacleVoxelTrayData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockObstacleVoxelTrayData.cs)): Khay Voxel chứa các khối mô hình voxel.

### Dữ liệu Đường đi & Hố
*   `CurPathData` ([PathData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/PathData.cs)): Đường đi Bezier (danh sách điểm `Points`, trạng thái đóng/mở `IsClosed`, normals góc quay).
*   `CurPathExitData` ([PathExitData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/PathExitData.cs)): Điểm thoát của đường đi (Prefab, vị trí, góc xoay).
*   `HolesGridSize` (`Vector2Int`): Kích thước lưới phân bố hố (Holes).
*   `AllHolesDataArr` ([HoleData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/HoleData.cs)): Danh sách hố (Loại hố, chướng ngại vật trên hố như Mystery/Ice/Lock/Jelly/Lid, sức chứa ghi đè).
*   `AllPipeDataArr` ([PipeData[]](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/PipeData.cs)): Danh sách ống thả hố (Obstacle type, độ bền sức mạnh chướng ngại vật).

---

## 4. Các Loại Cọ Vẽ & Công Cụ Biên Tập (Editor Brushes)

Khi biên tập level bằng [LevelLayout](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayout.cs) trong Editor, bạn sử dụng các loại Cọ vẽ (`BlockBoardBrushType`) sau:

1.  **Select (`0`):** Chọn và điều chỉnh riêng lẻ từng ô BlockCell trong Scene.
2.  **BlockPainter (`1`):** Vẽ ô block lên bàn cờ. Hỗ trợ 3 phân loại ô (`BlockPainterType`):
    *   `BlockSpawner` (`0`): Ô sinh block liên tục theo một hướng xác định (`SpawnerDirectionAngleZ`).
    *   `BlockSimple` (`1`): Ô block thông thường tĩnh.
    *   `BlockAccessible` (`2`): Ô block có thể tiếp xúc trực tiếp ngay từ đầu.
3.  **BlockCol (`2`):** Tô màu nhanh cho các ô block dựa trên màu được chọn (`ActiveBlockCol`).
4.  **BlockStackCt (`3`):** Thiết lập số lượng block xếp chồng của ô (`ActiveBlockStackCt`).
5.  **LinkPainter (`4`):** Vẽ đường liên kết (nối) các ô [BlockCellProxy](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellProxy.cs) lại với nhau. Khi Hole nuốt ô cha, các ô con liên kết sẽ được kéo theo hoặc kích hoạt.
6.  **UnlinkPainter (`5`):** Xóa liên kết giữa các ô.

Đối với **Hố (Holes)**, sử dụng cọ vẽ hố (`HolePaintBrushType`):
*   `Reset`: Xóa dữ liệu hố về mặc định.
*   `Paint`: Đặt loại vật phẩm thu thập của hố (`HoleCollectItem`).
*   `Select`: Chọn hố để chỉnh sửa.
*   `Obstacle`: Tô chướng ngại vật lên hố (`HoleObstacleType`: Jelly, Ice, Lock, Hammer, v.v.).
*   `Capacity`: Đặt sức chứa cụ thể cho từng hố.

---

## 5. Quy Trình Tạo và Lưu Level (Level Design Pipeline)

### Bước 1: Khởi tạo Level Asset
Người thiết kế tạo một file asset mới thông qua menu chuột phải trong Unity:
`Create -> Game -> New LevelV2`. File này chứa ScriptableObject [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs).

### Bước 2: Thiết kế Bàn cờ trong Scene
1.  Kéo component [LevelLayout](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayout.cs) vào một Scene thiết kế tạm thời.
2.  Dùng [LevelLayoutSpawner](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutSpawner.cs) để sinh nhanh lưới các ô, hoặc dùng [LevelLayoutGen](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayoutGen.cs) quét ảnh mẫu để sinh bố cục tự động.
3.  Sử dụng Custom Inspector của [LevelLayout](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelLayout.cs) để:
    *   Sắp đặt chướng ngại vật, trang trí (`Props`).
    *   Vẽ đường đi (`PathCreator` / `PathData`).
    *   Tô màu, đặt độ cao block (stack count).
    *   Thiết lập liên kết cha-con giữa các ô block.
    *   Cấu hình lưới Hố (`Holes`) và Ống thả (`Pipes`) tương ứng.

### Bước 3: Tuần tự hóa và Lưu (Save / Serialization)
Khi người thiết kế nhấn nút **Save / Apply** trong Editor GUI:
1.  Hệ thống quét toàn bộ [BlockCellProxy](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellProxy.cs) đang có trong Scene.
2.  Mỗi [BlockCellProxy](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellProxy.cs) được chuyển đổi thành một struct [BlockCellData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/BlockCellData.cs) (chuyển đổi danh sách liên kết `ChildCellProxies` thành mảng chỉ số `ChildCellIndexes`).
3.  Thông tin đường đi (`PathData`), ống dẫn (`PipeData`), hố (`HoleData`) và chướng ngại vật được gom lại.
4.  Tất cả dữ liệu được ghi đè lên file ScriptableObject [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs) tương ứng.

---

## 6. Quy Trình Khởi Động Level lúc Chạy Game (Level Load & Setup Flow)

Khi người chơi chọn màn hoặc nhấn nút chơi tiếp, luồng khởi tạo diễn ra như sau:

```
GameStateManager.OnLvlSelectPlayBtnPressed()
  │
  └──> GameStateManager.GotoInGame()
        │
        └──> InGameManager.StartNewGame()
              │
              ├── 1. GameBoardManager.SetUpForNewGame()
              │      - Xác định level cần tải từ tiến trình của người chơi.
              │      - Lấy ra file ScriptableObject LevelData tương ứng từ bộ Funnel.
              │
              ├── 2. VisualsSwapManager.SetUpForLevel()
              │      - Tạo và gán Material, màu sắc theo chủ đề của level.
              │
              ├── 3. BlockCellManager.SetUp(lvlData)
              │      - Sinh (spawn) các ô Block thực tế từ AllBlockCellDataArr.
              │
              ├── 4. GameBoardPropsManager.SetUp(lvlData)
              │      - Sinh các vật thể trang trí (Prop).
              │
              ├── 5. ModularMeshManager.SetUp(lvlData)
              │      - Sinh lưới kiến trúc 9-slice, 3-slice, mesh cong.
              │
              ├── 6. ObstacleBlockManager.SetUp(lvlData)
              │      - Sinh các chướng ngại vật (Ice, Barricade, Locks, Snake).
              │
              ├── 7. PathVisualsManager.SetUp(lvlData)
              │      - Áp dụng PathData để dựng đường Bezier thực tế và điểm thoát PathExit.
              │
              ├── 8. PipeManager.SetUp(lvlData) / HolesManager.SetUp(lvlData)
              │      - Dựng hệ thống ống dẫn và nạp dữ liệu Hố (sức chứa, chướng ngại vật hố).
              │
              └── 9. Chuyển trạng thái sang InGameState.InGame để bắt đầu chơi.
```

---

## 7. Hệ Thống Lặp Level (Level Looping System)

Để đảm bảo người chơi luôn có màn chơi kể cả khi đã vượt qua hết các màn thiết kế sẵn, [GameBoardManager](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/GameBoardManager.cs) duy trì cơ chế lặp:
*   Mảng level được chia làm 2 danh sách dựa trên cờ `SkipLevelLoop` trong [LevelData](file:///d:/Loc/Other/Ducjd/intotheHole/ExportedProject/Assets/Scripts/Assembly-CSharp/LevelData.cs):
    *   **OneTime Level:** Các màn chơi cốt truyện độc đáo, chỉ chơi một lần (`SkipLevelLoop = true`).
    *   **Looping Level:** Các màn chơi phổ thông, dùng để lặp lại xoay vòng (`SkipLevelLoop = false`).
*   Khi chỉ số level vượt quá tổng số lượng màn chơi độc đáo thiết kế sẵn, game sẽ tự động dùng phép chia lấy dư (modulo) để chọn và lặp lại các màn chơi trong danh sách **Looping Level**.
