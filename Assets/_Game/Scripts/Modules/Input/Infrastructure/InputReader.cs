using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour — dịch event New Input System thành ICommand object.
    // R03: Không có business logic — chỉ raw-input → Command translation.
    // R16: Đây là class DUY NHẤT được phép gọi CommandBuffer.Enqueue().
    // R17: Đăng ký qua builder.RegisterComponent<InputReader>() — VContainer resolve [Inject].
    public sealed class InputReader : MonoBehaviour
    {
        [Inject] private CommandBuffer _buffer; // được inject bởi VContainer

        [SerializeField] private PlayerInput _playerInput; // gắn trong Inspector

        private EntityId     _controlledEntityId = EntityId.Invalid;
        private InputContext _currentContext      = InputContext.Player;

        // Trạng thái giữ nút Attack
        private bool  _attackHeld;
        private float _attackHeldStart;

        // Gọi bởi GameBootstrapper sau khi PlayerSpawner.Spawn() trả về Runtime.
        public void SetControlledEntity(EntityId id) => _controlledEntityId = id;

        public void ValidateConfiguration()
        {
            if (_playerInput == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires a PlayerInput reference.");
            if (_playerInput.actions == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires PlayerInput.actions.");

            var playerMap = _playerInput.actions.FindActionMap(PlayerInputContext.ActionMapName);
            if (playerMap == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires action map {PlayerInputContext.ActionMapName}.");
            if (playerMap.FindAction("Move") == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires action Player/Move.");
            if (playerMap.FindAction("Attack") == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires action Player/Attack.");
            if (playerMap.FindAction("Interact") == null)
                throw new System.InvalidOperationException($"{nameof(InputReader)} requires action Player/Interact.");
        }

        public void SwitchContext(InputContext context)
        {
            _currentContext = context;
            _buffer.Clear(); // xả command cũ (CONTEXT §14E)
            _playerInput.SwitchCurrentActionMap(context switch
            {
                InputContext.Player  => PlayerInputContext.ActionMapName,
                InputContext.Vehicle => VehicleInputContext.ActionMapName,
                InputContext.UI      => UIInputContext.ActionMapName,
                _                   => PlayerInputContext.ActionMapName,
            });
        }

        private void Update()
        {
            if (!_controlledEntityId.IsValid) return;
            switch (_currentContext)
            {
                case InputContext.Player:  ReadPlayerMap();  break;
                case InputContext.Vehicle: ReadVehicleMap(); break;
                // UI do Unity EventSystem xử lý — không cần đọc thủ công
            }
        }

        // R16: Tất cả lệnh Enqueue chỉ nằm trong file này.
        private void ReadPlayerMap()
        {
            // Di chuyển — đọc mỗi frame (liên tục)
            var mv = _playerInput.actions["Player/Move"].ReadValue<Vector2>();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, mv.x, mv.y, Time.time));

            // Tấn công — theo dõi giữ nút
            var atk = _playerInput.actions["Player/Attack"];
            if (atk.WasPressedThisFrame()) { _attackHeld = true; _attackHeldStart = Time.time; }
            if (_attackHeld)
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time,
                                                  isHeld: true,
                                                  heldDuration: Time.time - _attackHeldStart));
            if (atk.WasReleasedThisFrame()) _attackHeld = false;

            // Tương tác — một lần mỗi lần nhấn
            if (_playerInput.actions["Player/Interact"].WasPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }

        private void ReadVehicleMap()
        {
            // Thêm đọc Vehicle action ở đây khi slice Vehicle được build.
        }
    }
}
