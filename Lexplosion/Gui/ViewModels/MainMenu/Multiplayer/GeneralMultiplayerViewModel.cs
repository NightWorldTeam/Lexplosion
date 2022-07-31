using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu.Multiplayer
{
    public class GeneralMultiplayerViewModel : VMBase
    {
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
    }
}
