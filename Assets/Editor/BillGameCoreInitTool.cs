using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// BillGameCore Init Tool v2.0
/// Blueprint: Modular Architecture + VContainer + MessagePipe
/// Convention: Interface-First | No Hard References | Data-Driven
/// </summary>
public static class BillGameCoreInitTool
{
    private const string ROOT = "Assets/_Game";

    [MenuItem("BillGameCore/Initialize Project (Enterprise Standard)")]
    public static void InitializeProject()
    {
        CreateFolders();
        CreateAsmdefs();
        CreateCoreSystem();    
        CreateGlobalInterfaces();
        CreateSignals();
        CreateSamplePlayerModule();
        CreateRegistrationGuide();

        AssetDatabase.Refresh();
        Debug.Log("<color=cyan><b>[BillGameCore v2.0]</b></color> <color=green>Khởi tạo thành công!</color>");
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
            "Scripts/Interfaces/Signals",
            "Scripts/Modules/Player/Interfaces",
            "Scripts/Modules/Player/Providers",
            "Scripts/Modules/InputSystem/Providers",
            "Scripts/Editor",
            "Scripts/Scenes",
        };
        foreach (var f in folders)
            EnsureDir(Path.Combine(ROOT, f));
    }

    // ─────────────────────────────────────────────
    // ASMDEFS  (autoReferenced = false → explicit deps only)
    // ─────────────────────────────────────────────
    private static void CreateAsmdefs()
    {
        // Tier 1 – no deps
        WriteAsmdef("Scripts/Interfaces", "BillGameCore.Interfaces",
            new string[] { },
            editorOnly: false);

        // Tier 2 – depends on Interfaces + 3rd-party
        WriteAsmdef("Scripts/Core", "BillGameCore.Core",
            new[] { "BillGameCore.Interfaces", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        // Tier 3 – each Module assembly depends on Interfaces + Core only
        // Player module gets its own asmdef
        WriteAsmdef("Scripts/Modules/Player", "BillGameCore.Modules.Player",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        // Editor-only asmdef – MUST declare includePlatforms to avoid build errors
        WriteAsmdef("Scripts/Editor", "BillGameCore.Editor",
            new[] { "BillGameCore.Core", "BillGameCore.Interfaces" },
            editorOnly: true);
    }

    // ─────────────────────────────────────────────
    // CORE SYSTEM  (ProjectLifetimeScope + SceneLifetimeScope)
    // ─────────────────────────────────────────────
    private static void CreateCoreSystem()
    {
        // ── ProjectLifetimeScope ──────────────────
        WriteFile("Scripts/Core/ProjectLifetimeScope.cs", @"// [SCOPE: ProjectLifetimeScope] [LIFETIME: Singleton — alive entire game]
// [REGISTER HERE]: IInventoryService, IMoneyService, ISaveService, IAudioService
using VContainer;
using VContainer.Unity;
using MessagePipe;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Core
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
            // builder.Register<IInventoryService, InventoryManager>(Lifetime.Singleton);
            // builder.Register<IMoneyService,     MoneyManager    >(Lifetime.Singleton);
            // builder.Register<ISaveService,      SaveManager     >(Lifetime.Singleton);
            // builder.Register<IAudioService,     AudioManager    >(Lifetime.Singleton);
        }
    }
}");

        // ── SceneLifetimeScope ────────────────────
        WriteFile("Scripts/Core/SceneLifetimeScope.cs", @"// [SCOPE: SceneLifetimeScope] [LIFETIME: Scoped — alive 1 scene]
// [REGISTER HERE]: IEnemyManager, IHarvestingSystem, IBuildingSystem
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace BillGameCore.Core
{
    public class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();

            // ── Scene Signals ─────────────────────────────────
            // builder.RegisterMessageBroker<EnemyDiedSignal>(options);

            // ── Scene Services ────────────────────────────────
            // builder.Register<IEnemyManager,     EnemyManager     >(Lifetime.Scoped);
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
        // IBaseService
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
    // SIGNALS  (one file per domain — easy to extend)
    // ─────────────────────────────────────────────
    private static void CreateSignals()
    {
        // Player signals
        WriteFile("Scripts/Interfaces/Signals/PlayerSignals.cs", @"// [SIGNALS: Player] [SCOPE: Global — register in ProjectLifetimeScope]
// Usage: Inject IPublisher<PlayerLevelUpSignal> to publish,
//               ISubscriber<PlayerLevelUpSignal> to subscribe (via MessagePipe.VContainer)
namespace BillGameCore.Interfaces.Signals
{
    public struct PlayerLevelUpSignal
    {
        public int NewLevel;
        public PlayerLevelUpSignal(int level) => NewLevel = level;
    }

    // [ADD MORE PLAYER SIGNALS HERE]
    // public struct PlayerDiedSignal { }
    // public struct PlayerRespawnedSignal { public UnityEngine.Vector3 Position; }
}");

        // Combat signals (placeholder)
        WriteFile("Scripts/Interfaces/Signals/CombatSignals.cs", @"// [SIGNALS: Combat] [SCOPE: Scene — register in SceneLifetimeScope]
namespace BillGameCore.Interfaces.Signals
{
    // [ADD COMBAT SIGNALS HERE]
    // public struct EnemyDiedSignal  { public int EnemyId; }
    // public struct DamageDealtSignal { public float Amount; }
}");

        // Inventory signals (placeholder)
        WriteFile("Scripts/Interfaces/Signals/InventorySignals.cs", @"// [SIGNALS: Inventory] [SCOPE: Global — register in ProjectLifetimeScope]
namespace BillGameCore.Interfaces.Signals
{
    // [ADD INVENTORY SIGNALS HERE]
    // public struct ItemPickedUpSignal { public string ItemId; public int Quantity; }
    // public struct InventoryFullSignal { }
}");
    }

    // ─────────────────────────────────────────────
    // SAMPLE MODULE — Player
    // Convention: I[Name]Service.cs  +  [Name]Manager.cs
    // ─────────────────────────────────────────────
    private static void CreateSamplePlayerModule()
    {
        // Interface
        WriteFile("Scripts/Modules/Player/Interfaces/IPlayerService.cs", @"// [MODULE: Player] [INTERFACE]
// [REGISTER_IN: ProjectLifetimeScope → builder.Register<IPlayerService, PlayerManager>]
namespace BillGameCore.Interfaces
{
    public interface IPlayerService : IBaseService
    {
        void Move(float speed);
        int GetLevel();
    }
}");

        // Manager (implementation)
        WriteFile("Scripts/Modules/Player/Providers/PlayerManager.cs", @"// [MODULE: Player] [MANAGER]
// [SCOPE: ProjectLifetimeScope] [DEPENDS_ON: IPublisher<PlayerLevelUpSignal>]
// [SIGNAL_PUBLISHES: PlayerLevelUpSignal]
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

        // Injected by VContainer — no hard references
        private readonly IPublisher<PlayerLevelUpSignal> _levelUpPublisher;

        [Inject]
        public PlayerManager(IPublisher<PlayerLevelUpSignal> levelUpPublisher)
        {
            _levelUpPublisher = levelUpPublisher;
        }

        public void Initialize()
        {
            Debug.Log(""[PlayerManager] Initialized."");
        }

        public void Move(float speed)
        {
            Debug.Log($""[PlayerManager] Moving at speed: {speed}"");
        }

        public int GetLevel() => _level;

        public void LevelUp()
        {
            _level++;
            _levelUpPublisher.Publish(new PlayerLevelUpSignal(_level));
            Debug.Log($""[PlayerManager] Level up → {_level}"");
        }
    }
}");
    }

    // ─────────────────────────────────────────────
    // REGISTRATION GUIDE  (assembly map for multi-session work)
    // ─────────────────────────────────────────────
    private static void CreateRegistrationGuide()
    {
        WriteFile("Scripts/Core/RegistrationGuide.cs", @"// ╔══════════════════════════════════════════════════════════════════╗
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
//   [x] IPlayerService  → PlayerManager    [MODULE: Player]
//   [ ] IInventoryService → InventoryManager [MODULE: Inventory] ← TODO
//   [ ] IMoneyService   → MoneyManager     [MODULE: Economy]    ← TODO
//   [ ] ISaveService    → SaveManager      [MODULE: Save]       ← TODO
//   [ ] IAudioService   → AudioManager     [MODULE: Audio]      ← TODO
//
// ── SCENE LIFETIME SCOPE (SceneLifetimeScope.cs) ─────────────────────
//
//   SIGNALS:
//   [ ] EnemyDiedSignal              [MODULE: Combat]     ← TODO
//   [ ] DamageDealtSignal            [MODULE: Combat]     ← TODO
//
//   SERVICES:
//   [ ] IEnemyManager   → EnemyManager      [MODULE: Enemy]      ← TODO
//   [ ] IHarvestingSystem → HarvestingManager [MODULE: Harvesting] ← TODO
//   [ ] IBuildingSystem → BuildingManager   [MODULE: Building]   ← TODO
//
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
");
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

        // References
        sb.AppendLine("    \"references\": [");
        for (int i = 0; i < refs.Length; i++)
            sb.AppendLine($"        \"{refs[i]}\"{(i < refs.Length - 1 ? "," : "")}");
        sb.AppendLine("    ],");

        // Editor-only assemblies must declare includePlatforms to prevent build errors
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

        // autoReferenced = false → prevent unintended cross-assembly pollution
        sb.AppendLine("    \"autoReferenced\": false,");
        sb.AppendLine("    \"defineConstraints\": [],");
        sb.AppendLine("    \"versionDefines\": [],");
        sb.AppendLine("    \"noEngineReferences\": false");
        sb.AppendLine("}");

        EnsureDir(Path.GetDirectoryName(path));
        File.WriteAllText(path, sb.ToString());
    }
}