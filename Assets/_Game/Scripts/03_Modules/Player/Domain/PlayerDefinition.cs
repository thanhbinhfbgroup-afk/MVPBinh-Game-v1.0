using System;

namespace BillGameCore.Modules.Player.Domain
{
    public sealed class PlayerDefinition
    {
        public PlayerDefinition(float moveSpeed)
        {
            if (moveSpeed <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(moveSpeed), "Move speed must be greater than 0.");
            }

            MoveSpeed = moveSpeed;
        }

        public float MoveSpeed { get; }
    }
}