using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _commands = new();
        public bool HasCommands => _commands.Count > 0;

        public void Enqueue(ICommand command)
        {
            _commands.Enqueue(command);
        }

        public bool TryDequeue(out ICommand command)
        {
            if (_commands.Count == 0)
            {
                command = null;
                return false;
            }

            command = _commands.Dequeue();
            return true;
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}