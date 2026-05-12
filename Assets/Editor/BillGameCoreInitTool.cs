// ============================================================
//  BillGameCoreInitTool.cs  —  v2.0
//  Aligned 1-to-1 with CONTEXT_v2_0.md
//
//  Menu paths
//  ──────────────────────────────────────────────────────────
//  BillGameCore / Initialize Project Structure
//  BillGameCore / New Module / Input  (Full Command Pattern)
//  BillGameCore / New Module / Entity (Player, Enemy …)
//  BillGameCore / New Module / Interaction (Chest, Door …)
//  BillGameCore / New Module / System (Inventory, Save …)
//  BillGameCore / New Module / Add Interaction Type to Group
//
//  Architecture contract (CONTEXT_v2_0.md — abridged)
//  ──────────────────────────────────────────────────────────
//  Core         – pure C# value-types & interfaces, no Unity, no logic
//  SharedPorts  – narrow interfaces only; refs Core only; NO module refs
//  Modules.*    – vertical slices; refs Core + SharedPorts only (cross-asmdef)
//  Composition  – integrator-owned; refs everything; wires DI scopes
//  Scenes       – SceneController mediator
//
//  Asmdef grouping (ADR-05)
//  ──────────────────────────────────────────────────────────
//  BillGameCore.Modules.Input            Input (Command Pattern)
//  BillGameCore.Modules.Player           Player slice
//  BillGameCore.Modules.CombatGroup      Combat + Projectile
//  BillGameCore.Modules.EnemyGroup       Enemy + Loot
//  BillGameCore.Modules.InventoryGroup   Inventory + Economy
//  BillGameCore.Modules.InteractionGroup Chest / Door / HealPoint / Trap …
//  BillGameCore.Modules.Save             Save
//  BillGameCore.Modules.Audio            Audio
//  BillGameCore.Modules.UI               HUD + InventoryPanel
//
//  Key rules enforced by generated code
//  ──────────────────────────────────────────────────────────
//  R03  MonoBehaviour callbacks → forward to Presenter only
//  R04  Entity State/App/Presenter/Runtime NEVER in DI scope
//  R07  Cross-module only via SharedPorts or MessagePipe (Slice 03+)
//  R09  SharedPorts refs Core only — never a Module asmdef
//  R10  ScriptableObject = config data only
//  R15  Domain & Application = pure C#, no UnityEngine types
//  R16  CommandBuffer.Enqueue only from Infrastructure (InputReader)
//  R17  Prefabs needing [Inject] → container.Instantiate()
//  R18  EntityId.New() only in Spawner
// ============================================================

using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class BillGameCoreInitTool
{
    private const string Root = "Assets/_Game";
    private const string PendingLogKey = "BillGameCore_PendingLog";

    // ═══════════════════════════════════════════════════════
    //  MENU ITEMS
    // ═══════════════════════════════════════════════════════

    [MenuItem("BillGameCore/Initialize Project Structure")]
    public static void InitializeProject()
    {
        CreateBaseDirectories();
        WriteAllCoreFiles();
        WriteAllSharedPortsFiles();
        WriteAllCompositionFiles();
        WriteScenesFile();
        WriteAllAsmdefs();
        AssetDatabase.Refresh();
        QueueLog("<color=cyan><b>[BillGameCore v2.0]</b></color> <color=green>Foundation scaffold done. " +
                 "Open CONTEXT_v2_0.md → Phase 0 checklist before starting any feature slice.</color>");
    }

    [MenuItem("BillGameCore/New Module/Input  (Full Command Pattern)")]
    public static void NewInputModule()
    {
        CreateInputModule();
        AssetDatabase.Refresh();
        //StepsWindow.Show("Input Module — Integration Steps", BuildInputSteps());
    }

    [MenuItem("BillGameCore/New Module/Entity  (Player, Enemy, Projectile ...)")]
    public static void NewEntityModule()
    {
        string name = InputDialog.Show("New Entity Module",
            "Module name  (e.g. Player | Enemy | Projectile):", "Player");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateEntityModule(name);
        AssetDatabase.Refresh();
        //StepsWindow.Show($"Entity '{name}' — Integration Steps", BuildEntitySteps(name));
    }

    [MenuItem("BillGameCore/New Module/Interaction  (Chest, Door, HealPoint, Trap ...)")]
    public static void NewInteractionModule()
    {
        string name = InputDialog.Show("New Interaction Module",
            "First interaction type  (e.g. Chest | Door | HealPoint):", "Chest");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateInteractionGroupModule(name);
        AssetDatabase.Refresh();
        //StepsWindow.Show($"InteractionGroup '{name}' — Integration Steps", BuildInteractionSteps(name));
    }

    [MenuItem("BillGameCore/New Module/Add Interaction Type to Group")]
    public static void AddInteractionType()
    {
        string name = InputDialog.Show("Add Interaction Type",
            "Type name to add  (e.g. Door | HealPoint | Trap):", "Door");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateInteractionType(name);
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>[BillGameCore]</color> Added interaction type '{name}' to InteractionGroup.");
    }

    [MenuItem("BillGameCore/New Module/System  (Inventory, Save, Audio ...)")]
    public static void NewSystemModule()
    {
        string name = InputDialog.Show("New System Module",
            "Module name  (e.g. Inventory | Economy | Audio | Save):", "Inventory");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateSystemModule(name);
        AssetDatabase.Refresh();
        //StepsWindow.Show($"System '{name}' — Integration Steps", BuildSystemSteps(name));
    }

    // ═══════════════════════════════════════════════════════
    //  FOUNDATION — directories
    // ═══════════════════════════════════════════════════════

    static void CreateBaseDirectories()
    {
        string[] dirs =
        {
            "Art",
            "Data/Items",
            "Data/Settings",
            "Scripts/Core/ValueObjects",
            "Scripts/Core/Combat",
            "Scripts/Core/Interaction",
            "Scripts/Core/Inventory",
            "Scripts/Core/Rewards",
            "Scripts/Core/Save",
            "Scripts/SharedPorts/Input",
            "Scripts/SharedPorts/Combat",
            "Scripts/SharedPorts/Inventory",
            "Scripts/SharedPorts/Economy",
            "Scripts/SharedPorts/Player",
            "Scripts/SharedPorts/Messages",
            "Scripts/Composition",
            "Scripts/Modules",
            "Scripts/Scenes",
            "Scripts/Editor",
        };
        foreach (string d in dirs) EnsureDir(Path.Combine(Root, d));
    }

    // ═══════════════════════════════════════════════════════
    //  CORE FILES  (pure C# — no UnityEngine, no logic)
    // ═══════════════════════════════════════════════════════

    static void WriteAllCoreFiles()
    {
        Write("Scripts/Core/ValueObjects/EntityId.cs",
@"namespace BillGameCore.Core.ValueObjects
{
    // ADR-04: Guid — unique, no static counter, safe for parallel spawn.
    // R18: EntityId.New() called ONLY from Spawner classes.
    public readonly struct EntityId : System.IEquatable<EntityId>
    {
        public static readonly EntityId Invalid = new EntityId(System.Guid.Empty);

        /// <summary>R18: call only from Spawner.Spawn().</summary>
        public static EntityId New() => new EntityId(System.Guid.NewGuid());

        private EntityId(System.Guid value) { Value = value; }

        public System.Guid Value   { get; }
        public bool        IsValid => Value != System.Guid.Empty;

        public bool   Equals(EntityId other)        => Value == other.Value;
        public override bool Equals(object obj)     => obj is EntityId e && Equals(e);
        public override int  GetHashCode()          => Value.GetHashCode();
        public override string ToString()           => Value.ToString(""N"").Substring(0, 8);

        public static bool operator ==(EntityId a, EntityId b) =>  a.Equals(b);
        public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);
    }
}");

        Write("Scripts/Core/Combat/DamageInfo.cs",
@"using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Core.Combat
{
    // Built by CombatApplication. Passed to IDamageReceiver.ReceiveDamage().
    // Pure C# — NO UnityEngine references allowed in Core.
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
}");

        Write("Scripts/Core/Combat/DamageResult.cs",
@"namespace BillGameCore.Core.Combat
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
}");

        Write("Scripts/Core/Combat/IDamageReceiver.cs",
@"namespace BillGameCore.Core.Combat
{
    // Method name is ReceiveDamage — NEVER rename (CONTEXT contract).
    // Implemented by: EnemyApplication, PlayerApplication.
    // Called by: CombatApplication ONLY — never from Presenter or View.
    public interface IDamageReceiver
    {
        DamageResult ReceiveDamage(DamageInfo damage);
    }
}");

        Write("Scripts/Core/Interaction/IInteractable.cs",
@"namespace BillGameCore.Core.Interaction
{
    // Implemented by Binder classes (ChestBinder, LootItemBinder ...).
    // PlayerPresenter calls GetComponent<IInteractable>() on overlap.
    // PlayerPresenter NEVER knows the concrete Binder type (R07).
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
    }
}");

        Write("Scripts/Core/Inventory/ItemStack.cs",
@"namespace BillGameCore.Core.Inventory
{
    public readonly struct ItemStack
    {
        public ItemStack(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int    Amount { get; }
    }
}");

        Write("Scripts/Core/Rewards/RewardBundle.cs",
@"using BillGameCore.Core.Inventory;

namespace BillGameCore.Core.Rewards
{
    // Emitted by EnemyApplication.OnDied.
    // Consumed by SceneController → LootSpawner + IRewardGrantService.
    public sealed class RewardBundle
    {
        public int         Gold       = 0;
        public int         Experience = 0;
        public ItemStack[] Items      = System.Array.Empty<ItemStack>();
    }
}");

        Write("Scripts/Core/Save/ISaveSnapshotProvider.cs",
@"namespace BillGameCore.Core.Save
{
    public interface ISaveSnapshotProvider<out TSnapshot>
    {
        TSnapshot CreateSnapshot();
    }
}");

        Write("Scripts/Core/Save/ISaveSnapshotConsumer.cs",
@"namespace BillGameCore.Core.Save
{
    public interface ISaveSnapshotConsumer<in TSnapshot>
    {
        void RestoreSnapshot(TSnapshot snapshot);
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  SHARED PORTS  (refs Core only — R09)
    //
    //  ICommand and CommandType live HERE (not in Modules.Input) so that
    //  consumers (PlayerPresenter) only need to reference SharedPorts,
    //  never Modules.Input directly. Concrete command classes in
    //  Modules.Input.Commands implement SharedPorts.Input.ICommand.
    // ═══════════════════════════════════════════════════════

    static void WriteAllSharedPortsFiles()
    {
        // ── Input ─────────────────────────────────────────
        Write("Scripts/SharedPorts/Input/ICommand.cs",
@"using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    // Declared in SharedPorts so consumers need only ref SharedPorts.
    // Concrete classes (MoveCommand, AttackCommand ...) live in Modules.Input.Commands
    // and implement this interface.
    public interface ICommand
    {
        EntityId    SourceId  { get; }   // who produced this command (ADR-02 identity)
        CommandType Type      { get; }
        float       Timestamp { get; }   // Time.time when created
    }
}");

        Write("Scripts/SharedPorts/Input/CommandType.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // Placed in SharedPorts so consumers don't ref Modules.Input.
    // NEVER delete or renumber existing values (replay / save compatibility).
    public enum CommandType
    {
        Move          = 0,
        Attack        = 1,
        Interact      = 2,
        SwitchContext = 3,
    }
}");

        Write("Scripts/SharedPorts/Input/InputContext.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    public enum InputContext
    {
        None    = 0,
        Player  = 1,
        Vehicle = 2,
        UI      = 3,
    }
}");

        Write("Scripts/SharedPorts/Input/IInputCommandSource.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // Consumed by: PlayerPresenter, AI controllers.
    // Implemented by: InputCommandDispatcher (Modules.Input).
    // Registered in SceneLifetimeScope as IInputCommandSource.
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
        bool HasCommands { get; }
    }
}");

        // ── Combat ────────────────────────────────────────
        Write("Scripts/SharedPorts/Combat/ICombatService.cs",
@"using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Combat
{
    public interface ICombatService
    {
        /// <summary>Melee — resolves damage directly onto the receiver.</summary>
        void RequestAttack(EntityId attackerId, IDamageReceiver target, string weaponId);

        /// <summary>Ranged — spawns a projectile; damage resolved on collision.</summary>
        void RequestRangedAttack(EntityId attackerId, float dirX, float dirY, string weaponId);

        /// <summary>Called from ProjectilePresenter when the projectile hits a target.</summary>
        void ResolveProjectileHit(EntityId attackerId, IDamageReceiver target, string projectileId);
    }
}");

        // ── Inventory ─────────────────────────────────────
        Write("Scripts/SharedPorts/Inventory/IInventoryReadService.cs",
@"using BillGameCore.Core.Inventory;
using System.Collections.Generic;

namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryReadService
    {
        IReadOnlyList<ItemStack> GetItems();
        bool HasItem(string itemId, int minAmount = 1);
    }
}");

        Write("Scripts/SharedPorts/Inventory/IInventoryWriteService.cs",
@"using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryWriteService
    {
        bool AddItem(ItemStack stack);
        bool RemoveItem(string itemId, int amount);
    }
}");

        // ── Economy ───────────────────────────────────────
        Write("Scripts/SharedPorts/Economy/IWalletService.cs",
@"namespace BillGameCore.SharedPorts.Economy
{
    public interface IWalletService
    {
        int Gold       { get; }
        int Experience { get; }
    }
}");

        Write("Scripts/SharedPorts/Economy/IRewardGrantService.cs",
@"using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    // Called by SceneController after enemy dies — grants exp+gold immediately (no loot object).
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}");

        // ── Player ────────────────────────────────────────
        Write("Scripts/SharedPorts/Player/IPlayerReadService.cs",
@"namespace BillGameCore.SharedPorts.Player
{
    // Consumed by: UI/HUD. Implemented by: PlayerApplication.
    public interface IPlayerReadService
    {
        float CurrentHealth  { get; }
        float MaxHealth      { get; }
        float CurrentStamina { get; }
    }
}");

        // ── Messages  (Slice 03+ MessagePipe events) ──────
        Write("Scripts/SharedPorts/Messages/EnemyDiedMessage.cs",
@"using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Messages
{
    // Published by EnemyPresenter from Slice 03+ (MessagePipe unlock — ADR-03).
    // Consumed by LootSpawner and/or SceneController subscribers.
    public sealed class EnemyDiedMessage
    {
        public RewardBundle Bundle { get; }
        public float        WorldX { get; }
        public float        WorldY { get; }

        public EnemyDiedMessage(RewardBundle bundle, float worldX, float worldY)
        {
            Bundle = bundle;
            WorldX = worldX;
            WorldY = worldY;
        }
    }
}");

        Write("Scripts/SharedPorts/Messages/ItemPickedUpMessage.cs",
@"using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Messages
{
    // Published by LootItemBinder after a successful pickup (Slice 03+).
    public sealed class ItemPickedUpMessage
    {
        public ItemStack Stack { get; }
        public ItemPickedUpMessage(ItemStack stack) { Stack = stack; }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  COMPOSITION  (integrator-owned)
    // ═══════════════════════════════════════════════════════

    static void WriteAllCompositionFiles()
    {
        Write("Scripts/Composition/ProjectLifetimeScope.cs",
@"using VContainer;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // INTEGRATOR-OWNED — do not edit from a feature-slice task.
    // ADR-06: register services that must survive scene loads:
    //   InventoryService, EconomyService, SaveService, AudioService.
    // R04: NEVER register EntityState / EntityApplication / Presenter / Runtime here.
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // ── Add registrations below as slices are merged ──────────────────

            // Slice 05 — InventoryGroup:
            // builder.Register<InventoryService>(Lifetime.Singleton)
            //        .As<IInventoryReadService, IInventoryWriteService>();

            // Slice 05 — EconomyService:
            // builder.Register<EconomyService>(Lifetime.Singleton)
            //        .As<IWalletService, IRewardGrantService>();

            // Slice 07 — Save:
            // builder.Register<SaveService>(Lifetime.Singleton);

            // Slice 08 — Audio:
            // builder.Register<AudioService>(Lifetime.Singleton);

            // Slice 03+ — MessagePipe unlock (ADR-03):
            // builder.RegisterMessagePipe();
            // builder.RegisterMessageBroker<EnemyDiedMessage>();
            // builder.RegisterMessageBroker<ItemPickedUpMessage>();
        }
    }
}");

        Write("Scripts/Composition/SceneLifetimeScope.cs",
@"using VContainer;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // INTEGRATOR-OWNED — do not edit from a feature-slice task.
    // Register: spawners, scene controllers, MonoBehaviour components, config SO refs.
    // Parent = ProjectLifetimeScope so SceneScope resolves ProjectScope services.
    // R04: NEVER register EntityState / EntityApplication / Presenter / Runtime here.
    public sealed class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameBootstrapper>();

            // ── Add registrations below as slices are merged ──────────────────

            // Slice 01 — Input:
            // builder.Register<CommandBuffer>(Lifetime.Singleton);
            // builder.Register<InputCommandDispatcher>(Lifetime.Singleton).As<IInputCommandSource>();
            // builder.RegisterComponent(inputReaderRef);   // MonoBehaviour on scene GO

            // Slice 02 — Player:
            // builder.Register<PlayerSpawner>(Lifetime.Scoped);
            // builder.RegisterInstance(playerConfigRef);   // SO asset dragged into Inspector slot
            // builder.RegisterInstance(playerViewPrefabRef);

            // Slice 03 — CombatGroup:
            // builder.Register<CombatApplication>(Lifetime.Scoped).As<ICombatService>();
            // builder.Register<ProjectileSpawner>(Lifetime.Scoped);

            // Slice 04 — EnemyGroup:
            // builder.Register<EnemySpawner>(Lifetime.Scoped);
            // builder.Register<LootSpawner>(Lifetime.Scoped); // R17: uses container.Instantiate

            // Scenes:
            // builder.RegisterComponent(sceneControllerRef);
        }
    }
}");

        Write("Scripts/Composition/GameBootstrapper.cs",
@"using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // Scene entry point — constructor injection via VContainer.
    // Add dependencies and startup calls as slices are merged.
    public sealed class GameBootstrapper : IStartable
    {
        // Example (Slice 02):
        // private readonly PlayerSpawner _playerSpawner;
        // private readonly InputReader   _inputReader;
        //
        // public GameBootstrapper(PlayerSpawner playerSpawner, InputReader inputReader)
        // {
        //     _playerSpawner = playerSpawner;
        //     _inputReader   = inputReader;
        // }

        public void Start()
        {
            Debug.Log(""[GameBootstrapper] Scene started."");

            // Example (Slice 02):
            // var runtime = _playerSpawner.Spawn(Vector2.zero);
            // _inputReader.SetControlledEntity(runtime.Id);
        }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  SCENES
    // ═══════════════════════════════════════════════════════

    static void WriteScenesFile()
    {
        Write("Scripts/Scenes/SceneController.cs",
@"using BillGameCore.Core.Rewards;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;
using VContainer;

// Slice 03+: using BillGameCore.SharedPorts.Messages;
// Slice 03+: using MessagePipe;

namespace BillGameCore.Scenes
{
    // Mediator — wires cross-module events using direct calls (Phase 1).
    // From Slice 03+: replace direct callbacks with MessagePipe publish/subscribe.
    // R01: No Find() or FindObjectOfType() — all refs injected via [Inject] or constructor.
    public sealed class SceneController : MonoBehaviour
    {
        [Inject] private IRewardGrantService _rewardGrant;
        // [Inject] private LootSpawner _lootSpawner;  // add when Slice 04 ready

        // Called by EnemyPresenter callback (Phase 1) — or via MessagePipe subscriber (Phase 2).
        public void HandleEnemyDied(RewardBundle bundle, float worldX, float worldY)
        {
            _rewardGrant?.Grant(bundle);
            // _lootSpawner?.Spawn(bundle, new UnityEngine.Vector2(worldX, worldY));
        }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  ASMDEFS
    // ═══════════════════════════════════════════════════════

    static void WriteAllAsmdefs()
    {
        // Core: no Unity.InputSystem, no module refs (R08)
        WriteAsmdef("Scripts/Core", "BillGameCore.Core",
            new string[0], editorOnly: false);

        // SharedPorts: refs Core only (R09)
        WriteAsmdef("Scripts/SharedPorts", "BillGameCore.SharedPorts",
            new[] { "BillGameCore.Core" }, editorOnly: false);

        // Composition: integrator — refs grow as slices merge
        WriteAsmdef("Scripts/Composition", "BillGameCore.Composition",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts",
                    "VContainer", "VContainer.Unity" }, editorOnly: false);

        // Scenes: SceneController mediator
        WriteAsmdef("Scripts/Scenes", "BillGameCore.Scenes",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts",
                    "BillGameCore.Composition", "VContainer" }, editorOnly: false);

        // Editor tool
        WriteAsmdef("Scripts/Editor", "BillGameCore.Editor",
            new[] { "BillGameCore.Core" }, editorOnly: true);
    }

    // ═══════════════════════════════════════════════════════
    //  INPUT MODULE  (Full Command Pattern — ADR-02)
    // ═══════════════════════════════════════════════════════

    static void CreateInputModule()
    {
        string[] dirs =
        {
            "Scripts/Modules/Input/Commands",
            "Scripts/Modules/Input/Context",
            "Scripts/Modules/Input/Application",
            "Scripts/Modules/Input/Infrastructure",
        };
        foreach (string d in dirs) EnsureDir(Path.Combine(Root, d));

        // Commands — implement SharedPorts.Input.ICommand ─────
        Write("Scripts/Modules/Input/Commands/MoveCommand.cs",
@"using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

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
        public float       DirX      { get; }
        public float       DirY      { get; }
        public bool        IsMoving  => DirX != 0f || DirY != 0f;
    }
}");

        Write("Scripts/Modules/Input/Commands/AttackCommand.cs",
@"using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class AttackCommand : ICommand
    {
        public AttackCommand(EntityId sourceId, float timestamp,
                             bool isHeld = false, float heldDuration = 0f)
        {
            SourceId     = sourceId;
            Timestamp    = timestamp;
            IsHeld       = isHeld;
            HeldDuration = heldDuration;
        }

        public EntityId    SourceId     { get; }
        public CommandType Type         => CommandType.Attack;
        public float       Timestamp    { get; }
        public bool        IsHeld       { get; }   // true while button held
        public float       HeldDuration { get; }   // seconds held so far
    }
}");

        Write("Scripts/Modules/Input/Commands/InteractCommand.cs",
@"using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class InteractCommand : ICommand
    {
        public InteractCommand(EntityId sourceId, float timestamp)
        {
            SourceId  = sourceId;
            Timestamp = timestamp;
        }

        public EntityId    SourceId  { get; }
        public CommandType Type      => CommandType.Interact;
        public float       Timestamp { get; }
    }
}");

        Write("Scripts/Modules/Input/Commands/SwitchContextCommand.cs",
@"using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class SwitchContextCommand : ICommand
    {
        public SwitchContextCommand(EntityId sourceId, InputContext targetContext, float timestamp)
        {
            SourceId      = sourceId;
            TargetContext = targetContext;
            Timestamp     = timestamp;
        }

        public EntityId     SourceId      { get; }
        public CommandType  Type          => CommandType.SwitchContext;
        public float        Timestamp     { get; }
        public InputContext  TargetContext { get; }
    }
}");

        Write("Scripts/Modules/Input/Commands/CommandBuffer.cs",
@"using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Thread-safe FIFO.
    // R16: Only InputReader (Infrastructure) is allowed to call Enqueue().
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue   = new Queue<ICommand>();
        private readonly object          _lock    = new object();
        private readonly int             _maxSize;

        public CommandBuffer(int maxSize = 32) { _maxSize = maxSize; }

        // R16: called ONLY from InputReader.
        public void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize) _queue.Dequeue(); // drop oldest on overflow
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

        // Called by InputReader.SwitchContext() to flush stale commands.
        public void Clear() { lock (_lock) _queue.Clear(); }
    }
}");

        // Application ─────────────────────────────────────
        Write("Scripts/Modules/Input/Application/InputCommandDispatcher.cs",
@"using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Registered in SceneLifetimeScope as IInputCommandSource.
    // Thin adapter: CommandBuffer → IInputCommandSource.
    // Consumer Presenters inject IInputCommandSource — never know InputReader exists.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer) { _buffer = buffer; }

        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
        public bool HasCommands                       => _buffer.HasCommands;
    }
}");

        // Context stubs ───────────────────────────────────
        Write("Scripts/Modules/Input/Context/PlayerInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    public static class PlayerInputContext  { public const string ActionMapName = ""Player"";  }
}");
        Write("Scripts/Modules/Input/Context/VehicleInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    public static class VehicleInputContext { public const string ActionMapName = ""Vehicle""; }
}");
        Write("Scripts/Modules/Input/Context/UIInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    public static class UIInputContext      { public const string ActionMapName = ""UI"";      }
}");

        // Infrastructure — the ONLY class allowed to call CommandBuffer.Enqueue() ─────
        Write("Scripts/Modules/Input/Infrastructure/InputReader.cs",
@"
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour — translates New Input System events into ICommand objects.
    // R03: No business logic — only raw-input → Command translation.
    // R16: This is the ONLY class allowed to call CommandBuffer.Enqueue().
    // R17: Registered via builder.RegisterComponent<InputReader>() — VContainer resolves [Inject].
    public sealed class InputReader : MonoBehaviour
    {
        [Inject] private CommandBuffer _buffer;            // injected by VContainer

        [SerializeField] private PlayerInput _playerInput; // assign in Inspector

        private EntityId     _controlledEntityId = EntityId.Invalid;
        private InputContext _currentContext      = InputContext.Player;

        // Hold-state for Attack
        private bool  _attackHeld;
        private float _attackHeldStart;

        // Called by GameBootstrapper after PlayerSpawner.Spawn() returns a Runtime.
        public void SetControlledEntity(EntityId id) => _controlledEntityId = id;

        public void SwitchContext(InputContext context)
        {
            _currentContext = context;
            _buffer.Clear(); // flush stale commands (CONTEXT 14E)
            _playerInput.SwitchCurrentActionMap(context switch
            {
                InputContext.Player  => PlayerInputContext.ActionMapName,
                InputContext.Vehicle => VehicleInputContext.ActionMapName,
                InputContext.UI      => UIInputContext.ActionMapName,
                _                   => PlayerInputContext.ActionMapName,
            });
        }

        private void Update()
        {
            if (!_controlledEntityId.IsValid) return;
            switch (_currentContext)
            {
                case InputContext.Player:  ReadPlayerMap();  break;
                case InputContext.Vehicle: ReadVehicleMap(); break;
            }
        }

        // R16: All Enqueue calls are inside this file only.
        private void ReadPlayerMap()
        {
            var mv = _playerInput.actions[""Player/Move""].ReadValue<Vector2>();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, mv.x, mv.y, Time.time));

            var atk = _playerInput.actions[""Player/Attack""];
            if (atk.WasPressedThisFrame()) { _attackHeld = true; _attackHeldStart = Time.time; }
            if (_attackHeld)
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time,
                                                  isHeld: true,
                                                  heldDuration: Time.time - _attackHeldStart));
            if (atk.WasReleasedThisFrame()) _attackHeld = false;

            if (_playerInput.actions[""Player/Interact""].WasPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }

        private void ReadVehicleMap()
        {
            // Implement Vehicle action reads here when Vehicle slice is built.
        }
    }
}");

        WriteAsmdef("Scripts/Modules/Input", "BillGameCore.Modules.Input",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts",
                    "VContainer", "Unity.InputSystem" }, editorOnly: false);
    }

    // ═══════════════════════════════════════════════════════
    //  ENTITY ARCHETYPE
    // ═══════════════════════════════════════════════════════

    static void CreateEntityModule(string n)
    {
        string[] dirs =
        {
            $"Scripts/Modules/{n}/Domain",
            $"Scripts/Modules/{n}/Application",
            $"Scripts/Modules/{n}/Application/Ports",
            $"Scripts/Modules/{n}/Presentation",
            $"Scripts/Modules/{n}/Infrastructure/Config",
        };
        foreach (string d in dirs) EnsureDir(Path.Combine(Root, d));
        Write($"Scripts/Modules/{n}/Application/Ports/.gitkeep", string.Empty);

        // Domain ─────────────────────────────────────────
        Write($"Scripts/Modules/{n}/Domain/{n}Definition.cs",
$@"namespace BillGameCore.Modules.{n}.Domain
{{
    // Static gameplay data. Populated by {n}Config.ToDefinition(). Immutable at runtime.
    // R15: Pure C# — NO UnityEngine types in Domain.
    [System.Serializable]
    public sealed class {n}Definition
    {{
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float MaxStamina = 100f;
        // Add entity-specific stats here.
    }}
}}");

        Write($"Scripts/Modules/{n}/Domain/{n}State.cs",
$@"namespace BillGameCore.Modules.{n}.Domain
{{
    // Mutable runtime state for ONE {n} instance.
    // Owned and mutated exclusively by {n}Application.
    // R04: NEVER registered in DI scope.
    // R10: NEVER stored in ScriptableObject.
    // R15: Pure C# — NO UnityEngine types.
    public sealed class {n}State
    {{
        public float CurrentHealth  {{ get; set; }}
        public float CurrentStamina {{ get; set; }}
        public float VelocityX      {{ get; set; }}
        public float VelocityY      {{ get; set; }}
        public bool  IsDead         {{ get; set; }}
        // Add entity-specific runtime fields here.
    }}
}}");

        // Application ─────────────────────────────────────
        Write($"Scripts/Modules/{n}/Application/{n}Application.cs",
$@"using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.{n}.Domain;

namespace BillGameCore.Modules.{n}.Application
{{
    // {n} use-case logic.
    // Implements IDamageReceiver — CombatApplication calls ReceiveDamage() via shared contract.
    // R15: NO MonoBehaviour, Transform, Animator, Rigidbody2D, ScriptableObject here.
    // R07: NO direct reference to other module namespaces — use SharedPorts contracts only.
    public sealed class {n}Application : IDamageReceiver
    {{
        private readonly {n}Definition _def;
        private readonly {n}State      _state;

        // C# event — Presenter listens and notifies SceneController (Phase 1 callback / Phase 2 MessagePipe).
        public event Action<EntityId> OnDied;

        // R18: EntityId provided by Spawner — never call EntityId.New() here.
        public EntityId Id {{ get; }}

        public {n}Application(EntityId id, {n}Definition def, {n}State state)
        {{
            Id     = id;
            _def   = def;
            _state = state;
            _state.CurrentHealth  = def.MaxHealth;
            _state.CurrentStamina = def.MaxStamina;
        }}

        public float CurrentHealth  => _state.CurrentHealth;
        public float CurrentStamina => _state.CurrentStamina;
        public float VelocityX      => _state.VelocityX;
        public float VelocityY      => _state.VelocityY;
        public bool  IsDead         => _state.IsDead;

        // Called by Presenter each frame with direction values from the command queue.
        public void Tick(float dirX, float dirY, float deltaTime)
        {{
            if (_state.IsDead) return;
            _state.VelocityX = dirX * _def.MoveSpeed;
            _state.VelocityY = dirY * _def.MoveSpeed;
        }}

        // IDamageReceiver — called by CombatApplication ONLY (CONTEXT Section 14A).
        public DamageResult ReceiveDamage(DamageInfo damage)
        {{
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {{
                _state.IsDead = true;
                OnDied?.Invoke(Id);
            }}

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }}
    }}
}}");

        // Presentation ─────────────────────────────────────
        Write($"Scripts/Modules/{n}/Presentation/{n}View.cs",
$@"using UnityEngine;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Unity-facing visual output for {n}.
    // R03: MonoBehaviour callbacks ONLY forward to Presenter — ZERO business logic here.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class {n}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        private Rigidbody2D  _rb;
        private {n}Presenter _presenter;

        private void Awake() => _rb = GetComponent<Rigidbody2D>(); // self-GetComponent is allowed

        // Called once by {n}Spawner immediately after Instantiate.
        public void Bind({n}Presenter presenter) => _presenter = presenter;

        // R03: forward only ──────────────────────────────
        private void Update()                         => _presenter?.OnUpdate(Time.deltaTime);
        private void FixedUpdate()                    => _presenter?.OnFixedUpdate();
        private void OnTriggerEnter2D(Collider2D col) => _presenter?.OnTriggerEnter2D(col);

        // View write methods — called by Presenter only ──
        public void SetVelocity(float vx, float vy)
        {{
            if (_rb) _rb.linearVelocity = new Vector2(vx, vy);
        }}

        public void UpdateMoveAnimation(float dirX, float dirY)
        {{
            if (!_animator) return;
            _animator.SetFloat(""MoveX"", dirX);
            _animator.SetFloat(""MoveY"", dirY);
            _animator.SetFloat(""Speed"",  new Vector2(dirX, dirY).sqrMagnitude);
        }}

        public void PlayDeath() {{ if (_animator) _animator.SetTrigger(""Die""); }}
    }}
}}");

        Write($"Scripts/Modules/{n}/Presentation/{n}Presenter.cs",
$@"using System;
using BillGameCore.Core.Interaction;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Bridges IInputCommandSource → {n}Application → {n}View each frame.
    // Constructed by {n}Spawner — NOT by DI container (R04).
    public sealed class {n}Presenter : IDisposable
    {{
        private readonly {n}Application  _app;
        private readonly {n}View         _view;
        private readonly IInputCommandSource _input;

        private bool  _deathPlayed;
        private float _lastDirX, _lastDirY;

        // Wired by SceneController so Presenter has no knowledge of LootSpawner etc.
        public Action<EntityId> OnDiedCallback;

        public {n}Presenter({n}Application app, {n}View view, IInputCommandSource input)
        {{
            _app   = app;
            _view  = view;
            _input = input;
            _app.OnDied += HandleDied;
        }}

        // Called from {n}View.Update() — not from scene code directly.
        public void OnUpdate(float deltaTime)
        {{
            if (_app.IsDead)
            {{
                if (!_deathPlayed) {{ _deathPlayed = true; _view.PlayDeath(); }}
                return;
            }}

            while (_input.TryDequeue(out ICommand cmd))
            {{
                switch (cmd)
                {{
                    case MoveCommand mv:
                        _lastDirX = mv.DirX;
                        _lastDirY = mv.DirY;
                        break;
                    case AttackCommand _:
                        // TODO (Slice 03): call ICombatService.RequestAttack()
                        break;
                    case InteractCommand _:
                        // Handled in OnTriggerEnter2D
                        break;
                }}
            }}

            _app.Tick(_lastDirX, _lastDirY, deltaTime);
            _view.UpdateMoveAnimation(_lastDirX, _lastDirY);
        }}

        // Called from {n}View.FixedUpdate() — physics write here, not in Update.
        public void OnFixedUpdate() => _view.SetVelocity(_app.VelocityX, _app.VelocityY);

        // Called from {n}View.OnTriggerEnter2D() — R03 forward pattern.
        public void OnTriggerEnter2D(Collider2D col)
        {{
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
                interactable.Interact();
        }}

        public void Dispose() => _app.OnDied -= HandleDied;

        private void HandleDied(EntityId id) => OnDiedCallback?.Invoke(id);
    }}
}}");

        Write($"Scripts/Modules/{n}/Presentation/{n}Runtime.cs",
$@"using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.Modules.{n}.Domain;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Immutable handle for one live {n} instance.
    // Returned by {n}Spawner. Call Dispose() to clean up event subscriptions.
    public sealed class {n}Runtime : IDisposable
    {{
        public {n}Runtime(EntityId id, {n}Definition def, {n}State state,
                          {n}Application app, {n}View view, {n}Presenter presenter)
        {{
            Id          = id;
            Definition  = def;
            State       = state;
            Application = app;
            View        = view;
            Presenter   = presenter;
        }}

        public EntityId        Id          {{ get; }}
        public {n}Definition  Definition  {{ get; }}
        public {n}State       State       {{ get; }}
        public {n}Application Application {{ get; }}
        public {n}View        View        {{ get; }}
        public {n}Presenter   Presenter   {{ get; }}

        public void Dispose() => Presenter.Dispose();
    }}
}}");

        Write($"Scripts/Modules/{n}/Presentation/{n}Spawner.cs",
$@"using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.Modules.{n}.Domain;
using BillGameCore.Modules.{n}.Infrastructure;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Registered in SceneLifetimeScope.
    // Receives prefab, config, and IInputCommandSource via DI constructor injection.
    // Creates per-entity Runtime — NEVER registers it back into DI (R04).
    // R18: EntityId.New() is called here — the ONLY valid location for this entity type.
    public sealed class {n}Spawner
    {{
        private readonly {n}View          _prefab;
        private readonly {n}Config        _config;
        private readonly IInputCommandSource _inputSource;

        public {n}Spawner({n}View prefab, {n}Config config, IInputCommandSource inputSource)
        {{
            _prefab      = prefab;
            _config      = config;
            _inputSource = inputSource;
        }}

        public {n}Runtime Spawn(Vector2 position)
        {{
            var id   = EntityId.New();                 // R18
            var def  = _config.ToDefinition();
            var state= new {n}State();
            var app  = new {n}Application(id, def, state);

            // R17: Object.Instantiate is correct here because {n}View has no [Inject] fields.
            // If you add [Inject] to {n}View later, switch to container.Instantiate().
            var view      = Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new {n}Presenter(app, view, _inputSource);
            view.Bind(presenter);

            return new {n}Runtime(id, def, state, app, view, presenter);
        }}
    }}
}}");

        // Infrastructure ──────────────────────────────────
        Write($"Scripts/Modules/{n}/Infrastructure/Config/{n}Config.cs",
$@"using BillGameCore.Modules.{n}.Domain;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Infrastructure
{{
    // ScriptableObject config mapper.
    // R10: Static config data ONLY — no runtime state, no mutable fields.
    [CreateAssetMenu(fileName = ""{n}Config"", menuName = ""BillGameCore/{n}/{n} Config"")]
    public sealed class {n}Config : ScriptableObject
    {{
        [SerializeField] private float _moveSpeed  = 5f;
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;

        public {n}Definition ToDefinition() => new {n}Definition
        {{
            MoveSpeed  = _moveSpeed,
            MaxHealth  = _maxHealth,
            MaxStamina = _maxStamina,
        }};
    }}
}}");

        WriteAsmdef($"Scripts/Modules/{n}", $"BillGameCore.Modules.{n}",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts",
                    "BillGameCore.Modules.Input", "VContainer" }, editorOnly: false);
    }

    // ═══════════════════════════════════════════════════════
    //  INTERACTION GROUP ARCHETYPE
    // ═══════════════════════════════════════════════════════

    static void CreateInteractionGroupModule(string firstName)
    {
        EnsureDir(Path.Combine(Root, "Scripts/Modules/InteractionGroup"));
        CreateInteractionType(firstName);

        WriteAsmdef("Scripts/Modules/InteractionGroup", "BillGameCore.Modules.InteractionGroup",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" },
            editorOnly: false);
    }

    static void CreateInteractionType(string n)
    {
        string[] dirs =
        {
            $"Scripts/Modules/InteractionGroup/{n}/Domain",
            $"Scripts/Modules/InteractionGroup/{n}/Application",
            $"Scripts/Modules/InteractionGroup/{n}/Presentation",
            $"Scripts/Modules/InteractionGroup/{n}/Infrastructure/Config",
        };
        foreach (string d in dirs) EnsureDir(Path.Combine(Root, d));

        Write($"Scripts/Modules/InteractionGroup/{n}/Domain/{n}Definition.cs",
$@"namespace BillGameCore.Modules.InteractionGroup.{n}.Domain
{{
    // R15: Pure C# — no UnityEngine.
    [System.Serializable]
    public sealed class {n}Definition
    {{
        public bool StartsActivated;
        // Add interaction-specific config data.
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Domain/{n}State.cs",
$@"namespace BillGameCore.Modules.InteractionGroup.{n}.Domain
{{
    // R15: Pure C#.  R10: Not in ScriptableObject.  R04: Not in DI scope.
    public sealed class {n}State
    {{
        public bool IsActivated {{ get; set; }}
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Application/{n}Application.cs",
$@"using System;
using BillGameCore.Modules.InteractionGroup.{n}.Domain;

namespace BillGameCore.Modules.InteractionGroup.{n}.Application
{{
    // R15: Pure C# — no UnityEngine.
    public sealed class {n}Application
    {{
        private readonly {n}State _state;

        public event Action Activated;
        public event Action Deactivated;

        public bool IsActivated => _state.IsActivated;

        public {n}Application({n}Definition def, {n}State state)
        {{
            _state             = state;
            _state.IsActivated = def.StartsActivated;
        }}

        public bool TryActivate()
        {{
            if (_state.IsActivated) return false;
            _state.IsActivated = true;
            Activated?.Invoke();
            return true;
        }}

        public void Deactivate()
        {{
            if (!_state.IsActivated) return;
            _state.IsActivated = false;
            Deactivated?.Invoke();
        }}
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Presentation/{n}View.cs",
$@"using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.{n}.Presentation
{{
    // R03: Drives Animator only — no business logic.
    public sealed class {n}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        public void PlayActivated()   => _animator?.SetTrigger(""Activate"");
        public void PlayDeactivated() => _animator?.SetTrigger(""Deactivate"");
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Presentation/{n}Presenter.cs",
$@"using System;
using BillGameCore.Modules.InteractionGroup.{n}.Application;

namespace BillGameCore.Modules.InteractionGroup.{n}.Presentation
{{
    public sealed class {n}Presenter : IDisposable
    {{
        private readonly {n}Application _app;
        private readonly {n}View        _view;

        public {n}Presenter({n}Application app, {n}View view)
        {{
            _app  = app;
            _view = view;
            _app.Activated   += _view.PlayActivated;
            _app.Deactivated += _view.PlayDeactivated;
        }}

        public bool TryActivate() => _app.TryActivate();

        public void Dispose()
        {{
            _app.Activated   -= _view.PlayActivated;
            _app.Deactivated -= _view.PlayDeactivated;
        }}
    }}
}}");

        // Binder: implements IInteractable — PlayerPresenter sees ONLY IInteractable (R07).
        Write($"Scripts/Modules/InteractionGroup/{n}/Presentation/{n}Binder.cs",
$@"using BillGameCore.Core.Interaction;
using BillGameCore.Modules.InteractionGroup.{n}.Application;
using BillGameCore.Modules.InteractionGroup.{n}.Domain;
using BillGameCore.Modules.InteractionGroup.{n}.Infrastructure;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.{n}.Presentation
{{
    // Placed on the {n} world prefab. Implements IInteractable.
    // PlayerPresenter calls GetComponent<IInteractable>() — never knows this type (R07).
    // Binder self-constructs Application/State/Presenter in Awake — NOT via DI (R04).
    //
    // R17 IMPORTANT: if you add [VContainer.Inject] fields (e.g. IInventoryWriteService),
    //   you MUST instantiate this prefab via container.Instantiate(), not Object.Instantiate().
    public sealed class {n}Binder : MonoBehaviour, IInteractable
    {{
        [SerializeField] private {n}View   _view;
        [SerializeField] private {n}Config _config;

        // Add [VContainer.Inject] service fields here if needed.
        // Remember to switch to container.Instantiate() in the spawner (R17).

        private {n}Application _app;
        private {n}Presenter   _presenter;

        private void Awake()
        {{
            if (_view == null) _view = GetComponent<{n}View>();
            var def   = _config != null ? _config.ToDefinition() : new {n}Definition();
            var state = new {n}State();
            _app      = new {n}Application(def, state);
            _presenter= new {n}Presenter(_app, _view);

            if (_app.IsActivated) _view.PlayActivated();
        }}

        public bool CanInteract() => _app != null && !_app.IsActivated;

        public void Interact()
        {{
            _presenter?.TryActivate();
            // Add service calls here (e.g. _inventory.AddItem(...)) after adding [Inject].
        }}

        private void OnDestroy() => _presenter?.Dispose();
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Infrastructure/Config/{n}Config.cs",
$@"using BillGameCore.Modules.InteractionGroup.{n}.Domain;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.{n}.Infrastructure
{{
    // R10: Config data only — no runtime state.
    [CreateAssetMenu(fileName = ""{n}Config"", menuName = ""BillGameCore/Interaction/{n} Config"")]
    public sealed class {n}Config : ScriptableObject
    {{
        [SerializeField] private bool _startsActivated;

        public {n}Definition ToDefinition() => new {n}Definition
        {{
            StartsActivated = _startsActivated,
        }};
    }}
}}");
    }

    // ═══════════════════════════════════════════════════════
    //  SYSTEM / SERVICE ARCHETYPE
    // ═══════════════════════════════════════════════════════

    static void CreateSystemModule(string n)
    {
        string[] dirs =
        {
            $"Scripts/Modules/{n}/Domain",
            $"Scripts/Modules/{n}/Application",
            $"Scripts/Modules/{n}/Infrastructure/Config",
            $"Scripts/Modules/{n}/Infrastructure/Persistence",
        };
        foreach (string d in dirs) EnsureDir(Path.Combine(Root, d));

        Write($"Scripts/Modules/{n}/Domain/{n}State.cs",
$@"namespace BillGameCore.Modules.{n}.Domain
{{
    // Runtime state owned exclusively by {n}Service.
    // R04: Never in DI scope.  R10: Not in ScriptableObject.  R15: Pure C#.
    public sealed class {n}State
    {{
        // Add runtime state fields here.
    }}
}}");

        Write($"Scripts/Modules/{n}/Application/{n}Service.cs",
$@"using System;
using BillGameCore.Core.Save;
using BillGameCore.Modules.{n}.Domain;
using BillGameCore.Modules.{n}.Infrastructure;

namespace BillGameCore.Modules.{n}.Application
{{
    // System service.
    // ADR-06: register in ProjectLifetimeScope (Save/Inventory/Economy/Audio)
    //         or SceneLifetimeScope for scene-only services.
    // R07: consuming modules inject a SharedPorts interface — never this class directly.
    // R04: {n}State is created here, not injected from outside.
    public sealed class {n}Service
        : ISaveSnapshotProvider<{n}SaveData>,
          ISaveSnapshotConsumer<{n}SaveData>
    {{
        private readonly {n}State _state = new {n}State();

        // Notify local UI presenters — not for cross-module broadcast (use MessagePipe for that).
        public event Action Changed;

        // ── ISaveSnapshotProvider ──────────────────────────
        public {n}SaveData CreateSnapshot()
        {{
            return new {n}SaveData(); // populate from _state fields
        }}

        // ── ISaveSnapshotConsumer ──────────────────────────
        public void RestoreSnapshot({n}SaveData snapshot)
        {{
            if (snapshot == null) return;
            // Restore _state from snapshot.
            Changed?.Invoke();
        }}

        // Add command/query methods here.
        // Expose narrow interfaces in SharedPorts/ for other modules to consume.
    }}
}}");

        Write($"Scripts/Modules/{n}/Infrastructure/Config/{n}Settings.cs",
$@"using UnityEngine;

namespace BillGameCore.Modules.{n}.Infrastructure
{{
    // R10: Static config data only.
    [CreateAssetMenu(fileName = ""{n}Settings"", menuName = ""BillGameCore/{n}/{n} Settings"")]
    public sealed class {n}Settings : ScriptableObject
    {{
        // Add serialized config fields.
    }}
}}");

        Write($"Scripts/Modules/{n}/Infrastructure/Persistence/{n}SaveData.cs",
$@"namespace BillGameCore.Modules.{n}.Infrastructure
{{
    // Serializable save DTO — snapshot of {n}State.  NOT a live state object.
    [System.Serializable]
    public sealed class {n}SaveData
    {{
        // Mirror {n}State fields as serializable types.
    }}
}}");

        WriteAsmdef($"Scripts/Modules/{n}", $"BillGameCore.Modules.{n}",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" },
            editorOnly: false);
    }

    // ═══════════════════════════════════════════════════════
    //  INTEGRATION STEP MESSAGES
    // ═══════════════════════════════════════════════════════

    static string BuildInputSteps() =>
@"<b>Slice 01 — Input  (Full Command Pattern)</b>

<b>1. Unity Editor — enable New Input System</b>
   Project Settings → Player → Active Input Handling = ""Input System Package (New)""
   Restart Editor when prompted.

<b>2. Create InputActions asset</b>
   Right-click in Project → Create → Input Actions → name ""InputActions""
   Add ActionMaps:
     Player  → Move (Value/Vector2/WASD+LeftStick), Attack (Button), Interact (Button/E)
     Vehicle → Throttle (Value/float), Steer (Value/float), Exit (Button)
     UI      → use Unity default bindings
   Enable 'Generate C# Class' if you prefer typed access (optional).
   Save asset.

<b>3. Scene setup</b>
   Create empty GameObject ""InputReader"" in scene.
   Attach InputReader.cs to it.
   Attach PlayerInput component to same GO → assign InputActions asset.
   Drag PlayerInput reference into InputReader.cs _playerInput slot.

<b>4. SceneLifetimeScope — add inside Configure():</b>
   builder.Register&lt;CommandBuffer&gt;(Lifetime.Singleton);
   builder.Register&lt;InputCommandDispatcher&gt;(Lifetime.Singleton).As&lt;IInputCommandSource&gt;();
   builder.RegisterComponent(inputReaderRef);   // drag InputReader GO into Inspector slot

<b>5. BillGameCore.Composition.asmdef — add reference:</b>
   ""BillGameCore.Modules.Input""

<b>6. Test</b>
   Play → in InputCommandDispatcher.TryDequeue add Debug.Log(command.Type)
   → verify MoveCommand appears when pressing WASD.

<color=orange>ADR-02: Each ICommand carries SourceId (EntityId).
Call InputReader.SetControlledEntity(runtime.Id) from GameBootstrapper after player spawns.</color>";

    static string BuildEntitySteps(string n) =>
$@"<b>Entity slice '{n}'</b>

<b>1. Create Unity assets</b>
   Right-click Data/Settings → Create → BillGameCore/{n}/{n} Config → fill stats.
   Create prefab '{n}':
     Add Sprite + Rigidbody2D (Gravity Scale = 0 for top-down) + Collider2D.
     Attach {n}View.cs to prefab root.
     Create Animator Controller with states: Idle / Walk / Attack / Die.
     Assign Controller to Animator component.

<b>2. SceneLifetimeScope — add inside Configure():</b>
   builder.Register&lt;{n}Spawner&gt;(Lifetime.Scoped);
   builder.RegisterInstance({n.ToLower()}ConfigRef);        // drag SO asset into slot
   builder.RegisterInstance({n.ToLower()}ViewPrefabRef);    // drag prefab into slot
   (IInputCommandSource already registered from Slice 01 — auto-injected into {n}Spawner.)

<b>3. GameBootstrapper — wire startup:</b>
   // Inject {n}Spawner and InputReader via constructor.
   var runtime = _{n.ToLower()}Spawner.Spawn(Vector2.zero);
   _inputReader.SetControlledEntity(runtime.Id);

<b>4. BillGameCore.Composition.asmdef — add reference:</b>
   ""BillGameCore.Modules.{n}""

<b>5. Test</b>
   Play → {n} moves with WASD.

<color=orange>R04: {n}Runtime is owned by caller (GameBootstrapper) — NEVER put it in DI scope.
R18: EntityId.New() is called inside {n}Spawner.Spawn() — the only valid location.</color>";

    static string BuildInteractionSteps(string n) =>
$@"<b>InteractionGroup — first type '{n}'</b>

<b>1. Create Unity assets</b>
   Right-click Data/Settings → Create → BillGameCore/Interaction/{n} Config.
   Create prefab '{n}':
     Add Sprite + Collider2D (Is Trigger = TRUE) + Animator.
     Attach {n}View.cs and {n}Binder.cs to root.
     In {n}Binder Inspector: drag {n}View and {n}Config references.

<b>2. DI registration</b>
   Base case (no injected services): no DI registration needed.
   {n}Binder self-constructs everything in Awake.

   If {n}Binder needs an injected service (e.g. IInventoryWriteService):
     Add [VContainer.Inject] field to {n}Binder.
     Instantiate via container.Instantiate(_prefab, pos, rot) in the spawner — NOT Object.Instantiate (R17).

<b>3. PlayerPresenter wiring (already done)</b>
   PlayerPresenter.OnTriggerEnter2D calls GetComponent&lt;IInteractable&gt;().
   PlayerPresenter does NOT know {n}Binder exists (R07).

<b>4. BillGameCore.Composition.asmdef — add reference:</b>
   ""BillGameCore.Modules.InteractionGroup""

<b>5. Test</b>
   Play → Player walks into {n} prefab → {n}Binder.Interact() fires → Animator plays Activate.

<b>Adding more types later:</b>
   Menu: BillGameCore / New Module / Add Interaction Type to Group
   All types share BillGameCore.Modules.InteractionGroup.asmdef.";

    static string BuildSystemSteps(string n) =>
$@"<b>System slice '{n}'</b>

<b>1. Decide lifetime scope (ADR-06)</b>
   ProjectLifetimeScope → survives scene loads: Inventory, Economy, Save, Audio
   SceneLifetimeScope   → scene-only services

<b>2. Register in chosen scope:</b>
   builder.Register&lt;{n}Service&gt;(Lifetime.Singleton).AsImplementedInterfaces();
   // AsImplementedInterfaces() exposes all SharedPorts interfaces {n}Service implements.

<b>3. Add narrow port interfaces to SharedPorts</b>
   Create interface files in Scripts/SharedPorts/... for what other modules need.
   e.g. IInventoryReadService, IInventoryWriteService
   {n}Service implements them; consuming modules inject the interface.

<b>4. BillGameCore.Composition.asmdef — add reference:</b>
   ""BillGameCore.Modules.{n}""

<b>5. Save wiring (Slice 07)</b>
   {n}Service already implements ISaveSnapshotProvider/Consumer.
   Register SaveService to collect snapshots from all providers.

<b>6. Test</b>
   Play → call service method → verify state via Debug.Log or UI.

<color=orange>R07: Other modules inject the SharedPorts interface — NEVER reference {n}Service directly.
R04: {n}State is created inside {n}Service — not registered in DI scope.</color>";

    // ═══════════════════════════════════════════════════════
    //  UTILITIES
    // ═══════════════════════════════════════════════════════

    static void Write(string relativePath, string content)
    {
        string full = Path.Combine(Root, relativePath);
        if (!File.Exists(full))
            File.WriteAllText(full, content, Encoding.UTF8);
    }

    static void EnsureDir(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    static void WriteAsmdef(string folderRelative, string name, string[] refs, bool editorOnly)
    {
        string path = Path.Combine(Root, folderRelative, name + ".asmdef");
        if (File.Exists(path)) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"    \"name\": \"{name}\",");
        sb.AppendLine("    \"references\": [");
        for (int i = 0; i < refs.Length; i++)
            sb.AppendLine($"        \"{refs[i]}\"{(i < refs.Length - 1 ? "," : "")}");
        sb.AppendLine("    ],");
        sb.AppendLine(editorOnly ? "    \"includePlatforms\": [\"Editor\"]," : "    \"includePlatforms\": [],");
        sb.AppendLine("    \"excludePlatforms\": [],");
        sb.AppendLine("    \"autoReferenced\": false,");
        sb.AppendLine("    \"defineConstraints\": [],");
        sb.AppendLine("    \"versionDefines\": [],");
        sb.AppendLine("    \"noEngineReferences\": false");
        sb.AppendLine("}");

        EnsureDir(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var sb = new StringBuilder(raw.Length);
        bool capNext = true;
        foreach (char c in raw.Trim())
        {
            if (char.IsLetterOrDigit(c)) { sb.Append(capNext ? char.ToUpperInvariant(c) : c); capNext = false; }
            else if (c == '_' || c == '-' || char.IsWhiteSpace(c)) capNext = true;
        }
        if (sb.Length > 0 && char.IsDigit(sb[0])) sb.Insert(0, '_');
        return sb.ToString();
    }

    static bool ValidateName(string name)
    {
        if (!string.IsNullOrEmpty(name)) return true;
        EditorUtility.DisplayDialog("Invalid Name",
            "Module name must contain at least one letter or digit.", "OK");
        return false;
    }

    static void QueueLog(string msg) => SessionState.SetString(PendingLogKey, msg);

    [DidReloadScripts]
    static void OnReload()
    {
        string msg = SessionState.GetString(PendingLogKey, string.Empty);
        if (string.IsNullOrEmpty(msg)) return;
        Debug.Log(msg);
        SessionState.EraseString(PendingLogKey);
    }
}

// ═══════════════════════════════════════════════════════════
//  EDITOR UTILITY WINDOWS
// ═══════════════════════════════════════════════════════════

public sealed class InputDialog : EditorWindow
{
    private static string _result;
    private string _label, _input;

    public static string Show(string title, string label, string defaultValue = "")
    {
        _result = null;
        var w = CreateInstance<InputDialog>();
        w.titleContent = new GUIContent(title);
        w._label = label;
        w._input = defaultValue;
        w.minSize = w.maxSize = new Vector2(400f, 106f);
        w.ShowModal();
        return _result;
    }

    private void OnGUI()
    {
        GUILayout.Space(12f);
        GUILayout.Label(_label, EditorStyles.wordWrappedLabel);
        GUILayout.Space(4f);
        GUI.SetNextControlName("F");
        _input = EditorGUILayout.TextField(_input);
        EditorGUI.FocusTextInControl("F");
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel")) Close();
        bool enter = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
        if (GUILayout.Button("Create") || enter) { _result = _input; Close(); }
        GUILayout.EndHorizontal();
    }
}

public sealed class StepsWindow : EditorWindow
{
    private string _content;
    private Vector2 _scroll;
    private GUIStyle _label;
    private GUIStyle _box;

    public static void Show(string title, string content)
    {
        var w = GetWindow<StepsWindow>(true, title, true);
        w._content = content;
        w.minSize = new Vector2(600f, 500f);
        w.Show();
    }

    private void OnGUI()
    {
        _label ??= new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true,
            fontSize = 13,
            alignment = TextAnchor.UpperLeft,
        };
        _label.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        _box ??= new GUIStyle("HelpBox") { padding = new RectOffset(14, 14, 14, 14) };

        GUILayout.Space(8f);
        GUILayout.Label("Integration Steps  —  complete these before wiring DI", EditorStyles.boldLabel);
        GUILayout.Space(6f);
        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.BeginVertical(_box);
        GUILayout.Label(_content, _label);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(8f);
        if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(28f)))
        {
            EditorGUIUtility.systemCopyBuffer =
                System.Text.RegularExpressions.Regex.Replace(_content, "<.*?>", string.Empty);
            Debug.Log("[BillGameCore] Steps copied to clipboard.");
        }
        GUILayout.Space(6f);
    }
}
