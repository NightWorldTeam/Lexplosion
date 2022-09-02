using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.Models
{
    public sealed class MultiplayerModel : VMBase
    {
        private OnlineGameStatus _gameStatus = OnlineGameStatus.None;
        public OnlineGameStatus GameStatus
        {
            get => _gameStatus; private set
            {
                _gameStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StrGameStatus));
            }
        }

        public string StrGameStatus 
        {
            get => ResourceGetter.GetString("status" + _gameStatus.ToString());
        }

        //private string _gameStatusStr;
        //public string GameStatusStr
        //{
        //    get => _gameStatusStr;
        //}

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
            App.Current.Dispatcher.Invoke(() =>
            {
                GameStatus = status;
                if (GameStatus == OnlineGameStatus.None)
                    Players.Clear();
            });
        }

        public MultiplayerModel()
        {
            Players = new ObservableCollection<Player>();
            LaunchGame.StateChanged += OnPlayerStateChanged;
            LaunchGame.UserConnected += OnPlayerConnected;
            LaunchGame.UserDisconnected += OnPlayerDisconnected;
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
}
