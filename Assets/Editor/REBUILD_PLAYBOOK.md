# REBUILD PLAYBOOK — BillGameCore

## 1. Mục tiêu của file này

File này là bản hướng dẫn rebuild lại dự án **từ đầu đến cuối** theo đúng baseline hiện tại của repo, nhưng theo mode học mới:

- **Bạn tự gõ code**
- **Không dùng code mẫu làm đường chính**
- **Repo hiện tại chỉ là baseline để đối chiếu sau khi bạn tự làm xong từng slice**
- **Mỗi slice phải chạy được, test được, rồi mới mở slice tiếp theo**

File này không phải là tài liệu kiến trúc thay thế cho [CONTEXT.v2.md](/D:\ProjectGame\Game\MVPBinh Game v1.0/Assets/Editor/CONTEXT.v2.md).  
Nó là **playbook thực thi**: bắt đầu từ đâu, đi theo thứ tự nào, ở mỗi bước phải làm gì, test gì, commit lúc nào, so với baseline ra sao.

---

## 2. Cách dùng playbook này

Bạn dùng file này theo đúng nhịp sau:

1. Đọc [CONTEXT.v2.md](/D:\ProjectGame\Game\MVPBinh Game v1.0/Assets/Editor/CONTEXT.v2.md) để hiểu baseline kiến trúc.
2. Dùng file này để biết **thứ tự làm việc**.
3. Chỉ nhìn repo baseline **sau khi** bạn đã tự code xong một slice.
4. Nếu bị kẹt:
- tự debug trước
- tự viết giả thuyết lỗi trước
- chỉ sau đó mới nhờ review

Nguyên tắc quan trọng:

- Không được tạo hàng loạt file rỗng chỉ vì `CONTEXT` có nhắc.
- Không được “fill đủ folder” trước khi có slice thật.
- Không được nhảy thẳng vào `Inventory`, `Loot`, `Enemy`, `Restart`, `UI`, nếu `Player Move` chưa sạch.
- Không được so từng dòng với baseline khi đang code dở. Chỉ so sau khi slice đã tự chạy được hoặc bạn thật sự bí.

---

## 3. Kết quả cuối cùng bạn phải đạt

Khi hoàn thành toàn bộ playbook này, bản rebuild của bạn phải chạy được tối thiểu các flow sau:

1. Scene startup sạch.
2. Player spawn đúng 1 lần.
3. Player move đúng.
4. Chest reward tăng HUD đúng.
5. Enemy gây damage cho player đúng.
6. Player gây damage cho enemy đúng.
7. Enemy chết spawn reward loot và item loot đúng.
8. Player nhặt reward loot thì wallet tăng.
9. Player nhặt item loot thì inventory tăng.
10. Inventory HUD hiện item đúng.
11. Player chết hiện death HUD đúng.
12. Restart bằng button và `UI Submit` đúng.
13. Stop Play không có exception teardown.

Nếu thiếu bất kỳ mục nào trong danh sách trên, coi như rebuild chưa hoàn thành.

---

## 4. Chuẩn bị trước khi bắt đầu

### 4.1. Chốt baseline

Việc phải làm:

1. Giữ repo hiện tại làm `baseline`.
2. Không tiếp tục mở feature mới trên baseline branch.
3. Chỉ sửa baseline nếu phát hiện bug thật hoặc tài liệu sai.
4. Dùng [CONTEXT.v2.md](/D:/Game/forkbinh/Assets/Editor/CONTEXT.v2.md) làm kiến trúc chuẩn hiện tại.

### 4.2. Tạo chỗ rebuild riêng

Việc phải làm:

1. Tạo một branch mới hoặc workspace mới chỉ dành cho rebuild.
2. Nếu muốn học nghiêm túc nhất:
- tốt hơn là copy repo sang nơi riêng để không nhìn code cũ quá dễ
- hoặc ít nhất tạo branch mới và tự ép mình không mở file baseline trước khi xong slice

Khuyến nghị:

- Tạo branch kiểu `codex/rebuild-from-scratch` hoặc tên riêng của bạn.

### 4.3. Chốt mode làm việc

Việc phải làm:

1. Tự viết code trước.
2. Chỉ dùng AI theo 1 trong 3 mode sau:
- review diff
- hỏi kiến trúc
- hỏi debug theo giả thuyết cụ thể

Không dùng AI theo mode sau:

- xin code mẫu hoàn chỉnh cho cả slice
- xin viết hộ file sản xuất chính
- xin “làm nốt luôn giúp tôi”

### 4.4. Tạo thói quen commit

Mỗi slice phải có ít nhất 3 checkpoint:

1. `skeleton pass compile`
2. `feature pass manual test`
3. `cleanup pass`

Không để 4-5 slice dồn vào một commit lớn.

---

## 5. Thứ tự rebuild bắt buộc

Bạn phải đi đúng thứ tự này:

1. Slice 0: Skeleton tối thiểu
2. Slice 1: Player Move
3. Slice 2: Chest Reward
4. Slice 3: Enemy Damage / Enemy Death
5. Slice 4: Player Death + Restart
6. Slice 5: Reward Loot
7. Slice 6: Inventory + Item Loot
8. Final Audit: cập nhật tài liệu + regression pass cuối

Lý do không được đổi thứ tự:

- `Player Move` là xương sống đầu tiên.
- `Chest Reward` mở `SceneController`, `Wallet`, `IInteractable`.
- `Enemy Damage/Death` mở combat contract thật.
- `Player Death + Restart` dùng lại combat và scene UI.
- `Reward Loot` chỉ nên làm sau khi enemy death đã có tín hiệu ổn.
- `Inventory + Item Loot` là lớp sau của loot, không được đi trước loot.

---

## 6. Slice 0 — Skeleton tối thiểu

### 6.1. Mục tiêu

Dựng khung đủ để bắt đầu `Player Move`, nhưng không dựng thừa.

### 6.2. Những gì phải có sau Slice 0

1. Scene `00_Bootstrap` tồn tại và mở được.
2. Có `ProjectScope`.
3. Có `SceneScope`.
4. Có `ProjectLifetimeScope`.
5. Có `BootstrapSceneLifetimeScope`.
6. Có `SceneBootstrapper`.
7. Có asmdef cơ bản cho `Core`, `SharedPorts`, `Modules.Input`, `Modules.Player`, `Composition`, `Scenes`.

### 6.3. Việc phải làm

1. Tạo đúng folder structure tối thiểu:
- `Assets/_Game/Scripts/01_Core`
- `Assets/_Game/Scripts/02_SharedPorts`
- `Assets/_Game/Scripts/03_Modules/Input`
- `Assets/_Game/Scripts/03_Modules/Player`
- `Assets/_Game/Scripts/04_Composition`
- `Assets/_Game/Scripts/05_Scenes`

2. Tạo asmdef tối thiểu:
- `BillGameCore.Core.asmdef`
- `BillGameCore.SharedPorts.asmdef`
- `BillGameCore.Modules.Input.asmdef`
- `BillGameCore.Modules.Player.asmdef`
- `BillGameCore.Composition.asmdef`
- `BillGameCore.Scenes.asmdef`

3. Chốt dependency graph đúng ngay từ đầu theo [CONTEXT.v2.md](/D:\ProjectGame\Game\MVPBinh Game v1.0/Assets/Editor/CONTEXT.v2.md).
4. Tạo `ProjectLifetimeScope.cs` rỗng hoặc gần rỗng.
5. Tạo `BootstrapSceneLifetimeScope.cs`.
6. Tạo `SceneBootstrapper.cs`.
7. Dựng scene `00_Bootstrap`.

### 6.4. Cái gì chưa được làm ở Slice 0

1. Chưa làm chest.
2. Chưa làm combat.
3. Chưa làm loot.
4. Chưa làm inventory.
5. Chưa làm death HUD.
6. Chưa làm wallet HUD.

### 6.5. Pass checklist

1. Project compile.
2. Scene mở được.
3. Không có circular asmdef reference.
4. `Core` có `noEngineReferences: true`.

### 6.6. Thao tác Unity

1. Tạo `00_Bootstrap.unity`.
2. Tạo root object `ProjectScope`.
3. Gắn `ProjectLifetimeScope`.
4. Tạo child `SceneScope`.
5. Gắn `BootstrapSceneLifetimeScope`.

### 6.7. Commit checkpoint

Commit khi:

1. asmdef compile sạch
2. scene mở được
3. scope hierarchy sống

---

## 7. Slice 1 — Player Move

### 7.1. Mục tiêu

Làm xong vertical slice nhỏ nhất nhưng chạm đủ kiến trúc:

- scene boot
- input
- player runtime
- player spawn
- move
- stop khi thả input

### 7.2. Thành phần phải có

#### Core

1. `Assets/_Game/Scripts/01_Core/ValueObjects/BillEntityId.cs`

#### SharedPorts/Input

1. `ICommand.cs`
2. `CommandType.cs`
3. `IMoveCommand.cs`
4. `IAttackCommand.cs`
5. `IInteractCommand.cs`
6. `IInputCommandSource.cs`
7. `InputContext.cs`

#### Modules/Input

1. `InputContextNames.cs`
2. `MoveCommand.cs`
3. `AttackCommand.cs`
4. `InteractCommand.cs`
5. `SwitchContextCommand.cs`
6. `CommandBuffer.cs`
7. `InputCommandDispatcher.cs`
8. `InputActionGateway.cs`
9. `InputReader.cs`

#### Modules/Player

1. `PlayerConfig.cs`
2. `PlayerDefinition.cs`
3. `PlayerState.cs`
4. `PlayerApplication.cs`
5. `PlayerView.cs`
6. `PlayerPresenter.cs`
7. `PlayerSpawner.cs`
8. `PlayerRuntime.cs`

#### Scenes

1. `BootstrapSceneLifetimeScope.cs`
2. `SceneBootstrapper.cs`

### 7.3. Việc phải làm theo thứ tự

1. Tạo `BillEntityId`.
2. Tạo input contracts ở `SharedPorts`.
3. Tạo command implementations ở `Modules/Input`.
4. Tạo `CommandBuffer`.
5. Tạo `InputActionGateway`.
6. Tạo `InputReader`.
7. Tạo `PlayerConfig`.
8. Tạo `PlayerDefinition`.
9. Tạo `PlayerState`.
10. Tạo `PlayerApplication` chỉ xử lý move.
11. Tạo `PlayerView`.
12. Tạo `PlayerPresenter`.
13. Tạo `PlayerRuntime`.
14. Tạo `PlayerSpawner`.
15. Register scene dependencies trong `BootstrapSceneLifetimeScope`.
16. Spawn player trong `SceneBootstrapper`.
17. Tick input và player runtime.

### 7.4. Quy tắc bắt buộc của slice này

1. Không dùng `PlayerInput` component.
2. Không dùng generated wrapper của Unity Input System.
3. Không dùng `Find()`, `FindObjectOfType()`, `Resources.Load()`.
4. Không nhét input logic vào `PlayerView`.
5. Không register `PlayerRuntime`, `PlayerPresenter`, `PlayerApplication`, `PlayerState` vào DI.
6. `PlayerSpawner` phải tạo runtime bằng `new`, không dùng container để resolve entity runtime.

### 7.5. Scene / prefab phải dựng

1. Tạo `Assets/_Game/Prefabs/Player/PlayerView.prefab`
2. Root player object phải có:
- `Rigidbody2D`
- `Collider2D`
- `PlayerView`

3. Tạo `PlayerConfig.asset`
4. Gán `PlayerView prefab` và `PlayerConfig` vào `BootstrapSceneLifetimeScope`

### 7.6. Pass checklist

1. Play vào scene không lỗi.
2. Player spawn đúng 1 lần.
3. Giữ input thì player di chuyển.
4. Thả input thì player dừng.
5. Stop Play không có exception.

### 7.7. Common mistakes

1. Register runtime entity vào DI.
2. Để `InputReader` tự biết player object trực tiếp.
3. Để `PlayerView` tự đọc input.
4. Dùng `transform.Translate()` thay vì output velocity qua `Rigidbody2D`.

### 7.8. Commit checkpoint

1. `player-move compile pass`
2. `player-move scene pass`
3. `player-move cleanup`

---

## 8. Slice 2 — Chest Reward

### 8.1. Mục tiêu

Mở vertical slice đầu tiên cho:

- `IInteractable`
- `SceneController`
- `RewardBundle`
- wallet service
- HUD đọc scene service

### 8.2. Thành phần phải có

#### Core

1. `IInteractable.cs`
2. `RewardBundle.cs`

#### SharedPorts/Economy

1. `IWalletService.cs`
2. `IWalletWriteService.cs`
3. `IRewardGrantService.cs`

#### Modules/InteractionGroup/Chest

1. `ChestDefinition.cs`
2. `ChestState.cs`
3. `ChestOpenResult.cs`
4. `ChestApplication.cs`
5. `ChestView.cs`
6. `ChestPresenter.cs`
7. `ChestBinder.cs`
8. `ChestConfig.cs`

#### Scenes

1. `SceneController.cs`
2. `WalletService.cs`
3. `RewardGrantService.cs`
4. `WalletReadSource.cs`
5. `WalletHudView.cs`

### 8.3. Việc phải làm theo thứ tự

1. Tạo `IInteractable`.
2. Tạo `RewardBundle`.
3. Tạo wallet ports.
4. Tạo chest domain/application/presentation.
5. Tạo `SceneController` để nhận callback chest opened.
6. Tạo `WalletService`.
7. Tạo `RewardGrantService`.
8. Tạo `WalletReadSource`.
9. Tạo `WalletHudView`.
10. Trong `SceneBootstrapper`, set wallet services vào scene.
11. Trong player interaction path, cho player gọi `Interact()` vào `IInteractable` target.

### 8.4. Cái phải cẩn thận

1. `ChestBinder` implement `IInteractable`, không phải `ChestApplication`.
2. Reward được grant khi chest mở thành công, không phải mỗi lần interact.
3. HUD chỉ đọc từ `WalletReadSource`, không chọc thẳng `WalletService`.

### 8.5. Thao tác Unity

1. Tạo chest prefab / scene object.
2. Gắn `ChestBinder`, `ChestView`, collider.
3. Tạo `WalletReadSource` object trong scene.
4. Tạo `WalletHudView` trong `Canvas`.
5. Gán reference vào `BootstrapSceneLifetimeScope`.
6. Dựng interaction sensor nếu cần cho player.

### 8.6. Pass checklist

1. Player tới gần chest.
2. Bấm interact.
3. Chest đổi state mở.
4. Reward chỉ grant 1 lần.
5. HUD cập nhật đúng.

### 8.7. Commit checkpoint

1. `chest-reward flow pass`
2. `wallet-hud pass`

---

## 9. Slice 3 — Enemy Damage / Enemy Death

### 9.1. Mục tiêu

Dựng combat path đầu tiên:

- player đánh enemy
- enemy nhận damage
- enemy chết
- reward contract lên scene

### 9.2. Thành phần phải có

#### Core/Combat

1. `DamageInfo.cs`
2. `DamageResult.cs`
3. `IDamageReceiver.cs`

#### Modules/Enemy

1. `EnemyConfig.cs`
2. `EnemyDefinition.cs`
3. `EnemyState.cs`
4. `EnemyHealthReadModel.cs`
5. `EnemyApplication.cs`
6. `EnemyPresenter.cs`
7. `EnemyRuntime.cs`
8. `EnemyView.cs`
9. `EnemyBinder.cs`
10. `EnemyRuntimeFactory.cs`
11. `EnemyAttackSensor.cs`

#### Player side additions

1. `PlayerAttackSensor.cs`
2. attack path trong `PlayerPresenter`

### 9.3. Việc phải làm theo thứ tự

1. Tạo combat contracts ở `Core`.
2. Mở player attack command path.
3. Tạo `PlayerAttackSensor`.
4. Tạo enemy config/domain/state/application/presentation/runtime.
5. Dùng `EnemyBinder` cho enemy scene object.
6. Tạo `EnemyRuntimeFactory`.
7. Cho `SceneBootstrapper` / `SceneController` init enemy binders.
8. Dùng `ReceiveDamage()` làm contract duy nhất để enemy nhận damage.
9. Tạo flow enemy death báo `RewardBundle` lên `SceneController`.
10. Grant reward trực tiếp ở giai đoạn này.

### 9.4. Cái phải cẩn thận

1. `EnemyBinder` là scene boundary, không phải chỗ nhét logic gameplay lớn.
2. Runtime path bình thường phải dùng `EnemyRuntimeFactory`, không cho binder tự assemble lung tung.
3. `EnemyAttackSensor` không được dùng `GetComponentInParent<IDamageReceiver>()` bừa bãi nếu làm target acquisition. Chỉ bắt đúng collider/body hợp lệ.

### 9.5. Thao tác Unity

1. Tạo enemy scene object.
2. Gắn `EnemyBinder`, `EnemyView`, collider thân chính.
3. Tạo child `AttackSensor` nếu cần cho enemy attack slice sau.
4. Tạo `EnemyConfig.asset`.
5. Gán các enemy binder vào `SceneController`.

### 9.6. Pass checklist

1. Player đánh enemy có damage.
2. Enemy chết đúng 1 lần.
3. Reward grant đúng.
4. Enemy reset/debug path vẫn hoạt động nếu bạn giữ tool debug.

### 9.7. Commit checkpoint

1. `enemy-damage pass`
2. `enemy-death reward pass`

---

## 10. Slice 4 — Player Death + Restart

### 10.1. Mục tiêu

Hoàn thiện nhánh ngược lại của combat:

- enemy đánh player
- player chết
- death HUD hiện
- restart scene

### 10.2. Thành phần phải có

#### Player side

1. health state trong `PlayerState`
2. `ReceiveDamage()` trong `PlayerApplication`
3. `PlayerCombatReceiver.cs`
4. death callback từ application -> presenter -> scene

#### Scenes

1. `PlayerDeathHudView.cs`
2. `SceneController.HandlePlayerDied(...)`
3. input context switch sang `UI`
4. restart flow

#### SharedPorts/Input

1. `IInputContextService.cs`

### 10.3. Việc phải làm theo thứ tự

1. Thêm health vào `PlayerDefinition`.
2. Thêm health + dead state vào `PlayerState`.
3. Mở `ReceiveDamage()` trong `PlayerApplication`.
4. Tạo `PlayerCombatReceiver` trên player root.
5. Tạo enemy attack flow dùng `EnemyAttackSensor`.
6. Khi player chết:
- disable targetability
- disable gameplay input effect
- show death HUD
- switch input context sang `UI`

7. Cho restart bằng button.
8. Cho restart bằng `UI Submit`.

### 10.4. Cái phải cẩn thận

1. Không poll `Keyboard.current` như đường chính nếu đã có input context.
2. `InputReader` và `EventSystem` phải dùng cùng `InputActionAsset`.
3. Khi player chết, object player phải trở thành untargetable.
4. Teardown phải sạch, không được để `MissingReferenceException` khi Stop Play.

### 10.5. Thao tác Unity

1. Tạo `PlayerDeathHudView` object trong `Canvas`.
2. Tạo `PlayerDeathMessage`.
3. Tạo `RestartButton`.
4. Gắn `PlayerCombatReceiver` lên root `PlayerView.prefab`.
5. Đảm bảo `SceneScope` dùng cùng input asset với `EventSystem`.

### 10.6. Pass checklist

1. Enemy đánh player có damage.
2. Player chết đúng một lần.
3. Death HUD hiện đúng.
4. Button restart chạy đúng.
5. `UI Submit` restart đúng ngay cả sau khi bấm phím lung tung.
6. Stop Play không exception.

### 10.7. Commit checkpoint

1. `player-death pass`
2. `restart pass`
3. `teardown pass`

---

## 11. Slice 5 — Reward Loot

### 11.1. Mục tiêu

Đổi flow `enemy chết grant reward ngay` sang:

- enemy chết
- spawn loot
- player nhặt
- lúc nhặt mới nhận reward

### 11.2. Thành phần phải có

#### Modules/InteractionGroup/Loot

1. `LootDefinition.cs`
2. `LootState.cs`
3. `LootCollectResult.cs`
4. `LootApplication.cs`
5. `LootPresenter.cs`
6. `LootView.cs`
7. `LootBinder.cs`
8. `LootSpawner.cs`

### 11.3. Việc phải làm theo thứ tự

1. Tạo loot domain/app cho reward loot.
2. Tạo `LootBinder` là `IInteractable`.
3. Cho loot bắt đầu ở trạng thái inert.
4. Chỉ khi `Initialize(RewardBundle)` thì loot mới active.
5. Tạo `LootSpawner`.
6. Đổi `SceneController.HandleEnemyDied(...)` để spawn loot thay vì grant ngay.
7. Khi nhặt loot:
- scene nhận `LootCollectResult`
- reward mới được grant
- loot tự biến mất

### 11.4. Cái phải cẩn thận

1. Loot không được “sống” khi chưa initialize.
2. Reward chỉ grant lúc nhặt, không grant lúc spawn.
3. Loot object sau khi collect phải biến mất và không collect hai lần.

### 11.5. Thao tác Unity

1. Tạo `LootBinder.prefab`.
2. Root loot có:
- `LootBinder`
- `LootView`
- trigger collider

3. Tạo `VisualRoot` làm graphic.
4. Gán `LootBinder.prefab` vào `BootstrapSceneLifetimeScope`.

### 11.6. Pass checklist

1. Giết enemy thì loot spawn.
2. Chưa nhặt loot thì wallet chưa tăng.
3. Nhặt loot thì wallet tăng.
4. Loot biến mất sau khi nhặt.
5. Restart scene không để lại state cũ.

### 11.7. Commit checkpoint

1. `reward-loot spawn pass`
2. `reward-loot collect pass`

---

## 12. Slice 6 — Inventory + Item Loot

### 12.1. Mục tiêu

Mở một vertical slice inventory thật:

- enemy có item drop
- player nhặt item loot
- inventory tăng
- inventory HUD hiện item

### 12.2. Thành phần phải có

#### Core

1. `ItemStack.cs`

#### SharedPorts/Inventory

1. `IInventoryReadService.cs`
2. `IInventoryWriteService.cs`

#### Modules/Inventory

1. `InventoryState.cs`
2. `InventoryService.cs`

#### Composition

1. `ProjectLifetimeScope.cs` register inventory service singleton

#### Scene bridges

1. `InventoryReadSource.cs`
2. `InventoryHudView.cs`

#### Loot expansion

1. `LootDefinition` hỗ trợ `ItemStack`
2. `LootCollectResult` hỗ trợ `ItemStack`
3. `LootApplication` xử lý item path
4. `LootBinder.Initialize(ItemStack)`
5. `LootSpawner.Spawn(..., ItemStack)`

#### Enemy expansion

1. `EnemyConfig` có `DroppedItemId`
2. `EnemyConfig` có `DroppedItemAmount`
3. `EnemyDefinition` có `ItemDrop`
4. enemy death contract mang `RewardBundle + ItemStack`

### 12.3. Việc phải làm theo thứ tự

1. Tạo `ItemStack`.
2. Tạo inventory ports.
3. Tạo `InventoryState`.
4. Tạo `InventoryService`.
5. Register `InventoryService` ở `ProjectLifetimeScope`.
6. Nối `ProjectScope -> SceneScope`.
7. Mở `Loot` sang item path.
8. Mở `SceneController.HandleLootCollected(...)` để:
- reward path -> grant reward
- item path -> add inventory

9. Mở `EnemyConfig` để có item drop dữ liệu thật.
10. Mở enemy death contract lên `SceneController`.
11. Spawn reward loot và item loot khi enemy có cả hai.
12. Tạo `InventoryReadSource`.
13. Tạo `InventoryHudView`.
14. Nối inventory read service vào scene HUD.

### 12.4. Cái phải cẩn thận

1. `InventoryService` phải là project-scope, không phải scene-scope.
2. `InventoryState` không register vào DI.
3. `GetItems()` nên trả snapshot, không lộ list sống.
4. Inventory HUD không được poll bừa; nên nghe event `Changed`.
5. Nếu `SceneScope` không resolve được inventory service, kiểm tra `parentReference` trước khi debug code.

### 12.5. Thao tác Unity

1. Tạo `ProjectScope` nếu chưa có.
2. Gắn `ProjectLifetimeScope`.
3. Đảm bảo `SceneScope` là child của `ProjectScope`.
4. Set `BootstrapSceneLifetimeScope.parentReference` đúng về `ProjectScope`.
5. Tạo object `InventoryReadSource`.
6. Register `InventoryReadSource` vào `BootstrapSceneLifetimeScope`.
7. Tạo `InventoryHudView` trong `Canvas`.
8. Tạo `InventoryText`.
9. Gán `_inventoryReadSource` và `_itemsText`.
10. Set item drop trong `EnemyConfig.asset`, ví dụ:
- `DroppedItemId = slime_gel`
- `DroppedItemAmount = 1`

### 12.6. Pass checklist

1. Giết enemy có item drop thì item loot spawn.
2. Nhặt item loot thì inventory tăng.
3. Inventory HUD đổi từ `Items:\n-` sang `slime_gel x1`.
4. Nhặt tiếp lần sau thành `slime_gel x2`.
5. Reward loot cũ không gãy.
6. Restart không gãy.

### 12.7. Commit checkpoint

1. `inventory-service pass`
2. `item-loot pass`
3. `inventory-hud pass`

---

## 13. Final Audit — việc phải làm sau khi hoàn tất các slice

### 13.1. Regression pass cuối

Bạn phải chạy lại toàn bộ checklist sau:

1. Scene startup sạch.
2. Player spawn đúng 1 lần.
3. Player move đúng.
4. Chest reward đúng.
5. Enemy damage/death đúng.
6. Player death + restart đúng.
7. Reward loot đúng.
8. Item loot đúng.
9. Inventory HUD đúng.
10. Stop Play sạch exception.

### 13.2. Audit scene

Kiểm tra:

1. Không còn player scene object dư nếu player thật được spawn runtime.
2. `InputReader` và `EventSystem` dùng cùng `InputActionAsset`.
3. `ProjectScope -> SceneScope` đúng.
4. Mọi prefab reference trên scope đều hợp lệ.

### 13.3. Audit kiến trúc

Kiểm tra:

1. `Composition` không reference `Scenes`.
2. `SharedPorts` không reference `Modules`.
3. `Core` không reference Unity.
4. Runtime entity không register vào DI.
5. `MonoBehaviour` không ôm business logic quá mức.

### 13.4. Audit log / cleanup

Kiểm tra:

1. Gỡ `Debug.Log` tạm trên runtime path thật.
2. Giữ tool debug riêng nếu cần.
3. Không để exception “biết rồi, kệ” tồn tại.

### 13.5. Cập nhật tài liệu

Việc phải làm:

1. So lại code với [CONTEXT.v2.md](/D:/Game/forkbinh/Assets/Editor/CONTEXT.v2.md).
2. Chỉ cập nhật `CONTEXT` khi baseline thực sự đổi.
3. Nếu có gì chưa build, ghi rõ `deferred / not built yet`, không được viết như thể đã có.

---

## 14. Cách tự học hiệu quả sau khi có playbook này

### 14.1. Với mỗi slice, quy trình học nên là

1. Đọc phần liên quan trong [CONTEXT.v2.md](/D:/Game/forkbinh/Assets/Editor/CONTEXT.v2.md).
2. Đọc đúng section tương ứng trong file này.
3. Viết ra note riêng:
- mục tiêu slice
- file phải có
- verify thế nào là pass

4. Tự code.
5. Tự test.
6. Tự commit.
7. Chỉ sau đó mới mở baseline repo để đối chiếu.

### 14.2. Cách đối chiếu baseline đúng

Khi mở baseline ra, chỉ hỏi 4 câu:

1. Tôi đang lệch ở **contract** hay chỉ lệch ở cách tổ chức code?
2. Tôi đang sai ở **trách nhiệm class** hay chỉ khác tên helper?
3. Flow của tôi có đi đúng `scene -> module -> core` như baseline không?
4. Nếu baseline làm khác tôi, baseline giải quyết rủi ro gì?

Không hỏi:

1. “Làm sao copy giống hệt?”
2. “Làm sao sửa từng dòng cho giống?”

### 14.3. Khi nào nên xin review

Chỉ xin review khi:

1. Slice đã compile nhưng behavior sai.
2. Bạn có 2 phương án kiến trúc và không biết chọn cái nào.
3. Bạn đã ghi rõ giả thuyết bug nhưng chưa tìm ra nguyên nhân.
4. Bạn muốn audit lại trách nhiệm module trước khi mở slice tiếp theo.

### 14.4. Khi nào không nên xin review

1. Chưa tự đọc `CONTEXT`.
2. Chưa tự test.
3. Chưa tự commit.
4. Chưa mô tả rõ mình đang kẹt ở đâu.

---

## 15. Câu lệnh tự kỷ luật cho bản thân

Trước khi bắt đầu một slice, tự nói rõ 3 câu này:

1. `Mục tiêu của slice này là gì.`
2. `Pass của slice này trông như thế nào.`
3. `Cái gì chưa được phép làm ở slice này.`

Nếu bạn không trả lời được 3 câu đó, không được code tiếp.

---

## 16. Điểm bắt đầu khuyến nghị ngay bây giờ

Nếu bạn chuẩn bị rebuild ngay sau khi đọc file này, hãy bắt đầu ở đúng đây:

1. Tạo branch/workspace rebuild riêng.
2. Tạo một note ngắn tên `REBUILD_PROGRESS.md`.
3. Copy nguyên thứ tự slice trong file này vào đó.
4. Đánh dấu `Slice 0` và `Slice 1` là việc đầu tiên.
5. Bắt đầu từ `Player Move`.

Lý do:

- đây là slice mở đầu tốt nhất
- nó chạm đủ kiến trúc mà chưa kéo theo combat/loot/UI nặng
- nếu bạn không tự dựng lại được `Player Move`, thì mọi slice sau chỉ là kéo dài vấn đề

---

## 17. Kết luận

Bạn không cần làm lại toàn bộ repo theo kiểu “chép tay từ baseline”.  
Bạn cần làm theo kiểu:

1. hiểu baseline
2. tự dựng lại bằng slice nhỏ
3. test từng slice
4. chỉ đối chiếu sau khi tự làm
5. dùng review để sửa tư duy, không dùng review để thay thế việc tự code

Nếu bạn đi hết file này theo đúng thứ tự, bạn sẽ học được:

1. cách dựng vertical slice
2. cách giữ architecture khi code còn ít
3. cách đưa scene orchestration về đúng chỗ
4. cách tách module, shared port, composition, runtime entity
5. cách tự build lại mà không phụ thuộc code mẫu
