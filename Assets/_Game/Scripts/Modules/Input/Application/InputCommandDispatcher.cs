using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Registered in SceneLifetimeScope as IInputCommandSource.
    // Thin adapter: CommandBuffer → IInputCommandSource.
    // Consumer Presenters inject IInputCommandSource — never know InputReader exists.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer) { _buffer = buffer; }

        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
        public bool HasCommands                       => _buffer.HasCommands;
    }
}