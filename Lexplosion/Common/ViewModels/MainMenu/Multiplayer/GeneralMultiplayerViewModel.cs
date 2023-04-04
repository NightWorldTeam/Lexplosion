using Lexplosion.Common.Models;
using Lexplosion.Logic.Management;
using System;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public sealed class GeneralMultiplayerViewModel : VMBase
    {
        private readonly Action<string, string, uint, byte> _doNotification = (header, message, time, type) => { };

        public MultiplayerModel Model { get; }


        #region Commands


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
                _doNotification("Успешно", "Сетевая игра перезапущена", 5, 0);
            }));
        }


        #endregion Commands


        #region Constructors


        public GeneralMultiplayerViewModel(Action<string, string, uint, byte> doNotification = null)
        {
            _doNotification = doNotification ?? _doNotification;
            Model = new MultiplayerModel();
        }


        #endregion Constructors
    }
}
