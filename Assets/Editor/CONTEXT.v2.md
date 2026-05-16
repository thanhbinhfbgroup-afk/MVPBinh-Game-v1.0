# CONTEXT v2.1 — BillGameCore

**Tài liệu kiến trúc bắt buộc cho dự án BillGameCore.**
Mọi AI hoặc người sửa code phải đọc và tuân thủ tài liệu này trước khi thay đổi script.

**Tài liệu phụ trợ cho AI:**
- AI hội thoại/tư vấn đọc thêm `Assets/Editor/AI_CONVERSATION_GUIDE.md`.
- AI/agent sửa code theo module đọc thêm `Assets/Editor/AI_MODULE_WORKFLOW.md`.

**Nguyên tắc vận hành:** không tự suy diễn pattern, không đổi convention ngầm, không thêm abstraction khi chưa có nhu cầu thật. Nếu code hiện tại và tài liệu này mâu thuẫn, phải dừng lại, báo rõ file/line liên quan và hỏi người sở hữu dự án trước khi sửa.

---

## 1. Trạng thái chuẩn hiện tại

**Project:** BillGameCore — Unity `6000.4.6f1`, 2D game, VContainer, New Input System. MessagePipe đã có package nhưng chưa phải backbone hiện tại.

**Baseline đã duyệt hiện tại:**
- `01_Core`
- `02_SharedPorts`
- `03_Modules/Input`
- `03_Modules/Player`
- `03_Modules/Inventory`
- `03_Modules/Enemy`
- `03_Modules/InteractionGroup/Chest`
- `04_Composition/ProjectLifetimeScope`
- `05_Scenes/BootstrapSceneLifetimeScope`
- `05_Scenes/SceneBootstrapper`
- `05_Scenes/SceneController`

**Ghi chú tích hợp Enemy:** `EnemySpawner` được register trong scene scope theo kiểu optional. Chỉ khi gán đủ `EnemyView prefab` và `EnemyConfig` trong Inspector thì `BootstrapSceneLifetimeScope` mới register `EnemySpawner`. `SceneBootstrapper` hiện chưa tự spawn enemy vì chưa có contract vị trí/spawn wave.

**Baseline scene asset hiện tại:** `Assets/_Game/Scripts/05_Scenes/Bootstrap.unity` có `SceneScope` gắn `BootstrapSceneLifetimeScope`, `Input` chỉ gắn `InputReader`, `SceneController`, `PlayerView` prefab ở `Assets/_Game/Prefabs/Player/PlayerView.prefab`, và `PlayerConfig` ở `Assets/_Game/Data/Settings/PlayerConfig.asset`. `InputReader` trỏ trực tiếp tới `Assets/Settings/InputSystem_Actions.inputactions` qua field `_actions`; không dùng `PlayerInput` component và không dùng Generate C# wrapper của Unity Input System.

---

## 2. Triết lý kiến trúc

- **Feature-first, không layer-first.** Mỗi module là một vertical slice: `Domain -> Application -> Presentation -> Infrastructure`.
- **Core tối thiểu.** Core chỉ chứa value objects và contracts thật sự dùng chung. Core không có game flow, không có UnityEngine, không có dependency package ngoài BCL.
- **SharedPorts là biên giao tiếp giữa module.** Module này muốn nói chuyện với module khác thì đi qua `SharedPorts` hoặc MessagePipe khi đã unlock.
- **Composition không biết Scenes.** Tuyệt đối không tạo reference `Composition -> Scenes`.
- **Scenes sở hữu orchestration của scene.** Startup scene nằm ở `05_Scenes/SceneBootstrapper`, không nằm ở `04_Composition/GameBootstrapper`.
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
    │   ├── ProjectLifetimeScope.cs
    │   ├── SceneLifetimeScope.cs  (generic/legacy placeholder, không chứa scene orchestration)
    │   └── GameBootstrapper.cs    (project-level placeholder)
    │
    └── 05_Scenes/
        ├── BillGameCore.Scenes.asmdef
        ├── BootstrapSceneLifetimeScope.cs
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
Core/ValueObjects/EntityId.cs
Core/Combat/DamageInfo.cs
Core/Combat/DamageResult.cs
Core/Combat/IDamageReceiver.cs
Core/Interaction/IInteractable.cs
Core/Inventory/ItemStack.cs
Core/Rewards/RewardBundle.cs
Core/Save/ISaveSnapshotProvider.cs
Core/Save/ISaveSnapshotConsumer.cs
```

Quy tắc Core:
- `EntityId.New()` chỉ gọi trong spawner/binder nơi entity instance được tạo.
- `IDamageReceiver.ReceiveDamage()` là contract combat duy nhất cho nhận damage.
- `ReceiveDamage()` implementations phải clamp damage âm về `0` trước khi trừ máu. Heal/buff không đi qua `DamageInfo`.
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

### Inventory contracts

```text
SharedPorts/Inventory/IInventoryReadService.cs
SharedPorts/Inventory/IInventoryWriteService.cs
```

Implemented by:
- `Modules.Inventory.Application.InventoryService`

Consumed by:
- UI, loot, chest/reward interaction khi cần inventory.

### Economy contracts

```text
SharedPorts/Economy/IWalletService.cs
SharedPorts/Economy/IRewardGrantService.cs
```

Status hiện tại:
- Contract đã tồn tại.
- Implementation chưa tồn tại.
- `SceneController.SetRewardGrantService()` cho phép wiring sau, nhưng hiện không inject bắt buộc để tránh DI fail khi chưa có EconomyService.

### Player read contract

```text
SharedPorts/Player/IPlayerReadService.cs
```

Implemented by:
- `PlayerApplication`

Không dùng contract này cho Enemy. Enemy không phải Player.

### Messages

```text
SharedPorts/Messages/EnemyDiedMessage.cs
SharedPorts/Messages/ItemPickedUpMessage.cs
```

Status:
- Message types đã tồn tại.
- MessagePipe chưa là backbone hiện tại.
- Chỉ dùng MessagePipe từ slice combat/reward/event khi đã đăng ký broker rõ ràng.

---

## 7. Composition

Composition chỉ chứa project-level composition và placeholder chung. Composition không chứa scene-specific orchestration.

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
- Register `PlayerView` prefab.
- Register `PlayerConfig`.
- Register `PlayerSpawner`.
- Optionally register `EnemyView` prefab.
- Optionally register `EnemyConfig`.
- Optionally register `EnemySpawner`.
- Register `SceneBootstrapper` as entry point.

Các field phải gán trong Inspector:

```text
InputReader
SceneController
PlayerView prefab
PlayerConfig
EnemyView prefab        (optional, chỉ cần nếu muốn resolve EnemySpawner)
EnemyConfig             (optional, chỉ cần nếu muốn resolve EnemySpawner)
```

Không gán thiếu field bắt buộc. Nếu thiếu `InputReader`, `SceneController`, `PlayerView prefab` hoặc `PlayerConfig`, VContainer build hoặc runtime startup có thể fail. Enemy fields là optional nhưng phải gán đủ cả prefab và config nếu muốn register `EnemySpawner`.

`BootstrapSceneLifetimeScope.Configure()` phải validate các field bắt buộc trước khi register. Nếu thiếu `InputReader`, `SceneController`, `PlayerView prefab` hoặc `PlayerConfig`, scope phải fail sớm bằng lỗi rõ tên field, không để NullReference mơ hồ ở startup.

`InputReader.ValidateConfiguration()` phải được gọi từ scene scope để xác nhận `InputActionAsset` đã được gán, action map `Player`, và các action `Player/Move`, `Player/Attack`, `Player/Interact` tồn tại trước khi runtime đọc input.

Scene `Bootstrap.unity` baseline hiện có:

```text
SceneScope
  -> BootstrapSceneLifetimeScope
Input
  -> InputReader
SceneController
  -> SceneController
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
- `HandleEnemyDied(...)` dành cho enemy death/reward/loot khi Enemy được tích hợp sau.
- `SetRewardGrantService(IRewardGrantService)` chỉ dùng khi EconomyService đã tồn tại.

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
03_Modules/Input/Context/PlayerInputContext.cs
03_Modules/Input/Context/VehicleInputContext.cs
03_Modules/Input/Context/UIInputContext.cs
```

Rules:
- `CommandBuffer.Enqueue()` chỉ được gọi từ `InputReader`.
- `InputCommandDispatcher` là adapter `CommandBuffer -> IInputCommandSource`.
- `InputActionGateway` là wrapper tự viết quanh `InputActionAsset`. Gateway clone asset runtime, cache action maps/actions cần dùng, bật/tắt map theo `InputContext`, và expose typed reads cho `InputReader`.
- `InputReader` là MonoBehaviour, giữ serialized `InputActionAsset`, dùng `InputActionGateway` để đọc New Input System và dịch raw input thành command object.
- `InputReader` có fallback editor-only để tự gán `Assets/Settings/InputSystem_Actions.inputactions` nếu `_actions` bị null trong Editor sau refresh scene/script. Đây chỉ là safety net cho baseline scene, không phải service locator runtime. Khi dự án có nhiều scene hoặc nhiều input asset, từng scene phải gán `_actions` rõ ràng hoặc dùng scene/input config được duyệt.
- Không gắn `PlayerInput` component vào scene object `Input`.
- Không bật Generate C# wrapper trên `.inputactions`; wrapper chính thức của dự án là `InputActionGateway`.
- Entity presenter không được biết `MoveCommand`, `AttackCommand`, `InteractCommand` concrete types.
- Khi switch context phải clear command buffer.
- `CommandBuffer` có constructor nhận capacity, nên scene scope phải register bằng factory rõ capacity (`new CommandBuffer(size)`), không để VContainer resolve primitive `int`.

Flow:

```text
InputReader.Update()
  -> đọc InputActionGateway trên runtime clone của InputActionAsset
  -> enqueue concrete command
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
- implement `IPlayerReadService`.
- không dùng UnityEngine.
- không biết world position.
- `OnDied` chỉ emit `EntityId` và `RewardBundle`.

### Presentation

`PlayerView`:
- MonoBehaviour.
- callback chỉ forward sang presenter.
- write Rigidbody2D/Animator output theo lệnh từ presenter.
- forward cả `OnTriggerEnter2D` và `OnTriggerExit2D` để presenter giữ interact target hiện tại.

`PlayerPresenter`:
- đọc input qua `IInputCommandSource`.
- không biết concrete command classes.
- lấy world position từ `PlayerView.WorldPosition` khi player chết.
- gọi `OnDiedCallback(EntityId, RewardBundle, Vector2)` cho scene layer.
- nhận `InteractCommand` thì interact với target đang overlap hiện tại; không dùng pending flag phụ thuộc đúng frame `OnTriggerEnter2D`.

`PlayerSpawner`:
- có đúng 1 public constructor để VContainer resolve rõ ràng.
- gọi `EntityId.New()`.
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
  -> nếu chết: PlayerApplication.OnDied(EntityId, RewardBundle)
    -> PlayerPresenter.HandleDied()
      -> lấy PlayerView.WorldPosition
      -> OnDiedCallback(EntityId, RewardBundle, Vector2)
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
Modules/Inventory/Infrastructure/Config/InventorySettings.cs
Modules/Inventory/Infrastructure/Persistence/InventorySaveData.cs
```

`InventoryService`:
- project-scope service.
- owns `InventoryState`.
- implements `IInventoryReadService`.
- implements `IInventoryWriteService`.
- implements `ISaveSnapshotProvider<InventorySaveData>`.
- implements `ISaveSnapshotConsumer<InventorySaveData>`.

Rules:
- Inventory module giữ tên `Inventory`, không đổi thành `InventoryGroup`.
- `InventoryState` không vào DI.
- `InventorySettings` chỉ chứa config data.
- Cross-module access đi qua `IInventoryReadService` hoặc `IInventoryWriteService`.

Current behavior:
- `GetItems()` trả snapshot read-only copy, không trả live `InventoryState.Items`.
- `HasItem(itemId, minAmount)`
- `AddItem(ItemStack)`
- `RemoveItem(itemId, amount)`
- `CreateSnapshot()`
- `RestoreSnapshot(InventorySaveData)`

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
Modules/Enemy/Infrastructure/Config/EnemyConfig.cs
Modules/Enemy/Presentation/EnemyView.cs
Modules/Enemy/Presentation/EnemyPresenter.cs
Modules/Enemy/Presentation/EnemySpawner.cs
Modules/Enemy/Presentation/EnemyRuntime.cs
```

### Domain

`EnemyDefinition`:
- config data runtime bất biến.
- tạo từ `EnemyConfig.ToDefinition()`.
- chứa movement/health/stamina và reward config: `GoldReward`, `ExperienceReward`.

`EnemyState`:
- runtime mutable state cho một enemy instance.
- chỉ `EnemyApplication` sở hữu và mutate.
- không register vào DI.
- không lưu trong ScriptableObject.

### Application

`EnemyApplication`:
- implement `IDamageReceiver`.
- không implement `IPlayerReadService`.
- không dùng UnityEngine.
- không biết world position.
- `OnDied` chỉ emit `EntityId` và `RewardBundle`.
- tạo `RewardBundle` từ reward config trong `EnemyDefinition`.

### Presentation

`EnemyView`:
- MonoBehaviour.
- callback chỉ forward sang presenter.
- write Rigidbody2D/Animator output theo lệnh từ presenter.

`EnemyPresenter`:
- không đọc player input.
- không biết concrete command classes.
- lấy world position từ `EnemyView.WorldPosition` khi enemy chết.
- gọi `OnDiedCallback(EntityId, RewardBundle, Vector2)` cho scene layer.

`EnemySpawner`:
- có đúng 1 public constructor để VContainer resolve rõ ràng.
- gọi `EntityId.New()`.
- tạo `EnemyDefinition`, `EnemyState`, `EnemyApplication`, `EnemyPresenter`, `EnemyRuntime`.
- instantiate `EnemyView` bằng `Object.Instantiate` vì `EnemyView` hiện không có `[Inject]`.

`EnemyRuntime`:
- handle bất biến cho một enemy instance.
- dispose presenter subscription.
- không register vào DI.

Scene integration:
- `Scenes.asmdef` reference `BillGameCore.Modules.Enemy`.
- `BootstrapSceneLifetimeScope` register `EnemySpawner` chỉ khi `_enemyPrefab` và `_enemyConfig` đều được gán.
- `SceneBootstrapper` chưa tự spawn enemy; spawn point/wave contract sẽ được thiết kế ở slice sau.

Approved enemy death flow:

```text
EnemyApplication.ReceiveDamage()
  -> clamp damage âm về 0
  -> OnDied(EntityId, RewardBundle)
    -> EnemyPresenter lấy EnemyView.WorldPosition
    -> OnDiedCallback(EntityId, RewardBundle, Vector2)
      -> SceneController.HandleEnemyDied()
        -> IRewardGrantService.Grant(bundle) nếu EconomyService đã tồn tại
        -> LootSpawner.Spawn(...) khi Loot được build
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
- Message types đã có trong `SharedPorts/Messages`.
- Broker chưa được register làm backbone.

---

## 15. VContainer contract

### Project scope

Project scope hiện tại:

```text
ProjectLifetimeScope
  -> InventoryService as implemented interfaces
```

Project scope dùng cho services sống qua scene:
- Inventory
- Save
- Economy
- Audio

### Scene scope

Scene scope hiện tại:

```text
BootstrapSceneLifetimeScope
  -> CommandBuffer
  -> InputCommandDispatcher as IInputCommandSource
  -> InputReader component
  -> SceneController component
  -> PlayerView prefab
  -> PlayerConfig
  -> PlayerSpawner
  -> EnemyView prefab       (optional)
  -> EnemyConfig            (optional)
  -> EnemySpawner           (optional, only when both Enemy fields are assigned)
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
    -> playerRuntime.Presenter.OnDiedCallback = SceneController.HandlePlayerDied
```

### Player move

```text
InputReader.ReadPlayerMap()
  -> CommandBuffer.Enqueue(MoveCommand)
InputCommandDispatcher.TryDequeue()
  -> PlayerPresenter reads IMoveCommand
  -> PlayerApplication.Tick(dirX, dirY, deltaTime)
  -> PlayerView.SetVelocity(...)
  -> PlayerView.UpdateMoveAnimation(...)
```

### Player interact

```text
InputReader
  -> InteractCommand
PlayerView.OnTriggerEnter2D / OnTriggerExit2D
  -> PlayerPresenter cập nhật current IInteractable target
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
| R18 | `EntityId.New()` chỉ gọi trong spawner/binder tạo entity instance. |
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
   - Enemy refs là optional, nhưng nếu muốn dùng `EnemySpawner` thì phải gán đủ `EnemyView prefab` và `EnemyConfig`.
   - `Input` scene object chỉ có `InputReader`; `InputReader._actions` phải trỏ tới `InputSystem_Actions.inputactions`.
   - Không có `PlayerInput` component trên scene object `Input`.
7. Play scene:
   - Project scope build thành công.
   - Scene scope build thành công.
   - `SceneBootstrapper.Start()` spawn player.
   - `InputReader.SetControlledEntity()` nhận player id hợp lệ.
   - Player di chuyển bằng input.
   - Player death gọi `SceneController.HandlePlayerDied`.

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
