namespace BillGameCore.Modules.Player.Domain
{
    public sealed class PlayerState
    {
        public float MoveDirX { get; private set; }

        public float MoveDirY { get; private set; }

        public float VelocityX { get; private set; }

        public float VelocityY { get; private set; }

        public bool IsMoving { get; private set; }

        public void SetMovement(float dirX, float dirY, float velocityX, float velocityY)
        {
            MoveDirX = dirX;
            MoveDirY = dirY;
            VelocityX = velocityX;
            VelocityY = velocityY;
            IsMoving = dirX != 0f || dirY != 0f;
        }
    }
}