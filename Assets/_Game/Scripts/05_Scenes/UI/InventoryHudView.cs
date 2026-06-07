using TMPro;
using UnityEngine;

namespace BillGameCore.Scenes.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class InventoryHudView : MonoBehaviour
    {
        private TextMeshProUGUI _countText;

        private void Awake()
        {
            _countText = GetComponent<TextMeshProUGUI>();
        }

        public void SetItemCount(string label, int amount)
        {
            _countText ??= GetComponent<TextMeshProUGUI>();
            _countText.text = $"{label}: {amount}";
        }
    }
}
