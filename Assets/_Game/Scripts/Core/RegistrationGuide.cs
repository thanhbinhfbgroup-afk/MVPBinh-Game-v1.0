// ╔══════════════════════════════════════════════════════════════════╗
// ║              BILLGAMECORE — REGISTRATION GUIDE                  ║
// ║   This file is the single source of truth for VContainer setup. ║
// ║   Update this file every time a new Service/Signal is added.    ║
// ║   When working across multiple sessions or AI tools,            ║
// ║   share this file as the 'assembly map' for lắp ráp (merge).   ║
// ╚══════════════════════════════════════════════════════════════════╝
//
// ── HOW TO READ ──────────────────────────────────────────────────────
//   [x] = already registered
//   [ ] = pending registration
//   SCOPE       = which LifetimeScope to register in
//   SIGNAL      = which MessageBroker to register
//   DEPENDS_ON  = what must be registered BEFORE this
//
// ── PROJECT LIFETIME SCOPE (ProjectLifetimeScope.cs) ─────────────────
//
//   SIGNALS:
//   [x] PlayerLevelUpSignal          [MODULE: Player]
//   [ ] ItemPickedUpSignal           [MODULE: Inventory]  ← TODO
//   [ ] InventoryFullSignal          [MODULE: Inventory]  ← TODO
//
//   SERVICES:
//   [x] IPlayerService    → PlayerManager    [MODULE: Player]
//   [ ] IInventoryService → InventoryManager [MODULE: Inventory]  ← TODO
//   [ ] IMoneyService     → MoneyManager     [MODULE: Economy]    ← TODO
//   [ ] ISaveService      → SaveManager      [MODULE: Save]       ← TODO
//   [ ] IAudioService     → AudioManager     [MODULE: Audio]      ← TODO
//
// ── SCENE LIFETIME SCOPE (SceneLifetimeScope.cs) ─────────────────────
//
//   SIGNALS:
//   [ ] EnemyDiedSignal              [MODULE: Combat]     ← TODO
//   [ ] DamageDealtSignal            [MODULE: Combat]     ← TODO
//
//   SERVICES:
//   [ ] IEnemyManager     → EnemyManager      [MODULE: Enemy]      ← TODO
//   [ ] IHarvestingSystem → HarvestingManager [MODULE: Harvesting] ← TODO
//   [ ] IBuildingSystem   → BuildingManager   [MODULE: Building]   ← TODO
//
//  ### ASMDEF ĐÃ TẠO:
//   [x] BillGameCore.Interfaces
//   [x] BillGameCore.Core
//   [x] BillGameCore.Modules.Player
//   [x] BillGameCore.Editor
//   [ ] BillGameCore.Modules.Inventory
//   [ ] BillGameCore.Modules.Economy
//   [ ] BillGameCore.Modules.Combat
//   [ ] BillGameCore.Modules.Enemy
//   [ ] BillGameCore.Modules.Harvesting
//   [ ] BillGameCore.Modules.Building
//   [ ] BillGameCore.Modules.Save
//   [ ] BillGameCore.Modules.Audio

// ── ASMDEF DEPENDENCY MAP ─────────────────────────────────────────────
//
//   BillGameCore.Interfaces          (no deps)
//         ↑
//   BillGameCore.Core                (→ Interfaces, VContainer, MessagePipe)
//         ↑
//   BillGameCore.Modules.Player      (→ Interfaces, Core, VContainer, MessagePipe)
//   BillGameCore.Modules.Inventory   (→ Interfaces, Core, VContainer, MessagePipe) ← TODO
//   BillGameCore.Modules.Combat      (→ Interfaces, Core, VContainer, MessagePipe) ← TODO
//   BillGameCore.Editor              (→ Core, Interfaces) [editorOnly]
//
// ── CONVENTION FOR NEW MODULES ───────────────────────────────────────
//
//   1. Create folder:  Scripts/Modules/[Name]/Interfaces/
//                      Scripts/Modules/[Name]/Providers/
//   2. Add Interface:  Scripts/Modules/[Name]/Interfaces/I[Name]Service.cs
//      Header:  // [MODULE: X] [INTERFACE]
//               // [REGISTER_IN: ProjectLifetimeScope or SceneLifetimeScope]
//   3. Add Manager:    Scripts/Modules/[Name]/Providers/[Name]Manager.cs
//      Header:  // [MODULE: X] [MANAGER]
//               // [SCOPE: ProjectLifetimeScope]
//               // [SIGNAL_PUBLISHES: XSignal]
//               // [SIGNAL_SUBSCRIBES: YSignal]
//               // [DEPENDS_ON: IInventoryService, ...]
//   4. Add Signals:    Scripts/Interfaces/Signals/[Name]Signals.cs
//   5. Create asmdef:  BillGameCore.Modules.[Name].asmdef
//   6. Register:       Update ProjectLifetimeScope.cs or SceneLifetimeScope.cs
//   7. Update this file (RegistrationGuide.cs)

// This is a documentation-only file — no runtime code.
