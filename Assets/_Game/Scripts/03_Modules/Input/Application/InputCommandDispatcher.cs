using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Application
{
    // Đăng ký ở scene scope dưới dạng IInputCommandSource.
    // Adapter mỏng để lộ contract đọc command ra ngoài module Input.
    public sealed class InputCommandDispatcher : IInputCommandSource
    {
        private readonly CommandBuffer _buffer;

        public InputCommandDispatcher(CommandBuffer buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public bool HasCommands => _buffer.HasCommands;

        public bool TryDequeue(out ICommand command) => _buffer.TryDequeue(out command);
    }
}