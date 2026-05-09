// [MODULE: Player]
// [TYPE: Logic]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: PlayerStateChangedSignal]
// [SIGNAL_SUBSCRIBES: OnMoveInputSignal]
// [DEPENDS_ON: none]
using System;
using UnityEngine;
using MessagePipe;
using VContainer;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals; // Nơi chứa OnMoveInputSignal của bạn

namespace BillGameCore.Modules.Player
{
    public class MovementLogic : IMovementService, IDisposable
    {
        private readonly IPublisher<PlayerStateChangedSignal> _statePublisher;
        private readonly IDisposable _inputSubscription;

        private Vector2 _inputDirection;
        private readonly float _baseSpeed = 5f; // Có thể chuyển sang ScriptableObject sau

        public Vector2 CurrentVelocity { get; private set; }
        public float SneakRadiusMultiplier { get; private set; } = 1.0f;

        [Inject]
        public MovementLogic(
            ISubscriber<OnMoveInputSignal> inputSubscriber,
            IPublisher<PlayerStateChangedSignal> statePublisher)
        {
            _statePublisher = statePublisher;
            _inputSubscription = inputSubscriber.Subscribe(OnInputReceived);
        }

        private void OnInputReceived(OnMoveInputSignal signal)
        {
            _inputDirection = signal.Direction; // Lấy input trực tiếp từ signal của module Input

            // Xử lý logic đi lén (nếu sau này có nút Sneak)
            bool isSneaking = false; // Tạm thời
            SneakRadiusMultiplier = isSneaking ? 0.5f : 1.0f;
            float currentSpeed = isSneaking ? _baseSpeed * 0.5f : _baseSpeed;

            // Tính toán vận tốc
            CurrentVelocity = _inputDirection.normalized * currentSpeed;

            // Bắn signal cập nhật trạng thái (cho View hoặc Animation)
            bool isMoving = CurrentVelocity.sqrMagnitude > 0.01f;
            _statePublisher.Publish(new PlayerStateChangedSignal(isMoving, _inputDirection));
        }
        public void Initialize()
        {
            // Để trống hoặc Log ra để kiểm tra
            UnityEngine.Debug.Log("MovementLogic đã khởi tạo");
        }

        public void Dispose()
        {
            _inputSubscription?.Dispose();
        }
    }
}