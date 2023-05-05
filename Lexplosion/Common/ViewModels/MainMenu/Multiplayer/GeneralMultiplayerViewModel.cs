using Lexplosion.Common.Models;
using Lexplosion.Controls;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public sealed class GeneralMultiplayerViewModel : VMBase, INotifiable
    {
        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; private set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }

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
                // TODO : Translate
                DoNotification(ResourceGetter.GetString("successful"), ResourceGetter.GetString("multiplayerReloaded"), 5, 0);
            }));
        }


        #endregion Commands


        #region Constructors


        public GeneralMultiplayerViewModel(DoNotificationCallback doNotification = null)
        {
            DoNotification = doNotification ?? DoNotification;
            Model = new MultiplayerModel();
        }


        #endregion Constructors
    }
}
