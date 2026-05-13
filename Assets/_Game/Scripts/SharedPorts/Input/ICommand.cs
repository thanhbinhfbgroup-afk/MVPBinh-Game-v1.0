using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Input
{
    // Khai báo trong SharedPorts để consumer chỉ cần ref SharedPorts.
    // Các class cụ thể (MoveCommand, AttackCommand ...) nằm trong
    // Modules.Input.Commands và implement interface này.
    public interface ICommand
    {
        EntityId    SourceId  { get; }   // ai tạo ra command này (ADR-02 identity)
        CommandType Type      { get; }
        float       Timestamp { get; }   // Time.time lúc tạo
    }
}