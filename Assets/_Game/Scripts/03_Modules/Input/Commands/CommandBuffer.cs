using System;
using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // FIFO command buffer cho input runtime.
    // R16: Chỉ InputReader (Infrastructure) được phép gọi Enqueue().
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

        public bool HasCommands => _queue.Count > 0;
        public int Count => _queue.Count;
        public int Capacity => _capacity;

        // R16: CHỈ được gọi từ InputReader.
        public void Enqueue(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_queue.Count >= _capacity)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(command);
        }

        public bool TryDequeue(out ICommand command)
        {
            if (_queue.Count == 0)
            {
                command = null;
                return false;
            }

            command = _queue.Dequeue();
            return true;
        }

        // Gọi bởi InputReader.SwitchContext() để xả command cũ.
        public void Clear()
        {
            _queue.Clear();
        }
    }
}