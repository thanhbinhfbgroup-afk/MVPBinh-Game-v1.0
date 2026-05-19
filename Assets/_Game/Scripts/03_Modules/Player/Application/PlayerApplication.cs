using System;
using BillGameCore.Modules.Player.Domain;

namespace BillGameCore.Modules.Player.Application
{
    public sealed class PlayerApplication
    {
        private readonly PlayerDefinition _definition;
        private readonly PlayerState _state;

        public PlayerApplication(PlayerDefinition definition, PlayerState state)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public float VelocityX => _state.VelocityX;

        public float VelocityY => _state.VelocityY;

        public bool IsMoving => _state.IsMoving;

        public void TickMove(float dirX, float dirY)
        {
            var lengthSquared = dirX * dirX + dirY * dirY;

            if (lengthSquared <= 0f)
            {
                _state.SetMovement(0f, 0f, 0f, 0f);
                return;
            }

            var length = MathF.Sqrt(lengthSquared);
            var normalizedX = dirX / length;
            var normalizedY = dirY / length;

            var velocityX = normalizedX * _definition.MoveSpeed;
            var velocityY = normalizedY * _definition.MoveSpeed;

            _state.SetMovement(normalizedX, normalizedY, velocityX, velocityY);
        }
    }
}