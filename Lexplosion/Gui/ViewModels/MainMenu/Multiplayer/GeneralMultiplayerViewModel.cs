using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management;

namespace Lexplosion.Gui.ViewModels.MainMenu.Multiplayer
{
    public sealed class GeneralMultiplayerViewModel : VMBase
    {
        public MultiplayerModel Model { get; }

        private RelayCommand _multiplayerOff;
        public RelayCommand MultiplayerOff
        {
            get => _multiplayerOff ?? (_multiplayerOff = new RelayCommand(obj =>
            {
            }));
        }

        private RelayCommand _multiplayerRefresh;
        public RelayCommand MultiplayerRefresh
        {
            get => _multiplayerRefresh ?? (_multiplayerRefresh = new RelayCommand(obj =>
            {
                LaunchGame.RebootOnlineGame();
            }));
        }

        public GeneralMultiplayerViewModel()
        {
            Model = new MultiplayerModel();
        }
    }
}
