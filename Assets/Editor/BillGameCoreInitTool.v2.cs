// ============================================================
//  BillGameCoreInitTool.cs  —  v3.0
//  Căn chỉnh 1-1 với CONTEXT_v2_0.md
//
//  DANH SÁCH SỬA LỖI so với v2.0
//  ──────────────────────────────────────────────────────────
//  FIX-01  EntityApplication giờ implement IPlayerReadService
//          (expose qua SharedPorts.Player) cho archetype Player.
//          Archetype Enemy chỉ giữ IDamageReceiver — IPlayerReadService
//          là đặc thù Player theo Context §8.
//  FIX-02  Signature event OnDied đổi từ Action<EntityId>
//          thành Action<EntityId, RewardBundle, Vector2> để
//          SceneController nhận đủ bundle + vị trí thế giới (Context §14C).
//  FIX-03  {n}Presenter không còn tham chiếu namespace
//          Modules.Input.Commands. Dispatch command thực hiện qua
//          enum CommandType + interface ICommand trong SharedPorts (R06/R07).
//  FIX-04  Asmdef của {n}Spawner không còn tham chiếu BillGameCore.Modules.Input.
//          IInputCommandSource nằm trong SharedPorts — ref đó là đủ (R06/R07).
//  FIX-05  Signature OnDiedCallback của {n}Presenter sửa đúng theo
//          Application.OnDied (EntityId, RewardBundle, Vector2).
//  FIX-06  Template PlayerApplication giờ implement rõ ràng
//          IPlayerReadService và expose MaxHealth từ Definition.
//  FIX-07  IPlayerReadService.MaxHealth lấy từ Definition
//          (config bất biến) thay vì State — đúng theo Context §8.
//  FIX-08  Using-directive namespace Infrastructure.Config của {n}Spawner sửa đúng.
//  FIX-09  Xử lý InteractCommand chuyển vào vòng lặp command queue
//          (nhất quán với luồng Context §14D).
//  FIX-10  Signature SceneController.HandleEnemyDied sửa đúng thành
//          (EntityId, RewardBundle, Vector2) khớp contract OnDied mới.
//  FIX-11  Sửa lỗi compile: BuildRewardBundle() trong {n}Application
//          đổi từ "protected virtual" (không hợp lệ trên sealed class)
//          sang Func<RewardBundle> được inject vào constructor, cho phép
//          Enemy truyền lambda lấy loot từ EnemyDefinition.
//
//  ĐƯỜNG DẪN MENU
//  ──────────────────────────────────────────────────────────
//  BillGameCore / Initialize Project Structure
//  BillGameCore / New Module / Input  (Full Command Pattern)
//  BillGameCore / New Module / Entity (Player, Enemy …)
//  BillGameCore / New Module / Interaction (Chest, Door …)
//  BillGameCore / New Module / System (Inventory, Save …)
//  BillGameCore / New Module / Add Interaction Type to Group
//
//  HỢP ĐỒNG KIẾN TRÚC (CONTEXT_v2_0.md — tóm tắt)
//  ──────────────────────────────────────────────────────────
//  Core         – kiểu giá trị & interface thuần C#, không Unity, không logic
//  SharedPorts  – chỉ interface hẹp; chỉ ref Core; KHÔNG ref module
//  Modules.*    – vertical slice; chỉ ref Core + SharedPorts (cross-asmdef)
//  Composition  – thuộc integrator; ref mọi thứ; wire DI scope
//  Scenes       – SceneController làm mediator
//
//  NHÓM ASMDEF (ADR-05)
//  ──────────────────────────────────────────────────────────
//  BillGameCore.Modules.Input            Input (Command Pattern)
//  BillGameCore.Modules.Player           Slice Player
//  BillGameCore.Modules.CombatGroup      Combat + Projectile
//  BillGameCore.Modules.EnemyGroup       Enemy + Loot
//  BillGameCore.Modules.InventoryGroup   Inventory + Economy
//  BillGameCore.Modules.InteractionGroup Chest / Door / HealPoint / Trap …
//  BillGameCore.Modules.Save             Save
//  BillGameCore.Modules.Audio            Audio
//  BillGameCore.Modules.UI               HUD + InventoryPanel
//
//  QUY TẮC QUAN TRỌNG ĐƯỢC ÁP DỤNG TRONG CODE SINH RA
//  ──────────────────────────────────────────────────────────
//  R03  MonoBehaviour callback → chỉ forward sang Presenter
//  R04  State/App/Presenter/Runtime của entity KHÔNG BAO GIỜ vào DI scope
//  R06  Không tham chiếu trực tiếp namespace nội bộ module khác
//  R07  Cross-module chỉ qua SharedPorts hoặc MessagePipe (từ Slice 03)
//  R09  SharedPorts chỉ ref Core — không bao giờ ref asmdef Module
//  R10  ScriptableObject = chỉ chứa config data
//  R15  Domain & Application = thuần C#, không UnityEngine types
//  R16  CommandBuffer.Enqueue chỉ từ Infrastructure (InputReader)
//  R17  Prefab cần [Inject] → dùng container.Instantiate()
//  R18  EntityId.New() chỉ trong Spawner
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
        WriteAllSharedPortsFiles();   // bao gồm cả WriteCommandDataInterfaces()
        WriteAllCompositionFiles();
        WriteScenesFile();
        WriteAllAsmdefs();
        AssetDatabase.Refresh();
        QueueLog("<color=cyan><b>[BillGameCore v3.0]</b></color> <color=green>Scaffold foundation xong. " +
                 "Mở CONTEXT_v2_0.md → checklist Phase 0 trước khi bắt đầu bất kỳ slice nào.</color>");
    }

    [MenuItem("BillGameCore/New Module/Input  (Full Command Pattern)")]
    public static void NewInputModule()
    {
        CreateInputModule();
        AssetDatabase.Refresh();
    }

    [MenuItem("BillGameCore/New Module/Entity  (Player, Enemy, Projectile ...)")]
    public static void NewEntityModule()
    {
        string name = InputDialog.Show("Module Entity Mới",
            "Tên module  (ví dụ: Player | Enemy | Projectile):", "Player");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateEntityModule(name);
        AssetDatabase.Refresh();
    }

    [MenuItem("BillGameCore/New Module/Interaction  (Chest, Door, HealPoint, Trap ...)")]
    public static void NewInteractionModule()
    {
        string name = InputDialog.Show("Module Interaction Mới",
            "Tên loại interaction đầu tiên  (ví dụ: Chest | Door | HealPoint):", "Chest");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateInteractionGroupModule(name);
        AssetDatabase.Refresh();
    }

    [MenuItem("BillGameCore/New Module/Add Interaction Type to Group")]
    public static void AddInteractionType()
    {
        string name = InputDialog.Show("Thêm Loại Interaction",
            "Tên loại cần thêm  (ví dụ: Door | HealPoint | Trap):", "Door");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateInteractionType(name);
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>[BillGameCore]</color> Đã thêm loại interaction '{name}' vào InteractionGroup.");
    }

    [MenuItem("BillGameCore/New Module/System  (Inventory, Save, Audio ...)")]
    public static void NewSystemModule()
    {
        string name = InputDialog.Show("Module System Mới",
            "Tên module  (ví dụ: Inventory | Economy | Audio | Save):", "Inventory");
        name = Normalize(name);
        if (!ValidateName(name)) return;
        CreateSystemModule(name);
        AssetDatabase.Refresh();
    }

    // ═══════════════════════════════════════════════════════
    //  FOUNDATION — tạo thư mục gốc
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
    //  CORE FILES  (thuần C# — không UnityEngine, không logic)
    // ═══════════════════════════════════════════════════════

    static void WriteAllCoreFiles()
    {
        Write("Scripts/Core/ValueObjects/EntityId.cs",
@"namespace BillGameCore.Core.ValueObjects
{
    // ADR-04: Dùng Guid — duy nhất tuyệt đối, không cần static counter,
    //         an toàn khi spawn song song.
    // R18: EntityId.New() CHỈ được gọi từ class Spawner.
    public readonly struct EntityId : System.IEquatable<EntityId>
    {
        public static readonly EntityId Invalid = new EntityId(System.Guid.Empty);

        /// <summary>R18: chỉ gọi từ Spawner.Spawn().</summary>
        public static EntityId New() => new EntityId(System.Guid.NewGuid());

        private EntityId(System.Guid value) { Value = value; }

        public System.Guid Value   { get; }
        public bool        IsValid => Value != System.Guid.Empty;

        public bool   Equals(EntityId other)    => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityId e && Equals(e);
        public override int  GetHashCode()      => Value.GetHashCode();
        public override string ToString()       => Value.ToString(""N"").Substring(0, 8);

        public static bool operator ==(EntityId a, EntityId b) =>  a.Equals(b);
        public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);
    }
}");

        Write("Scripts/Core/Combat/DamageInfo.cs",
@"using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Core.Combat
{
    // Được tạo bởi CombatApplication. Truyền vào IDamageReceiver.ReceiveDamage().
    // Thuần C# — KHÔNG có UnityEngine reference trong Core.
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
    // Kết quả trả về sau khi xử lý damage.
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
    // Tên method là ReceiveDamage — KHÔNG ĐƯỢC đổi tên (hợp đồng CONTEXT).
    // Implement bởi: EnemyApplication, PlayerApplication.
    // Gọi bởi: CHỈ CombatApplication — không gọi từ Presenter hay View.
    public interface IDamageReceiver
    {
        DamageResult ReceiveDamage(DamageInfo damage);
    }
}");

        Write("Scripts/Core/Interaction/IInteractable.cs",
@"namespace BillGameCore.Core.Interaction
{
    // Implement bởi các class Binder (ChestBinder, LootItemBinder ...).
    // PlayerPresenter gọi GetComponent<IInteractable>() khi overlap.
    // PlayerPresenter KHÔNG BAO GIỜ biết kiểu Binder cụ thể (R07).
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
    }
}");

        Write("Scripts/Core/Inventory/ItemStack.cs",
@"namespace BillGameCore.Core.Inventory
{
    // DTO item + số lượng dùng chung giữa Loot, Inventory và Interaction.
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
    // Được emit bởi EnemyApplication.OnDied.
    // Được tiêu thụ bởi SceneController → LootSpawner + IRewardGrantService.
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
    // Implement trên Service để SaveService có thể xuất snapshot.
    public interface ISaveSnapshotProvider<out TSnapshot>
    {
        TSnapshot CreateSnapshot();
    }
}");

        Write("Scripts/Core/Save/ISaveSnapshotConsumer.cs",
@"namespace BillGameCore.Core.Save
{
    // Implement trên Service để SaveService có thể khôi phục snapshot.
    public interface ISaveSnapshotConsumer<in TSnapshot>
    {
        void RestoreSnapshot(TSnapshot snapshot);
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  SHARED PORTS  (chỉ ref Core — R09)
    //
    //  ICommand và CommandType đặt TẠI ĐÂY (không trong Modules.Input)
    //  để consumer (PlayerPresenter) chỉ cần ref SharedPorts,
    //  không bao giờ ref Modules.Input trực tiếp.
    //  Các class command cụ thể trong Modules.Input.Commands implement
    //  SharedPorts.Input.ICommand.
    // ═══════════════════════════════════════════════════════

    static void WriteAllSharedPortsFiles()
    {
        // ── Input ─────────────────────────────────────────
        Write("Scripts/SharedPorts/Input/ICommand.cs",
@"using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    // Khai báo trong SharedPorts để consumer chỉ cần ref SharedPorts.
    // Các class cụ thể (MoveCommand, AttackCommand ...) nằm trong
    // Modules.Input.Commands và implement interface này.
    public interface ICommand
    {
        EntityId    SourceId  { get; }   // ai tạo ra command này (ADR-02 identity)
        CommandType Type      { get; }
        float       Timestamp { get; }   // Time.time lúc tạo
    }
}");

        Write("Scripts/SharedPorts/Input/CommandType.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // Đặt trong SharedPorts để consumer không cần ref Modules.Input.
    // KHÔNG XÓA hoặc đánh số lại các giá trị cũ (tương thích replay/save).
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
    // Context input hiện tại — điều khiển ActionMap nào đang active.
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
    // Tiêu thụ bởi: PlayerPresenter, AI controller.
    // Implement bởi: InputCommandDispatcher (Modules.Input).
    // Đăng ký trong SceneLifetimeScope với type IInputCommandSource.
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
        bool HasCommands { get; }
    }
}");

        // FIX-03: Interface dữ liệu hẹp để Entity Presenter đọc payload
        //         command mà không cần ref namespace Modules.Input (R06/R07).
        WriteCommandDataInterfaces();

        // ── Combat ────────────────────────────────────────
        Write("Scripts/SharedPorts/Combat/ICombatService.cs",
@"using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Combat
{
    public interface ICombatService
    {
        /// <summary>Cận chiến — xử lý damage trực tiếp lên receiver.</summary>
        void RequestAttack(EntityId attackerId, IDamageReceiver target, string weaponId);

        /// <summary>Tầm xa — spawn projectile; damage tính khi va chạm.</summary>
        void RequestRangedAttack(EntityId attackerId, float dirX, float dirY, string weaponId);

        /// <summary>Gọi từ ProjectilePresenter khi projectile trúng mục tiêu.</summary>
        void ResolveProjectileHit(EntityId attackerId, IDamageReceiver target, string projectileId);
    }
}");

        // ── Inventory ─────────────────────────────────────
        Write("Scripts/SharedPorts/Inventory/IInventoryReadService.cs",
@"using BillGameCore.Core.Inventory;
using System.Collections.Generic;

namespace BillGameCore.SharedPorts.Inventory
{
    // Tiêu thụ bởi: UI/InventoryPanel. Implement bởi: InventoryService.
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
    // Tiêu thụ bởi: LootItemBinder, ChestBinder. Implement bởi: InventoryService.
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
    // Tiêu thụ bởi: UI/HUD. Implement bởi: EconomyService.
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
    // Gọi bởi SceneController sau khi enemy chết — cộng exp+gold ngay lập tức.
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}");

        // ── Player ────────────────────────────────────────
        // FIX-01/06: IPlayerReadService expose MaxHealth từ Definition (bất biến).
        // PlayerApplication implement interface này để UI/HUD chỉ thấy SharedPorts,
        // không bao giờ thấy Modules.Player.
        Write("Scripts/SharedPorts/Player/IPlayerReadService.cs",
@"namespace BillGameCore.SharedPorts.Player
{
    // Tiêu thụ bởi: module UI/HUD.
    // Implement bởi: PlayerApplication (Modules.Player).
    // Đăng ký sau khi spawn: builder.RegisterInstance(runtime.Application).As<IPlayerReadService>().
    public interface IPlayerReadService
    {
        float CurrentHealth  { get; }
        float MaxHealth      { get; }   // FIX-07: lấy từ Definition (config bất biến)
        float CurrentStamina { get; }
    }
}");

        // ── Messages  (event MessagePipe từ Slice 03 trở đi) ──────
        Write("Scripts/SharedPorts/Messages/EnemyDiedMessage.cs",
@"using BillGameCore.Core.Rewards;
using UnityEngine;

namespace BillGameCore.SharedPorts.Messages
{
    // Publish bởi EnemyPresenter từ Slice 03+ (MessagePipe unlock — ADR-03).
    // Tiêu thụ bởi LootSpawner và/hoặc SceneController subscriber.
    public sealed class EnemyDiedMessage
    {
        public RewardBundle Bundle   { get; }
        public Vector2      Position { get; }

        public EnemyDiedMessage(RewardBundle bundle, Vector2 position)
        {
            Bundle   = bundle;
            Position = position;
        }
    }
}");

        Write("Scripts/SharedPorts/Messages/ItemPickedUpMessage.cs",
@"using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Messages
{
    // Publish bởi LootItemBinder sau khi nhặt item thành công (Slice 03+).
    public sealed class ItemPickedUpMessage
    {
        public ItemStack Stack { get; }
        public ItemPickedUpMessage(ItemStack stack) { Stack = stack; }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  SHARED PORTS — IMoveCommand / IAttackCommand / IInteractCommand
    //  (Hỗ trợ FIX-03: interface dữ liệu hẹp để Entity Presenter
    //   đọc payload command mà không ref Modules.Input)
    // ═══════════════════════════════════════════════════════

    static void WriteCommandDataInterfaces()
    {
        Write("Scripts/SharedPorts/Input/IMoveCommand.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // FIX-03: Interface dữ liệu hẹp để Entity Presenter đọc hướng di chuyển
    //         mà không cần ref namespace BillGameCore.Modules.Input (R06/R07).
    // Implement bởi: BillGameCore.Modules.Input.Commands.MoveCommand.
    public interface IMoveCommand : ICommand
    {
        float DirX     { get; }
        float DirY     { get; }
        bool  IsMoving { get; }
    }
}");

        Write("Scripts/SharedPorts/Input/IAttackCommand.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // FIX-03: Interface dữ liệu hẹp để Entity Presenter đọc trạng thái Attack
    //         mà không cần ref namespace BillGameCore.Modules.Input (R06/R07).
    // Implement bởi: BillGameCore.Modules.Input.Commands.AttackCommand.
    public interface IAttackCommand : ICommand
    {
        bool  IsHeld       { get; }
        float HeldDuration { get; }
    }
}");

        Write("Scripts/SharedPorts/Input/IInteractCommand.cs",
@"namespace BillGameCore.SharedPorts.Input
{
    // FIX-03/09: Interface marker cho Interact command.
    // Implement bởi: BillGameCore.Modules.Input.Commands.InteractCommand.
    public interface IInteractCommand : ICommand { }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  COMPOSITION  (thuộc quyền integrator)
    // ═══════════════════════════════════════════════════════

    static void WriteAllCompositionFiles()
    {
        Write("Scripts/Composition/ProjectLifetimeScope.cs",
@"using VContainer;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // THUỘC INTEGRATOR — không được sửa từ task feature slice (R14).
    // ADR-06: đăng ký service tồn tại xuyên scene load:
    //   InventoryService, EconomyService, SaveService, AudioService.
    // R04: KHÔNG BAO GIỜ đăng ký EntityState / EntityApplication / Presenter / Runtime ở đây.
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // ── Thêm registration khi từng slice được merge ───────────────────

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

            // Slice 03+ — Mở khóa MessagePipe (ADR-03, R13):
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
    // THUỘC INTEGRATOR — không được sửa từ task feature slice (R14).
    // Parent = ProjectLifetimeScope để SceneScope resolve được service của ProjectScope.
    // R04: KHÔNG BAO GIỜ đăng ký EntityState / EntityApplication / Presenter / Runtime ở đây.
    public sealed class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameBootstrapper>();

            // ── Thêm registration khi từng slice được merge ───────────────────

            // Slice 01 — Input:
            // builder.Register<CommandBuffer>(Lifetime.Singleton);
            // builder.Register<InputCommandDispatcher>(Lifetime.Singleton).As<IInputCommandSource>();
            // builder.RegisterComponent(inputReaderRef);   // kéo MonoBehaviour GO vào slot

            // Slice 02 — Player:
            // builder.Register<PlayerSpawner>(Lifetime.Scoped);
            // builder.RegisterInstance(playerConfigRef);        // SO asset
            // builder.RegisterInstance(playerViewPrefabRef);    // Prefab
            // Sau Spawn: builder.RegisterInstance(runtime.Application).As<IPlayerReadService>();

            // Slice 03 — CombatGroup:
            // builder.Register<CombatApplication>(Lifetime.Scoped).As<ICombatService>();
            // builder.Register<ProjectileSpawner>(Lifetime.Scoped);

            // Slice 04 — EnemyGroup:
            // builder.Register<EnemySpawner>(Lifetime.Scoped);
            // builder.Register<LootSpawner>(Lifetime.Scoped); // R17: dùng container.Instantiate

            // Scenes:
            // builder.RegisterComponent(sceneControllerRef);

            // Slice 08 — UI:
            // builder.Register<HUDPresenter>(Lifetime.Scoped);
            // builder.Register<InventoryPanelPresenter>(Lifetime.Scoped);
        }
    }
}");

        Write("Scripts/Composition/GameBootstrapper.cs",
@"using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // Entry point của scene — VContainer gọi Start() sau khi build DI xong.
    // Chỉ dùng constructor injection — không có [Inject] field (pure C# class).
    // Thêm dependency và startup call khi từng slice được merge.
    public sealed class GameBootstrapper : IStartable
    {
        // ── Ví dụ Slice 02 (bỏ comment khi Player slice được merge) ──────────
        // private readonly PlayerSpawner  _playerSpawner;
        // private readonly InputReader    _inputReader;
        // private readonly SceneController _sceneController;
        //
        // public GameBootstrapper(PlayerSpawner playerSpawner,
        //                         InputReader   inputReader,
        //                         SceneController sceneController)
        // {
        //     _playerSpawner   = playerSpawner;
        //     _inputReader     = inputReader;
        //     _sceneController = sceneController;
        // }

        public void Start()
        {
            Debug.Log(""[GameBootstrapper] Scene đã khởi động."");

            // ── Slice 02: spawn player và bind input ─────────────────────────
            // var runtime = _playerSpawner.Spawn(Vector2.zero);
            // _inputReader.SetControlledEntity(runtime.Id);
            // _sceneController.SetPlayerRuntime(runtime);
        }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  SCENES
    // ═══════════════════════════════════════════════════════

    static void WriteScenesFile()
    {
        // FIX-10: Signature HandleEnemyDied sửa đúng thành (EntityId, RewardBundle, Vector2)
        //         khớp contract event OnDied mới (FIX-02).
        Write("Scripts/Scenes/SceneController.cs",
@"using BillGameCore.Core.Rewards;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;
using VContainer;

// Slice 03+: using BillGameCore.SharedPorts.Messages;
// Slice 03+: using MessagePipe;

namespace BillGameCore.Scenes
{
    // Mediator — wire cross-module event bằng direct callback (Phase 1).
    // Từ Slice 03+: thay direct callback bằng MessagePipe publish/subscribe.
    // R01: Không dùng Find() hay FindObjectOfType() — mọi ref qua [Inject] hoặc constructor.
    // R06: SceneController chỉ đụng interface SharedPorts, không đụng nội bộ module.
    public sealed class SceneController : MonoBehaviour
    {
        [Inject] private IRewardGrantService _rewardGrant;
        // Slice 04: [Inject] private LootSpawner _lootSpawner;

        // FIX-10: Signature khớp với EnemyApplication.OnDied (EntityId, RewardBundle, Vector2).
        // Được gọi bởi EnemyPresenter.OnDiedCallback — wire trong GameBootstrapper sau EnemySpawner.Spawn().
        public void HandleEnemyDied(EntityId entityId, RewardBundle bundle, Vector2 worldPosition)
        {
            _rewardGrant?.Grant(bundle);
            // Slice 04: _lootSpawner?.Spawn(bundle, worldPosition);
        }

        // Slice 02: lưu PlayerRuntime để expose IPlayerReadService vào DI sau khi spawn.
        // public void SetPlayerRuntime(PlayerRuntime runtime) { ... }
    }
}");
    }

    // ═══════════════════════════════════════════════════════
    //  ASMDEFS
    // ═══════════════════════════════════════════════════════

    static void WriteAllAsmdefs()
    {
        // Core: không ref Unity.InputSystem, không ref module (R08)
        WriteAsmdef("Scripts/Core", "BillGameCore.Core",
            new string[0], editorOnly: false);

        // SharedPorts: chỉ ref Core (R09)
        WriteAsmdef("Scripts/SharedPorts", "BillGameCore.SharedPorts",
            new[] { "BillGameCore.Core" }, editorOnly: false);

        // Composition: thuộc integrator — ref tăng dần khi slice được merge
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
    //  MODULE INPUT  (Full Command Pattern — ADR-02)
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
    // Implement cả IMoveCommand để Entity Presenter đọc DirX/Y mà không ref module này (FIX-03).
    public sealed class MoveCommand : IMoveCommand
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
    // Implement cả IAttackCommand để Entity Presenter đọc IsHeld/HeldDuration (FIX-03).
    public sealed class AttackCommand : IAttackCommand
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
        public bool        IsHeld       { get; }   // true khi đang giữ nút
        public float       HeldDuration { get; }   // số giây đã giữ
    }
}");

        Write("Scripts/Modules/Input/Commands/InteractCommand.cs",
@"using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Implement cả IInteractCommand (marker interface trong SharedPorts — FIX-03/09).
    public sealed class InteractCommand : IInteractCommand
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

        public EntityId    SourceId      { get; }
        public CommandType Type          => CommandType.SwitchContext;
        public float       Timestamp     { get; }
        public InputContext TargetContext { get; }
    }
}");

        Write("Scripts/Modules/Input/Commands/CommandBuffer.cs",
@"using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // FIFO thread-safe.
    // R16: Chỉ InputReader (Infrastructure) được phép gọi Enqueue().
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue   = new Queue<ICommand>();
        private readonly object          _lock    = new object();
        private readonly int             _maxSize;

        public CommandBuffer(int maxSize = 32) { _maxSize = maxSize; }

        // R16: CHỈ được gọi từ InputReader.
        public void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize) _queue.Dequeue(); // bỏ cũ nhất khi tràn
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

        // Gọi bởi InputReader.SwitchContext() để xả command cũ.
        public void Clear() { lock (_lock) _queue.Clear(); }
    }
}");

        // Application ─────────────────────────────────────
        Write("Scripts/Modules/Input/Application/InputCommandDispatcher.cs",
@"using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Đăng ký trong SceneLifetimeScope với type IInputCommandSource.
    // Adapter mỏng: CommandBuffer → IInputCommandSource.
    // Consumer Presenter inject IInputCommandSource — không biết InputReader tồn tại.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer) { _buffer = buffer; }

        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
        public bool HasCommands                       => _buffer.HasCommands;
    }
}");

        // Context constants ───────────────────────────────
        Write("Scripts/Modules/Input/Context/PlayerInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    // Tên ActionMap trong InputActions asset cho context Player.
    public static class PlayerInputContext  { public const string ActionMapName = ""Player"";  }
}");
        Write("Scripts/Modules/Input/Context/VehicleInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    // Tên ActionMap trong InputActions asset cho context Vehicle.
    public static class VehicleInputContext { public const string ActionMapName = ""Vehicle""; }
}");
        Write("Scripts/Modules/Input/Context/UIInputContext.cs",
@"namespace BillGameCore.Modules.Input.Context
{
    // Tên ActionMap trong InputActions asset cho context UI.
    public static class UIInputContext      { public const string ActionMapName = ""UI"";      }
}");

        // Infrastructure — class DUY NHẤT được phép gọi CommandBuffer.Enqueue() ─────
        Write("Scripts/Modules/Input/Infrastructure/InputReader.cs",
@"using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour — dịch event New Input System thành ICommand object.
    // R03: Không có business logic — chỉ raw-input → Command translation.
    // R16: Đây là class DUY NHẤT được phép gọi CommandBuffer.Enqueue().
    // R17: Đăng ký qua builder.RegisterComponent<InputReader>() — VContainer resolve [Inject].
    public sealed class InputReader : MonoBehaviour
    {
        [Inject] private CommandBuffer _buffer; // được inject bởi VContainer

        [SerializeField] private PlayerInput _playerInput; // gắn trong Inspector

        private EntityId     _controlledEntityId = EntityId.Invalid;
        private InputContext _currentContext      = InputContext.Player;

        // Trạng thái giữ nút Attack
        private bool  _attackHeld;
        private float _attackHeldStart;

        // Gọi bởi GameBootstrapper sau khi PlayerSpawner.Spawn() trả về Runtime.
        public void SetControlledEntity(EntityId id) => _controlledEntityId = id;

        public void SwitchContext(InputContext context)
        {
            _currentContext = context;
            _buffer.Clear(); // xả command cũ (CONTEXT §14E)
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
                // UI do Unity EventSystem xử lý — không cần đọc thủ công
            }
        }

        // R16: Tất cả lệnh Enqueue chỉ nằm trong file này.
        private void ReadPlayerMap()
        {
            // Di chuyển — đọc mỗi frame (liên tục)
            var mv = _playerInput.actions[""Player/Move""].ReadValue<Vector2>();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, mv.x, mv.y, Time.time));

            // Tấn công — theo dõi giữ nút
            var atk = _playerInput.actions[""Player/Attack""];
            if (atk.WasPressedThisFrame()) { _attackHeld = true; _attackHeldStart = Time.time; }
            if (_attackHeld)
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time,
                                                  isHeld: true,
                                                  heldDuration: Time.time - _attackHeldStart));
            if (atk.WasReleasedThisFrame()) _attackHeld = false;

            // Tương tác — một lần mỗi lần nhấn
            if (_playerInput.actions[""Player/Interact""].WasPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }

        private void ReadVehicleMap()
        {
            // Thêm đọc Vehicle action ở đây khi slice Vehicle được build.
        }
    }
}");

        WriteAsmdef("Scripts/Modules/Input", "BillGameCore.Modules.Input",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts",
                    "VContainer", "Unity.InputSystem" }, editorOnly: false);
    }

    // ═══════════════════════════════════════════════════════
    //  ARCHETYPE ENTITY
    //
    //  Dùng cho: Player, Enemy, Boss, Projectile, Mount, NPC.
    //
    //  Điểm sửa quan trọng so với v2.0:
    //  FIX-01/06: {n}Application giờ implement IPlayerReadService (archetype Player).
    //  FIX-02:    Event OnDied: Action<EntityId, RewardBundle, Vector2>.
    //  FIX-03:    Presenter dùng CommandType enum (SharedPorts) thay vì
    //             kiểu cụ thể MoveCommand/AttackCommand (Modules.Input).
    //  FIX-04:    Asmdef Spawner không còn ref Modules.Input.
    //  FIX-05:    OnDiedCallback khớp với signature OnDied đã sửa.
    //  FIX-09:    InteractCommand xử lý trong vòng lặp command queue.
    //  FIX-11:    BuildRewardBundle() đổi từ "protected virtual" (lỗi trên
    //             sealed class) sang Func<RewardBundle> inject vào constructor.
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
    // Dữ liệu gameplay tĩnh. Được điền bởi {n}Config.ToDefinition(). Bất biến khi runtime.
    // R15: Thuần C# — KHÔNG có UnityEngine types trong Domain.
    [System.Serializable]
    public sealed class {n}Definition
    {{
        public float MoveSpeed  = 5f;
        public float MaxHealth  = 100f;
        public float MaxStamina = 100f;
        // Thêm stat đặc thù của entity ở đây.
    }}
}}");

        Write($"Scripts/Modules/{n}/Domain/{n}State.cs",
$@"namespace BillGameCore.Modules.{n}.Domain
{{
    // Trạng thái runtime có thể thay đổi cho MỘT instance {n}.
    // Chỉ được sở hữu và thay đổi bởi {n}Application.
    // R04: KHÔNG BAO GIỜ đăng ký vào DI scope.
    // R10: KHÔNG BAO GIỜ lưu trong ScriptableObject.
    // R15: Thuần C# — KHÔNG có UnityEngine types.
    public sealed class {n}State
    {{
        public float CurrentHealth  {{ get; set; }}
        public float CurrentStamina {{ get; set; }}
        public float VelocityX      {{ get; set; }}
        public float VelocityY      {{ get; set; }}
        public bool  IsDead         {{ get; set; }}
        // Thêm trường runtime đặc thù của entity ở đây.
    }}
}}");

        // Application ─────────────────────────────────────
        // FIX-01/06: Implement IPlayerReadService để UI inject qua SharedPorts.
        //            MaxHealth lấy từ _def (Definition) — config bất biến (FIX-07).
        // FIX-02:    OnDied mang (EntityId, RewardBundle, Vector2) cho SceneController (§14C).
        // FIX-11:    BuildRewardBundle là Func<RewardBundle> inject vào constructor thay vì
        //            protected virtual method — tránh lỗi compile trên sealed class.
        //            Enemy Spawner truyền lambda lấy loot từ EnemyDefinition.
        //            Player Spawner truyền null hoặc lambda trả về RewardBundle rỗng.
        Write($"Scripts/Modules/{n}/Application/{n}Application.cs",
$@"using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.{n}.Domain;
using BillGameCore.SharedPorts.Player;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Application
{{
    // Logic use-case của {n}.
    // Implement IDamageReceiver — CombatApplication gọi ReceiveDamage() qua shared contract.
    // Implement IPlayerReadService — UI/HUD inject qua SharedPorts (không qua Modules.{n}).
    // R15: KHÔNG có MonoBehaviour, Transform, Animator, Rigidbody2D, ScriptableObject ở đây.
    // R07: KHÔNG tham chiếu trực tiếp namespace module khác — dùng SharedPorts contracts.
    //
    // GHI CHÚ: IPlayerReadService đặc thù cho Player. Với archetype Enemy, xóa interface đó
    //          và các property tương ứng — UI không bao giờ đọc stat enemy trực tiếp.
    //
    // FIX-11: rewardBundleFactory là Func<RewardBundle> inject vào constructor.
    //         Enemy Spawner truyền: () => new RewardBundle {{ Gold = def.Gold, ... }}
    //         Player Spawner truyền: null (sẽ dùng bundle rỗng mặc định).
    public sealed class {n}Application : IDamageReceiver, IPlayerReadService
    {{
        private readonly {n}Definition        _def;
        private readonly {n}State             _state;
        private readonly Func<RewardBundle>   _rewardBundleFactory; // FIX-11

        // FIX-02: Event mang EntityId + RewardBundle + vị trí thế giới để SceneController
        //         gọi LootSpawner.Spawn(bundle, pos) và IRewardGrantService.Grant(bundle).
        //         Với archetype Player, RewardBundle sẽ null/rỗng — điều đó là bình thường.
        public event Action<EntityId, RewardBundle, Vector2> OnDied;

        // R18: EntityId do Spawner cung cấp — không bao giờ gọi EntityId.New() ở đây.
        public EntityId Id {{ get; }}

        // FIX-11: rewardBundleFactory có thể null (Player không drop loot).
        public {n}Application(EntityId id, {n}Definition def, {n}State state,
                              Func<RewardBundle> rewardBundleFactory = null)
        {{
            Id                   = id;
            _def                 = def;
            _state               = state;
            _rewardBundleFactory = rewardBundleFactory;
            _state.CurrentHealth  = def.MaxHealth;
            _state.CurrentStamina = def.MaxStamina;
        }}

        // ── IPlayerReadService ────────────────────────────────────────────────
        // FIX-06/07: MaxHealth từ _def (bất biến), CurrentHealth/Stamina từ _state.
        public float CurrentHealth  => _state.CurrentHealth;
        public float MaxHealth      => _def.MaxHealth;
        public float CurrentStamina => _state.CurrentStamina;

        // Accessor nội bộ cho Presenter (write velocity ra View).
        public float VelocityX => _state.VelocityX;
        public float VelocityY => _state.VelocityY;
        public bool  IsDead    => _state.IsDead;

        // Gọi bởi Presenter mỗi frame với giá trị direction từ command queue.
        public void Tick(float dirX, float dirY, float deltaTime)
        {{
            if (_state.IsDead) return;
            _state.VelocityX = dirX * _def.MoveSpeed;
            _state.VelocityY = dirY * _def.MoveSpeed;
        }}

        // IDamageReceiver — CHỈ được gọi bởi CombatApplication (CONTEXT §14A).
        public DamageResult ReceiveDamage(DamageInfo damage)
        {{
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {{
                _state.IsDead = true;
                // FIX-11: Dùng factory được inject thay vì protected virtual method.
                //         Truyền Vector2.zero — Presenter sẽ cung cấp vị trí thực từ View.
                var bundle = _rewardBundleFactory != null
                    ? _rewardBundleFactory.Invoke()
                    : new RewardBundle();
                OnDied?.Invoke(Id, bundle, Vector2.zero);
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
    // Output visual phía Unity cho {n}.
    // R03: MonoBehaviour callback CHỈ forward sang Presenter — KHÔNG có business logic nào ở đây.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class {n}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        private Rigidbody2D  _rb;
        private {n}Presenter _presenter;

        private void Awake() => _rb = GetComponent<Rigidbody2D>(); // GetComponent trên self được phép

        // Gọi một lần bởi {n}Spawner ngay sau Instantiate.
        public void Bind({n}Presenter presenter) => _presenter = presenter;

        // R03: chỉ forward ──────────────────────────────────
        private void Update()                         => _presenter?.OnUpdate(Time.deltaTime);
        private void FixedUpdate()                    => _presenter?.OnFixedUpdate();
        private void OnTriggerEnter2D(Collider2D col) => _presenter?.OnTriggerEnter2D(col);

        // Các method write của View — chỉ được gọi bởi Presenter ──────
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

        // Vị trí thế giới thực — Presenter dùng khi emit OnDied (FIX-05).
        public Vector2 WorldPosition => transform.position;
    }}
}}");

        // FIX-03: Presenter KHÔNG còn ref namespace Modules.Input.Commands.
        //         Dispatch command qua CommandType enum và cast sang interface
        //         IMoveCommand / IAttackCommand / IInteractCommand trong SharedPorts.
        // FIX-05: Signature OnDiedCallback khớp với Application.OnDied (EntityId, RewardBundle, Vector2).
        // FIX-09: InteractCommand xử lý trong vòng lặp command queue, dùng flag _pendingInteract.
        Write($"Scripts/Modules/{n}/Presentation/{n}Presenter.cs",
$@"using System;
using BillGameCore.Core.Interaction;
using BillGameCore.Core.Rewards;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.SharedPorts.Combat;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Bridge IInputCommandSource → {n}Application → {n}View mỗi frame.
    // Được tạo bởi {n}Spawner — KHÔNG qua DI container (R04).
    //
    // FIX-03: Dùng CommandType enum + interface IMoveCommand/IAttackCommand (SharedPorts).
    //         KHÔNG tham chiếu BillGameCore.Modules.Input.Commands namespace (R06/R07).
    // FIX-05: Signature OnDiedCallback khớp với Application.OnDied (§14C).
    public sealed class {n}Presenter : IDisposable
    {{
        private readonly {n}Application      _app;
        private readonly {n}View             _view;
        private readonly IInputCommandSource _input;
        private readonly ICombatService      _combat; // null cho đến khi Slice 03 được merge

        private bool  _deathPlayed;
        private float _lastDirX, _lastDirY;
        private bool  _pendingInteract; // FIX-09

        // FIX-05: Callback mang (EntityId, RewardBundle, Vector2) khớp với
        //         EnemyApplication.OnDied và SceneController.HandleEnemyDied.
        public Action<EntityId, RewardBundle, Vector2> OnDiedCallback;

        // Constructor khi ICombatService chưa có (Slice 01-02).
        public {n}Presenter({n}Application app, {n}View view, IInputCommandSource input)
            : this(app, view, input, null) {{ }}

        // Constructor từ Slice 03 trở đi khi ICombatService đã có.
        public {n}Presenter({n}Application app, {n}View view,
                            IInputCommandSource input, ICombatService combat)
        {{
            _app    = app;
            _view   = view;
            _input  = input;
            _combat = combat;
            _app.OnDied += HandleDied;
        }}

        // Gọi từ {n}View.Update() — không gọi trực tiếp từ scene code.
        public void OnUpdate(float deltaTime)
        {{
            if (_app.IsDead)
            {{
                if (!_deathPlayed) {{ _deathPlayed = true; _view.PlayDeath(); }}
                return;
            }}

            // FIX-03: Chỉ dùng CommandType enum và cast sang SharedPorts interface.
            //         KHÔNG dùng kiểu MoveCommand/AttackCommand cụ thể (R06/R07).
            while (_input.TryDequeue(out ICommand cmd))
            {{
                switch (cmd.Type)
                {{
                    case CommandType.Move:
                        // Cast sang IMoveCommand (SharedPorts) để đọc DirX/Y (FIX-03).
                        if (cmd is IMoveCommand mv)
                        {{
                            _lastDirX = mv.DirX;
                            _lastDirY = mv.DirY;
                        }}
                        break;

                    case CommandType.Attack:
                        // TODO Slice 03: gọi _combat?.RequestAttack(...)
                        // if (cmd is IAttackCommand atk) {{ ... }}
                        break;

                    case CommandType.Interact:
                        // FIX-09: Đặt flag, thực hiện IInteractable call trong OnTriggerEnter2D
                        //         vì cần collider reference mới có ở đó.
                        _pendingInteract = true;
                        break;
                }}
            }}

            _app.Tick(_lastDirX, _lastDirY, deltaTime);
            _view.UpdateMoveAnimation(_lastDirX, _lastDirY);
        }}

        // Gọi từ {n}View.FixedUpdate() — write physics ở đây, không trong Update.
        public void OnFixedUpdate() => _view.SetVelocity(_app.VelocityX, _app.VelocityY);

        // Gọi từ {n}View.OnTriggerEnter2D() — pattern forward R03.
        // PlayerPresenter gọi GetComponent<IInteractable>() — không biết kiểu Binder (R07).
        public void OnTriggerEnter2D(Collider2D col)
        {{
            // FIX-09: Chỉ thực hiện interact nếu có pending flag từ InteractCommand.
            if (!_pendingInteract) return;
            _pendingInteract = false;
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
                interactable.Interact();
        }}

        public void Dispose() => _app.OnDied -= HandleDied;

        // FIX-05: Lấy vị trí thực từ View (transform) thay vì Vector2.zero từ Application.
        private void HandleDied(EntityId id, RewardBundle bundle, Vector2 _)
        {{
            var worldPos = _view != null ? _view.WorldPosition : Vector2.zero;
            OnDiedCallback?.Invoke(id, bundle, worldPos);
        }}
    }}
}}");

        Write($"Scripts/Modules/{n}/Presentation/{n}Runtime.cs",
$@"using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.Modules.{n}.Domain;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Handle bất biến cho một instance {n} đang sống.
    // Trả về bởi {n}Spawner. Gọi Dispose() để dọn dẹp event subscription.
    // R04: KHÔNG BAO GIỜ đăng ký vào DI scope — được sở hữu bởi GameBootstrapper / SceneController.
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
        public {n}Definition   Definition  {{ get; }}
        public {n}State        State       {{ get; }}
        public {n}Application  Application {{ get; }}
        public {n}View         View        {{ get; }}
        public {n}Presenter    Presenter   {{ get; }}

        public void Dispose() => Presenter.Dispose();
    }}
}}");

        // FIX-04: Spawner chỉ dùng SharedPorts.Input.IInputCommandSource.
        //         Không ref namespace Modules.Input (R06/R07).
        // FIX-08: Using namespace Infrastructure.Config đúng với sub-folder.
        Write($"Scripts/Modules/{n}/Presentation/{n}Spawner.cs",
$@"using BillGameCore.Core.Rewards;
using BillGameCore.Modules.{n}.Application;
using BillGameCore.Modules.{n}.Domain;
using BillGameCore.Modules.{n}.Infrastructure.Config;
using BillGameCore.SharedPorts.Input;
using System;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.{n}.Presentation
{{
    // Đăng ký trong SceneLifetimeScope.
    // Nhận prefab, config và IInputCommandSource qua DI constructor injection.
    // Tạo Runtime per-entity — KHÔNG BAO GIỜ đăng ký ngược lại vào DI (R04).
    // R18: EntityId.New() được gọi tại đây — vị trí DUY NHẤT hợp lệ cho entity type này.
    // FIX-04: Không tham chiếu BillGameCore.Modules.Input — chỉ dùng SharedPorts (R06/R07).
    // FIX-08: Using BillGameCore.Modules.{n}.Infrastructure.Config cho {n}Config.
    // FIX-11: rewardBundleFactory truyền vào {n}Application để tránh protected virtual (sealed).
    public sealed class {n}Spawner
    {{
        private readonly {n}View             _prefab;
        private readonly {n}Config           _config;
        private readonly IInputCommandSource _inputSource;

        // FIX-11: Factory tạo RewardBundle — truyền null nếu entity không drop loot (e.g. Player).
        //         Enemy Spawner ghi đè factory này với lambda lấy loot từ EnemyDefinition.
        private readonly Func<RewardBundle> _rewardBundleFactory;

        // Constructor cho Player hoặc entity không drop loot.
        public {n}Spawner({n}View prefab, {n}Config config, IInputCommandSource inputSource)
            : this(prefab, config, inputSource, null) {{ }}

        // Constructor cho Enemy hoặc entity có drop loot.
        public {n}Spawner({n}View prefab, {n}Config config,
                          IInputCommandSource inputSource, Func<RewardBundle> rewardBundleFactory)
        {{
            _prefab              = prefab;
            _config              = config;
            _inputSource         = inputSource;
            _rewardBundleFactory = rewardBundleFactory;
        }}

        public {n}Runtime Spawn(Vector2 position)
        {{
            var id    = EntityId.New();                    // R18
            var def   = _config.ToDefinition();
            var state = new {n}State();

            // FIX-11: Truyền factory vào Application — không dùng protected virtual.
            var app   = new {n}Application(id, def, state, _rewardBundleFactory);

            // R17: Object.Instantiate đúng ở đây vì {n}View không có [Inject] field.
            // Nếu thêm [Inject] vào {n}View sau này, đổi sang container.Instantiate().
            var view      =  UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new {n}Presenter(app, view, _inputSource);
            view.Bind(presenter);

            return new {n}Runtime(id, def, state, app, view, presenter);
        }}
    }}
}}");

        // Infrastructure ──────────────────────────────────
        // FIX-08: Namespace khớp với sub-folder Infrastructure/Config/.
        Write($"Scripts/Modules/{n}/Infrastructure/Config/{n}Config.cs",
$@"using BillGameCore.Modules.{n}.Domain;
using UnityEngine;

namespace BillGameCore.Modules.{n}.Infrastructure.Config
{{
    // ScriptableObject config mapper.
    // R10: CHỈ chứa config data tĩnh — không có runtime state, không có mutable field.
    [CreateAssetMenu(fileName = ""{n}Config"", menuName = ""BillGameCore/{n}/{n} Config"")]
    public sealed class {n}Config : ScriptableObject
    {{
        [SerializeField] private float _moveSpeed  = 5f;
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;
        // Thêm serialized config field khớp với {n}Definition.

        public {n}Definition ToDefinition() => new {n}Definition
        {{
            MoveSpeed  = _moveSpeed,
            MaxHealth  = _maxHealth,
            MaxStamina = _maxStamina,
        }};
    }}
}}");

        // FIX-04: Asmdef không còn ref BillGameCore.Modules.Input.
        //         IInputCommandSource nằm trong SharedPorts — ref đó là đủ.
        WriteAsmdef($"Scripts/Modules/{n}", $"BillGameCore.Modules.{n}",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" },
            editorOnly: false);
    }

    // ═══════════════════════════════════════════════════════
    //  ARCHETYPE INTERACTION GROUP
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
    // R15: Thuần C# — không có UnityEngine.
    [System.Serializable]
    public sealed class {n}Definition
    {{
        public bool StartsActivated;
        // Thêm config data đặc thù của interaction ở đây.
    }}
}}");

        Write($"Scripts/Modules/InteractionGroup/{n}/Domain/{n}State.cs",
$@"namespace BillGameCore.Modules.InteractionGroup.{n}.Domain
{{
    // R15: Thuần C#.  R10: Không trong ScriptableObject.  R04: Không trong DI scope.
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
    // R15: Thuần C# — không có UnityEngine.
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
    // R03: Chỉ drive Animator — không có business logic.
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
    // Bridge {n}Application event → {n}View animation.
    // Được tạo bởi {n}Binder trong Awake.
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

        // Binder: implement IInteractable — PlayerPresenter CHỈ thấy IInteractable (R07).
        Write($"Scripts/Modules/InteractionGroup/{n}/Presentation/{n}Binder.cs",
$@"using BillGameCore.Core.Interaction;
using BillGameCore.Modules.InteractionGroup.{n}.Application;
using BillGameCore.Modules.InteractionGroup.{n}.Domain;
using BillGameCore.Modules.InteractionGroup.{n}.Infrastructure.Config;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.{n}.Presentation
{{
    // Gắn trên prefab {n} trong scene. Implement IInteractable (Core).
    // PlayerPresenter gọi GetComponent<IInteractable>() — không bao giờ biết type này (R07).
    // Binder tự tạo Application/State/Presenter trong Awake — KHÔNG qua DI (R04).
    //
    // QUAN TRỌNG R17: Nếu thêm [VContainer.Inject] field (ví dụ IInventoryWriteService),
    //   BẮT BUỘC instantiate prefab qua container.Instantiate(), không dùng Object.Instantiate().
    public sealed class {n}Binder : MonoBehaviour, IInteractable
    {{
        [SerializeField] private {n}View   _view;
        [SerializeField] private {n}Config _config;

        // Thêm [VContainer.Inject] service field ở đây nếu cần.
        // Nhớ đổi sang container.Instantiate() trong spawner (R17).

        private {n}Application _app;
        private {n}Presenter   _presenter;

        private void Awake()
        {{
            if (_view == null) _view = GetComponent<{n}View>();
            var def   = _config != null ? _config.ToDefinition() : new {n}Definition();
            var state = new {n}State();
            _app       = new {n}Application(def, state);
            _presenter = new {n}Presenter(_app, _view);

            if (_app.IsActivated) _view.PlayActivated();
        }}

        public bool CanInteract() => _app != null && !_app.IsActivated;

        public void Interact()
        {{
            _presenter?.TryActivate();
            // Thêm service call ở đây (ví dụ _inventory.AddItem(...)) sau khi thêm [Inject].
        }}

        private void OnDestroy() => _presenter?.Dispose();
    }}
}}");

        // FIX-08: Namespace khớp với sub-folder Infrastructure/Config/.
        Write($"Scripts/Modules/InteractionGroup/{n}/Infrastructure/Config/{n}Config.cs",
$@"using BillGameCore.Modules.InteractionGroup.{n}.Domain;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.{n}.Infrastructure.Config
{{
    // R10: Chỉ chứa config data — không có runtime state.
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
    //  ARCHETYPE SYSTEM / SERVICE
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
    // Trạng thái runtime được sở hữu độc quyền bởi {n}Service.
    // R04: Không vào DI scope.  R10: Không trong ScriptableObject.  R15: Thuần C#.
    public sealed class {n}State
    {{
        // Thêm field trạng thái runtime ở đây.
    }}
}}");

        Write($"Scripts/Modules/{n}/Application/{n}Service.cs",
$@"using System;
using BillGameCore.Core.Save;
using BillGameCore.Modules.{n}.Domain;
using BillGameCore.Modules.{n}.Infrastructure.Persistence;

namespace BillGameCore.Modules.{n}.Application
{{
    // System service.
    // ADR-06: đăng ký trong ProjectLifetimeScope (Save/Inventory/Economy/Audio)
    //         hoặc SceneLifetimeScope cho service chỉ sống trong một scene.
    // R07: module khác inject interface SharedPorts — không bao giờ ref class này trực tiếp.
    // R04: {n}State được tạo ngay tại đây, không inject từ ngoài.
    public sealed class {n}Service
        : ISaveSnapshotProvider<{n}SaveData>,
          ISaveSnapshotConsumer<{n}SaveData>
    {{
        private readonly {n}State _state = new {n}State();

        // Thông báo cho UI Presenter cục bộ — không dùng cho cross-module broadcast (dùng MessagePipe).
        public event Action Changed;

        // ── ISaveSnapshotProvider ──────────────────────────
        public {n}SaveData CreateSnapshot()
        {{
            return new {n}SaveData(); // điền từ các field của _state
        }}

        // ── ISaveSnapshotConsumer ──────────────────────────
        public void RestoreSnapshot({n}SaveData snapshot)
        {{
            if (snapshot == null) return;
            // Khôi phục _state từ các field trong snapshot.
            Changed?.Invoke();
        }}

        // Thêm command/query method ở đây.
        // Khai báo interface hẹp trong SharedPorts/ cho module khác consume (R07).
    }}
}}");

        Write($"Scripts/Modules/{n}/Infrastructure/Config/{n}Settings.cs",
$@"using UnityEngine;

namespace BillGameCore.Modules.{n}.Infrastructure.Config
{{
    // R10: Chỉ chứa config data tĩnh.
    [CreateAssetMenu(fileName = ""{n}Settings"", menuName = ""BillGameCore/{n}/{n} Settings"")]
    public sealed class {n}Settings : ScriptableObject
    {{
        // Thêm serialized config field ở đây.
    }}
}}");

        Write($"Scripts/Modules/{n}/Infrastructure/Persistence/{n}SaveData.cs",
$@"namespace BillGameCore.Modules.{n}.Infrastructure.Persistence
{{
    // DTO save serializable — snapshot của {n}State. KHÔNG phải live state object.
    [System.Serializable]
    public sealed class {n}SaveData
    {{
        // Mirror các field của {n}State sang kiểu serializable.
    }}
}}");

        WriteAsmdef($"Scripts/Modules/{n}", $"BillGameCore.Modules.{n}",
            new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" },
            editorOnly: false);
    }

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
        EditorUtility.DisplayDialog("Tên không hợp lệ",
            "Tên module phải chứa ít nhất một chữ cái hoặc chữ số.", "OK");
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
        if (GUILayout.Button("Hủy")) Close();
        bool enter = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
        if (GUILayout.Button("Tạo") || enter) { _result = _input; Close(); }
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
        GUILayout.Label("Các bước tích hợp  —  hoàn thành trước khi wire DI", EditorStyles.boldLabel);
        GUILayout.Space(6f);
        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.BeginVertical(_box);
        GUILayout.Label(_content, _label);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.Space(8f);
        if (GUILayout.Button("Sao chép vào Clipboard", GUILayout.Height(28f)))
        {
            EditorGUIUtility.systemCopyBuffer =
                System.Text.RegularExpressions.Regex.Replace(_content, "<.*?>", string.Empty);
            Debug.Log("[BillGameCore] Đã sao chép các bước vào clipboard.");
        }
        GUILayout.Space(6f);
    }
}
