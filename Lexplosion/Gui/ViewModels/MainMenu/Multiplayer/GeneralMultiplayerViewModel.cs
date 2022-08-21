using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu.Multiplayer
{
    public class MultiplayerModel : VMBase
    {
        private OnlineGameStatus _gameStatus = OnlineGameStatus.None;
        public OnlineGameStatus GameStatus 
        {
            get => _gameStatus; set 
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Player> _players;
        public ObservableCollection<Player> Players
        {
            get => _players; set
            {
                _players = value;
                OnPropertyChanged();
            }
        }

        public void OnPlayerConnected(Player player)
        {
            Players.Add(player);
        }

        public void OnPlayerDisconnected(Player player) 
        {
            Players.Remove(player);
        }

        private void OnPlayerStateChanged(OnlineGameStatus status, string strangeString) 
        {
            GameStatus = status;
        }

        public MultiplayerModel()
        {
            LaunchGame.StateChanged += OnPlayerStateChanged;
            LaunchGame.UserConnected += OnPlayerConnected;
            LaunchGame.UserDisconnected += OnPlayerDisconnected;
        }
    }

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
