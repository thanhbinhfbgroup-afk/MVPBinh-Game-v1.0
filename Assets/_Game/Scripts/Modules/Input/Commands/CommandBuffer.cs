using System.Collections.Generic;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Thread-safe FIFO.
    // R16: Only InputReader (Infrastructure) is allowed to call Enqueue().
    public sealed class CommandBuffer
    {
        private readonly Queue<ICommand> _queue   = new Queue<ICommand>();
        private readonly object          _lock    = new object();
        private readonly int             _maxSize;

        public CommandBuffer(int maxSize = 32) { _maxSize = maxSize; }

        // R16: called ONLY from InputReader.
        public void Enqueue(ICommand command)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize) _queue.Dequeue(); // drop oldest on overflow
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

        // Called by InputReader.SwitchContext() to flush stale commands.
        public void Clear() { lock (_lock) _queue.Clear(); }
    }
}