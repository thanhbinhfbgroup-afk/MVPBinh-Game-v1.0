using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    public interface ICommand
    {
        CommandType Type { get; }
        EntityId ControlledEntityId { get; }
    }
}