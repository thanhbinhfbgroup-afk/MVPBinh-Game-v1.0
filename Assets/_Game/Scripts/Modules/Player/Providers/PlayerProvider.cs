// [MODULE: Player]
// [TYPE: Provider]
// [SCOPE: ProjectLifetimeScope (hoặc Scene nếu tách riêng)]
// [DEPENDS_ON: IPlayerService, IInputService]
// Provider: bridge Unity input → PlayerLogic
// TODO: thay bằng IInputService khi module Input đã có
using VContainer;
using UnityEngine;
using BillGameCore.Interfaces;

namespace BillGameCore.Modules.Player
{
    public class PlayerProvider : MonoBehaviour
    {
        [Inject] private IPlayerService _playerService;

        private void Update()
        {
            // Tạm: dùng legacy Input để test trước khi có IInputService
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");
            if (h != 0 || v != 0)
                _playerService.Move(new UnityEngine.Vector2(h, v).magnitude);
        }
    }
}