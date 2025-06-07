using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendRequestsViewModel : ViewModelBase, IRefreshable
    {
        private readonly AppCore _appCore;

        public FriendRequestsModel Model { get; }


        #region Command


        private RelayCommand _declineFriendRequestCommand;
        public ICommand DeclineFriendRequestCommand
        {
            get => RelayCommand.GetCommand<NightWorldUserRequest>(ref _declineFriendRequestCommand, friend =>
            {
                Model.DeclineFriend(friend);
                _appCore.MessageService.Info("YouHaveDeclinedYourFriendRequest_", true, friend.Login);
            });
        }

        /// <summary>
        /// Добавление друга, в качестве агрумента obj, будет передаваться ссылка на объект друга.
        /// </summary>
        private RelayCommand _addFriendCommand;
        public ICommand AddFriendCommand
        {
            get => RelayCommand.GetCommand<NightWorldUserRequest>(ref _addFriendCommand, friend =>
            {
                Model.AddFriend(friend);
                _appCore.MessageService.Info("YouAnd_AreFriendsNow", true, friend.Login);
            });
        }


        #endregion Command


        public FriendRequestsViewModel(AppCore appCore)
        {
            Model = new FriendRequestsModel(appCore);
            _appCore = appCore;
        }

        public void Refresh()
        {
            Model.UpdateRequestsData();
        }
    }
}
