using System;

namespace BillGameCore.Modules.Player.Application
{
    public sealed class PlayerApplication
    {
        private readonly float _moveSpeed;

        public PlayerApplication(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        public void ComputeMoveVelocity(
            float inputX,
            float inputY,
            out float velocityX,
            out float velocityY)
        {
            var magnitudeSquared = (inputX * inputX) + (inputY * inputY);

            if (magnitudeSquared > 1f)
            {
                var magnitude = MathF.Sqrt(magnitudeSquared);
                inputX /= magnitude;
                inputY /= magnitude;
            }

            velocityX = inputX * _moveSpeed;
            velocityY = inputY * _moveSpeed;
        }
    }
}