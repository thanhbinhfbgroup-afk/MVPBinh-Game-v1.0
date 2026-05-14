// BillGameCore baseline tool.
//
// This editor tool intentionally does not generate gameplay modules.
// The approved architecture is documented in Assets/Editor/CONTEXT.v2.md.
// Use this file only to validate and repair the current baseline shell:
// folders, asmdefs, and a few high-risk serialized settings.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class BillGameCoreInitTool
{
    private const string ProjectRoot = "Assets/_Game";
    private const string ScriptsRoot = ProjectRoot + "/Scripts";
    private const string ContextPath = "Assets/Editor/CONTEXT.v2.md";
    private const string BootstrapScenePath = ScriptsRoot + "/Scenes/Bootstrap.unity";
    private const string InputActionsMetaPath = "Assets/Settings/InputSystem_Actions.inputactions.meta";

    private static readonly string[] RequiredDirectories =
    {
        ProjectRoot + "/Art",
        ProjectRoot + "/Data/Items",
        ProjectRoot + "/Data/Settings",

        ScriptsRoot + "/Core/Combat",
        ScriptsRoot + "/Core/Interaction",
        ScriptsRoot + "/Core/Inventory",
        ScriptsRoot + "/Core/Rewards",
        ScriptsRoot + "/Core/Save",
        ScriptsRoot + "/Core/ValueObjects",

        ScriptsRoot + "/SharedPorts/Combat",
        ScriptsRoot + "/SharedPorts/Economy",
        ScriptsRoot + "/SharedPorts/Input",
        ScriptsRoot + "/SharedPorts/Inventory",
        ScriptsRoot + "/SharedPorts/Messages",
        ScriptsRoot + "/SharedPorts/Player",

        ScriptsRoot + "/Modules/Input/Application",
        ScriptsRoot + "/Modules/Input/Commands",
        ScriptsRoot + "/Modules/Input/Context",
        ScriptsRoot + "/Modules/Input/Infrastructure",
        ScriptsRoot + "/Modules/Player/Application",
        ScriptsRoot + "/Modules/Player/Domain",
        ScriptsRoot + "/Modules/Player/Infrastructure/Config",
        ScriptsRoot + "/Modules/Player/Presentation",
        ScriptsRoot + "/Modules/Inventory/Application",
        ScriptsRoot + "/Modules/Inventory/Domain",
        ScriptsRoot + "/Modules/Inventory/Infrastructure/Config",
        ScriptsRoot + "/Modules/Inventory/Infrastructure/Persistence",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Application",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Domain",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Infrastructure/Config",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Presentation",
        ScriptsRoot + "/Modules/Enemy/Application",
        ScriptsRoot + "/Modules/Enemy/Domain",
        ScriptsRoot + "/Modules/Enemy/Infrastructure/Config",
        ScriptsRoot + "/Modules/Enemy/Presentation",

        ScriptsRoot + "/Composition",
        ScriptsRoot + "/Scenes",
        ScriptsRoot + "/Editor"
    };

    private static readonly string[] RequiredFiles =
    {
        ContextPath,

        ScriptsRoot + "/Core/BillGameCore.Core.asmdef",
        ScriptsRoot + "/Core/ValueObjects/EntityId.cs",
        ScriptsRoot + "/Core/Combat/DamageInfo.cs",
        ScriptsRoot + "/Core/Combat/DamageResult.cs",
        ScriptsRoot + "/Core/Combat/IDamageReceiver.cs",
        ScriptsRoot + "/Core/Interaction/IInteractable.cs",
        ScriptsRoot + "/Core/Inventory/ItemStack.cs",
        ScriptsRoot + "/Core/Rewards/RewardBundle.cs",
        ScriptsRoot + "/Core/Save/ISaveSnapshotProvider.cs",
        ScriptsRoot + "/Core/Save/ISaveSnapshotConsumer.cs",

        ScriptsRoot + "/SharedPorts/BillGameCore.SharedPorts.asmdef",
        ScriptsRoot + "/SharedPorts/Input/ICommand.cs",
        ScriptsRoot + "/SharedPorts/Input/CommandType.cs",
        ScriptsRoot + "/SharedPorts/Input/IMoveCommand.cs",
        ScriptsRoot + "/SharedPorts/Input/IAttackCommand.cs",
        ScriptsRoot + "/SharedPorts/Input/IInteractCommand.cs",
        ScriptsRoot + "/SharedPorts/Input/IInputCommandSource.cs",
        ScriptsRoot + "/SharedPorts/Input/InputContext.cs",
        ScriptsRoot + "/SharedPorts/Inventory/IInventoryReadService.cs",
        ScriptsRoot + "/SharedPorts/Inventory/IInventoryWriteService.cs",
        ScriptsRoot + "/SharedPorts/Economy/IWalletService.cs",
        ScriptsRoot + "/SharedPorts/Economy/IRewardGrantService.cs",
        ScriptsRoot + "/SharedPorts/Player/IPlayerReadService.cs",
        ScriptsRoot + "/SharedPorts/Combat/ICombatService.cs",
        ScriptsRoot + "/SharedPorts/Messages/EnemyDiedMessage.cs",
        ScriptsRoot + "/SharedPorts/Messages/ItemPickedUpMessage.cs",

        ScriptsRoot + "/Modules/Input/BillGameCore.Modules.Input.asmdef",
        ScriptsRoot + "/Modules/Input/Application/InputCommandDispatcher.cs",
        ScriptsRoot + "/Modules/Input/Commands/CommandBuffer.cs",
        ScriptsRoot + "/Modules/Input/Commands/MoveCommand.cs",
        ScriptsRoot + "/Modules/Input/Commands/AttackCommand.cs",
        ScriptsRoot + "/Modules/Input/Commands/InteractCommand.cs",
        ScriptsRoot + "/Modules/Input/Commands/SwitchContextCommand.cs",
        ScriptsRoot + "/Modules/Input/Context/PlayerInputContext.cs",
        ScriptsRoot + "/Modules/Input/Context/VehicleInputContext.cs",
        ScriptsRoot + "/Modules/Input/Context/UIInputContext.cs",
        ScriptsRoot + "/Modules/Input/Infrastructure/InputActionGateway.cs",
        ScriptsRoot + "/Modules/Input/Infrastructure/InputReader.cs",

        ScriptsRoot + "/Modules/Player/BillGameCore.Modules.Player.asmdef",
        ScriptsRoot + "/Modules/Player/Domain/PlayerDefinition.cs",
        ScriptsRoot + "/Modules/Player/Domain/PlayerState.cs",
        ScriptsRoot + "/Modules/Player/Application/PlayerApplication.cs",
        ScriptsRoot + "/Modules/Player/Infrastructure/Config/PlayerConfig.cs",
        ScriptsRoot + "/Modules/Player/Presentation/PlayerView.cs",
        ScriptsRoot + "/Modules/Player/Presentation/PlayerPresenter.cs",
        ScriptsRoot + "/Modules/Player/Presentation/PlayerSpawner.cs",
        ScriptsRoot + "/Modules/Player/Presentation/PlayerRuntime.cs",

        ScriptsRoot + "/Modules/Inventory/BillGameCore.Modules.Inventory.asmdef",
        ScriptsRoot + "/Modules/Inventory/Domain/InventoryState.cs",
        ScriptsRoot + "/Modules/Inventory/Application/InventoryService.cs",
        ScriptsRoot + "/Modules/Inventory/Infrastructure/Config/InventorySettings.cs",
        ScriptsRoot + "/Modules/Inventory/Infrastructure/Persistence/InventorySaveData.cs",

        ScriptsRoot + "/Modules/InteractionGroup/BillGameCore.Modules.InteractionGroup.asmdef",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Domain/ChestDefinition.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Domain/ChestState.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Application/ChestApplication.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Infrastructure/Config/ChestConfig.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Presentation/ChestView.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Presentation/ChestPresenter.cs",
        ScriptsRoot + "/Modules/InteractionGroup/Chest/Presentation/ChestBinder.cs",

        ScriptsRoot + "/Modules/Enemy/BillGameCore.Modules.Enemy.asmdef",
        ScriptsRoot + "/Modules/Enemy/Domain/EnemyDefinition.cs",
        ScriptsRoot + "/Modules/Enemy/Domain/EnemyState.cs",
        ScriptsRoot + "/Modules/Enemy/Application/EnemyApplication.cs",
        ScriptsRoot + "/Modules/Enemy/Infrastructure/Config/EnemyConfig.cs",
        ScriptsRoot + "/Modules/Enemy/Presentation/EnemyView.cs",
        ScriptsRoot + "/Modules/Enemy/Presentation/EnemyPresenter.cs",
        ScriptsRoot + "/Modules/Enemy/Presentation/EnemySpawner.cs",
        ScriptsRoot + "/Modules/Enemy/Presentation/EnemyRuntime.cs",

        ScriptsRoot + "/Composition/BillGameCore.Composition.asmdef",
        ScriptsRoot + "/Composition/ProjectLifetimeScope.cs",
        ScriptsRoot + "/Composition/SceneLifetimeScope.cs",
        ScriptsRoot + "/Composition/GameBootstrapper.cs",

        ScriptsRoot + "/Scenes/BillGameCore.Scenes.asmdef",
        ScriptsRoot + "/Scenes/BootstrapSceneLifetimeScope.cs",
        ScriptsRoot + "/Scenes/SceneBootstrapper.cs",
        ScriptsRoot + "/Scenes/SceneController.cs"
    };

    private static readonly BaselineAsmdef[] BaselineAsmdefs =
    {
        new BaselineAsmdef(ScriptsRoot + "/Core", "BillGameCore.Core", Array.Empty<string>(), false, true),
        new BaselineAsmdef(ScriptsRoot + "/SharedPorts", "BillGameCore.SharedPorts", new[] { "BillGameCore.Core" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Modules/Input", "BillGameCore.Modules.Input", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer", "Unity.InputSystem" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Modules/Player", "BillGameCore.Modules.Player", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Modules/Inventory", "BillGameCore.Modules.Inventory", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Modules/InteractionGroup", "BillGameCore.Modules.InteractionGroup", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Modules/Enemy", "BillGameCore.Modules.Enemy", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Composition", "BillGameCore.Composition", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "BillGameCore.Modules.Inventory", "VContainer", "VContainer.Unity" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Scenes", "BillGameCore.Scenes", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "BillGameCore.Composition", "BillGameCore.Modules.Input", "BillGameCore.Modules.Player", "BillGameCore.Modules.Enemy", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/Editor", "BillGameCore.Editor", new[] { "BillGameCore.Core" }, true, false)
    };

    [MenuItem("BillGameCore/Baseline/Validate Current Baseline")]
    public static void ValidateCurrentBaseline()
    {
        var report = BuildReport();
        LogReport(report);

        string title = report.HasErrors ? "BillGameCore baseline has issues" : "BillGameCore baseline is valid";
        string body = report.HasErrors
            ? $"Found {report.Errors.Count} error(s) and {report.Warnings.Count} warning(s). See Console for details."
            : report.Warnings.Count == 0
                ? "No baseline issue found."
                : $"No errors. Found {report.Warnings.Count} warning(s). See Console for details.";

        EditorUtility.DisplayDialog(title, body, "OK");
    }

    [MenuItem("BillGameCore/Baseline/Repair Missing Directories")]
    public static void RepairMissingDirectories()
    {
        CreateMissingDirectories("Directory repair");
    }

    [MenuItem("BillGameCore/Baseline/Create Empty Folder Tree")]
    public static void CreateEmptyFolderTree()
    {
        CreateMissingDirectories("Empty folder tree");
    }

    private static void CreateMissingDirectories(string operationName)
    {
        int created = 0;
        foreach (string directory in RequiredDirectories)
        {
            if (Directory.Exists(directory))
                continue;

            Directory.CreateDirectory(directory);
            created++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[BillGameCore] {operationName} completed. Created {created} missing director{(created == 1 ? "y" : "ies")}.");
    }

    [MenuItem("BillGameCore/Baseline/Repair Missing Asmdefs")]
    public static void RepairMissingAsmdefs()
    {
        int created = 0;
        foreach (var asmdef in BaselineAsmdefs)
        {
            string path = asmdef.Path;
            if (File.Exists(path))
                continue;

            Directory.CreateDirectory(asmdef.Folder);
            File.WriteAllText(path, BuildAsmdefJson(asmdef), Encoding.UTF8);
            created++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[BillGameCore] Asmdef repair completed. Created {created} missing asmdef file(s). Existing asmdefs were not overwritten.");
    }

    [MenuItem("BillGameCore/Baseline/Open Context")]
    public static void OpenContext()
    {
        var context = AssetDatabase.LoadAssetAtPath<TextAsset>(ContextPath);
        if (context == null)
        {
            EditorUtility.DisplayDialog("Context not found", $"Missing file: {ContextPath}", "OK");
            return;
        }

        Selection.activeObject = context;
        EditorGUIUtility.PingObject(context);
    }

    private static Report BuildReport()
    {
        var report = new Report();

        foreach (string directory in RequiredDirectories)
        {
            if (!Directory.Exists(directory))
                report.Errors.Add($"Missing directory: {directory}");
        }

        foreach (string file in RequiredFiles)
        {
            if (!File.Exists(file))
                report.Errors.Add($"Missing baseline file: {file}");
        }

        foreach (var asmdef in BaselineAsmdefs)
            ValidateAsmdef(asmdef, report);

        ValidateInputSystemSettings(report);
        ValidateBootstrapScene(report);
        ValidateCompositionBoundary(report);
        ValidateToolSurface(report);

        return report;
    }

    private static void ValidateAsmdef(BaselineAsmdef expected, Report report)
    {
        if (!File.Exists(expected.Path))
            return;

        string json = File.ReadAllText(expected.Path);
        string name = ExtractStringProperty(json, "name");
        if (name != expected.Name)
            report.Errors.Add($"Asmdef name mismatch in {expected.Path}. Expected '{expected.Name}', found '{name}'.");

        string[] actualRefs = ExtractReferences(json);
        string[] missingRefs = expected.References.Except(actualRefs).ToArray();
        string[] extraRefs = actualRefs.Except(expected.References).ToArray();

        if (missingRefs.Length > 0)
            report.Errors.Add($"{expected.Name} is missing reference(s): {string.Join(", ", missingRefs)}");
        if (extraRefs.Length > 0)
            report.Errors.Add($"{expected.Name} has unexpected reference(s): {string.Join(", ", extraRefs)}");

        bool noEngineReferences = ExtractBoolProperty(json, "noEngineReferences");
        if (noEngineReferences != expected.NoEngineReferences)
            report.Errors.Add($"{expected.Name} noEngineReferences must be {expected.NoEngineReferences.ToString().ToLowerInvariant()}.");

        string[] includePlatforms = ExtractStringArrayProperty(json, "includePlatforms");
        bool editorOnly = includePlatforms.Contains("Editor");
        if (editorOnly != expected.EditorOnly)
            report.Errors.Add($"{expected.Name} editor-only platform setting must be {expected.EditorOnly.ToString().ToLowerInvariant()}.");
    }

    private static void ValidateInputSystemSettings(Report report)
    {
        if (!File.Exists(InputActionsMetaPath))
        {
            report.Errors.Add($"Missing Input System meta file: {InputActionsMetaPath}");
            return;
        }

        string meta = File.ReadAllText(InputActionsMetaPath);
        if (!Regex.IsMatch(meta, @"generateWrapperCode:\s*0"))
            report.Errors.Add("InputSystem_Actions.inputactions must keep Generate C# wrapper disabled.");
    }

    private static void ValidateBootstrapScene(Report report)
    {
        if (!File.Exists(BootstrapScenePath))
            return;

        string scene = File.ReadAllText(BootstrapScenePath);
        if (scene.Contains("UnityEngine.InputSystem.PlayerInput"))
            report.Errors.Add("Bootstrap scene must not contain a PlayerInput component.");
        if (!scene.Contains("BillGameCore.Modules.Input.Infrastructure.InputReader"))
            report.Errors.Add("Bootstrap scene must contain InputReader.");
        if (!scene.Contains("_actions:"))
            report.Errors.Add("Bootstrap scene InputReader must reference InputSystem_Actions through _actions.");
    }

    private static void ValidateCompositionBoundary(Report report)
    {
        string compositionAsmdef = ScriptsRoot + "/Composition/BillGameCore.Composition.asmdef";
        if (!File.Exists(compositionAsmdef))
            return;

        string json = File.ReadAllText(compositionAsmdef);
        if (ExtractReferences(json).Contains("BillGameCore.Scenes"))
            report.Errors.Add("Composition asmdef must not reference BillGameCore.Scenes.");
    }

    private static void ValidateToolSurface(Report report)
    {
        string source = File.ReadAllText("Assets/Editor/BillGameCoreInitTool.v2.cs");
        string[] forbiddenMenuFragments =
        {
            "New Module/Entity",
            "New Module/Interaction",
            "New Module/System",
            "Initialize Project Structure"
        };

        foreach (string fragment in forbiddenMenuFragments)
        {
            string menuPattern = @"\[MenuItem\(""BillGameCore/[^""]*" + Regex.Escape(fragment) + @"[^""]*""";
            if (Regex.IsMatch(source, menuPattern))
                report.Warnings.Add($"Init tool still exposes old generator menu: {fragment}");
        }
    }

    private static string BuildAsmdefJson(BaselineAsmdef asmdef)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"    \"name\": \"{asmdef.Name}\",");
        sb.AppendLine("    \"rootNamespace\": \"\",");
        sb.AppendLine("    \"references\": [");

        for (int i = 0; i < asmdef.References.Length; i++)
            sb.AppendLine($"        \"{asmdef.References[i]}\"{(i < asmdef.References.Length - 1 ? "," : "")}");

        sb.AppendLine("    ],");
        sb.AppendLine(asmdef.EditorOnly ? "    \"includePlatforms\": [\"Editor\"]," : "    \"includePlatforms\": [],");
        sb.AppendLine("    \"excludePlatforms\": [],");
        sb.AppendLine("    \"allowUnsafeCode\": false,");
        sb.AppendLine("    \"overrideReferences\": false,");
        sb.AppendLine("    \"precompiledReferences\": [],");
        sb.AppendLine("    \"autoReferenced\": false,");
        sb.AppendLine("    \"defineConstraints\": [],");
        sb.AppendLine("    \"versionDefines\": [],");
        sb.AppendLine($"    \"noEngineReferences\": {asmdef.NoEngineReferences.ToString().ToLowerInvariant()}");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void LogReport(Report report)
    {
        if (!report.HasErrors && report.Warnings.Count == 0)
        {
            Debug.Log("[BillGameCore] Baseline validation passed.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("[BillGameCore] Baseline validation report");

        if (report.Errors.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Errors:");
            foreach (string error in report.Errors)
                sb.AppendLine("- " + error);
        }

        if (report.Warnings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Warnings:");
            foreach (string warning in report.Warnings)
                sb.AppendLine("- " + warning);
        }

        if (report.HasErrors)
            Debug.LogError(sb.ToString());
        else
            Debug.LogWarning(sb.ToString());
    }

    private static string ExtractStringProperty(string json, string property)
    {
        var match = Regex.Match(json, $"\"{Regex.Escape(property)}\"\\s*:\\s*\"(?<value>[^\"]*)\"");
        return match.Success ? match.Groups["value"].Value : string.Empty;
    }

    private static bool ExtractBoolProperty(string json, string property)
    {
        var match = Regex.Match(json, $"\"{Regex.Escape(property)}\"\\s*:\\s*(?<value>true|false)", RegexOptions.IgnoreCase);
        return match.Success && bool.TryParse(match.Groups["value"].Value, out bool value) && value;
    }

    private static string[] ExtractReferences(string json)
    {
        return ExtractStringArrayProperty(json, "references");
    }

    private static string[] ExtractStringArrayProperty(string json, string property)
    {
        var match = Regex.Match(json, $"\"{Regex.Escape(property)}\"\\s*:\\s*\\[(?<body>.*?)\\]", RegexOptions.Singleline);
        if (!match.Success)
            return Array.Empty<string>();

        return Regex.Matches(match.Groups["body"].Value, "\"(?<value>[^\"]+)\"")
            .Cast<Match>()
            .Select(m => m.Groups["value"].Value)
            .ToArray();
    }

    private readonly struct BaselineAsmdef
    {
        public BaselineAsmdef(string folder, string name, string[] references, bool editorOnly, bool noEngineReferences)
        {
            Folder = folder;
            Name = name;
            References = references;
            EditorOnly = editorOnly;
            NoEngineReferences = noEngineReferences;
        }

        public string Folder { get; }
        public string Name { get; }
        public string[] References { get; }
        public bool EditorOnly { get; }
        public bool NoEngineReferences { get; }
        public string Path => Folder + "/" + Name + ".asmdef";
    }

    private sealed class Report
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public bool HasErrors => Errors.Count > 0;
    }
}
