using System;
using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _commands;

        public CommandBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Command buffer capacity must be greater than 0.");
            }

            _commands = new Queue<ICommand>(capacity);
        }

        public bool HasCommands => _commands.Count > 0;

        public void Enqueue(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

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