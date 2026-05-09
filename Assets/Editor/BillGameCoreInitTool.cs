using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor.Callbacks;
/// <summary>
/// BillGameCore Init Tool v3.1
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
    private const string PENDING_LOG_KEY = "BillGameCore_PendingLog"; // Thêm dòng này để định danh bộ nhớ tạm

    private const string ROOT = "Assets/_Game";

    [MenuItem("BillGameCore/Initialize Project (Enterprise Standard)")]
    public static void InitializeProject()
    {
        CreateFolders();
        CreateAsmdefs();
        CreateCompositionRoot();
        CreateGlobalInterfaces();
        CreateSignals();

        AssetDatabase.Refresh();
        //Debug.Log("<color=cyan><b>[BillGameCore v3.1]</b></color> <color=green>Khởi tạo thành công!</color>");
        SessionState.SetString(PENDING_LOG_KEY, "<color=cyan><b>[BillGameCore v3.1]</b></color> <color=green>Khởi tạo thành công!</color>");
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
            "Scripts/Modules",
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
            $"Scripts/Modules/{moduleName}/Interfaces",
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

        WriteAsmdef("Scripts/Composition", "BillGameCore.Composition",
            new[] {
                "BillGameCore.Interfaces",
                "BillGameCore.Core",
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
    /// Entity Module: folders + Signal + Interface + Provider + Logic + View + Spawner + Asmdef
    /// Áp dụng cho: Player, Enemy, Boss, NPC, Projectile, Mount
    /// </summary>
    private static void CreateEntityModule(string name)
    {
        CreateEntityFolders(name);

        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs",
$@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// TODO: đăng ký trong SceneLifetimeScope: builder.RegisterMessageBroker<{name}DiedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}DiedSignal
    {{
        public int EntityId;
        public {name}DiedSignal(int id) => EntityId = id;
    }}
    // [ADD MORE {name.ToUpper()} SIGNALS HERE]
}}");

        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs",
$@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// [REGISTER_IN: SceneLifetimeScope.cs → Scene Services block]
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service : IBaseService
    {{
        // TODO: khai báo capability của entity
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Providers/{name}Provider.cs",
$@"// [MODULE: {name}]
// [TYPE: Provider]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: none]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: {name}Logic]
// Provider: MonoBehaviour — bridge Unity world → Logic (input, physics data...)
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
            // TODO: đọc data từ Unity world, đẩy vào _logic
        }}
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Providers/{name}Logic.cs",
$@"// [MODULE: {name}]
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

        // TODO: implement logic thuần C#
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Views/{name}View.cs",
$@"// [MODULE: {name}]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: none]
// View: MonoBehaviour — chỉ animation / VFX / sound cue
// KHÔNG chứa if/else logic game
using UnityEngine;

namespace BillGameCore.Modules.{name}
{{
    public class {name}View : MonoBehaviour
    {{
        [SerializeField] private Animator _animator;

        // TODO: các method hiển thị — gọi từ Provider qua event/delegate binh boong
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Spawners/{name}Spawner.cs",
$@"// [MODULE: {name}]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: IObjectResolver]
// Spawner: Instantiate prefab, inject dependencies qua VContainer — class riêng, KHÔNG nhúng vào Scope
using VContainer;
using UnityEngine;
using VContainer.Unity;
namespace BillGameCore.Modules.{name}
{{
    public class {name}Spawner
    {{
        private readonly IObjectResolver _resolver;

        [Inject]
        public {name}Spawner(IObjectResolver resolver)
        {{
            _resolver = resolver;
        }}

        public {name}View Spawn(GameObject prefab, Vector3 position)
        {{
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(go);
            return go.GetComponent<{name}View>();
        }}
    }}
}}");

        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Entity");
    }

    /// <summary>
    /// Interaction Module: folders + Signal + Interface + Logic + View + Asmdef
    /// Áp dụng cho: Chest, Door, HealPoint, Trap, Switch, Shrine...
    /// </summary>
    private static void CreateInteractionModule(string name)
    {
        CreateInteractionFolders(name);

        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs",
$@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// TODO: uncomment và đăng ký trong SceneLifetimeScope nếu cần:
// builder.RegisterMessageBroker<{name}ActivatedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}ActivatedSignal
    {{
         public UnityEngine.EntityId ObjectId; // int → EntityId
        public {name}ActivatedSignal(UnityEngine.EntityId objectId) => ObjectId = objectId;
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs",
$@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// CHỈ giữ file này nếu module khác cần inject I{name}Service.
// Nếu không → xoá file này.
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service
    {{
        void Interact();
        bool IsActivated {{ get; }}
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Providers/{name}Logic.cs",
$@"// [MODULE: {name}]
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
        public bool IsActivated {{ get; private set; }}

        public event Action OnActivated;
        public event Action OnReset;

        private readonly IPublisher<Interfaces.Signals.{name}ActivatedSignal> _publisher;

        [Inject]
        public {name}Logic(IPublisher<Interfaces.Signals.{name}ActivatedSignal> publisher)
        {{
            _publisher = publisher;
        }}

        public void Interact(UnityEngine.EntityId objectId)
        {{
            if (IsActivated) return;
            IsActivated = true;
            _publisher.Publish(new Interfaces.Signals.{name}ActivatedSignal(objectId));
            OnActivated?.Invoke();
        }}

        public void Reset()
        {{
            IsActivated = false;
            OnReset?.Invoke();
        }}
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Views/{name}View.cs",
$@"// [MODULE: {name}]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: {name}Logic]
// View: MonoBehaviour — animation + visual feedback khi trạng thái thay đổi
// KHÔNG chứa logic game
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
                _logic.Interact(GetEntityId());
        }}

        private void HandleActivated() {{ /* TODO: trigger animation */ }}
        private void HandleReset()     {{ /* TODO: trigger reset animation */ }}
    }}
}}");

        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Interaction");
    }

    /// <summary>
    /// Service Module: folders + Signal + Interface + Manager + Asmdef
    /// Áp dụng cho: Inventory, Economy, Save, Audio, Combat, Harvesting, Building...
    /// </summary>
    private static void CreateServiceModule(string name)
    {
        CreateServiceFolders(name);

        WriteFile($"Scripts/Interfaces/Signals/{name}Signals.cs",
$@"// [MODULE: {name}]
// [TYPE: Signal]
// [SCOPE: ProjectLifetimeScope]
// TODO: đăng ký trong ProjectLifetimeScope: builder.RegisterMessageBroker<{name}ChangedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{{
    public struct {name}ChangedSignal
    {{
        // TODO: thêm dữ liệu cần truyền
    }}
    // [ADD MORE {name.ToUpper()} SIGNALS HERE]
}}");

        WriteFile($"Scripts/Modules/{name}/Interfaces/I{name}Service.cs",
$@"// [MODULE: {name}]
// [TYPE: Interface]
// [SCOPE: ProjectLifetimeScope]
// [REGISTER_IN: ProjectLifetimeScope.cs → Global Services block]
namespace BillGameCore.Interfaces
{{
    public interface I{name}Service : IBaseService
    {{
        // TODO: khai báo các method public cần thiết
    }}
}}");

        WriteFile($"Scripts/Modules/{name}/Providers/{name}Manager.cs",
$@"// [MODULE: {name}]
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

        WriteAsmdef($"Scripts/Modules/{name}", $"BillGameCore.Modules.{name}",
            new[] { "BillGameCore.Interfaces", "BillGameCore.Core", "VContainer", "VContainer.Unity", "MessagePipe", "MessagePipe.VContainer" },
            editorOnly: false);

        LogNextSteps(name, "Service");
    }

    // ─────────────────────────────────────────────
    // COMPOSITION ROOT
    // ─────────────────────────────────────────────
    private static void CreateCompositionRoot()
    {
        WriteFile("Scripts/Composition/ProjectLifetimeScope.cs",
@"// [SCOPE: ProjectLifetimeScope] [LIFETIME: Singleton — alive entire game]
// [REGISTER HERE]: tất cả service sống suốt game
// Khi thêm module mới: thêm Register vào đây VÀ thêm asmdef vào BillGameCore.Composition.asmdef
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace BillGameCore.Composition
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();

            // ── Global Signals ────────────────────────────────
            // [ADD GLOBAL SIGNALS HERE]

            // ── Global Services ───────────────────────────────
            // [ADD GLOBAL SERVICES HERE]
        }
    }
}");

        WriteFile("Scripts/Composition/SceneLifetimeScope.cs",
@"// [SCOPE: SceneLifetimeScope] [LIFETIME: Scoped — alive 1 scene]
// [REGISTER HERE]: tất cả service chỉ sống trong 1 scene
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
            // [ADD SCENE SIGNALS HERE]

            // ── Scene Services — Entities ─────────────────────
            // [ADD ENTITY REGISTRATIONS HERE]

            // ── Scene Services — Interactions ─────────────────
            // [ADD INTERACTION REGISTRATIONS HERE]

            // ── Scene Services — Service Modules ─────────────
            // [ADD SCENE SERVICE REGISTRATIONS HERE]
        }
    }
}");
    }

    // ─────────────────────────────────────────────
    // GLOBAL INTERFACES
    // ─────────────────────────────────────────────
    private static void CreateGlobalInterfaces()
    {
        WriteFile("Scripts/Interfaces/IBaseService.cs",
@"// [INTERFACE: Global] All services optionally implement this.
namespace BillGameCore.Interfaces
{
    public interface IBaseService
    {
        void Initialize();
    }
}");
    }

    // ─────────────────────────────────────────────
    // SIGNALS — placeholder file duy nhất lúc init
    // ─────────────────────────────────────────────
    private static void CreateSignals()
    {
        WriteFile("Scripts/Interfaces/Signals/.gitkeep", "");
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

        if (group == "Entity")
        {
            sb.AppendLine("<b>1. Thêm asmdef vào Composition:</b>");
            sb.AppendLine($"   BillGameCore.Composition.asmdef → thêm \"BillGameCore.Modules.{name}\"");
            sb.AppendLine("");
            sb.AppendLine("<b>2. Đăng ký signal trong SceneLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.RegisterMessageBroker<{name}DiedSignal>(options);");
            sb.AppendLine("");
            sb.AppendLine("<b>3. Đăng ký components trong SceneLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.Register<{name}Logic>(Lifetime.Transient);");
            sb.AppendLine($"   builder.Register<{name}Spawner>(Lifetime.Scoped);");
            sb.AppendLine($"   builder.RegisterComponentInHierarchy<{name}Provider>();");
        }
        else if (group == "Interaction")
        {
            sb.AppendLine("<b>1. (Tuỳ chọn) Thêm asmdef vào Composition nếu có interface:</b>");
            sb.AppendLine($"   BillGameCore.Composition.asmdef → thêm \"BillGameCore.Modules.{name}\"");
            sb.AppendLine("");
            sb.AppendLine("<b>2. Đăng ký trong SceneLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.Register<{name}Logic>(Lifetime.Transient);");
            sb.AppendLine($"   builder.RegisterComponentInHierarchy<{name}View>();");
            sb.AppendLine("");
            sb.AppendLine("<b>3. (Tuỳ chọn) Đăng ký signal nếu dùng:</b>");
            sb.AppendLine($"   builder.RegisterMessageBroker<{name}ActivatedSignal>(options);");
        }
        else // Service
        {
            sb.AppendLine("<b>1. Thêm asmdef vào Composition:</b>");
            sb.AppendLine($"   BillGameCore.Composition.asmdef → thêm \"BillGameCore.Modules.{name}\"");
            sb.AppendLine("");
            sb.AppendLine("<b>2. Đăng ký signal trong ProjectLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.RegisterMessageBroker<{name}ChangedSignal>(options);");
            sb.AppendLine("");
            sb.AppendLine("<b>3. Đăng ký service trong ProjectLifetimeScope.cs:</b>");
            sb.AppendLine($"   builder.Register<I{name}Service, {name}Manager>(Lifetime.Singleton);");
        }

        sb.AppendLine("");
        sb.AppendLine("<color=green>✓ Sau đó cập nhật Phần 12 trong CONTEXT.md</color>");
        //Debug.Log(sb.ToString());
        // Sửa đoạn cuối: Thay Debug.Log bằng cửa sổ hướng dẫn
        BillGameCoreStepsWindow.ShowWindow($"Next Steps: {group} Module '{name}'", sb.ToString());
        // Sửa đoạn cuối: Lưu vào SessionState thay vì Debug.Log
        SessionState.SetString(PENDING_LOG_KEY, sb.ToString());
    }
    // ─────────────────────────────────────────────
    // NEXT STEPS LOGGER — Sửa để hiện log
    // ─────────────────────────────────────────────
    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // Kiểm tra xem có log nào đang chờ in không
        string pendingLog = SessionState.GetString(PENDING_LOG_KEY, "");

        if (!string.IsNullOrEmpty(pendingLog))
        {
            // In ra Console (Lúc này "Clear on Recompile" đã chạy xong nên log sẽ không bị mất)
            Debug.Log(pendingLog);

            // Xóa log trong bộ nhớ để không bị lặp lại lần sau
            SessionState.EraseString(PENDING_LOG_KEY);
        }
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
public class BillGameCoreStepsWindow : EditorWindow
{
    private string _steps;
    private Vector2 _scroll;
    private GUIStyle _richLabelStyle;
    private GUIStyle _boxStyle;

    public static void ShowWindow(string title, string content)
    {
        var win = GetWindow<BillGameCoreStepsWindow>(true, title, true);
        win._steps = content;
        win.minSize = new Vector2(500, 400);
        win.Show();
    }

    private void OnGUI()
    {
        InitStyles();

        // Background đậm chất Editor hiện đại
        EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

        // Tiêu đề chính
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🚀 NEXT STEPS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        // Hiển thị nội dung với RichText
        // Thay vì SelectableLabel (khó căn chỉnh), ta dùng TextArea giả lập hoặc Label có RichText
        EditorGUILayout.BeginVertical(_boxStyle);

        // Render nội dung chính
        EditorGUILayout.LabelField(_steps, _richLabelStyle);

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        // Footer với nút chức năng
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Copy Instructions to Clipboard", GUILayout.Height(30)))
        {
            EditorGUIUtility.systemCopyBuffer = StripUnityTags(_steps);
            Debug.Log("Copied to clipboard!");
        }
        EditorGUILayout.Space(10);

        EditorGUILayout.EndVertical();
    }

    private void InitStyles()
    {
        if (_richLabelStyle == null)
        {
            _richLabelStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true,
                fontSize = 13,
                alignment = TextAnchor.UpperLeft
            };
            // Chỉnh màu text mặc định cho dễ nhìn trên nền tối/sáng 
            _richLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }

        if (_boxStyle == null)
        {
            _boxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(15, 15, 15, 15)
            };
        }
    }

    // Hàm phụ để xóa tag màu khi copy ra ngoài (để code sạch)
    private string StripUnityTags(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
    }
}