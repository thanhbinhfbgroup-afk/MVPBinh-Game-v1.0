using TMPro;
using UnityEngine;

namespace BillGameCore.Scenes.UI
{
    public sealed class WalletHudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _experienceText;
        [SerializeField] private TextMeshProUGUI _coinText;

        public void SetWallet(int gold, int experience, int coin)
        {
            if (_goldText != null)
            {
                _goldText.text = $"Gold: {gold}";
            }

            if (_experienceText != null)
            {
                _experienceText.text = $"XP: {experience}";
            }
            if (_coinText  != null)
            {
                _coinText.text = $"Coin: {coin}";
            }    
        }
    }
}