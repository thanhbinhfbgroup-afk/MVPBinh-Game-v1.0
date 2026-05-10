// [MODULE: Save]
// [TYPE: Interface]
// [SCOPE: ProjectLifetimeScope]
// [REGISTER_IN: Scripts/Composition/ProjectLifetimeScope.cs → Global Services block]
using Cysharp.Threading.Tasks;

namespace BillGameCore.Interfaces
{
    public interface ISaveService : IBaseService
    {
        UniTask SaveAsync<T>(string key, T data);
        UniTask<T> LoadAsync<T>(string key);
        bool HasSave(string key);
        void DeleteSave(string key);
    }
}