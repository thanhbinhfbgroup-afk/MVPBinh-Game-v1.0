using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using System;

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
            // LỚP 1: Kiểm tra xem kho có hàng không (Tránh mất thời gian)
            if (!_inputCommandSource.HasCommands) return;

            // LỚP 2: Thực bốc hàng ra (Nếu bốc trượt vì lý do nào đó, dừng lại)
            if (!_inputCommandSource.TryDequeue(out var command)) return;

            // LỚP 3: Kiểm tra ý nghĩa (Ý đồ người chơi)
            // "Tôi chỉ muốn xử lý di chuyển ở đây, nếu nhãn là Attack thì tôi không nuốt"
            if (command.Type != CommandType.Move) return;

            // LỚP 4: Kiểm tra hình dáng contract (Shape dữ liệu)
            // "Nhãn là Move rồi, nhưng cái xác có đúng là interface IMoveCommand để tôi lấy DirX, DirY không?"
            if (command is not IMoveCommand moveCommand) return;

            _application.ComputeMoveVelocity(
                moveCommand.DirX,
                moveCommand.DirY,
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