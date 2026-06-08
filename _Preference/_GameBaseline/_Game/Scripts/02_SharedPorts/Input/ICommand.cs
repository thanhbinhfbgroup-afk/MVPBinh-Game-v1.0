using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    public interface ICommand
    {
        CommandType Type { get; }
        BillEntityId ControlledEntityId { get; }
    }
}