// ╔══════════════════════════════════════════════════════════════════════╗
// ║              BILLGAMECORE — REGISTRATION GUIDE v3.0                 ║
// ║   Single source of truth cho VContainer setup.                      ║
// ║   Update file này mỗi khi thêm Service/Signal mới.                 ║
// ║   Symbols: [x] = done  |  [ ] = pending                            ║
// ╚══════════════════════════════════════════════════════════════════════╝
//
// ── MODULE GROUPS ─────────────────────────────────────────────────────
//   Entity      → Provider + Logic + View + Spawner  (Player, Enemy, Boss, NPC, Projectile, Mount)
//   Interaction → Logic + View (± Interface)         (Chest, Door, HealPoint, Trap, Switch)
//   Service     → Manager                            (Inventory, Economy, Save, Audio, Combat)
//
// ── PROJECT LIFETIME SCOPE ────────────────────────────────────────────
//   SIGNALS:
//   [x] PlayerLevelUpSignal          [MODULE: Player]     [GROUP: Entity]
//   [ ] ItemPickedUpSignal           [MODULE: Inventory]  [GROUP: Service]
//   [ ] InventoryFullSignal          [MODULE: Inventory]  [GROUP: Service]
//
//   SERVICES:
//   [x] IPlayerService  → PlayerManager    [MODULE: Player]    [GROUP: Entity]
//   [ ] IInventoryService → InventoryManager [MODULE: Inventory] [GROUP: Service]
//   [ ] IMoneyService   → MoneyManager     [MODULE: Economy]   [GROUP: Service]
//   [ ] ISaveService    → SaveManager      [MODULE: Save]      [GROUP: Service]
//   [ ] IAudioService   → AudioManager     [MODULE: Audio]     [GROUP: Service]
//
// ── SCENE LIFETIME SCOPE ──────────────────────────────────────────────
//   ENTITY SIGNALS:
//   [ ] EnemyDiedSignal              [MODULE: Enemy]      [GROUP: Entity]
//
//   INTERACTION SIGNALS:
//   [ ] ChestActivatedSignal         [MODULE: Chest]      [GROUP: Interaction]
//   [ ] DoorOpenedSignal             [MODULE: Door]       [GROUP: Interaction]
//
//   ENTITY REGISTRATIONS:
//   [ ] EnemyLogic    (Transient)    [MODULE: Enemy]      [GROUP: Entity]
//   [ ] EnemySpawner  (Scoped)       [MODULE: Enemy]      [GROUP: Entity]
//   [ ] EnemyProvider (ComponentInHierarchy)
//
//   INTERACTION REGISTRATIONS:
//   [ ] ChestLogic    (Transient)    [MODULE: Chest]      [GROUP: Interaction]
//   [ ] ChestView     (ComponentInHierarchy)
//
//   SERVICE REGISTRATIONS:
//   [ ] IHarvestingSystem → HarvestingManager (Scoped)
//   [ ] IBuildingSystem   → BuildingManager   (Scoped)
//
// ── ASMDEF MAP ────────────────────────────────────────────────────────
//   BillGameCore.Interfaces          (no deps)
//         ↑
//   BillGameCore.Core                (→ Interfaces, VContainer, MessagePipe)
//         ↑
//   BillGameCore.Modules.Player      [x] done
//   BillGameCore.Modules.Enemy       [ ] todo
//   BillGameCore.Modules.Inventory   [ ] todo
//   BillGameCore.Modules.Economy     [ ] todo
//   BillGameCore.Modules.Chest       [ ] todo
//   BillGameCore.Modules.Door        [ ] todo
//         ↑
//   BillGameCore.Composition         (→ Interfaces, Core, tất cả Modules)
//   BillGameCore.Editor              (→ Core, Interfaces) [editorOnly]
//
// ⚠ KHI THÊM MODULE MỚI:
//   1. Dùng menu BillGameCore/New Module/[Entity|Interaction|Service]
//   2. Thêm asmdef vào BillGameCore.Composition.asmdef references
//   3. Register vào ProjectLifetimeScope.cs hoặc SceneLifetimeScope.cs
//   4. Cập nhật file này

// This is a documentation-only file — no runtime code.
