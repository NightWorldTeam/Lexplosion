using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management;

namespace Lexplosion.Gui.ViewModels.MainMenu.Multiplayer
{
    public class GeneralMultiplayerViewModel : VMBase
    {
        public MultiplayerModel Model { get; }

        private RelayCommand _multiplayerOff;
        public RelayCommand MultiplayerOff 
        {
            get => _multiplayerOff ?? new RelayCommand(obj => 
            {
                LaunchGame.RebootOnlineGame();
            });
        }

        private RelayCommand _multiplayerRefresh;
        public RelayCommand MultiplayerRefresh
        {
            get => _multiplayerRefresh ?? new RelayCommand(obj =>
            {
            });
        }

        public GeneralMultiplayerViewModel()
        {
            Model = new MultiplayerModel();
        }
    }
}
