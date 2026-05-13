using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // FIFO thread-safe.
    // R16: Chỉ InputReader (Infrastructure) được phép gọi Enqueue().
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue   = new Queue<ICommand>();
        private readonly object          _lock    = new object();
        private readonly int             _maxSize;

        public CommandBuffer(int maxSize = 32) { _maxSize = maxSize; }

        // R16: CHỈ được gọi từ InputReader.
        public void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize) _queue.Dequeue(); // bỏ cũ nhất khi tràn
                _queue.Enqueue(command);
            }
        }

        public bool TryDequeue(out ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count == 0) { command = null; return false; }
                command = _queue.Dequeue();
                return true;
            }
        }

        public bool HasCommands { get { lock (_lock) return _queue.Count > 0; } }

        // Gọi bởi InputReader.SwitchContext() để xả command cũ.
        public void Clear() { lock (_lock) _queue.Clear(); }
    }
}