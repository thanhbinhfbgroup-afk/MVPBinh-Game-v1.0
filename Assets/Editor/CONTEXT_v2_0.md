# CONTEXT v2.0 — BillGameCore
**Tài liệu kiến trúc dứt khoát. Mọi AI làm việc trên dự án này phải đọc và tuân thủ toàn bộ.**
**Không được phép suy diễn, tự thêm pattern, tự đổi convention ngoài tài liệu này.**

---

## MỤC LỤC
1. Tổng quan & Triết lý
2. Quyết định kiến trúc đã chốt (ADR)
3. Cấu trúc thư mục & Asmdef
4. Shared Kernel — Core
5. Shared Ports — BillGameCore.SharedPorts
6. Nhóm module và Asmdef grouping
7. Archetype chi tiết: Input (Full Command Pattern)
8. Archetype chi tiết: Entity
9. Archetype chi tiết: Interaction
10. Archetype chi tiết: System / Service
11. VContainer Scope Contract
12. Cross-module Communication — 2 giai đoạn
13. MessagePipe — quy tắc sử dụng
14. Approved Game Flow Contracts
15. Rules — danh sách đầy đủ không được phá vỡ
16. Build Order & Checklist
17. Câu hỏi thường gặp (FAQ)

---

## 1. TỔNG QUAN & TRIẾT LÝ

**Tên dự án:** BillGameCore — 2D survival game, Unity 2022+, New Input System, VContainer, MessagePipe (Slice 03+), DOTween, UniTask.

**Triết lý cốt lõi:**
- **Feature-first, không layer-first.** Mỗi module là một vertical slice hoàn chỉnh: Domain → Application → Presentation → Infrastructure.
- **Direct-call trước.** Chỉ thêm abstraction khi có nhu cầu cụ thể, không phòng thủ trước.
- **Cross-module dependency chỉ qua SharedPorts hoặc MessagePipe.** Không bao giờ module A tham chiếu trực tiếp namespace nội bộ của module B.
- **Core giữ tối thiểu.** Core chỉ chứa value types và interfaces mà ≥3 module cần. Không có logic trong Core.
- **SceneController là mediator duy nhất** cho wiring thủ công trong giai đoạn đầu.
- **Runtime-per-entity.** Spawner/Binder tạo instance, không bao giờ đưa entity runtime vào DI scope.

---

## 2. QUYẾT ĐỊNH KIẾN TRÚC ĐÃ CHỐT (ADR)

| # | Chủ đề | Quyết định | Lý do |
|---|--------|-----------|-------|
| ADR-01 | Cross-module dep | SharedPorts asmdef riêng (giai đoạn 1), kết hợp MessagePipe từ Slice 03 | Tránh Core phình, tránh circular dep |
| ADR-02 | Input pattern | Full Command Pattern: ICommand, CommandBuffer, InputContext switching | Scale tốt cho hold/combo/AI/vehicle |
| ADR-03 | MessagePipe | Được phép từ Slice 03 trở đi, chỉ cho cross-module và cross-scene events | Foundation phải chắc trước |
| ADR-04 | EntityId | System.Guid — unique tuyệt đối, không cần static counter | Safe khi spawn parallel |
| ADR-05 | Asmdef grouping | Core + SharedPorts + nhóm module gần nhau chung asmdef | Giảm compile unit, dễ quản lý |
| ADR-06 | VContainer Scope | ProjectScope: Save/Inventory/Economy/Audio. SceneScope: còn lại | Các service xuyên scene cần persist |
| ADR-07 | EntityId trong MVP | Bắt buộc dùng, không bỏ | Combat và Loot cần định danh source |

---

## 3. CẤU TRÚC THƯ MỤC & ASMDEF

```
Assets/_Game/
├── Art/
├── Data/
│   ├── Items/
│   └── Settings/
└── Scripts/
    ├── Core/                          → BillGameCore.Core (asmdef)
    │   ├── ValueObjects/
    │   │   └── EntityId.cs
    │   ├── Combat/
    │   │   ├── DamageInfo.cs
    │   │   ├── DamageResult.cs
    │   │   └── IDamageReceiver.cs
    │   ├── Interaction/
    │   │   └── IInteractable.cs
    │   ├── Inventory/
    │   │   └── ItemStack.cs
    │   ├── Rewards/
    │   │   └── RewardBundle.cs
    │   └── Save/
    │       ├── ISaveSnapshotProvider.cs
    │       └── ISaveSnapshotConsumer.cs
    │
    ├── SharedPorts/                   → BillGameCore.SharedPorts (asmdef)
    │   │   refs: BillGameCore.Core
    │   ├── Input/
    │   │   ├── IInputCommandSource.cs
    │   │   └── InputContext.cs
    │   ├── Combat/
    │   │   └── ICombatService.cs
    │   ├── Inventory/
    │   │   ├── IInventoryReadService.cs
    │   │   └── IInventoryWriteService.cs
    │   ├── Economy/
    │   │   ├── IWalletService.cs
    │   │   └── IRewardGrantService.cs
    │   └── Player/
    │       └── IPlayerReadService.cs
    │
    ├── Composition/                   → BillGameCore.Composition (asmdef)
    │   │   refs: Core, SharedPorts, VContainer, VContainer.Unity
    │   │         + mọi module asmdef khi được tích hợp
    │   ├── ProjectLifetimeScope.cs
    │   ├── SceneLifetimeScope.cs
    │   └── GameBootstrapper.cs
    │
    ├── Modules/
    │   ├── Input/                     → BillGameCore.Modules.Input (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   ├── Commands/
    │   │   │   ├── ICommand.cs
    │   │   │   ├── MoveCommand.cs
    │   │   │   ├── AttackCommand.cs
    │   │   │   ├── InteractCommand.cs
    │   │   │   └── CommandBuffer.cs
    │   │   ├── Context/
    │   │   │   ├── PlayerInputContext.cs
    │   │   │   ├── VehicleInputContext.cs
    │   │   │   └── UIInputContext.cs
    │   │   ├── Application/
    │   │   │   └── InputCommandDispatcher.cs
    │   │   └── Infrastructure/
    │   │       └── InputReader.cs     (MonoBehaviour)
    │   │
    │   ├── Player/                    → BillGameCore.Modules.Combat (asmdef)
    │   │   │   (Combat + Projectile chung asmdef vì phụ thuộc nhau chặt)
    │   │   ...
    │   │
    │   ├── CombatGroup/               → BillGameCore.Modules.CombatGroup (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   │   (Combat + Projectile)
    │   │   ├── Combat/
    │   │   │   ├── Domain/WeaponDefinition.cs
    │   │   │   └── Application/CombatApplication.cs
    │   │   └── Projectile/
    │   │       ├── Domain/ProjectileDefinition.cs · ProjectileState.cs
    │   │       ├── Application/ProjectileApplication.cs
    │   │       └── Presentation/ ...
    │   │
    │   ├── Player/                    → BillGameCore.Modules.Player (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   ├── Domain/
    │   │   ├── Application/
    │   │   │   └── Ports/             (ports nội bộ nếu cần)
    │   │   ├── Presentation/
    │   │   └── Infrastructure/
    │   │
    │   ├── EnemyGroup/               → BillGameCore.Modules.EnemyGroup (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   │   (Enemy + Loot chung vì Enemy emit RewardBundle → LootSpawner)
    │   │   ├── Enemy/
    │   │   └── Loot/
    │   │
    │   ├── InventoryGroup/           → BillGameCore.Modules.InventoryGroup (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   │   (Inventory + Economy chung — IRewardGrantService impl ở đây)
    │   │   ├── Inventory/
    │   │   └── Economy/
    │   │
    │   ├── InteractionGroup/         → BillGameCore.Modules.InteractionGroup (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   │   (Chest, Door, HealPoint, Trap... mỗi cái 1 thư mục con)
    │   │   └── Chest/ · Door/ · HealPoint/ ...
    │   │
    │   ├── Save/                     → BillGameCore.Modules.Save (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   ├── Application/SaveService.cs
    │   │   └── Infrastructure/Persistence/SaveAdapter.cs
    │   │
    │   ├── Audio/                    → BillGameCore.Modules.Audio (asmdef)
    │   │   │   refs: Core, SharedPorts
    │   │   └── Application/AudioService.cs
    │   │
    │   └── UI/                       → BillGameCore.Modules.UI (asmdef)
    │       │   refs: Core, SharedPorts
    │       ├── HUD/
    │       └── InventoryPanel/
    │
    ├── Scenes/                        → BillGameCore.Scenes (asmdef)
    │   │   refs: Core, SharedPorts, Composition, VContainer
    │   └── SceneController.cs
    │
    └── Editor/                        → BillGameCore.Editor (asmdef, editorOnly)
        └── BillGameCoreInitTool.cs
```

### Asmdef dependency graph (chỉ cho phép chiều này):

```
Core  ←── SharedPorts ←── Modules.* ←── Composition
                                    ←── Scenes
                       ←── Composition
```

**Tuyệt đối không được:**
- Module A ref trực tiếp namespace nội bộ của Module B
- SharedPorts ref bất kỳ Module nào
- Core ref bất kỳ gì ngoài C# BCL

---

## 4. SHARED KERNEL — Core

**Nguyên tắc thêm vào Core:** Chỉ khi ≥3 module khác nhau cần, stable (không thay đổi thường xuyên), và không có logic.

### Core/ValueObjects/EntityId.cs
```csharp
namespace BillGameCore.Core.ValueObjects
{
    public readonly struct EntityId : System.IEquatable<EntityId>
    {
        public static readonly EntityId Invalid = new EntityId(System.Guid.Empty);
        public static EntityId New() => new EntityId(System.Guid.NewGuid());

        private EntityId(System.Guid value) { Value = value; }

        public System.Guid Value { get; }
        public bool IsValid => Value != System.Guid.Empty;

        public bool Equals(EntityId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityId e && Equals(e);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString("N").Substring(0, 8); // short display

        public static bool operator ==(EntityId a, EntityId b) => a.Equals(b);
        public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);
    }
}
```

### Core/Combat/DamageInfo.cs
```csharp
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Core.Combat
{
    // Built by CombatApplication, passed to IDamageReceiver.ReceiveDamage().
    // Pure C#: no UnityEngine references in Core.
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, EntityId sourceId, bool isCritical = false)
        {
            Amount     = amount;
            SourceId   = sourceId;
            IsCritical = isCritical;
        }

        public float    Amount     { get; }
        public EntityId SourceId   { get; }
        public bool     IsCritical { get; }
    }
}
```

### Core/Combat/DamageResult.cs
```csharp
namespace BillGameCore.Core.Combat
{
    public readonly struct DamageResult
    {
        public DamageResult(float appliedDamage, float remainingHealth, bool justDied)
        {
            AppliedDamage   = appliedDamage;
            RemainingHealth = remainingHealth;
            JustDied        = justDied;
        }

        public float AppliedDamage   { get; }
        public float RemainingHealth { get; }
        public bool  JustDied        { get; }
    }
}
```

### Core/Combat/IDamageReceiver.cs
```csharp
namespace BillGameCore.Core.Combat
{
    // Method tên PHẢI là ReceiveDamage — không được đổi.
    // Implemented by: EnemyApplication, PlayerApplication.
    // Called by: CombatApplication ONLY.
    public interface IDamageReceiver
    {
        DamageResult ReceiveDamage(DamageInfo damage);
    }
}
```

### Core/Interaction/IInteractable.cs
```csharp
namespace BillGameCore.Core.Interaction
{
    // Implemented by: Binder classes (ChestBinder, LootItemBinder...).
    // Called by: PlayerPresenter qua GetComponent<IInteractable>() — không biết type cụ thể.
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
    }
}
```

### Core/Inventory/ItemStack.cs
```csharp
namespace BillGameCore.Core.Inventory
{
    public readonly struct ItemStack
    {
        public ItemStack(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int    Amount { get; }
    }
}
```

### Core/Rewards/RewardBundle.cs
```csharp
using BillGameCore.Core.Inventory;

namespace BillGameCore.Core.Rewards
{
    // Emitted by EnemyApplication.OnDied, consumed by SceneController → LootSpawner + IRewardGrantService.
    public sealed class RewardBundle
    {
        public int         Gold       = 0;
        public int         Experience = 0;
        public ItemStack[] Items      = System.Array.Empty<ItemStack>();
    }
}
```

### Core/Save/ISaveSnapshotProvider.cs & ISaveSnapshotConsumer.cs
```csharp
namespace BillGameCore.Core.Save
{
    public interface ISaveSnapshotProvider<out TSnapshot> { TSnapshot CreateSnapshot(); }
    public interface ISaveSnapshotConsumer<in TSnapshot>  { void RestoreSnapshot(TSnapshot snapshot); }
}
```

---

## 5. SHARED PORTS — BillGameCore.SharedPorts

**Đây là asmdef trung gian duy nhất.** Module A muốn dùng service của Module B → chỉ ref SharedPorts, không ref Module B.

### SharedPorts/Input/IInputCommandSource.cs
```csharp
using BillGameCore.Modules.Input.Commands;

namespace BillGameCore.SharedPorts.Input
{
    // Consumed by: PlayerPresenter, EnemyAI (khi cần AI override input).
    // Implemented by: InputCommandDispatcher (trong Module Input).
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
        bool HasCommands { get; }
    }
}
```

### SharedPorts/Input/InputContext.cs
```csharp
namespace BillGameCore.SharedPorts.Input
{
    public enum InputContext
    {
        None    = 0,
        Player  = 1,
        Vehicle = 2,
        UI      = 3,
    }
}
```

### SharedPorts/Combat/ICombatService.cs
```csharp
using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Combat
{
    public interface ICombatService
    {
        // Melee: resolve damage trực tiếp lên IDamageReceiver
        void RequestAttack(EntityId attackerId, IDamageReceiver target, string weaponId);

        // Ranged: spawn projectile, damage tính khi va chạm
        void RequestRangedAttack(EntityId attackerId, float dirX, float dirY, string weaponId);

        // Gọi từ ProjectilePresenter khi hit
        void ResolveProjectileHit(EntityId attackerId, IDamageReceiver target, string projectileId);
    }
}
```

### SharedPorts/Inventory/IInventoryReadService.cs & IInventoryWriteService.cs
```csharp
using BillGameCore.Core.Inventory;
using System.Collections.Generic;

namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryReadService
    {
        IReadOnlyList<ItemStack> GetItems();
        bool HasItem(string itemId, int minAmount = 1);
    }

    public interface IInventoryWriteService
    {
        bool AddItem(ItemStack stack);
        bool RemoveItem(string itemId, int amount);
    }
}
```

### SharedPorts/Economy/IWalletService.cs & IRewardGrantService.cs
```csharp
using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    public interface IWalletService
    {
        int Gold       { get; }
        int Experience { get; }
    }

    public interface IRewardGrantService
    {
        // Gọi từ SceneController sau khi Enemy chết — cộng exp+gold ngay, không qua Loot.
        void Grant(RewardBundle bundle);
    }
}
```

### SharedPorts/Player/IPlayerReadService.cs
```csharp
namespace BillGameCore.SharedPorts.Player
{
    // Consumed by: UI module (HUD).
    // Implemented by: PlayerApplication (exposed qua PlayerService wrapper nếu cần).
    public interface IPlayerReadService
    {
        float CurrentHealth { get; }
        float MaxHealth     { get; }
        float CurrentStamina { get; }
    }
}
```

---

## 6. NHÓM MODULE VÀ ASMDEF GROUPING

| Asmdef | Modules bên trong | Refs |
|--------|------------------|------|
| `BillGameCore.Modules.Input` | Input (Commands, Context, Reader) | Core, SharedPorts |
| `BillGameCore.Modules.Player` | Player | Core, SharedPorts |
| `BillGameCore.Modules.CombatGroup` | Combat + Projectile | Core, SharedPorts |
| `BillGameCore.Modules.EnemyGroup` | Enemy + Loot | Core, SharedPorts |
| `BillGameCore.Modules.InventoryGroup` | Inventory + Economy | Core, SharedPorts |
| `BillGameCore.Modules.InteractionGroup` | Chest, Door, HealPoint, Trap... | Core, SharedPorts |
| `BillGameCore.Modules.Save` | Save | Core, SharedPorts |
| `BillGameCore.Modules.Audio` | Audio | Core, SharedPorts |
| `BillGameCore.Modules.UI` | HUD, InventoryPanel | Core, SharedPorts |

**Quy tắc nhóm:** Các module trong cùng asmdef được phép tham chiếu lẫn nhau trực tiếp bởi vì chúng có **coupling bắt buộc theo thiết kế** (Combat ↔ Projectile, Enemy ↔ Loot). Module ở asmdef khác → BẮT BUỘC qua SharedPorts.

---

## 7. ARCHETYPE: INPUT (FULL COMMAND PATTERN)

### Vấn đề giải quyết
- **Identity:** Mỗi Command biết ai tạo ra nó (`SourceId`) → không bị rối khi có Player + AI + Vehicle.
- **State/Hold/Combo:** CommandBuffer tích lũy theo thời gian, Dispatcher xử lý logic phức tạp tại đây.
- **Context switching:** PlayerMap / VehicleMap / UIMap tách biệt trong Input Action Asset.
- **Decoupling:** PlayerPresenter inject `IInputCommandSource` từ SharedPorts, không biết InputReader tồn tại.

### File structure
```
Modules/Input/
├── Commands/
│   ├── ICommand.cs
│   ├── CommandType.cs
│   ├── MoveCommand.cs
│   ├── AttackCommand.cs
│   ├── InteractCommand.cs
│   ├── SwitchContextCommand.cs
│   └── CommandBuffer.cs
├── Context/
│   ├── PlayerInputContext.cs
│   ├── VehicleInputContext.cs
│   └── UIInputContext.cs
├── Application/
│   └── InputCommandDispatcher.cs
└── Infrastructure/
    └── InputReader.cs            ← MonoBehaviour, đọc New Input System
```

### ICommand.cs
```csharp
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Input.Commands
{
    public interface ICommand
    {
        EntityId    SourceId  { get; }   // Ai tạo ra command này
        CommandType Type      { get; }
        float       Timestamp { get; }   // Time.time khi tạo
    }
}
```

### CommandType.cs
```csharp
namespace BillGameCore.Modules.Input.Commands
{
    public enum CommandType
    {
        Move      = 0,
        Attack    = 1,
        Interact  = 2,
        SwitchContext = 3,
        // Thêm khi cần — không xóa value cũ (save compatibility)
    }
}
```

### MoveCommand.cs (ví dụ đại diện)
```csharp
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class MoveCommand : ICommand
    {
        public MoveCommand(EntityId sourceId, float dirX, float dirY, float timestamp)
        {
            SourceId  = sourceId;
            DirX      = dirX;
            DirY      = dirY;
            Timestamp = timestamp;
        }

        public EntityId    SourceId  { get; }
        public CommandType Type      => CommandType.Move;
        public float       Timestamp { get; }

        public float DirX { get; }
        public float DirY { get; }
        public bool  IsMoving => DirX != 0f || DirY != 0f;
    }
}
```

### AttackCommand.cs
```csharp
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class AttackCommand : ICommand
    {
        public AttackCommand(EntityId sourceId, float timestamp, bool isHeld = false, float heldDuration = 0f)
        {
            SourceId     = sourceId;
            Timestamp    = timestamp;
            IsHeld       = isHeld;
            HeldDuration = heldDuration;
        }

        public EntityId    SourceId     { get; }
        public CommandType Type         => CommandType.Attack;
        public float       Timestamp    { get; }
        public bool        IsHeld       { get; }      // true = đang giữ nút
        public float       HeldDuration { get; }      // giây đã giữ
    }
}
```

### CommandBuffer.cs
```csharp
using System.Collections.Generic;

namespace BillGameCore.Modules.Input.Commands
{
    // Thread-safe bằng lock nhỏ — đủ cho single-scene game loop.
    // Dispatcher Enqueue từ InputReader (Update), Consumer Dequeue từ Presenter (Update).
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue = new Queue<ICommand>();
        private readonly object _lock = new object();
        private readonly int    _maxSize;

        public CommandBuffer(int maxSize = 32) { _maxSize = maxSize; }

        public void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize) _queue.Dequeue(); // drop oldest
                _queue.Enqueue(command);
            }
        }

        public bool TryDequeue(out ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count == 0) { command = null; return false; }
                command = _queue.Dequeue();
                return true;
            }
        }

        public bool HasCommands { get { lock (_lock) return _queue.Count > 0; } }

        public void Clear() { lock (_lock) _queue.Clear(); }
    }
}
```

### InputCommandDispatcher.cs
```csharp
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Registered in SceneLifetimeScope as IInputCommandSource.
    // InputReader → Enqueue → CommandBuffer → Dispatcher → Consumer Presenter.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer)
        {
            _buffer = buffer;
        }

        // IInputCommandSource
        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
        public bool HasCommands => _buffer.HasCommands;
    }
}
```

### InputReader.cs (MonoBehaviour)
```csharp
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour — đọc New Input System, chuyển thành ICommand, push vào CommandBuffer.
    // RULE: không có business logic ở đây — chỉ translate raw input sang Command objects.
    // Registered: builder.RegisterComponent<InputReader>() trong SceneLifetimeScope.
    // CommandBuffer và InputCommandDispatcher nhận qua DI constructor.
    public sealed class InputReader : MonoBehaviour
    {
        // Inject qua VContainer [Inject] attribute (MonoBehaviour không dùng constructor injection).
        [Inject] private CommandBuffer _buffer;

        private PlayerInput _playerInput;
        private EntityId    _controlledEntityId; // set bởi SceneController sau khi Player spawn

        // Hold tracking
        private bool  _attackHeld;
        private float _attackHeldStart;

        // Context hiện tại
        private InputContext _currentContext = InputContext.Player;

        private void Update()
        {
            if (_currentContext == InputContext.Player)
                ReadPlayerMap();
        }

        // Gọi từ SceneController sau khi PlayerSpawner.Spawn() trả về Runtime.
        public void SetControlledEntity(EntityId id) => _controlledEntityId = id;

        public void SwitchContext(InputContext context)
        {
            _currentContext = context;
            _buffer.Clear(); // flush pending commands khi đổi context
        }

        private void ReadPlayerMap()
        {
            // Move — đọc mỗi frame (continuous)
            var move = _playerInput.actions["Player/Move"].ReadValue<Vector2>();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, move.x, move.y, Time.time));

            // Attack — held tracking
            var attackAction = _playerInput.actions["Player/Attack"];
            if (attackAction.WasPressedThisFrame())
            {
                _attackHeld      = true;
                _attackHeldStart = Time.time;
            }
            if (_attackHeld)
            {
                float held = Time.time - _attackHeldStart;
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time, isHeld: true, heldDuration: held));
            }
            if (attackAction.WasReleasedThisFrame())
                _attackHeld = false;

            // Interact
            if (_playerInput.actions["Player/Interact"].WasPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }
    }
}
```

### Unity Input Action Asset — cấu trúc bắt buộc
```
InputActions.inputactions
├── ActionMap: Player
│   ├── Move        (Value, Vector2, Composite: WASD + Left Stick)
│   ├── Attack      (Button, Left Mouse + Gamepad West)
│   └── Interact    (Button, E + Gamepad South)
├── ActionMap: Vehicle
│   ├── Throttle    (Value, float)
│   ├── Steer       (Value, float)
│   └── Exit        (Button)
└── ActionMap: UI
    ├── Navigate    (Value, Vector2)
    ├── Submit      (Button)
    └── Cancel      (Button)
```

---

## 8. ARCHETYPE: ENTITY

Áp dụng cho: Player, Enemy, Boss, Mount, NPC.

### Nguyên tắc
- **Domain:** pure C# — không có UnityEngine, không có float MoveX/MoveY dùng Vector2 Unity trong Domain/Application.
- **Application:** implement `IDamageReceiver`. Emit C# event `OnDied`. Không biết Presenter, View, hay bất kỳ module khác.
- **Presenter:** đọc `IInputCommandSource` (từ SharedPorts), process từng frame, gọi Application.Tick(). Gọi View cho visual output.
- **Spawner:** registered trong SceneLifetimeScope, nhận prefab + config qua DI. Tạo Runtime, không đưa Runtime vào DI.
- **Runtime:** immutable handle, implement `IDisposable`, Dispose() clean up event subscriptions.

### Entity file layout
```
Modules/Player/
├── Domain/
│   ├── PlayerDefinition.cs     ← pure C#, không có UnityEngine
│   └── PlayerState.cs          ← pure C#, mutable runtime state
├── Application/
│   ├── PlayerApplication.cs    ← IDamageReceiver, emit OnDied
│   └── Ports/                  ← narrow ports nếu feature khác cần đọc Player
│       └── IPlayerReadService.cs  (implement luôn, register trong SharedPorts)
├── Presentation/
│   ├── PlayerView.cs           ← MonoBehaviour, chỉ forward Unity callbacks
│   ├── PlayerPresenter.cs      ← đọc IInputCommandSource, orchestrate
│   ├── PlayerRuntime.cs        ← IDisposable handle
│   └── PlayerSpawner.cs        ← registered in SceneLifetimeScope
└── Infrastructure/
    └── Config/
        └── PlayerConfig.cs     ← ScriptableObject mapper, không có runtime state
```

### PlayerApplication.cs (template chuẩn)
```csharp
using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.SharedPorts.Player;

namespace BillGameCore.Modules.Player.Application
{
    public sealed class PlayerApplication : IDamageReceiver, IPlayerReadService
    {
        private readonly PlayerDefinition _def;
        private readonly PlayerState      _state;

        public event Action<EntityId, RewardBundle> OnDied; // không dùng ở Player nhưng giữ pattern

        public EntityId Id { get; }

        public PlayerApplication(EntityId id, PlayerDefinition definition, PlayerState state)
        {
            Id     = id;
            _def   = definition;
            _state = state;
            _state.CurrentHealth  = definition.MaxHealth;
            _state.CurrentStamina = definition.MaxStamina;
        }

        // IPlayerReadService (expose cho UI qua SharedPorts)
        public float CurrentHealth  => _state.CurrentHealth;
        public float MaxHealth      => _def.MaxHealth;
        public float CurrentStamina => _state.CurrentStamina;

        public bool IsDead => _state.IsDead;

        public void Tick(float dirX, float dirY, float deltaTime)
        {
            if (_state.IsDead) return;
            float speed = _def.MoveSpeed;
            _state.VelocityX = dirX * speed;
            _state.VelocityY = dirY * speed;
        }

        // IDamageReceiver — gọi bởi CombatApplication ONLY
        public DamageResult ReceiveDamage(DamageInfo damage)
        {
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied) _state.IsDead = true;

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }
    }
}
```

### PlayerPresenter.cs (template chuẩn — đọc IInputCommandSource)
```csharp
using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter : IDisposable
    {
        private readonly PlayerApplication   _application;
        private readonly PlayerView          _view;
        private readonly IInputCommandSource _inputSource; // từ SharedPorts — không biết InputReader

        private bool  _deathVisualPlayed;
        private float _lastDirX, _lastDirY;

        public PlayerPresenter(
            PlayerApplication   application,
            PlayerView          view,
            IInputCommandSource inputSource)   // injected qua Spawner
        {
            _application = application;
            _view        = view;
            _inputSource = inputSource;
        }

        // Gọi từ PlayerView.Update()
        public void OnUpdate(float deltaTime)
        {
            if (_application.IsDead)
            {
                if (!_deathVisualPlayed) { _deathVisualPlayed = true; _view.PlayDeath(); }
                return;
            }

            // Process command queue — mỗi command xử lý một lần
            while (_inputSource.TryDequeue(out ICommand cmd))
            {
                switch (cmd)
                {
                    case MoveCommand move:
                        _lastDirX = move.DirX;
                        _lastDirY = move.DirY;
                        break;

                    case AttackCommand atk:
                        // Gọi ICombatService — được inject vào Presenter nếu cần
                        // Đặt vào đây khi Combat slice sẵn sàng
                        break;

                    case InteractCommand _:
                        // GetComponent<IInteractable>() từ overlap list — xử lý tại View/Presenter
                        break;
                }
            }

            _application.Tick(_lastDirX, _lastDirY, deltaTime);
            _view.UpdateMoveAnimation(_lastDirX, _lastDirY);
        }

        // Gọi từ PlayerView.FixedUpdate()
        public void OnFixedUpdate()
        {
            _view.SetVelocity(_application._state.VelocityX, _application._state.VelocityY);
            // Note: expose VelocityX/Y qua property công khai trong PlayerApplication nếu cần
        }

        public void Dispose() { /* cleanup nếu có event subscription */ }
    }
}
```

### PlayerSpawner.cs (template chuẩn)
```csharp
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.Modules.Player.Infrastructure;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    // Registered in SceneLifetimeScope.
    // Nhận prefab + config + IInputCommandSource qua DI constructor.
    public sealed class PlayerSpawner
    {
        private readonly PlayerView          _prefab;
        private readonly PlayerConfig        _config;
        private readonly IInputCommandSource _inputSource;

        public PlayerSpawner(PlayerView prefab, PlayerConfig config, IInputCommandSource inputSource)
        {
            _prefab      = prefab;
            _config      = config;
            _inputSource = inputSource;
        }

        public PlayerRuntime Spawn(Vector2 position)
        {
            var id          = EntityId.New();
            var definition  = _config.ToDefinition();
            var state       = new PlayerState();
            var application = new PlayerApplication(id, definition, state);
            var view        = Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter   = new PlayerPresenter(application, view, _inputSource);

            view.Bind(presenter);

            return new PlayerRuntime(id, definition, state, application, view, presenter);
        }
    }
}
```

---

## 9. ARCHETYPE: INTERACTION

Áp dụng cho: Chest, Door, HealPoint, Trap, LootItem — tất cả trong `InteractionGroup` asmdef.

### Nguyên tắc
- Binder là MonoBehaviour, implement `IInteractable` (từ Core).
- PlayerPresenter gọi `GetComponent<IInteractable>()` khi trigger overlap → không bao giờ biết type cụ thể.
- Binder có thể inject service từ SharedPorts (VD: LootItemBinder inject `IInventoryWriteService`).
- Binder KHÔNG được register vào DI scope — nó tự tạo Application/State trong Awake.
- Dependency service (IInventoryWriteService) được inject vào Binder qua `[Inject]` attribute nếu Binder là component trên prefab được instantiate sau khi container đã build.

### LootItemBinder.cs (ví dụ cụ thể nhất)
```csharp
using BillGameCore.Core.Interaction;
using BillGameCore.Core.Inventory;
using BillGameCore.SharedPorts.Inventory;
using UnityEngine;
using VContainer;

namespace BillGameCore.Modules.InteractionGroup.Loot.Presentation
{
    // Đặt trên LootItem prefab.
    // LootSpawner gọi GetComponent<LootItemBinder>().SetItemStack(stack) sau khi Instantiate.
    // [Inject] được resolve bởi VContainer nếu prefab instantiated qua container.
    public sealed class LootItemBinder : MonoBehaviour, IInteractable
    {
        private ItemStack              _itemStack;
        private bool                   _pickedUp;
        [Inject] private IInventoryWriteService _inventory; // injected sau Instantiate

        // Gọi bởi LootSpawner ngay sau Instantiate
        public void SetItemStack(ItemStack stack) => _itemStack = stack;

        public bool CanInteract() => !_pickedUp && _inventory != null;

        public void Interact()
        {
            if (!CanInteract()) return;
            if (_inventory.AddItem(_itemStack))
            {
                _pickedUp = true;
                Destroy(gameObject);
            }
        }
    }
}
```

**Lưu ý quan trọng — Inject vào prefab runtime:**
Để `[Inject]` hoạt động trên runtime instantiated prefab, dùng:
```csharp
// Trong LootSpawner (SceneLifetimeScope registered):
var go = _container.Instantiate(_lootItemPrefab, position, Quaternion.identity);
// Không dùng Object.Instantiate thông thường nếu cần [Inject] trên component.
```

---

## 10. ARCHETYPE: SYSTEM / SERVICE

Áp dụng cho: InventoryService, EconomyService, SaveService, AudioService.

### Nguyên tắc
- Service tự tạo và sở hữu State nội bộ — State không inject qua DI.
- Service implement interfaces từ SharedPorts để module khác dùng.
- Service implement `ISaveSnapshotProvider<T>` và `ISaveSnapshotConsumer<T>` khi cần persist.
- Register trong ProjectLifetimeScope (sống xuyên scene) hoặc SceneLifetimeScope tùy loại.
- Không expose State property công khai — chỉ expose qua narrow interface methods.

### InventoryService.cs (template chuẩn)
```csharp
using System;
using System.Collections.Generic;
using BillGameCore.Core.Inventory;
using BillGameCore.Core.Save;
using BillGameCore.Modules.InventoryGroup.Inventory.Infrastructure;
using BillGameCore.SharedPorts.Inventory;

namespace BillGameCore.Modules.InventoryGroup.Inventory.Application
{
    public sealed class InventoryService
        : IInventoryReadService,
          IInventoryWriteService,
          ISaveSnapshotProvider<InventorySaveData>,
          ISaveSnapshotConsumer<InventorySaveData>
    {
        private readonly List<ItemStack> _items = new List<ItemStack>();

        public event Action Changed;

        // IInventoryReadService
        public IReadOnlyList<ItemStack> GetItems() => _items;
        public bool HasItem(string itemId, int minAmount = 1)
            => _items.Exists(s => s.ItemId == itemId && s.Amount >= minAmount);

        // IInventoryWriteService
        public bool AddItem(ItemStack stack)
        {
            // merge stack nếu cùng itemId
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].ItemId == stack.ItemId)
                {
                    _items[i] = new ItemStack(stack.ItemId, _items[i].Amount + stack.Amount);
                    Changed?.Invoke();
                    return true;
                }
            }
            _items.Add(stack);
            Changed?.Invoke();
            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].ItemId != itemId) continue;
                if (_items[i].Amount < amount) return false;
                int remaining = _items[i].Amount - amount;
                if (remaining == 0) _items.RemoveAt(i);
                else _items[i] = new ItemStack(itemId, remaining);
                Changed?.Invoke();
                return true;
            }
            return false;
        }

        // ISaveSnapshotProvider
        public InventorySaveData CreateSnapshot()
            => new InventorySaveData { Items = _items.ToArray() };

        // ISaveSnapshotConsumer
        public void RestoreSnapshot(InventorySaveData snapshot)
        {
            if (snapshot == null) return;
            _items.Clear();
            if (snapshot.Items != null) _items.AddRange(snapshot.Items);
            Changed?.Invoke();
        }
    }
}
```

---

## 11. VCONTAINER SCOPE CONTRACT

### ProjectLifetimeScope (sống xuyên scene)
```csharp
// Register:
// InventoryService  → as IInventoryReadService, IInventoryWriteService
// EconomyService    → as IWalletService, IRewardGrantService
// SaveService       → singleton
// AudioService      → singleton
// KHÔNG register: bất kỳ thứ gì per-entity (State, Application, Presenter, Runtime)
```

### SceneLifetimeScope (sống trong một scene)
```csharp
// Register:
// GameBootstrapper (IStartable)
// InputCommandDispatcher → as IInputCommandSource
// CommandBuffer (singleton trong scene)
// InputReader (MonoBehaviour component)
// CombatApplication → as ICombatService
// PlayerSpawner
// EnemySpawner
// LootSpawner
// ProjectileSpawner
// SceneController
// [Config SO và Prefab refs kéo vào Inspector slots của SceneLifetimeScope]
// KHÔNG register: EntityState, EntityApplication, EntityPresenter, EntityRuntime
```

### VContainer wiring — SceneLifetimeScope cha/con
```csharp
// SceneLifetimeScope.Parent = ProjectLifetimeScope
// → SceneScope thấy được mọi service của ProjectScope
// → ProjectScope KHÔNG thấy SceneScope services (đúng — tránh scene leak)
```

### [Inject] trên MonoBehaviour
```csharp
// Dùng [Inject] attribute (using VContainer) thay vì constructor injection
// cho MonoBehaviour được gắn trong scene hoặc prefab runtime.
// VContainer resolve [Inject] sau khi container build xong.
```

---

## 12. CROSS-MODULE COMMUNICATION — 2 GIAI ĐOẠN

### Giai đoạn 1 (Slice 00 → 02): SharedPorts + Direct call qua SceneController

```
PlayerPresenter
  → inject IInputCommandSource (SharedPorts)       [Input → Player]
  → inject ICombatService (SharedPorts)             [Player → Combat]

SceneController (mediator)
  → nhận EnemyPresenter.OnDied callback
  → gọi LootSpawner.Spawn(rewardBundle, position)  [Enemy → Loot: qua SceneController]
  → gọi IRewardGrantService.Grant(bundle)           [Enemy → Economy: qua SceneController]

LootItemBinder
  → inject IInventoryWriteService (SharedPorts)     [Loot → Inventory]
```

**Không có module nào gọi trực tiếp sang namespace nội bộ của module khác.**

### Giai đoạn 2 (từ Slice 03): Thêm MessagePipe cho cross-module events

```csharp
// Ví dụ: Enemy chết → publish EnemyDiedMessage (thay cho callback qua SceneController)
// MessagePipe setup trong ProjectLifetimeScope:
builder.RegisterMessagePipe();
builder.RegisterMessageBroker<EnemyDiedMessage>();

// EnemyPresenter (publisher):
[Inject] IPublisher<EnemyDiedMessage> _publisher;
// OnDied: _publisher.Publish(new EnemyDiedMessage(rewardBundle, position));

// LootSpawner (subscriber):
[Inject] ISubscriber<EnemyDiedMessage> _subscriber;
// Awake/Start: _subscriber.Subscribe(msg => Spawn(msg.Bundle, msg.Position));
```

**EnemyDiedMessage.cs nằm ở đâu?**
→ SharedPorts, không phải Core (message có context game, không phải universal type).

---

## 13. MESSAGEPIPE — QUY TẮC SỬ DỤNG

| Được phép | Không được phép |
|-----------|-----------------|
| Dùng từ Slice 03 trở đi | Dùng trước Slice 03 |
| Cross-module events (EnemyDied, ItemPickedUp) | Thay thế direct call trong cùng module |
| Cross-scene events (GameOver, SceneTransition) | Làm MVP backbone thay thế toàn bộ direct call |
| Global events (OnGoldChanged cho UI) | Tạo event cho mọi thứ nhỏ nhặt |

**Message types nằm ở:** `SharedPorts/Messages/` — không phải Core.

```
SharedPorts/
└── Messages/
    ├── EnemyDiedMessage.cs
    ├── ItemPickedUpMessage.cs
    ├── SceneTransitionMessage.cs
    └── GameOverMessage.cs
```

---

## 14. APPROVED GAME FLOW CONTRACTS

### 14A. Melee Combat Flow
```
PlayerPresenter nhận AttackCommand từ IInputCommandSource
  → ICombatService.RequestAttack(playerId, target_IDamageReceiver, weaponId)
    → CombatApplication tính DamageInfo (damage, crit, source)
    → IDamageReceiver.ReceiveDamage(damageInfo) trên target
      → EnemyApplication.ReceiveDamage() → mutate CurrentHealth
        → nếu HP ≤ 0: IsDead = true, emit OnDied(EntityId)
```

### 14B. Ranged Combat Flow
```
PlayerPresenter nhận AttackCommand (ranged weapon)
  → ICombatService.RequestRangedAttack(playerId, dirX, dirY, weaponId)
    → ProjectileSpawner.Spawn(projDef, origin, dir)
      → ProjectilePresenter.OnTriggerEnter2D
        → GetComponent<IDamageReceiver>() trên collided object
        → ICombatService.ResolveProjectileHit(playerId, receiver, projectileId)
          → (cùng luồng Melee từ bước CombatApplication trở đi)
```

### 14C. Death → Reward → Loot Flow
```
EnemyApplication OnDied event → EnemyPresenter lắng nghe
  → EnemyPresenter gọi _onDiedCallback(rewardBundle, worldPosition)
    [callback được wiring bởi SceneController khi spawn Enemy]

SceneController.HandleEnemyDied(bundle, position):
  → IRewardGrantService.Grant(bundle)        // exp+gold cộng ngay
  → LootSpawner.Spawn(bundle, position)      // spawn vật phẩm vật lý

  [Từ Slice 03+: thay bằng MessagePipe publish EnemyDiedMessage]
```

### 14D. Player Pickup Loot Flow
```
PlayerView.OnTriggerEnter2D(Collider2D col)
  → forward sang PlayerPresenter.OnTriggerEnter(col)
    → col.GetComponent<IInteractable>()
    → IInteractable.Interact()
      → LootItemBinder.Interact()
        → IInventoryWriteService.AddItem(itemStack)
        → _pickedUp = true → Destroy(gameObject)
```

### 14E. Input Context Switch Flow
```
// Khi Player vào xe:
InputReader.SwitchContext(InputContext.Vehicle)
  → CommandBuffer.Clear()           // flush pending player commands
  → _currentContext = Vehicle
  → VehicleInputContext bắt đầu đọc VehicleMap actions

// Khi Player rời xe:
InputReader.SwitchContext(InputContext.Player)
```

---

## 15. RULES — DANH SÁCH ĐẦY ĐỦ

| ID | Rule | Vi phạm dẫn đến |
|----|------|-----------------|
| R01 | Không dùng Find(), FindObjectOfType(), scene locator | Invisible coupling |
| R02 | Không dùng Singleton cho gameplay systems | Hidden state, test nightmare |
| R03 | MonoBehaviour callback chỉ forward → Presenter | Logic rò rỉ vào Unity layer |
| R04 | Không register EntityState / EntityApplication / Presenter / Runtime vào DI scope | Entity lifetime bị sai |
| R05 | Không tạo abstraction/interface trước khi có 2 implementation thực sự cần | Premature abstraction |
| R06 | Không tham chiếu namespace nội bộ của module khác | Cross-module coupling |
| R07 | Cross-module chỉ qua SharedPorts hoặc MessagePipe (Slice 03+) | Phá vỡ isolation |
| R08 | Core không ref bất kỳ module hay UnityEngine.InputSystem | Core phình to |
| R09 | SharedPorts không ref bất kỳ Module asmdef | Circular dep |
| R10 | ScriptableObject chỉ chứa config data, không runtime state | State lost on reload |
| R11 | Không tạo EntityBase<T>, ManagerBase<T>, global event bus tùy tiện | Premature generalization |
| R12 | Mọi dependency phải hiện qua constructor hoặc [Inject] attribute | Hidden dep |
| R13 | Không dùng MessagePipe trước Slice 03 | Foundation chưa chắc |
| R14 | Core và Composition chỉ do integrator chỉnh sửa | Concurrent edit chaos |
| R15 | Domain và Application không có UnityEngine types | Layer violation |
| R16 | Không Enqueue Command từ Application layer — chỉ từ Infrastructure (InputReader) | Wrong direction |
| R17 | `[Inject]` trên MonoBehaviour prefab runtime: instantiate qua `container.Instantiate()` | [Inject] không được resolve |
| R18 | EntityId.New() chỉ gọi tại Spawner — không gọi trong Application constructor | ID generation centralized |

---

## 16. BUILD ORDER & CHECKLIST

### Phase 0 — Foundation (prerequisite của mọi thứ)
- [ ] Package: VContainer, UniTask, DOTween, MessagePipe cài đủ
- [ ] Project Settings → Input: Enable New Input System, restart
- [ ] Tạo asmdef: Core, SharedPorts, Composition, Editor
- [ ] Tạo toàn bộ Core files (EntityId, DamageInfo, DamageResult, IDamageReceiver, IInteractable, ItemStack, RewardBundle, ISaveSnapshotProvider, ISaveSnapshotConsumer)
- [ ] Tạo toàn bộ SharedPorts files (IInputCommandSource, InputContext, ICombatService, IInventoryReadService, IInventoryWriteService, IWalletService, IRewardGrantService, IPlayerReadService)
- [ ] ProjectLifetimeScope.cs, SceneLifetimeScope.cs, GameBootstrapper.cs
- [ ] Play scene → VContainer không báo lỗi → Phase 0 done

### Phase 1 — Slice 01: Input
- [ ] Asmdef: BillGameCore.Modules.Input (refs: Core, SharedPorts)
- [ ] ICommand.cs, CommandType.cs
- [ ] MoveCommand, AttackCommand, InteractCommand, SwitchContextCommand
- [ ] CommandBuffer.cs
- [ ] InputCommandDispatcher.cs (implement IInputCommandSource)
- [ ] InputReader.cs (MonoBehaviour, [Inject] CommandBuffer)
- [ ] InputActions.inputactions: 3 ActionMaps (Player/Vehicle/UI)
- [ ] SceneLifetimeScope: register CommandBuffer, InputCommandDispatcher as IInputCommandSource, InputReader component
- [ ] Play → log MoveCommand.DirX khi nhấn phím → Slice 01 done

### Phase 2 — Slice 02: Player
- [ ] Asmdef: BillGameCore.Modules.Player (refs: Core, SharedPorts, Modules.Input)
- [ ] PlayerDefinition.cs, PlayerState.cs (pure C#)
- [ ] PlayerApplication.cs (IDamageReceiver, IPlayerReadService)
- [ ] PlayerView.cs (MonoBehaviour, forward callbacks)
- [ ] PlayerPresenter.cs (inject IInputCommandSource từ SharedPorts)
- [ ] PlayerRuntime.cs (IDisposable)
- [ ] PlayerSpawner.cs (registered SceneLifetimeScope)
- [ ] PlayerConfig.cs (ScriptableObject)
- [ ] SceneLifetimeScope: register PlayerSpawner, config ref, prefab ref
- [ ] GameBootstrapper.Start(): gọi PlayerSpawner.Spawn()
- [ ] Play → Player di chuyển theo input → Slice 02 done

### Phase 3 — Slice 03: CombatGroup (Combat + Projectile) + MessagePipe unlock
- [ ] Asmdef: BillGameCore.Modules.CombatGroup (refs: Core, SharedPorts)
- [ ] WeaponDefinition.cs, ProjectileDefinition.cs, ProjectileState.cs
- [ ] CombatApplication.cs (implement ICombatService)
- [ ] ProjectileApplication, ProjectileView, ProjectilePresenter, ProjectileSpawner
- [ ] SceneLifetimeScope: register CombatApplication as ICombatService, ProjectileSpawner
- [ ] **MessagePipe unlock:** register IPublisher/ISubscriber trong ProjectLifetimeScope
- [ ] Play → Player attack → Enemy nhận damage → Slice 03 done

### Phase 4 — Slice 04: EnemyGroup (Enemy + Loot)
- [ ] Asmdef: BillGameCore.Modules.EnemyGroup (refs: Core, SharedPorts)
- [ ] EnemyDefinition.cs, EnemyState.cs (AIState enum: Idle/Patrol/Chase/Attack)
- [ ] EnemyApplication.cs (IDamageReceiver, state machine, emit OnDied)
- [ ] EnemyView, EnemyPresenter, EnemyRuntime, EnemySpawner
- [ ] LootItemBinder.cs (IInteractable, [Inject] IInventoryWriteService)
- [ ] LootSpawner.cs (dùng container.Instantiate cho prefab có [Inject])
- [ ] EnemyConfig.cs, LootItemConfig.cs
- [ ] SceneLifetimeScope: register EnemySpawner, LootSpawner
- [ ] SceneController: wire EnemyPresenter.OnDied → HandleEnemyDied
- [ ] Play → Enemy chết → Loot spawn → Player nhặt → Inventory có item → Slice 04 done

### Phase 5 — Slice 05: InventoryGroup (Inventory + Economy)
- [ ] Asmdef: BillGameCore.Modules.InventoryGroup (refs: Core, SharedPorts)
- [ ] InventoryService.cs (IInventoryReadService, IInventoryWriteService, ISaveSnapshotProvider/Consumer)
- [ ] EconomyService.cs (IWalletService, IRewardGrantService)
- [ ] ProjectLifetimeScope: register InventoryService, EconomyService
- [ ] Play → verify DI chain: LootItemBinder → IInventoryWriteService → InventoryService (ProjectScope)

### Phase 6 — Slice 06: InteractionGroup
- [ ] Asmdef: BillGameCore.Modules.InteractionGroup (refs: Core, SharedPorts)
- [ ] ChestBinder, DoorBinder, HealPointBinder (mỗi cái 1 thư mục con)
- [ ] Prefab cho từng loại: Collider2D IsTrigger=true
- [ ] Play → Player chạm Chest → inventory nhận item

### Phase 7 — Slice 07: Save
- [ ] Asmdef: BillGameCore.Modules.Save (refs: Core, SharedPorts)
- [ ] SaveService: thu thập snapshot từ mọi ISaveSnapshotProvider, serialize JSON
- [ ] ProjectLifetimeScope: register SaveService
- [ ] Test: SaveService.Save() → file JSON trong persistentDataPath

### Phase 8 — Slice 08: UI
- [ ] Asmdef: BillGameCore.Modules.UI (refs: Core, SharedPorts)
- [ ] HUDPresenter (inject IPlayerReadService, IWalletService)
- [ ] InventoryPanelPresenter (inject IInventoryReadService)
- [ ] SceneLifetimeScope: register UI presenters
- [ ] Play → HP bar cập nhật đúng khi player nhận damage

---

## 17. FAQ

**Q: Player cần đọc Input — tại sao không để Player ref Module Input trực tiếp?**
A: Player ref `IInputCommandSource` từ SharedPorts. Sau này nếu thay InputReader bằng AI controller hay Replay system, không cần sửa PlayerPresenter. Đây chính là lý do SharedPorts tồn tại.

**Q: Module asmdef chung (CombatGroup) có được phép gọi thẳng nhau không?**
A: Có — Combat và Projectile trong cùng `BillGameCore.Modules.CombatGroup.asmdef` được phép ref nhau vì coupling của chúng là *có chủ đích theo thiết kế*. Đây là trade-off đã chốt ở ADR-05.

**Q: EnemyGroup có Loot trong đó — Loot cần IInventoryWriteService, Enemy không cần. Có vấn đề không?**
A: Không. EnemyGroup asmdef ref SharedPorts (có IInventoryWriteService). LootItemBinder dùng [Inject] để nhận implementation từ DI container. Enemy hoàn toàn không biết IInventoryWriteService.

**Q: MessagePipe message type đặt ở Core hay SharedPorts?**
A: SharedPorts. Core không có context game logic. Message types như `EnemyDiedMessage` có RewardBundle, Vector2 position — đây là game context, thuộc SharedPorts/Messages/.

**Q: Khi nào một interface mới được thêm vào SharedPorts?**
A: Khi có ≥2 module ở asmdef khác nhau cần communicate qua nó. Interface chỉ trong một asmdef thì để trong `Application/Ports/` của asmdef đó.

**Q: ScriptableObject config có được dùng [CreateAssetMenu] không?**
A: Có, bắt buộc. Nhưng chỉ được store config fields (serialized readonly data). Không có mutable fields, không có runtime state, không có `[NonSerialized]` state fields ẩn.

**Q: `container.Instantiate()` vs `Object.Instantiate()` — khi nào dùng cái nào?**
A: `container.Instantiate()`: khi prefab có `[Inject]` trên component và cần DI resolve. `Object.Instantiate()`: khi prefab không cần DI (chỉ có View/Presenter được wired thủ công bởi Spawner). Sai loại → [Inject] không được gọi → NullReferenceException runtime.

---

*CONTEXT v2.0 — Final. Không còn điểm nhập nhằng. Mọi quyết định đã được chốt với lý do rõ ràng.*
*Khi cần thêm pattern mới: tạo ADR mới, không sửa ngầm existing rules.*
