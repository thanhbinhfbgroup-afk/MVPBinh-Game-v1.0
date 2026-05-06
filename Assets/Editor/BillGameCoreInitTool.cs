using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public static class BillGameCoreInitTool
{
    private const string ROOT_PATH = "Assets/_Game";

    [MenuItem("BillGameCore/Initialize Project (Final Standard)")]
    public static void InitializeProject()
    {
        CreateFolders();
        CreateCoreSystem();
        CreateBaseDataAndInterfaces();

        AssetDatabase.Refresh();
        Debug.Log("<color=green><b>[BillGameCore]</b> Khởi tạo thành công. Toàn bộ hệ thống Scopes, EventBus và BaseData đã sẵn sàng!</color>");
    }

    private static void CreateFolders()
    {
        string[] folders = {
            "Art", "Data/Items", "Data/Settings", "Scripts/Core",
            "Scripts/Interfaces", "Scripts/Modules", "Scripts/Editor", "Scripts/Scenes"
        };
        foreach (var folder in folders)
        {
            string path = Path.Combine(ROOT_PATH, folder);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }

    private static void CreateCoreSystem()
    {
        // 1. EventBus - Hệ thống truyền tin
        string eventBus = @"using System;
using System.Collections.Generic;

namespace BillGameCore.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Action<object>>> _subscribers = new();
        public static void Subscribe<T>(Action<T> handler) {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) _subscribers[type] = new List<Action<object>>();
            _subscribers[type].Add(obj => handler((T)obj));
        }
        public static void Publish<T>(T signal) {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type)) foreach (var handler in _subscribers[type]) handler(signal);
        }
    }
}";
        GenerateFile("Scripts/Core/EventBus.cs", eventBus);

        // 2. ProjectLifetimeScope - Chứa Global Services
        string projectScope = @"using VContainer;
using VContainer.Unity;

namespace BillGameCore.Core
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // [REGISTER GLOBAL SERVICES HERE] - Inventory, Save, Audio, Input...
            // builder.Register<IInventoryService, InventoryManager>(Lifetime.Singleton);
        }
    }
}";
        GenerateFile("Scripts/Core/ProjectLifetimeScope.cs", projectScope);

        // 3. SceneLifetimeScope - Base cho các Scene
        string sceneScope = @"using VContainer;
using VContainer.Unity;

namespace BillGameCore.Core
{
    public class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // [REGISTER SCENE SERVICES HERE] - EnemyManager, Spawner, Harvesting...
        }
    }
}";
        GenerateFile("Scripts/Core/SceneLifetimeScope.cs", sceneScope);
    }

    private static void CreateBaseDataAndInterfaces()
    {
        // Base Item Data cho mọi AI dùng chung
        string baseItem = @"using UnityEngine;
namespace BillGameCore.Data
{
    public abstract class BaseItemData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        [TextArea] public string Description;
    }
}";
        GenerateFile("Data/BaseItemData.cs", baseItem);

        // Các Interface nền tảng
        GenerateFile("Scripts/Interfaces/IService.cs", "namespace BillGameCore.Interfaces { public interface IService { } }");
        GenerateFile("Scripts/Interfaces/IInteractable.cs", "namespace BillGameCore.Interfaces { public interface IInteractable { void OnInteract(); string GetInteractText(); } }");
    }

    private static void GenerateFile(string relativePath, string content)
    {
        string fullPath = Path.Combine(ROOT_PATH, relativePath);
        if (!File.Exists(fullPath)) File.WriteAllText(fullPath, content, Encoding.UTF8);
    }
}