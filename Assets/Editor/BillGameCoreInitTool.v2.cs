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
        ProjectRoot + "/GlobalScene",

        ScriptsRoot + "/01_Core/Combat",
        ScriptsRoot + "/01_Core/Interaction",
        ScriptsRoot + "/01_Core/Inventory",
        ScriptsRoot + "/01_Core/Rewards",
        ScriptsRoot + "/01_Core/Save",
        ScriptsRoot + "/01_Core/ValueObjects",

        ScriptsRoot + "/02_SharedPorts/Combat",
        ScriptsRoot + "/02_SharedPorts/Economy",
        ScriptsRoot + "/02_SharedPorts/Input",
        ScriptsRoot + "/02_SharedPorts/Inventory",
        ScriptsRoot + "/02_SharedPorts/Messages",
        ScriptsRoot + "/02_SharedPorts/Player",

        ScriptsRoot + "/03_Modules/Input/Application",
        ScriptsRoot + "/03_Modules/Input/Commands",
        ScriptsRoot + "/03_Modules/Input/Context",
        ScriptsRoot + "/03_Modules/Input/Infrastructure",
        ScriptsRoot + "/03_Modules/Player/Application",
        ScriptsRoot + "/03_Modules/Player/Domain",
        ScriptsRoot + "/03_Modules/Player/Infrastructure/Config",
        ScriptsRoot + "/03_Modules/Player/Presentation",
        ScriptsRoot + "/03_Modules/Inventory/Application",
        ScriptsRoot + "/03_Modules/Inventory/Domain",
        ScriptsRoot + "/03_Modules/Inventory/Infrastructure/Config",
        ScriptsRoot + "/03_Modules/Inventory/Infrastructure/Persistence",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Application",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Domain",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Infrastructure/Config",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Presentation",
        ScriptsRoot + "/03_Modules/Enemy/Application",
        ScriptsRoot + "/03_Modules/Enemy/Domain",
        ScriptsRoot + "/03_Modules/Enemy/Infrastructure/Config",
        ScriptsRoot + "/03_Modules/Enemy/Presentation",

        ScriptsRoot + "/04_Composition",
        ScriptsRoot + "/05_Scenes",
    };

    private static readonly string[] RequiredFiles =
    {
        ContextPath,

        ScriptsRoot + "/01_Core/BillGameCore.Core.asmdef",
        ScriptsRoot + "/01_Core/ValueObjects/EntityId.cs",
        ScriptsRoot + "/01_Core/Combat/DamageInfo.cs",
        ScriptsRoot + "/01_Core/Combat/DamageResult.cs",
        ScriptsRoot + "/01_Core/Combat/IDamageReceiver.cs",
        ScriptsRoot + "/01_Core/Interaction/IInteractable.cs",
        ScriptsRoot + "/01_Core/Inventory/ItemStack.cs",
        ScriptsRoot + "/01_Core/Rewards/RewardBundle.cs",
        ScriptsRoot + "/01_Core/Save/ISaveSnapshotProvider.cs",
        ScriptsRoot + "/01_Core/Save/ISaveSnapshotConsumer.cs",

        ScriptsRoot + "/02_SharedPorts/BillGameCore.SharedPorts.asmdef",
        ScriptsRoot + "/02_SharedPorts/Input/ICommand.cs",
        ScriptsRoot + "/02_SharedPorts/Input/CommandType.cs",
        ScriptsRoot + "/02_SharedPorts/Input/IMoveCommand.cs",
        ScriptsRoot + "/02_SharedPorts/Input/IAttackCommand.cs",
        ScriptsRoot + "/02_SharedPorts/Input/IInteractCommand.cs",
        ScriptsRoot + "/02_SharedPorts/Input/IInputCommandSource.cs",
        ScriptsRoot + "/02_SharedPorts/Input/InputContext.cs",
        ScriptsRoot + "/02_SharedPorts/Inventory/IInventoryReadService.cs",
        ScriptsRoot + "/02_SharedPorts/Inventory/IInventoryWriteService.cs",
        ScriptsRoot + "/02_SharedPorts/Economy/IWalletService.cs",
        ScriptsRoot + "/02_SharedPorts/Economy/IRewardGrantService.cs",
        ScriptsRoot + "/02_SharedPorts/Player/IPlayerReadService.cs",
        ScriptsRoot + "/02_SharedPorts/Combat/ICombatService.cs",
        ScriptsRoot + "/02_SharedPorts/Messages/EnemyDiedMessage.cs",
        ScriptsRoot + "/02_SharedPorts/Messages/ItemPickedUpMessage.cs",

        ScriptsRoot + "/03_Modules/Input/BillGameCore.Modules.Input.asmdef",
        ScriptsRoot + "/03_Modules/Input/Application/InputCommandDispatcher.cs",
        ScriptsRoot + "/03_Modules/Input/Commands/CommandBuffer.cs",
        ScriptsRoot + "/03_Modules/Input/Commands/MoveCommand.cs",
        ScriptsRoot + "/03_Modules/Input/Commands/AttackCommand.cs",
        ScriptsRoot + "/03_Modules/Input/Commands/InteractCommand.cs",
        ScriptsRoot + "/03_Modules/Input/Commands/SwitchContextCommand.cs",
        ScriptsRoot + "/03_Modules/Input/Context/PlayerInputContext.cs",
        ScriptsRoot + "/03_Modules/Input/Context/VehicleInputContext.cs",
        ScriptsRoot + "/03_Modules/Input/Context/UIInputContext.cs",
        ScriptsRoot + "/03_Modules/Input/Infrastructure/InputActionGateway.cs",
        ScriptsRoot + "/03_Modules/Input/Infrastructure/InputReader.cs",

        ScriptsRoot + "/03_Modules/Player/BillGameCore.Modules.Player.asmdef",
        ScriptsRoot + "/03_Modules/Player/Domain/PlayerDefinition.cs",
        ScriptsRoot + "/03_Modules/Player/Domain/PlayerState.cs",
        ScriptsRoot + "/03_Modules/Player/Application/PlayerApplication.cs",
        ScriptsRoot + "/03_Modules/Player/Infrastructure/Config/PlayerConfig.cs",
        ScriptsRoot + "/03_Modules/Player/Presentation/PlayerView.cs",
        ScriptsRoot + "/03_Modules/Player/Presentation/PlayerPresenter.cs",
        ScriptsRoot + "/03_Modules/Player/Presentation/PlayerSpawner.cs",
        ScriptsRoot + "/03_Modules/Player/Presentation/PlayerRuntime.cs",

        ScriptsRoot + "/03_Modules/Inventory/BillGameCore.Modules.Inventory.asmdef",
        ScriptsRoot + "/03_Modules/Inventory/Domain/InventoryState.cs",
        ScriptsRoot + "/03_Modules/Inventory/Application/InventoryService.cs",
        ScriptsRoot + "/03_Modules/Inventory/Infrastructure/Config/InventorySettings.cs",
        ScriptsRoot + "/03_Modules/Inventory/Infrastructure/Persistence/InventorySaveData.cs",

        ScriptsRoot + "/03_Modules/InteractionGroup/BillGameCore.Modules.InteractionGroup.asmdef",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Domain/ChestDefinition.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Domain/ChestState.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Application/ChestApplication.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Infrastructure/Config/ChestConfig.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Presentation/ChestView.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Presentation/ChestPresenter.cs",
        ScriptsRoot + "/03_Modules/InteractionGroup/Chest/Presentation/ChestBinder.cs",

        ScriptsRoot + "/03_Modules/Enemy/BillGameCore.Modules.Enemy.asmdef",
        ScriptsRoot + "/03_Modules/Enemy/Domain/EnemyDefinition.cs",
        ScriptsRoot + "/03_Modules/Enemy/Domain/EnemyState.cs",
        ScriptsRoot + "/03_Modules/Enemy/Application/EnemyApplication.cs",
        ScriptsRoot + "/03_Modules/Enemy/Infrastructure/Config/EnemyConfig.cs",
        ScriptsRoot + "/03_Modules/Enemy/Presentation/EnemyView.cs",
        ScriptsRoot + "/03_Modules/Enemy/Presentation/EnemyPresenter.cs",
        ScriptsRoot + "/03_Modules/Enemy/Presentation/EnemySpawner.cs",
        ScriptsRoot + "/03_Modules/Enemy/Presentation/EnemyRuntime.cs",

        ScriptsRoot + "/04_Composition/BillGameCore.Composition.asmdef",
        ScriptsRoot + "/04_Composition/ProjectLifetimeScope.cs",

        ScriptsRoot + "/05_Scenes/BillGameCore.Scenes.asmdef",
        ScriptsRoot + "/05_Scenes/BootstrapSceneLifetimeScope.cs",
        ScriptsRoot + "/05_Scenes/SceneBootstrapper.cs",
        ScriptsRoot + "/05_Scenes/SceneController.cs"
    };

    private static readonly BaselineAsmdef[] BaselineAsmdefs =
    {
        new BaselineAsmdef(ScriptsRoot + "/01_Core", "BillGameCore.Core", Array.Empty<string>(), false, true),
        new BaselineAsmdef(ScriptsRoot + "/02_SharedPorts", "BillGameCore.SharedPorts", new[] { "BillGameCore.Core" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/03_Modules/Input", "BillGameCore.Modules.Input", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer", "Unity.InputSystem" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/03_Modules/Player", "BillGameCore.Modules.Player", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/03_Modules/Inventory", "BillGameCore.Modules.Inventory", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/03_Modules/InteractionGroup", "BillGameCore.Modules.InteractionGroup", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/03_Modules/Enemy", "BillGameCore.Modules.Enemy", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "VContainer" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/04_Composition", "BillGameCore.Composition", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "BillGameCore.Modules.Inventory", "VContainer", "VContainer.Unity" }, false, false),
        new BaselineAsmdef(ScriptsRoot + "/05_Scenes", "BillGameCore.Scenes", new[] { "BillGameCore.Core", "BillGameCore.SharedPorts", "BillGameCore.Composition", "BillGameCore.Modules.Input", "BillGameCore.Modules.Player", "BillGameCore.Modules.Enemy", "VContainer" }, false, false),
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
