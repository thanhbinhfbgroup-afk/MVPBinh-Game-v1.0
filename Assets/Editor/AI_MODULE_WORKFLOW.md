# AI Module Workflow - BillGameCore

File này dành cho AI/agent được giao sửa code theo từng module. Nếu chỉ dùng AI hội thoại để hỏi đáp, dùng `Assets/Editor/AI_CONVERSATION_GUIDE.md`.

## 1. Read Order

AI/agent sửa code phải đọc theo thứ tự:

1. `Assets/Editor/CONTEXT.v2.md`
2. `Assets/Editor/AI_MODULE_WORKFLOW.md`
3. Folder module được giao.
4. SharedPorts/Core/Scenes liên quan nếu task yêu cầu.

Nếu không đọc được file bắt buộc, phải dừng và báo rõ.

## 2. Working Contract

Mỗi task phải có scope rõ:
- Module/slice được giao.
- File/folder được phép sửa.
- File/folder không được chạm.
- Có được sửa `SharedPorts` không.
- Có được sửa `Scenes` integration không.
- Có được sửa asmdef không.

Nếu task không nói rõ được sửa `Core`, `SharedPorts`, `Composition`, `Scenes` hoặc asmdef, mặc định không sửa các vùng đó.

## 3. Ownership Rules

### Input worker

Được sửa:
- `Assets/_Game/Scripts/Modules/Input/**`
- `Assets/_Game/Scripts/SharedPorts/Input/**` nếu task cho phép.

Không tự sửa:
- `Scenes/**` trừ khi task là input integration.
- `Player/**` trừ khi task cho phép.
- `.inputactions` asset trừ khi task nói rõ.

Input rule:
- Không dùng `PlayerInput` component.
- Không bật Generate C# wrapper.
- Runtime đọc input qua `InputActionGateway`.
- `CommandBuffer.Enqueue()` chỉ từ `InputReader`.

### Player worker

Được sửa:
- `Assets/_Game/Scripts/Modules/Player/**`

Không tự sửa:
- `Modules/Input/**`
- `Scenes/**` trừ khi task là player integration.
- `Core/**` hoặc `SharedPorts/**` nếu chưa được cho phép.

Player rule:
- `PlayerApplication` không dùng UnityEngine.
- `PlayerPresenter` đọc input qua `IInputCommandSource`.
- Không biết concrete input command classes.
- Death callback đi ra scene layer qua `OnDiedCallback`.

### Inventory worker

Được sửa:
- `Assets/_Game/Scripts/Modules/Inventory/**`
- `Assets/_Game/Scripts/SharedPorts/Inventory/**` nếu task cho phép.

Inventory rule:
- Module giữ tên `Inventory`.
- `InventoryService` project-scope.
- `InventoryState` không vào DI.
- Cross-module access qua `IInventoryReadService` hoặc `IInventoryWriteService`.

### Enemy worker

Được sửa:
- `Assets/_Game/Scripts/Modules/Enemy/**`

Không tự sửa:
- `Scenes/**` trừ khi task là enemy integration/spawn.
- `Player/**`.

Enemy rule:
- Enemy là module optional/draft integration cho tới khi có spawn/wave contract.
- `EnemyApplication` không dùng UnityEngine.
- Enemy không implement `IPlayerReadService`.
- Không tự spawn enemy trong `SceneBootstrapper` nếu chưa có task integration.

### InteractionGroup / Chest worker

Được sửa:
- `Assets/_Game/Scripts/Modules/InteractionGroup/Chest/**`

Chest rule:
- `ChestBinder` implement `IInteractable`.
- Player chỉ biết `IInteractable`.
- Nếu Chest cần DI về sau, prefab phải instantiate qua container.

### Scenes integration worker

Được sửa:
- `Assets/_Game/Scripts/Scenes/**`
- Scene asset/prefab liên quan nếu task cho phép.

Scenes rule:
- Scene orchestration nằm trong `Scenes`.
- `Composition` không reference `Scenes`.
- `SceneBootstrapper` chỉ wiring startup, không chứa business logic.
- `SceneController` là mediator cho scene-level events.

### Composition worker

Được sửa:
- `Assets/_Game/Scripts/Composition/**`

Composition rule:
- Chỉ project-scope services.
- Không biết scene components.
- Không reference `Scenes`.
- Không spawn player/enemy.

## 4. Forbidden Changes

AI/agent không được:
- Dùng `git reset --hard`, revert hoặc xóa file ngoài scope.
- Đổi tên module/folder đã duyệt nếu task không yêu cầu.
- Thêm asmdef reference tùy tiện.
- Tạo vòng dependency.
- Đưa UnityEngine vào `Core`.
- Đưa `Modules.*` reference vào `SharedPorts`.
- Register per-entity runtime object vào DI.
- Thêm Singleton/Manager/Service Locator gameplay.
- Dùng `Find()`/`FindObjectOfType()`.
- Dùng `PlayerInput` component hoặc Generate C# wrapper.
- Sửa `CONTEXT.v2.md` nếu task không yêu cầu cập nhật tài liệu.

## 5. Integration Rules

Module worker không tự lắp module vào scene.

Integration phải là task riêng khi cần sửa:
- `BootstrapSceneLifetimeScope`
- `SceneBootstrapper`
- `SceneController`
- scene asset
- prefab references
- asmdef references giữa scene và module

Khi thêm module mới:
- Tạo module code theo scope được giao.
- Chỉ thêm `SharedPorts` nếu thật sự có cross-module contract.
- Chỉ thêm scene registration khi task integration cho phép.
- Cập nhật `CONTEXT.v2.md` nếu module trở thành baseline.

## 6. Output Contract

Khi trả kết quả, AI/agent phải ghi:

```text
Changed files:
- ...

Architecture impact:
- ...

Asmdef changes:
- ...

Scene/prefab/inspector steps:
- ...

Verification:
- ...

Risks / follow-up:
- ...
```

Nếu không chạy được Unity compile/test, phải nói rõ lý do.

## 7. Prompt Template For Module AI

Bạn có thể copy mẫu này khi giao việc cho AI/agent:

```text
Bạn là AI worker cho Unity project BillGameCore.
Phải đọc và tuân thủ:
- Assets/Editor/CONTEXT.v2.md
- Assets/Editor/AI_MODULE_WORKFLOW.md

Task scope:
- Module/slice:
- Được sửa:
- Không được sửa:
- Có được sửa SharedPorts không:
- Có được sửa Scenes integration không:
- Có được sửa asmdef không:

Yêu cầu:
...

Trước khi sửa, hãy đọc các file liên quan và nêu ngắn gọn kế hoạch.
Sau khi sửa, báo changed files, architecture impact, asmdef changes, scene/prefab steps, verification, risks.
```

## 8. Baseline Tool

`Assets/Editor/BillGameCoreInitTool.v2.cs` chỉ dùng để:
- Validate baseline.
- Tạo cây thư mục rỗng còn thiếu.
- Repair asmdef baseline còn thiếu.
- Mở context.

Tool này không còn sinh gameplay script generic. Khi cần scaffold module mới, tạo task riêng với spec rõ.
