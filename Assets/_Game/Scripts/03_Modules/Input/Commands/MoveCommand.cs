using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands

{
   public sealed class MoveCommand : IMoveCommand

    {
        public CommandType Type => CommandType.Move;
        public MoveCommand(float dirX, float dirY)

        {
            DirX = dirX;
            DirY = dirY;
            IsMoving = dirX != 0f || dirY != 0f;
        }

        public float DirX { get; }

        public float DirY { get; }

        public bool IsMoving { get; }
    }
}