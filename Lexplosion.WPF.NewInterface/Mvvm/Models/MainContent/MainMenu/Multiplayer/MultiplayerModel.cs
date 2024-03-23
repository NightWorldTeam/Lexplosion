using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu
{
    public sealed class MultiplayerModel : ViewModelBase
    {
        #region Properties


        private ObservableCollection<PlayerWrapper> _players = new ObservableCollection<PlayerWrapper>() 
        {
            new PlayerWrapper("Editor"), new PlayerWrapper("Tester"),
            new PlayerWrapper("Editor1"), new PlayerWrapper("Tester1"),
            new PlayerWrapper("Editor2"), new PlayerWrapper("Tester2"),
            new PlayerWrapper("Editor3"), new PlayerWrapper("Tester3"),
        };


        public IEnumerable<PlayerWrapper> Players { get => _players; }


        private OnlineGameStatus _gameStatus = OnlineGameStatus.None;
        public OnlineGameStatus GameStatus 
        {
            get => _gameStatus; set 
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isPlayersListEmpty = true;
        public bool IsPlayersListEmpty
        {
            get => _isPlayersListEmpty; private set
            {
                _isPlayersListEmpty = value;
                OnPropertyChanged();
            }
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


        public string StrGameStatus
        {
            get => "status" + _gameStatus.ToString();
        }


        #endregion Properties



        #region Constructors


        public MultiplayerModel()
        {
            LaunchGame.StateChanged += LaunchGame_StateChanged;
            LaunchGame.UserConnected += LaunchGame_UserConnected;
            LaunchGame.UserDisconnected += LaunchGame_UserDisconnected;
        }


        #endregion Constructors


        #region Private Methods


        /// <summary>
        /// Удаляет игрока из списка.
        /// </summary>
        /// <param name="player"></param>
        private void RemovePlayerFromList(Player player)
        {
            var wrapper = new PlayerWrapper(player);
            _players.Remove(wrapper);
        }

        private void LaunchGame_UserDisconnected(Player player)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (player != null)
                {
                    var wrapper = new PlayerWrapper(player);

                    wrapper.SetUnkickedAction(RemovePlayerFromList);
                    IsPlayersListEmpty = _players.Count == 0 && !player.IsKicked;
                }
            });
        }

        private void LaunchGame_UserConnected(Player player)
        {
            App.Current.Dispatcher.Invoke(delegate ()
            {
                if (player != null)
                {
                    var wrapper = new PlayerWrapper(player);

                    if (_players.Contains(wrapper))
                        _players.Remove(wrapper);

                    _players.Add(wrapper);
                    IsPlayersListEmpty = _players.Count == 0;
                }
            });
        }

        private void LaunchGame_StateChanged(OnlineGameStatus status, string arg2)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                GameStatus = status;
                if (GameStatus == OnlineGameStatus.None)
                {
                    _players?.Clear();
                }

                IsPlayersListEmpty = true;
            });
        }


        #endregion Private Methods
    }
}
