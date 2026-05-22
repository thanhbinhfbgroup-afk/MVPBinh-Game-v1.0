using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Core.ValueObjects;
using UnityEngine;
using System;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter
    {
        private readonly PlayerApplication _application;
        private readonly PlayerView _view;     
        private readonly IInputCommandSource _inputCommandSource;
        private readonly BillEntityId _entityId;

        public PlayerPresenter(PlayerApplication application, PlayerView view, IInputCommandSource inputCommandSource, BillEntityId entityId)
        {
            _application = application;
            _view = view;
            _entityId = entityId;
            _inputCommandSource = inputCommandSource;
        }

        public void Tick()
        {
            IMoveCommand latestMoveCommand = null;

            while (_inputCommandSource.TryDequeue(out var command))
            {
                if (command.Type != CommandType.Move)
                {
                    continue;
                }

                if (command.ControlledEntityId != _entityId)
                {
                    continue;
                }

                if (command is not IMoveCommand moveCommand)
                {
                    continue;
                }

                latestMoveCommand = moveCommand;
            }

            if (latestMoveCommand == null)
            {
                return;
            }

            _application.ComputeMoveVelocity(
                latestMoveCommand.DirX,
                latestMoveCommand.DirY,
                out var velocityX,
                out var velocityY);

            _view.SetMoveVelocity(new Vector2(velocityX, velocityY));
        }
        public Action OnDiedCallback { get; set; }
        public void Stop()
        {
            _view.SetMoveVelocity(Vector2.zero);
        }
    }
}