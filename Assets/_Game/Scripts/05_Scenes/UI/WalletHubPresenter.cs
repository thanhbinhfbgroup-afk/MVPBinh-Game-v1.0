using BillGameCore.SharedPorts.Economy;

namespace BillGameCore.Scenes.UI
{
    public sealed class WalletHudPresenter
    {
        private readonly IWalletService _walletService;
        private readonly WalletHudView _view;

        public WalletHudPresenter(IWalletService walletService, WalletHudView view)
        {
            _walletService = walletService;
            _view = view;
        }

        public void Refresh()
        {
            _view.SetWallet(_walletService.Gold, _walletService.Experience, _walletService.Coin);
        }
    }
}