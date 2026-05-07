// [MODULE: Input]
// [TYPE: Interface]
// [SCOPE: ProjectLifetimeScope]
// [REGISTER_IN: ProjectLifetimeScope.cs → Global Services block]
namespace BillGameCore.Interfaces
{
    public interface IInputService : IBaseService
    {
        void EnableInput();
        void DisableInput();
        bool IsEnabled { get; }
    }
}