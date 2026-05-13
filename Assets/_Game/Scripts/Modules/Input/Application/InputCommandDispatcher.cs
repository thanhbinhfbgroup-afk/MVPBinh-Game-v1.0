using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Đăng ký trong SceneLifetimeScope với type IInputCommandSource.
    // Adapter mỏng: CommandBuffer → IInputCommandSource.
    // Consumer Presenter inject IInputCommandSource — không biết InputReader tồn tại.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer) { _buffer = buffer; }

        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
        public bool HasCommands                       => _buffer.HasCommands;
    }
}