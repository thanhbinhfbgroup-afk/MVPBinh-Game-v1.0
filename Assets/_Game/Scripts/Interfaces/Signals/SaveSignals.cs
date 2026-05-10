// [MODULE: Save]
// [TYPE: Signal]
// [SCOPE: ProjectLifetimeScope]
namespace BillGameCore.Interfaces.Signals
{
    public struct GameSavedSignal
    {
        public readonly string SaveName;
        public GameSavedSignal(string saveName) => SaveName = saveName;
    }

    public struct GameLoadedSignal
    {
        public readonly string SaveName;
        public GameLoadedSignal(string saveName) => SaveName = saveName;
    }
}