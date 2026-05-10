// [MODULE: Save]
// [TYPE: Manager]
// [SCOPE: ProjectLifetimeScope]
// [SIGNAL_PUBLISHES: GameSavedSignal, GameLoadedSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: Scripts/Composition/ProjectLifetimeScope.cs → Global Services block]
using MessagePipe;
using VContainer;
using UnityEngine;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace BillGameCore.Modules.Save
{
    public class SaveManager : ISaveService, IInitializable
    {
        private readonly IPublisher<GameSavedSignal> _savedPublisher;
        private readonly IPublisher<GameLoadedSignal> _loadedPublisher;

        [Inject]
        public SaveManager(
            IPublisher<GameSavedSignal> savedPublisher,
            IPublisher<GameLoadedSignal> loadedPublisher)
        {
            _savedPublisher = savedPublisher;
            _loadedPublisher = loadedPublisher;
        }

        public void Initialize()
        {
            UnityEngine.Debug.Log("[SaveManager] Initialized.");
        }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            // Logic lưu file (ví dụ dùng JsonUtility hoặc Newtonsoft Json)
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();

            await UniTask.Yield(); // Giả lập async
            _savedPublisher.Publish(new GameSavedSignal(key));
            Debug.Log($"[SaveManager] Data saved for key: {key}");
        }

        public async UniTask<T> LoadAsync<T>(string key)
        {
            if (!HasSave(key)) return default;

            string json = PlayerPrefs.GetString(key);
            T data = JsonUtility.FromJson<T>(json);

            await UniTask.Yield();
            _loadedPublisher.Publish(new GameLoadedSignal(key));
            return data;
        }

        public bool HasSave(string key) => PlayerPrefs.HasKey(key);

        public void DeleteSave(string key) => PlayerPrefs.DeleteKey(key);
    }
}