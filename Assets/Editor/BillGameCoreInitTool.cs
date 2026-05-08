using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// BillGameCore Init Tool v3.0
/// Blueprint: Modular Architecture + VContainer + MessagePipe
/// Convention: Interface-First | No Hard References | Data-Driven
/// Groups:
///   - Entity      → Provider + Logic + View + Spawner  (Player, Enemy, Boss, NPC, Projectile, Mount)
///   - Interaction → Logic + View                       (Chest, Door, HealPoint, Trap, Switch)
///   - Service     → Manager                            (Inventory, Economy, Save, Audio, Combat...)
/// Asmdef: Interfaces → Core → Modules.* → Composition (Composition Root)
/// </summary>
public static class BillGameCoreInitTool
{
    private const string ROOT = "Assets/_Game";

    [MenuItem("BillGameCore/Initialize Project (Enterprise Standard)")]
    public static void InitializeProject()
    {
        CreateFolders();
        CreateAsmdefs();
        CreateCompositionRoot();
        CreateGlobalInterfaces();
        CreateSignals();
        CreateSamplePlayerModule();
        CreateRegistrationGuide();

        AssetDatabase.Refresh();
        Debug.Log("<color=cyan><b>[BillGameCore v3.0]</b></color> <color=green>Khởi tạo thành công!</color>");
    }

    // ─────────────────────────────────────────────
    // MENU ITEMS — TẠO MODULE THEO NHÓM
    // ─────────────────────────────────────────────

    [MenuItem("BillGameCore/New Module/Entity (Player, Enemy, Boss, NPC...)")]
    public static void NewEntityModule()
    {
        string name = EditorInputDialog.Show("New Entity Module", "Tên module (VD: Enemy, Boss, Mount):", "Enemy");
        if (string.IsNullOrEmpty(name)) return;
        CreateEntityModule(name);
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan><b>[BillGameCore]</b></color> <color=green>Entity module '{name}' đã được tạo!</color>");
    }

    [MenuItem("BillGameCore/New Module/Interaction (Chest, Door, Trap...)")]
    public static void NewInteractionModule()
    {
        string name = EditorInputDialog.Show("New Interaction Module", "Tên module (VD: Chest, Door, HealPoint):", "Chest");
        if (string.IsNullOrEmpty(name)) return;
        CreateInteractionModule(name);
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan><b>[BillGameCore]</b></color> <color=green>Interaction module '{name}' đã được tạo!</color>");
    }

    [MenuItem("BillGameCore/New Module/Service (Inventory, Economy, Audio...)")]
    public static void NewServiceModule()
    {
        string name = EditorInputDialog.Show("New Service Module", "Tên module (VD: Inventory, Economy, Audio):", "Inventory");
        if (string.IsNullOrEmpty(name)) return;
        CreateServiceModule(name);
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan><b>[BillGameCore]</b></color> <color=green>Service module '{name}' đã được tạo!</color>");
    }

    // ─────────────────────────────────────────────
    // FOLDERS
    // ─────────────────────────────────────────────
    private static void CreateFolders()
    {
        string[] folders =
        {
            "Art",
            "Data/Items",
            "Data/Settings",
            "Scripts/Core",
            "Scripts/Composition",

            "Scripts/Interfaces/Signals",

            // Player — Entity group (mẫu)
            "Scripts/Modules/Player/Interfaces",
            "Scripts/Modules/Player/Providers",
            "Scripts/Modules/Player/Views",
            "Scripts/Modules/Player/Spawners",

            "Scripts/Editor",
            "Scripts/Scenes",
        };
        foreach (var f in folders)
            EnsureDir(Path.Combine(ROOT, f));
    }

    // ─────────────────────────────────────────────
    // FOLDER HELPERS — theo nhóm
    // ─────────────────────────────────────────────

    private static void CreateEntityFolders(string moduleName)
    {
        string[] folders =
        {
            $"Scripts/Modules/{moduleName}/Interfaces",
            $"Scripts/Modules/{moduleName}/Providers",
            $"Scripts/Modules/{moduleName}/Views",
            $"Scripts/Modules/{moduleName}/Spawners",
        };
        foreach (var f in folders)
            EnsureDir(Path.Combine(ROOT, f));
    }

    private static void CreateInteractionFolders(string moduleName)
    {
        string[] folders =
        {
            $"Scripts/Modules/{moduleName}/Interfaces",  // tạo sẵn, dùng khi cần
            $"Scripts/Modules/{moduleName}/Providers",
            $"Scripts/Modules/{moduleName}/Views",
        };
        foreach (var f in folders)
            EnsureDir(Path.Combine(ROOT, f));
    }

    private static void CreateServiceFolders(string moduleName)
    {
        string[] folders =
        {
            $"Scripts/Modules/{moduleName}/Interfaces",
            $"Scripts/Modules/{moduleName}/Providers",
        };
        foreach (var f in folders)
            EnsureDir(Path.Combine(ROOT, f));
    }

    // ─────────────────────────────────────────────
    // ASMDEFS
    // ─────────────────────────────────────────────
    private static void CreateAsmdefs()
    {
        WriteAsmdef("Scripts/Interfaces", "BillGameCore.Interfaces",
            new string[] { }, editorOnly: false);

        WriteAsmdef("Scripts/Core", "BillGameCore.Core",
            new[] { "BillGameCore.Interfaces", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        WriteAsmdef("Scripts/Modules/Player", "BillGameCore.Modules.Player",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        WriteAsmdef("Scripts/Composition", "BillGameCore.Composition",
            new[] {
                "BillGameCore.Interfaces",
                "BillGameCore.Core",
                "BillGameCore.Modules.Player",
                // [ADD NEW MODULE ASMDEFS HERE]
                "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer"
            },
            editorOnly: false);

        WriteAsmdef("Scripts/Editor", "BillGameCore.Editor",
            new[] { "BillGameCore.Core", "BillGameCore.Interfaces" },
            editorOnly: true);
    }

    // ─────────────────────────────────────────────
    // MODULE GENERATORS
    // ─────────────────────────────────────────────

    /// <summary>
    /// Entity Module: Provider + Logic + View + Spawner + Interface + Signal + Asmdef
    /// Áp dụng cho: Player, Enemy, Boss, NPC, Projectile, Mount
    /// </summary>
    private static void CreateEntityModule(string name)
    {
        string lower = name.ToLower();
        CreateEntityFolders(name);

        // Signal
        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs", $@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// Thêm vào SceneLifetimeScope: builder.RegisterMessageBroker<{name}DiedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}DiedSignal
    {{
        public int EntityId;
        public {name}DiedSignal(int id) => EntityId = id;
    }}
    // [ADD MORE {name.ToUpper()} SIGNALS HERE]
}}");

        // Interface
        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs", $@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// [REGISTER_IN: SceneLifetimeScope.cs → Scene Services block]
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service : IBaseService
    {{
        // khai báo capability của entity
        int GetId();
    }}
}}");

        // Provider — bridge Unity input/data → Logic
        WriteFile($"Scripts/Modules/{name}/Providers/{name}Provider.cs", $@"// [MODULE: {name}]
// [TYPE: Provider]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: {name}Logic]
// [SIGNAL_PUBLISHES: none]
// [SIGNAL_SUBSCRIBES: none]
// Provider: đọc input / data từ Unity-world và đẩy vào Logic
using VContainer;
using UnityEngine;

namespace BillGameCore.Modules.{name}
{{
    public class {name}Provider : MonoBehaviour
    {{
        private {name}Logic _logic;

        [Inject]
        public void Construct({name}Logic logic)
        {{
            _logic = logic;
        }}

        private void Update()
        {{
            // Bridge Unity input → Logic
            // VD: _logic.SetMoveDirection(Input.GetAxis(...));
        }}
    }}
}}");

        // Logic — pure calculation, no Unity knowledge
        WriteFile($"Scripts/Modules/{name}/Providers/{name}Logic.cs", $@"// [MODULE: {name}]
// [TYPE: Logic]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: {name}DiedSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// Logic: thuần tính toán — KHÔNG using UnityEngine (ngoại lệ: Vector2, Mathf)
using MessagePipe;
using VContainer;

namespace BillGameCore.Modules.{name}
{{
    public class {name}Logic
    {{
        private readonly IPublisher<Interfaces.Signals.{name}DiedSignal> _publisher;

        [Inject]
        public {name}Logic(IPublisher<Interfaces.Signals.{name}DiedSignal> publisher)
        {{
            _publisher = publisher;
        }}

        public void Initialize() {{ }}

        // Thuần tính toán — KHÔNG MonoBehaviour, KHÔNG Transform trực tiếp
        // Dùng Vector2/Mathf nếu cần
        public void OnDie(int id)
        {{
            _publisher.Publish(new Interfaces.Signals.{name}DiedSignal(id));
        }}
    }}
}}");

        // View — MonoBehaviour, display only
        WriteFile($"Scripts/Modules/{name}/Views/{name}View.cs", $@"// [MODULE: {name}]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: none]
// View: MonoBehaviour — chỉ hiển thị / animation / VFX
// KHÔNG chứa if/else logic game
using UnityEngine;

namespace BillGameCore.Modules.{name}
{{
    public class {name}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        // Gọi từ Provider hoặc Logic thông qua event/delegate
        public void PlayIdleAnimation() => _animator.SetTrigger(""Idle"");
        public void PlayDieAnimation() => _animator.SetTrigger(""Die"");
        public void PlayAttackAnimation() => _animator.SetTrigger(""Attack"");
    }}
}}");

        // Spawner — creates body and injects logic
        WriteFile($"Scripts/Modules/{name}/Spawners/{name}Spawner.cs", $@"// [MODULE: {name}]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: IObjectResolver]
// Spawner: tạo thân thể (Instantiate), bơm logic vào — KHÔNG nhúng vào Scope trực tiếp
using VContainer;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Modules.{name}
{{
    public class {name}Spawner
    {{
        private readonly IObjectResolver _resolver;

        // Kéo prefab vào qua RegisterInstance hoặc ScriptableObject config
        private GameObject _prefab;

        [Inject]
        public {name}Spawner(IObjectResolver resolver)
        {{
            _resolver = resolver;
        }}

        public {name}View Spawn(Vector3 position)
        {{
            var go = Object.Instantiate(_prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(go);   // VContainer inject vào tất cả MonoBehaviour trong prefab
            return go.GetComponent<{name}View>();
        }}
    }}
}}");

        // Asmdef
        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Entity");
    }

    /// <summary>
    /// Interaction Module: Logic + View + (optional Interface + Signal) + Asmdef
    /// Áp dụng cho: Chest, Door, HealPoint, Trap, Switch, Shrine...
    /// </summary>
    private static void CreateInteractionModule(string name)
    {
        CreateInteractionFolders(name);

        // Signal (tạo sẵn — uncommnet khi cần)
        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs", $@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// Uncomment và đăng ký trong SceneLifetimeScope nếu cần:
// builder.RegisterMessageBroker<{name}ActivatedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}ActivatedSignal
    {{
        public int ObjectId;
        public {name}ActivatedSignal(int id) => ObjectId = id;
    }}
}}");

        // Interface (tạo sẵn — chỉ dùng nếu module khác cần inject)
        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs", $@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// CHỈ tạo Interface này nếu module khác cần inject I{name}Service.
// Nếu không có module nào dùng → có thể xoá file này.
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service
    {{
        void Interact();
        bool IsActivated {{ get; }}
    }}
}}");

        // Logic — state machine, pure C#
        WriteFile($"Scripts/Modules/{name}/Providers/{name}Logic.cs", $@"// [MODULE: {name}]
// [TYPE: Logic]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: {name}ActivatedSignal]
// [DEPENDS_ON: none]
// Logic: xử lý trạng thái — KHÔNG biết Unity
using MessagePipe;
using VContainer;
using System;

namespace BillGameCore.Modules.{name}
{{
    public class {name}Logic
    {{
        private bool _isActivated;

        public bool IsActivated => _isActivated;

        // Callback để View react khi trạng thái thay đổi (thay vì direct reference)
        public event Action OnActivated;
        public event Action OnReset;

        private readonly IPublisher<Interfaces.Signals.{name}ActivatedSignal> _publisher;

        [Inject]
        public {name}Logic(IPublisher<Interfaces.Signals.{name}ActivatedSignal> publisher)
        {{
            _publisher = publisher;
        }}

        public void Interact(int objectId)
        {{
            if (_isActivated) return;
            _isActivated = true;
            _publisher.Publish(new Interfaces.Signals.{name}ActivatedSignal(objectId));
            OnActivated?.Invoke();
        }}

        public void Reset()
        {{
            _isActivated = false;
            OnReset?.Invoke();
        }}
    }}
}}");

        // View — MonoBehaviour, animation + visual feedback
        WriteFile($"Scripts/Modules/{name}/Views/{name}View.cs", $@"// [MODULE: {name}]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: {name}Logic]
// View: MonoBehaviour — animation + visual feedback khi trạng thái thay đổi
// Nhận lệnh từ Logic qua event — KHÔNG chứa logic game
using VContainer;
using UnityEngine;

namespace BillGameCore.Modules.{name}
{{
    public class {name}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        private {name}Logic _logic;

        [Inject]
        public void Construct({name}Logic logic)
        {{
            _logic = logic;
            _logic.OnActivated += HandleActivated;
            _logic.OnReset     += HandleReset;
        }}

        private void OnDestroy()
        {{
            if (_logic == null) return;
            _logic.OnActivated -= HandleActivated;
            _logic.OnReset     -= HandleReset;
        }}

        private void OnTriggerEnter2D(Collider2D other)
        {{
            if (other.CompareTag(""Player""))
                _logic.Interact(gameObject.GetInstanceID());
        }}

        private void HandleActivated() => _animator.SetTrigger(""Activate"");
        private void HandleReset()     => _animator.SetTrigger(""Reset"");
    }}
}}");

        // Asmdef
        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Interaction");
    }

    /// <summary>
    /// Service Module: Interface + Manager + Signal + Asmdef
    /// Áp dụng cho: Inventory, Economy, Save, Audio, Combat, Harvesting, Building...
    /// </summary>
    private static void CreateServiceModule(string name)
    {
        CreateServiceFolders(name);

        // Signal
        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs", $@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: ProjectLifetimeScope]
// Thêm vào ProjectLifetimeScope: builder.RegisterMessageBroker<{name}ChangedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}ChangedSignal
    {{
        // thêm dữ liệu cần truyền
        public {name}ChangedSignal(int dummy) {{ }}
    }}
    // [ADD MORE {name.ToUpper()} SIGNALS HERE]
}}");

        // Interface
        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs", $@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: ProjectLifetimeScope]
// [REGISTER_IN: ProjectLifetimeScope.cs → Global Services block]
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service : IBaseService
    {{
        // khai báo các method public cần thiết
    }}
}}");

        // Manager
        WriteFile($"Scripts/Modules/{name}/Providers/{name}Manager.cs", $@"// [MODULE: {name}]
// [TYPE: Manager]
// [SCOPE: ProjectLifetimeScope]
// [SIGNAL_PUBLISHES: {name}ChangedSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: Scripts/Composition/ProjectLifetimeScope.cs → Global Services block]
using MessagePipe;
using VContainer;
using UnityEngine;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Modules.{name}
{{
    public class {name}Manager : I{name}Service
    {{
        private readonly IPublisher<{name}ChangedSignal> _publisher;

        [Inject]
        public {name}Manager(IPublisher<{name}ChangedSignal> publisher)
        {{
            _publisher = publisher;
        }}

        public void Initialize()
        {{
            Debug.Log($""[{name}Manager] Initialized."");
        }}

        // TODO: implement I{name}Service
    }}
}}");

        // Asmdef
        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Service");
    }

    // ─────────────────────────────────────────────
    // SAMPLE PLAYER MODULE (chạy lần đầu)
    // ─────────────────────────────────────────────
    private static void CreateSamplePlayerModule()
    {
        // Interface
        WriteFile("Scripts/Modules/Player/Interfaces/IPlayerService.cs", @"// [MODULE: Player]
// [TYPE: Interface]
// [SCOPE: ProjectLifetimeScope]
// [REGISTER_IN: ProjectLifetimeScope → builder.Register<IPlayerService, PlayerManager>]
namespace BillGameCore.Interfaces
{
    public interface IPlayerService : IBaseService
    {
        void Move(float speed);
        int GetLevel();
        void LevelUp();
    }
}");

        // Provider
        WriteFile("Scripts/Modules/Player/Providers/PlayerProvider.cs", @"// [MODULE: Player]
// [TYPE: Provider]
// [SCOPE: ProjectLifetimeScope (hoặc Scene nếu tách riêng)]
// [DEPENDS_ON: IPlayerService, IInputService]
// Provider: bridge Unity input → PlayerLogic
// TODO: thay bằng IInputService khi module Input đã có
using VContainer;
using UnityEngine;

namespace BillGameCore.Modules.Player
{
    public class PlayerProvider : MonoBehaviour
    {
        [Inject] private IPlayerService _playerService;

        private void Update()
        {
            // Tạm: dùng legacy Input để test trước khi có IInputService
            float h = UnityEngine.Input.GetAxisRaw(""Horizontal"");
            float v = UnityEngine.Input.GetAxisRaw(""Vertical"");
            if (h != 0 || v != 0)
                _playerService.Move(new UnityEngine.Vector2(h, v).magnitude);
        }
    }
}");

        // Logic (giữ PlayerManager cho backward-compat với Scope hiện tại)
        WriteFile("Scripts/Modules/Player/Providers/PlayerManager.cs", @"// [MODULE: Player]
// [TYPE: Manager]
// [SCOPE: ProjectLifetimeScope]
// [SIGNAL_PUBLISHES: PlayerLevelUpSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: Scripts/Composition/ProjectLifetimeScope.cs → Global Services block]
using MessagePipe;
using VContainer;
using UnityEngine;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Modules.Player
{
    public class PlayerManager : IPlayerService
    {
        private int _level = 1;
        private readonly IPublisher<PlayerLevelUpSignal> _publisher;

        [Inject]
        public PlayerManager(IPublisher<PlayerLevelUpSignal> publisher)
        {
            _publisher = publisher;
        }

        public void Initialize() => Debug.Log(""[PlayerManager] Initialized."");
        public void Move(float speed) => Debug.Log($""[PlayerManager] Speed: {speed}"");
        public int GetLevel() => _level;

        public void LevelUp()
        {
            _level++;
            _publisher.Publish(new PlayerLevelUpSignal(_level));
            Debug.Log($""[PlayerManager] Level up → {_level}"");
        }
    }
}");

        // View
        WriteFile("Scripts/Modules/Player/Views/PlayerView.cs", @"// [MODULE: Player]
// [TYPE: View]
// [SCOPE: Scene]
// [DEPENDS_ON: none]
// View: chỉ hiển thị / animation. KHÔNG chứa logic game.
using UnityEngine;

namespace BillGameCore.Modules.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetMoving(bool isMoving) => _animator.SetBool(""isMoving"", isMoving);
        public void SetFlipX(bool flipX) => _spriteRenderer.flipX = flipX;
        public void PlayLevelUpVFX() => _animator.SetTrigger(""LevelUp"");
    }
}");

        // Spawner (optional cho Player vì thường singleton)
        WriteFile("Scripts/Modules/Player/Spawners/PlayerSpawner.cs", @"// [MODULE: Player]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: IObjectResolver]
// Spawner: tạo Player prefab và inject toàn bộ dependencies vào
// Optional cho Player vì thường chỉ có 1 Player trong scene
using VContainer;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Modules.Player
{
    public class PlayerSpawner
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public PlayerSpawner(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public PlayerView Spawn(GameObject prefab, Vector3 position)
        {
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(go);
            return go.GetComponent<PlayerView>();
        }
    }
}");
    }

    // ─────────────────────────────────────────────
    // COMPOSITION ROOT
    // ─────────────────────────────────────────────
    private static void CreateCompositionRoot()
    {
        WriteFile("Scripts/Composition/ProjectLifetimeScope.cs", @"// [SCOPE: ProjectLifetimeScope] [LIFETIME: Singleton — alive entire game]
// [ASSEMBLY: BillGameCore.Composition — được phép depend tất cả Modules]
// [REGISTER HERE]: Tất cả service sống suốt game
// Khi thêm module mới: thêm Register vào đây VÀ thêm asmdef ref vào BillGameCore.Composition.asmdef
using VContainer;
using VContainer.Unity;
using MessagePipe;
using BillGameCore.Interfaces.Signals;
using BillGameCore.Modules.Player;

namespace BillGameCore.Composition
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();

            // ── Global Signals ────────────────────────────────
            builder.RegisterMessageBroker<PlayerLevelUpSignal>(options);
            // [ADD GLOBAL SIGNALS HERE]

            // ── Global Services ───────────────────────────────
            builder.Register<IPlayerService, PlayerManager>(Lifetime.Singleton);
            // builder.Register<IInventoryService, InventoryManager>(Lifetime.Singleton);
            // builder.Register<IMoneyService,     MoneyManager    >(Lifetime.Singleton);
            // builder.Register<ISaveService,      SaveManager     >(Lifetime.Singleton);
            // builder.Register<IAudioService,     AudioManager    >(Lifetime.Singleton);
        }
    }
}");

        WriteFile("Scripts/Composition/SceneLifetimeScope.cs", @"// [SCOPE: SceneLifetimeScope] [LIFETIME: Scoped — alive 1 scene]
// [ASSEMBLY: BillGameCore.Composition — được phép depend tất cả Modules]
// [REGISTER HERE]: Tất cả service chỉ sống trong 1 scene
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace BillGameCore.Composition
{
    public class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();

            // ── Scene Signals ─────────────────────────────────
            // Entity signals:
            // builder.RegisterMessageBroker<EnemyDiedSignal>(options);
            // Interaction signals:
            // builder.RegisterMessageBroker<ChestActivatedSignal>(options);

            // ── Scene Services — Entities ─────────────────────
            // builder.Register<EnemyLogic>(Lifetime.Transient);
            // builder.Register<EnemySpawner>(Lifetime.Scoped);
            // builder.RegisterComponentInHierarchy<EnemyProvider>();

            // ── Scene Services — Interactions ─────────────────
            // builder.Register<ChestLogic>(Lifetime.Transient);
            // builder.RegisterComponentInHierarchy<ChestView>();

            // ── Scene Services — Service Modules ─────────────
            // builder.Register<IHarvestingSystem, HarvestingManager>(Lifetime.Scoped);
        }
    }
}");
    }

    // ─────────────────────────────────────────────
    // GLOBAL INTERFACES
    // ─────────────────────────────────────────────
    private static void CreateGlobalInterfaces()
    {
        WriteFile("Scripts/Interfaces/IBaseService.cs", @"// [INTERFACE: Global] All services optionally implement this.
namespace BillGameCore.Interfaces
{
    public interface IBaseService
    {
        void Initialize();
    }
}");
    }

    // ─────────────────────────────────────────────
    // SIGNALS
    // ─────────────────────────────────────────────
    private static void CreateSignals()
    {
        WriteFile("Scripts/Interfaces/Signals/PlayerSignals.cs", @"// [SIGNALS: Player] [SCOPE: Global — register in ProjectLifetimeScope]
namespace BillGameCore.Interfaces.Signals
{
    public struct PlayerLevelUpSignal
    {
        public int NewLevel;
        public PlayerLevelUpSignal(int level) => NewLevel = level;
    }
    // [ADD MORE PLAYER SIGNALS HERE]
}");
    }

    // ─────────────────────────────────────────────
    // REGISTRATION GUIDE
    // ─────────────────────────────────────────────
    private static void CreateRegistrationGuide()
    {
        WriteFile("Scripts/Composition/RegistrationGuide.cs", @"// ╔══════════════════════════════════════════════════════════════════════╗
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
");
    }

    // ─────────────────────────────────────────────
    // NEXT STEPS LOGGER
    // ─────────────────────────────────────────────
    private static void LogNextSteps(string name, string group)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<color=cyan><b>[BillGameCore — {group} Module '{name}']</b></color>");
        sb.AppendLine("<color=yellow>⚠ 3 bước còn lại bạn phải làm thủ công:</color>");
        sb.AppendLine("");

        if (group == "Entity" || group == "Service")
        {
            string scope = group == "Entity" ? "SceneLifetimeScope" : "ProjectLifetimeScope";
            sb.AppendLine($"<b>1. Thêm asmdef vào Composition:</b>");
            sb.AppendLine($"   → Mở <b>BillGameCore.Composition.asmdef</b>");
            sb.AppendLine($"   → Thêm: <b>\"BillGameCore.Modules.{name}\"</b> vào mảng references");
            sb.AppendLine("");
            sb.AppendLine($"<b>2. Register signal trong {scope}.cs:</b>");
            sb.AppendLine($"   builder.RegisterMessageBroker<{name}DiedSignal>(options);");
            sb.AppendLine("");
            sb.AppendLine($"<b>3. Register service trong {scope}.cs:</b>");
            if (group == "Entity")
            {
                sb.AppendLine($"   builder.Register<{name}Logic>(Lifetime.Transient);");
                sb.AppendLine($"   builder.Register<{name}Spawner>(Lifetime.Scoped);");
                sb.AppendLine($"   builder.RegisterComponentInHierarchy<{name}Provider>();");
            }
            else
            {
                sb.AppendLine($"   builder.Register<I{name}Service, {name}Manager>(Lifetime.Singleton);");
            }
        }
        else // Interaction
        {
            sb.AppendLine($"<b>1. (Tuỳ chọn) Thêm asmdef vào Composition nếu có I{name}Service:</b>");
            sb.AppendLine($"   → Thêm: <b>\"BillGameCore.Modules.{name}\"</b> vào BillGameCore.Composition.asmdef");
            sb.AppendLine("");
            sb.AppendLine($"<b>2. Register trong SceneLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.Register<{name}Logic>(Lifetime.Transient);");
            sb.AppendLine($"   builder.RegisterComponentInHierarchy<{name}View>();");
            sb.AppendLine("");
            sb.AppendLine($"<b>3. (Tuỳ chọn) Register signal nếu dùng:</b>");
            sb.AppendLine($"   builder.RegisterMessageBroker<{name}ActivatedSignal>(options);");
        }

        sb.AppendLine("");
        sb.AppendLine("<color=green>✓ Sau đó cập nhật Phần 12 trong CONTEXT.md</color>");
        Debug.Log(sb.ToString());
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────
    private static void WriteFile(string relativePath, string content)
    {
        string full = Path.Combine(ROOT, relativePath);
        if (!File.Exists(full))
            File.WriteAllText(full, content, Encoding.UTF8);
    }

    private static void EnsureDir(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private static void WriteAsmdef(string folderRelative, string name, string[] refs, bool editorOnly)
    {
        string path = Path.Combine(ROOT, folderRelative, name + ".asmdef");
        if (File.Exists(path)) return;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"    \"name\": \"{name}\",");
        sb.AppendLine("    \"references\": [");
        for (int i = 0; i < refs.Length; i++)
            sb.AppendLine($"        \"{refs[i]}\"{(i < refs.Length - 1 ? "," : "")}");
        sb.AppendLine("    ],");
        if (editorOnly)
        {
            sb.AppendLine("    \"includePlatforms\": [\"Editor\"],");
            sb.AppendLine("    \"excludePlatforms\": [],");
        }
        else
        {
            sb.AppendLine("    \"includePlatforms\": [],");
            sb.AppendLine("    \"excludePlatforms\": [],");
        }
        sb.AppendLine("    \"autoReferenced\": false,");
        sb.AppendLine("    \"defineConstraints\": [],");
        sb.AppendLine("    \"versionDefines\": [],");
        sb.AppendLine("    \"noEngineReferences\": false");
        sb.AppendLine("}");

        EnsureDir(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString());
    }
}

/// <summary>
/// Simple input dialog for Unity Editor.
/// </summary>
public class EditorInputDialog : EditorWindow
{
    private static string _result;
    private string _label;
    private string _input;

    public static string Show(string title, string label, string defaultValue = "")
    {
        _result = null;
        var win = CreateInstance<EditorInputDialog>();
        win.titleContent = new GUIContent(title);
        win._label = label;
        win._input = defaultValue;
        win.minSize = win.maxSize = new Vector2(340, 90);
        win.ShowModal();
        return _result;
    }

    private void OnGUI()
    {
        GUILayout.Space(12);
        GUILayout.Label(_label, EditorStyles.wordWrappedLabel);
        GUILayout.Space(4);
        GUI.SetNextControlName("InputField");
        _input = EditorGUILayout.TextField(_input);
        EditorGUI.FocusTextInControl("InputField");
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel")) Close();
        if (GUILayout.Button("Create") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            _result = _input;
            Close();
        }
        GUILayout.EndHorizontal();
    }
}
