using System;
using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue;
        private readonly int _capacity;

        public CommandBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Command buffer capacity must be greater than zero.");
            }

            _capacity = capacity;
            _queue = new Queue<ICommand>(capacity);
        }

        internal bool HasCommands => _queue.Count > 0;

        internal void Enqueue(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_queue.Count >= _capacity)
            {
                throw new InvalidOperationException($"CommandBuffer overflow. Capacity: {_capacity}.");
            }

            _queue.Enqueue(command);
        }

        internal bool TryDequeue(out ICommand command)
        {
            if (_queue.Count > 0)
            {
                command = _queue.Dequeue();
                return true;
            }

            command = default;
            return false;
        }

        internal void Clear()
        {
            _queue.Clear();
        }
    }
}