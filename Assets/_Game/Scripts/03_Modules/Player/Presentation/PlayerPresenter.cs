using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter : IDisposable
    {
        private readonly EntityId _id;
        private readonly PlayerApplication _application;
        private readonly PlayerView _view;
        private readonly IInputCommandSource _inputCommandSource;

        public PlayerPresenter(
            EntityId id,
            PlayerApplication application,
            PlayerView view,
            IInputCommandSource inputCommandSource)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("Player id must be valid.", nameof(id));
            }

            _id = id;
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputCommandSource = inputCommandSource ?? throw new ArgumentNullException(nameof(inputCommandSource));
        }

        public void Tick(float deltaTime)
        {
            while (_inputCommandSource.TryDequeue(out var command))
            {
                if (command.ControlledEntityId != _id)
                {
                    continue;
                }

                if (command is IMoveCommand moveCommand)
                {
                    HandleMove(moveCommand);
                }
            }

            _view.SetVelocity(_application.VelocityX, _application.VelocityY);
        }

        public void Dispose()
        {
        }

        private void HandleMove(IMoveCommand command)
        {
            _application.TickMove(command.DirX, command.DirY);
        }
    }
}
