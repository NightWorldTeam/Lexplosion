using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
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
            get => _gameStatus; private set 
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        public bool DirectConnetion 
        {
            get => UserData.GeneralSettings.OnlineGameDirectConnection; set 
            {
                UserData.GeneralSettings.OnlineGameDirectConnection = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        private ObservableCollection<Player> _players;
        public ObservableCollection<Player> Players
        {
            get => _players; private set
            {
                _players = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyPlayers = true;
        public bool IsEmptyPlayers 
        {
            get => _isEmptyPlayers; private set 
            {
                _isEmptyPlayers = value;
                OnPropertyChanged();
            }
        }

        public void OnPlayerConnected(Player player)
        {
            App.Current.Dispatcher.Invoke(delegate ()
            {
                Players.Add(player);
                IsEmptyPlayers = Players.Count == 0;
            });
        }

        public void OnPlayerDisconnected(Player player) 
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                if (!player.IsKicked)
                    Players.Remove(player);
                player.SetUnkickedAction(RemoveObjFromList);
                IsEmptyPlayers = (Players.Count == 0) && !player.IsKicked;
            });
            
        }

        private void OnPlayerStateChanged(OnlineGameStatus status, string strangeString) 
        {
            GameStatus = status;
            if (GameStatus == OnlineGameStatus.None)
                Players.Clear();
        }

        public MultiplayerModel()
        {
            Players = new ObservableCollection<Player>();
            //SetTestPlayers();
            LaunchGame.StateChanged += OnPlayerStateChanged;
            LaunchGame.UserConnected += OnPlayerConnected;
            LaunchGame.UserDisconnected += OnPlayerDisconnected;
        }

        private void SetTestPlayers() 
        {
            Players = new ObservableCollection<Player>();
            OnPlayerConnected(new Player("123123", () => { }));
        }

        private bool IsExistPlayers(Player player) 
        {
            foreach (var pl in Players)
            {
                if (pl.UUID == player.UUID)
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveObjFromList(Player player) 
        {
            Players.Remove(player);
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
