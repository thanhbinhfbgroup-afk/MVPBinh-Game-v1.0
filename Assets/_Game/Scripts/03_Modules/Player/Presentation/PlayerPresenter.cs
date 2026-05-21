using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter
    {
        private readonly PlayerApplication _application;
        private readonly PlayerView _view;     
        private readonly IInputCommandSource _inputCommandSource;

        public PlayerPresenter(PlayerApplication application, PlayerView view, IInputCommandSource inputCommandSource)
        {
            _application = application;
            _view = view;
            _inputCommandSource = inputCommandSource;
        }

        public void Tick()
        {
            var moveCommand = _inputCommandSource.ReadMoveCommand();

            _application.ComputeMoveVelocity(
                moveCommand.DirX,
                moveCommand.DirY,
                out var velocityX,
                out var velocityY);

            _view.SetMoveVelocity(new Vector2(velocityX, velocityY));
        }
        public void Stop()
        {
            _view.SetMoveVelocity(Vector2.zero);
        }
    }
}