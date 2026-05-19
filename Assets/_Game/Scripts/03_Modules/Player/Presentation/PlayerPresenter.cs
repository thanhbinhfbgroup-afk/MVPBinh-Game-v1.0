using BillGameCore.Modules.Player.Application;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter
    {
        private readonly PlayerApplication _application;
        private readonly PlayerView _view;

        public PlayerPresenter(PlayerApplication application, PlayerView view)
        {
            _application = application;
            _view = view;
        }

        public void TickMove(Vector2 moveInput)
        {
            _application.ComputeMoveVelocity(
                moveInput.x,
                moveInput.y,
                out var velocityX,
                out var velocityY);

            _view.SetMoveVelocity(new Vector2(velocityX, velocityY));
        }
    }
}