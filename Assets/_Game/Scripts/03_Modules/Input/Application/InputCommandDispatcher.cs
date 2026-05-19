using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _commandBuffer;

        public InputCommandDispatcher(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer ?? throw new ArgumentNullException(nameof(commandBuffer));
        }

        public bool HasCommands => _commandBuffer.HasCommands;

        public bool TryDequeue(out ICommand command)
        {
            return _commandBuffer.TryDequeue(out command);
        }
    }
}