using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    // Declared in SharedPorts so consumers need only ref SharedPorts.
    // Concrete classes (MoveCommand, AttackCommand ...) live in Modules.Input.Commands
    // and implement this interface.
    public interface ICommand
    {
        EntityId    SourceId  { get; }   // who produced this command (ADR-02 identity)
        CommandType Type      { get; }
        float       Timestamp { get; }   // Time.time when created
    }
}