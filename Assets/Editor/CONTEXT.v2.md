# CONTEXT v2.2 — BillGameCore

**Tài liệu kiến trúc bắt buộc cho dự án BillGameCore.**
Mọi AI hoặc người sửa code phải đọc và tuân thủ tài liệu này trước khi thay đổi script.

**Tài liệu phụ trợ cho AI:**
- AI hội thoại/tư vấn đọc thêm `Assets/Editor/AI_CONVERSATION_GUIDE.md`.
- AI/agent sửa code theo module đọc thêm `Assets/Editor/AI_MODULE_WORKFLOW.md`.

**Nguyên tắc vận hành:** không tự suy diễn pattern, không đổi convention ngầm, không thêm abstraction khi chưa có nhu cầu thật. Nếu code hiện tại và tài liệu này mâu thuẫn, phải dừng lại, báo rõ file/line liên quan và hỏi người sở hữu dự án trước khi sửa.

---

## 1. Trạng thái chuẩn hiện tại

**Project:** BillGameCore — Unity `6000.4.6f1`, 2D game, VContainer, New Input System. MessagePipe đã có package nhưng chưa phải backbone hiện tại.

**Cách học/thực hành hiện tại:** vẫn giữ khung kiến trúc đầy đủ A-Z bên dưới, nhưng triển khai theo vertical slice để hiểu rõ từng luồng chạy trước khi mở rộng số lượng tính năng. Không hạ cấp kiến trúc thành MVP sơ sài và không xóa định hướng Player/Inventory/Enemy/Scenes khỏi khung.

**Baseline đã duyệt hiện tại:**
- `01_Core`
- `02_SharedPorts`
- `03_Modules/Input`
- `03_Modules/Player`
- `03_Modules/Inventory`
- `03_Modules/Enemy`
- `03_Modules/InteractionGroup/Chest`
- `03_Modules/InteractionGroup/Loot`
- `04_Composition/ProjectLifetimeScope`
- `05_Scenes/BootstrapSceneLifetimeScope`
- `05_Scenes/SceneBootstrapper`
- `05_Scenes/SceneController`

**Ghi chú tích hợp Enemy:** baseline hiện tại dùng **scene-placed enemy objects** qua `EnemyBinder`. `BootstrapSceneLifetimeScope` register `EnemyRuntimeFactory`, còn `SceneBootstrapper` giao factory này cho `SceneController.InitializeEnemyBinders(...)` để mỗi `EnemyBinder` tự nhận một `EnemyRuntime` cho object enemy đã có sẵn trong scene. `EnemySpawner` vẫn tồn tại trong module Enemy nhưng chưa nằm trên startup path hiện tại và chưa có contract vị trí/wave.

**Baseline scene asset hiện tại:** `Assets/_Game/GlobalScenes/00_Bootstrap.unity` có root `ProjectScope` gắn `ProjectLifetimeScope`, child `SceneScope` gắn `BootstrapSceneLifetimeScope`, `Input` chỉ gắn `InputReader`, `SceneController`, `WalletReadSource`, `InventoryReadSource`, `WalletHudView`, `InventoryHudView`, `PlayerDeathHudView`, scene-placed `ChestBinder` và `EnemyBinder`, cùng `PlayerView` prefab ở `Assets/_Game/Prefabs/Player/PlayerView.prefab`, `PlayerConfig` ở `Assets/_Game/Data/Settings/PlayerConfig.asset`, và `LootBinder` prefab ở `Assets/_Game/Prefabs/InteractionGroup/Loot/LootBinder.prefab`. `InputReader` trỏ trực tiếp tới `Assets/Settings/InputSystem_Actions.inputactions` qua field `_actions`; `EventSystem/InputSystemUIInputModule` cũng dùng cùng `InputActionAsset`. Không dùng `PlayerInput` component và không dùng Generate C# wrapper của Unity Input System.

---

## 2. Triết lý kiến trúc

- **Feature-first, không layer-first.** Mỗi module là một vertical slice: `Domain -> Application -> Presentation -> Infrastructure`.
- **Core tối thiểu.** Core chỉ chứa value objects và contracts thật sự dùng chung. Core không có game flow, không có UnityEngine, không có dependency package ngoài BCL.
- **SharedPorts là biên giao tiếp giữa module.** Module này muốn nói chuyện với module khác thì đi qua `SharedPorts` hoặc MessagePipe khi đã unlock.
- **Composition không biết Scenes.** Tuyệt đối không tạo reference `Composition -> Scenes`.
- **Scenes sở hữu orchestration của scene.** Startup scene nằm ở `05_Scenes/SceneBootstrapper`, không nằm ở `04_Composition`.
- **Runtime-per-entity.** `State`, `Application`, `Presenter`, `Runtime` của entity được tạo bởi spawner/binder, không register vào DI container.
- **MonoBehaviour chỉ forward hoặc write Unity output.** Business logic nằm ở Application/Presenter tùy layer, không để trong View.

---

## 3. Asmdef dependency graph

Chiều phụ thuộc hợp lệ:

```text
BillGameCore.Core
  <- BillGameCore.SharedPorts
      <- BillGameCore.Modules.Input
      <- BillGameCore.Modules.Player
      <- BillGameCore.Modules.Inventory
      <- BillGameCore.Modules.InteractionGroup
      <- BillGameCore.Modules.Enemy
      <- BillGameCore.Composition          (chỉ cho project-scope services)
      <- BillGameCore.Scenes               (scene orchestration)
```

Reference hiện tại:

```text
Core
  refs: none
  noEngineReferences: true

SharedPorts
  refs: Core

Modules.Input
  refs: Core, SharedPorts, VContainer, Unity.InputSystem

Modules.Player
  refs: Core, SharedPorts, VContainer

Modules.Inventory
  refs: Core, SharedPorts, VContainer

Modules.InteractionGroup
  refs: Core, SharedPorts, VContainer

Modules.Enemy
  refs: Core, SharedPorts, VContainer

Composition
  refs: Core, SharedPorts, Modules, VContainer
  không được ref Scenes

Scenes
  refs: Core, SharedPorts, Modules, Composition, VContainer
  được phép biết scene components và scene bootstrap
```

Tuyệt đối không được:
- `Core` reference UnityEngine hoặc bất kỳ module/package gameplay nào.
- `SharedPorts` reference bất kỳ `Modules.*`.
- Module A reference namespace nội bộ của Module B.
- `Composition` reference `Scenes`.
- Register entity runtime objects vào DI scope.

---

## 4. Cấu trúc thư mục chuẩn

```text
Assets/_Game/
├── Art/
├── Data/
│   ├── Items/
│   └── Settings/
├── GlobalScenes/
│   └── 00_Bootstrap.unity
└── Scripts/
    ├── 01_Core/
    │   ├── BillGameCore.Core.asmdef
    │   ├── Combat/
    │   ├── Interaction/
    │   ├── Inventory/
    │   ├── Rewards/
    │   ├── Save/
    │   └── ValueObjects/
    │
    ├── 02_SharedPorts/
    │   ├── BillGameCore.SharedPorts.asmdef
    │   ├── Combat/
    │   ├── Economy/
    │   ├── Input/
    │   ├── Inventory/
    │   ├── Messages/
    │   └── Player/
    │
    ├── 03_Modules/
    │   ├── Input/
    │   ├── Player/
    │   ├── Inventory/
    │   ├── InteractionGroup/
    │   └── Enemy/
    │
    ├── 04_Composition/
    │   ├── BillGameCore.Composition.asmdef
    │   └── ProjectLifetimeScope.cs
    │
    └── 05_Scenes/
        ├── BillGameCore.Scenes.asmdef
        ├── BootstrapSceneLifetimeScope.cs
        ├── WalletReadSource.cs
        ├── WalletHudView.cs
        ├── InventoryReadSource.cs
        ├── InventoryHudView.cs
        ├── PlayerDeathHudView.cs
        ├── WalletService.cs
        ├── RewardGrantService.cs
        ├── SceneBootstrapper.cs
        └── SceneController.cs

```

---

## 5. Core — Shared Kernel

Core là BCL-only. `BillGameCore.Core.asmdef` phải luôn có:

```json
"noEngineReferences": true
```

Core hiện gồm:

```text
Core/ValueObjects/BillEntityId.cs
Core/Combat/DamageInfo.cs
Core/Combat/DamageResult.cs
Core/Combat/IDamageReceiver.cs
Core/Interaction/IInteractable.cs
Core/Inventory/ItemStack.cs
Core/Rewards/RewardBundle.cs
```

`Core/Save/` hiện mới là placeholder folder; các save snapshot interfaces chưa materialize trong baseline hiện tại.

Quy tắc Core:
- `BillEntityId.New()` chỉ gọi trong spawner/binder nơi entity instance được tạo.
- `BillEntityId` dùng `Guid`, có `Invalid`, `IsValid`, constructor private, và `ToString()` rút gọn để log/debug dễ đọc.
- `IDamageReceiver.ReceiveDamage()` là contract combat duy nhất cho nhận damage.
- `ReceiveDamage()` implementations phải clamp damage âm về `0` trước khi trừ máu. Heal/buff không đi qua `DamageInfo`.
- `DamageInfo` baseline dùng `float Amount`, `BillEntityId SourceId`, `bool IsCritical`.
- `DamageResult` baseline dùng `float AppliedDamage`, `float RemainingHealth`, `bool JustDied`.
- `IInteractable` được implement bởi binder/presentation object, không bởi Application.
- `RewardBundle` là DTO reward dùng bởi enemy/death/reward flow.
- Không thêm interface vào Core nếu chỉ 1-2 module dùng. Trường hợp đó đặt ở `SharedPorts` hoặc `Application/Ports` nội bộ module.

---

## 6. SharedPorts

SharedPorts là asmdef trung gian cho cross-module contracts. SharedPorts được phép reference `Core`, không được reference `Modules.*`.

### Input contracts

Hiện tại input contracts nằm trong `SharedPorts/Input`, không nằm trong `Modules/Input`.

```text
SharedPorts/Input/ICommand.cs
SharedPorts/Input/CommandType.cs
SharedPorts/Input/IMoveCommand.cs
SharedPorts/Input/IAttackCommand.cs
SharedPorts/Input/IInteractCommand.cs
SharedPorts/Input/IInputCommandSource.cs
SharedPorts/Input/InputContext.cs
```

Lý do: `PlayerPresenter` và các entity presenter chỉ cần biết narrow interfaces từ SharedPorts, không được cast sang concrete command type trong `Modules.Input.Commands`.

Quy tắc:
- Concrete commands nằm trong `Modules/Input/Commands`.
- Concrete commands implement interfaces từ `SharedPorts/Input`.
- Consumer ngoài module Input chỉ dùng `ICommand`, `IMoveCommand`, `IAttackCommand`, `IInteractCommand`, `CommandType`.
- `ICommand` dùng `BillEntityId ControlledEntityId`, không dùng `TargetId` hoặc `SourceId` cho input command.
- `CommandType` phải có `None = 0`, sau đó `Move = 1`, `Attack = 2`, `Interact = 3`, `SwitchContext = 4`. Không đánh số lại sau khi đã có replay/save.
- `IMoveCommand` expose `DirX`, `DirY`, `IsMoving`.
- `IAttackCommand` expose `IsHeld`, `HeldDuration`.
- `IInputCommandSource` expose `TryDequeue(out ICommand command)` và `HasCommands`.

### Inventory contracts

```text
SharedPorts/Inventory/IInventoryReadService.cs
SharedPorts/Inventory/IInventoryWriteService.cs
```

Implemented by:
- `Modules.Inventory.Application.InventoryService`

Consumed by:
- UI, loot, chest/reward interaction khi cần inventory.

Quy ước hiện tại:
- `IInventoryReadService` expose `event Action Changed`.
- `IInventoryReadService.GetItems()` trả snapshot read-only list.
- `IInventoryWriteService` hiện chỉ có `AddItem(ItemStack)` và `RemoveItem(itemId, amount)`.

### Economy contracts

```text
SharedPorts/Economy/IWalletService.cs
SharedPorts/Economy/IRewardGrantService.cs
```

Status hiện tại:
- Contract đã tồn tại.
- `WalletService` và `RewardGrantService` đã tồn tại trong `05_Scenes` như baseline scene-scope implementation, chưa tách thành economy module/project-scope service riêng.
- `IWalletService` là read-only port cho UI/HUD: `Gold`, `Experience`, `Changed`.
- `IRewardGrantService` là cổng grant toàn bộ `RewardBundle`: `void Grant(RewardBundle bundle)`.
- `IRewardGrantService` chỉ được gọi khi reward thực sự được nhận. Với chest thì grant lúc mở chest; với enemy world-loot thì grant lúc loot được nhặt, không grant ngay ở enemy death.
- Implementation mục tiêu nên là `RewardGrantService`, điều phối reward vào đúng hệ thống sở hữu dữ liệu. Không mặc định cho `EconomyService` ôm cả item reward.

### Player read contract

`SharedPorts/Player/` hiện mới là placeholder folder; `IPlayerReadService` chưa materialize trong baseline hiện tại.

### Messages

Status:
- `SharedPorts/Messages/` hiện mới là placeholder folder; các message types như `EnemyDiedMessage` và `ItemPickedUpMessage` chưa materialize trong baseline hiện tại.
- MessagePipe chưa là backbone hiện tại.
- Chỉ dùng MessagePipe từ slice combat/reward/event khi đã đăng ký broker rõ ràng.

---

## 7. Composition

Composition hiện tại chỉ chứa project-level composition. Composition không chứa scene-specific orchestration.

### ProjectLifetimeScope

File:

```text
Composition/ProjectLifetimeScope.cs
```

Hiện register:

```text
InventoryService as implemented interfaces
```

`InventoryService` sống project-scope vì inventory cần persist qua scene.

Baseline scene hiện tại có root `ProjectScope` gắn `ProjectLifetimeScope`, và `SceneScope/BootstrapSceneLifetimeScope` phải trỏ parent scope về `ProjectScope` để resolve được `IInventoryReadService` và `IInventoryWriteService`.

Cho phép register sau này:
- SaveService
- EconomyService
- AudioService

Không được register:
- PlayerState
- PlayerApplication
- PlayerPresenter
- PlayerRuntime
- EnemyState
- EnemyApplication
- EnemyPresenter
- EnemyRuntime
- bất kỳ per-entity runtime object nào

## 8. Scenes

Scenes là nơi chứa scene-level orchestration.

### BootstrapSceneLifetimeScope

File:

```text
Scenes/BootstrapSceneLifetimeScope.cs
```

Nhiệm vụ:
- Register `CommandBuffer`.
- Register `InputCommandDispatcher` as `IInputCommandSource`.
- Register scene component `InputReader`.
- Register scene component `SceneController`.
- Register scene component `WalletReadSource`.
- Register scene component `InventoryReadSource`.
- Register `PlayerView` prefab.
- Register `PlayerConfig`.
- Register `PlayerSpawner`.
- Register `WalletService` as `IWalletService` and `IWalletWriteService`.
- Register `RewardGrantService` as `IRewardGrantService`.
- Register `EnemyRuntimeFactory`.
- Register `LootSpawner`.
- Register `SceneBootstrapper` as entry point.

Các field phải gán trong Inspector:

```text
InputReader
SceneController
WalletReadSource
InventoryReadSource
PlayerView prefab
PlayerConfig
LootBinder prefab
```

Không gán thiếu field bắt buộc. Nếu thiếu `InputReader`, `SceneController`, `WalletReadSource`, `InventoryReadSource`, `PlayerView prefab`, `PlayerConfig` hoặc `LootBinder prefab`, VContainer build hoặc runtime startup có thể fail.

`BootstrapSceneLifetimeScope.Configure()` phải validate các field bắt buộc trước khi register. Nếu thiếu một field bắt buộc, scope phải fail sớm bằng lỗi rõ tên field, không để NullReference mơ hồ ở startup.

`InputReader.ValidateConfiguration()` phải được gọi từ scene scope để xác nhận `InputActionAsset` đã được gán, action map `Player` và `UI`, cùng các action `Player/Move`, `Player/Attack`, `Player/Interact`, `UI/Submit` tồn tại trước khi runtime đọc input.

Scene `Bootstrap.unity` baseline hiện có:

```text
ProjectScope
  -> ProjectLifetimeScope
  -> SceneScope
       -> BootstrapSceneLifetimeScope
Input
  -> InputReader
SceneController
  -> SceneController
WalletReadSource
  -> WalletReadSource
InventoryReadSource
  -> InventoryReadSource
Canvas
  -> WalletHudView
  -> InventoryHudView
  -> PlayerDeathHudView
```

### SceneBootstrapper

File:

```text
Scenes/SceneBootstrapper.cs
```

Nhiệm vụ:
- Spawn Player bằng `PlayerSpawner.Spawn(Vector2.zero)`.
- Gán controlled entity cho `InputReader`.
- Wire `PlayerPresenter.OnDiedCallback` sang `SceneController.HandlePlayerDied`.
- Gán `IWalletService` vào `WalletReadSource`.
- Gán `IInventoryReadService` vào `InventoryReadSource`.
- Gán `IRewardGrantService`, `IInventoryWriteService`, `LootSpawner`, `IInputContextService` vào `SceneController`.
- Gọi `SceneController.InitializeEnemyBinders(EnemyRuntimeFactory)`.
- Dispose `PlayerRuntime` khi scope dispose.

Không được:
- Register service.
- Biết concrete input command.
- Chứa business logic gameplay.

### SceneController

File:

```text
Scenes/SceneController.cs
```

Nhiệm vụ:
- Mediator cho scene-level events.
- `HandlePlayerDied(...)` là handler riêng cho player death.
- `HandleEnemyDied(...)` spawn world loot từ `RewardBundle` và/hoặc `ItemStack`.
- `HandleLootCollected(...)` route reward loot sang `IRewardGrantService` và item loot sang `IInventoryWriteService`.
- `HandleChestOpened(...)` grant chest reward.
- `SetRewardGrantService(...)`, `SetInventoryWriteService(...)`, `SetLootSpawner(...)`, `SetInputContextService(...)` là scene-wiring setters được gọi từ `SceneBootstrapper`.

Không được:
- Dùng `Find()`, `FindObjectOfType()`, scene locator.
- Gọi namespace nội bộ module khác nếu có SharedPorts hoặc callback đã đủ.

---

## 9. Module Input

Asmdef:

```text
BillGameCore.Modules.Input
refs: Core, SharedPorts, VContainer, Unity.InputSystem
```

Files:

```text
03_Modules/Input/Commands/MoveCommand.cs
03_Modules/Input/Commands/AttackCommand.cs
03_Modules/Input/Commands/InteractCommand.cs
03_Modules/Input/Commands/SwitchContextCommand.cs
03_Modules/Input/Commands/CommandBuffer.cs
03_Modules/Input/Application/InputCommandDispatcher.cs
03_Modules/Input/Infrastructure/InputActionGateway.cs
03_Modules/Input/Infrastructure/InputReader.cs
03_Modules/Input/Context/InputContextNames.cs
```

Rules:
- `CommandBuffer.Enqueue()` chỉ được gọi từ `InputReader`.
- `InputCommandDispatcher` là adapter `CommandBuffer -> IInputCommandSource`.
- `InputActionGateway` là wrapper tự viết quanh `InputActionAsset`. Gateway cache action maps/actions cần dùng, bật/tắt map theo `InputContext`, và expose typed reads cho `InputReader`.
- `InputActionGateway` dùng `InputContextNames`, không dùng `PlayerInputContext`, `VehicleInputContext`, `UIInputContext`.
- `InputActionGateway` có `CurrentContext`; khi nhận context không hỗ trợ thì phải throw lỗi rõ, không fallback ngầm về Player.
- `InputContextNames` là nơi duy nhất trong module Input gom tên action map: `Player`, `UI`, `Vehicle`.
- `InputReader` là MonoBehaviour, giữ serialized `InputActionAsset`, dùng `InputActionGateway` để đọc New Input System và dịch raw input thành command object. `InputReader` implement `IInputContextService`.
- `InputReader` nhận `CommandBuffer` qua `[Inject]`.
- `InputReader.SetControlledEntity(BillEntityId)` phải validate `BillEntityId.IsValid`.
- `InputReader.ReadPlayerMap()` enqueue `MoveCommand` mỗi frame, kể cả khi không di chuyển, để consumer có thể set velocity về `0`.
- `InputReader.SwitchContext(...)` phải disable map cũ, set context mới, enable map mới, rồi clear command buffer.
- `InputReader.WasSubmitPressedThisFrame()` chỉ đọc được khi context hiện tại là `UI` và được dùng cho death/restart flow.
- `InputReader._actions` phải được gán rõ trong scene. `InputReader` và `EventSystem/InputSystemUIInputModule` phải dùng cùng một `InputActionAsset`.
- Không gắn `PlayerInput` component vào scene object `Input`.
- Không bật Generate C# wrapper trên `.inputactions`; wrapper chính thức của dự án là `InputActionGateway`.
- Entity presenter không được biết `MoveCommand`, `AttackCommand`, `InteractCommand` concrete types.
- Khi switch context phải clear command buffer.
- `CommandBuffer` có constructor nhận capacity, nên scene scope phải register bằng factory rõ capacity (`new CommandBuffer(size)`), không để VContainer resolve primitive `int`.

Flow:

```text
InputReader.Update()
  -> đọc InputActionGateway trên InputActionAsset đã gán ở scene
  -> enqueue concrete command với ControlledEntityId hợp lệ
InputCommandDispatcher.TryDequeue()
  -> trả ICommand cho consumer
PlayerPresenter
  -> đọc ICommand/IMoveCommand/IAttackCommand/IInteractCommand qua SharedPorts
```

---

## 10. Module Player

Asmdef:

```text
BillGameCore.Modules.Player
refs: Core, SharedPorts, VContainer
```

Files:

```text
Modules/Player/Domain/PlayerDefinition.cs
Modules/Player/Domain/PlayerState.cs
Modules/Player/Application/PlayerApplication.cs
Modules/Player/Infrastructure/Config/PlayerConfig.cs
Modules/Player/Presentation/PlayerAttackSensor.cs
Modules/Player/Presentation/PlayerInteractSensor.cs
Modules/Player/Presentation/PlayerCombatReceiver.cs
Modules/Player/Presentation/PlayerView.cs
Modules/Player/Presentation/PlayerPresenter.cs
Modules/Player/Presentation/PlayerSpawner.cs
Modules/Player/Presentation/PlayerRuntime.cs
```

### Domain

Domain không dùng UnityEngine.

`PlayerDefinition`:
- config data runtime bất biến.
- tạo từ `PlayerConfig.ToDefinition()`.

`PlayerState`:
- runtime mutable state.
- chỉ `PlayerApplication` sở hữu và mutate.
- không register vào DI.
- không lưu trong ScriptableObject.

### Application

`PlayerApplication`:
- implement `IDamageReceiver`.
- không dùng UnityEngine.
- không biết world position.
- `OnDied` chỉ emit `BillEntityId` và `RewardBundle`.

### Presentation

`PlayerView`:
- MonoBehaviour.
- expose `WorldPosition`, `PlayerAttackSensor`, `PlayerInteractSensor`.
- write Rigidbody2D output theo lệnh từ presenter.
- có `SetDeadState()` để disable collider/combat receiver/sensors khi player chết.

`PlayerPresenter`:
- đọc input qua `IInputCommandSource`.
- không biết concrete command classes.
- lấy world position từ `PlayerView.WorldPosition` khi player chết.
- gọi `OnDiedCallback(BillEntityId, RewardBundle, Vector2)` cho scene layer.
- nhận `InteractCommand` thì interact với target hiện tại từ `PlayerInteractSensor.CurrentTarget`.
- forward damage xuống `PlayerApplication.ReceiveDamage(...)`.

`PlayerCombatReceiver`:
- scene-side adapter `MonoBehaviour, IDamageReceiver`.
- chỉ forward `ReceiveDamage()` từ physics/scene boundary vào `PlayerRuntime`.

`PlayerSpawner`:
- có đúng 1 public constructor để VContainer resolve rõ ràng.
- gọi `BillEntityId.New()`.
- tạo `PlayerDefinition`, `PlayerState`, `PlayerApplication`, `PlayerPresenter`, `PlayerRuntime`.
- instantiate `PlayerView` bằng `Object.Instantiate` vì `PlayerView` hiện không có `[Inject]`.

`PlayerRuntime`:
- handle bất biến cho một player instance.
- dispose presenter subscription.
- không register vào DI.

Player death flow:

```text
Combat/Application gọi PlayerApplication.ReceiveDamage()
  -> clamp damage âm về 0
  -> nếu chết: PlayerApplication.OnDied(BillEntityId, RewardBundle)
    -> PlayerPresenter.HandleDied()
      -> lấy PlayerView.WorldPosition
      -> OnDiedCallback(BillEntityId, RewardBundle, Vector2)
        -> SceneController.HandlePlayerDied()
```

---

## 11. Module Inventory

Asmdef:

```text
BillGameCore.Modules.Inventory
refs: Core, SharedPorts, VContainer
```

Files:

```text
Modules/Inventory/Domain/InventoryState.cs
Modules/Inventory/Application/InventoryService.cs
```

`InventoryService`:
- project-scope service.
- owns `InventoryState`.
- implements `IInventoryReadService`.
- implements `IInventoryWriteService`.
- expose `Changed` event qua `IInventoryReadService`.

Rules:
- Inventory module giữ tên `Inventory`, không đổi thành `InventoryGroup`.
- `InventoryState` không vào DI.
- Cross-module access đi qua `IInventoryReadService` hoặc `IInventoryWriteService`.
- `InventoryService` trả snapshot copy từ `GetItems()`, không trả live list nội bộ.
- Save/load inventory chưa materialize trong baseline hiện tại.

Current behavior:
- `GetItems()` trả snapshot read-only copy, không trả live `InventoryState.Items`.
- `HasItem(itemId, minAmount)`
- `AddItem(ItemStack)`
- `RemoveItem(itemId, amount)`
- `Changed` fire khi inventory thay đổi

---

## 12. InteractionGroup / Chest

Asmdef:

```text
BillGameCore.Modules.InteractionGroup
refs: Core, SharedPorts, VContainer
```

Files:

```text
Modules/InteractionGroup/Chest/Domain/ChestDefinition.cs
Modules/InteractionGroup/Chest/Domain/ChestState.cs
Modules/InteractionGroup/Chest/Application/ChestApplication.cs
Modules/InteractionGroup/Chest/Infrastructure/Config/ChestConfig.cs
Modules/InteractionGroup/Chest/Presentation/ChestView.cs
Modules/InteractionGroup/Chest/Presentation/ChestPresenter.cs
Modules/InteractionGroup/Chest/Presentation/ChestBinder.cs
```

Rules:
- `ChestBinder` implements `IInteractable`.
- Player chỉ gọi `GetComponent<IInteractable>()`, không biết `ChestBinder`.
- Binder được phép tự tạo `ChestApplication`, `ChestState`, `ChestPresenter` trong `Awake`.
- Nếu sau này Chest cần `[Inject] IInventoryWriteService`, prefab phải được instantiate bằng `container.Instantiate()`, không dùng `Object.Instantiate()`.

### InteractionGroup / Loot

Files:

```text
Modules/InteractionGroup/Loot/Domain/LootDefinition.cs
Modules/InteractionGroup/Loot/Domain/LootState.cs
Modules/InteractionGroup/Loot/Application/LootApplication.cs
Modules/InteractionGroup/Loot/Application/LootCollectResult.cs
Modules/InteractionGroup/Loot/Presentation/LootView.cs
Modules/InteractionGroup/Loot/Presentation/LootPresenter.cs
Modules/InteractionGroup/Loot/Presentation/LootBinder.cs
Modules/InteractionGroup/Loot/Presentation/LootSpawner.cs
```

Rules:
- `LootBinder` implements `IInteractable`.
- `LootBinder` khởi đầu ở trạng thái inert và chỉ hoạt động sau `Initialize(...)`.
- `LootDefinition` hiện support hai payload path:
  - `RewardBundle`
  - `ItemStack`
- Một `LootBinder` chỉ mang một payload thực tế tại một thời điểm.
- Khi enemy chết có cả reward và item, scene hiện tại spawn **hai loot object** tách nhẹ vị trí, không nhồi cả hai payload vào một binder.
- `LootSpawner` chỉ instantiate prefab và initialize payload; grant reward hoặc add item luôn đi qua `SceneController.HandleLootCollected(...)`.

---

## 13. Module Enemy

Asmdef:

```text
BillGameCore.Modules.Enemy
refs: Core, SharedPorts, VContainer
```

Files:

```text
Modules/Enemy/Domain/EnemyDefinition.cs
Modules/Enemy/Domain/EnemyState.cs
Modules/Enemy/Application/EnemyApplication.cs
Modules/Enemy/Application/EnemyHealthReadModel.cs
Modules/Enemy/Infrastructure/Config/EnemyConfig.cs
Modules/Enemy/Presentation/EnemyAttackSensor.cs
Modules/Enemy/Presentation/EnemyBinder.cs
Modules/Enemy/Presentation/EnemyView.cs
Modules/Enemy/Presentation/EnemyPresenter.cs
Modules/Enemy/Presentation/EnemyRuntimeFactory.cs
Modules/Enemy/Presentation/EnemySpawner.cs
Modules/Enemy/Presentation/EnemyRuntime.cs
```

### Domain

`EnemyDefinition`:
- config data runtime bất biến.
- tạo từ `EnemyConfig.ToDefinition()`.
- chứa health/combat config, reward config (`GoldReward`, `ExperienceReward`), và optional item drop từ `DroppedItemId`, `DroppedItemAmount`.

`EnemyState`:
- runtime mutable state cho một enemy instance.
- chỉ `EnemyApplication` sở hữu và mutate.
- không register vào DI.
- không lưu trong ScriptableObject.

### Application

`EnemyApplication`:
- implement `IDamageReceiver`.
- không dùng UnityEngine.
- không biết world position.
- `OnDied` emit `RewardBundle` và `ItemStack`.
- tạo `RewardBundle` và item drop từ `EnemyDefinition`.

### Presentation

`EnemyView`:
- MonoBehaviour.
- callback chỉ forward sang presenter.
- expose `WorldPosition`.

`EnemyAttackSensor`:
- scene-side trigger sensor để enemy biết `CurrentTarget`.
- chỉ target collider scene hợp lệ, không couple trực tiếp sang player internals ngoài combat boundary.

`EnemyBinder`:
- scene boundary/host cho enemy object đã có sẵn trong scene.
- own serialized `EnemyConfig`, `EnemyView`, attack sensor, target collider.
- nhận `EnemyRuntime` từ scene orchestration qua `InitializeRuntime(...)`.
- không còn tự assemble runtime trên gameplay path chính.

`EnemyPresenter`:
- không đọc player input.
- không biết concrete command classes.
- forward `RewardBundle` và `ItemStack` từ application lên binder/runtime path.

`EnemyRuntimeFactory`:
- assemble `EnemyDefinition`, `EnemyState`, `EnemyApplication`, `EnemyPresenter`, `EnemyRuntime` từ `EnemyConfig`.
- là factory dùng trên runtime path chính của scene-placed enemy baseline hiện tại.

`EnemySpawner`:
- có đúng 1 public constructor để VContainer resolve rõ ràng.
- gọi `BillEntityId.New()`.
- tạo `EnemyDefinition`, `EnemyState`, `EnemyApplication`, `EnemyPresenter`, `EnemyRuntime`.
- instantiate `EnemyView` bằng `Object.Instantiate` vì `EnemyView` hiện không có `[Inject]`.
- hiện vẫn tồn tại trong module nhưng chưa nằm trên startup path của `00_Bootstrap`.

`EnemyRuntime`:
- handle bất biến cho một enemy instance.
- dispose presenter subscription.
- không register vào DI.

Scene integration:
- `Scenes.asmdef` reference `BillGameCore.Modules.Enemy`.
- `SceneBootstrapper` giao `EnemyRuntimeFactory` cho `SceneController.InitializeEnemyBinders(...)`.
- `SceneController.InitializeEnemyBinders(...)` set callback và init runtime cho từng `EnemyBinder` scene-placed.
- `EnemySpawner`/wave/spawn point contract vẫn deferred cho slice sau.

Approved enemy death flow:

```text
EnemyApplication.ReceiveDamage()
  -> clamp damage âm về 0
  -> OnDied(RewardBundle, ItemStack)
    -> EnemyPresenter forward reward + item drop
    -> EnemyBinder lấy EnemyView.WorldPosition
    -> DiedCallback(BillEntityId, RewardBundle, ItemStack, Vector2)
      -> SceneController.HandleEnemyDied()
        -> spawn reward loot nếu có RewardBundle
        -> spawn item loot nếu có ItemStack
LootBinder / LootPresenter
  -> khi player nhặt loot
    -> SceneController.HandleLootCollected(...)
      -> reward loot: IRewardGrantService.Grant(bundle)
      -> item loot: IInventoryWriteService.AddItem(itemStack)
```

---

## 14. MessagePipe policy

MessagePipe chỉ dùng khi có cross-module hoặc cross-scene event thật sự cần publish/subscribe.

Được phép:
- Enemy died event khi Enemy/Loot/Economy đã đủ module.
- Item picked up event cho UI/audio/global feedback.
- Scene transition/game over global event.

Không được:
- Dùng MessagePipe để thay direct call trong cùng module.
- Dùng cho mọi event nhỏ.
- Dùng trước khi broker được register rõ trong Project/Scene scope.
- Dùng để che giấu dependency đáng ra phải là constructor hoặc `[Inject]`.

Hiện tại:
- `SharedPorts/Messages` mới là placeholder folder, chưa có concrete message types trong baseline hiện tại.
- Broker chưa được register làm backbone.

---

## 15. VContainer contract

### Project scope

Project scope hiện tại:

```text
ProjectLifetimeScope
  -> InventoryService as IInventoryReadService
  -> InventoryService as IInventoryWriteService
```

Project scope dùng cho services sống qua scene:
- Inventory
- Save
- Economy
- Audio

Baseline scene hiện tại dùng hierarchy:

```text
ProjectScope
  -> ProjectLifetimeScope
  -> SceneScope
       -> BootstrapSceneLifetimeScope
```

### Scene scope

Scene scope hiện tại:

```text
BootstrapSceneLifetimeScope
  -> CommandBuffer
  -> InputCommandDispatcher as IInputCommandSource
  -> InputReader component
  -> SceneController component
  -> WalletReadSource component
  -> InventoryReadSource component
  -> PlayerView prefab
  -> PlayerConfig
  -> WalletService as IWalletService/IWalletWriteService
  -> RewardGrantService as IRewardGrantService
  -> EnemyRuntimeFactory
  -> LootSpawner
  -> PlayerSpawner
  -> SceneBootstrapper entry point
```

Scene scope dùng cho:
- scene components
- scene startup orchestration
- scene-only factories/spawners
- scene-only services

### Per-entity

Không bao giờ register:

```text
*State
*Application
*Presenter
*Runtime
```

Entity lifetime thuộc spawner/binder.

---

## 16. Approved flows hiện tại

### Scene startup

```text
BootstrapSceneLifetimeScope.Configure()
  -> build scene container
  -> SceneBootstrapper.Start()
    -> PlayerSpawner.Spawn(Vector2.zero)
    -> InputReader.SetControlledEntity(playerRuntime.Id)
    -> WalletReadSource.SetWalletService(...)
    -> InventoryReadSource.SetInventoryReadService(...)
    -> SceneController.SetRewardGrantService(...)
    -> SceneController.SetInventoryWriteService(...)
    -> SceneController.SetLootSpawner(...)
    -> SceneController.InitializeEnemyBinders(EnemyRuntimeFactory)
    -> playerRuntime.Presenter.OnDiedCallback = SceneController.HandlePlayerDied
```

### Player move

```text
InputReader.ReadPlayerMap()
  -> CommandBuffer.Enqueue(MoveCommand)
InputCommandDispatcher.TryDequeue()
  -> PlayerPresenter reads IMoveCommand
  -> PlayerApplication.ComputeMoveVelocity(dirX, dirY, out velocityX, out velocityY)
  -> PlayerView.SetMoveVelocity(...)
```

### Player interact

```text
InputReader
  -> InteractCommand
PlayerInteractSensor.CurrentTarget
  -> PlayerPresenter đọc target hiện tại
PlayerPresenter
  -> khi nhận InteractCommand
  -> CanInteract()
  -> Interact()
```

### Inventory add/remove

```text
Consumer uses IInventoryWriteService
  -> InventoryService.AddItem/RemoveItem
  -> InventoryState changes
  -> Changed event
```

### Enemy death -> loot pickup

```text
EnemyApplication.ReceiveDamage()
  -> nếu chết: DiedCallback(RewardBundle, ItemStack)
EnemyBinder
  -> DiedCallback(BillEntityId, RewardBundle, ItemStack, Vector2)
SceneController.HandleEnemyDied(...)
  -> spawn reward loot nếu có
  -> spawn item loot nếu có
LootBinder.Interact()
  -> LootApplication.TryCollect()
SceneController.HandleLootCollected(...)
  -> reward loot: IRewardGrantService.Grant(...)
  -> item loot: IInventoryWriteService.AddItem(...)
```

### Player death -> restart

```text
PlayerApplication.ReceiveDamage()
  -> nếu chết: DiedCallback(BillEntityId, RewardBundle)
PlayerPresenter.HandleDied()
  -> PlayerView.SetDeadState()
  -> SceneController.HandlePlayerDied(...)
SceneController
  -> SwitchContext(InputContext.UI)
  -> PlayerDeathHudView.ShowPlayerDied()
PlayerDeathHudView / SceneController.Update()
  -> UI Submit hoặc button Restart
  -> SceneManager.LoadScene(activeScene.path)
```

### Inventory HUD refresh

```text
InventoryService.AddItem/RemoveItem
  -> IInventoryReadService.Changed
InventoryReadSource
  -> Changed
InventoryHudView
  -> RefreshItemsText()
```

---

## 17. Rules không được phá

| ID | Rule |
|---|---|
| R01 | Không dùng `Find()`, `FindObjectOfType()`, scene locator. |
| R02 | Không dùng Singleton cho gameplay systems. |
| R03 | MonoBehaviour callback chỉ forward hoặc write Unity output; không chứa business logic. |
| R04 | Không register entity state/application/presenter/runtime vào DI. |
| R05 | Không tạo abstraction/interface trước khi có nhu cầu thật. |
| R06 | Không reference namespace nội bộ module khác từ module khác. |
| R07 | Cross-module chỉ qua SharedPorts hoặc MessagePipe đã register. |
| R08 | Core BCL-only, `noEngineReferences = true`. |
| R09 | SharedPorts không reference Modules. |
| R10 | ScriptableObject chỉ chứa config data, không chứa runtime state. |
| R11 | Không tạo base class/framework chung như `EntityBase<T>`, `ManagerBase<T>` nếu chưa cần. |
| R12 | Dependency phải hiện qua constructor, `[Inject]`, serialized scene field, hoặc callback wiring rõ ràng. |
| R13 | Không dùng MessagePipe làm backbone khi direct call/callback đang đủ. |
| R14 | Composition không reference Scenes. |
| R15 | Domain/Application không dùng UnityEngine. |
| R16 | Chỉ InputReader được enqueue command. |
| R17 | Prefab có `[Inject]` phải instantiate qua `container.Instantiate()`. |
| R18 | `BillEntityId.New()` CHỈ được gọi từ Spawner/Binder nơi tạo entity instance. |
| R19 | Player death và Enemy death là hai handler riêng trong SceneController. |
| R20 | Active scene startup nằm trong Scenes, không nằm trong Composition. |
| R21 | Input runtime không dùng `PlayerInput` component và không dùng Generate C# wrapper; `InputReader` phải đi qua `InputActionGateway`. |

---

## 18. Build / verification checklist

Sau khi sửa script:

1. Unity Editor refresh asmdef/csproj.
2. Không còn compile error trong Console.
3. `BillGameCore.Core` không có UnityEngine reference.
4. `PlayerApplication` không import UnityEngine.
5. `ProjectLifetimeScope` build được với `BillGameCore.Modules.Inventory` reference.
6. `BootstrapSceneLifetimeScope` có đủ Inspector refs.
   - `ProjectScope` tồn tại và `SceneScope/BootstrapSceneLifetimeScope` trỏ parent scope đúng về `ProjectScope`.
   - Có đủ `WalletReadSource`, `InventoryReadSource`, `PlayerView prefab`, `PlayerConfig`, `LootBinder prefab`.
   - `Input` scene object chỉ có `InputReader`; `InputReader._actions` phải trỏ tới `InputSystem_Actions.inputactions`.
   - `EventSystem/InputSystemUIInputModule` phải dùng cùng `InputActionAsset` với `InputReader`.
   - Không có `PlayerInput` component trên scene object `Input`.
7. Play scene:
   - Project scope build thành công.
   - Scene scope build thành công.
   - `SceneBootstrapper.Start()` spawn player.
   - `InputReader.SetControlledEntity()` nhận player id hợp lệ.
   - Player di chuyển bằng input.
   - Player death gọi `SceneController.HandlePlayerDied`.
   - Death HUD hiện và restart bằng button hoặc `UI Submit` chạy đúng.
   - Enemy death spawn reward loot và/hoặc item loot đúng theo `EnemyConfig`.
   - Nhặt reward loot mới tăng `Gold/Exp`.
   - Nhặt item loot mới tăng inventory và `InventoryHudView` refresh đúng.
   - Stop Play không còn exception teardown.

Nếu Unity batchmode báo project đang mở, đóng Unity Editor rồi chạy lại compile hoặc kiểm tra Console trực tiếp trong Editor.

---

## 19. Quy tắc cập nhật tài liệu

Khi thêm module/slice mới:
- Cập nhật asmdef graph.
- Cập nhật folder map.
- Ghi rõ module đã là baseline hay draft.
- Ghi rõ scope register ở Project hay Scene.
- Ghi rõ flow được duyệt.
- Nếu cần phá rule, tạo ADR mới trước, không sửa ngầm code.

Khi AI làm việc:
- Đọc file liên quan trước.
- Không sửa Core/Composition nếu request không yêu cầu hoặc chưa hỏi rõ.
- Không tự thêm module reference nếu chưa hiểu graph.
- Nếu phát hiện code hiện tại lệch tài liệu, báo lại trước khi sửa.
