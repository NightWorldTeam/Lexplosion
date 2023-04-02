using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.Models
{
    public sealed class MultiplayerModel : VMBase
    {
        #region Properties


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


        public bool DirectConnetion
        {
            get => GlobalData.GeneralSettings.NetworkDirectConnection; set
            {
                GlobalData.GeneralSettings.NetworkDirectConnection = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        private ObservableCollection<PlayerClub> _players;
        public ObservableCollection<PlayerClub> Players
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


        #endregion Properities


        #region Construtors


        public MultiplayerModel()
        {
            Players = new ObservableCollection<PlayerClub>();
            LaunchGame.StateChanged += OnPlayerStateChanged;
            LaunchGame.UserConnected += OnPlayerConnected;
            LaunchGame.UserDisconnected += OnPlayerDisconnected;
        }


        #endregion Construtors


        #region Public & Protected Methods


        public void OnPlayerConnected(Player player)
        {
            App.Current.Dispatcher.Invoke(delegate ()
            {
                var playerClub = (PlayerClub)player;
                if (player != null)
                {
                    if (Players.Contains(playerClub))
                        Players.Remove(playerClub);

                    Players.Add(playerClub);
                    IsEmptyPlayers = Players.Count == 0;
                }
            });
        }

        public void OnPlayerDisconnected(Player player)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var playerClub = (PlayerClub)player;
                if (player != null)
                {
                    if (!player.IsKicked)
                        Players.Remove(playerClub);
                    player.SetUnkickedAction(RemoveObjFromList);
                    IsEmptyPlayers = (Players.Count == 0) && !player.IsKicked;
                }
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private bool IsExistPlayers(PlayerClub player)
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
            var playerClub = (PlayerClub)player;
            Players.Remove(playerClub);
        }

        private void OnPlayerStateChanged(OnlineGameStatus status, string strangeString)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                GameStatus = status;
                if (GameStatus == OnlineGameStatus.None)
                    Players?.Clear();

                IsEmptyPlayers = true;
            });
        }


        #endregion Private Methods
    }
}
